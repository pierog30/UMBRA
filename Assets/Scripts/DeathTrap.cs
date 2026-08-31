using UnityEngine;

public class DeathTrap : MonoBehaviour
{
    public bool IsArmed { get; private set; } = true;

    private Collider2D trapCollider;
    private SpriteRenderer trapRenderer;
    private SimpleMover2D mover;
    private Color armedColor;
    private Vector3 armedScale;

    private void Awake()
    {
        trapCollider = GetComponent<Collider2D>();
        trapRenderer = GetComponent<SpriteRenderer>();
        mover = GetComponent<SimpleMover2D>();
        armedColor = trapRenderer != null ? trapRenderer.color : Color.white;
        armedScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsArmed)
        {
            return;
        }

        PlayerRespawn player = other.GetComponent<PlayerRespawn>();
        if (player != null)
        {
            player.Die();
        }
    }

    public void SetArmed(bool armed)
    {
        IsArmed = armed;
        if (trapCollider != null)
        {
            trapCollider.enabled = armed;
        }

        if (mover != null)
        {
            mover.enabled = armed;
        }

        if (trapRenderer != null)
        {
            trapRenderer.color = armed
                ? armedColor
                : new Color(0.48f, 0.95f, 0.84f, 0.72f);
        }

        if (mover == null)
        {
            transform.localScale = armed
                ? armedScale
                : new Vector3(armedScale.x, armedScale.y * 0.32f, armedScale.z);
        }
    }
}
