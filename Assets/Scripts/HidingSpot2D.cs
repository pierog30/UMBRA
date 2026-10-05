using UnityEngine;

public class HidingSpot2D : MonoBehaviour
{
    private PlayerController2D nearbyPlayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        nearbyPlayer = other.GetComponent<PlayerController2D>();
        nearbyPlayer?.EnterHidingSpot(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        player?.ExitHidingSpot(this);
        if (player == nearbyPlayer) nearbyPlayer = null;
    }

    private void OnGUI()
    {
        if (nearbyPlayer == null || GameManager.Instance == null || !GameManager.Instance.CanPlayerMove) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 15;
        style.normal.textColor = Color.white;
        string action = nearbyPlayer.IsHidden ? "E — SALIR" : "E — ESCONDERSE";
        GUI.Label(new Rect((Screen.width - 240f) * 0.5f, Screen.height - 80f, 240f, 35f), action, style);
    }
}
