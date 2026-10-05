using UnityEngine;

public class NarrativeEcho2D : MonoBehaviour
{
    public string message;
    public float displaySeconds = 3.2f;
    private float timer;
    private bool fired;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (fired || other.GetComponent<PlayerController2D>() == null) return;
        fired = true;
        timer = displaySeconds;
    }

    private void Update()
    {
        if (timer > 0f) timer -= Time.unscaledDeltaTime;
    }

    private void OnGUI()
    {
        if (timer <= 0f) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 17;
        style.fontStyle = FontStyle.Italic;
        style.normal.textColor = new Color(0.84f, 0.84f, 0.82f, Mathf.Clamp01(timer));
        GUI.Label(new Rect((Screen.width - 720f) * 0.5f, Screen.height - 120f, 720f, 45f), message, style);
    }
}
