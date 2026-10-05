using UnityEngine;

/// <summary>
/// Concrete Observer. It can be replaced by a HUD, logger or achievement system
/// without changing the gameplay objects that publish events.
/// </summary>
public sealed class UmbraEventJournal : MonoBehaviour
{
    public string LastEvent { get; private set; }
    public int ReceivedEventCount { get; private set; }

    private float visibleTimer;

    private void OnEnable()
    {
        UmbraGameEvents.InteractionPerformed += OnInteractionPerformed;
        UmbraGameEvents.ProgressChanged += OnProgressChanged;
        UmbraGameEvents.PlayerStateChanged += OnPlayerStateChanged;
    }

    private void OnDisable()
    {
        UmbraGameEvents.InteractionPerformed -= OnInteractionPerformed;
        UmbraGameEvents.ProgressChanged -= OnProgressChanged;
        UmbraGameEvents.PlayerStateChanged -= OnPlayerStateChanged;
    }

    private void Update()
    {
        if (visibleTimer > 0f)
        {
            visibleTimer -= Time.unscaledDeltaTime;
        }
    }

    private void OnInteractionPerformed(string description)
    {
        Record("INTERACCION: " + description);
    }

    private void OnProgressChanged(UmbraProgressSnapshot snapshot)
    {
        string keyState = snapshot.HasKey ? "LLAVE OBTENIDA" : "LLAVE PENDIENTE";
        Record("PROGRESO " + snapshot.LevelNumber + "/" + snapshot.TotalLevels + " - " + keyState);
    }

    private void OnPlayerStateChanged(UmbraPlayerStateSnapshot snapshot)
    {
        Record(snapshot.State + " - INTENTO " + snapshot.AttemptNumber + " - CAIDAS " + snapshot.DeathCount);
    }

    private void Record(string message)
    {
        LastEvent = message;
        ReceivedEventCount++;
        visibleTimer = 2.4f;
        Debug.Log("UMBRA OBSERVER: " + message);
    }

    private void OnGUI()
    {
        if (visibleTimer <= 0f || GameManager.Instance == null || !GameManager.Instance.gameStarted)
        {
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.72f, 0.9f, 1f);
        GUI.Label(new Rect((Screen.width - 620f) * 0.5f, 18f, 620f, 28f), "OBSERVER  |  " + LastEvent, style);
    }
}
