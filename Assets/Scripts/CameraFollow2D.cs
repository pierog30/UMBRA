using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 9f;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);
    public bool useBounds = true;
    public Vector2 minBounds = new Vector2(-8f, -1f);
    public Vector2 maxBounds = new Vector2(22f, 4f);
    private float trauma;

    private void Start()
    {
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }
        if (trauma > 0f)
        {
            desiredPosition += (Vector3)(Random.insideUnitCircle * trauma * 0.22f);
            trauma = Mathf.MoveTowards(trauma, 0f, Time.unscaledDeltaTime * 1.8f);
        }
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 position = target.position + offset;
            if (useBounds)
            {
                position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
                position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
            }
            transform.position = position;
        }
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }
}
