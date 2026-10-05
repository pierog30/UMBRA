using UnityEngine;

public class DeathTrap : MonoBehaviour
{
    [Header("Strategy pattern")]
    public TrapResponseMode responseMode = TrapResponseMode.Lethal;

    private Collider2D hitbox;
    private SpriteRenderer body;
    private SimpleMover2D mover;
    private Vector3 originalScale;
    private ITrapResponseStrategy responseStrategy;

    public string ActiveStrategyName => responseStrategy != null ? responseStrategy.Name : "NotConfigured";

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        body = GetComponent<SpriteRenderer>();
        mover = GetComponent<SimpleMover2D>();
        originalScale = transform.localScale;
        ConfigureStrategy();
    }

    public void SetResponseMode(TrapResponseMode mode)
    {
        responseMode = mode;
        ConfigureStrategy();
    }

    private void ConfigureStrategy()
    {
        responseStrategy = responseMode == TrapResponseMode.WarningOnly
            ? new WarningTrapResponseStrategy()
            : new LethalTrapResponseStrategy();
    }

    public void SetArmed(bool armed)
    {
        enabled = armed;
        if (hitbox != null) hitbox.enabled = armed;
        if (mover != null) mover.enabled = armed;
        transform.localScale = armed
            ? originalScale
            : new Vector3(originalScale.x, originalScale.y * 0.28f, originalScale.z);
        if (body != null)
        {
            Color color = body.color;
            color.a = armed ? 1f : 0.28f;
            body.color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRespawn player = other.GetComponent<PlayerRespawn>();
        if (player != null)
        {
            responseStrategy?.Execute(player, this);
        }
    }
}
