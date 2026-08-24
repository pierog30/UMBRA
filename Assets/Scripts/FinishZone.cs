using UnityEngine;

public class FinishZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController2D>() != null)
        {
            GameManager.Instance.CompleteLevel();
        }
    }
}
