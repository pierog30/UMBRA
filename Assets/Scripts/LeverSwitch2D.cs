using UnityEngine;

public class LeverSwitch2D : MonoBehaviour
{
    public DeathTrap targetTrap;
    public SpriteRenderer leverRenderer;
    public Color activatedColor = new Color(0.85f, 0.85f, 0.8f, 1f);

    private bool playerNearby;
    private bool activated;

    private void Update()
    {
        if (!activated && playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            activated = true;
            if (targetTrap != null)
            {
                targetTrap.SetArmed(false);
            }

            if (leverRenderer != null)
            {
                leverRenderer.color = activatedColor;
                leverRenderer.flipX = true;
            }

            UmbraAudio.Instance?.PlayMechanism();
            UmbraGameEvents.PublishInteraction("Palanca activada");
        }
    }

    private void OnGUI()
    {
        if (!playerNearby || activated || GameManager.Instance == null || !GameManager.Instance.CanPlayerMove) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 15;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect((Screen.width - 260f) * 0.5f, Screen.height - 82f, 260f, 32f), "E — ACCIONAR PALANCA", style);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController2D>() != null)
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController2D>() != null)
        {
            playerNearby = false;
        }
    }
}
