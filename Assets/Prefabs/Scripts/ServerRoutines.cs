using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ServerRoutines : MonoBehaviour
{
    public const int BUFFERSIZE = 1024; // started with 512
    public static int recHostId;
    public static int recConnectionId;
    public static int recChannelId;
    public static byte[] recBuffer = new byte[BUFFERSIZE];
    public static int dataSize;
    public static byte recError;

    public static int hostId;
    public static int reliableChannelId;

    static byte sendError;
    static byte[] sendBuffer = new byte[BUFFERSIZE];

    // Update is called once per frame
    void Update ()
    {
        if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Server)
        {
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    GlobalDefinitions.writeToLogFile("ServerRoutines update: ConnectEvent (hostId = " + recHostId + ", connectionId = " + recConnectionId + ", error = " + recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                    GlobalDefinitions.clientHostID = recHostId;
                    GlobalDefinitions.clientConnectionID = recConnectionId;
                    GlobalDefinitions.clientChannelID = recChannelId;

                    sendClientMessage("Connection Confirmed", recHostId, recConnectionId, recChannelId);
                    break;

                case NetworkEventType.DisconnectEvent:
                    GlobalDefinitions.guiUpdateStatusMessage("ServerRoutines update: Disconnect event received from remote computer - resetting connection");
                    GlobalDefinitions.removeGUI(GameObject.Find("NetworkSettingsCanvas"));
                    // Need to add code here to drop the specific client that is sending a disconnect event
                    break;

                case NetworkEventType.DataEvent:
                    GlobalDefinitions.writeToLogFile("ServerRoutines update: data event");
                    Stream stream = new MemoryStream(recBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    TransportScript.OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);

                    break;

                case NetworkEventType.Nothing:
                    break;
                default:
                    GlobalDefinitions.writeToLogFile("ServerRoutines update(): Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                    break;
            }
        }
    }

    public void StartListening()
    {
        GlobalDefinitions.writeToLogFile("StartListening: executing");

        GlobalConfig globalConfig = new GlobalConfig();
        globalConfig.ReactorModel = ReactorModel.SelectReactor; // Process messages as soon as they come in (not good for mobile)
        globalConfig.MaxPacketSize = 1500;

        ConnectionConfig config = new ConnectionConfig();


        reliableChannelId = config.AddChannel(QosType.AllCostDelivery);
        GlobalDefinitions.writeToLogFile("StartListening: ReliableChannelID set to " + reliableChannelId);

        config.PacketSize = 1400;
        config.MaxConnectionAttempt = Byte.MaxValue;

        int maxConnections = 2;
        HostTopology topology = new HostTopology(config, maxConnections);
        topology.ReceivedMessagePoolSize = 128;
        topology.SentMessagePoolSize = 1024; // Default 128

        NetworkTransport.Init(globalConfig);

        hostId = NetworkTransport.AddHost(topology, GlobalDefinitions.port);

        //NetworkServer.Listen(GlobalDefinitions.port);

        //if (NetworkTransport.Connect(hostId, GlobalDefinitions.serverIPAddress, GlobalDefinitions.port, 0, out error) <= 0)
        //    GlobalDefinitions.guiUpdateStatusMessage("StartListening: Server connection request failed");
        //else
        //    GlobalDefinitions.guiUpdateStatusMessage("StartListening: Server connection request successful");
    }

    private void sendClientMessage(string message, int clientHostID, int clientConnectionID, int clientChannelID)
    {
        Stream stream = new MemoryStream(sendBuffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, message);
        NetworkTransport.Send(clientHostID, clientConnectionID, clientChannelID, sendBuffer, BUFFERSIZE, out sendError);
        GlobalDefinitions.writeToLogFile("Sending client message - " + message + " HostID=" + hostId + "  ConnectionID=" + clientConnectionID + " ChannelID=" + clientChannelID + " Error: " + (NetworkError)sendError);

        if ((NetworkError)sendError != NetworkError.Ok)
        {
            GlobalDefinitions.guiUpdateStatusMessage("ERROR IN TRANSMISSION - Network Error returned = " + (NetworkError)sendError);
        }
    }
}
