using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public Rigidbody2D rb;
    public PlayerPlatformAttachment platformAttachment;

    public float jumpforce;

    [Header("Jumping")]
    [Tooltip("Maximum number of jumps allowed (1 = single jump, 2 = double jump)")]
    public int maxJumps = 2;
    [Tooltip("Enable or disable double-jump behavior")]
    public bool allowDoubleJump = true;

    private int jumpsPerformed = 0;
    private bool prevJumpInput = false;
    private bool isDoubleJumping = false;

    public bool IsDoubleJumping => isDoubleJumping;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (platformAttachment == null)
        {
            platformAttachment = GetComponent<PlayerPlatformAttachment>();
        }
    }

    public void HandleJump(bool jumpInput, bool isGrounded)
    {
        if (!jumpInput || prevJumpInput)
        {
            prevJumpInput = jumpInput;
            return;
        }

        if (isGrounded)
        {
            PerformGroundJump();
        }
        else if (allowDoubleJump && jumpsPerformed < maxJumps)
        {
            PerformDoubleJump();
        }

        prevJumpInput = jumpInput;
    }

    public void OnLanded()
    {
        jumpsPerformed = 0;
        prevJumpInput = false;
        isDoubleJumping = false;
    }

    public void UpdateFallingState(bool isFalling)
    {
        if (isFalling)
        {
            isDoubleJumping = false;
        }
    }

    private void PerformGroundJump()
    {
        if (platformAttachment != null)
        {
            platformAttachment.DetachFromPlatform();
        }

        rb.velocity = new Vector2(rb.velocity.x, jumpforce);
        jumpsPerformed = 1;
        isDoubleJumping = false;
    }

    private void PerformDoubleJump()
    {
        if (platformAttachment != null)
        {
            platformAttachment.DetachFromPlatform();
        }

        rb.velocity = new Vector2(rb.velocity.x, jumpforce);
        jumpsPerformed++;
        isDoubleJumping = true;
    }
}
