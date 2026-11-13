using UnityEngine;
using UnityEngine.InputSystem;

// Minimap in the top right corner. A second camera looks down on the map
// and draws into a texture. Shows the player (white) and where the quest
// wants you to go (yellow). Hold M to see the whole map.
public class MiniMap : MonoBehaviour
{
    [SerializeField] private float zoomedInSize = 22f;
    [SerializeField] private float wholeMapSize = 90f;
    [SerializeField] private Vector2 wholeMapCentre = new Vector2(18f, -11f);
    [SerializeField] private int mapSize = 170;
    [SerializeField] private int bigMapSize = 420;

    private Camera mapCamera;
    private RenderTexture mapTexture;
    private Transform player;
    private bool showWholeMap;
    private GUIStyle labelStyle;

    private Transform questTarget;
    private QuestManager.QuestStep lastStep;
    private bool targetFound;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        mapTexture = new RenderTexture(512, 512, 16);

        GameObject cameraObject = new GameObject("MiniMapCamera");
        mapCamera = cameraObject.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = zoomedInSize;
        mapCamera.targetTexture = mapTexture;
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.backgroundColor = new Color(0.15f, 0.3f, 0.6f);
    }

    private void LateUpdate()
    {
        if (mapCamera == null || player == null)
            return;

        showWholeMap = Keyboard.current.mKey.isPressed;

        if (showWholeMap)
        {
            mapCamera.orthographicSize = wholeMapSize;
            mapCamera.transform.position = new Vector3(wholeMapCentre.x, wholeMapCentre.y, -20f);
        }
        else
        {
            mapCamera.orthographicSize = zoomedInSize;
            mapCamera.transform.position = new Vector3(player.position.x, player.position.y, -20f);
        }
    }

    private void OnGUI()
    {
        if (mapCamera == null || player == null)
            return;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = Color.white;
        }

        int size = showWholeMap ? bigMapSize : mapSize;
        Rect area = showWholeMap
            ? new Rect(Screen.width / 2f - size / 2f, Screen.height / 2f - size / 2f, size, size)
            : new Rect(Screen.width - size - 12, 12, size, size);

        UIHelper.DrawPanel(area);
        GUI.DrawTexture(area, mapTexture);

        // quest marker, kept on the edge of the map if it's too far away
        Transform target = GetQuestTarget();

        if (target != null)
        {
            Vector2 point = WorldToMap(target.position, area, true);
            UIHelper.DrawRect(new Rect(point.x - 7, point.y - 7, 14, 14), Color.black);
            UIHelper.DrawRect(new Rect(point.x - 5, point.y - 5, 10, 10), Color.yellow);
        }

        Vector2 playerPoint = WorldToMap(player.position, area, false);
        UIHelper.DrawRect(new Rect(playerPoint.x - 5, playerPoint.y - 5, 10, 10), Color.black);
        UIHelper.DrawRect(new Rect(playerPoint.x - 3, playerPoint.y - 3, 6, 6), Color.white);

        string hint = showWholeMap ? "Yellow = where to go" : "Hold M for full map";
        UIHelper.DrawRect(new Rect(area.x, area.yMax + 2, area.width, 18), new Color(0, 0, 0, 0.6f));
        GUI.Label(new Rect(area.x, area.yMax + 2, area.width, 18), hint, labelStyle);
    }

    // turns a world position into a point on the minimap
    private Vector2 WorldToMap(Vector3 worldPosition, Rect area, bool clampToEdge)
    {
        Vector3 offset = worldPosition - mapCamera.transform.position;
        float viewSize = mapCamera.orthographicSize;

        float x = offset.x / viewSize;
        float y = offset.y / viewSize;

        if (clampToEdge)
        {
            x = Mathf.Clamp(x, -0.95f, 0.95f);
            y = Mathf.Clamp(y, -0.95f, 0.95f);
        }

        // GUI y goes downwards so flip it
        return new Vector2(area.center.x + x * area.width / 2f, area.center.y - y * area.height / 2f);
    }

    // only searches for the target again when the quest step changes
    private Transform GetQuestTarget()
    {
        if (QuestManager.Instance == null)
            return null;

        if (!targetFound || lastStep != QuestManager.Instance.Step)
        {
            lastStep = QuestManager.Instance.Step;
            questTarget = FindQuestTarget();
            targetFound = true;
        }

        return questTarget;
    }

    private Transform FindQuestTarget()
    {
        string targetName;

        switch (QuestManager.Instance.Step)
        {
            case QuestManager.QuestStep.DeliverMessage:
            case QuestManager.QuestStep.VisitTheodor:
                targetName = "Theodor";
                break;
            case QuestManager.QuestStep.ForestDungeon:
                targetName = "DungeonEntrance";
                break;
            case QuestManager.QuestStep.CollectTaxes:
                targetName = "Mage";
                break;
            case QuestManager.QuestStep.WizardDungeon:
                targetName = "WizardDungeonEntrance";
                break;
            case QuestManager.QuestStep.Finished:
                return null;
            default:
                targetName = "Cornelius";
                break;
        }

        GameObject target = GameObject.Find(targetName);
        return target != null ? target.transform : null;
    }
}
