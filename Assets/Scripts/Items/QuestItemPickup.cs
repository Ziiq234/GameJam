using UnityEngine;

// A side quest item lying somewhere (like Rose's necklace).
// Only shows up once the side quest has been started.
public class QuestItemPickup : MonoBehaviour
{
    [SerializeField] private string sideQuest = "Necklace";
    [SerializeField] private string foundMessage = "You found Rose's necklace!";

    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector3 startPosition;

    private void Start()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteMaker.MakeOrb(new Color(1f, 0.85f, 0.3f));
        spriteRenderer.sortingOrder = 6;
        startPosition = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        QuestManager quest = QuestManager.Instance;
        bool lookingForIt = quest != null && quest.GetSideQuest(sideQuest) == 1;

        spriteRenderer.enabled = lookingForIt;
        if (!lookingForIt)
            return;

        // twinkle so it's easier to spot
        transform.position = startPosition + Vector3.up * Mathf.Sin(Time.time * 4f) * 0.08f;
        transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 6f) * 0.15f);

        if (player != null && Vector2.Distance(transform.position, player.position) < 0.9f)
        {
            quest.SetSideQuest(sideQuest, 2);
            SoundManager.Play("pickup");
            DialogueBox.Show("", new[] { foundMessage });
        }
    }
}
