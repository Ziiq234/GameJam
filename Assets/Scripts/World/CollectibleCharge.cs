using UnityEngine;

public class CollectibleCharge : MonoBehaviour
{
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CollectCharge();
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("No QuestManager exists in the scene.");
            collected = false;
        }
    }
}