using UnityEngine;
using TMPro;

public class PlayerMove : MonoBehaviour
{
    public Rigidbody2D rb;
    private Animator anim;
    public PlayerPlatformAttachment platformAttachment;
    public PlayerJump playerJump;
    public SerialInput serial;

    [Header("Movement")]
    public float Movespeed = 6f;
    [Tooltip("Enable to use serial speedValue (microphone) instead of Movespeed")]
    public bool useVoiceSpeed = false;
    [Tooltip("Multiplier applied to serial speedValue when voice speed is enabled")]
    public float voiceSpeedMultiplier = 1f;

    [Tooltip("Horizontal acceleration applied when starting to run")]
    public float acceleration = 40f;
    [Tooltip("Horizontal deceleration applied when stopping")]
    public float deceleration = 60f;

    [Header("UI")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private string speedTextPrefix = "Speed";

    [SerializeField] public LayerMask GroundLayer;
    public bool isGrounded;
    public bool IsRUN;
    public bool isjump;
    public bool isFalling;

    private bool wasRunning = false;
    private bool wasGrounded = false;

    private void Start()
    {
        anim = GetComponent<Animator>();

        if (platformAttachment == null)
        {
            platformAttachment = GetComponent<PlayerPlatformAttachment>();
        }

        if (playerJump == null)
        {
            playerJump = GetComponent<PlayerJump>();
        }
    }

    private void Update()
    {
        ReadInputFlags();
        HandleRunStartDetach();
        RefreshGroundedState();

        if (playerJump != null)
        {
            playerJump.HandleJump(isjump, isGrounded);
        }

        UpdateFallingState();
        UpdateAnimatorState();
        UpdateSpeedText();
        CacheFrameState();
    }

    private void FixedUpdate()
    {
        float activeMoveSpeed = GetActiveMoveSpeed();
        float targetX = IsRUN ? activeMoveSpeed : 0f;
        float currentX = rb.velocity.x;

        float maxDelta = (Mathf.Abs(targetX) > Mathf.Abs(currentX))
            ? acceleration * Time.fixedDeltaTime
            : deceleration * Time.fixedDeltaTime;

        float newX = Mathf.MoveTowards(currentX, targetX, maxDelta);
        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

    private float GetActiveMoveSpeed()
    {
        if (useVoiceSpeed && serial != null)
        {
            return Mathf.Max(0f, serial.speedValue * voiceSpeedMultiplier);
        }

        return Movespeed;
    }

    private void ReadInputFlags()
    {
        if (serial == null)
        {
            IsRUN = false;
            isjump = false;
            return;
        }

        IsRUN = serial.moveValue == 0;
        isjump = serial.jumpValue == 0;
    }

    private void HandleRunStartDetach()
    {
        if (IsRUN && !wasRunning && platformAttachment != null)
        {
            platformAttachment.DetachFromPlatform();
        }
    }

    private void RefreshGroundedState()
    {
        bool groundedNow = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, GroundLayer);

        if (!wasGrounded && groundedNow)
        {
            if (platformAttachment != null)
            {
                platformAttachment.TryAttachToPlatform();
            }

            if (playerJump != null)
            {
                playerJump.OnLanded();
            }
        }

        isGrounded = groundedNow;
    }

    private void UpdateFallingState()
    {
        isFalling = rb.velocity.y < 0f;

        if (playerJump != null)
        {
            playerJump.UpdateFallingState(isFalling);
        }
    }

    private void UpdateAnimatorState()
    {
        anim.SetBool("IsRUN", IsRUN);
        anim.SetBool("IsJump", isjump);
        anim.SetBool("IsFalling", isFalling);
        anim.SetBool("IsGrounded", isGrounded);

        if (playerJump != null)
        {
            anim.SetBool("IsDoublejump", playerJump.IsDoubleJumping);
        }
        else
        {
            anim.SetBool("IsDoublejump", false);
        }
    }

    private void CacheFrameState()
    {
        wasRunning = IsRUN;
        wasGrounded = isGrounded;
    }

    private void UpdateSpeedText()
    {
        if (speedText == null)
        {
            return;
        }

        float currentSpeed = Mathf.Abs(rb.velocity.x);
        speedText.text = $"{speedTextPrefix}: {currentSpeed:0.0}";
    }
}
