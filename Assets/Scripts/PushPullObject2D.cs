using UnityEngine;

[DefaultExecutionOrder(50)]
[RequireComponent(typeof(Rigidbody2D))]
public class PushPullObject2D : MonoBehaviour
{
    public float pullDistance = 1.45f;
    public float pushDistance = 1.15f;
    public float pushSpeed = 3.6f;
    public float pullSpeed = 3.8f;
    public float maxHorizontalSpeed = 4.2f;
    public float acceleration = 32f;
    public float braking = 14f;

    private Rigidbody2D body;
    private PlayerController2D player;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.linearDamping = Mathf.Max(body.linearDamping, 1.5f);
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController2D>();
        }

        bool isBeingControlled = false;
        if (player != null)
        {
            Vector2 difference = player.transform.position - transform.position;
            float horizontalInput = player.HorizontalInput;
            bool isCloseVertically = Mathf.Abs(difference.y) <= 1.1f;
            bool isPulling = player.IsInteracting &&
                Mathf.Abs(difference.x) <= pullDistance && isCloseVertically;
            bool isPushing = player.IsGrounded && !player.IsCrouching &&
                Mathf.Abs(horizontalInput) > 0.05f && Mathf.Abs(difference.x) > 0.05f &&
                Mathf.Abs(difference.x) <= pushDistance && isCloseVertically &&
                Mathf.Sign(horizontalInput) == -Mathf.Sign(difference.x);

            if (isPulling || isPushing)
            {
                float targetSpeed = horizontalInput * (isPulling ? pullSpeed : pushSpeed);
                float nextSpeed = Mathf.MoveTowards(
                    body.linearVelocity.x,
                    targetSpeed,
                    acceleration * Time.fixedDeltaTime);
                body.linearVelocity = new Vector2(nextSpeed, body.linearVelocity.y);
                if (isPulling)
                {
                    Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
                    if (playerBody != null &&
                        Mathf.Sign(playerBody.linearVelocity.x) == Mathf.Sign(targetSpeed) &&
                        Mathf.Abs(playerBody.linearVelocity.x) > Mathf.Abs(targetSpeed))
                    {
                        playerBody.linearVelocity = new Vector2(targetSpeed, playerBody.linearVelocity.y);
                    }
                }

                isBeingControlled = true;
            }
        }

        float horizontalSpeed = Mathf.Clamp(
            body.linearVelocity.x,
            -maxHorizontalSpeed,
            maxHorizontalSpeed);

        if (!isBeingControlled)
        {
            horizontalSpeed = Mathf.MoveTowards(
                horizontalSpeed,
                0f,
                braking * Time.fixedDeltaTime);
        }

        body.linearVelocity = new Vector2(horizontalSpeed, body.linearVelocity.y);
    }
}
