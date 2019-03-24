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
        UdpClient udpClient = new UdpClient(11000);
        try
        {
            udpClient.Connect(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);

            // Sends a message to the host to which you have connected.
            Byte[] sendBytes = Encoding.ASCII.GetBytes("Sending file name " + savedFileName);

            udpClient.Send(sendBytes, sendBytes.Length);

            udpClient.Close();
        }
        catch (Exception e)
        {
            GlobalDefinitions.WriteToLogFile(e.ToString());
        }
    }

    public static void ReceiveFileTransfer()
    {
        UdpClient udpClient = new UdpClient(11000);
        try
        {
            udpClient.Connect(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);

            //IPEndPoint object will allow us to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

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
