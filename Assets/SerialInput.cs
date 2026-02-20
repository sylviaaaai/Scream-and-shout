using System.IO.Ports;
using UnityEngine;

public class SerialInput : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM3", 9600);

    public int moveValue;
    public int jumpValue;

    void Start()
    {
        sp.Open();
    }

    void Update()
    {
        if (sp.IsOpen)
        {
            string data = sp.ReadLine();   // e.g. "0,1"
            string[] values = data.Split(',');

            moveValue = int.Parse(values[0]);
            jumpValue = int.Parse(values[1]);
        }
    }
}

