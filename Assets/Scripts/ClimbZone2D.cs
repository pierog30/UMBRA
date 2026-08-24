using UnityEngine;

public class ClimbZone2D : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.EnterClimbZone(this);
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
