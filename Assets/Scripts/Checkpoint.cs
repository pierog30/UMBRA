using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform respawnSpot;
    private bool activated;
    private float messageTimer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRespawn player = other.GetComponent<PlayerRespawn>();
        if (player == null)
        {
            return;
        }

        Vector3 point = respawnSpot != null ? respawnSpot.position : transform.position;
        player.SetCheckpoint(point);
        if (!activated)
        {
            activated = true;
            messageTimer = 1.8f;
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = new Color(0.72f, 0.92f, 1f, 1f);
            UmbraAudio.Instance?.PlayPickup();
            UmbraGameEvents.PublishInteraction("Checkpoint registrado");
        }
    }

    private void Update()
    {
        if (messageTimer > 0f) messageTimer -= Time.unscaledDeltaTime;
    }

    private void OnGUI()
    {
        if (messageTimer <= 0f) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 15;
        style.normal.textColor = new Color(0.75f, 0.9f, 1f, Mathf.Clamp01(messageTimer));
        GUI.Label(new Rect((Screen.width - 300f) * 0.5f, Screen.height - 84f, 300f, 32f), "PUNTO DE CONTROL", style);
    }
}
