using UnityEngine;

// A magic bolt fired by the Mage. Flies in a straight line and hurts
// the player if it gets close enough.
public class MagicBolt : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private Transform player;
    private float lifeTime = 3f;

    public void Launch(Vector2 moveDirection, float moveSpeed, int boltDamage, Transform target)
    {
        direction = moveDirection;
        speed = moveSpeed;
        damage = boltDamage;
        player = target;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (player != null && Vector2.Distance(transform.position, player.position) < 0.45f)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
