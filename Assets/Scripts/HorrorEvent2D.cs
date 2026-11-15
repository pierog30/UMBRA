using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HorrorEvent2D : MonoBehaviour
{
    public enum HorrorStyle { BackgroundRise, ForegroundRush, EnvironmentShift, SpawnThreat, CameraPulse }
    public enum ScenarioState { Waiting, Activating, Active, Completed }

    [Header("Identidad del evento")]
    public string eventId = "event";
    public HorrorStyle style;
    public GameObject revealObject;

    [Header("Respuesta del escenario")]
    public float duration = 1.4f;
    public bool persistForRun = true;
    public AnimationCurve motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool animateSprite = true;
    [Range(0f, 0.2f)] public float pulseAmount = 0.08f;
    public Color activeTint = new Color(0.72f, 0.82f, 1f, 1f);

    [Header("Depuracion en Play")]
    [SerializeField] private ScenarioState debugState = ScenarioState.Waiting;

    private bool fired;
    private SpriteRenderer revealRenderer;
    private Color baseColor = Color.white;
    private Vector3 baseScale = Vector3.one;

    public ScenarioState CurrentState { get; private set; } = ScenarioState.Waiting;
    public event Action<ScenarioState, ScenarioState> StateChanged;

    private string SaveKey => "UmbralHorror_" + SceneManager.GetActiveScene().buildIndex + "_" + eventId;

    private void Awake()
    {
        fired = persistForRun && PlayerPrefs.GetInt(SaveKey, 0) == 1;
        if (revealObject != null)
        {
            bool shouldRemain = fired && (style == HorrorStyle.EnvironmentShift || style == HorrorStyle.SpawnThreat);
            revealObject.SetActive(shouldRemain);
            revealRenderer = revealObject.GetComponentInChildren<SpriteRenderer>(true);
            if (revealRenderer != null) baseColor = revealRenderer.color;
            baseScale = revealObject.transform.localScale;
        }
        ChangeState(fired ? ScenarioState.Active : ScenarioState.Waiting);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (fired || other.GetComponent<PlayerController2D>() == null) return;
        fired = true;
        if (persistForRun)
        {
            PlayerPrefs.SetInt(SaveKey, 1);
            PlayerPrefs.Save();
        }
        ChangeState(ScenarioState.Activating);
        UmbraGameEvents.PublishInteraction("Escenario dinamico activado: " + style);
        StartCoroutine(PlayEvent());
    }

    private IEnumerator PlayEvent()
    {
        UmbraAudio.Instance?.PlayScare();
        Camera.main?.GetComponent<CameraFollow2D>()?.AddTrauma(style == HorrorStyle.CameraPulse ? 0.45f : 0.8f);
        if (revealObject != null)
        {
            revealObject.SetActive(true);
            Vector3 start = revealObject.transform.position;
            Vector3 offset = style == HorrorStyle.ForegroundRush ? Vector3.left * 3f : Vector3.down * 1.3f;
            revealObject.transform.position = start + offset;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                float progress = motionCurve == null ? normalized : motionCurve.Evaluate(normalized);
                revealObject.transform.position = Vector3.LerpUnclamped(start + offset, start, progress);
                if (animateSprite)
                {
                    float pulse = 1f + Mathf.Sin(normalized * Mathf.PI) * pulseAmount;
                    revealObject.transform.localScale = baseScale * pulse;
                    if (revealRenderer != null)
                    {
                        revealRenderer.color = Color.Lerp(baseColor, activeTint, Mathf.Sin(normalized * Mathf.PI));
                    }
                }
                yield return null;
            }
            revealObject.transform.position = start;
            revealObject.transform.localScale = baseScale;
            if (revealRenderer != null) revealRenderer.color = baseColor;
            if (style != HorrorStyle.EnvironmentShift && style != HorrorStyle.SpawnThreat)
            {
                revealObject.SetActive(false);
                ChangeState(ScenarioState.Completed);
            }
            else
            {
                ChangeState(ScenarioState.Active);
            }
        }
        else
        {
            ChangeState(ScenarioState.Completed);
        }
    }

    private void ChangeState(ScenarioState nextState)
    {
        if (CurrentState == nextState)
        {
            debugState = nextState;
            return;
        }
        ScenarioState previousState = CurrentState;
        CurrentState = nextState;
        debugState = nextState;
        StateChanged?.Invoke(previousState, nextState);
    }

    private void OnDisable()
    {
        if (revealObject != null) revealObject.transform.localScale = baseScale;
        if (revealRenderer != null) revealRenderer.color = baseColor;
    }

    public static void ClearAllForScene(int sceneIndex)
    {
        string prefix = "UmbralHorror_" + sceneIndex + "_";
        foreach (string id in new[] { "a", "b", "c" }) PlayerPrefs.DeleteKey(prefix + id);
    }
}
