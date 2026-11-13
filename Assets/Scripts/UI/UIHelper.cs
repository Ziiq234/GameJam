using UnityEngine;

// Small drawing helpers so all the on-screen text is easy to read.
public static class UIHelper
{
    private static Texture2D whiteTexture;
    private static GUIStyle promptStyle;
    private static GUIStyle nameStyle;

    public static Texture2D White
    {
        get
        {
            if (whiteTexture == null)
            {
                whiteTexture = new Texture2D(1, 1);
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.Apply();
            }
            return whiteTexture;
        }
    }

    // solid coloured rectangle
    public static void DrawRect(Rect rect, Color colour)
    {
        Color old = GUI.color;
        GUI.color = colour;
        GUI.DrawTexture(rect, White);
        GUI.color = old;
    }

    // dark panel with a thin gold border
    public static void DrawPanel(Rect rect)
    {
        DrawRect(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), new Color(0.75f, 0.6f, 0.3f));
        DrawRect(rect, new Color(0.08f, 0.07f, 0.1f, 0.95f));
    }

    // "Press E to ..." message above the dialogue area
    public static void DrawPrompt(string text)
    {
        if (promptStyle == null)
        {
            promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 14;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = Color.white;
        }

        Rect rect = new Rect(Screen.width / 2f - 140, Screen.height - 170, 280, 28);
        DrawPanel(rect);
        GUI.Label(rect, text, promptStyle);
    }

    // small name floating above a character's head
    public static void DrawNameTag(Vector3 worldPosition, string label)
    {
        if (Camera.main == null)
            return;

        if (nameStyle == null)
        {
            nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize = 12;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);
        }

        Vector3 screen = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 1f);

        // off screen
        if (screen.z < 0 || screen.x < 0 || screen.x > Screen.width || screen.y < 0 || screen.y > Screen.height)
            return;

        Vector2 size = nameStyle.CalcSize(new GUIContent(label));
        Rect rect = new Rect(screen.x - size.x / 2f - 3, Screen.height - screen.y - 9, size.x + 6, 18);

        DrawRect(rect, new Color(0, 0, 0, 0.6f));
        GUI.Label(rect, label, nameStyle);
    }
}
