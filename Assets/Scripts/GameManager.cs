using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static readonly string[] LevelNames =
    {
        "EL JARDIN DE LAS PRIMERAS VOCES",
        "LA CIUDAD DE LAS CARTAS NO ENVIADAS",
        "EL TALLER DE LAS HORAS PRESTADAS",
        "LA BIBLIOTECA BAJO LA LLUVIA",
        "EL OBSERVATORIO DE LOS ECOS QUE REGRESAN"
    };
    private static readonly string[] MemoryStages =
    {
        "INFANCIA",
        "ADOLESCENCIA",
        "ADULTEZ",
        "VEJEZ",
        "ACEPTACION"
    };
    private static readonly Color[] ChapterColors =
    {
        new Color(0.18f, 0.78f, 0.74f),
        new Color(0.88f, 0.32f, 0.52f),
        new Color(0.95f, 0.62f, 0.18f),
        new Color(0.34f, 0.62f, 0.92f),
        new Color(0.62f, 0.43f, 0.92f)
    };

    public static GameManager Instance { get; private set; }

    public bool hasKey;
    public bool finishedGame;
    public bool gameStarted;
    public bool isPaused;
    public bool isDead;

    public bool CanPlayerMove => gameStarted && !finishedGame && !isPaused && !isDead;
    public int LevelNumber { get; private set; }
    public int TotalLevels { get; private set; }

    private PlayerRespawn pendingRespawn;
    private float deathTimer;
    private float echoMessageTimer;
    private float hintTimer;
    private string hintMessage = string.Empty;
    private int sceneIndex;

    private string KeySaveName => "UmbraHasKey_" + sceneIndex;
    private int ChapterIndex => Mathf.Clamp(LevelNumber - 1, 0, LevelNames.Length - 1);
    private string LevelTitle => LevelNames[ChapterIndex];
    private string MemoryStage => MemoryStages[ChapterIndex];
    private Color ChapterColor => ChapterColors[ChapterIndex];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ConfigurePerformance();
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        LevelNumber = sceneIndex + 1;
        TotalLevels = Mathf.Max(1, SceneManager.sceneCountInBuildSettings);
        hasKey = PlayerPrefs.GetInt(KeySaveName, 0) == 1;
        Time.timeScale = 0f;
    }

    private static void ConfigurePerformance()
    {
        int mediumQuality = Mathf.Min(2, QualitySettings.names.Length - 1);
        if (mediumQuality >= 0)
        {
            QualitySettings.SetQualityLevel(mediumQuality, true);
        }

        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1f / 60f;
        Time.maximumDeltaTime = 0.1f;
    }

    private void Update()
    {
        if (echoMessageTimer > 0f)
        {
            echoMessageTimer -= Time.unscaledDeltaTime;
        }

        if (hintTimer > 0f)
        {
            hintTimer -= Time.unscaledDeltaTime;
        }

        if (!gameStarted && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            gameStarted = true;
            Time.timeScale = 1f;
        }

        if (gameStarted && !finishedGame && !isDead && Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }

        if (isDead)
        {
            deathTimer -= Time.unscaledDeltaTime;
            if (deathTimer <= 0f)
            {
                pendingRespawn?.Respawn();
                pendingRespawn = null;
                isDead = false;
                Time.timeScale = 1f;
            }
        }

        if (isPaused && Input.GetKeyDown(KeyCode.R))
        {
            RestartFromCheckpoint();
        }

        if (finishedGame && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (LevelNumber < TotalLevels)
            {
                LoadNextLevel();
            }
            else
            {
                StartNewGame();
            }
        }
    }

    public void CollectKey()
    {
        hasKey = true;
        echoMessageTimer = 2.2f;
        PlayerPrefs.SetInt(KeySaveName, 1);
        PlayerPrefs.Save();
        UmbraAudio.Instance?.PlayPickup();
    }

    public void ShowHint(string message, float duration)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        hintMessage = message;
        hintTimer = Mathf.Max(hintTimer, duration);
    }

    public void PlayerDied(PlayerRespawn player)
    {
        if (isDead)
        {
            return;
        }

        pendingRespawn = player;
        isDead = true;
        deathTimer = 0.65f;
        Time.timeScale = 0f;
        UmbraAudio.Instance?.PlayDeath();
    }

    public void CompleteLevel()
    {
        if (finishedGame)
        {
            return;
        }

        finishedGame = true;
        int unlockedLevel = Mathf.Min(TotalLevels, LevelNumber + 1);
        PlayerPrefs.SetInt("UmbraUnlockedLevel", Mathf.Max(PlayerPrefs.GetInt("UmbraUnlockedLevel", 1), unlockedLevel));
        PlayerPrefs.Save();
        Time.timeScale = 0f;
    }

    private void LoadNextLevel()
    {
        ClearSceneProgress(sceneIndex);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex + 1);
    }

    private void StartNewGame()
    {
        for (int i = 0; i < TotalLevels; i++)
        {
            ClearSceneProgress(i);
        }

        PlayerPrefs.SetInt("UmbraUnlockedLevel", 1);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private static void ClearSceneProgress(int index)
    {
        PlayerRespawn.ClearSavedCheckpoint(index);
        PlayerPrefs.DeleteKey("UmbraHasKey_" + index);
    }

    public void RestartFromCheckpoint()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    private void OnGUI()
    {
        GUIStyle small = new GUIStyle(GUI.skin.label);
        small.fontSize = 16;
        small.normal.textColor = new Color(1f, 0.97f, 0.84f);

        GUIStyle centered = new GUIStyle(small);
        centered.alignment = TextAnchor.MiddleCenter;
        centered.fontSize = 24;
        centered.fontStyle = FontStyle.Bold;

        if (gameStarted && !finishedGame)
        {
            GUI.Label(new Rect(20, 18, 230, 28), hasKey ? "FRAGMENTO DE ECO [X]" : "FRAGMENTO DE ECO [ ]", small);
            GUI.Label(new Rect(Screen.width - 210, 18, 190, 28), "RECUERDO " + LevelNumber + "/" + TotalLevels, small);
        }

        if (echoMessageTimer > 0f)
        {
            float pulse = Mathf.Clamp01(echoMessageTimer / 2.2f);
            Color previous = GUI.color;
            GUI.color = new Color(ChapterColor.r, ChapterColor.g, ChapterColor.b, 0.12f * pulse);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
            centered.fontSize = 20;
            GUI.Label(new Rect((Screen.width - 360f) * 0.5f, 70f, 360f, 42f), "ECO RECUPERADO", centered);
        }

        if (hintTimer > 0f && gameStarted && !finishedGame && !isDead)
        {
            Rect hintRect = new Rect((Screen.width - 470f) * 0.5f, Screen.height - 76f, 470f, 42f);
            Color previous = GUI.color;
            GUI.color = new Color(0.03f, 0.07f, 0.08f, 0.88f);
            GUI.DrawTexture(hintRect, Texture2D.whiteTexture);
            GUI.color = previous;
            centered.fontSize = 15;
            centered.fontStyle = FontStyle.Bold;
            GUI.Label(hintRect, hintMessage, centered);
        }

        if (!gameStarted)
        {
            DrawPanel(620f, 245f);
            GUI.Label(CenteredRect(-92f, 560f, 48f), "UMBRA", centered);
            centered.fontSize = 18;
            GUI.Label(CenteredRect(-49f, 560f, 34f), "EL ARCHIVO DE LOS ECOS", centered);
            centered.fontSize = 15;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(-3f, 560f, 30f), "RECUERDO " + LevelNumber + " - " + MemoryStage, centered);
            GUI.Label(CenteredRect(27f, 580f, 34f), LevelTitle, centered);
            GUI.Label(CenteredRect(73f, 560f, 30f), "ENTER", centered);
        }

        if (isPaused)
        {
            DrawPanel(500f, 165f);
            GUI.Label(CenteredRect(-48f, 420f, 45f), "PAUSA", centered);
            centered.fontSize = 16;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(20f, 420f, 35f), "ESC CONTINUAR     R REINICIAR", centered);
        }

        if (isDead)
        {
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), "");
            GUI.Label(CenteredRect(-10f, 420f, 45f), "HAS CAIDO", centered);
        }

        if (finishedGame && LevelNumber < TotalLevels)
        {
            DrawPanel(560f, 195f);
            GUI.Label(CenteredRect(-65f, 520f, 45f), "RECUERDO RECUPERADO", centered);
            centered.fontSize = 16;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(-12f, 520f, 34f), LevelTitle, centered);
            GUI.Label(CenteredRect(42f, 520f, 35f), "ENTER - SIGUIENTE RECUERDO", centered);
        }

        if (finishedGame && LevelNumber == TotalLevels)
        {
            DrawPanel(560f, 350f);
            GUI.Label(CenteredRect(-145f, 520f, 45f), "ARCHIVO RECONSTRUIDO", centered);
            centered.fontSize = 15;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(-82f, 520f, 30f), "CREDITOS", centered);
            GUI.Label(CenteredRect(-42f, 520f, 28f), "Victor Cardenas - Project Owner", centered);
            GUI.Label(CenteredRect(-12f, 520f, 28f), "Andres Obispo - Scrum Master", centered);
            GUI.Label(CenteredRect(18f, 520f, 28f), "Gian Piero Gonzales - Programacion", centered);
            GUI.Label(CenteredRect(48f, 520f, 28f), "Jim Davila - Niveles", centered);
            GUI.Label(CenteredRect(78f, 520f, 28f), "Segundo Silva - Arte", centered);
            GUI.Label(CenteredRect(108f, 520f, 28f), "Luis Sotelo - QA y Sonido", centered);
            GUI.Label(CenteredRect(150f, 520f, 28f), "ENTER - VOLVER A EMPEZAR", centered);
        }
    }

    private void DrawPanel(float width, float height)
    {
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        Color previous = GUI.color;
        GUI.color = new Color(0.035f, 0.075f, 0.09f, 0.92f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = ChapterColor;
        GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 5f), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static Rect CenteredRect(float verticalOffset, float width, float height)
    {
        return new Rect((Screen.width - width) * 0.5f, (Screen.height * 0.5f) + verticalOffset, width, height);
    }
}
