using UnityEngine;

public class DoorGoal : MonoBehaviour
{
    public bool needsKey = true;
    private bool playerNearby;

    private void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController2D player = collision.collider.GetComponent<PlayerController2D>();
        if (player == null)
        {
            return;
        }
        playerNearby = true;

        GameManager manager = GameManager.Instance;
        if (manager == null || (needsKey && !manager.hasKey))
        {
            return;
        }

        if (!Input.GetKey(KeyCode.E)) return;

        UmbraAudio.Instance?.PlayMechanism();
        UmbraGameEvents.PublishInteraction("Puerta abierta");
        gameObject.SetActive(false);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerController2D>() != null) playerNearby = false;
    }

    private void OnGUI()
    {
        GameManager manager = GameManager.Instance;
        if (!playerNearby || manager == null || !manager.CanPlayerMove) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 15;
        style.normal.textColor = Color.white;
        string prompt = needsKey && !manager.hasKey ? "FALTA LA LLAVE" : "E — ABRIR";
        GUI.Label(new Rect((Screen.width - 240f) * 0.5f, Screen.height - 82f, 240f, 32f), prompt, style);
    }
}
