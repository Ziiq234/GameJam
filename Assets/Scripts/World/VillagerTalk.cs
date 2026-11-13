using UnityEngine;
using UnityEngine.InputSystem;

// Lets the player chat to villagers. Some villagers also have a small side quest.
// Side quest states: 0 = not started, 1 = in progress, 2 = ready to hand in, 3 = done
public class VillagerTalk : MonoBehaviour
{
    [SerializeField] private string villagerName = "Villager";
    [Tooltip("Necklace, Bats, Letter or leave empty")]
    [SerializeField] private string sideQuest = "";
    [SerializeField] private string[] chatLines = { "Lovely day for it.", "Can't wait for the festival!" };
    [SerializeField] private float talkRange = 1.5f;

    private Transform player;
    private bool playerNearby;
    private int nextChatLine;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null || QuestManager.Instance == null)
            return;

        playerNearby = Vector2.Distance(transform.position, player.position) <= talkRange;

        if (playerNearby && !DialogueBox.Busy && !ShopKeeper.IsOpen && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Talk();
        }
    }

    private void Talk()
    {
        QuestManager quest = QuestManager.Instance;

        if (sideQuest == "Necklace" && quest.GetSideQuest("Necklace") < 3)
            NecklaceQuest(quest);
        else if (sideQuest == "Bats" && quest.GetSideQuest("Bats") < 3)
            BatQuest(quest);
        else if (sideQuest == "Letter" && quest.GetSideQuest("Letter") == 1)
            LetterQuest(quest);
        else
            Chat();
    }

    private void NecklaceQuest(QuestManager quest)
    {
        int state = quest.GetSideQuest("Necklace");

        if (state == 0)
        {
            Say(new[]
            {
                "Oh no, oh no... I've lost my mother's necklace!",
                "I was walking along the beach north-east of town. Could you look for it?"
            }, () => quest.SetSideQuest("Necklace", 1));
        }
        else if (state == 1)
        {
            Say(new[] { "Any luck? I'm sure I dropped it on the beach north-east of town." });
        }
        else
        {
            Say(new[] { "My necklace! Thank you so much!", "Here, take 25 gold. It's all I've got." }, () =>
            {
                quest.AddGold(25);
                quest.SetSideQuest("Necklace", 3);
            });
        }
    }

    private void BatQuest(QuestManager quest)
    {
        int state = quest.GetSideQuest("Bats");

        if (state == 0)
        {
            Say(new[]
            {
                "Those bats on the forest road keep eating my crops!",
                "Kill three of them for me and I'll make it worth your while."
            }, () => quest.SetSideQuest("Bats", 1));
        }
        else if (state == 1)
        {
            Say(new[] { "Still bats out there. Three of them, on the forest road south-east of town." });
        }
        else
        {
            Say(new[] { "You did it! My crops are safe.", "Here's 30 gold, and have some of my soup. You look like you need it." }, () =>
            {
                quest.AddGold(30);
                player.GetComponent<PlayerHealth>().HealFull();
                quest.SetSideQuest("Bats", 3);
            });
        }
    }

    private void LetterQuest(QuestManager quest)
    {
        Say(new[]
        {
            "A letter from Theodor? For me?",
            "...He wants to dance with me at the festival! Thank you! Here's 20 gold."
        }, () =>
        {
            quest.AddGold(20);
            quest.SetSideQuest("Letter", 3);
        });
    }

    private void Chat()
    {
        if (chatLines.Length == 0)
            return;

        // go through their lines in order so they don't repeat straight away
        Say(new[] { chatLines[nextChatLine] });
        nextChatLine = (nextChatLine + 1) % chatLines.Length;
    }

    private bool HasQuestForPlayer()
    {
        QuestManager quest = QuestManager.Instance;
        if (quest == null) return false;

        if (sideQuest == "Necklace") return quest.GetSideQuest("Necklace") != 1 && quest.GetSideQuest("Necklace") < 3;
        if (sideQuest == "Bats") return quest.GetSideQuest("Bats") != 1 && quest.GetSideQuest("Bats") < 3;
        if (sideQuest == "Letter") return quest.GetSideQuest("Letter") == 1;
        return false;
    }

    private void Say(string[] lines, System.Action afterwards = null)
    {
        DialogueBox.Show(villagerName, lines, afterwards);
    }

    private void OnGUI()
    {
        string label = HasQuestForPlayer() ? villagerName + " !" : villagerName;
        UIHelper.DrawNameTag(transform.position, label);

        if (playerNearby && !DialogueBox.IsOpen && !ShopKeeper.IsOpen)
        {
            UIHelper.DrawPrompt("Press E to talk to " + villagerName);
        }
    }
}
