using UnityEngine;

// Animates characters from the sheets in Art/Characters.
// Each sheet has 4 characters, each character has 3 rows (side, down, up).
// First 4 frames in a row are idle, last 4 are walking.
public class CharacterSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Texture2D sheet;
    [SerializeField] private int characterIndex = 0;
    [SerializeField] private float pixelsPerUnit = 32f;
    [SerializeField] private bool sideFramesFaceLeft = true;

    [SerializeField] private float idleFramesPerSecond = 4f;
    [SerializeField] private float walkFramesPerSecond = 8f;
    [SerializeField] private float movingThreshold = 0.05f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private Sprite[,] idleFrames = new Sprite[3, 4];
    private Sprite[,] walkFrames = new Sprite[3, 4];

    // 0 = side, 1 = down, 2 = up
    private int direction = 1;
    private bool facingLeft;
    private bool isMoving;
    private int frame;
    private float timer;
    private Vector3 lastPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
        LoadFrames();
    }

    private void LoadFrames()
    {
        for (int row = 0; row < 3; row++)
        {
            // texture y starts at the bottom so count up from there
            int y = sheet.height - (characterIndex * 3 + row + 1) * 48;

            for (int i = 0; i < 4; i++)
            {
                idleFrames[row, i] = MakeSprite(i * 32, y);
                walkFrames[row, i] = MakeSprite((i + 4) * 32, y);
            }
        }
    }

    private Sprite MakeSprite(int x, int y)
    {
        Rect rect = new Rect(x, y, 32, 48);
        return Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }

    private void Update()
    {
        Vector2 velocity = GetVelocity();
        bool moving = velocity.magnitude > movingThreshold;

        if (moving)
        {
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                direction = 0;
                facingLeft = velocity.x < 0;
            }
            else if (velocity.y > 0)
            {
                direction = 2;
            }
            else
            {
                direction = 1;
            }
        }

        // start the new animation from the first frame
        if (moving != isMoving)
        {
            isMoving = moving;
            frame = 0;
            timer = 0;
        }

        float fps = isMoving ? walkFramesPerSecond : idleFramesPerSecond;
        timer += Time.deltaTime;

        if (timer >= 1f / fps)
        {
            timer = 0;
            frame = (frame + 1) % 4;
        }

        spriteRenderer.sprite = isMoving ? walkFrames[direction, frame] : idleFrames[direction, frame];
        spriteRenderer.flipX = direction == 0 && facingLeft != sideFramesFaceLeft;
    }

    // player uses its rigidbody, NPCs just use how far they moved
    private Vector2 GetVelocity()
    {
        // game is paused
        if (Time.deltaTime == 0)
        {
            return Vector2.zero;
        }

        Vector2 moved = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        if (rb != null)
        {
            return rb.linearVelocity;
        }

        return moved;
    }
}
