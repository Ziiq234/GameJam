using UnityEngine;

// Animates a character from the sheets in Assets/Art/Characters.
// Each sheet is 8 columns x 12 rows of 32x48 frames and holds 4 characters.
// Each character uses 3 rows: side, front (down), back (up).
// Columns 0-3 are the idle loop, columns 4-7 are the walk loop.
// Works for the Player (reads Rigidbody2D velocity) and for NPCs
// (falls back to how far the object moved since the last frame).
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterSpriteAnimator : MonoBehaviour
{
    private enum Facing { Side, Down, Up }

    [Header("Sprite sheet")]
    [SerializeField] private Texture2D sheet;
    [Tooltip("Which character on the sheet: 0 = top block, 3 = bottom block")]
    [Range(0, 3)]
    [SerializeField] private int characterIndex = 0;
    [SerializeField] private float pixelsPerUnit = 32f;
    [Tooltip("Tick if the side-view frames on the sheet face left")]
    [SerializeField] private bool sideFramesFaceLeft = true;

    [Header("Animation")]
    [SerializeField] private float idleFramesPerSecond = 4f;
    [SerializeField] private float walkFramesPerSecond = 8f;
    [SerializeField] private float movingThreshold = 0.05f;

    private const int FrameWidth = 32;
    private const int FrameHeight = 48;
    private const int FramesPerLoop = 4;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;

    // [facing, frame] for each loop
    private Sprite[,] idleFrames;
    private Sprite[,] walkFrames;

    private Facing facing = Facing.Down;
    private bool facingLeft;
    private float frameTimer;
    private int frameIndex;
    private bool wasMoving;
    private Vector3 lastPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
        BuildFrames();
    }

    // Lets other scripts (e.g. NPC wandering) swap character at runtime.
    public void SetCharacter(Texture2D newSheet, int newCharacterIndex)
    {
        sheet = newSheet;
        characterIndex = Mathf.Clamp(newCharacterIndex, 0, 3);
        BuildFrames();
    }

    private void BuildFrames()
    {
        if (sheet == null)
        {
            Debug.LogWarning($"{name}: CharacterSpriteAnimator has no sprite sheet assigned.");
            return;
        }

        idleFrames = new Sprite[3, FramesPerLoop];
        walkFrames = new Sprite[3, FramesPerLoop];

        for (int facingRow = 0; facingRow < 3; facingRow++)
        {
            // Rows are counted from the top of the image, but texture
            // coordinates start at the bottom, so flip the row.
            int rowFromTop = characterIndex * 3 + facingRow;
            int y = sheet.height - (rowFromTop + 1) * FrameHeight;

            for (int i = 0; i < FramesPerLoop; i++)
            {
                idleFrames[facingRow, i] = MakeFrame(i, y);
                walkFrames[facingRow, i] = MakeFrame(i + FramesPerLoop, y);
            }
        }

        UpdateSprite();
    }

    private Sprite MakeFrame(int column, int y)
    {
        Rect rect = new Rect(column * FrameWidth, y, FrameWidth, FrameHeight);
        Sprite sprite = Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f),
            pixelsPerUnit, 0, SpriteMeshType.FullRect);
        sprite.name = $"{sheet.name}_{characterIndex}_{column}";
        return sprite;
    }

    private void Update()
    {
        if (idleFrames == null) return;

        Vector2 velocity = GetVelocity();
        bool moving = velocity.sqrMagnitude > movingThreshold * movingThreshold;

        if (moving)
        {
            // Pick the strongest direction so diagonals still look right.
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                facing = Facing.Side;
                facingLeft = velocity.x < 0f;
            }
            else
            {
                facing = velocity.y > 0f ? Facing.Up : Facing.Down;
            }
        }

        // Restart the loop when switching between idle and walking.
        if (moving != wasMoving)
        {
            frameIndex = 0;
            frameTimer = 0f;
            wasMoving = moving;
        }

        float fps = moving ? walkFramesPerSecond : idleFramesPerSecond;
        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / fps)
        {
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % FramesPerLoop;
        }

        UpdateSprite();
    }

    private Vector2 GetVelocity()
    {
        Vector3 position = transform.position;
        Vector2 moved = (position - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = position;

        if (body != null && body.bodyType != RigidbodyType2D.Static)
        {
            return body.linearVelocity;
        }

        return moved;
    }

    private void UpdateSprite()
    {
        Sprite[,] frames = wasMoving ? walkFrames : idleFrames;
        spriteRenderer.sprite = frames[(int)facing, frameIndex];
        // Mirror the side frames when the character faces the other way
        // from how they're drawn on the sheet.
        spriteRenderer.flipX = facing == Facing.Side && (facingLeft != sideFramesFaceLeft);
    }
}
