using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class UmbraFullPlaythrough : MonoBehaviour
{
    private const float LevelTimeout = 180f;

    private PlayerController2D player;
    private PlayerRespawn respawn;
    private GameManager manager;
    private CollectKey key;
    private float levelStartedAt;
    private float lastJumpAt;
    private float furthestX;
    private float lastProgressAt;
    private int deaths;
    private int reloads;
    private int activeLevel = -1;
    private bool wasDead;
    private bool changingLevel;
    private static bool started;
    private static bool progressCleared;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        if (started || !System.Environment.GetCommandLineArgs().Contains("-umbraFullPlaythrough"))
        {
            return;
        }

        started = true;
        GameObject testObject = new GameObject("UMBRA Full Playthrough");
        DontDestroyOnLoad(testObject);
        testObject.AddComponent<UmbraFullPlaythrough>();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private IEnumerator Start()
    {
        if (!progressCleared)
        {
            progressCleared = true;
            ClearProgress();
            yield return null;
            SceneManager.LoadScene(0);
            yield break;
        }

        BeginLevel();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        player?.ClearAutomationInput();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(BeginLevelNextFrame());
    }

    private IEnumerator BeginLevelNextFrame()
    {
        yield return null;
        BeginLevel();
    }

    private void BeginLevel()
    {
        player = FindAnyObjectByType<PlayerController2D>();
        respawn = player != null ? player.GetComponent<PlayerRespawn>() : null;
        manager = FindAnyObjectByType<GameManager>();
        key = FindAnyObjectByType<CollectKey>();
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex != activeLevel)
        {
            activeLevel = sceneIndex;
            levelStartedAt = Time.realtimeSinceStartup;
            deaths = 0;
            reloads = 0;
            Debug.Log("UMBRA PLAYTHROUGH START LEVEL " + (sceneIndex + 1));
        }
        else
        {
            Debug.Log("UMBRA PLAYTHROUGH RESUME LEVEL " + (sceneIndex + 1) +
                " RECOVERY=" + reloads);
        }

        lastJumpAt = Time.realtimeSinceStartup;
        furthestX = player != null ? player.transform.position.x : -100f;
        lastProgressAt = Time.realtimeSinceStartup;
        wasDead = false;
        changingLevel = false;

        if (manager != null)
        {
            manager.gameStarted = true;
            Time.timeScale = 1f;
        }

    }

    private void Update()
    {
        if (changingLevel || player == null || manager == null)
        {
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (manager.finishedGame)
        {
            changingLevel = true;
            player.ClearAutomationInput();
            StartCoroutine(FinishLevel());
            return;
        }

        if (manager.isDead)
        {
            if (!wasDead)
            {
                deaths++;
                wasDead = true;
                Debug.LogWarning("UMBRA PLAYTHROUGH DEATH LEVEL " + manager.LevelNumber +
                    " AT X=" + player.transform.position.x.ToString("F1") +
                    " Y=" + player.transform.position.y.ToString("F1") +
                    " RESPAWN_X=" + (respawn != null ? respawn.respawnPoint.x.ToString("F1") : "NA"));
            }

            player.SetAutomationInput(0f, 0f, false, false, false);
            return;
        }

        wasDead = false;
        if (now - levelStartedAt > LevelTimeout)
        {
            Fail("timeout at X=" + player.transform.position.x.ToString("F1"));
            return;
        }

        if (player.transform.position.x > furthestX + 0.3f)
        {
            furthestX = player.transform.position.x;
            lastProgressAt = now;
        }

        if (now - lastProgressAt > 10f)
        {
            reloads++;
            if (reloads > 3)
            {
                Fail("too many recoveries in level " + manager.LevelNumber +
                    " at X=" + player.transform.position.x.ToString("F1") +
                    " Y=" + player.transform.position.y.ToString("F1"));
                return;
            }

            Debug.LogWarning("UMBRA PLAYTHROUGH RECOVERY LEVEL " + manager.LevelNumber +
                " AT X=" + player.transform.position.x.ToString("F1") +
                " Y=" + player.transform.position.y.ToString("F1") +
                " GROUNDED=" + player.IsGrounded +
                " CLIMB_ZONE=" + player.HasClimbZone +
                " CLIMBING=" + player.IsClimbing +
                " VELOCITY=" + player.GetComponent<Rigidbody2D>().linearVelocity);
            manager.RestartFromCheckpoint();
            changingLevel = true;
            return;
        }

        DrivePlayer(now);
    }

    private void DrivePlayer(float now)
    {
        float horizontal = 1f;
        float vertical = 0f;
        bool jump = false;
        bool interact = false;

        LeverSwitch2D activeLever = FindObjectsByType<LeverSwitch2D>()
            .Where(lever => lever.targetTrap != null && lever.targetTrap.IsArmed &&
                lever.transform.position.x > player.transform.position.x - 1.5f)
            .OrderBy(lever => lever.transform.position.x)
            .FirstOrDefault();
        Transform routeTarget = activeLever != null
            ? activeLever.transform
            : key != null && key.gameObject.activeInHierarchy ? key.transform : null;
        ClimbZone2D routeLadder = routeTarget != null
            ? FindObjectsByType<ClimbZone2D>()
                .OrderBy(ladder => Mathf.Abs(routeTarget.position.x - ladder.transform.position.x))
                .FirstOrDefault()
            : null;

        PressureSwitch2D activeSwitch = FindObjectsByType<PressureSwitch2D>()
            .Where(pressure => pressure.targetTrap != null && pressure.targetTrap.IsArmed)
            .OrderBy(pressure => Mathf.Abs(pressure.transform.position.x - player.transform.position.x))
            .FirstOrDefault();
        PushPullObject2D activeBox = null;
        if (activeSwitch != null && activeSwitch.transform.position.x > player.transform.position.x - 1f)
        {
            activeBox = FindObjectsByType<PushPullObject2D>()
                .OrderBy(box => Mathf.Abs(box.transform.position.x - activeSwitch.transform.position.x))
                .FirstOrDefault();
        }

        bool solvingBox = activeBox != null &&
            activeBox.transform.position.x - player.transform.position.x < 9f &&
            activeBox.transform.position.x < activeSwitch.transform.position.x - 0.08f;
        if (solvingBox)
        {
            if (activeBox.transform.position.y < -5f)
            {
                manager.RestartFromCheckpoint();
                changingLevel = true;
                return;
            }

            float playerSide = player.transform.position.x - activeBox.transform.position.x;
            if (playerSide > activeBox.pullDistance * 0.82f)
            {
                horizontal = -1f;
            }
            else if (playerSide > 0.05f)
            {
                horizontal = 1f;
                interact = true;
            }
            else
            {
                horizontal = 1f;
            }

            if (playerSide <= 0.05f && player.IsGrounded &&
                now - lastJumpAt > 0.38f && !HasGroundAhead(1.5f))
            {
                jump = true;
                lastJumpAt = now;
            }
        }

        Collider2D ladderCollider = routeLadder != null
            ? routeLadder.GetComponent<Collider2D>()
            : null;
        float clearLadderY = ladderCollider != null
            ? ladderCollider.bounds.max.y + 0.72f
            : 1f;
        bool climbingToTarget = routeTarget != null && routeLadder != null &&
            Mathf.Abs(player.transform.position.x - routeLadder.transform.position.x) < 0.75f &&
            (player.HasClimbZone || player.transform.position.y < clearLadderY);
        if (climbingToTarget)
        {
            float difference = routeLadder.transform.position.x - player.transform.position.x;
            horizontal = Mathf.Abs(difference) > 0.12f ? Mathf.Sign(difference) : 0f;
            vertical = player.HasClimbZone ? 1f : 0f;
        }
        else if (activeLever != null && player.transform.position.y > -0.2f &&
            Mathf.Abs(player.transform.position.x - activeLever.transform.position.x) < 0.8f)
        {
            interact = true;
            horizontal = Mathf.Abs(activeLever.transform.position.x - player.transform.position.x) > 0.12f
                ? Mathf.Sign(activeLever.transform.position.x - player.transform.position.x)
                : 0f;
        }
        else if (!solvingBox && ShouldJump(now))
        {
            jump = true;
            lastJumpAt = now;
        }

        player.SetAutomationInput(horizontal, vertical, interact, false, jump);
    }

    private bool ShouldJump(float now)
    {
        if (!player.IsGrounded || now - lastJumpAt < 0.38f)
        {
            return false;
        }

        Vector2 origin = player.transform.position;
        bool groundAhead = HasGroundAhead(1.5f);
        bool obstacleAhead = Physics2D.Raycast(
            origin + new Vector2(0f, -0.25f),
            Vector2.right,
            1.15f,
            LayerMask.GetMask("Ground"));
        bool trapAhead = FindObjectsByType<DeathTrap>().Any(trap =>
            trap.IsArmed && trap.transform.position.x > origin.x &&
            trap.transform.position.x - origin.x < 2.2f &&
            Mathf.Abs(trap.transform.position.y - origin.y) < 2.2f);

        return !groundAhead || obstacleAhead || trapAhead || now - lastJumpAt > 1.15f;
    }

    private bool HasGroundAhead(float distance)
    {
        return Physics2D.Raycast(
            (Vector2)player.transform.position + new Vector2(distance, -0.2f),
            Vector2.down,
            2.2f,
            LayerMask.GetMask("Ground"));
    }

    private IEnumerator FinishLevel()
    {
        float elapsed = Time.realtimeSinceStartup - levelStartedAt;
        int level = SceneManager.GetActiveScene().buildIndex + 1;
        Debug.Log("UMBRA PLAYTHROUGH PASSED LEVEL " + level +
            " TIME=" + elapsed.ToString("F1") + "s" +
            " DEATHS=" + deaths + " RELOADS=" + reloads +
            " FURTHEST_X=" + furthestX.ToString("F1"));

        yield return new WaitForSecondsRealtime(0.25f);
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextScene);
            yield break;
        }

        Debug.Log("UMBRA FULL PLAYTHROUGH COMPLETE: ALL FIVE LEVELS PASSED");
        UmbraTestExit.Quit(0);
    }

    private static void ClearProgress()
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            PlayerRespawn.ClearSavedCheckpoint(i);
            PlayerPrefs.DeleteKey("UmbraHasKey_" + i);
        }

        PlayerPrefs.Save();
    }

    private static void Fail(string reason)
    {
        Debug.LogError("UMBRA FULL PLAYTHROUGH FAILED: " + reason);
        UmbraTestExit.Quit(3);
    }
}
