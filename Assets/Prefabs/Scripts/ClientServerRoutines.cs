using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class ClientServerRoutines : MonoBehaviour
{

    public const int BUFFERSIZE = 1024; // started with 512
    public static int reliableChannelId;
    public static int unreliableChannelId;
    public static int socketPort = 5016;

    public static int connectionId = -1;

    public static int serverSocket = -1;
    public static int clientSocket = -1;

    public static bool channelEstablished = false;
    public static bool connectionConfirmed = false;
    public static bool handshakeConfirmed = false;
    public static bool serverConfirmsSync = false;
    public static bool gameDataSent = false;

    static byte sendError;
    static byte[] sendBuffer = new byte[BUFFERSIZE];

    public static int recHostId;
    public static int recConnectionId;
    public static int recChannelId;
    public static byte[] recBuffer = new byte[BUFFERSIZE];
    public static int dataSize;
    public static byte recError;

    public static string fileName;

    /// <summary>
    /// This routine sets up the parameters for network communication.  Called when initially setting up a connection or resetting an existing connection
    /// </summary>
    public static void initiateServerConnection()
    {
        byte error;

        GlobalDefinitions.writeToLogFile("initiateServerConnection: executing");
        GlobalConfig globalConfig = new GlobalConfig();
        globalConfig.ReactorModel = ReactorModel.SelectReactor; // Process messages as soon as they come in (not good for mobile)
        globalConfig.MaxPacketSize = 1500;

        ConnectionConfig config = new ConnectionConfig();
        config.PacketSize = 1400;
        config.MaxConnectionAttempt = Byte.MaxValue;

        reliableChannelId = config.AddChannel(QosType.AllCostDelivery);

        int maxConnections = 2;
        HostTopology topology = new HostTopology(config, maxConnections);
        topology.ReceivedMessagePoolSize = 128;
        topology.SentMessagePoolSize = 1024; // Default 128

        NetworkTransport.Init(globalConfig);

        // If either of the socket variables are set they need to be disconnected and reset (-1 indicates that they aren't assigned)
        if (serverSocket != -1)
        {
            GlobalDefinitions.writeToLogFile("initiateServerConnection: server socket set to " + serverSocket + " - disconnecting and resetting to -1");
            NetworkTransport.Disconnect(serverSocket, connectionId, out error);
            serverSocket = -1;
        }
        if (clientSocket != -1)
        {
            GlobalDefinitions.writeToLogFile("initiateServerConnection: client socket set to " + clientSocket + " - disconnecting and resetting to -1");
            NetworkTransport.Disconnect(clientSocket, connectionId, out error);
            clientSocket = -1;
        }

        serverSocket = NetworkTransport.AddHost(topology, socketPort);
        clientSocket = NetworkTransport.AddHost(topology);

        GlobalDefinitions.serverIPAddress = "192.168.1.67";

        GlobalDefinitions.writeToLogFile("initiateServerConnection: executing");
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    channelEstablished - " + channelEstablished);
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    gameStarted - " + GlobalDefinitions.gameStarted);
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    opponentComputerConfirmsSync - " + serverConfirmsSync);
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    handshakeConfirmed - " + handshakeConfirmed);
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    gameDataSent - " + gameDataSent);

        if (channelEstablished)
        {
            GlobalDefinitions.writeToLogFile("initiateServerConnection: sending message InControl");
            TransportScript.SendSocketMessage("InControl");
            GlobalDefinitions.userIsIntiating = true;
            GlobalDefinitions.writeToLogFile("initiateServerConnection: checkForHandshakeReceipt(NotInControl)");
            TransportScript.checkForHandshakeReceipt("NotInControl");
        }
    }
}

// State object for receiving data from remote device.  
public class StateObject
{
    // Client socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 256;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousClient
{

    // ManualResetEvent instances signal completion.  
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    // The response from the remote device.  
    private static String response = String.Empty;

    public static void StartClient()
    {
        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            GlobalDefinitions.serverIPAddress = "192.168.1.67";
            IPAddress ipAddress = IPAddress.Parse(GlobalDefinitions.serverIPAddress);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, GlobalDefinitions.port);

            // Create a TCP/IP socket.  
            Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();

            // Send test data to the remote device.  
            Send(client, "This is a test<EOF>");
            sendDone.WaitOne();

            // Receive the response from the remote device.  
            Receive(client);
            receiveDone.WaitOne();

            // Write the response to the console.  
            GlobalDefinitions.writeToLogFile("Response received : " + response);

            // Release the socket.  
            client.Shutdown(SocketShutdown.Both);
            client.Close();

        }
        catch (Exception e)
        {
            GlobalDefinitions.writeToLogFile(e.ToString());
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            GlobalDefinitions.writeToLogFile("Socket connected to " + client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.  
            connectDone.Set();
        }
        catch (Exception e)
        {
            GlobalDefinitions.writeToLogFile(e.ToString());
        }
    }

    private static void Receive(Socket client)
    {
        try
        {
            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.  
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            GlobalDefinitions.writeToLogFile(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the client socket   
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            // Read data from the remote device.  
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Get the rest of the data.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                // All the data has arrived; put it in response.  
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                // Signal that all bytes have been received.  
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
            GlobalDefinitions.writeToLogFile(e.ToString());
        }
    }

    private static void Send(Socket client, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            GlobalDefinitions.writeToLogFile("Sent " + bytesSent + " bytes to server.");

            // Signal that all bytes have been sent.  
            sendDone.Set();
        }
        catch (Exception e)
        {
            GlobalDefinitions.writeToLogFile(e.ToString());
        }
    }
}