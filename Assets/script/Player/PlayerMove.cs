using UnityEngine;
using TMPro;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class PlayerMove : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WebGLMicrophone_Start();

    [DllImport("__Internal")]
    private static extern float WebGLMicrophone_GetVolume();

    [DllImport("__Internal")]
    private static extern void WebGLMicrophone_Stop();
#endif

    public Rigidbody2D rb;
    private Animator anim;
    public PlayerPlatformAttachment platformAttachment;
    public PlayerJump playerJump;
    public SerialInput serial;

    [Header("Movement")]
    public float Movespeed = 6f;
    [Tooltip("Allow keyboard controls when serial input is missing, or for quick editor testing")]
    public bool enableKeyboardFallback = true;
    [Tooltip("Enable to use serial speedValue (microphone) instead of Movespeed")]
    public bool useVoiceSpeed = false;
    [Tooltip("Multiplier applied to serial speedValue when voice speed is enabled")]
    public float voiceSpeedMultiplier = 1f;

    [Header("Voice Speed")]
    [Tooltip("Use microphone volume to control speed when voice speed mode is enabled")]
    public bool useMicrophoneVoiceSpeed = true;
    [Tooltip("Optional microphone device name. Leave empty to use the default device")]
    public string microphoneDeviceName = "";
    [Tooltip("Small volumes below this value are ignored")]
    public float voiceNoiseFloor = 0.02f;
    [Tooltip("How strongly microphone volume affects movement speed")]
    public float voiceSensitivity = 30f;
    [Tooltip("Minimum speed while voice speed mode is active")]
    public float minVoiceMoveSpeed = 2f;
    [Tooltip("Maximum speed while voice speed mode is active")]
    public float maxVoiceMoveSpeed = 12f;
    [Tooltip("How quickly speed reacts to volume changes")]
    public float voiceSpeedSmoothing = 10f;

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
    private AudioClip microphoneClip;
    private string activeMicrophoneDevice;
    private float smoothedVoiceSpeed;
    private const int VoiceSampleWindow = 128;

    private void Start()
    {
        anim = GetComponent<Animator>();
        useVoiceSpeed = GameModeConfig.UseVoiceSpeed;

        if (platformAttachment == null)
        {
            platformAttachment = GetComponent<PlayerPlatformAttachment>();
        }

        if (playerJump == null)
        {
            playerJump = GetComponent<PlayerJump>();
        }

        StartMicrophoneIfNeeded();
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
        if (useVoiceSpeed && useMicrophoneVoiceSpeed && microphoneClip != null)
        {
            float targetSpeed = ResolveMicrophoneSpeed();
            smoothedVoiceSpeed = Mathf.Lerp(
                smoothedVoiceSpeed,
                targetSpeed,
                1f - Mathf.Exp(-voiceSpeedSmoothing * Time.fixedDeltaTime));
            return smoothedVoiceSpeed;
        }

        if (useVoiceSpeed && serial != null)
        {
            return Mathf.Max(0f, serial.speedValue * voiceSpeedMultiplier);
        }

        return Movespeed;
    }

    private void ReadInputFlags()
    {
        bool serialRun = false;
        bool serialJump = false;
        bool hasSerial = serial != null;

        if (hasSerial)
        {
            serialRun = serial.moveValue == 0;
            serialJump = serial.jumpValue == 0;
        }

        bool keyboardRun = false;
        bool keyboardJump = false;
        if (enableKeyboardFallback)
        {
            keyboardRun = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            keyboardJump = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }

        IsRUN = serialRun || keyboardRun;
        isjump = serialJump || keyboardJump;
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

    private void StartMicrophoneIfNeeded()
    {
        smoothedVoiceSpeed = minVoiceMoveSpeed;

        if (!useVoiceSpeed || !useMicrophoneVoiceSpeed)
        {
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLMicrophone_Start();
        microphoneClip = AudioClip.Create("WebGLMicrophone", VoiceSampleWindow, 1, AudioSettings.outputSampleRate, false);
#else
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("Voice speed is enabled, but no microphone device was found.");
            return;
        }

        activeMicrophoneDevice = string.IsNullOrWhiteSpace(microphoneDeviceName)
            ? Microphone.devices[0]
            : microphoneDeviceName;

        microphoneClip = Microphone.Start(activeMicrophoneDevice, true, 1, AudioSettings.outputSampleRate);
#endif
    }

    private float ResolveMicrophoneSpeed()
    {
        float volume = GetMicrophoneVolume();
        float usableVolume = Mathf.Max(0f, volume - voiceNoiseFloor);
        float targetSpeed = minVoiceMoveSpeed + usableVolume * voiceSensitivity;
        return Mathf.Clamp(targetSpeed, minVoiceMoveSpeed, maxVoiceMoveSpeed);
    }

    private float GetMicrophoneVolume()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return WebGLMicrophone_GetVolume();
#else
        int position = Microphone.GetPosition(activeMicrophoneDevice);
        if (position <= 0)
        {
            return 0f;
        }

        float[] samples = new float[VoiceSampleWindow];
        int startPosition = position - VoiceSampleWindow;
        if (startPosition < 0)
        {
            startPosition += microphoneClip.samples;
        }

        microphoneClip.GetData(samples, startPosition);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        return Mathf.Sqrt(sum / samples.Length);
#endif
    }

    private void OnDestroy()
    {
        if (microphoneClip == null)
        {
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLMicrophone_Stop();
#else
        if (!string.IsNullOrEmpty(activeMicrophoneDevice) && Microphone.IsRecording(activeMicrophoneDevice))
        {
            Microphone.End(activeMicrophoneDevice);
        }
#endif
    }
}
