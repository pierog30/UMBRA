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
                ? new Color(0.88f, 0.88f, 0.82f, 1f)
                : new Color(0.24f, 0.24f, 0.26f, 1f);
        }

        UmbraAudio.Instance?.PlayMechanism();
    }
}
