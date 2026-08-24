using UnityEngine;

public class DoorGoal : MonoBehaviour
{
    public bool needsKey = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerController2D>() == null)
        {
            return;
        }

        if (needsKey && !GameManager.Instance.hasKey)
        {
            return;
        }

        UmbraAudio.Instance?.PlayMechanism();
        gameObject.SetActive(false);
    }
}
