using UnityEngine;

public static class UmbraInput
{
    public static float Horizontal => Mobile != null && Mobile.HorizontalActive
        ? Mobile.Horizontal
        : Input.GetAxisRaw("Horizontal");

    public static float Vertical => Mobile != null && Mobile.VerticalActive
        ? Mobile.Vertical
        : Input.GetAxisRaw("Vertical");

    public static bool JumpDown =>
        Input.GetKeyDown(KeyCode.Space) ||
        Input.GetKeyDown(KeyCode.W) ||
        Input.GetKeyDown(KeyCode.UpArrow) ||
        (Mobile != null && Mobile.JumpDown);

    public static bool JumpUp =>
        Input.GetKeyUp(KeyCode.Space) ||
        Input.GetKeyUp(KeyCode.W) ||
        Input.GetKeyUp(KeyCode.UpArrow) ||
        (Mobile != null && Mobile.JumpUp);

    public static bool CrouchHeld =>
        Input.GetKey(KeyCode.S) ||
        Input.GetKey(KeyCode.DownArrow) ||
        (Mobile != null && Mobile.CrouchHeld);

    public static bool RunHeld =>
        Input.GetKey(KeyCode.LeftShift) ||
        (Mobile != null && Mobile.RunHeld);

    public static bool InteractHeld => Input.GetKey(KeyCode.E) || (Mobile != null && Mobile.InteractHeld);
    public static bool InteractDown => Input.GetKeyDown(KeyCode.E) || (Mobile != null && Mobile.InteractDown);
    public static bool DashDown => Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.RightControl) || (Mobile != null && Mobile.DashDown);
    public static bool RestartDown => Input.GetKeyDown(KeyCode.R) || (Mobile != null && Mobile.RestartDown);
    public static bool ConfirmDown => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || (Mobile != null && Mobile.ConfirmDown);
    public static bool PauseDown => Input.GetKeyDown(KeyCode.Escape) || (Mobile != null && Mobile.PauseDown);
    public static bool MobileMode => Mobile != null && Mobile.ControlsVisible;

    private static UmbraMobileControls Mobile => UmbraMobileControls.Instance;
}

[DefaultExecutionOrder(-1000)]
public sealed class UmbraMobileControls : MonoBehaviour
{
    private const string PreviewKey = "UMBRA_MOBILE_PREVIEW";

    public static UmbraMobileControls Instance { get; private set; }

    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool HorizontalActive => Mathf.Abs(Horizontal) > 0.01f;
    public bool VerticalActive => Mathf.Abs(Vertical) > 0.01f;
    public bool JumpDown { get; private set; }
    public bool JumpUp { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool RunHeld { get; private set; }
    public bool InteractHeld { get; private set; }
    public bool InteractDown { get; private set; }
    public bool DashDown { get; private set; }
    public bool RestartDown { get; private set; }
    public bool ConfirmDown => InteractDown || JumpDown;
    public bool PauseDown { get; private set; }
    public bool ControlsVisible => Application.isMobilePlatform || previewEnabled;
    public string LastLayout { get; private set; } = "1280x720 landscape";

    private bool previewEnabled;
    private bool previousJump;
    private bool previousInteract;
    private bool previousDash;
    private bool previousPause;
    private bool previousRestart;
    private GUIStyle buttonStyle;
    private GUIStyle captionStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntimeControls()
    {
        if (Instance != null) return;
        GameObject controls = new GameObject("UMBRA Mobile Controls");
        DontDestroyOnLoad(controls);
        controls.AddComponent<UmbraMobileControls>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        previewEnabled = PlayerPrefs.GetInt(PreviewKey, 0) == 1;

        if (Application.isMobilePlatform)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Application.targetFrameRate = 60;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            previewEnabled = !previewEnabled;
            PlayerPrefs.SetInt(PreviewKey, previewEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UmbraGameEvents.PublishInteraction(previewEnabled ? "Vista movil activada" : "Vista movil desactivada");
        }

        ResetFramePulses();
        if (!ControlsVisible)
        {
            ResetHeldState();
            return;
        }

        Rect safe = GetSafeAreaInGuiCoordinates();
        LastLayout = $"{Screen.width}x{Screen.height} landscape safe {Mathf.RoundToInt(safe.width)}x{Mathf.RoundToInt(safe.height)}";
        TouchLayout layout = new TouchLayout(safe);

        bool left = IsPressed(layout.Left);
        bool right = IsPressed(layout.Right);
        bool up = IsPressed(layout.Up);
        bool down = IsPressed(layout.Down);
        bool jump = IsPressed(layout.Jump);
        bool interact = IsPressed(layout.Interact);
        bool dash = IsPressed(layout.Dash);
        bool run = IsPressed(layout.Run);
        bool pause = IsPressed(layout.Pause);
        bool restart = IsPressed(layout.Restart);

        Horizontal = left == right ? 0f : left ? -1f : 1f;
        Vertical = up == down ? 0f : up ? 1f : -1f;
        CrouchHeld = down;
        RunHeld = run;
        InteractHeld = interact;

        JumpDown = jump && !previousJump;
        JumpUp = !jump && previousJump;
        InteractDown = interact && !previousInteract;
        DashDown = dash && !previousDash;
        PauseDown = pause && !previousPause;
        RestartDown = restart && !previousRestart;

        previousJump = jump;
        previousInteract = interact;
        previousDash = dash;
        previousPause = pause;
        previousRestart = restart;
    }

    private void OnGUI()
    {
        if (!ControlsVisible) return;

        EnsureStyles();
        Rect safe = GetSafeAreaInGuiCoordinates();
        TouchLayout layout = new TouchLayout(safe);

        Color previousColor = GUI.color;
        GUI.color = new Color(0.5f, 0.2f, 0.72f, 0.18f);
        GUI.Box(safe, GUIContent.none);
        GUI.color = previousColor;

        DrawButton(layout.Left, "◀");
        DrawButton(layout.Right, "▶");
        DrawButton(layout.Up, "▲");
        DrawButton(layout.Down, "▼");
        DrawButton(layout.Run, "CORRER");
        DrawButton(layout.Jump, "SALTAR");
        DrawButton(layout.Dash, "IMPULSO");
        DrawButton(layout.Interact, "ACCIÓN");
        DrawButton(layout.Restart, "R");
        DrawButton(layout.Pause, "II");

        GUI.Label(new Rect(safe.x + 12f, safe.y + 8f, safe.width - 24f, 28f),
            "MODO MÓVIL   HORIZONTAL   ÁREA SEGURA", captionStyle);
    }

    private void DrawButton(Rect rect, string label)
    {
        Color previous = GUI.color;
        GUI.color = IsPressed(rect)
            ? new Color(0.72f, 0.33f, 0.96f, 0.92f)
            : new Color(0.08f, 0.07f, 0.12f, 0.68f);
        GUI.Box(rect, label, buttonStyle);
        GUI.color = previous;
    }

    private static Rect GetSafeAreaInGuiCoordinates()
    {
        Rect area = Screen.safeArea;
        return new Rect(area.x, Screen.height - area.yMax, area.width, area.height);
    }

    private static bool IsPressed(Rect guiRect)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) continue;
            Vector2 point = new Vector2(touch.position.x, Screen.height - touch.position.y);
            if (guiRect.Contains(point)) return true;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            return guiRect.Contains(mouse);
        }

        return false;
    }

    private void ResetFramePulses()
    {
        JumpDown = false;
        JumpUp = false;
        InteractDown = false;
        DashDown = false;
        PauseDown = false;
        RestartDown = false;
    }

    private void ResetHeldState()
    {
        Horizontal = 0f;
        Vertical = 0f;
        CrouchHeld = false;
        RunHeld = false;
        InteractHeld = false;
        previousJump = false;
        previousInteract = false;
        previousDash = false;
        previousPause = false;
        previousRestart = false;
    }

    private void EnsureStyles()
    {
        if (buttonStyle != null) return;

        buttonStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 45f), 14, 24)
        };
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.padding = new RectOffset(4, 4, 4, 4);

        captionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperCenter,
            fontStyle = FontStyle.Bold,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 52f), 12, 18)
        };
        captionStyle.normal.textColor = new Color(0.9f, 0.82f, 1f, 0.92f);
    }

    private readonly struct TouchLayout
    {
        public readonly Rect Left;
        public readonly Rect Right;
        public readonly Rect Up;
        public readonly Rect Down;
        public readonly Rect Run;
        public readonly Rect Jump;
        public readonly Rect Dash;
        public readonly Rect Interact;
        public readonly Rect Pause;
        public readonly Rect Restart;

        public TouchLayout(Rect safe)
        {
            float scale = Mathf.Clamp(Mathf.Min(safe.width / 1280f, safe.height / 720f), 0.65f, 1.45f);
            float button = 82f * scale;
            float large = 104f * scale;
            float gap = 12f * scale;
            float edge = 22f * scale;
            float bottom = safe.yMax - edge;

            Left = new Rect(safe.x + edge, bottom - button, button, button);
            Right = new Rect(Left.xMax + gap, Left.y, button, button);
            Up = new Rect(Left.x + button + gap * 0.5f, Left.y - button - gap, button, button);
            Down = new Rect(Up.x, Left.y, button, button);
            Run = new Rect(Left.x, Up.y, button * 1.35f, button * 0.72f);

            Interact = new Rect(safe.xMax - edge - large, bottom - large, large, large);
            Jump = new Rect(Interact.x - gap - large, Interact.y, large, large);
            Dash = new Rect(Interact.x - button * 0.55f, Interact.y - button - gap, button * 1.25f, button * 0.78f);

            Restart = new Rect(safe.x + edge, safe.y + edge, button * 0.72f, button * 0.55f);
            Pause = new Rect(safe.xMax - edge - button * 0.72f, safe.y + edge, button * 0.72f, button * 0.55f);
        }
    }
}
