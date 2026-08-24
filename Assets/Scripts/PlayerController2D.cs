using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6.5f;
    public float jumpForce = 12f;
    public float climbSpeed = 4.5f;
    public float acceleration = 55f;
    public float deceleration = 70f;
    public float airControl = 0.75f;

    [Header("Salto tolerante")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Suelo")]
    public Transform groundCheck;
    public float groundRadius = 0.18f;
    public LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsClimbing { get; private set; }
    public bool IsInteracting { get; private set; }
    public float HorizontalInput { get; private set; }
    public bool HasClimbZone => activeClimbZone != null;

    private Rigidbody2D rb;
    private BoxCollider2D bodyCollider;
    private Vector2 standingSize;
    private Vector2 standingOffset;
    private float standingGravity;
    private float verticalInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private ClimbZone2D activeClimbZone;
    private float stepTimer;
    private bool automationEnabled;
    private float automationHorizontal;
    private float automationVertical;
    private bool automationInteract;
    private bool automationCrouch;
    private bool automationJumpPressed;
    private bool automationJumpReleased;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        standingSize = bodyCollider.size;
        standingOffset = bodyCollider.offset;
        standingGravity = rb.gravityScale;
    }

    private void Update()
    {
        bool canMove = GameManager.Instance == null || GameManager.Instance.CanPlayerMove;
        HorizontalInput = canMove
            ? (automationEnabled ? automationHorizontal : Input.GetAxisRaw("Horizontal"))
            : 0f;
        verticalInput = canMove
            ? (automationEnabled ? automationVertical : Input.GetAxisRaw("Vertical"))
            : 0f;
        IsInteracting = canMove &&
            (automationEnabled ? automationInteract : Input.GetKey(KeyCode.E));
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        coyoteCounter = IsGrounded ? coyoteTime : coyoteCounter - Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
        bool jumpPressed = automationEnabled
            ? automationJumpPressed
            : Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        if (canMove && jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }

        IsClimbing = canMove && activeClimbZone != null &&
            (IsClimbing || Mathf.Abs(verticalInput) > 0.05f);

        bool wantsCrouch = canMove && IsGrounded && !IsClimbing &&
            (automationEnabled
                ? automationCrouch
                : Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
        IsCrouching = wantsCrouch || (IsCrouching && IsGrounded && HasCeiling());
        UpdateCrouchCollider();

        if (canMove && jumpBufferCounter > 0f && coyoteCounter > 0f && !IsCrouching && !IsClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            UmbraAudio.Instance?.PlayJump();
        }

        bool jumpReleased = automationEnabled
            ? automationJumpReleased
            : Input.GetKeyUp(KeyCode.Space);
        if (canMove && jumpReleased && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.55f);
        }

        automationJumpPressed = false;
        automationJumpReleased = false;

        UpdateFootsteps(canMove);

        if (canMove && Input.GetKeyDown(KeyCode.R))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartFromCheckpoint();
            }
            else
            {
                GetComponent<PlayerRespawn>().Respawn();
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(HorizontalInput * moveSpeed * 0.55f, verticalInput * climbSpeed);
            return;
        }

        rb.gravityScale = standingGravity;
        float crouchMultiplier = IsCrouching ? 0.35f : 1f;
        float targetSpeed = HorizontalInput * moveSpeed * crouchMultiplier;
        float control = IsGrounded ? 1f : airControl;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float nextSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * control * Time.fixedDeltaTime);
        float verticalSpeed = rb.linearVelocity.y;
        if (!IsGrounded && IsTouchingWall() && verticalSpeed > -1.2f)
        {
            verticalSpeed = -1.2f;
        }

        rb.linearVelocity = new Vector2(nextSpeed, verticalSpeed);
    }

    private bool IsTouchingWall()
    {
        Bounds bounds = bodyCollider.bounds;
        float distance = bounds.extents.x + 0.08f;
        Vector2 center = bounds.center;
        return Physics2D.Raycast(center, Vector2.left, distance, groundLayer) ||
            Physics2D.Raycast(center, Vector2.right, distance, groundLayer);
    }

    public void EnterClimbZone(ClimbZone2D zone)
    {
        activeClimbZone = zone;
    }

    public void ExitClimbZone(ClimbZone2D zone)
    {
        if (activeClimbZone == zone)
        {
            activeClimbZone = null;
            IsClimbing = false;
            rb.gravityScale = standingGravity;
        }
    }

    public void SetAutomationInput(
        float horizontal,
        float vertical,
        bool interact,
        bool crouch,
        bool jumpPressed,
        bool jumpReleased = false)
    {
        automationEnabled = true;
        automationHorizontal = Mathf.Clamp(horizontal, -1f, 1f);
        automationVertical = Mathf.Clamp(vertical, -1f, 1f);
        automationInteract = interact;
        automationCrouch = crouch;
        automationJumpPressed |= jumpPressed;
        automationJumpReleased |= jumpReleased;
    }

    public void ClearAutomationInput()
    {
        automationEnabled = false;
        automationHorizontal = 0f;
        automationVertical = 0f;
        automationInteract = false;
        automationCrouch = false;
        automationJumpPressed = false;
        automationJumpReleased = false;
    }

    private bool HasCeiling()
    {
        float crouchHeight = standingSize.y * 0.58f;
        float extraHeight = standingSize.y - crouchHeight;
        Vector2 center = (Vector2)transform.position + new Vector2(
            standingOffset.x,
            standingOffset.y + (standingSize.y * 0.5f) - (extraHeight * 0.5f));
        Collider2D hit = Physics2D.OverlapBox(
            center,
            new Vector2(standingSize.x * 0.9f, extraHeight * 0.85f),
            0f,
            groundLayer);
        return hit != null;
    }

    private void UpdateFootsteps(bool canMove)
    {
        if (!canMove || !IsGrounded || IsCrouching || Mathf.Abs(HorizontalInput) < 0.05f)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            stepTimer = 0.32f;
            UmbraAudio.Instance?.PlayStep();
        }
    }

    private void UpdateCrouchCollider()
    {
        if (IsCrouching)
        {
            float crouchHeight = standingSize.y * 0.58f;
            bodyCollider.size = new Vector2(standingSize.x, crouchHeight);
            bodyCollider.offset = new Vector2(
                standingOffset.x,
                standingOffset.y - ((standingSize.y - crouchHeight) * 0.5f));
            return;
        }

        bodyCollider.size = standingSize;
        bodyCollider.offset = standingOffset;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
