using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool isGameOver;
    private bool isPaused;
    private bool isVictory;
    private bool isEnding;
    private FireworkController fireworks;

    private PlayerHealth playerHealth;

    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;
    public bool IsVictory => isVictory;

    // true from the fireworks starting until the player leaves the victory screen
    public bool IsEnding => isEnding;

    private GUIStyle hudStyle;
    private GUIStyle objectiveStyle;
    private GUIStyle sideStyle;

    // which main menu page is showing ("" = the main buttons)
    private string menuPage = "";
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
            !isVictory &&
            !isEnding)
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
        MoveToSpawnPoint();
    }

    // when coming out of a dungeon, put the player by the entrance instead of the start
    private void MoveToSpawnPoint()
    {
        string spawnName = SceneTransition.NextSpawnPoint;
        SceneTransition.NextSpawnPoint = null;

        if (string.IsNullOrEmpty(spawnName) || playerHealth == null)
            return;

        GameObject spawn = GameObject.Find(spawnName);

        if (spawn == null)
        {
            Debug.LogWarning("Spawn point not found: " + spawnName);
            return;
        }

        // just below the entrance so we don't walk straight back in
        Vector3 spot = spawn.transform.position + Vector3.down * 2f;
        Transform player = playerHealth.transform;

        player.position = spot;
        player.GetComponent<Rigidbody2D>().position = spot;
        Physics2D.SyncTransforms();

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(spot.x, spot.y, Camera.main.transform.position.z);
        }
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

    // called when the last quest is handed in
    public void StartEnding()
    {
        if (isEnding)
            return;

        isEnding = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StopTimer();
        }

        GameObject show = new GameObject("Fireworks");
        fireworks = show.AddComponent<FireworkController>();
        fireworks.Play(8f, Victory);
    }

    // closes the victory screen so you can keep walking around
    public void KeepExploring()
    {
        isVictory = false;
        isEnding = false;
        Time.timeScale = 1f;

        if (fireworks != null)
        {
            Destroy(fireworks.gameObject);
        }
    }

    public void Victory()
    {
        SoundManager.Play("fanfare");
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
        // come back to life with full health
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SavedHealth = -1;
        }

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
        ClearEnding();

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
        ClearEnding();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetQuest();
        }

        SceneManager.LoadScene("Village");
    }

    private void ClearEnding()
    {
        isEnding = false;

        if (fireworks != null)
        {
            Destroy(fireworks.gameObject);
        }
    }

    private void SetupStyles()
    {
        if (hudStyle != null)
            return;

        hudStyle = new GUIStyle(GUI.skin.label);
        hudStyle.fontSize = 16;
        hudStyle.normal.textColor = Color.white;
        hudStyle.fontStyle = FontStyle.Bold;

        objectiveStyle = new GUIStyle(hudStyle);
        objectiveStyle.fontSize = 14;
        objectiveStyle.wordWrap = true;
        objectiveStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

        sideStyle = new GUIStyle(objectiveStyle);
        sideStyle.fontSize = 12;
        sideStyle.normal.textColor = new Color(0.7f, 0.9f, 1f);

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

        if (!isEnding)
        {
            DrawHUD();
        }

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

        // dark backing so the text shows up on any background
        // keep clear of the minimap in the top right
        float hudWidth = Mathf.Min(460, Screen.width - 220);
        string sideText = QuestManager.Instance != null ? QuestManager.Instance.GetSideQuestText() : "";
        float hudHeight = sideText == "" ? 84 : 124;
        UIHelper.DrawPanel(new Rect(10, 10, hudWidth, hudHeight));

        if (sideText != "")
        {
            GUI.Label(new Rect(20, 88, hudWidth - 20, 40), "Side: " + sideText, sideStyle);
        }

        if (playerHealth != null)
        {
            GUI.Label(
                new Rect(20, 12, 300, 26),
                "Health: " + playerHealth.CurrentHealth + "/" + playerHealth.MaxHealth,
                hudStyle
            );
        }

        if (QuestManager.Instance != null)
        {
            QuestManager quest = QuestManager.Instance;

            GUI.Label(
                new Rect(20, 36, hudWidth - 20, 36),
                quest.GetObjectiveText(),
                objectiveStyle
            );

            string items = "Gold: " + quest.Gold;

            if (quest.HasMageSword) items += "   Mage's Sword";
            else if (quest.HasSword) items += "   Sword";
            if (quest.HasShield) items += "   Shield";
            if (quest.HasRing) items += "   Enchanted Ring";

            GUI.Label(
                new Rect(20, 66, 400, 26),
                items,
                hudStyle
            );
        }
    }

    private void DrawMainMenu()
    {
        float centreX = Screen.width / 2f;

        if (menuPage == "Controls")
        {
            DrawInfoPage("CONTROLS", new[]
            {
                "W A S D  -  Move",
                "Space  -  Attack",
                "E  -  Talk / use boats / next line",
                "Hold M  -  Show the whole map",
                "1 / 2  -  Buy items in the shop",
                "Esc  -  Pause"
            });
            return;
        }

        if (menuPage == "Credits")
        {
            DrawInfoPage("CREDITS", new[]
            {
                "Made by Jamie Jones and Joe Buckley for Game Jam 2026",
                "Story by Jamie Jones and Joe Buckley",
                "Village map by Joe Buckley",
                "Characters: Old Town Citizens by Electric Lemon",
                "Beach art: see PLANNING/06_Asset_Credits.txt",
                "Village: Fantasy Lands Houses Demo by Szadi art",
                "Dungeon: S.Salmon's Underground by S.Salmon",
                "Enemies: Dark Fantasy Enemies by MonoPixelArt",
                "UI: Dark Ages UI by Hypnobius",
                "Made with help from AI tools (see the AI use log)"
            });
            return;
        }

        GUI.Label(
            new Rect(centreX - 300, 90, 600, 70),
            "THE DARKNESS BELOW",
            titleStyle
        );

        GUI.Label(
            new Rect(centreX - 250, 160, 500, 40),
            "Recover the enchanted rocket charges",
            hudStyle
        );

        if (GUI.Button(new Rect(centreX - 120, 220, 240, 50), "Play", buttonStyle))
        {
            StartGame();
        }

        if (GUI.Button(new Rect(centreX - 120, 280, 240, 50), "Controls", buttonStyle))
        {
            menuPage = "Controls";
        }

        if (GUI.Button(new Rect(centreX - 120, 340, 240, 50), "Credits", buttonStyle))
        {
            menuPage = "Credits";
        }

        if (GUI.Button(new Rect(centreX - 120, 400, 240, 50), "Quit", buttonStyle))
        {
            Application.Quit();
        }
    }

    // a page of text with a back button, used for Controls and Credits
    private void DrawInfoPage(string title, string[] lines)
    {
        float centreX = Screen.width / 2f;
        UIHelper.DrawPanel(new Rect(centreX - 300, 60, 600, 175 + lines.Length * 32));

        GUI.Label(new Rect(centreX - 300, 70, 600, 60), title, titleStyle);

        for (int i = 0; i < lines.Length; i++)
        {
            GUI.Label(new Rect(centreX - 280, 140 + i * 32, 560, 30), lines[i], hudStyle);
        }

        if (GUI.Button(new Rect(centreX - 100, 150 + lines.Length * 32 + 20, 200, 45), "Back", buttonStyle))
        {
            menuPage = "";
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
        float centreX = Screen.width / 2f;
        Rect panel = new Rect(centreX - 260, 80, 520, 380);
        UIHelper.DrawPanel(panel);

        GUI.Label(new Rect(centreX - 250, 100, 500, 60), "FESTIVAL RESTORED!", titleStyle);
        GUI.Label(new Rect(centreX - 230, 165, 460, 30), "The enchanted fireworks light up the night sky.", hudStyle);

        if (QuestManager.Instance != null)
        {
            QuestManager quest = QuestManager.Instance;
            int seconds = Mathf.RoundToInt(quest.PlayTime);
            string time = (seconds / 60) + "m " + (seconds % 60).ToString("00") + "s";

            GUI.Label(new Rect(centreX - 230, 205, 460, 26), "Time: " + time, hudStyle);
            GUI.Label(new Rect(centreX - 230, 230, 460, 26), "Enemies defeated: " + quest.EnemiesDefeated, hudStyle);
            GUI.Label(new Rect(centreX - 230, 255, 460, 26), "Gold: " + quest.Gold, hudStyle);
        }

        if (GUI.Button(new Rect(centreX - 240, 310, 150, 50), "Play Again", buttonStyle))
        {
            StartGame();
        }

        if (GUI.Button(new Rect(centreX - 75, 310, 150, 50), "Main Menu", buttonStyle))
        {
            LoadMainMenu();
        }

        if (GUI.Button(new Rect(centreX + 90, 310, 150, 50), "Keep Exploring", buttonStyle))
        {
            KeepExploring();
        }
    }

    private void DrawOverlayPanel(string title)
    {
        UIHelper.DrawPanel(new Rect(Screen.width / 2f - 250, 120, 500, 380));

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