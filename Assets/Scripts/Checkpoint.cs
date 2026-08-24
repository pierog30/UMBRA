using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform respawnSpot;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRespawn player = other.GetComponent<PlayerRespawn>();
        if (player == null)
        {
            return;
        }

        Vector3 point = respawnSpot != null ? respawnSpot.position : transform.position;
        player.SetCheckpoint(point);
    }
}
