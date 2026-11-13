using UnityEngine;
using UnityEngine.InputSystem;

// ============================================================
// DEV TOOLS for testing the story quickly:
//   L = teleport to whoever the quest wants you to see next
//   K = complete the current quest step
// ============================================================
public class DevMenu : MonoBehaviour
{
    // makes this exist in every scene without adding it to any scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        GameObject dev = new GameObject("DevMenu");
        dev.AddComponent<DevMenu>();
        DontDestroyOnLoad(dev);
    }

    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            TeleportToQuestTarget();
        }

        if (Keyboard.current.kKey.wasPressedThisFrame && QuestManager.Instance != null)
        {
            QuestManager.Instance.DevCompleteStep();
            Debug.Log("DEV: quest step is now " + QuestManager.Instance.Step);
        }
    }

    private void TeleportToQuestTarget()
    {
        if (QuestManager.Instance == null)
            return;

        GameObject target = GameObject.Find(GetTargetName(QuestManager.Instance.Step));
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (target == null || player == null)
        {
            Debug.Log("DEV: nothing to teleport to in this scene");
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

        Debug.Log("DEV: teleported to " + target.name);
    }

    private string GetTargetName(QuestManager.QuestStep step)
    {
        switch (step)
        {
            case QuestManager.QuestStep.DeliverMessage:
            case QuestManager.QuestStep.VisitTheodor:
                return "Theodor";
            case QuestManager.QuestStep.BuySword:
                return "Blacksmith";
            case QuestManager.QuestStep.ForestDungeon:
                return "DungeonEntrance";
            case QuestManager.QuestStep.CollectTaxes:
                return "Mage";
            case QuestManager.QuestStep.WizardDungeon:
                return "WizardDungeonEntrance";
            default:
                return "Cornelius";
        }
    }

    private System.Collections.IEnumerator TurnInterpolationBackOn(Rigidbody2D rb, RigidbodyInterpolation2D interpolation)
    {
        yield return new WaitForFixedUpdate();
        rb.interpolation = interpolation;
    }
}
