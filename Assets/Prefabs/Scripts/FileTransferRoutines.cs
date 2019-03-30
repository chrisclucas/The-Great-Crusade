//using UnityEngine;
//using System.Text;
//// Communications:
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using System.Collections.Generic;
//using System.Collections;

//public class FileTransferRoutines : MonoBehaviour
//{

//    Thread receiveThread;
//    UdpClient udpClient;
//    List<string> messageBuffer = new List<string>();
//    string message;
//    IPEndPoint anyIP;

//    public void SendFileTransfer(string savedFileName)
//    {
//        int PORT = 5017;
//        UdpClient udpClient = new UdpClient(PORT);
//        //udpClient.Client.Bind(new IPEndPoint(IPAddress.Parse(TransportScript.remoteComputerIPAddress), PORT));

//        var data = Encoding.UTF8.GetBytes("ABCD");
//        udpClient.Send(data, data.Length, TransportScript.remoteComputerIPAddress, PORT);
//        GlobalDefinitions.WriteToLogFile("SendFileTransfer: sent ABCD message  to ip address = " + TransportScript.remoteComputerIPAddress + " port = " + PORT);
//    }

//    private IEnumerator RunListening()
//    {
//        GlobalDefinitions.WriteToLogFile("RunListening: executing  listening from ip address = " + TransportScript.remoteComputerIPAddress + "  port = " + TransportScript.fileTransferPort);
//        var client = new UdpClient();
//        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(TransportScript.remoteComputerIPAddress), TransportScript.fileTransferPort);
//        client.Connect(ep);

//        // then receive data
//        var receivedData = client.Receive(ref ep);
//        GlobalDefinitions.WriteToLogFile("RunListening: received = " + receivedData);

//        yield return new WaitForSeconds(1f);
//    }

//    public void ReceiveFileTransfer()
//    {
//        GlobalDefinitions.WriteToLogFile("ReceiveFileTransfer: executing");
//        receiveThread = new Thread(new ThreadStart(ReceiveData));
//        receiveThread.IsBackground = true;
//        receiveThread.Start();
//    }

//    void ReceiveData()
//    {
//        try
//        {
//            anyIP = new IPEndPoint(IPAddress.Any, 0);
//            // Reads received data:
//            byte[] data = udpClient.Receive(ref anyIP);
//            char[] chars = new char[data.Length];
//            for (int c = 0; c < data.Length; c++)
//                chars[c] = (char)data[c];
//            message = new string(chars);
//            GlobalDefinitions.WriteToLogFile("ReceiveData: message recevied = " + message);
//        }
//        catch { }
//    }

//    private void Update()
//    {
//        if (message != null)
//        {
//            Debug.Log("Message received = " + message);
//            message = null;
//            receiveThread.Abort();
//        }
//    }

//    void Disconnect()
//    {
//        GlobalDefinitions.WriteToLogFile("FileTransferServer Disconnect; executing");

//        //receiveThread.Abort();
//        udpClient.Close();
//    }
//    void OnApplicationQuit()
//    {
//        GlobalDefinitions.WriteToLogFile("OnApplicationQuit: executing");
//        Disconnect();
//    }
//}
