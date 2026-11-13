using UnityEngine;

// The Mage boss fight. Starts when QuestNPC calls StartFight().
// He teleports around his island and throws magic bolts at the player.
// When he dies he drops his sword and the tax money.
[RequireComponent(typeof(EnemyHealth))]
public class MageBoss : MonoBehaviour
{
    [SerializeField] private float shotInterval = 1.3f;
    [SerializeField] private float boltSpeed = 6f;
    [SerializeField] private int boltDamage = 1;
    [SerializeField] private float blinkInterval = 4.5f;
    [SerializeField] private float blinkRadius = 4f;

    private EnemyHealth health;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector3 home;
    private Color normalColour;
    private Sprite boltSprite;

    private bool fighting;
    private float nextShot;
    private float nextBlink;
    private float flashTimer;
    private GUIStyle barStyle;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        home = transform.position;
        normalColour = spriteRenderer.color;
        boltSprite = SpriteMaker.MakeOrb(new Color(0.7f, 0.3f, 1f));

        health.Damaged += OnHit;
        health.Died += OnDefeated;
    }

    private void Start()
    {
        // already beaten in an earlier visit
        if (QuestManager.Instance != null && QuestManager.Instance.MageDefeated)
        {
            gameObject.SetActive(false);
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    public void StartFight()
    {
        fighting = true;
        health.enabled = true;

        // no more chatting during the fight
        GetComponent<QuestNPC>().enabled = false;

        nextShot = Time.time + 1f;
        nextBlink = Time.time + blinkInterval;
    }

    private void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            spriteRenderer.color = flashTimer > 0 ? Color.red : normalColour;
        }

        if (!fighting || player == null || DialogueBox.IsOpen)
            return;

        if (Time.time >= nextShot)
        {
            nextShot = Time.time + shotInterval;
            ShootBolt();
        }

        if (Time.time >= nextBlink)
        {
            nextBlink = Time.time + blinkInterval;
            Vector2 offset = Random.insideUnitCircle * blinkRadius;
            transform.position = home + new Vector3(offset.x, offset.y, 0);
        }
    }

    private void ShootBolt()
    {
        GameObject bolt = new GameObject("MagicBolt");
        bolt.transform.position = transform.position;

        SpriteRenderer sr = bolt.AddComponent<SpriteRenderer>();
        sr.sprite = boltSprite;
        sr.sortingOrder = 8;

        MagicBolt magic = bolt.AddComponent<MagicBolt>();
        Vector2 direction = (player.position - transform.position).normalized;
        magic.Launch(direction, boltSpeed, boltDamage, player);
    }

    private void OnHit()
    {
        flashTimer = 0.1f;
    }

    private void OnDefeated()
    {
        fighting = false;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.DefeatMage();
        }

        // drop his sword where he fell
        GameObject drop = new GameObject("MageSwordPickup");
        drop.transform.position = transform.position;
        drop.AddComponent<SwordPickup>();

        DialogueBox.Show("Mage", new[] { "Impossible... beaten by a tax collector...", "Take my gold. Take my sword. Just... leave..." });

        gameObject.SetActive(false);
    }

    private void OnGUI()
    {
        if (!fighting)
            return;

        if (barStyle == null)
        {
            barStyle = new GUIStyle(GUI.skin.label);
            barStyle.fontSize = 14;
            barStyle.fontStyle = FontStyle.Bold;
            barStyle.alignment = TextAnchor.MiddleCenter;
            barStyle.normal.textColor = Color.white;
        }

        // boss health bar across the bottom of the screen
        Rect bar = new Rect(Screen.width / 2f - 200, Screen.height - 40, 400, 22);
        UIHelper.DrawPanel(bar);

        float percent = (float)health.CurrentHealth / health.MaxHealth;
        UIHelper.DrawRect(new Rect(bar.x, bar.y, bar.width * percent, bar.height), new Color(0.6f, 0.2f, 0.9f));
        GUI.Label(bar, "The Mage", barStyle);
    }
}
