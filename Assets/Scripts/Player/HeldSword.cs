using UnityEngine;

// Shows the sword in the player's hand once they've got one,
// and swings it when they attack.
public class HeldSword : MonoBehaviour
{
    [SerializeField] private float swingTime = 0.12f;

    private CharacterSpriteAnimator characterAnimator;
    private SpriteRenderer swordRenderer;
    private Sprite ironSword;
    private Sprite mageSword;
    private float swingTimer;

    private void Awake()
    {
        characterAnimator = GetComponent<CharacterSpriteAnimator>();

        ironSword = SpriteMaker.MakeSword(new Color(0.85f, 0.85f, 0.9f));
        mageSword = SpriteMaker.MakeSword(new Color(0.75f, 0.45f, 1f));

        GameObject sword = new GameObject("HeldSword");
        sword.transform.SetParent(transform, false);
        swordRenderer = sword.AddComponent<SpriteRenderer>();
        swordRenderer.enabled = false;
    }

    public void Swing()
    {
        swingTimer = swingTime;
    }

    private void LateUpdate()
    {
        QuestManager quest = QuestManager.Instance;
        bool hasSword = quest != null && quest.HasSword;

        swordRenderer.enabled = hasSword;
        if (!hasSword)
            return;

        swordRenderer.sprite = quest.HasMageSword ? mageSword : ironSword;

        Vector2 facing = characterAnimator != null ? characterAnimator.Facing : Vector2.down;
        Transform sword = swordRenderer.transform;

        if (swingTimer > 0)
        {
            // point the sword straight out the way we're facing
            swingTimer -= Time.deltaTime;
            float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
            sword.localPosition = facing * 0.3f;
            sword.localRotation = Quaternion.Euler(0, 0, angle);
            swordRenderer.sortingOrder = 6;
            return;
        }

        // resting positions for each direction
        if (facing == Vector2.right)
        {
            sword.localPosition = new Vector3(0.3f, -0.25f, 0);
            sword.localRotation = Quaternion.Euler(0, 0, -30);
            swordRenderer.sortingOrder = 6;
        }
        else if (facing == Vector2.left)
        {
            sword.localPosition = new Vector3(-0.3f, -0.25f, 0);
            sword.localRotation = Quaternion.Euler(0, 0, 30);
            swordRenderer.sortingOrder = 6;
        }
        else if (facing == Vector2.up)
        {
            sword.localPosition = new Vector3(-0.28f, -0.15f, 0);
            sword.localRotation = Quaternion.Euler(0, 0, 20);
            swordRenderer.sortingOrder = 4;
        }
        else
        {
            sword.localPosition = new Vector3(0.28f, -0.3f, 0);
            sword.localRotation = Quaternion.Euler(0, 0, -20);
            swordRenderer.sortingOrder = 6;
        }
    }
}
