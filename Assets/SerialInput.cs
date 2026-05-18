using System;
using System.IO.Ports;
using UnityEngine;

public class SerialInput : MonoBehaviour
{
    [Header("Serial")]
    [SerializeField] private bool enableSerialInput = false;
    [SerializeField] private string portName = "COM3";
    [SerializeField] private int baudRate = 230400;
    [SerializeField] private int readTimeoutMs = 5;

    [Header("Input Values")]
    public int moveValue = 1;
    public int jumpValue = 1;
    public int speedValue = 3;
    public int monsterSpeed = 7;
    public bool monsterReady = true;
    public bool obstacleReady = true;

    [Header("Startup Guard")]
    [SerializeField] private bool holdNeutralUntilFirstPacket = true;
    [SerializeField] private float startupGuardMaxSeconds = 1f;

    private SerialPort sp;
    private bool hasReceivedValidPacket;
    private float startupTime;

    private void Start()
    {
        startupTime = Time.unscaledTime;
        ApplyNeutralInput();

        if (!enableSerialInput)
        {
            return;
        }

        try
        {
            sp = new SerialPort(portName, baudRate);
            sp.ReadTimeout = readTimeoutMs;
            sp.Open();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to open serial port {portName} @ {baudRate}: {e.Message}");
        }
    }

    private void Update()
    {
        if (ShouldHoldNeutral())
        {
            ApplyNeutralInput();
        }

        if (sp == null || !sp.IsOpen)
        {
            return;
        }

        try
        {
            string data = sp.ReadLine(); // e.g. "0,1,8,7,1,1"
            string[] values = data.Split(',');

            if (values.Length >= 2)
            {
                bool parsedMove = false;
                bool parsedJump = false;

                if (int.TryParse(values[0], out int move))
                {
                    moveValue = move;
                    parsedMove = true;
                }

                if (int.TryParse(values[1], out int jump))
                {
                    jumpValue = jump;
                    parsedJump = true;
                }

                if (parsedMove && parsedJump)
                {
                    hasReceivedValidPacket = true;
                }
            }

            if (values.Length >= 3 && int.TryParse(values[2], out int speed))
            {
                speedValue = speed;
            }

            if (values.Length >= 4 && int.TryParse(values[3], out int parsedMonsterSpeed))
            {
                monsterSpeed = parsedMonsterSpeed;
            }

            if (values.Length >= 5 && TryParseBoolLike(values[4], out bool parsedMonsterReady))
            {
                monsterReady = parsedMonsterReady;
            }

            if (values.Length >= 6 && TryParseBoolLike(values[5], out bool parsedObstacleReady))
            {
                obstacleReady = parsedObstacleReady;
            }
        }
        catch (TimeoutException)
        {
            // Non-blocking read timeout, ignore this frame.
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Serial read error: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (sp != null && sp.IsOpen)
        {
            sp.Close();
        }
    }

    private bool TryParseBoolLike(string raw, out bool value)
    {
        value = false;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        string trimmed = raw.Trim();
        if (trimmed == "1")
        {
            value = true;
            return true;
        }

        if (trimmed == "0")
        {
            value = false;
            return true;
        }

        return bool.TryParse(trimmed, out value);
    }

    private bool ShouldHoldNeutral()
    {
        if (!holdNeutralUntilFirstPacket || hasReceivedValidPacket)
        {
            return false;
        }

        return Time.unscaledTime - startupTime < startupGuardMaxSeconds;
    }

    private void ApplyNeutralInput()
    {
        moveValue = 1;
        jumpValue = 1;
    }
}
