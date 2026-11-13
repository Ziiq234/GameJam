using UnityEngine;

// Plays enemy animations from horizontal sprite strips (one row of frames
// per image), like the Dark Fantasy Enemies Bat sheets.
// Idle when still, Move when chasing, Attack when close to the player,
// Hurt when damaged and Death when killed (called by EnemyHealth).
public class EnemySpriteAnimator : MonoBehaviour
{
    private enum State { Idle, Move, Attack, Hurt, Death }

    [Header("Where to draw")]
    [Tooltip("The SpriteRenderer that shows the enemy (e.g. the bat child object)")]
    [SerializeField] private SpriteRenderer target;

    [Header("Sprite strips")]
    [SerializeField] private Texture2D idleStrip;
    [SerializeField] private Texture2D moveStrip;
    [SerializeField] private Texture2D attackStrip;
    [SerializeField] private Texture2D hurtStrip;
    [SerializeField] private Texture2D deathStrip;
    [SerializeField] private int frameSize = 64;
    [SerializeField] private float pixelsPerUnit = 100f;

    [Header("Timing")]
    [SerializeField] private float framesPerSecond = 12f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float movingThreshold = 0.1f;

    [Header("Facing")]
    [Tooltip("Tick if the frames on the sheet face left")]
    [SerializeField] private bool framesFaceLeft = true;

    private Sprite[] idleFrames, moveFrames, attackFrames, hurtFrames, deathFrames;
    private Rigidbody2D body;
    private Transform player;

    private State state = State.Idle;
    private int frameIndex;
    private float frameTimer;
    private Color baseColour = Color.white;

    public float DeathDuration =>
        deathFrames != null ? deathFrames.Length / framesPerSecond : 0f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (target == null) target = GetComponentInChildren<SpriteRenderer>();
        if (target != null) baseColour = target.color;

        idleFrames = Slice(idleStrip);
        moveFrames = Slice(moveStrip) ?? idleFrames;
        attackFrames = Slice(attackStrip) ?? moveFrames;
        hurtFrames = Slice(hurtStrip);
        deathFrames = Slice(deathStrip);
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    private Sprite[] Slice(Texture2D strip)
    {
        if (strip == null) return null;

        int count = strip.width / frameSize;
        Sprite[] frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            Rect rect = new Rect(i * frameSize, 0, frameSize, frameSize);
            frames[i] = Sprite.Create(strip, rect, new Vector2(0.5f, 0.5f),
                pixelsPerUnit, 0, SpriteMeshType.FullRect);
            frames[i].name = $"{strip.name}_{i}";
        }
        return frames;
    }

    // Called by EnemyHealth when the enemy takes damage but survives.
    public void PlayHurt()
    {
        if (state == State.Death) return;
        if (hurtFrames != null) SetState(State.Hurt);
        if (target != null) target.color = new Color(1f, 0.5f, 0.5f);
    }

    // Called by EnemyHealth when the enemy dies.
    public void PlayDeath()
    {
        if (target != null) target.color = baseColour;
        if (deathFrames != null) SetState(State.Death);
    }

    private void SetState(State newState)
    {
        if (state == newState) return;
        state = newState;
        frameIndex = 0;
        frameTimer = 0f;
    }

    private void Update()
    {
        if (target == null || idleFrames == null) return;

        // One-shot animations (hurt, death) play to the end before anything else.
        bool locked = state == State.Death || state == State.Hurt;
        if (!locked)
        {
            SetState(ChooseLoopState());
        }

        UpdateFacing();

        Sprite[] frames = FramesFor(state);
        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / framesPerSecond)
        {
            frameTimer = 0f;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                if (state == State.Death)
                {
                    frameIndex = frames.Length - 1; // hold the last frame
                }
                else if (state == State.Hurt)
                {
                    target.color = baseColour;
                    state = ChooseLoopState();
                    frameIndex = 0;
                    frames = FramesFor(state);
                }
                else
                {
                    frameIndex = 0;
                }
            }
        }

        target.sprite = frames[Mathf.Min(frameIndex, frames.Length - 1)];
    }

    private State ChooseLoopState()
    {
        if (player != null &&
            Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            return State.Attack;
        }

        Vector2 velocity = body != null ? body.linearVelocity : Vector2.zero;
        return velocity.sqrMagnitude > movingThreshold * movingThreshold
            ? State.Move
            : State.Idle;
    }

    private void UpdateFacing()
    {
        if (state == State.Death) return;

        float directionX = 0f;
        if (body != null && Mathf.Abs(body.linearVelocity.x) > movingThreshold)
            directionX = body.linearVelocity.x;
        else if (player != null)
            directionX = player.position.x - transform.position.x;

        if (Mathf.Abs(directionX) > 0.01f)
        {
            bool movingLeft = directionX < 0f;
            target.flipX = movingLeft != framesFaceLeft;
        }
    }

    private Sprite[] FramesFor(State s)
    {
        switch (s)
        {
            case State.Move: return moveFrames;
            case State.Attack: return attackFrames;
            case State.Hurt: return hurtFrames ?? idleFrames;
            case State.Death: return deathFrames ?? idleFrames;
            default: return idleFrames;
        }
    }
}
