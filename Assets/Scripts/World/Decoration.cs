using UnityEngine;

// Simple decorations drawn in code: glowing lanterns and signposts.
// Signposts show their text when the player walks up to them.
public class Decoration : MonoBehaviour
{
    public enum Kind { Lantern, Signpost }

    [SerializeField] private Kind kind = Kind.Lantern;
    [SerializeField] private string signText = "";
    [SerializeField] private float readRange = 2.5f;

    private SpriteRenderer spriteRenderer;
    private Transform player;
    private float flickerOffset;

    private void Start()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 4;

        if (kind == Kind.Lantern)
        {
            // random warm colour so they don't all look the same
            Color[] glows = { new Color(1f, 0.75f, 0.3f), new Color(1f, 0.45f, 0.35f), new Color(0.95f, 0.9f, 0.45f) };
            spriteRenderer.sprite = SpriteMaker.MakeLantern(glows[Random.Range(0, glows.Length)]);
            flickerOffset = Random.Range(0f, 10f);
        }
        else
        {
            spriteRenderer.sprite = SpriteMaker.MakeSignpost();
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (kind == Kind.Lantern)
        {
            // gentle flicker
            float brightness = 0.85f + Mathf.Sin(Time.time * 7f + flickerOffset) * 0.1f + Mathf.Sin(Time.time * 13f + flickerOffset) * 0.05f;
            spriteRenderer.color = new Color(brightness, brightness, brightness, 1f);
        }
    }

    private void OnGUI()
    {
        if (kind != Kind.Signpost || player == null || signText == "")
            return;

        if (Vector2.Distance(transform.position, player.position) <= readRange)
        {
            UIHelper.DrawNameTag(transform.position + Vector3.up * 0.2f, signText);
        }
    }
}
