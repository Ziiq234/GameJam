using UnityEngine;
using UnityEngine.InputSystem;

// ============================================================
// DEV ONLY - admin menu for testing. Press L to open/close.
// Delete this whole file (and QuestManager.DevCompleteStep)
// before submitting the game!
// ============================================================
public class DevMenu : MonoBehaviour
{
    private bool open;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;

    private readonly string[] teleportTargets =
    {
        "Cornelius", "Blacksmith", "Theodor", "Mage",
        "DungeonEntrance", "Dock_ToHalberd", "Dock_ToWizardIsland"
    };

    // makes the menu exist in every scene without adding it to any scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateMenu()
    {
        GameObject menu = new GameObject("DevMenu");
        menu.AddComponent<DevMenu>();
        DontDestroyOnLoad(menu);
    }

    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            open = !open;
        }
    }

    private void OnGUI()
    {
        if (!open)
            return;

        GUI.depth = -200;

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(1f, 0.4f, 0.4f);

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12;
        }

        Rect box = new Rect(10, 110, 220, 420);
        UIHelper.DrawPanel(box);

        float y = box.y + 8;
        GUI.Label(new Rect(box.x + 8, y, 200, 20), "DEV MENU (L to close)", titleStyle);
        y += 24;

        QuestManager quest = QuestManager.Instance;

        if (quest != null)
        {
            GUI.Label(new Rect(box.x + 8, y, 200, 20), "Step: " + quest.Step, buttonStyle);
            y += 24;

            if (GUI.Button(new Rect(box.x + 8, y, 204, 24), "Complete current step", buttonStyle))
                quest.DevCompleteStep();
            y += 28;

            if (GUI.Button(new Rect(box.x + 8, y, 100, 24), "+100 gold", buttonStyle))
                quest.AddGold(100);
            if (GUI.Button(new Rect(box.x + 112, y, 100, 24), "Full heal", buttonStyle))
                HealPlayer();
            y += 34;
        }

        GUI.Label(new Rect(box.x + 8, y, 200, 20), "Teleport to:", titleStyle);
        y += 22;

        foreach (string target in teleportTargets)
        {
            if (GUI.Button(new Rect(box.x + 8, y, 204, 24), target, buttonStyle))
                TeleportTo(target);
            y += 28;
        }
    }

    private void HealPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            player.GetComponent<PlayerHealth>().HealFull();
        }
    }

    private void TeleportTo(string targetName)
    {
        GameObject target = GameObject.Find(targetName);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (target == null || player == null)
        {
            Debug.Log("DEV: can't find " + targetName + " in this scene");
            return;
        }

        // land just below them so we don't end up inside them
        Vector3 spot = target.transform.position + Vector3.down * 1.2f;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        RigidbodyInterpolation2D oldInterpolation = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.linearVelocity = Vector2.zero;
        rb.position = spot;
        player.transform.position = spot;
        Physics2D.SyncTransforms();
        StartCoroutine(TurnInterpolationBackOn(rb, oldInterpolation));

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(spot.x, spot.y, Camera.main.transform.position.z);
        }
    }

    private System.Collections.IEnumerator TurnInterpolationBackOn(Rigidbody2D rb, RigidbodyInterpolation2D interpolation)
    {
        yield return new WaitForFixedUpdate();
        rb.interpolation = interpolation;
    }
}
