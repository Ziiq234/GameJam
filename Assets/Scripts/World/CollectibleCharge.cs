using UnityEngine;

public class CollectibleCharge : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        QuestManager questManager = FindFirstObjectByType<QuestManager>();

        if (questManager != null)
        {
            questManager.CollectCharge();
            Destroy(gameObject);
        }
    }
}