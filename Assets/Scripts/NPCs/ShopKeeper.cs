using UnityEngine;
using UnityEngine.InputSystem;

// Blacksmith shop. Opens after you've delivered Cornelius's message.
// Press 1 or 2 (or click) to buy, E to leave.
public class ShopKeeper : MonoBehaviour
{
    [SerializeField] private int swordPrice = 75;
    [SerializeField] private int potionPrice = 15;
    [SerializeField] private float talkRange = 1.8f;

    private Transform player;
    private bool playerNearby;
    private string shopMessage = "";

    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;

    public static bool IsOpen { get; private set; }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        IsOpen = false;
    }

    private void Update()
    {
        if (player == null || QuestManager.Instance == null)
            return;

        playerNearby = Vector2.Distance(transform.position, player.position) <= talkRange;

        if (IsOpen)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) BuySword();
            if (Keyboard.current.digit2Key.wasPressedThisFrame) BuyPotion();

            if (Keyboard.current.eKey.wasPressedThisFrame || !playerNearby)
            {
                IsOpen = false;
            }
            return;
        }

        if (playerNearby && !DialogueBox.Busy && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenShop();
        }
    }

    private void OpenShop()
    {
        // the shop only opens once you've been to Halberd
        if (QuestManager.Instance.Step < QuestManager.QuestStep.ReturnToCornelius)
        {
            DialogueBox.Show("Blacksmith", new[]
            {
                "My forge isn't open to strangers.",
                "Do a job for Cornelius first and then we'll talk."
            });
            return;
        }

        shopMessage = "What can I get you?";
        IsOpen = true;
    }

    private void BuySword()
    {
        QuestManager quest = QuestManager.Instance;

        if (quest.HasSword)
        {
            shopMessage = "You've already got the best blade I make.";
        }
        else if (quest.SpendGold(swordPrice))
        {
            quest.GiveSword();
            SoundManager.Play("coin");
            shopMessage = "A fine choice. Mind the sharp end!";
        }
        else
        {
            shopMessage = "Not enough gold. Bats in the dungeon carry coins, you know.";
        }
    }

    private void BuyPotion()
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health.CurrentHealth >= health.MaxHealth)
        {
            shopMessage = "You look healthy enough to me.";
        }
        else if (QuestManager.Instance.SpendGold(potionPrice))
        {
            health.HealFull();
            SoundManager.Play("coin");
            shopMessage = "Drink up. Good as new.";
        }
        else
        {
            shopMessage = "Not enough gold for that.";
        }
    }

    private void OnGUI()
    {
        UIHelper.DrawNameTag(transform.position, "Blacksmith");

        if (!IsOpen)
        {
            if (playerNearby && !DialogueBox.IsOpen)
            {
                UIHelper.DrawPrompt("Press E to talk to the Blacksmith");
            }
            return;
        }

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 14;
            textStyle.wordWrap = true;
            textStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
        }

        QuestManager quest = QuestManager.Instance;
        Rect box = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 130, 400, 240);
        UIHelper.DrawPanel(box);

        GUI.Label(new Rect(box.x + 15, box.y + 10, 370, 26), "Blacksmith's Forge", titleStyle);
        GUI.Label(new Rect(box.x + 15, box.y + 38, 370, 22), "Your gold: " + quest.Gold, textStyle);

        string swordText = quest.HasSword ? "1. Iron Sword - SOLD" : "1. Iron Sword - " + swordPrice + " gold (more damage and reach)";
        if (GUI.Button(new Rect(box.x + 15, box.y + 68, 370, 32), swordText, buttonStyle)) BuySword();

        string potionText = "2. Health Potion - " + potionPrice + " gold (heal fully)";
        if (GUI.Button(new Rect(box.x + 15, box.y + 108, 370, 32), potionText, buttonStyle)) BuyPotion();

        GUI.Label(new Rect(box.x + 15, box.y + 150, 370, 44), "Blacksmith: " + shopMessage, textStyle);
        GUI.Label(new Rect(box.x + 15, box.y + 205, 370, 22), "Press E to leave", textStyle);
    }
}
