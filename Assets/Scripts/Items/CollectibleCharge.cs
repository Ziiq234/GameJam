using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectibleCharge : MonoBehaviour
{
    [Tooltip("Forest or Wizard - which dungeon quest this charge belongs to")]
    [SerializeField] private string dungeon = "Forest";

    private bool collected;
    private string chargeId;

    private void Start()
    {
        // each charge has its own id so it can't be picked up again after leaving
        chargeId = SceneManager.GetActiveScene().name + "/" + name;

        if (QuestManager.Instance != null && QuestManager.Instance.HasCollected(chargeId))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("No QuestManager exists in the scene.");
            return;
        }

        // only picked up if the quest says you're looking for this dungeon's charges
        if (QuestManager.Instance.CollectCharge(dungeon, chargeId))
        {
            collected = true;
            SoundManager.Play("pickup");
            Destroy(gameObject);
        }
    }
}
