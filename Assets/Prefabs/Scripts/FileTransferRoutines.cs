using UnityEngine;
using System;
using System.Text;
// Communications:
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class FileTransferRoutines : MonoBehaviour
{

    Thread receiveThread;
    UdpClient udpClient;
    List<string> messageBuffer = new List<string>();
    string message;

    // This constructor arbitrarily assigns the local port number.
    public void SendFileTransfer(string savedFileName)
    {
        GlobalDefinitions.WriteToLogFile("SendFileTransfer: executing");
        UdpClient udpServer = new UdpClient(11000);

        while (true)
        {
            var remoteEP = new IPEndPoint(IPAddress.Any, TransportScript.fileTransferPort);
            udpServer.Send(new byte[] { 1 }, 1, remoteEP);
        }
    }

    public void ReceiveFileTransfer()
    {
        GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: executing");
        var client = new UdpClient();
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.1.73"), TransportScript.fileTransferPort);
        client.Connect(ep);

        // then receive data
        var receivedData = client.Receive(ref ep);
        GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: received = " + receivedData);
    }

    void ReceiveData()
    {
        while (udpClient != null)
        {
            try
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);
                // Reads received data:
                byte[] data = udpClient.Receive(ref RemoteIpEndPoint);
                char[] chars = new char[data.Length];
                for (int c = 0; c < data.Length; c++)
                    chars[c] = (char)data[c];
                message = new string(chars);
            }
            catch { }
        }
    }

    private void Update()
    {
        if (message != null)
        {
            Debug.Log("Message received = " + message);
            message = null;
            receiveThread.Abort();
        }
    }

    void Disconnect()
    {
        GlobalDefinitions.WriteToLogFile("FileTransferServer Disconnect; executing");

        receiveThread.Abort();
        udpClient.Close();
    }
    void OnApplicationQuit()
    {
        GlobalDefinitions.WriteToLogFile("OnApplicationQuit: executing");
        Disconnect();
    }
}
