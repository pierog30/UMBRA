using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HorrorEvent2D : MonoBehaviour
{
    public enum HorrorStyle { BackgroundRise, ForegroundRush, EnvironmentShift, SpawnThreat, CameraPulse }
    public string eventId = "event";
    public HorrorStyle style;
    public GameObject revealObject;
    public float duration = 1.4f;
    public bool persistForRun = true;
    private bool fired;

    private string SaveKey => "UmbralHorror_" + SceneManager.GetActiveScene().buildIndex + "_" + eventId;

    private void Awake()
    {
        fired = persistForRun && PlayerPrefs.GetInt(SaveKey, 0) == 1;
        if (revealObject != null)
        {
            bool shouldRemain = fired && (style == HorrorStyle.EnvironmentShift || style == HorrorStyle.SpawnThreat);
            revealObject.SetActive(shouldRemain);
        }
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
                revealObject.transform.position = Vector3.Lerp(start + offset, start, elapsed / duration);
                yield return null;
            }
            if (style != HorrorStyle.EnvironmentShift && style != HorrorStyle.SpawnThreat)
            {
                revealObject.SetActive(false);
            }
        }
    }

    public static void ClearAllForScene(int sceneIndex)
    {
        string prefix = "UmbralHorror_" + sceneIndex + "_";
        foreach (string id in new[] { "a", "b", "c" }) PlayerPrefs.DeleteKey(prefix + id);
    }
}
