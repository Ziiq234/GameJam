using System.Collections.Generic;
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
        BuySword,
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
    [SerializeField] private int mageBatsNeeded = 5;

    private QuestStep step;
    private int collectedCharges;
    private int gold;
    private bool carryingTaxes;
    private bool hasShield;
    private bool hasRing;
    private bool hasSword;
    private bool mageTaskStarted;
    private bool mageDefeated;
    private bool hasMageSword;
    private int enemiesDefeated;
    private List<string> collectedChargeIds = new List<string>();

    // side quests: 0 = not started, 1 = in progress, 2 = ready to hand in, 3 = done
    private Dictionary<string, int> sideQuests = new Dictionary<string, int>();
    private int roadBatsKilled;
    private int roadBatsNeeded = 3;
    private float startTime;
    private float endTime = -1f;
    private int mageBatsKilled;

    public QuestStep Step => step;
    public int CollectedCharges => collectedCharges;
    public int RequiredCharges => requiredCharges;
    public int Gold => gold;
    public bool CarryingTaxes => carryingTaxes;
    public bool HasShield => hasShield;
    public bool HasRing => hasRing;
    public bool HasSword => hasSword;
    public bool MageTaskStarted => mageTaskStarted;
    public bool MageDefeated => mageDefeated;
    public bool HasMageSword => hasMageSword;
    public int EnemiesDefeated => enemiesDefeated;

    // player health carried between scenes (-1 means start with full health)
    public int SavedHealth { get; set; } = -1;

    // how long this playthrough has taken (stops when the ending starts)
    public float PlayTime => (endTime >= 0 ? endTime : Time.time) - startTime;
    public bool MageTaskDone => mageBatsKilled >= mageBatsNeeded;

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
        startTime = Time.time;
    }

    public void StopTimer()
    {
        endTime = Time.time;
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

    public void GiveSword()
    {
        hasSword = true;

        // buying the sword is part of the story
        if (step == QuestStep.BuySword)
        {
            SetStep(QuestStep.ForestDungeon);
        }
    }

    // the Mage's sword is better than the Blacksmith's
    public void GiveMageSword()
    {
        hasMageSword = true;
        hasSword = true;
    }

    // beating the Mage gets you his tax money
    public void DefeatMage()
    {
        mageDefeated = true;
        carryingTaxes = true;
        SetStep(QuestStep.BringTaxesBack);
    }

    public void StartMageTask()
    {
        mageTaskStarted = true;
    }

    // called by EnemyHealth when an enemy dies
    public void EnemyKilled(string area)
    {
        enemiesDefeated++;

        if (area == "ForestRoad" && GetSideQuest("Bats") == 1)
        {
            roadBatsKilled++;

            if (roadBatsKilled >= roadBatsNeeded)
            {
                SetSideQuest("Bats", 2);
            }
        }

        if (area == "WizardIsland" && mageTaskStarted && step == QuestStep.CollectTaxes)
        {
            mageBatsKilled++;
            Debug.Log("Wizard Island bats: " + mageBatsKilled + "/" + mageBatsNeeded);
        }
    }

    public int GetSideQuest(string questName)
    {
        int state;
        return sideQuests.TryGetValue(questName, out state) ? state : 0;
    }

    public void SetSideQuest(string questName, int state)
    {
        sideQuests[questName] = state;

        if (questName == "Bats" && state == 1)
        {
            roadBatsKilled = 0;
        }
    }

    // short text for the HUD listing side quests in progress
    public string GetSideQuestText()
    {
        string text = "";

        if (GetSideQuest("Necklace") == 1) text += "Find Rose's necklace on the north-east beach. ";
        if (GetSideQuest("Necklace") == 2) text += "Return the necklace to Rose. ";
        if (GetSideQuest("Bats") == 1) text += "Kill forest road bats for Tom (" + roadBatsKilled + "/" + roadBatsNeeded + "). ";
        if (GetSideQuest("Bats") == 2) text += "Tell Tom the bats are gone. ";
        if (GetSideQuest("Letter") == 1) text += "Give Theodor's letter to Ella in the start town. ";

        return text.Trim();
    }

    public bool HasCollected(string chargeId)
    {
        return collectedChargeIds.Contains(chargeId);
    }

    // returns false if the player can't pick up this charge yet
    public bool CollectCharge(string dungeon, string chargeId)
    {
        bool forestCharge = dungeon == "Forest" && step == QuestStep.ForestDungeon;
        bool wizardCharge = dungeon == "Wizard" && step == QuestStep.WizardDungeon;

        if (!forestCharge && !wizardCharge)
            return false;

        collectedChargeIds.Add(chargeId);
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
            GameManager.Instance.StartEnding();
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
            case QuestStep.BuySword:
                return "Objective: Buy a sword from the Blacksmith (75 gold). Bats near the forest road drop gold";
            case QuestStep.ForestDungeon:
                return "Objective: Find the charges in the Dense Forest dungeon (" + collectedCharges + "/" + forestCharges + ")";
            case QuestStep.BringChargesBack:
                return "Objective: Bring the charges to Cornelius";
            case QuestStep.CollectTaxes:
                if (!mageTaskStarted)
                    return "Objective: Collect the mage's taxes on Wizard Island (boat on the south shore)";
                if (!MageTaskDone)
                    return "Objective: Clear the bats on Wizard Island (" + mageBatsKilled + "/" + mageBatsNeeded + ")";
                return "Objective: Confront the Mage";
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
        hasSword = false;
        mageTaskStarted = false;
        mageBatsKilled = 0;
        mageDefeated = false;
        hasMageSword = false;
        enemiesDefeated = 0;
        collectedChargeIds.Clear();
        sideQuests.Clear();
        roadBatsKilled = 0;
        SavedHealth = -1;
        startTime = Time.time;
        endTime = -1f;
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

    // DEV ONLY - used by DevMenu to skip the current quest step. Delete before submitting!
    public void DevCompleteStep()
    {
        switch (step)
        {
            case QuestStep.TalkToCornelius: SetStep(QuestStep.DeliverMessage); break;
            case QuestStep.DeliverMessage: AddGold(50); SetStep(QuestStep.ReturnToCornelius); break;
            case QuestStep.ReturnToCornelius: SetStep(hasSword ? QuestStep.ForestDungeon : QuestStep.BuySword); break;
            case QuestStep.BuySword: GiveSword(); break;
            case QuestStep.ForestDungeon: collectedCharges = forestCharges; SetStep(QuestStep.BringChargesBack); break;
            case QuestStep.BringChargesBack: AddGold(100); SetStep(QuestStep.CollectTaxes); break;
            case QuestStep.CollectTaxes: mageTaskStarted = true; mageBatsKilled = mageBatsNeeded; DefeatMage(); GiveMageSword(); break;
            case QuestStep.BringTaxesBack: carryingTaxes = false; GiveShield(); SetStep(QuestStep.VisitTheodor); break;
            case QuestStep.VisitTheodor: SetStep(QuestStep.WizardDungeon); break;
            case QuestStep.WizardDungeon: collectedCharges = requiredCharges; SetStep(QuestStep.FinalReturn); break;
            case QuestStep.FinalReturn: GiveRing(); CompleteQuest(); break;
        }
    }
}
