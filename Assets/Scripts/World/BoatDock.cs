using UnityEngine;
using UnityEngine.InputSystem;

// Press E at a dock to sail to the dock it's linked to.
public class BoatDock : MonoBehaviour
{
    [SerializeField] private BoatDock destination;
    [SerializeField] private string destinationName = "Halberd";
    [SerializeField] private float useRange = 1.8f;

    private Transform player;
    private bool playerNearby;

    // the dock the player is sailing from (only that one draws the fade)
    private static BoatDock activeDock;
    private float fade;
    private GUIStyle sailStyle;

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
        if (player == null || destination == null)
            return;

        playerNearby = Vector2.Distance(transform.position, player.position) <= useRange;

        if (playerNearby && activeDock == null && !DialogueBox.Busy && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(SailTrip());
        }
    }

    private System.Collections.IEnumerator SailTrip()
    {
        activeDock = this;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        movement.enabled = false;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // fade out
        while (fade < 1f)
        {
            fade += Time.deltaTime * 2.5f;
            yield return null;
        }

        Sail();
        yield return new WaitForSeconds(0.8f);

        // fade back in
        while (fade > 0f)
        {
            fade -= Time.deltaTime * 2.5f;
            yield return null;
        }

        fade = 0f;
        movement.enabled = true;
        activeDock = null;
    }

    private void Sail()
    {
        Debug.Log("Sailing to " + destinationName);

        Vector3 arrival = destination.transform.position;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        // interpolation would drag the player back to where they were,
        // so turn it off for the move and back on after the next physics step
        RigidbodyInterpolation2D oldInterpolation = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.linearVelocity = Vector2.zero;
        rb.position = arrival;
        player.position = arrival;
        Physics2D.SyncTransforms();
        StartCoroutine(TurnInterpolationBackOn(rb, oldInterpolation));

        // snap the camera so it doesn't slide across the whole map
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(arrival.x, arrival.y, Camera.main.transform.position.z);
        }
    }

    private System.Collections.IEnumerator TurnInterpolationBackOn(Rigidbody2D rb, RigidbodyInterpolation2D interpolation)
    {
        yield return new WaitForFixedUpdate();
        rb.interpolation = interpolation;
    }

    private void OnGUI()
    {
        if (activeDock == this)
        {
            // draw on top of the HUD and minimap
            GUI.depth = -100;

            if (sailStyle == null)
            {
                sailStyle = new GUIStyle(GUI.skin.label);
                sailStyle.fontSize = 22;
                sailStyle.fontStyle = FontStyle.Bold;
                sailStyle.alignment = TextAnchor.MiddleCenter;
                sailStyle.normal.textColor = Color.white;
            }

            UIHelper.DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, Mathf.Clamp01(fade)));
            GUI.Label(new Rect(0, Screen.height / 2f - 20, Screen.width, 40), "Sailing to " + destinationName + "...", sailStyle);
            return;
        }

        if (playerNearby && activeDock == null && !DialogueBox.IsOpen)
        {
            UIHelper.DrawPrompt("Press E to sail to " + destinationName);
        }
    }
}
