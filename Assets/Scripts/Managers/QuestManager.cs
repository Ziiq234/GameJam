using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private int requiredCharges = 3;

    private int collectedCharges = 0;

    public void CollectCharge()
    {
        collectedCharges++;

        Debug.Log("Gunpowder Charges: " + collectedCharges + "/" + requiredCharges);

        if (collectedCharges >= requiredCharges)
        {
            QuestComplete();
        }
    }

    private void QuestComplete()
    {
        Debug.Log("All gunpowder charges collected!");
    }
}