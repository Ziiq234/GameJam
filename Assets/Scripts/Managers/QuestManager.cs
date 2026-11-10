using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private int requiredCharges = 3;

    private int collectedCharges;
    private bool questStarted;
    private bool chargesComplete;

    public int CollectedCharges => collectedCharges;
    public int RequiredCharges => requiredCharges;
    public bool QuestStarted => questStarted;
    public bool ChargesComplete => chargesComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartQuest()
    {
        if (questStarted)
            return;

        questStarted = true;
        Debug.Log("Quest started: Recover three enchanted gunpowder charges.");
    }

    public void CollectCharge()
    {
        if (!questStarted)
        {
            StartQuest();
        }

        if (chargesComplete)
            return;

        collectedCharges++;
        collectedCharges = Mathf.Min(collectedCharges, requiredCharges);

        Debug.Log(
            "Gunpowder Charges: "
            + collectedCharges
            + "/"
            + requiredCharges
        );

        if (collectedCharges >= requiredCharges)
        {
            chargesComplete = true;
            Debug.Log("All gunpowder charges collected!");
        }
    }

    public void CompleteQuest()
    {
        if (!chargesComplete)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Victory();
        }
    }

    public string GetObjectiveText()
    {
        if (!questStarted)
        {
            return "Objective: Speak to the village keeper";
        }

        if (!chargesComplete)
        {
            return "Enchanted Charges: "
                   + collectedCharges
                   + "/"
                   + requiredCharges;
        }

        return "Objective: Return to the village keeper";
    }

    public void ResetQuest()
    {
        collectedCharges = 0;
        questStarted = false;
        chargesComplete = false;
    }
}