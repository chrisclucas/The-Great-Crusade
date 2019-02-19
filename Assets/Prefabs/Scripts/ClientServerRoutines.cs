using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ClientServerRoutines : MonoBehaviour
{

    public const int BUFFERSIZE = 1024; // started with 512
    public static int reliableChannelId;
    public static int unreliableChannelId;

    public static int connectionId = -1;

    public static int hostId;

    private static bool channelRequested = false;
    private static bool channelEstablished = false;

    static byte sendError;
    static byte[] sendBuffer = new byte[BUFFERSIZE];

    public static int recHostId;
    public static int recConnectionId;
    public static int recChannelId;
    public static byte[] recBuffer = new byte[BUFFERSIZE];
    public static int dataSize;
    public static byte recError;

    public static string fileName;

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
                    SendServerMessage("PlayerID: cbc");

                    break;

                case NetworkEventType.DisconnectEvent:
                    GlobalDefinitions.GuiUpdateStatusMessage("ClientServerRoutines update: Disconnect event received from remote computer - resetting connection");
                    TransportScript.ResetConnection(recHostId);
                    break;

                case NetworkEventType.DataEvent:
                    GlobalDefinitions.WriteToLogFile("ClientServerRoutines update: data event");
                    Stream stream = new MemoryStream(recBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    TransportScript.OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);

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
        //byte error;

        GlobalDefinitions.WriteToLogFile("initiateServerConnection: executing");

        GlobalConfig globalConfig = new GlobalConfig();
        globalConfig.ReactorModel = ReactorModel.SelectReactor; // Process messages as soon as they come in (not good for mobile)
        globalConfig.MaxPacketSize = 1500;

        ConnectionConfig config = new ConnectionConfig();


        reliableChannelId = config.AddChannel(QosType.AllCostDelivery);
        GlobalDefinitions.WriteToLogFile("initiateServerConnection: ReliableChannelID set to " + reliableChannelId);

        config.PacketSize = 1400;
        config.MaxConnectionAttempt = Byte.MaxValue;

        int maxConnections = 2;
        HostTopology topology = new HostTopology(config, maxConnections);
        topology.ReceivedMessagePoolSize = 128;
        topology.SentMessagePoolSize = 1024; // Default 128

        NetworkTransport.Init(globalConfig);

        hostId = NetworkTransport.AddHost(topology);
        GlobalDefinitions.WriteToLogFile("initiateServerConnection: HostID set to " + hostId);

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
        NetworkTransport.Send(hostId, connectionId, reliableChannelId, sendBuffer, BUFFERSIZE, out sendError);
        GlobalDefinitions.WriteToLogFile("Sending message - " + message + " HostID=" + hostId + "  ConnectionID=" + connectionId + " ChannelID=" + reliableChannelId + " Error: " + (NetworkError)sendError);

        if ((NetworkError)sendError != NetworkError.Ok)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("ERROR IN TRANSMISSION - Network Error returned = " + (NetworkError)sendError);
        }
    }
}