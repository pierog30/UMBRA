using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform2D : MonoBehaviour
{
    public Vector2 offset = new Vector2(3f, 0f);
    public float speed = 1.4f;

    private Rigidbody2D body;
    private Vector2 startPosition;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        startPosition = body.position;
    }

    private void FixedUpdate()
    {
        float progress = (Mathf.Sin(Time.fixedTime * speed) + 1f) * 0.5f;
        body.MovePosition(Vector2.Lerp(startPosition, startPosition + offset, progress));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerController2D>() != null)
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerController2D>() != null && collision.transform.parent == transform)
        {
            collision.transform.SetParent(null);
        }
    }
}
