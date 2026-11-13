using System.Collections.Generic;
using UnityEngine;

// The firework show at the end of the game. The sky goes dark and rockets
// shoot up and burst into coloured sparks. Drawn straight onto the screen
// so it doesn't need any particle assets.
public class FireworkController : MonoBehaviour
{
    [SerializeField] private float rocketInterval = 0.35f;
    [SerializeField] private int sparksPerRocket = 40;

    private class Rocket
    {
        public Vector2 position;
        public Vector2 velocity;
        public Color colour;
        public float fuse;
    }

    private class Spark
    {
        public Vector2 position;
        public Vector2 velocity;
        public Color colour;
        public float life;
    }

    private List<Rocket> rockets = new List<Rocket>();
    private List<Spark> sparks = new List<Spark>();

    private Color[] colours =
    {
        new Color(1f, 0.3f, 0.3f), new Color(1f, 0.85f, 0.3f), new Color(0.4f, 0.8f, 1f),
        new Color(0.6f, 1f, 0.4f), new Color(0.85f, 0.45f, 1f), Color.white
    };

    private float showLength;
    private float timer;
    private float nextRocket;
    private bool finished;
    private System.Action onFinished;
    private GUIStyle titleStyle;

    public void Play(float seconds, System.Action whenFinished)
    {
        showLength = seconds;
        onFinished = whenFinished;
    }

    private void Update()
    {
        // unscaled so it keeps going when the victory screen pauses the game
        float delta = Time.unscaledDeltaTime;
        timer += delta;

        if (timer >= nextRocket)
        {
            nextRocket = timer + rocketInterval;
            LaunchRocket();
        }

        UpdateRockets(delta);
        UpdateSparks(delta);

        if (!finished && timer >= showLength)
        {
            finished = true;
            onFinished?.Invoke();
        }
    }

    private void LaunchRocket()
    {
        Rocket rocket = new Rocket();
        rocket.position = new Vector2(Random.Range(Screen.width * 0.15f, Screen.width * 0.85f), Screen.height);
        // screen y goes down, so a negative speed goes up
        rocket.velocity = new Vector2(Random.Range(-40f, 40f), Random.Range(-420f, -320f));
        rocket.colour = colours[Random.Range(0, colours.Length)];
        rocket.fuse = Random.Range(0.9f, 1.4f);
        rockets.Add(rocket);
    }

    private void UpdateRockets(float delta)
    {
        for (int i = rockets.Count - 1; i >= 0; i--)
        {
            Rocket rocket = rockets[i];
            rocket.position += rocket.velocity * delta;
            rocket.fuse -= delta;

            if (rocket.fuse <= 0)
            {
                Explode(rocket);
                rockets.RemoveAt(i);
            }
        }
    }

    private void Explode(Rocket rocket)
    {
        SoundManager.Play("pop");

        for (int i = 0; i < sparksPerRocket; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(60f, 200f);

            Spark spark = new Spark();
            spark.position = rocket.position;
            spark.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            spark.colour = rocket.colour;
            spark.life = Random.Range(0.8f, 1.5f);
            sparks.Add(spark);
        }
    }

    private void UpdateSparks(float delta)
    {
        for (int i = sparks.Count - 1; i >= 0; i--)
        {
            Spark spark = sparks[i];
            spark.velocity.y += 120f * delta; // gravity pulls them down
            spark.position += spark.velocity * delta;
            spark.life -= delta;

            if (spark.life <= 0)
            {
                sparks.RemoveAt(i);
            }
        }
    }

    private void OnGUI()
    {
        // drawn behind the victory screen
        GUI.depth = 10;

        // fade the sky to night over the first second and a half
        float darkness = Mathf.Clamp01(timer / 1.5f) * 0.7f;
        UIHelper.DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0.02f, 0.02f, 0.08f, darkness));

        foreach (Rocket rocket in rockets)
        {
            UIHelper.DrawRect(new Rect(rocket.position.x - 2, rocket.position.y - 4, 4, 8), rocket.colour);
        }

        foreach (Spark spark in sparks)
        {
            Color colour = spark.colour;
            colour.a = Mathf.Clamp01(spark.life);
            UIHelper.DrawRect(new Rect(spark.position.x - 2, spark.position.y - 2, 4, 4), colour);
        }

        if (!finished)
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontSize = 26;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.alignment = TextAnchor.MiddleCenter;
                titleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);
            }

            GUI.Label(new Rect(0, 40, Screen.width, 40), "Let the festival begin!", titleStyle);
        }
    }
}
