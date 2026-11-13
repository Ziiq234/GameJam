using UnityEngine;

// Keeps track of where the player is in the main quest, plus gold and items.
// Lives across scenes so progress isn't lost going in and out of dungeons.
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public enum QuestStep
    {
        TalkToCornelius,
        DeliverMessage,
        ReturnToCornelius,
        ForestDungeon,
        BringChargesBack,
        CollectTaxes,
        BringTaxesBack,
        VisitTheodor,
        WizardDungeon,
        FinalReturn,
        Finished
    }

    [SerializeField] private int forestCharges = 2;
    [SerializeField] private int requiredCharges = 3;

    private QuestStep step;
    private int collectedCharges;
    private int gold;
    private bool carryingTaxes;
    private bool hasShield;
    private bool hasRing;

    public QuestStep Step => step;
    public int CollectedCharges => collectedCharges;
    public int RequiredCharges => requiredCharges;
    public int Gold => gold;
    public bool CarryingTaxes => carryingTaxes;
    public bool HasShield => hasShield;
    public bool HasRing => hasRing;

    // the shop opens once the forest dungeon is done
    public bool ShopUnlocked => step >= QuestStep.CollectTaxes;

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

    public void SetStep(QuestStep newStep)
    {
        step = newStep;
        Debug.Log("Quest step: " + step);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log("Gold: " + gold);
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;
        return true;
    }

    public void SetCarryingTaxes(bool value)
    {
        carryingTaxes = value;
    }

    public void GiveShield()
    {
        hasShield = true;
    }

    public void GiveRing()
    {
        hasRing = true;
    }

    // returns false if the player can't pick up a charge yet
    public bool CollectCharge()
    {
        if (step != QuestStep.ForestDungeon && step != QuestStep.WizardDungeon)
            return false;

        collectedCharges++;
        Debug.Log("Charges: " + collectedCharges + "/" + requiredCharges);

        if (step == QuestStep.ForestDungeon && collectedCharges >= forestCharges)
        {
            SetStep(QuestStep.BringChargesBack);
        }
        else if (step == QuestStep.WizardDungeon && collectedCharges >= requiredCharges)
        {
            SetStep(QuestStep.FinalReturn);
        }

        return true;
    }

    public void CompleteQuest()
    {
        SetStep(QuestStep.Finished);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Victory();
        }
    }

    public string GetObjectiveText()
    {
        switch (step)
        {
            case QuestStep.TalkToCornelius:
                return "Objective: Speak to Cornelius";
            case QuestStep.DeliverMessage:
                return "Objective: Take the message to Theodor in Halberd (boat on the north shore)";
            case QuestStep.ReturnToCornelius:
                return "Objective: Return to Cornelius";
            case QuestStep.ForestDungeon:
                return "Objective: Find the charges in the Dense Forest dungeon (" + collectedCharges + "/" + forestCharges + ")";
            case QuestStep.BringChargesBack:
                return "Objective: Bring the charges to Cornelius";
            case QuestStep.CollectTaxes:
                return "Objective: Collect the mage's taxes on Wizard Island (boat on the south shore)";
            case QuestStep.BringTaxesBack:
                return "Objective: Give the taxes to Cornelius";
            case QuestStep.VisitTheodor:
                return "Objective: Visit Theodor in Halberd";
            case QuestStep.WizardDungeon:
                return "Objective: Find the last charge in the Wizard Island dungeon";
            case QuestStep.FinalReturn:
                return "Objective: Bring the last charge to Cornelius";
            default:
                return "The festival is saved!";
        }
    }

    public void ResetQuest()
    {
        step = QuestStep.TalkToCornelius;
        collectedCharges = 0;
        gold = 0;
        carryingTaxes = false;
        hasShield = false;
        hasRing = false;
    }

    // still used by the old NPCInteraction script
    public bool QuestStarted => step != QuestStep.TalkToCornelius;
    public bool ChargesComplete => collectedCharges >= requiredCharges;

    public void StartQuest()
    {
        if (step == QuestStep.TalkToCornelius)
        {
            SetStep(QuestStep.DeliverMessage);
        }
    }
}
