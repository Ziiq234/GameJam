using UnityEngine;

// Evil wizards in the Wizard Island dungeon. They walk towards the player
// (EnemyController does that) and throw magic bolts when in range.
[RequireComponent(typeof(EnemyHealth))]
public class WizardShooter : MonoBehaviour
{
    [SerializeField] private float shotInterval = 2f;
    [SerializeField] private float shootRange = 7f;
    [SerializeField] private float boltSpeed = 5f;
    [SerializeField] private int boltDamage = 1;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Color normalColour;
    private Sprite boltSprite;
    private float nextShot;
    private float flashTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalColour = spriteRenderer.color;
        boltSprite = SpriteMaker.MakeOrb(new Color(0.7f, 0.3f, 1f));

        // flash red when hit
        GetComponent<EnemyHealth>().Damaged += () => flashTimer = 0.12f;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // don't all shoot at exactly the same time
        nextShot = Time.time + Random.Range(1f, 2.5f);
    }

    private void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            spriteRenderer.color = flashTimer > 0 ? Color.red : normalColour;
        }

        if (player == null || DialogueBox.IsOpen)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= shootRange && Time.time >= nextShot)
        {
            nextShot = Time.time + shotInterval;
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject bolt = new GameObject("MagicBolt");
        bolt.transform.position = transform.position;

        SpriteRenderer sr = bolt.AddComponent<SpriteRenderer>();
        sr.sprite = boltSprite;
        sr.sortingOrder = 8;

        SoundManager.Play("bolt");
        MagicBolt magic = bolt.AddComponent<MagicBolt>();
        Vector2 direction = (player.position - transform.position).normalized;
        magic.Launch(direction, boltSpeed, boltDamage, player);
    }
}
