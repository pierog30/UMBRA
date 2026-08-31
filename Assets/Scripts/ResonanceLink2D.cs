using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ResonanceLink2D : MonoBehaviour
{
    public DeathTrap targetTrap;

    private SpriteRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        bool active = targetTrap != null && !targetTrap.IsArmed;
        float pulse = active ? (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f : 0f;
        lineRenderer.color = active
            ? new Color(0.35f, 1f, 0.82f, Mathf.Lerp(0.65f, 0.95f, pulse))
            : new Color(1f, 0.38f, 0.28f, 0.58f);
    }
}
