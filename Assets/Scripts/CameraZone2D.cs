using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone2D : MonoBehaviour
{
    [Header("Identidad")]
    public string zoneName = "Zona de camara";

    [Header("Transicion")]
    [Min(0.1f)] public float transitionSpeed = 3.5f;
    public bool overrideOffset = true;
    public Vector3 cameraOffset = new Vector3(1.2f, 1.5f, -10f);
    public bool overrideZoom = true;
    [Min(1f)] public float orthographicSize = 4.7f;

    [Header("Limites opcionales")]
    public bool overrideBounds;
    public Vector2 minBounds = new Vector2(-8f, -1f);
    public Vector2 maxBounds = new Vector2(22f, 4f);

    private CameraFollow2D cameraFollow;

    private void Awake()
    {
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        ResolveCamera();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController2D>() == null) return;
        ResolveCamera();
        cameraFollow?.EnterZone(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController2D>() == null) return;
        ResolveCamera();
        cameraFollow?.ExitZone(this);
    }

    private void ResolveCamera()
    {
        if (cameraFollow == null && Camera.main != null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow2D>();
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;
        Gizmos.color = new Color(0.55f, 0.2f, 0.85f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.offset, box.size);
        Gizmos.color = new Color(0.75f, 0.45f, 1f, 0.95f);
        Gizmos.DrawWireCube(box.offset, box.size);
    }
}
