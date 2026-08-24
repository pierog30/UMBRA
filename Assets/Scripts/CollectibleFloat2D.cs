using UnityEngine;

public class CollectibleFloat2D : MonoBehaviour
{
    public float bobHeight = 0.16f;
    public float bobSpeed = 2.2f;
    public float tiltAmount = 7f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float wave = Mathf.Sin(Time.time * bobSpeed);
        transform.position = startPosition + Vector3.up * wave * bobHeight;
        transform.rotation = Quaternion.Euler(0f, 0f, wave * tiltAmount);
    }
}
