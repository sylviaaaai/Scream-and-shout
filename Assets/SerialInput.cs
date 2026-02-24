using System;
using System.IO.Ports;
using UnityEngine;

public class SerialInput : MonoBehaviour
{
    [Header("Serial")]
    [SerializeField] private string portName = "COM3";
    [SerializeField] private int baudRate = 230400;
    [SerializeField] private int readTimeoutMs = 5;

    [Header("Input Values")]
    public int moveValue = 1;
    public int jumpValue = 1;
    public int speedValue = 3;

    private SerialPort sp;

    private void Start()
    {
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
        if (sp == null || !sp.IsOpen)
        {
            return;
        }

        try
        {
            string data = sp.ReadLine(); // e.g. "0,1,8"
            string[] values = data.Split(',');

            if (values.Length >= 2)
            {
                if (int.TryParse(values[0], out int move))
                {
                    moveValue = move;
                }

                if (int.TryParse(values[1], out int jump))
                {
                    jumpValue = jump;
                }
            }

            if (values.Length >= 3 && int.TryParse(values[2], out int speed))
            {
                speedValue = speed;
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
}
