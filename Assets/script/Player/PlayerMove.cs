using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D rb;
    private Animator anim;
    public PlayerPlatformAttachment platformAttachment;
    public float Movespeed;//connect to voice sensor
    public float jumpforce;//a set number for jump height
    public SerialInput serial;
    [Tooltip("Horizontal acceleration applied when starting to run")]
    public float acceleration = 40f;
    [Tooltip("Horizontal deceleration applied when stopping")]
    public float deceleration = 60f;
    [SerializeField] public LayerMask GroundLayer; // 地面层
    public bool isGrounded; // 地面检测状态
    public bool IsRUN;
    public bool isjump;
    public bool isFalling; // 掉落状态
    private bool wasRunning = false;
    private bool wasGrounded = false;
    [Header("Jumping")]
    [Tooltip("Maximum number of jumps allowed (1 = single jump, 2 = double jump)")]
    public int maxJumps = 2;
    private int jumpsPerformed = 0;
    private bool prevJumpInput = false;
    [Tooltip("Enable or disable double-jump behavior")]
    public bool allowDoubleJump = true;
    private bool isDoubleJumping = false;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        if (platformAttachment == null)
        {
            platformAttachment = GetComponent<PlayerPlatformAttachment>();
        }
    }   
    // Update is called once per frame
    void Update()
    {
        // input flags (preserve existing serial semantics)
        bool inputRun = serial.moveValue == 0;
        bool inputJump = serial.jumpValue == 0;

        IsRUN = inputRun;
        isjump = inputJump;

        // If player starts running (from stop), detach from platform to avoid stickiness
        if (inputRun && !wasRunning)
            if (platformAttachment != null)
                platformAttachment.DetachFromPlatform();

        // Jump input edge detection (allow double jump)
        if (inputJump && !prevJumpInput)
        {
            // pressing jump now
            if (isGrounded)
            {
                if (platformAttachment != null)
                    platformAttachment.DetachFromPlatform();
                rb.velocity = new Vector2(rb.velocity.x, jumpforce);
                jumpsPerformed = 1;
            }
            else if (allowDoubleJump && jumpsPerformed < maxJumps)
            {
                // double-jump in air
                if (platformAttachment != null)
                    platformAttachment.DetachFromPlatform();
                rb.velocity = new Vector2(rb.velocity.x, jumpforce);
                jumpsPerformed++;
                // trigger double-jump animation state (only for the upward double-jump)
                isDoubleJumping = true;
            }
        }
        prevJumpInput = inputJump;

        // Fall检测 - 当向下速度为负时
        isFalling = rb.velocity.y < 0;

        // when starting to fall, end double-jump upward animation
        if (isFalling)
            isDoubleJumping = false;

        anim.SetBool("IsRUN", IsRUN);
        anim.SetBool("IsJump", isjump);
        anim.SetBool("IsFalling", isFalling);
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, GroundLayer);
        // detect landing event and try to reattach to platform beneath
        if (!wasGrounded && isGrounded)
        {
            if (platformAttachment != null)
                platformAttachment.TryAttachToPlatform();
            // reset jump count on landing
            jumpsPerformed = 0;
            prevJumpInput = false;
        }
        anim.SetBool("IsGrounded", isGrounded);
            anim.SetBool("IsDoublejump", isDoubleJumping);

        // update previous-state trackers
        wasRunning = inputRun;
        wasGrounded = isGrounded;
    }

    // Physics update: smooth horizontal acceleration/deceleration
    void FixedUpdate()
    {
        // Determine desired horizontal speed from input (preserve original mapping)
        float targetX = (serial.moveValue == 0) ? Movespeed : 0f;

        float currentX = rb.velocity.x;

        float maxDelta = (Mathf.Abs(targetX) > Mathf.Abs(currentX)) ? acceleration * Time.fixedDeltaTime : deceleration * Time.fixedDeltaTime;
        float newX = Mathf.MoveTowards(currentX, targetX, maxDelta);

        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

}
