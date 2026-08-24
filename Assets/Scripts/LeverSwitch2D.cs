using UnityEngine;

public class LeverSwitch2D : MonoBehaviour
{
    public DeathTrap targetTrap;
    public SpriteRenderer leverRenderer;
    public Color activatedColor = new Color(0.85f, 0.85f, 0.8f, 1f);

    private bool playerNearby;
    private bool activated;
    private PlayerController2D nearbyPlayer;

    private void Update()
    {
        bool interacting = Input.GetKeyDown(KeyCode.E) ||
            (nearbyPlayer != null && nearbyPlayer.IsInteracting);
        if (!activated && playerNearby && interacting)
        {
            activated = true;
            Debug.Log("UMBRA LEVER ACTIVATED: " + gameObject.scene.name + " / " + gameObject.name);
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
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            playerNearby = true;
            nearbyPlayer = player;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            playerNearby = false;
            if (nearbyPlayer == player)
            {
                nearbyPlayer = null;
            }
        }
    }
}
