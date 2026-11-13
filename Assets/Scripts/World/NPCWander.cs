using UnityEngine;
using UnityEngine.Tilemaps;

// Makes a villager walk between random spots on the path tiles near where
// they start, wait a bit, then pick somewhere else.
public class NPCWander : MonoBehaviour
{
    [SerializeField] private Tilemap pathTilemap;
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float wanderRadius = 6f;
    [SerializeField] private float minWait = 0.5f;
    [SerializeField] private float maxWait = 1.5f;
    [SerializeField] private float stopNearPlayer = 1.2f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 startPosition;
    private Vector2 target;
    private bool walking;
    private float waitTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        waitTimer = Random.Range(minWait, maxWait);
    }

    private void FixedUpdate()
    {
        // stop if the player is close so they can't get shoved about
        if (player != null && Vector2.Distance(rb.position, player.position) < stopNearPlayer)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (walking)
        {
            Vector2 toTarget = target - rb.position;

            if (toTarget.magnitude < 0.1f)
            {
                walking = false;
                rb.linearVelocity = Vector2.zero;
                waitTimer = Random.Range(minWait, maxWait);
            }
            else
            {
                rb.linearVelocity = toTarget.normalized * moveSpeed;
            }
        }
        else
        {
            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0)
            {
                PickNewTarget();
            }
        }
    }

    private void PickNewTarget()
    {
        // try a few random spots and use the first one we can walk straight to
        for (int i = 0; i < 10; i++)
        {
            Vector2 spot = startPosition + Random.insideUnitCircle * wanderRadius;
            Vector3Int cell = pathTilemap.WorldToCell(spot);
            Vector2 cellCentre = pathTilemap.GetCellCenterWorld(cell);

            if (pathTilemap.HasTile(cell) && PathIsClear(rb.position, cellCentre))
            {
                target = cellCentre;
                walking = true;
                return;
            }
        }

        // nowhere found this time, wait and try again
        waitTimer = minWait;
    }

    // checks every half tile along the line is still on the path
    private bool PathIsClear(Vector2 from, Vector2 to)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(from, to) * 2);

        for (int i = 1; i <= steps; i++)
        {
            Vector2 point = Vector2.Lerp(from, to, (float)i / steps);

            if (!pathTilemap.HasTile(pathTilemap.WorldToCell(point)))
            {
                return false;
            }
        }

        return true;
    }
}
