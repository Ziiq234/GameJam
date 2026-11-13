using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 2;

    private int currentHealth;
    private bool isDead;
    private EnemySpriteAnimator spriteAnimator;

    private void Awake()
    {
        spriteAnimator = GetComponent<EnemySpriteAnimator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log("Enemy Health: " + currentHealth);

        if (currentHealth <= 0)
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

        if (spriteAnimator == null)
        {
            Destroy(gameObject);
            return;
        }

        // Stop the enemy chasing, hurting or blocking the player
        // while its death animation plays.
        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null) controller.enabled = false;

        EnemyDamage enemyDamage = GetComponent<EnemyDamage>();
        if (enemyDamage != null) enemyDamage.enabled = false;

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        spriteAnimator.PlayDeath();
        Destroy(gameObject, spriteAnimator.DeathDuration);
    }
}
