using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;

    private Transform player;
    private bool playerNearby;

    private string currentMessage = "";
    private float messageEndTime;

    private GUIStyle messageStyle;
    private GUIStyle boxStyle;

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
        if (player == null)
            return;

        playerNearby =
            Vector2.Distance(transform.position, player.position)
            <= interactionRange;

        if (playerNearby &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (QuestManager.Instance == null)
            return;

        if (!QuestManager.Instance.QuestStarted)
        {
            QuestManager.Instance.StartQuest();

            ShowMessage(
                "Village Keeper: Three enchanted gunpowder charges were stolen. Recover them from the dungeon!"
            );
        }
        else if (!QuestManager.Instance.ChargesComplete)
        {
            ShowMessage(
                "Village Keeper: The festival cannot begin until all three charges are returned."
            );
        }
        else
        {
            ShowMessage(
                "Village Keeper: You recovered them! The festival can begin!"
            );

            QuestManager.Instance.CompleteQuest();
        }
    }

    private void ShowMessage(string message)
    {
        currentMessage = message;
        messageEndTime = Time.unscaledTime + 4f;
    }

    private void OnGUI()
    {
        if (playerNearby)
        {
            GUI.Label(
                new Rect(
                    Screen.width / 2f - 100,
                    Screen.height - 130,
                    200,
                    35
                ),
                "Press E to interact"
            );
        }

        if (Time.unscaledTime < messageEndTime)
        {
            if (messageStyle == null)
            {
                messageStyle = new GUIStyle(GUI.skin.label);
                messageStyle.fontSize = 19;
                messageStyle.wordWrap = true;
                messageStyle.alignment = TextAnchor.MiddleCenter;
                messageStyle.normal.textColor = Color.white;

                boxStyle = new GUIStyle(GUI.skin.box);
            }

            GUI.Box(
                new Rect(
                    Screen.width / 2f - 350,
                    Screen.height - 110,
                    700,
                    90
                ),
                "",
                boxStyle
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 330,
                    Screen.height - 100,
                    660,
                    70
                ),
                currentMessage,
                messageStyle
            );
        }
    }
}