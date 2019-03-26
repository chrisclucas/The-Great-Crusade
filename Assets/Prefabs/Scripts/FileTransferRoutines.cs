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
        UdpClient udpClient = new UdpClient(TransportScript.fileTransferPort);
        GlobalDefinitions.WriteToLogFile("SendFileTransfer: created udp client");
        try
        {
            udpClient.Connect(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);
            GlobalDefinitions.WriteToLogFile("SendFileTransfer: connection executed  " + IPAddress.Parse(TransportScript.remoteComputerIPAddress) + " " + TransportScript.fileTransferPort);

            // Sends a message to the host to which you have connected.
            Byte[] sendBytes = Encoding.ASCII.GetBytes("Sending file name " + savedFileName);

            udpClient.Send(sendBytes, sendBytes.Length);
            GlobalDefinitions.WriteToLogFile("SendFileTransfer: sent message - Sending file name " + savedFileName);

            //udpClient.Close();
            //GlobalDefinitions.WriteToLogFile("SendFileTransfer: closed udp connection");
        }
        catch (Exception e)
        {
            GlobalDefinitions.WriteToLogFile(e.ToString());
        }
    }

    public void ReceiveFileTransfer()
    {
        udpClient = new UdpClient(TransportScript.fileTransferPort);
        GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: created udp client");

        try
        {
            udpClient.Connect(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);
            GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: connection executed  " + IPAddress.Parse(TransportScript.remoteComputerIPAddress) + " " + TransportScript.fileTransferPort);

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);
            GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: setup RemoteIPEndPoint");

            // Blocks until a message returns on this socket from a remote host.
            //Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            //string returnData = Encoding.ASCII.GetString(receiveBytes);

            // Thread listening incoming messages:
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            // Uses the IPEndPoint object to determine which of these two hosts responded.
            //GlobalDefinitions.WriteToLogFile("SetupFileTransfer: message received = " + returnData.ToString());
            //GlobalDefinitions.WriteToLogFile("This message was sent from " + RemoteIpEndPoint.Address.ToString() + " on their port number " + RemoteIpEndPoint.Port.ToString());

            //udpClient.Close();
        }
        catch (Exception e)
        {
            GlobalDefinitions.WriteToLogFile(e.ToString());
        }
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
