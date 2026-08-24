using UnityEngine;

public class CollectKey : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.hasKey)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController2D>() == null)
        {
            return;
        }

        GameManager.Instance.CollectKey();
        gameObject.SetActive(false);
    }
}
