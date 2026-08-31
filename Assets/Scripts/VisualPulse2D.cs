using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VisualPulse2D : MonoBehaviour
{
    public float speed = 2.2f;
    public float scaleAmount = 0.06f;
    public float minimumAlpha = 0.22f;
    public float maximumAlpha = 0.55f;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private Color baseColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = spriteRenderer.color;
    }

    private void Update()
    {
        float pulse = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        transform.localScale = baseScale * (1f + (pulse * scaleAmount));
        Color color = baseColor;
        color.a = Mathf.Lerp(minimumAlpha, maximumAlpha, pulse);
        spriteRenderer.color = color;
    }
}
