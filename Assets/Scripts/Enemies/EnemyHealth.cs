using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private int goldDrop = 10;
    [Tooltip("Which area this enemy belongs to, used for quests (e.g. WizardIsland)")]
    [SerializeField] private string questArea = "";
    [Tooltip("Extra health once the player has a sword, so fights stay tough")]
    [SerializeField] private int extraHealthWithSword = 3;
    [SerializeField] private bool destroyOnDeath = true;

    private int damageTaken;
    private bool isDead;
    private EnemySpriteAnimator spriteAnimator;

    // other scripts (like the Mage boss) can listen for these
    public event System.Action Damaged;
    public event System.Action Died;

    public int MaxHealth
    {
        get
        {
            if (QuestManager.Instance != null && QuestManager.Instance.HasSword)
                return maxHealth + extraHealthWithSword;

            return maxHealth;
        }
    }

    public int CurrentHealth => Mathf.Max(MaxHealth - damageTaken, 0);

    private void Awake()
    {
        spriteAnimator = GetComponent<EnemySpriteAnimator>();
    }

    public void TakeDamage(int damage)
    {
        // switched off enemies (like the Mage before his fight) can't be hurt
        if (isDead || !enabled)
            return;

        damageTaken += damage;

        Debug.Log(name + " health: " + CurrentHealth);

        Damaged?.Invoke();

        if (damageTaken >= MaxHealth)
        {
            Die();
        }
        else if (spriteAnimator != null)
        {
            spriteAnimator.PlayHurt();
        }
    }

    private void Die()
    {
        isDead = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddGold(goldDrop);
            if (goldDrop > 0) SoundManager.Play("coin");
            QuestManager.Instance.EnemyKilled(questArea);
        }

        Died?.Invoke();

        if (!destroyOnDeath)
            return;

        if (spriteAnimator == null)
        {
            Destroy(gameObject);
            return;
        }

        // stop it moving and hurting the player while the death animation plays
        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null) controller.enabled = false;

        EnemyDamage enemyDamage = GetComponent<EnemyDamage>();
        if (enemyDamage != null) enemyDamage.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        spriteAnimator.PlayDeath();
        Destroy(gameObject, spriteAnimator.DeathTime());
    }
}
