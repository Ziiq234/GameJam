using UnityEngine;
using UnityEngine.InputSystem;

// Shows lines of dialogue one at a time at the bottom of the screen.
// Press E to go to the next line. Other scripts call DialogueBox.Show(...).
public class DialogueBox : MonoBehaviour
{
    private static DialogueBox instance;

    private string speaker;
    private string[] lines;
    private int lineIndex;
    private System.Action onFinished;
    private int openedFrame;
    private int closedFrame = -1;

    private GUIStyle nameStyle;
    private GUIStyle textStyle;
    private GUIStyle hintStyle;

    public static bool IsOpen => instance != null && instance.lines != null;

    // stops the E press that closes the box from starting the chat again
    public static bool Busy => IsOpen || (instance != null && instance.closedFrame == Time.frameCount);

    public static void Show(string speakerName, string[] dialogueLines, System.Action whenFinished = null)
    {
        // make the box the first time it's needed
        if (instance == null)
        {
            GameObject box = new GameObject("DialogueBox");
            instance = box.AddComponent<DialogueBox>();
        }

        instance.speaker = speakerName;
        instance.lines = dialogueLines;
        instance.lineIndex = 0;
        instance.onFinished = whenFinished;
        instance.openedFrame = Time.frameCount;
    }

    private void Update()
    {
        if (lines == null)
            return;

        // ignore the same E press that opened the box
        if (Time.frameCount == openedFrame)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            lineIndex++;

            if (lineIndex >= lines.Length)
            {
                lines = null;
                closedFrame = Time.frameCount;

                // run whatever should happen after the conversation
                System.Action finished = onFinished;
                onFinished = null;
                finished?.Invoke();
            }
        }
    }

    private void OnGUI()
    {
        if (lines == null)
            return;

        if (textStyle == null)
        {
            nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize = 16;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 15;
            textStyle.wordWrap = true;
            textStyle.normal.textColor = Color.white;

            hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.fontSize = 12;
            hintStyle.alignment = TextAnchor.LowerRight;
            hintStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
        }

        Rect box = new Rect(Screen.width / 2f - 300, Screen.height - 130, 600, 115);
        UIHelper.DrawPanel(box);

        GUI.Label(new Rect(box.x + 15, box.y + 8, 570, 24), speaker, nameStyle);
        GUI.Label(new Rect(box.x + 15, box.y + 32, 570, 60), lines[lineIndex], textStyle);
        GUI.Label(new Rect(box.x + 15, box.y + 92, 570, 20), "E to continue", hintStyle);
    }
}
