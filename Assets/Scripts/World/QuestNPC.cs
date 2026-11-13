using UnityEngine;
using UnityEngine.InputSystem;

// Story characters. What they say depends on how far through the quest you are.
public class QuestNPC : MonoBehaviour
{
    public enum Character { Cornelius, Theodor, Mage }

    [SerializeField] private Character character;
    [SerializeField] private float talkRange = 1.8f;

    private Transform player;
    private bool playerNearby;

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

        if (playerNearby && !DialogueBox.Busy && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Talk();
        }
    }

    private void Talk()
    {
        if (character == Character.Cornelius)
            TalkCornelius();
        else if (character == Character.Theodor)
            TalkTheodor();
        else
            TalkMage();
    }

    private void TalkCornelius()
    {
        QuestManager quest = QuestManager.Instance;

        switch (quest.Step)
        {
            case QuestManager.QuestStep.TalkToCornelius:
                Say(new[]
                {
                    "Ah, a traveller! You're just who I needed.",
                    "Could you take this message to my old friend Theodor? He lives in Halberd, over the water to the north-west.",
                    "There's a boat on the north shore. Off you go!"
                }, () => quest.SetStep(QuestManager.QuestStep.DeliverMessage));
                break;

            case QuestManager.QuestStep.DeliverMessage:
                Say(new[] { "Theodor lives in Halberd, north-west. The boat is on the north shore." });
                break;

            case QuestManager.QuestStep.ReturnToCornelius:
                Say(new[]
                {
                    "So Theodor told you about the rocket charges...",
                    "Every year we hold a firework festival, but this year the enchanted charges were stolen and hidden in the dungeons.",
                    "There's a dungeon in the dense forest to the south-east. Bring me back the charges and I'll pay you well.",
                    "But you're not going down there with your bare hands. Buy a sword from the Blacksmith, just south of here.",
                    "Theodor's gold won't quite cover it. The bats along the forest road carry coins, go and earn the rest."
                }, () =>
                {
                    if (quest.HasSword)
                        quest.SetStep(QuestManager.QuestStep.ForestDungeon);
                    else
                        quest.SetStep(QuestManager.QuestStep.BuySword);
                });
                break;

            case QuestManager.QuestStep.BuySword:
                Say(new[] { "Get that sword from the Blacksmith first. 75 gold. The bats on the forest road drop coins." });
                break;

            case QuestManager.QuestStep.ForestDungeon:
                Say(new[]
                {
                    "The dense forest is to the south-east. Bring me those charges!",
                    "If you need a better weapon, the Blacksmith just south of here sells swords."
                });
                break;

            case QuestManager.QuestStep.BringChargesBack:
                Say(new[]
                {
                    "You found them! The festival might happen after all.",
                    "Here's 100 gold, as promised.",
                    "One more favour... the mage on Wizard Island hasn't paid his taxes. Go and collect his 50 gold.",
                    "The boat to Wizard Island is on the south shore."
                }, () =>
                {
                    quest.AddGold(100);
                    quest.SetStep(QuestManager.QuestStep.CollectTaxes);
                });
                break;

            case QuestManager.QuestStep.CollectTaxes:
                Say(new[] { "The mage lives on Wizard Island, to the south. Get his 50 gold tax." });
                break;

            case QuestManager.QuestStep.BringTaxesBack:
                Say(new[]
                {
                    "The tax money! And the mage attacked you? Well, he always was a bad loser.",
                    "Take this shield. It was my father's, and you'll need it more than me.",
                    "Oh, and Theodor sent word. He wants to see you in Halberd again."
                }, () =>
                {
                    quest.SetCarryingTaxes(false);
                    quest.GiveShield();
                    quest.SetStep(QuestManager.QuestStep.VisitTheodor);
                });
                break;

            case QuestManager.QuestStep.VisitTheodor:
                Say(new[] { "Theodor wants to see you in Halberd." });
                break;

            case QuestManager.QuestStep.WizardDungeon:
                Say(new[] { "The last charge is in the dungeon on Wizard Island. Be careful in there." });
                break;

            case QuestManager.QuestStep.FinalReturn:
                Say(new[]
                {
                    "The final charge! You've saved the festival!",
                    "Please, take this enchanted ring as thanks.",
                    "Now... let the fireworks begin!"
                }, () =>
                {
                    quest.GiveRing();
                    quest.CompleteQuest();
                });
                break;

            default:
                Say(new[] { "Thank you for everything. Enjoy the festival!" });
                break;
        }
    }

    private void TalkTheodor()
    {
        QuestManager quest = QuestManager.Instance;

        switch (quest.Step)
        {
            case QuestManager.QuestStep.TalkToCornelius:
                Say(new[] { "Hello stranger. Welcome to Halberd." });
                break;

            case QuestManager.QuestStep.DeliverMessage:
                Say(new[]
                {
                    "A message from Cornelius? Thank you!",
                    "Here's 50 gold for your trouble.",
                    "Have you heard about the mysterious rocket charges? They say they can only be found in secret dungeons...",
                    "But you should head back to Cornelius first. I think he knows more than he's letting on."
                }, () =>
                {
                    quest.AddGold(50);
                    quest.SetStep(QuestManager.QuestStep.ReturnToCornelius);
                });
                break;

            case QuestManager.QuestStep.VisitTheodor:
                Say(new[]
                {
                    "You're back! I've heard there's another dungeon, on Wizard Island.",
                    "Evil wizards guard it, and they've got the last rocket charge. Be careful."
                }, () => quest.SetStep(QuestManager.QuestStep.WizardDungeon));
                break;

            case QuestManager.QuestStep.WizardDungeon:
                Say(new[] { "The wizard dungeon is on Wizard Island, to the south." });
                break;

            case QuestManager.QuestStep.ReturnToCornelius:
            case QuestManager.QuestStep.BringChargesBack:
            case QuestManager.QuestStep.BringTaxesBack:
            case QuestManager.QuestStep.FinalReturn:
                Say(new[] { "Cornelius will be waiting for you back in the start town." });
                break;

            case QuestManager.QuestStep.ForestDungeon:
                Say(new[] { "The dense forest is south-east of the start town. Watch out for the bats." });
                break;

            case QuestManager.QuestStep.CollectTaxes:
                Say(new[] { "Getting money out of that mage? Good luck with that!" });
                break;

            default:
                Say(new[] { "Good luck out there, friend." });
                break;
        }
    }

    private void TalkMage()
    {
        QuestManager quest = QuestManager.Instance;

        if (quest.Step == QuestManager.QuestStep.CollectTaxes)
        {
            if (!quest.MageTaskStarted)
            {
                Say(new[]
                {
                    "Taxes? TAXES?!",
                    "How am I meant to earn money with bats nesting all over my island?",
                    "Get rid of five of them and maybe I'll think about paying."
                }, () => quest.StartMageTask());
            }
            else if (!quest.MageTaskDone)
            {
                Say(new[] { "There are still bats flapping about. I'm not paying until they're gone!" });
            }
            else
            {
                Say(new[]
                {
                    "You killed my bats?! Those were my pets!",
                    "Taxes? I'll pay you in FIREBALLS!"
                }, () => GetComponent<MageBoss>().StartFight());
            }
        }
        else if (quest.Step == QuestManager.QuestStep.BringTaxesBack)
        {
            Say(new[] { "You've got my gold. Take it to Cornelius and leave me be!" });
        }
        else
        {
            Say(new[] { "Hmph. What do you want?" });
        }
    }

    private void Say(string[] lines, System.Action afterwards = null)
    {
        DialogueBox.Show(character.ToString(), lines, afterwards);
    }

    private void OnGUI()
    {
        UIHelper.DrawNameTag(transform.position, character.ToString());

        if (playerNearby && !DialogueBox.IsOpen)
        {
            UIHelper.DrawPrompt("Press E to talk to " + character);
        }
    }
}
