using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
    private int sceneIndex;

    private string KeySaveName => "UmbraHasKey_" + sceneIndex;

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
        PlayerPrefs.SetInt(KeySaveName, 1);
        PlayerPrefs.Save();
        UmbraAudio.Instance?.PlayPickup();
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
        small.normal.textColor = new Color(0.9f, 0.9f, 0.88f);

        GUIStyle centered = new GUIStyle(small);
        centered.alignment = TextAnchor.MiddleCenter;
        centered.fontSize = 24;
        centered.fontStyle = FontStyle.Bold;

        if (gameStarted && !finishedGame)
        {
            GUI.Label(new Rect(20, 18, 180, 28), hasKey ? "LLAVE [X]" : "LLAVE [ ]", small);
            GUI.Label(new Rect(Screen.width - 140, 18, 120, 28), "NIVEL " + LevelNumber + "/" + TotalLevels, small);
        }

        if (!gameStarted)
        {
            DrawPanel(460f, 180f);
            GUI.Label(CenteredRect(-60f, 420f, 50f), "UMBRA", centered);
            centered.fontSize = 16;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(-5f, 420f, 32f), "NIVEL " + LevelNumber, centered);
            GUI.Label(CenteredRect(40f, 420f, 32f), "ENTER", centered);
        }

        if (isPaused)
        {
            DrawPanel(460f, 165f);
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
            DrawPanel(500f, 180f);
            GUI.Label(CenteredRect(-55f, 460f, 45f), "NIVEL " + LevelNumber + " COMPLETADO", centered);
            centered.fontSize = 16;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(25f, 460f, 35f), "ENTER - SIGUIENTE NIVEL", centered);
        }

        if (finishedGame && LevelNumber == TotalLevels)
        {
            DrawPanel(560f, 350f);
            GUI.Label(CenteredRect(-145f, 520f, 45f), "ESCAPASTE DE UMBRA", centered);
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

    private static void DrawPanel(float width, float height)
    {
        GUI.Box(new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height), "");
    }

    private static Rect CenteredRect(float verticalOffset, float width, float height)
    {
        return new Rect((Screen.width - width) * 0.5f, (Screen.height * 0.5f) + verticalOffset, width, height);
    }
}
