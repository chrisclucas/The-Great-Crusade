using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class NetworkRoutines : MonoBehaviour
{
    public const int BUFFERSIZE = 1024; // started with 512
    private static int reliableChannelId;
    private static int unreliableChannelId;
    private static int socketPort = 5016;

    public static bool channelEstablished = false;
    public static bool connectionConfirmed = false;
    public static bool handshakeConfirmed = false;
    public static bool opponentComputerConfirmsSync = false;
    public static bool gameDataSent = false;

    private static byte sendError;
    private static byte[] sendBuffer = new byte[BUFFERSIZE];

    public static int remoteComputerId;
    public static int remoteConnectionId;
    public static int remoteChannelId;
    public static byte[] remoteBuffer = new byte[BUFFERSIZE];
    public static int dataSize;
    public static byte receivedError;

    /// <summary>
    /// This routine sets up the parameters for network communication.  Called when initially setting up a connection or resetting an existing connection
    /// </summary>
    public static int NetworkInit()
    {
        //byte error;

        GlobalDefinitions.WriteToLogFile("networkInit(): executing");
        GlobalConfig globalConfig = new GlobalConfig
        {
            ReactorModel = ReactorModel.SelectReactor, // Process messages as soon as they come in (not good for mobile)
            MaxPacketSize = 1500
        };

        ConnectionConfig config = new ConnectionConfig
        {
            PacketSize = 1400,
            MaxConnectionAttempt = Byte.MaxValue
        };

        reliableChannelId = config.AddChannel(QosType.AllCostDelivery);

        NetworkTransport.Init(globalConfig);

        int maxConnections = 2;
        HostTopology topology = new HostTopology(config, maxConnections)
        {
            ReceivedMessagePoolSize = 128,
            SentMessagePoolSize = 1024 // Default 128
        };

        return (NetworkTransport.AddHost(topology));

        // If either of the socket variables are set they need to be disconnected and reset (-1 indicates that they aren't assigned)
        //if (serverSocket != -1)
        //{
        //    GlobalDefinitions.WriteToLogFile("networkInit: server socket set to " + serverSocket + " - disconnecting and resetting to -1");
        //    NetworkTransport.Disconnect(serverSocket, connectionId, out error);
        //    serverSocket = -1;
        //}
        //if (clientSocket != -1)
        //{
        //    GlobalDefinitions.WriteToLogFile("networkInit: client socket set to " + clientSocket + " - disconnecting and resetting to -1");
        //    NetworkTransport.Disconnect(clientSocket, connectionId, out error);
        //    clientSocket = -1;
        //}

        //serverSocket = NetworkTransport.AddHost(topology, socketPort);
    }

    /// <summary>
    /// This is the routine that runs when the connect button on the gui is clicked
    /// </summary>
    /// <param name="opponentIPaddr"></param>
    /// <returns></returns>
    public static bool Connect(string opponentIPaddr)
    {
        byte error;

        NetworkTransport.Init();

        remoteConnectionId = NetworkTransport.Connect(remoteComputerId, opponentIPaddr, socketPort, 0, out error);

        GlobalDefinitions.WriteToLogFile("Initial Connection(clientSocket (hostId) = " + remoteComputerId + ", Remote Connection Id = " + remoteConnectionId + ", socketPort = " + socketPort + ", error = " + error.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

        if (remoteConnectionId <= 0)
            return (false);
        else
            return (true);
    }

    /// <summary>
    /// This is the routine that sends messages to the opposing computer
    /// </summary>
    /// <param name="message"></param>
    public static void SendMessageToRemoteComputer(string message)
    {
        if (connectionConfirmed)
        {
            Stream stream = new MemoryStream(sendBuffer);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);
            NetworkTransport.Send(remoteComputerId, remoteConnectionId, reliableChannelId, sendBuffer, BUFFERSIZE, out sendError);
            GlobalDefinitions.WriteToLogFile("Sending message - " + message + " remoteComputerId=" + remoteComputerId + "  remoteConnectionId=" + remoteConnectionId + " Error: " + (NetworkError)sendError);

            if ((NetworkError)sendError != NetworkError.Ok)
            {
                GlobalDefinitions.GuiUpdateStatusMessage("ERROR IN TRANSMISSION - Network Error returned = " + (NetworkError)sendError);
            }
        }
        else
        {
            GlobalDefinitions.WriteToLogFile("Connection hasn't been confirmed message = " + message + "  " + DateTime.Now.ToString("h:mm:ss tt"));
        }
    }

    /// <summary>
    /// Sends a message to determine if the two computers are in agreement
    /// </summary>
    public void SendHandshakeMessage()
    {
        if (GlobalDefinitions.userIsIntiating)
        {
            GlobalDefinitions.WriteToLogFile("sendHandshakeMessage: sending InControl");
            SendMessageToRemoteComputer("InControl");
        }
        else
        {
            GlobalDefinitions.WriteToLogFile("sendHandshakeMessage: sending NotInControl");
            SendMessageToRemoteComputer("NotInControl");
        }
    }

    /// <summary>
    /// This routine checks if the two computers are in agreement about who is initiating the game
    /// </summary>
    /// <param name="message"></param>
    public static void CheckForHandshakeReceipt(string message)
    {
        // Check to confirm that the remote computer is set appropriately compared to what is set on the local computer.
        if (GlobalDefinitions.userIsIntiating)
        {
            if (message == "InControl")
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Remote computer also indicated that it was initiating the game");
                handshakeConfirmed = false;
            }
            else if (message == "NotInControl")
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Handshaking confirmed");
                handshakeConfirmed = true;
            }
            else
            {
                GlobalDefinitions.WriteToLogFile("checkForHandshakeReceipt: Unknown message received (user is initiating) - " + message);
                handshakeConfirmed = false;
            }
        }
        else
        {
            if (message == "NotInControl")
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Remote computer also indicated that it was not initiating the game");
                handshakeConfirmed = false;
            }
            else if (message == "InControl")
            {
                GlobalDefinitions.WriteToLogFile("Handshaking confirmed" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                handshakeConfirmed = true;
            }
            else
            {
                GlobalDefinitions.WriteToLogFile("checkForHandshakeReceipt: Unknown message received - (user is not initiating)" + message);
                handshakeConfirmed = false;
            }
        }
    }

    /// <summary>
    /// Executes when the other computer is disconnected
    /// </summary>
    /// <param name="hostId"></param>
    public static void ResetConnection(int hostId)
    {
        byte error;

        GlobalDefinitions.WriteToLogFile("TransportScript.resetConnection: (hostId = " + hostId + ", connectionId = "
                + remoteConnectionId + ", error = " + receivedError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

        // Send a disconnect command to the remote computer
        SendMessageToRemoteComputer(GlobalDefinitions.DISCONNECTFROMREMOTECOMPUTER);

        GlobalDefinitions.SwitchLocalControl(false);
        GlobalDefinitions.opponentIPAddress = "";
        GlobalDefinitions.userIsIntiating = false;
        GlobalDefinitions.isServer = false;
        GlobalDefinitions.hasReceivedConfirmation = false;
        GlobalDefinitions.gameStarted = false;
        NetworkRoutines.channelEstablished = false;
        NetworkRoutines.connectionConfirmed = false;
        NetworkRoutines.handshakeConfirmed = false;
        NetworkRoutines.opponentComputerConfirmsSync = false;
        NetworkRoutines.gameDataSent = false;

        if (hostId == remoteComputerId)
        {
            NetworkTransport.Disconnect(remoteComputerId, remoteConnectionId, out error);
            GlobalDefinitions.WriteToLogFile("resetConnection: NetworkTransport.Disconnect(serverSocket=" + remoteComputerId + ", connectionId=" + remoteConnectionId + ", error = " + ((NetworkError)error).ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
        }
        else
            GlobalDefinitions.WriteToLogFile("resetConnectin: Request recieved to disconnect unknown host - " + hostId);

        NetworkInit();
    }

    /// <summary>
    /// Writes data event to log file
    /// </summary>
    /// <param name="hostId"></param>
    /// <param name="connectionId"></param>
    /// <param name="channelId"></param>
    /// <param name="message"></param>
    /// <param name="size"></param>
    /// <param name="error"></param>
    public static void OnData(int hostId, int connectionId, int channelId, string message, int size, NetworkError error)
    {
        GlobalDefinitions.WriteToLogFile("Data Event Received: (hostId = " + hostId + ", connectionId = "
            + connectionId + ", channelId = " + channelId + ", data = "
            + message + ", size = " + size + ", error = " + error.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
    }
}
