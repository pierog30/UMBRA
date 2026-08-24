using UnityEngine;

public class PlayerSpriteAnimator : MonoBehaviour
{
    public PlayerController2D controller;
    public SpriteRenderer body;
    public Sprite[] idleFrames;
    public Sprite[] runFrames;
    public Sprite[] jumpFrames;
    public Sprite[] crouchFrames;
    public float framesPerSecond = 12f;

    private Rigidbody2D rb;
    private Sprite[] currentFrames;
    private float frameTimer;
    private int frameIndex;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Sprite[] wantedFrames = ChooseFrames();
        if (wantedFrames == null || wantedFrames.Length == 0)
        {
            return;
        }

        if (wantedFrames != currentFrames)
        {
            currentFrames = wantedFrames;
            frameIndex = 0;
            frameTimer = 0f;
        }

        if (!controller.IsGrounded && currentFrames == jumpFrames)
        {
            frameIndex = rb.linearVelocity.y >= 0f ? 0 : Mathf.Min(1, jumpFrames.Length - 1);
        }
        else
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / framesPerSecond)
            {
                frameTimer = 0f;
                frameIndex = (frameIndex + 1) % currentFrames.Length;
            }
        }

        body.sprite = currentFrames[frameIndex];

        if (Mathf.Abs(controller.HorizontalInput) > 0.05f)
        {
            body.flipX = controller.HorizontalInput < 0f;
        }
    }

    private Sprite[] ChooseFrames()
    {
        if (GameManager.Instance != null && !GameManager.Instance.gameStarted)
        {
            return idleFrames;
        }

        if (controller.IsCrouching)
        {
            return crouchFrames;
        }

        if (controller.IsClimbing)
        {
            return runFrames;
        }

        if (!controller.IsGrounded)
        {
            return jumpFrames;
        }

        return Mathf.Abs(controller.HorizontalInput) > 0.05f ? runFrames : idleFrames;
    }
}
