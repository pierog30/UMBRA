using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// Deterministic gameplay walkthrough used only when the executable receives
/// -umbraVideoCapture. It records real Unity frames without affecting normal play.
/// </summary>
public sealed class UmbraGameplayVideoCapture : MonoBehaviour
{
    private const int CaptureFps = 15;
    private const int CaptureWidth = 1280;
    private const int CaptureHeight = 720;

    private string captureDirectory;
    private int frameIndex;
    private bool captureFinished;
    private string phase = "INICIANDO DEMOSTRACION";
    private float phaseProgress;
    private PlayerController2D player;
    private Rigidbody2D playerBody;
    private SpriteRenderer playerRenderer;
    private PlayerSpriteAnimator playerAnimator;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        bool enabled = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-umbraVideoCapture")
            {
                enabled = true;
                break;
            }
        }

        if (enabled && FindAnyObjectByType<UmbraGameplayVideoCapture>() == null)
        {
            new GameObject("UMBRA Gameplay Video Capture").AddComponent<UmbraGameplayVideoCapture>();
        }
    }

    private IEnumerator Start()
    {
        captureDirectory = GetArgumentValue("-umbraCaptureDir");
        if (string.IsNullOrWhiteSpace(captureDirectory))
        {
            captureDirectory = Path.Combine(Application.persistentDataPath, "UmbraGameplayFrames");
        }

        if (Directory.Exists(captureDirectory))
        {
            Directory.Delete(captureDirectory, true);
        }
        Directory.CreateDirectory(captureDirectory);

        Screen.SetResolution(CaptureWidth, CaptureHeight, false);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = CaptureFps;
        Time.captureFramerate = CaptureFps;
        Time.timeScale = 1f;

        yield return null;
        yield return null;
        ConfigureScene();
        StartCoroutine(CaptureFrames());
        yield return RunWalkthrough();

        phase = "DEMO COMPLETA · UMBRA FUNCIONA EN UNITY";
        phaseProgress = 1f;
        yield return Hold(2.2f);
        captureFinished = true;
        File.WriteAllText(
            Path.Combine(captureDirectory, "capture_complete.txt"),
            "frames=" + frameIndex + "\nfps=" + CaptureFps + "\nresolution=" + CaptureWidth + "x" + CaptureHeight);
        Time.captureFramerate = 0;
        yield return null;
        Application.Quit();
    }

    private void ConfigureScene()
    {
        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.PrepareVideoCaptureState();
        }

        player = FindAnyObjectByType<PlayerController2D>();
        playerBody = player.GetComponent<Rigidbody2D>();
        playerRenderer = player.GetComponent<SpriteRenderer>();
        playerAnimator = player.GetComponent<PlayerSpriteAnimator>();

        AwakeningSequence2D awakening = player.GetComponent<AwakeningSequence2D>();
        if (awakening != null) awakening.enabled = false;
        player.enabled = false;
        if (playerAnimator != null) playerAnimator.enabled = false;
        playerBody.linearVelocity = Vector2.zero;
        playerBody.gravityScale = 0f;
        playerBody.bodyType = RigidbodyType2D.Kinematic;

        foreach (EnemyAI2D enemy in FindObjectsByType<EnemyAI2D>())
        {
            enemy.enabled = false;
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null) enemyCollider.enabled = false;
        }

        foreach (DeathTrap trap in FindObjectsByType<DeathTrap>())
        {
            trap.SetResponseMode(TrapResponseMode.WarningOnly);
        }

        CameraFollow2D cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow2D>() : null;
        if (cameraFollow != null)
        {
            cameraFollow.target = player.transform;
            cameraFollow.SnapToTarget();
        }
    }

    private IEnumerator RunWalkthrough()
    {
        phase = "CONTROLES · A/D MOVER · ESPACIO SALTAR · E INTERACTUAR";
        yield return Hold(2.2f);

        phase = "MOVIMIENTO REAL DEL PERSONAJE";
        yield return MovePlayer(new Vector3(-4.9f, -1.45f, 0f), 2.4f, false);

        phase = "FISICA 2D · LA CAJA ACTIVA LA PLACA DE PRESION";
        PushPullObject2D box = FindAnyObjectByType<PushPullObject2D>();
        PressureSwitch2D pressure = FindAnyObjectByType<PressureSwitch2D>();
        if (box != null && pressure != null)
        {
            Rigidbody2D boxBody = box.GetComponent<Rigidbody2D>();
            boxBody.bodyType = RigidbodyType2D.Kinematic;
            boxBody.gravityScale = 0f;
            yield return MoveObject(box.transform, pressure.transform.position + new Vector3(0f, 0.55f, 0f), 2.5f);
            yield return Hold(1.2f);
        }

        phase = "SEMANA 8 · IMPULSO UMBRAL CON Q Y RECARGA VISUAL";
        ShadowDash2D dash = player.GetComponent<ShadowDash2D>();
        if (dash != null)
        {
            dash.TryStartDash(1f);
            while (dash.IsDashing)
            {
                phaseProgress = 0.5f;
                yield return null;
            }
            playerBody.linearVelocity = Vector2.zero;
            yield return Hold(1.4f);
        }

        phase = "PARALLAX · FONDO Y NIEBLA SE MUEVEN A DISTINTA VELOCIDAD";
        yield return MovePlayer(new Vector3(1.2f, -1.4f, 0f), 3f, false);

        phase = "EXPLORACION · EL JUGADOR AVANZA POR EL ESCENARIO";
        yield return MovePlayer(new Vector3(0.6f, -1.4f, 0f), 2.7f, true);

        phase = "CHECKPOINT · GUARDA EL PUNTO DE REAPARICION";
        Checkpoint checkpoint = FindAnyObjectByType<Checkpoint>();
        if (checkpoint != null)
        {
            yield return MovePlayer(checkpoint.transform.position, 2f, false);
            yield return Hold(1.5f);
        }

        phase = "STRATEGY · LA TRAMPA CAMBIA A MODO ADVERTENCIA";
        DeathTrap visibleTrap = FindVisibleTrap();
        if (visibleTrap != null)
        {
            yield return MovePlayer(visibleTrap.transform.position + new Vector3(0f, 0.45f, 0f), 2f, false);
            yield return Hold(1.3f);
        }

        phase = "ESCALERA · ASCENSO HACIA LA LLAVE";
        yield return MovePlayer(new Vector3(4.9f, -1.15f, 0f), 1.5f, false);
        yield return MovePlayer(new Vector3(4.9f, 1.05f, 0f), 1.8f, true);

        phase = "PROGRESO · RECOGER LA LLAVE DESBLOQUEA LA PUERTA";
        CollectKey key = FindAnyObjectByType<CollectKey>();
        if (key != null)
        {
            yield return MovePlayer(key.transform.position, 1.8f, false);
            yield return Hold(1.5f);
        }

        phase = "ESCONDITE · EL ENEMIGO PIERDE DE VISTA AL JUGADOR";
        HidingSpot2D hidingSpot = FindAnyObjectByType<HidingSpot2D>();
        if (hidingSpot != null)
        {
            yield return MovePlayer(hidingSpot.transform.position, 2f, false);
            SetPlayerHidden(true);
            UmbraGameEvents.PublishInteraction("Jugador oculto");
            yield return Hold(1.5f);
            SetPlayerHidden(false);
            UmbraGameEvents.PublishInteraction("Jugador visible");
        }

        phase = "PUERTA · LA LLAVE PERMITE CONTINUAR";
        DoorGoal door = FindAnyObjectByType<DoorGoal>();
        if (door != null)
        {
            yield return MovePlayer(door.transform.position + new Vector3(-1f, 0f, 0f), 2.3f, false);
            UmbraGameEvents.PublishInteraction("Puerta abierta");
            door.gameObject.SetActive(false);
            yield return Hold(1.4f);
        }

        phase = "SALIDA · EL PORTAL CONECTA CON EL SIGUIENTE CAPITULO";
        FinishZone finish = FindAnyObjectByType<FinishZone>();
        if (finish != null)
        {
            yield return MovePlayer(finish.transform.position + new Vector3(-1.25f, 0f, 0f), 2.2f, false);
            UmbraGameEvents.PublishInteraction("Salida del capitulo preparada");
            yield return Hold(1.8f);
        }
    }

    private DeathTrap FindVisibleTrap()
    {
        foreach (DeathTrap trap in FindObjectsByType<DeathTrap>())
        {
            if (trap.GetComponent<SpriteRenderer>() != null && trap.GetComponent<SpriteRenderer>().enabled)
            {
                return trap;
            }
        }
        return null;
    }

    private IEnumerator MovePlayer(Vector3 target, float duration, bool jumpArc)
    {
        Vector3 start = player.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            Vector3 position = Vector3.Lerp(start, target, amount);
            if (jumpArc) position.y += Mathf.Sin(amount * Mathf.PI) * 1.05f;
            player.transform.position = position;
            phaseProgress = amount;
            AnimatePlayer(target.x - start.x, jumpArc, elapsed);
            Physics2D.SyncTransforms();
            yield return null;
        }
        player.transform.position = target;
        Physics2D.SyncTransforms();
    }

    private IEnumerator MoveObject(Transform item, Vector3 target, float duration)
    {
        Vector3 start = item.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            item.position = Vector3.Lerp(start, target, amount);
            phaseProgress = amount;
            Physics2D.SyncTransforms();
            yield return null;
        }
        item.position = target;
        Physics2D.SyncTransforms();
    }

    private IEnumerator Hold(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            phaseProgress = Mathf.Clamp01(elapsed / duration);
            AnimatePlayer(0f, false, elapsed);
            yield return null;
        }
    }

    private void AnimatePlayer(float direction, bool jumping, float elapsed)
    {
        if (playerAnimator == null || playerRenderer == null) return;
        Sprite[] frames = jumping ? playerAnimator.jumpFrames :
            Mathf.Abs(direction) > 0.01f ? playerAnimator.runFrames : playerAnimator.idleFrames;
        if (frames == null || frames.Length == 0) return;
        int index = Mathf.FloorToInt(elapsed * playerAnimator.framesPerSecond) % frames.Length;
        playerRenderer.sprite = frames[index];
        if (Mathf.Abs(direction) > 0.01f) playerRenderer.flipX = direction < 0f;
    }

    private void SetPlayerHidden(bool hidden)
    {
        Color color = playerRenderer.color;
        color.a = hidden ? 0.18f : 1f;
        playerRenderer.color = color;
    }

    private IEnumerator CaptureFrames()
    {
        while (!captureFinished)
        {
            yield return new WaitForEndOfFrame();
            Texture2D frame = UnityEngine.ScreenCapture.CaptureScreenshotAsTexture();
            byte[] jpg = UnityEngine.ImageConversion.EncodeToJPG(frame, 86);
            Destroy(frame);
            File.WriteAllBytes(Path.Combine(captureDirectory, "frame_" + frameIndex.ToString("D5") + ".jpg"), jpg);
            frameIndex++;
        }
    }

    private void OnGUI()
    {
        GUIStyle caption = new GUIStyle(GUI.skin.label);
        caption.fontStyle = FontStyle.Bold;
        caption.normal.textColor = Color.white;
        caption.fontSize = 22;
        caption.alignment = TextAnchor.MiddleCenter;

        Color previous = GUI.color;
        GUI.color = new Color(0.03f, 0.04f, 0.05f, 0.78f);
        GUI.DrawTexture(new Rect(0f, Screen.height - 82f, Screen.width, 82f), Texture2D.whiteTexture);
        GUI.color = previous;
        GUI.Label(new Rect(40f, Screen.height - 70f, Screen.width - 80f, 42f), phase, caption);

        GUI.color = new Color(0.7f, 0.12f, 0.15f, 0.95f);
        GUI.DrawTexture(new Rect(0f, Screen.height - 8f, Screen.width * Mathf.Clamp01(phaseProgress), 8f), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static string GetArgumentValue(string name)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name) return args[i + 1];
        }
        return null;
    }
}
