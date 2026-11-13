using UnityEngine;

// The Mage's sword lying on the ground. Walk over it to pick it up.
public class SwordPickup : MonoBehaviour
{
    private Transform player;
    private Vector3 startPosition;

    private void Start()
    {
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteMaker.MakeSword(new Color(0.75f, 0.45f, 1f));
        sr.sortingOrder = 6;
        transform.localScale = Vector3.one * 1.5f;

        startPosition = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        // bob up and down so it stands out
        transform.position = startPosition + Vector3.up * Mathf.Sin(Time.time * 3f) * 0.1f;

        if (player != null && Vector2.Distance(transform.position, player.position) < 0.8f)
        {
            QuestManager.Instance.GiveMageSword();
            DialogueBox.Show("", new[] { "You picked up the Mage's Sword! It hits even harder than the Blacksmith's." });
            Destroy(gameObject);
        }
    }
}
