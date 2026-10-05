using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            // Unity can reset static fields when scripts are recompiled during Play Mode
            // without invoking Awake again on the restored scene objects.
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();
            }

            return instance;
        }
        private set => instance = value;
    }

    public bool hasKey;
    public bool finishedGame;
    public bool gameStarted;
    public bool isPaused;
    public bool isDead;
    public bool hospitalEnding;

    public bool CanPlayerMove => gameStarted && !finishedGame && !isPaused && !isDead;
    public int LevelNumber { get; private set; }
    public int TotalLevels { get; private set; }
    public int AttemptNumber { get; private set; }
    public int DeathCount { get; private set; }

    private PlayerRespawn pendingRespawn;
    private float deathTimer;
    private float endingTimer;
    private float masterVolume;
    private int sceneIndex;

    private string KeySaveName => "UmbralHasSeal_" + sceneIndex;
    private const string AttemptSaveName = "UmbralAttemptNumber";
    private const string DeathCountSaveName = "UmbralDeathCount";
    private static readonly string[] ChapterNames =
    {
        "EL FONDO", "LOS ABANDONADOS", "LA CARNE", "EL PESO", "EL UMBRAL"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (GetComponent<UmbraEventJournal>() == null)
        {
            gameObject.AddComponent<UmbraEventJournal>();
        }
        ConfigurePerformance();
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        LevelNumber = sceneIndex + 1;
        TotalLevels = Mathf.Max(1, SceneManager.sceneCountInBuildSettings);
        hasKey = PlayerPrefs.GetInt(KeySaveName, 0) == 1;
        AttemptNumber = Mathf.Max(1, PlayerPrefs.GetInt(AttemptSaveName, 1));
        DeathCount = Mathf.Max(0, PlayerPrefs.GetInt(DeathCountSaveName, 0));
        masterVolume = PlayerPrefs.GetFloat("UmbralMasterVolume", 0.8f);
        AudioListener.volume = masterVolume;
        bool resumeAfterDeath = PlayerPrefs.GetInt("UmbralResumeScene", -1) == sceneIndex;
        bool directEditorChapterPreview = Application.isEditor && sceneIndex > 0;
        if (resumeAfterDeath || directEditorChapterPreview)
        {
            if (resumeAfterDeath)
            {
                PlayerPrefs.DeleteKey("UmbralResumeScene");
            }
            gameStarted = true;
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }

        PublishProgress();
        PublishPlayerState(
            resumeAfterDeath ? "REAPARICION" :
            directEditorChapterPreview ? "JUGANDO" : "PREPARADO");
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
            PublishPlayerState("JUGANDO");
        }

        if (!gameStarted && Input.GetKeyDown(KeyCode.C))
        {
            int unlocked = Mathf.Clamp(PlayerPrefs.GetInt("UmbralUnlockedChapter", 1), 1, TotalLevels);
            Time.timeScale = 1f;
            SceneManager.LoadScene(unlocked - 1);
        }

        if (!gameStarted && Input.GetKeyDown(KeyCode.N))
        {
            StartNewGame();
        }

        if (gameStarted && !finishedGame && !isDead && Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            if (!isPaused) PlayerPrefs.Save();
            PublishPlayerState(isPaused ? "PAUSA" : "JUGANDO");
        }

        if (isDead)
        {
            deathTimer -= Time.unscaledDeltaTime;
            if (deathTimer <= 0f)
            {
                PlayerPrefs.SetInt("UmbralResumeScene", sceneIndex);
                PlayerPrefs.Save();
                Time.timeScale = 1f;
                SceneManager.LoadScene(sceneIndex);
            }
        }


        if (hospitalEnding)
        {
            endingTimer += Time.unscaledDeltaTime;
            if (endingTimer > 10f && Input.GetKeyDown(KeyCode.N))
            {
                StartNewGame();
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
        UmbraGameEvents.PublishInteraction("Llave del capitulo recogida");
        PublishProgress();
    }

    public void PrepareVideoCaptureState()
    {
        gameStarted = true;
        isPaused = false;
        isDead = false;
        finishedGame = false;
        hospitalEnding = false;
        hasKey = false;
        AttemptNumber = 1;
        DeathCount = 0;
        Time.timeScale = 1f;
        PublishProgress();
        PublishPlayerState("DEMOSTRACION");
    }

    public void PlayerDied(PlayerRespawn player)
    {
        if (isDead)
        {
            return;
        }

        pendingRespawn = player;
        isDead = true;
        DeathCount++;
        AttemptNumber++;
        PlayerPrefs.SetInt(DeathCountSaveName, DeathCount);
        PlayerPrefs.SetInt(AttemptSaveName, AttemptNumber);
        PlayerPrefs.Save();
        deathTimer = 0.65f;
        Time.timeScale = 0f;
        UmbraAudio.Instance?.PlayDeath();
        PublishPlayerState("HAS CAIDO");
    }

    public void CompleteLevel()
    {
        if (finishedGame)
        {
            return;
        }

        finishedGame = true;
        int unlockedLevel = Mathf.Min(TotalLevels, LevelNumber + 1);
        PlayerPrefs.SetInt("UmbralUnlockedChapter", Mathf.Max(PlayerPrefs.GetInt("UmbralUnlockedChapter", 1), unlockedLevel));
        PlayerPrefs.Save();
        Time.timeScale = 0f;
        if (LevelNumber == TotalLevels)
        {
            hospitalEnding = true;
            endingTimer = 0f;
        }
        UmbraGameEvents.PublishInteraction("Salida del capitulo alcanzada");
        PublishProgress();
        PublishPlayerState("NIVEL SUPERADO");
    }

    private void LoadNextLevel()
    {
        ClearSceneProgress(sceneIndex);
        PlayerPrefs.SetInt("UmbralResumeScene", sceneIndex + 1);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex + 1);
    }

    private void StartNewGame()
    {
        for (int i = 0; i < TotalLevels; i++)
        {
            ClearSceneProgress(i);
        }

        PlayerPrefs.SetInt("UmbralUnlockedChapter", 1);
        PlayerPrefs.SetInt(AttemptSaveName, 1);
        PlayerPrefs.SetInt(DeathCountSaveName, 0);
        PlayerPrefs.DeleteKey("UmbralAwakeningSeen");
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private static void ClearSceneProgress(int index)
    {
        PlayerRespawn.ClearSavedCheckpoint(index);
        PlayerPrefs.DeleteKey("UmbralHasSeal_" + index);
        HorrorEvent2D.ClearAllForScene(index);
    }

    public void RestartFromCheckpoint()
    {
        UmbraGameEvents.PublishInteraction("Reinicio desde checkpoint");
        PlayerPrefs.SetInt("UmbralResumeScene", sceneIndex);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }

    private void PublishProgress()
    {
        UmbraGameEvents.PublishProgress(new UmbraProgressSnapshot(
            LevelNumber,
            TotalLevels,
            hasKey,
            finishedGame));
    }

    private void PublishPlayerState(string state)
    {
        UmbraGameEvents.PublishPlayerState(new UmbraPlayerStateSnapshot(
            state,
            AttemptNumber,
            DeathCount));
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
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
            GUI.Label(new Rect(20, 18, 220, 28), hasKey ? "LLAVE [X]" : "LLAVE [ ]", small);
            GUI.Label(new Rect(20, 45, 220, 28), "INTENTO " + AttemptNumber + " · CAIDAS " + DeathCount, small);
            GUI.Label(new Rect(Screen.width - 310, 18, 290, 28), "PROGRESO " + LevelNumber + "/" + TotalLevels, small);
            GUI.Label(new Rect(Screen.width - 310, 45, 290, 28), ChapterNames[Mathf.Clamp(sceneIndex, 0, 4)], small);
        }

        if (!gameStarted)
        {
            DrawPanel(460f, 180f);
            GUI.Label(CenteredRect(-72f, 420f, 50f), "UMBRAL", centered);
            centered.fontSize = 16;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(-24f, 420f, 32f), "CAPITULO " + LevelNumber + " — " + ChapterNames[Mathf.Clamp(sceneIndex, 0, 4)], centered);
            GUI.Label(CenteredRect(18f, 420f, 32f), "ENTER: COMENZAR ASCENSO   C: CONTINUAR   N: NUEVA PARTIDA", centered);
            GUI.Label(CenteredRect(52f, 420f, 28f), "A/D MOVER · SHIFT CORRER · ESPACIO SALTAR · E INTERACTUAR", centered);
        }

        if (isPaused)
        {
            DrawPanel(520f, 225f);
            GUI.Label(CenteredRect(-48f, 420f, 45f), "PAUSA", centered);
            centered.fontSize = 16;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(5f, 470f, 35f), "ESC CONTINUAR     R REINICIAR", centered);
            GUI.Label(CenteredRect(42f, 470f, 28f), "VOLUMEN", centered);
            masterVolume = GUI.HorizontalSlider(CenteredRect(76f, 280f, 24f), masterVolume, 0f, 1f);
            AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat("UmbralMasterVolume", masterVolume);
        }

        if (isDead)
        {
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), "");
            GUI.Label(CenteredRect(-10f, 420f, 45f), "HAS CAIDO", centered);
            centered.fontSize = 16;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(30f, 420f, 32f), "EL INTENTO " + AttemptNumber + " COMIENZA EN EL ULTIMO CHECKPOINT", centered);
        }

        if (finishedGame && LevelNumber < TotalLevels)
        {
            DrawPanel(500f, 180f);
            GUI.Label(CenteredRect(-55f, 460f, 45f), ChapterNames[Mathf.Clamp(sceneIndex, 0, 4)] + " SUPERADO", centered);
            centered.fontSize = 16;
            centered.fontStyle = FontStyle.Normal;
            GUI.Label(CenteredRect(25f, 460f, 35f), "ENTER - SIGUIENTE NIVEL", centered);
        }

        if (hospitalEnding)
        {
            GUI.color = endingTimer < 2.2f ? Color.Lerp(Color.black, Color.white, endingTimer / 2.2f) : new Color(0.9f, 0.92f, 0.9f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            centered.normal.textColor = new Color(0.08f, 0.1f, 0.12f);
            if (endingTimer > 2.4f)
            {
                DrawHospitalRoom();
                GUI.Label(CenteredRect(-70f, 620f, 45f), "HABITACION 307", centered);
            }
            if (endingTimer > 4.2f)
            {
                centered.fontSize = 18;
                GUI.Label(CenteredRect(10f, 760f, 35f), "Doctor: Estuviste muerto durante tres minutos.", centered);
                GUI.Label(CenteredRect(48f, 760f, 35f), "Logramos reanimarte. Ahora descansa.", centered);
            }
            if (endingTimer > 8f)
            {
                centered.fontSize = 24;
                GUI.Label(CenteredRect(112f, 620f, 42f), "UMBRAL", centered);
                centered.fontSize = 15;
                GUI.Label(CenteredRect(154f, 620f, 32f), "FIN — N: NUEVA PARTIDA", centered);
            }
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

    private static void DrawHospitalRoom()
    {
        Color previous = GUI.color;
        GUI.color = new Color(0.7f, 0.74f, 0.75f);
        GUI.DrawTexture(new Rect(Screen.width * 0.1f, Screen.height * 0.62f, Screen.width * 0.64f, 72f), Texture2D.whiteTexture);
        GUI.color = new Color(0.36f, 0.4f, 0.42f);
        GUI.DrawTexture(new Rect(Screen.width * 0.12f, Screen.height * 0.59f, 12f, 120f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(Screen.width * 0.71f, Screen.height * 0.59f, 12f, 120f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(Screen.width * 0.78f, Screen.height * 0.42f, 120f, 78f), Texture2D.whiteTexture);
        GUI.color = new Color(0.25f, 0.62f, 0.46f);
        GUI.DrawTexture(new Rect(Screen.width * 0.795f, Screen.height * 0.45f, 88f, 4f), Texture2D.whiteTexture);
        GUI.color = new Color(0.18f, 0.2f, 0.22f);
        GUI.DrawTexture(new Rect(Screen.width * 0.86f, Screen.height * 0.48f, 38f, 145f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(Screen.width * 0.845f, Screen.height * 0.44f, 68f, 52f), Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
