using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;

    [SerializeField] private int swordBonusDamage = 1;
    [SerializeField] private float swordBonusRange = 1f;

    private CharacterSpriteAnimator characterAnimator;
    private HeldSword heldSword;
    private float nextAttackTime;
    private Sprite slashSprite;

    private void Awake()
    {
        characterAnimator = GetComponent<CharacterSpriteAnimator>();
        heldSword = GetComponent<HeldSword>();
        slashSprite = MakeSlashSprite();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !DialogueBox.Busy && !ShopKeeper.IsOpen && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Attack();
        }
    }

    private void Attack()
    {
        QuestManager quest = QuestManager.Instance;
        bool hasSword = quest != null && quest.HasSword;
        bool hasMageSword = quest != null && quest.HasMageSword;
        bool hasRing = quest != null && quest.HasRing;

        int damage = attackDamage;
        float range = attackRange;

        if (hasSword)
        {
            damage += swordBonusDamage;
            range += swordBonusRange;
        }

        // the Mage's sword hits one harder again
        if (hasMageSword)
        {
            damage += 1;
        }

        // the enchanted ring adds a bit more damage
        if (hasRing)
        {
            damage += 1;
        }

        Vector2 facing = characterAnimator != null ? characterAnimator.Facing : Vector2.down;

        // hit area is a circle slightly in front of the player
        Vector2 hitCentre = (Vector2)transform.position + facing * (range * 0.4f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCentre, range * 0.75f);
        SoundManager.Play("swing");

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                SoundManager.Play("hit");
            }
        }

        ShowSlash(facing, range, hasSword, hasMageSword);

        if (heldSword != null)
        {
            heldSword.Swing();
        }
    }

    private void ShowSlash(Vector2 facing, float range, bool hasSword, bool hasMageSword)
    {
        GameObject slash = new GameObject("Slash");
        slash.transform.position = (Vector2)transform.position + facing * 0.2f;

        // point the arc the way the player is facing
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        slash.transform.rotation = Quaternion.Euler(0, 0, angle);
        slash.transform.localScale = Vector3.one * range * 1.4f;

        SpriteRenderer sr = slash.AddComponent<SpriteRenderer>();
        sr.sprite = slashSprite;
        sr.sortingOrder = 10;
        if (hasMageSword)
            sr.color = new Color(0.8f, 0.5f, 1f);
        else if (hasSword)
            sr.color = new Color(1f, 0.9f, 0.4f);
        else
            sr.color = Color.white;

        Destroy(slash, 0.12f);
    }

    // draws a simple pixel crescent so we don't need a slash image
    private Sprite MakeSlashSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = x - size / 2f;
                float dy = y - size / 2f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                bool inArc = distance > 10 && distance < 15 && Mathf.Abs(angle) < 70;
                texture.SetPixel(x, y, inArc ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
