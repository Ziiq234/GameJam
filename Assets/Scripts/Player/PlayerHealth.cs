using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;

    private int currentHealth;

    public int CurrentHealth => currentHealth;

    // the shield from Cornelius gives 2 extra hearts
    public int MaxHealth
    {
        get
        {
            if (QuestManager.Instance != null && QuestManager.Instance.HasShield)
                return maxHealth + 2;

            return maxHealth;
        }
    }

    private void Start()
    {
        // keep the same health as in the last scene
        if (QuestManager.Instance != null && QuestManager.Instance.SavedHealth > 0)
        {
            currentHealth = Mathf.Min(QuestManager.Instance.SavedHealth, MaxHealth);
        }
        else
        {
            currentHealth = MaxHealth;
        }
    }

    public void HealFull()
    {
        currentHealth = MaxHealth;
        SaveHealth();
    }

    private void SaveHealth()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SavedHealth = currentHealth;
        }
    }

    private void Update()
    {
        // DEV TEST: Press H to damage the player
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        SaveHealth();
        SoundManager.Play("hurt");

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerAttack attack = GetComponent<PlayerAttack>();

        if (attack != null)
        {
            attack.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}