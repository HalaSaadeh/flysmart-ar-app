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


}

public class BluetoothModule : MonoBehaviour
{
    private BluetoothHelper helper;

    private bool connected = false;


    public float yaw;
    public float pitch;
    public float roll;


    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

    }

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
            
        }
        catch (Exception ex)
        {
            helper.ScanNearbyDevices();
            
        }
    }

    void OnConnected(BluetoothHelper helper)
    {
        helper.StartListening();
        connected = true;
    }

    void OnConnectionFailed(BluetoothHelper helper)
    {
        
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
       
            // YPRData data = new YPRData(0, 0, 0);
            YPRData data = JsonUtility.FromJson<YPRData>(msg);
            
       
            
            yaw = data.yaw;
            pitch = data.pitch;
            roll = data.roll;

        
    }

    public void Close() {
        helper.Disconnect();
    }
}
