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
        bool shouldStart = arguments.Contains("-umbraSmoke");

        foreach (string argument in arguments)
        {
            const string prefix = "-umbraStressCycles=";
            if (!argument.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (int.TryParse(argument.Substring(prefix.Length), out int cycles))
            {
                requestedCycles = Mathf.Clamp(cycles, 1, 50);
                shouldStart = true;
            }
        }

        if (!shouldStart)
        {
            return;
        }

        var diagnostics = new GameObject("UMBRA Runtime Diagnostics");
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
        CheckComponent<Checkpoint>("Echo Lantern", errors);
        CheckComponent<CollectKey>("Echo Shard", errors);
        CheckComponent<DoorGoal>("Memory Threshold", errors);
        CheckComponent<FinishZone>("Return Portal", errors);

        if (FindObjectsByType<Checkpoint>().Length < 2)
        {
            errors.Add("extended checkpoint route");
        }

        FinishZone finish = FindAnyObjectByType<FinishZone>();
        if (finish == null || finish.transform.position.x < 118f)
        {
            errors.Add("extended level length");
        }

        GameManager manager = FindAnyObjectByType<GameManager>();
        PlayerController2D player = FindAnyObjectByType<PlayerController2D>();
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

        CheckCollider("Echo Shard", true, errors);
        CheckCollider("Return Portal", true, errors);
        CheckCollider("Memory Threshold", false, errors);
        CheckGroundedGameplayObjects(errors);
        CheckInteractionReadability(level, errors);

        if (FindObjectsByType<DeathTrap>().Length < 2)
        {
            errors.Add("death traps");
        }

        PressureSwitch2D pressureSwitch = FindAnyObjectByType<PressureSwitch2D>();
        if (pressureSwitch != null && pressureSwitch.targetTrap == null)
        {
            errors.Add("pressure switch target");
        }
        else if (pressureSwitch != null)
        {
            CheckTrapFeedback(pressureSwitch.targetTrap, errors);
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

            if (pushBox != null && pushBoxBody != null)
            {
                yield return StartCoroutine(CheckPushBoxControl(player, pushBox, pushBoxBody, errors));
            }
        }

        if (errors.Count > 0)
        {
            Debug.LogError(
                "UMBRA RUNTIME TEST FAILED CYCLE " + (completedCycles + 1) +
                " LEVEL " + level + ": " + string.Join(", ", errors));
            UmbraTestExit.Quit(2);
            yield break;
        }

        validatedLevelLoads++;
        if (requestedCycles <= 5 || validatedLevelLoads % 100 == 0)
        {
            Debug.Log(
                "UMBRA RUNTIME TEST PASSED LOAD " + validatedLevelLoads +
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
                Debug.Log("UMBRA STRESS PROGRESS: " + completedCycles + "/" + requestedCycles + " CYCLES");
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
            yield break;
        }

        Debug.Log(
            "UMBRA RUNTIME STRESS COMPLETE: " + completedCycles + " CYCLES, " +
            validatedLevelLoads + " LEVEL LOADS PASSED");
        UmbraTestExit.Quit(0);
    }

    private static IEnumerator CheckWallFall(
        PlayerController2D player,
        Rigidbody2D playerBody,
        GameManager manager,
        List<string> errors)
    {
        manager.gameStarted = true;
        Time.timeScale = 1f;

        Vector2 testPosition = new Vector2(-7f, 1f);
        GameObject wall = new GameObject("Diagnostics Wall");
        wall.layer = LayerMask.NameToLayer("Ground");
        wall.transform.position = new Vector2(testPosition.x + 0.46f, 1f);
        BoxCollider2D wallCollider = wall.AddComponent<BoxCollider2D>();
        wallCollider.size = new Vector2(0.4f, 4f);

        playerBody.position = testPosition;
        playerBody.linearVelocity = new Vector2(2f, 0f);
        Physics2D.SyncTransforms();
        float startY = playerBody.position.y;

        for (int i = 0; i < 24; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (float.IsNaN(playerBody.position.x) || float.IsNaN(playerBody.position.y))
        {
            errors.Add("invalid player physics values");
        }
        else if (playerBody.position.y > startY - 0.25f)
        {
            errors.Add(
                "player remained stuck to a wall (drop=" +
                (startY - playerBody.position.y).ToString("F2") + ")");
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

        if (Mathf.Abs(pushBoxBody.linearVelocity.x) > 0.2f)
        {
            errors.Add("push box is slippery after pushing");
        }
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
        GameObject obj = FindSceneObject(objectName);
        if (obj == null || obj.GetComponent<T>() == null)
        {
            errors.Add(objectName + "/" + typeof(T).Name);
        }
    }

    private static void CheckCollider(string objectName, bool shouldBeTrigger, List<string> errors)
    {
        GameObject obj = FindSceneObject(objectName);
        Collider2D collider = obj != null ? obj.GetComponent<Collider2D>() : null;
        if (collider == null || collider.isTrigger != shouldBeTrigger)
        {
            errors.Add(objectName + " collider");
        }
    }

    private static void CheckInteractionReadability(int level, List<string> errors)
    {
        DoorGoal door = FindAnyObjectByType<DoorGoal>();
        BoxCollider2D doorCollider = door != null ? door.GetComponent<BoxCollider2D>() : null;
        if (door == null || doorCollider == null || doorCollider.bounds.size.y < 7f ||
            door.barrierRenderer == null || !door.barrierRenderer.enabled)
        {
            errors.Add("unskippable visible memory barrier");
        }

        ClimbZone2D ladder = FindAnyObjectByType<ClimbZone2D>();
        if (ladder == null || FindSceneObject("Ladder Beacon") == null ||
            ladder.transform.Find("Ladder Highlight") == null)
        {
            errors.Add("visible ladder guidance");
        }

        if (level == 1 || level == 3 || level == 5)
        {
            PushPullObject2D cube = FindAnyObjectByType<PushPullObject2D>();
            if (cube == null || cube.transform.Find("Cube Highlight") == null ||
                FindAnyObjectByType<ResonanceLink2D>() == null)
            {
                errors.Add("readable resonance puzzle");
            }
        }
    }

    private static void CheckTrapFeedback(DeathTrap trap, List<string> errors)
    {
        if (trap == null)
        {
            return;
        }

        Collider2D trapCollider = trap.GetComponent<Collider2D>();
        SpriteRenderer trapRenderer = trap.GetComponent<SpriteRenderer>();
        Vector3 armedScale = trap.transform.localScale;
        trap.SetArmed(false);

        bool remainsVisible = trapRenderer != null && trapRenderer.enabled && trapRenderer.color.a >= 0.6f;
        bool clearlyRetracted = trap.GetComponent<SimpleMover2D>() != null ||
            trap.transform.localScale.y <= armedScale.y * 0.4f;
        if (trap.IsArmed || (trapCollider != null && trapCollider.enabled) || !remainsVisible || !clearlyRetracted)
        {
            errors.Add("visible retracted trap feedback");
        }

        trap.SetArmed(true);
        if (!trap.IsArmed || (trapCollider != null && !trapCollider.enabled) ||
            Vector3.Distance(trap.transform.localScale, armedScale) > 0.01f)
        {
            errors.Add("trap rearm state");
        }
    }

    private static GameObject FindSceneObject(string objectName)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(obj => obj.scene.IsValid() && obj.name == objectName);
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

    private static void CheckGroundedGameplayObjects(List<string> errors)
    {
        Physics2D.SyncTransforms();
        int groundMask = LayerMask.GetMask("Ground");
        foreach (SpriteRenderer renderer in FindObjectsByType<SpriteRenderer>())
        {
            string objectName = renderer.gameObject.name;
            bool shouldBeGrounded = objectName == "Memory Cube" || objectName == "Resonance Pad" ||
                objectName == "Echo Lantern" || objectName == "Ribbon Ladder" ||
                objectName == "Thorn Knot" || objectName == "Tuning Fork" ||
                objectName == "Memory Threshold" || objectName == "Return Portal" ||
                objectName.StartsWith("Memory Tablet", System.StringComparison.Ordinal);
            if (!shouldBeGrounded)
            {
                continue;
            }

            Bounds bounds = renderer.bounds;
            Collider2D[] hits = Physics2D.OverlapPointAll(
                new Vector2(bounds.center.x, bounds.min.y - 0.035f),
                groundMask);
            bool supported = hits.Any(hit => hit.gameObject != renderer.gameObject && !hit.isTrigger);
            if (!supported)
            {
                errors.Add(objectName + " floating");
            }
        }
    }
}
