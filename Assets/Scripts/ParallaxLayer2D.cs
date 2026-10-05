using UnityEngine;

public sealed class ParallaxLayer2D : MonoBehaviour
{
    [Header("Profundidad visual")]
    [Range(0f, 1f)] public float horizontalFactor = 0.12f;
    [Range(0f, 1f)] public float verticalFactor = 0.04f;
    public Vector2 ambientDrift = Vector2.zero;

    private Transform cameraTransform;
    private Vector3 initialPosition;
    private Vector3 initialCameraPosition;

    private void Start()
    {
        Camera activeCamera = Camera.main;
        if (activeCamera == null)
        {
            enabled = false;
            return;
        }

        cameraTransform = activeCamera.transform;
        initialPosition = transform.position;
        initialCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            return;
        }

        Vector3 cameraDelta = cameraTransform.position - initialCameraPosition;
        Vector2 drift = ambientDrift * Time.time;
        transform.position = new Vector3(
            initialPosition.x + cameraDelta.x * horizontalFactor + drift.x,
            initialPosition.y + cameraDelta.y * verticalFactor + drift.y,
            initialPosition.z);
    }
}
