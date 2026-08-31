using UnityEngine;

public class DoorGoal : MonoBehaviour
{
    public bool needsKey = true;
    public SpriteRenderer barrierRenderer;

    private bool showedUnlockedHint;

    private void Update()
    {
        bool unlocked = GameManager.Instance != null && GameManager.Instance.hasKey;
        if (barrierRenderer != null)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * 3.5f) + 1f) * 0.5f;
            barrierRenderer.color = unlocked
                ? new Color(0.35f, 1f, 0.82f, Mathf.Lerp(0.32f, 0.58f, pulse))
                : new Color(1f, 0.36f, 0.25f, Mathf.Lerp(0.42f, 0.66f, pulse));
        }

        if (unlocked && !showedUnlockedHint)
        {
            showedUnlockedHint = true;
            GameManager.Instance?.ShowHint("EL UMBRAL RESPONDE AL ECO", 2f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerController2D>() == null)
        {
            return;
        }

        if (needsKey && !GameManager.Instance.hasKey)
        {
            GameManager.Instance.ShowHint("NECESITAS EL FRAGMENTO DE ECO", 2f);
            UmbraAudio.Instance?.PlayMechanism();
            return;
        }

        UmbraAudio.Instance?.PlayMechanism();
        gameObject.SetActive(false);
    }
}
