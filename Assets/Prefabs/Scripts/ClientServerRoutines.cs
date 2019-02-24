using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ClientServerRoutines : MonoBehaviour
{

    private const int BUFFERSIZE = 1024; // started with 512
    private static int allCostDeliveryChannelId;
    private static int unreliableChannelId;

    private static int connectionId = -1;

    private static int hostId;

    private static bool channelRequested = false;
    private static bool channelEstablished = false;

    private static byte sendError;
    private static byte[] sendBuffer = new byte[BUFFERSIZE];

    private static int recHostId;
    private static int recConnectionId;
    private static int recChannelId;
    private static byte[] recBuffer = new byte[BUFFERSIZE];
    private static int dataSize;
    private static byte recError;

    private byte error;

    void Update()
    {
        if ((channelRequested) && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.ClientServerNetwork))
        {
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    GlobalDefinitions.WriteToLogFile("ClientServerRoutines update: ConnectEvent (hostId = " + recHostId + ", connectionId = " + recConnectionId + ", error = " + recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

                    channelEstablished = true;
                    
                    // At this point I need to pass off control to the normal game flow depending on whether this client in control or waiting on the other computer

                    break;

                case NetworkEventType.DisconnectEvent:
                    GlobalDefinitions.GuiUpdateStatusMessage("ClientServerRoutines update: Disconnect event received from remote computer - resetting connection");
                    NetworkRoutines.ResetConnection(recHostId);
                    break;

                case NetworkEventType.DataEvent:
                    GlobalDefinitions.WriteToLogFile("ClientServerRoutines update: data event");
                    Stream stream = new MemoryStream(recBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    NetworkRoutines.OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);

                    break;

                case NetworkEventType.Nothing:
                    break;
                default:
                    GlobalDefinitions.WriteToLogFile("ClientServerRoutines update(): Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                    break;
            }
        }
    }

    /// <summary>
    /// This routine sets up the parameters for network communication.  Called when initially setting up a connection or resetting an existing connection
    /// </summary>
    public void InitiateServerConnection()
    {
        GlobalDefinitions.WriteToLogFile("initiateServerConnection: executing");

        GlobalConfig globalConfig = new GlobalConfig
        {
            ReactorModel = ReactorModel.SelectReactor, // Process messages as soon as they come in (not good for mobile)
            MaxPacketSize = 1500
        };

        ConnectionConfig config = new ConnectionConfig();

        allCostDeliveryChannelId = config.AddChannel(QosType.AllCostDelivery);

        config.PacketSize = 1400;
        config.MaxConnectionAttempt = Byte.MaxValue;

        int maxConnections = 2;
        HostTopology topology = new HostTopology(config, maxConnections)
        {
            ReceivedMessagePoolSize = 128,
            SentMessagePoolSize = 1024 // Default 128
        };

        NetworkTransport.Init(globalConfig);

        hostId = NetworkTransport.AddHost(topology);

        if (ConnectToServer())
        {
            channelRequested = true;
            GlobalDefinitions.GuiUpdateStatusMessage("initiateServerConnection: Channel requested");
        }
        else
            GlobalDefinitions.GuiUpdateStatusMessage("initiateServerConnection: Connection request failed");
    }

    /// <summary>
    /// Used to connect to the server
    /// </summary>
    /// <returns></returns>
    public static bool ConnectToServer()
    {
        if (!channelEstablished)
        {
            byte error;

            NetworkTransport.Init();

            connectionId = NetworkTransport.Connect(hostId, GlobalDefinitions.serverIPAddress, GlobalDefinitions.port, 0, out error);

            GlobalDefinitions.WriteToLogFile("ConnectToServer: ConnectionID set to " + connectionId + " hostId = " + hostId + ", IP addr = " + GlobalDefinitions.serverIPAddress + ", port = " + GlobalDefinitions.port + ", error = " + error.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

            if (connectionId <= 0)
                return (false);
            else
            {
                return (true);
            }
        }
        return (true); // Connection already established
    }

    /// <summary>
    /// This is the routine that sends messages to the opposing computer
    /// </summary>
    /// <param name="message"></param>
    public static void SendServerMessage(string message)
    {
        Stream stream = new MemoryStream(sendBuffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, message);
        NetworkTransport.Send(hostId, connectionId, allCostDeliveryChannelId, sendBuffer, BUFFERSIZE, out sendError);
        GlobalDefinitions.WriteToLogFile("Sending message - " + message + " HostID=" + hostId + "  ConnectionID=" + connectionId + " ChannelID=" + allCostDeliveryChannelId + " Error: " + (NetworkError)sendError);

        if ((NetworkError)sendError != NetworkError.Ok)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("ERROR IN TRANSMISSION - Network Error returned = " + (NetworkError)sendError);
        }
    }
}