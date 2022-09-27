using System;
using System.Collections;
using System.Collections.Generic;
using ArduinoBluetoothAPI;
using UnityEngine;
using UnityEngine.UI;

public class YPRData
{
    public float yaw;

    public float pitch;

    public float roll;

    // public YPRData(float yaw, float pitch, float roll){
    //     this.yaw = yaw;
    //     this.pitch = pitch;
    //     this.roll = roll;
    // }
}

public class BluetoothModule : MonoBehaviour
{
    private BluetoothHelper helper;

    private bool connected = false;

    public Text yawField;
    public Text pitchField;
    public Text rollField;

    void Start()
    {
        BluetoothHelper.BLE = true;
        helper = BluetoothHelper.GetInstance("MLT-BT05");
        helper.OnConnected += OnConnected;
        helper.OnConnectionFailed += OnConnectionFailed;
        helper.OnDataReceived += OnDataReceived;
        helper.OnScanEnded += OnScanEnded;
        helper.setTerminatorBasedStream("\n");
        Connect();
    }

    public void Connect()
    {
        if (connected)
        {
            helper.Disconnect();
            connected = false;
        }
        else
        {
            helper.ScanNearbyDevices();
        }
    }

    private void OnScanEnded(
        BluetoothHelper helper,
        LinkedList<BluetoothDevice> devices
    )
    {
        if (devices.Count == 0)
        {
            helper.ScanNearbyDevices();
            return;
        }
        try
        {
            helper.setDeviceName("MLT-BT05");
            helper.Connect();
            Debug.Log("Connecting");
        }
        catch (Exception ex)
        {
            helper.ScanNearbyDevices();
            Debug.Log(ex.Message);
        }
    }

    void OnConnected(BluetoothHelper helper)
    {
        helper.StartListening();
        connected = true;
    }

    void OnConnectionFailed(BluetoothHelper helper)
    {
        Debug.Log("Failed to connect");
        connected = false;
        if (!helper.isConnected())
        {
            helper.Connect();
        }
    }

    // public void sendData()
    // {
    //     var toSend = sendText.text.ToString();
    //     try
    //     {
    //         helper.SendData (toSend);
    //     }
    //     catch (BluetoothHelper.BlueToothNotConnectedException e)
    //     {
    //         receivedText.text = "Device not connected.";
    //     }
    // }

    void OnDataReceived(BluetoothHelper helper)
    {
        string msg = helper.Read();
        try
        {
            // YPRData data = new YPRData(0, 0, 0);
            YPRData data = JsonUtility.FromJson<YPRData>(msg);
            yawField.text = "" + data.yaw;
            pitchField.text = "" + data.pitch;
            rollField.text = "" + data.roll;
            print("Raw message: ");
            print(msg);
            print("JSON data: ");
            print(data.yaw);
            print(data.pitch);
            print(data.roll);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error: " + e);
        }
    }
}
