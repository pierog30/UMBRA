using UnityEngine;

public class ClimbZone2D : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.EnterClimbZone(this);
            GameManager.Instance?.ShowHint("W / S - TREPAR", 1.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.ExitClimbZone(this);
        }
    }
}
