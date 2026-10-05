using System.Collections;
using UnityEngine;

public class AwakeningSequence2D : MonoBehaviour
{
    public float duration = 1.8f;
    private bool played;
    private bool active;
    private PlayerController2D controller;
    private SpriteRenderer body;

    private void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        body = GetComponent<SpriteRenderer>();
        played = PlayerPrefs.GetInt("UmbralAwakeningSeen", 0) == 1;
    }

    private void Update()
    {
        if (!played && GameManager.Instance != null && GameManager.Instance.gameStarted)
        {
            played = true;
            PlayerPrefs.SetInt("UmbralAwakeningSeen", 1);
            PlayerPrefs.Save();
            StartCoroutine(Wake());
        }
    }

    private IEnumerator Wake()
    {
        active = true;
        controller.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color color = body.color;
            color.a = Mathf.Lerp(0.12f, 1f, elapsed / duration);
            body.color = color;
            yield return null;
        }
        controller.enabled = true;
        active = false;
    }

    private void OnGUI()
    {
        if (!active) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 18;
        style.fontStyle = FontStyle.Italic;
        style.normal.textColor = new Color(0.82f, 0.84f, 0.86f);
        GUI.Label(new Rect((Screen.width - 320f) * 0.5f, Screen.height * 0.72f, 320f, 40f), "RESPIRA", style);
    }
}
