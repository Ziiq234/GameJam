using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool isGameOver;
    private bool isPaused;
    private bool isVictory;

    private PlayerHealth playerHealth;

    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;
    public bool IsVictory => isVictory;

    private GUIStyle hudStyle;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle panelStyle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame &&
            !isGameOver &&
            !isVictory)
        {
            TogglePause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
    }

    public void Victory()
    {
        isVictory = true;
        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void RestartCurrentScene()
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        isGameOver = false;
        isPaused = false;
        isVictory = false;
        Time.timeScale = 1f;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetQuest();
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void StartGame()
    {
        isGameOver = false;
        isPaused = false;
        isVictory = false;
        Time.timeScale = 1f;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetQuest();
        }

        SceneManager.LoadScene("Village");
    }

    private void SetupStyles()
    {
        if (hudStyle != null)
            return;

        hudStyle = new GUIStyle(GUI.skin.label);
        hudStyle.fontSize = 24;
        hudStyle.normal.textColor = Color.white;
        hudStyle.fontStyle = FontStyle.Bold;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 42;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;
        titleStyle.fontStyle = FontStyle.Bold;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.fontStyle = FontStyle.Bold;

        panelStyle = new GUIStyle(GUI.skin.box);
    }

    private void OnGUI()
    {
        SetupStyles();

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            DrawMainMenu();
            return;
        }

        DrawHUD();

        if (isPaused)
        {
            DrawPauseMenu();
        }

        if (isGameOver)
        {
            DrawGameOver();
        }

        if (isVictory)
        {
            DrawVictory();
        }
    }

    private void DrawHUD()
    {
        if (playerHealth == null)
        {
            FindPlayer();
        }

        if (playerHealth != null)
        {
            GUI.Label(
                new Rect(20, 20, 300, 50),
                "Health: " + playerHealth.CurrentHealth + "/" + playerHealth.MaxHealth,
                hudStyle
            );
        }

        if (QuestManager.Instance != null)
        {
            GUI.Label(
                new Rect(20, 65, 700, 50),
                QuestManager.Instance.GetObjectiveText(),
                hudStyle
            );
        }
    }

    private void DrawMainMenu()
    {
        float centreX = Screen.width / 2f;

        GUI.Label(
            new Rect(centreX - 300, 90, 600, 70),
            "THE DARKNESS BELOW",
            titleStyle
        );

        GUI.Label(
            new Rect(centreX - 250, 160, 500, 40),
            "Recover the enchanted gunpowder",
            hudStyle
        );

        if (GUI.Button(
            new Rect(centreX - 120, 240, 240, 60),
            "Play",
            buttonStyle))
        {
            StartGame();
        }

        if (GUI.Button(
            new Rect(centreX - 120, 320, 240, 60),
            "Quit",
            buttonStyle))
        {
            Application.Quit();
        }
    }

    private void DrawPauseMenu()
    {
        DrawOverlayPanel("PAUSED");

        float centreX = Screen.width / 2f;

        if (GUI.Button(
            new Rect(centreX - 120, 300, 240, 55),
            "Resume",
            buttonStyle))
        {
            TogglePause();
        }

        if (GUI.Button(
            new Rect(centreX - 120, 370, 240, 55),
            "Main Menu",
            buttonStyle))
        {
            LoadMainMenu();
        }
    }

    private void DrawGameOver()
    {
        DrawOverlayPanel("GAME OVER");

        float centreX = Screen.width / 2f;

        if (GUI.Button(
            new Rect(centreX - 120, 300, 240, 55),
            "Restart",
            buttonStyle))
        {
            RestartCurrentScene();
        }

        if (GUI.Button(
            new Rect(centreX - 120, 370, 240, 55),
            "Main Menu",
            buttonStyle))
        {
            LoadMainMenu();
        }
    }

    private void DrawVictory()
    {
        GUI.Box(
            new Rect(0, 0, Screen.width, Screen.height),
            "",
            panelStyle
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 350,
                120,
                700,
                80
            ),
            "FESTIVAL RESTORED!",
            titleStyle
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 300,
                205,
                600,
                60
            ),
            "The enchanted fireworks light the night sky.",
            hudStyle
        );

        if (GUI.Button(
            new Rect(
                Screen.width / 2f - 120,
                320,
                240,
                60
            ),
            "Main Menu",
            buttonStyle))
        {
            LoadMainMenu();
        }
    }

    private void DrawOverlayPanel(string title)
    {
        GUI.Box(
            new Rect(
                Screen.width / 2f - 250,
                120,
                500,
                380
            ),
            "",
            panelStyle
        );

        GUI.Label(
            new Rect(
                Screen.width / 2f - 200,
                170,
                400,
                70
            ),
            title,
            titleStyle
        );
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}