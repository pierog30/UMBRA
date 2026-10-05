using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController2D), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public sealed class ShadowDash2D : MonoBehaviour
{
    [Header("Impulso umbral")]
    public KeyCode dashKey = KeyCode.Q;
    public float dashSpeed = 16.5f;
    public float dashDuration = 0.16f;
    public float cooldown = 1.15f;
    public float afterimageInterval = 0.035f;
    public Color afterimageColor = new Color(0.42f, 0.18f, 0.72f, 0.72f);

    public bool IsDashing { get; private set; }
    public float CooldownNormalized => IsDashing
        ? 0f
        : Mathf.Clamp01(1f - Mathf.Max(0f, nextDashTime - Time.time) / Mathf.Max(0.01f, cooldown));
    public bool IsReady => !IsDashing && Time.time >= nextDashTime;

    private PlayerController2D controller;
    private Rigidbody2D body;
    private SpriteRenderer bodyRenderer;
    private float nextDashTime;
    private float originalGravity;
    private Coroutine activeDash;

    private void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        body = GetComponent<Rigidbody2D>();
        bodyRenderer = GetComponent<SpriteRenderer>();
        originalGravity = body.gravityScale;
    }

    private void Update()
    {
        bool canAct = GameManager.Instance == null || GameManager.Instance.CanPlayerMove;
        if (!canAct || controller.IsCrouching || controller.IsClimbing || controller.IsHidden)
        {
            return;
        }

        if (Input.GetKeyDown(dashKey) || Input.GetKeyDown(KeyCode.RightControl))
        {
            TryStartDash(0f);
        }
    }

    public bool TryStartDash(float directionOverride)
    {
        if (!IsReady || controller.IsCrouching || controller.IsClimbing || controller.IsHidden)
        {
            return false;
        }

        float direction = Mathf.Abs(directionOverride) > 0.01f
            ? Mathf.Sign(directionOverride)
            : Mathf.Abs(controller.HorizontalInput) > 0.01f
                ? Mathf.Sign(controller.HorizontalInput)
                : bodyRenderer.flipX ? -1f : 1f;

        activeDash = StartCoroutine(DashRoutine(direction));
        return true;
    }

    private IEnumerator DashRoutine(float direction)
    {
        IsDashing = true;
        controller.ExternalMovementLocked = true;
        originalGravity = body.gravityScale;
        body.gravityScale = 0f;
        body.linearVelocity = new Vector2(direction * dashSpeed, 0f);
        nextDashTime = Time.time + cooldown;
        UmbraAudio.Instance?.PlayMechanism();
        UmbraGameEvents.PublishInteraction("Impulso umbral activado");

        float elapsed = 0f;
        float afterimageTimer = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            afterimageTimer -= Time.fixedDeltaTime;
            body.linearVelocity = new Vector2(direction * dashSpeed, 0f);
            if (afterimageTimer <= 0f)
            {
                CreateAfterimage();
                afterimageTimer = afterimageInterval;
            }
            yield return new WaitForFixedUpdate();
        }

        body.linearVelocity = new Vector2(direction * dashSpeed * 0.2f, body.linearVelocity.y);
        body.gravityScale = originalGravity;
        controller.ExternalMovementLocked = false;
        IsDashing = false;
        activeDash = null;
    }

    private void CreateAfterimage()
    {
        GameObject afterimage = new GameObject("Umbra Dash Afterimage");
        afterimage.transform.position = transform.position;
        afterimage.transform.rotation = transform.rotation;
        afterimage.transform.localScale = transform.lossyScale;

        SpriteRenderer renderer = afterimage.AddComponent<SpriteRenderer>();
        renderer.sprite = bodyRenderer.sprite;
        renderer.flipX = bodyRenderer.flipX;
        renderer.sortingLayerID = bodyRenderer.sortingLayerID;
        renderer.sortingOrder = bodyRenderer.sortingOrder - 1;
        renderer.color = afterimageColor;

        DashAfterimage2D fade = afterimage.AddComponent<DashAfterimage2D>();
        fade.lifetime = 0.28f;
    }

    private void OnDisable()
    {
        if (activeDash != null)
        {
            StopCoroutine(activeDash);
            activeDash = null;
        }

        if (controller != null) controller.ExternalMovementLocked = false;
        if (body != null) body.gravityScale = originalGravity;
        IsDashing = false;
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null || !GameManager.Instance.gameStarted || GameManager.Instance.finishedGame)
        {
            return;
        }

        const float width = 230f;
        const float height = 10f;
        float x = Screen.width - width - 24f;
        // Se ubica debajo del bloque de progreso/capitulo del GameManager.
        // Así el recurso permanece legible en todas las resoluciones 16:9.
        float y = 82f;
        GUIStyle label = new GUIStyle(GUI.skin.label);
        label.alignment = TextAnchor.MiddleRight;
        label.fontSize = 14;
        label.fontStyle = FontStyle.Bold;
        label.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y - 6f, width, 22f), IsReady ? "Q  IMPULSO UMBRAL" : "RECARGANDO IMPULSO", label);

        Color previous = GUI.color;
        GUI.color = new Color(0.05f, 0.04f, 0.08f, 0.85f);
        GUI.DrawTexture(new Rect(x, y + 19f, width, height), Texture2D.whiteTexture);
        GUI.color = IsReady ? new Color(0.66f, 0.25f, 0.95f, 0.95f) : new Color(0.4f, 0.18f, 0.62f, 0.9f);
        GUI.DrawTexture(new Rect(x, y + 19f, width * CooldownNormalized, height), Texture2D.whiteTexture);
        GUI.color = previous;
    }
}

public sealed class DashAfterimage2D : MonoBehaviour
{
    public float lifetime = 0.28f;
    private float remaining;
    private SpriteRenderer body;

    private void Awake()
    {
        remaining = lifetime;
        body = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        remaining = lifetime;
    }

    private void Update()
    {
        remaining -= Time.deltaTime;
        if (body != null)
        {
            Color color = body.color;
            color.a = Mathf.Clamp01(remaining / Mathf.Max(0.01f, lifetime)) * 0.72f;
            body.color = color;
        }

        if (remaining <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
