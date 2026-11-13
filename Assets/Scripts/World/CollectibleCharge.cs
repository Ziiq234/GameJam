using UnityEngine;

public class CollectibleCharge : MonoBehaviour
{
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("No QuestManager exists in the scene.");
            return;
        }

        // only picked up if the quest says you're looking for charges
        if (QuestManager.Instance.CollectCharge())
        {
            collected = true;
            Destroy(gameObject);
        }
    }
}