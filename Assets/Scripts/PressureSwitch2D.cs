using UnityEngine;

public class PressureSwitch2D : MonoBehaviour
{
    public DeathTrap targetTrap;
    public SpriteRenderer indicator;

    private int boxesOnSwitch;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PushPullObject2D>() == null)
        {
            return;
        }

        boxesOnSwitch++;
        UpdateMechanism();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PushPullObject2D>() == null)
        {
            return;
        }

        boxesOnSwitch = Mathf.Max(0, boxesOnSwitch - 1);
        UpdateMechanism();
    }

    private void UpdateMechanism()
    {
        bool activated = boxesOnSwitch > 0;
        if (targetTrap != null)
        {
            targetTrap.SetArmed(!activated);
        }

        if (indicator != null)
        {
            indicator.color = activated
                ? new Color(0.55f, 1f, 0.78f, 1f)
                : new Color(1f, 0.72f, 0.58f, 1f);
        }

        if (activated)
        {
            GameManager.Instance?.ShowHint("RESONANCIA ACTIVADA", 1.7f);
        }

        UmbraAudio.Instance?.PlayMechanism();
    }
}
