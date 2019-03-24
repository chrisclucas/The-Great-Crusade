using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;
// Communications:
using System.Net;
using System.Net.Sockets;

public class FileTransferRoutines : MonoBehaviour
{
    // This constructor arbitrarily assigns the local port number.
    public static void SendFileTransfer(string savedFileName)
    {
        UdpClient udpClient = new UdpClient(TransportScript.fileTransferPort);
        GlobalDefinitions.WriteToLogFile("SendFileTransfer: created udp client");
        try
        {
            udpClient.Connect(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);
            GlobalDefinitions.WriteToLogFile("SendFileTransfer: connection executed");

            // Sends a message to the host to which you have connected.
            Byte[] sendBytes = Encoding.ASCII.GetBytes("Sending file name " + savedFileName);

            udpClient.Send(sendBytes, sendBytes.Length);
            GlobalDefinitions.WriteToLogFile("SendFileTransfer: sent message");

            udpClient.Close();
            GlobalDefinitions.WriteToLogFile("SendFileTransfer: closed udp connection");
        }
        catch (Exception e)
        {
            GlobalDefinitions.WriteToLogFile(e.ToString());
        }
    }

    public static void ReceiveFileTransfer()
    {
        UdpClient udpClient = new UdpClient(TransportScript.fileTransferPort);
        GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: created udp client");

        try
        {
            udpClient.Connect(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);
            GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: connection executed");

            //IPEndPoint object will allow us to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: setup RemoteIPEndPoint");

            // Blocks until a message returns on this socket from a remote host.
            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);

            // Uses the IPEndPoint object to determine which of these two hosts responded.
            GlobalDefinitions.WriteToLogFile("SetupFileTransfer: message received = " + returnData.ToString());
            GlobalDefinitions.WriteToLogFile("This message was sent from " + RemoteIpEndPoint.Address.ToString() + " on their port number " + RemoteIpEndPoint.Port.ToString());

            udpClient.Close();
        }
        catch (Exception e)
        {
            GlobalDefinitions.WriteToLogFile(e.ToString());
        }
    }
}
