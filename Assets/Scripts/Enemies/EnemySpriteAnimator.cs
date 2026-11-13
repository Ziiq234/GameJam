using UnityEngine;

// Plays the bat animations from the sprite strips in Art/Enemies/Bat.
// Each strip is one row of 64x64 frames.
public class EnemySpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;

    [SerializeField] private Texture2D idleStrip;
    [SerializeField] private Texture2D moveStrip;
    [SerializeField] private Texture2D attackStrip;
    [SerializeField] private Texture2D hurtStrip;
    [SerializeField] private Texture2D deathStrip;
    [SerializeField] private int frameSize = 64;
    [SerializeField] private float pixelsPerUnit = 100f;

    [SerializeField] private float framesPerSecond = 12f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float movingThreshold = 0.1f;
    [SerializeField] private bool framesFaceLeft = true;

    private Sprite[] idleFrames;
    private Sprite[] moveFrames;
    private Sprite[] attackFrames;
    private Sprite[] hurtFrames;
    private Sprite[] deathFrames;

    private Sprite[] currentFrames;
    private int frame;
    private float timer;
    private bool isHurt;
    private bool isDead;

    private Rigidbody2D rb;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        idleFrames = CutStrip(idleStrip);
        moveFrames = CutStrip(moveStrip);
        attackFrames = CutStrip(attackStrip);
        hurtFrames = CutStrip(hurtStrip);
        deathFrames = CutStrip(deathStrip);

        currentFrames = idleFrames;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private Sprite[] CutStrip(Texture2D strip)
    {
        int count = strip.width / frameSize;
        Sprite[] frames = new Sprite[count];

        for (int i = 0; i < count; i++)
        {
            Rect rect = new Rect(i * frameSize, 0, frameSize, frameSize);
            frames[i] = Sprite.Create(strip, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        return frames;
    }

    public float DeathTime()
    {
        return deathFrames.Length / framesPerSecond;
    }

    public void PlayHurt()
    {
        if (isDead) return;

        isHurt = true;
        target.color = new Color(1f, 0.5f, 0.5f);
        SwitchTo(hurtFrames);
    }

    public void PlayDeath()
    {
        isDead = true;
        target.color = Color.white;
        SwitchTo(deathFrames);
    }

    private void SwitchTo(Sprite[] frames)
    {
        if (currentFrames == frames) return;

        currentFrames = frames;
        frame = 0;
        timer = 0;
    }

    private void Update()
    {
        if (!isDead && !isHurt)
        {
            SwitchTo(PickLoop());
        }

        if (!isDead)
        {
            FacePlayerOrMovement();
        }

        timer += Time.deltaTime;

        if (timer >= 1f / framesPerSecond)
        {
            timer = 0;
            frame++;

            if (frame >= currentFrames.Length)
            {
                if (isDead)
                {
                    // stay on the last death frame until destroyed
                    frame = currentFrames.Length - 1;
                }
                else if (isHurt)
                {
                    isHurt = false;
                    target.color = Color.white;
                    SwitchTo(PickLoop());
                }
                else
                {
                    frame = 0;
                }
            }
        }

        target.sprite = currentFrames[frame];
    }

    private Sprite[] PickLoop()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            return attackFrames;
        }

        if (rb.linearVelocity.magnitude > movingThreshold)
        {
            return moveFrames;
        }

        return idleFrames;
    }

    private void FacePlayerOrMovement()
    {
        float x = rb.linearVelocity.x;

        if (Mathf.Abs(x) < movingThreshold && player != null)
        {
            x = player.position.x - transform.position.x;
        }

        if (x != 0)
        {
            bool goingLeft = x < 0;
            target.flipX = goingLeft != framesFaceLeft;
        }
    }
}
