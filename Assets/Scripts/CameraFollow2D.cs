using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Seguimiento")]
    public Transform target;
    public float smoothSpeed = 9f;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);
    public float lookAheadDistance = 1.25f;
    public float lookAheadSmooth = 5f;

    [Header("Limites")]
    public bool useBounds = true;
    public Vector2 minBounds = new Vector2(-8f, -1f);
    public Vector2 maxBounds = new Vector2(22f, 4f);

    [Header("Zoom y transiciones")]
    public float defaultOrthographicSize = 5f;
    public float defaultTransitionSpeed = 3.5f;

    public CameraZone2D ActiveZone { get; private set; }
    public string ActiveProfileName => ActiveZone == null ? "Seguimiento general" : ActiveZone.zoneName;

    private Camera controlledCamera;
    private Rigidbody2D targetBody;
    private Vector3 currentLookAhead;
    private float trauma;

    private void Awake()
    {
        controlledCamera = GetComponent<Camera>();
        if (controlledCamera != null && controlledCamera.orthographic)
        {
            defaultOrthographicSize = controlledCamera.orthographicSize;
        }
    }

    private void Start()
    {
        CacheTargetBody();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        CacheTargetBody();
        float deltaTime = Time.deltaTime;
        float horizontalVelocity = targetBody != null ? targetBody.linearVelocity.x : 0f;
        Vector3 targetLookAhead = Vector3.right * Mathf.Clamp(
            horizontalVelocity / 5f,
            -1f,
            1f) * lookAheadDistance;
        currentLookAhead = Vector3.Lerp(
            currentLookAhead,
            targetLookAhead,
            1f - Mathf.Exp(-lookAheadSmooth * deltaTime));

        Vector3 activeOffset = ActiveZone != null && ActiveZone.overrideOffset
            ? ActiveZone.cameraOffset
            : offset;
        Vector3 desiredPosition = target.position + activeOffset + currentLookAhead;
        ApplyBounds(ref desiredPosition);

        if (trauma > 0f)
        {
            desiredPosition += (Vector3)(Random.insideUnitCircle * trauma * 0.22f);
            trauma = Mathf.MoveTowards(trauma, 0f, Time.unscaledDeltaTime * 1.8f);
        }

        float transitionSpeed = ActiveZone != null
            ? ActiveZone.transitionSpeed
            : defaultTransitionSpeed;
        float followSpeed = Mathf.Max(0.01f, Mathf.Min(smoothSpeed, transitionSpeed));
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            1f - Mathf.Exp(-followSpeed * deltaTime));

        if (controlledCamera != null && controlledCamera.orthographic)
        {
            float targetSize = ActiveZone != null && ActiveZone.overrideZoom
                ? ActiveZone.orthographicSize
                : defaultOrthographicSize;
            controlledCamera.orthographicSize = Mathf.Lerp(
                controlledCamera.orthographicSize,
                targetSize,
                1f - Mathf.Exp(-transitionSpeed * deltaTime));
        }
    }

    public void SnapToTarget()
    {
        if (target != null)
        {
            CacheTargetBody();
            Vector3 activeOffset = ActiveZone != null && ActiveZone.overrideOffset
                ? ActiveZone.cameraOffset
                : offset;
            Vector3 position = target.position + activeOffset;
            ApplyBounds(ref position);
            transform.position = position;

            if (controlledCamera != null && controlledCamera.orthographic)
            {
                controlledCamera.orthographicSize = ActiveZone != null && ActiveZone.overrideZoom
                    ? ActiveZone.orthographicSize
                    : defaultOrthographicSize;
            }
        }
    }

    public void EnterZone(CameraZone2D zone)
    {
        if (zone == null || ActiveZone == zone) return;
        ActiveZone = zone;
        UmbraGameEvents.PublishInteraction("Camara entra en zona: " + zone.zoneName);
    }

    public void ExitZone(CameraZone2D zone)
    {
        if (ActiveZone != zone) return;
        ActiveZone = null;
        UmbraGameEvents.PublishInteraction("Camara vuelve al seguimiento general");
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    private void CacheTargetBody()
    {
        if (target != null && (targetBody == null || targetBody.transform != target))
        {
            targetBody = target.GetComponent<Rigidbody2D>();
        }
    }

    private void ApplyBounds(ref Vector3 position)
    {
        bool boundsEnabled = ActiveZone != null && ActiveZone.overrideBounds
            ? true
            : useBounds;
        if (!boundsEnabled) return;

        Vector2 activeMin = ActiveZone != null && ActiveZone.overrideBounds
            ? ActiveZone.minBounds
            : minBounds;
        Vector2 activeMax = ActiveZone != null && ActiveZone.overrideBounds
            ? ActiveZone.maxBounds
            : maxBounds;
        position.x = Mathf.Clamp(position.x, activeMin.x, activeMax.x);
        position.y = Mathf.Clamp(position.y, activeMin.y, activeMax.y);
    }
}
