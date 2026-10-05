using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform2D : MonoBehaviour
{
    public Vector2 offset = new Vector2(3f, 0f);
    public float speed = 1.4f;

    private Rigidbody2D body;
    private Collider2D platformCollider;
    private Vector2 startPosition;
    private readonly HashSet<Rigidbody2D> riders = new HashSet<Rigidbody2D>();
    private readonly List<Rigidbody2D> detachedRiders = new List<Rigidbody2D>();

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        platformCollider = GetComponent<Collider2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        startPosition = body.position;
    }

    private void FixedUpdate()
    {
        float progress = (Mathf.Sin(Time.fixedTime * speed) + 1f) * 0.5f;
        Vector2 nextPosition = Vector2.Lerp(startPosition, startPosition + offset, progress);
        Vector2 delta = nextPosition - body.position;
        body.MovePosition(nextPosition);

        detachedRiders.Clear();
        foreach (Rigidbody2D rider in riders)
        {
            if (!IsStandingOnPlatform(rider))
            {
                detachedRiders.Add(rider);
                continue;
            }

            rider.MovePosition(rider.position + delta);
        }

        foreach (Rigidbody2D rider in detachedRiders)
        {
            riders.Remove(rider);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateRider(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateRider(collision);
    }

    private void UpdateRider(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerController2D>() == null)
        {
            return;
        }

        Rigidbody2D rider = collision.collider.attachedRigidbody;
        if (rider == null)
        {
            return;
        }

        if (IsStandingOnPlatform(rider))
        {
            riders.Add(rider);
        }
        else
        {
            riders.Remove(rider);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerController2D>() != null)
        {
            Rigidbody2D rider = collision.collider.attachedRigidbody;
            if (rider != null) riders.Remove(rider);
        }
    }

    private bool IsStandingOnPlatform(Rigidbody2D rider)
    {
        if (rider == null || platformCollider == null || !rider.simulated)
        {
            return false;
        }

        Collider2D riderCollider = rider.GetComponent<Collider2D>();
        if (riderCollider == null || !riderCollider.enabled)
        {
            return false;
        }

        Bounds platformBounds = platformCollider.bounds;
        Bounds riderBounds = riderCollider.bounds;
        float feetDistance = riderBounds.min.y - platformBounds.max.y;
        bool feetOnTop = feetDistance >= -0.12f && feetDistance <= 0.18f;
        bool horizontallySupported =
            riderBounds.max.x > platformBounds.min.x + 0.04f &&
            riderBounds.min.x < platformBounds.max.x - 0.04f;
        bool isJumpingAway = rider.linearVelocity.y > 0.35f;

        return feetOnTop && horizontallySupported && !isJumpingAway;
    }
}
