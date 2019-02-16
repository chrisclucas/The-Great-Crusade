using UnityEngine;
using UnityEngine.Networking;
using System;

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

        GlobalDefinitions.writeToLogFile("initiateServerConnection: executing");
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    channelEstablished - " + channelEstablished);
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    gameStarted - " + GlobalDefinitions.gameStarted);
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    opponentComputerConfirmsSync - " + serverConfirmsSync);
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    handshakeConfirmed - " + handshakeConfirmed);
        GlobalDefinitions.writeToLogFile("initiateServerConnection:    gameDataSent - " + gameDataSent);

        if (TransportScript.Connect(GlobalDefinitions.serverIPAddress))
        {
            TransportScript.channelEstablished = true;
            GlobalDefinitions.writeToLogFile("okNetworkSettings: Channel Established");
            GlobalDefinitions.guiUpdateStatusMessage("Channel Established");
            TransportScript.SendSocketMessage("InControl");
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Connection Failed");
    }
}