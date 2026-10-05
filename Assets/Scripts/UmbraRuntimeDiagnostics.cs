using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UmbraRuntimeDiagnostics : MonoBehaviour
{
    private static int requestedCycles = 1;

    private int completedCycles;
    private int validatedLevelLoads;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        string[] arguments = System.Environment.GetCommandLineArgs();
        bool shouldStart = arguments.Contains("-umbralSmoke") || arguments.Contains("-umbraSmoke");

        foreach (string argument in arguments)
        {
            const string prefix = "-umbraStressCycles=";
            if (!argument.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (int.TryParse(argument.Substring(prefix.Length), out int cycles))
            {
                requestedCycles = Mathf.Clamp(cycles, 1, 1000);
                shouldStart = true;
            }
        }

        if (!shouldStart)
        {
            return;
        }

        var diagnostics = new GameObject("UMBRAL Runtime Diagnostics");
        DontDestroyOnLoad(diagnostics);
        diagnostics.AddComponent<UmbraRuntimeDiagnostics>();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(ValidateCurrentLevel());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ValidateCurrentLevel());
    }

    private IEnumerator ValidateCurrentLevel()
    {
        yield return null;

        int level = SceneManager.GetActiveScene().buildIndex + 1;
        var errors = new List<string>();
        CheckComponent<GameManager>("GameManager", errors);
        CheckComponent<PlayerController2D>("Player", errors);
        CheckComponent<PlayerRespawn>("Player", errors);
        CheckComponent<PlayerSpriteAnimator>("Player", errors);
        CheckComponent<CameraFollow2D>("Main Camera", errors);
        CheckComponent<Checkpoint>("Checkpoint", errors);
        CheckComponent<CollectKey>("Key", errors);
        CheckComponent<DoorGoal>("Locked Door", errors);
        CheckComponent<FinishZone>("Finish Zone", errors);
        CheckComponent<HidingSpot2D>("Hiding Alcove", errors);
        CheckComponent<NarrativeEcho2D>("Memory Echo " + level, errors);

        GameManager manager = FindAnyObjectByType<GameManager>();
        PlayerController2D player = FindAnyObjectByType<PlayerController2D>();
        UmbraEventJournal eventJournal = FindAnyObjectByType<UmbraEventJournal>();
        if (eventJournal == null || UmbraGameEvents.ObserverCount < 3 || UmbraGameEvents.PublishedEventCount < 2)
        {
            errors.Add("observer pattern wiring");
        }
        Rigidbody2D playerBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
        BoxCollider2D playerCollider = player != null ? player.GetComponent<BoxCollider2D>() : null;
        if (playerCollider == null || playerCollider.sharedMaterial == null || playerCollider.sharedMaterial.friction > 0.01f)
        {
            errors.Add("player wall-friction setup");
        }

        if (playerBody == null || playerBody.gravityScale <= 0f || playerBody.interpolation != RigidbodyInterpolation2D.Interpolate)
        {
            errors.Add("player rigidbody setup");
        }

        if (player == null || player.coyoteTime <= 0f || player.jumpBufferTime <= 0f || player.acceleration <= 0f)
        {
            errors.Add("smooth movement settings");
        }

        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic)
        {
            errors.Add("orthographic camera");
        }

        AudioListener[] listeners = FindObjectsByType<AudioListener>();
        UmbraAudio audio = FindAnyObjectByType<UmbraAudio>();
        if (listeners.Length != 1 || camera == null || camera.GetComponent<AudioListener>() == null)
        {
            errors.Add("audio listener");
        }

        if (audio == null || !audio.IsConfigured || !HasAudibleAmbience(audio))
        {
            errors.Add("audible audio signal");
        }

        CheckCollider("Key", true, errors);
        CheckCollider("Finish Zone", true, errors);
        CheckCollider("Locked Door", false, errors);

        if (FindObjectsByType<DeathTrap>().Length < 2)
        {
            errors.Add("death traps");
        }

        EnemyAI2D enemy = FindAnyObjectByType<EnemyAI2D>();
        Collider2D enemyCollider = enemy != null ? enemy.GetComponent<Collider2D>() : null;
        if (enemy == null || enemyCollider == null || !enemyCollider.isTrigger ||
            enemy.chaseSpeed <= enemy.patrolSpeed || enemy.viewDistance <= 0f || enemy.obstacleMask.value == 0)
        {
            errors.Add("enemy AI configuration");
        }

        int horrorCount = FindObjectsByType<HorrorEvent2D>().Length;
        if (horrorCount != (level == 5 ? 2 : 1))
        {
            errors.Add("horror event count");
        }

        CameraFollow2D boundedCamera = Camera.main != null ? Camera.main.GetComponent<CameraFollow2D>() : null;
        if (boundedCamera == null || !boundedCamera.useBounds || boundedCamera.maxBounds.x <= boundedCamera.minBounds.x)
        {
            errors.Add("camera bounds");
        }

        if (level == 1 && FindAnyObjectByType<AwakeningSequence2D>() == null)
        {
            errors.Add("awakening sequence");
        }

        HidingSpot2D hidingSpot = FindAnyObjectByType<HidingSpot2D>();
        RaycastHit2D hidingGround = hidingSpot != null
            ? Physics2D.Raycast(hidingSpot.transform.position, Vector2.down, 3f, LayerMask.GetMask("Ground"))
            : default;
        if (hidingSpot == null || hidingGround.collider == null)
        {
            errors.Add("hiding spot unsupported");
        }

        DeathTrap visibleTrap = FindObjectsByType<DeathTrap>()
            .FirstOrDefault(trap => trap.GetComponent<SpriteRenderer>() != null && trap.GetComponent<SpriteRenderer>().enabled);
        if (visibleTrap != null)
        {
            Collider2D trapCollider = visibleTrap.GetComponent<Collider2D>();
            TrapResponseMode originalMode = visibleTrap.responseMode;
            visibleTrap.SetResponseMode(TrapResponseMode.WarningOnly);
            if (visibleTrap.ActiveStrategyName != "WarningOnly") errors.Add("warning trap strategy");
            visibleTrap.SetResponseMode(TrapResponseMode.Lethal);
            if (visibleTrap.ActiveStrategyName != "Lethal") errors.Add("lethal trap strategy");
            visibleTrap.SetResponseMode(originalMode);
            visibleTrap.SetArmed(false);
            if (trapCollider != null && trapCollider.enabled) errors.Add("trap did not disarm");
            visibleTrap.SetArmed(true);
            if (trapCollider != null && !trapCollider.enabled) errors.Add("trap did not rearm");
        }

        PressureSwitch2D pressureSwitch = FindAnyObjectByType<PressureSwitch2D>();
        if (pressureSwitch != null && pressureSwitch.targetTrap == null)
        {
            errors.Add("pressure switch target");
        }

        LeverSwitch2D lever = FindAnyObjectByType<LeverSwitch2D>();
        if (lever != null && lever.targetTrap == null)
        {
            errors.Add("lever target");
        }

        if ((level == 2 || level == 4 || level == 5) &&
            FindObjectsByType<MovingPlatform2D>().Length == 0)
        {
            errors.Add("moving platforms");
        }

        PushPullObject2D pushBox = FindAnyObjectByType<PushPullObject2D>();
        Rigidbody2D pushBoxBody = pushBox != null ? pushBox.GetComponent<Rigidbody2D>() : null;
        if ((level == 1 || level == 3 || level == 5) && pushBox == null)
        {
            errors.Add("push box");
        }

        if (pushBox != null &&
            (pushBoxBody == null || pushBoxBody.mass < 3f || pushBoxBody.linearDamping < 1f ||
             pushBox.pushSpeed < 3.2f || pushBox.maxHorizontalSpeed < pushBox.pushSpeed ||
             pushBox.maxHorizontalSpeed > 4.5f || pushBox.acceleration < 25f || pushBox.braking <= 0f))
        {
            errors.Add("controlled push box settings");
        }

        if (completedCycles == 0 && player != null && playerBody != null && manager != null)
        {
            yield return StartCoroutine(CheckWallFall(player, playerBody, manager, errors));
            yield return StartCoroutine(CheckLadderTraversal(player, playerBody, errors));

            if (pushBox != null && pushBoxBody != null)
            {
                yield return StartCoroutine(CheckPushBoxControl(player, pushBox, pushBoxBody, errors));
            }
        }

        if (errors.Count > 0)
        {
            Debug.LogError(
                "UMBRAL RUNTIME TEST FAILED CYCLE " + (completedCycles + 1) +
                " LEVEL " + level + ": " + string.Join(", ", errors));
            Application.Quit(2);
            yield break;
        }

        validatedLevelLoads++;
        if (requestedCycles <= 5 || validatedLevelLoads % 100 == 0)
        {
            Debug.Log(
                "UMBRAL RUNTIME TEST PASSED LOAD " + validatedLevelLoads +
                " CYCLE " + (completedCycles + 1) + " LEVEL " + level);
        }

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextIndex);
            yield break;
        }

        completedCycles++;
        if (completedCycles < requestedCycles)
        {
            if (completedCycles % 20 == 0)
            {
                Debug.Log("UMBRAL STRESS PROGRESS: " + completedCycles + "/" + requestedCycles + " CYCLES");
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
            yield break;
        }

        Debug.Log(
            "UMBRAL RUNTIME STRESS COMPLETE: " + completedCycles + " CYCLES, " +
            validatedLevelLoads + " LEVEL LOADS PASSED");
        Application.Quit(0);
    }

    private static IEnumerator CheckWallFall(
        PlayerController2D player,
        Rigidbody2D playerBody,
        GameManager manager,
        List<string> errors)
    {
        manager.gameStarted = true;
        Time.timeScale = 1f;

        Vector2 testPosition = new Vector2(player.transform.position.x, 1f);
        GameObject wall = new GameObject("Diagnostics Wall");
        wall.layer = LayerMask.NameToLayer("Ground");
        wall.transform.position = new Vector2(testPosition.x + 0.46f, 1f);
        BoxCollider2D wallCollider = wall.AddComponent<BoxCollider2D>();
        wallCollider.size = new Vector2(0.4f, 4f);

        playerBody.position = testPosition;
        playerBody.linearVelocity = new Vector2(2f, 0f);
        Physics2D.SyncTransforms();
        float startY = playerBody.position.y;

        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (float.IsNaN(playerBody.position.x) || float.IsNaN(playerBody.position.y))
        {
            errors.Add("invalid player physics values");
        }
        else if (playerBody.position.y > startY - 0.15f)
        {
            errors.Add("player remained stuck to a wall");
        }

        Destroy(wall);
    }

    private static IEnumerator CheckPushBoxControl(
        PlayerController2D player,
        PushPullObject2D pushBox,
        Rigidbody2D pushBoxBody,
        List<string> errors)
    {
        pushBoxBody.linearVelocity = new Vector2(20f, pushBoxBody.linearVelocity.y);
        yield return new WaitForFixedUpdate();

        if (Mathf.Abs(pushBoxBody.linearVelocity.x) > pushBox.maxHorizontalSpeed + 0.1f)
        {
            errors.Add("push box exceeded speed limit");
        }

        for (int i = 0; i < 30; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (pushBoxBody == null)
        {
            errors.Add("push box was destroyed during braking test");
            yield break;
        }

        if (Mathf.Abs(pushBoxBody.linearVelocity.x) > 0.2f)
        {
            errors.Add("push box did not brake");
        }

        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        player.enabled = false;
        playerBody.simulated = false;
        pushBoxBody.gravityScale = 0f;
        pushBoxBody.position = new Vector2(0f, 5f);
        pushBoxBody.linearVelocity = Vector2.zero;
        SetPlayerState(player, 1f, false, true);
        float startX = pushBoxBody.position.x;

        for (int i = 0; i < 60; i++)
        {
            if (pushBoxBody == null || player == null)
            {
                errors.Add("push box scene changed during control test");
                yield break;
            }
            player.transform.position = pushBoxBody.position + new Vector2(-0.8f, 0f);
            yield return new WaitForFixedUpdate();
        }

        float distanceInOneSecond = pushBoxBody.position.x - startX;
        if (distanceInOneSecond < 2.8f)
        {
            errors.Add("push box feels too slow");
        }
        else if (distanceInOneSecond > 4.3f)
        {
            errors.Add("push box moves too fast");
        }

        SetPlayerState(player, 0f, false, true);
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (pushBoxBody == null)
        {
            errors.Add("push box was destroyed during stop test");
            yield break;
        }

        if (Mathf.Abs(pushBoxBody.linearVelocity.x) > 0.2f)
        {
            errors.Add("push box is slippery after pushing");
        }
    }

    private static IEnumerator CheckLadderTraversal(
        PlayerController2D player,
        Rigidbody2D playerBody,
        List<string> errors)
    {
        ClimbZone2D climbZone = FindAnyObjectByType<ClimbZone2D>();
        if (climbZone == null)
        {
            errors.Add("ladder missing");
            yield break;
        }

        PlatformEffector2D platform = FindObjectsByType<PlatformEffector2D>()
            .OrderBy(effector => Mathf.Abs(effector.transform.position.x - climbZone.transform.position.x))
            .FirstOrDefault();
        Collider2D platformCollider = platform != null ? platform.GetComponent<Collider2D>() : null;
        if (platformCollider == null)
        {
            errors.Add("one-way ladder platform missing");
            yield break;
        }

        DeathTrap[] traps = FindObjectsByType<DeathTrap>();
        foreach (DeathTrap trap in traps) trap.SetArmed(false);
        EnemyAI2D[] enemies = FindObjectsByType<EnemyAI2D>();
        foreach (EnemyAI2D enemy in enemies) enemy.enabled = false;

        Vector2 originalPosition = playerBody.position;
        float originalGravity = playerBody.gravityScale;
        player.enabled = false;
        playerBody.simulated = true;
        playerBody.gravityScale = 0f;
        float platformTop = platformCollider.bounds.max.y;
        playerBody.position = new Vector2(climbZone.transform.position.x, platformCollider.bounds.min.y - 0.85f);
        Physics2D.SyncTransforms();

        for (int i = 0; i < 75; i++)
        {
            playerBody.linearVelocity = new Vector2(0f, player.climbSpeed);
            yield return new WaitForFixedUpdate();
        }

        if (playerBody.position.y <= platformTop + 0.45f)
        {
            errors.Add("ladder blocked by upper platform");
        }

        playerBody.linearVelocity = Vector2.zero;
        playerBody.position = originalPosition;
        playerBody.gravityScale = originalGravity;
        Physics2D.SyncTransforms();
        player.enabled = true;
        foreach (EnemyAI2D enemy in enemies) if (enemy != null) enemy.enabled = true;
        foreach (DeathTrap trap in traps) if (trap != null) trap.SetArmed(true);
    }

    private static void SetPlayerState(
        PlayerController2D player,
        float horizontalInput,
        bool isInteracting,
        bool isGrounded)
    {
        const System.Reflection.BindingFlags flags =
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;
        typeof(PlayerController2D).GetProperty(nameof(PlayerController2D.HorizontalInput), flags)
            ?.SetValue(player, horizontalInput);
        typeof(PlayerController2D).GetProperty(nameof(PlayerController2D.IsInteracting), flags)
            ?.SetValue(player, isInteracting);
        typeof(PlayerController2D).GetProperty(nameof(PlayerController2D.IsGrounded), flags)
            ?.SetValue(player, isGrounded);
    }

    private static void CheckComponent<T>(string objectName, List<string> errors) where T : Component
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null || obj.GetComponent<T>() == null)
        {
            errors.Add(objectName + "/" + typeof(T).Name);
        }
    }

    private static void CheckCollider(string objectName, bool shouldBeTrigger, List<string> errors)
    {
        GameObject obj = GameObject.Find(objectName);
        Collider2D collider = obj != null ? obj.GetComponent<Collider2D>() : null;
        if (collider == null || collider.isTrigger != shouldBeTrigger)
        {
            errors.Add(objectName + " collider");
        }
    }

    private static bool HasAudibleAmbience(UmbraAudio audio)
    {
        AudioSource ambience = audio.GetComponents<AudioSource>()
            .FirstOrDefault(source => source.loop && source.clip != null);
        if (ambience == null || ambience.clip.samples <= 0)
        {
            return false;
        }

        int sampleCount = Mathf.Min(8192, ambience.clip.samples);
        float[] samples = new float[sampleCount];
        if (!ambience.clip.GetData(samples, 0))
        {
            return false;
        }

        return samples.Max(sample => Mathf.Abs(sample)) * ambience.volume >= 0.025f;
    }
}
