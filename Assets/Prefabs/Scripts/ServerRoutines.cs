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

    // Update is called once per frame
    void Update ()
    {
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

        switch (recNetworkEvent)
        {
            case NetworkEventType.ConnectEvent:
                GlobalDefinitions.writeToLogFile("ServerRoutines update: ConnectEvent (hostId = " + recHostId + ", connectionId = " + recConnectionId + ", error = " + recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                GlobalDefinitions.communicationSocket = recHostId;
                GlobalDefinitions.communicationChannel = recConnectionId;

                TransportScript.SendSocketMessage("ConfirmSync");

                break;

            case NetworkEventType.DisconnectEvent:
                GlobalDefinitions.guiUpdateStatusMessage("ServerRoutines update: Disconnect event received from remote computer - resetting connection");
                GlobalDefinitions.removeGUI(GameObject.Find("NetworkSettingsCanvas"));
                TransportScript.resetConnection(recHostId);
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

    public static void StartListening()
    {
        byte error;

        GlobalDefinitions.writeToLogFile("initiateServerConnection: executing");

        GlobalConfig globalConfig = new GlobalConfig();
        globalConfig.ReactorModel = ReactorModel.SelectReactor; // Process messages as soon as they come in (not good for mobile)
        globalConfig.MaxPacketSize = 1500;

        ConnectionConfig config = new ConnectionConfig();


        reliableChannelId = config.AddChannel(QosType.AllCostDelivery);
        GlobalDefinitions.writeToLogFile("initiateServerConnection: ReliableChannelID set to " + reliableChannelId);

        config.PacketSize = 1400;
        config.MaxConnectionAttempt = Byte.MaxValue;

        int maxConnections = 2;
        HostTopology topology = new HostTopology(config, maxConnections);
        topology.ReceivedMessagePoolSize = 128;
        topology.SentMessagePoolSize = 1024; // Default 128

        NetworkTransport.Init(globalConfig);

        hostId = NetworkTransport.AddHost(topology, GlobalDefinitions.port);

        if (NetworkTransport.Connect(hostId, GlobalDefinitions.serverIPAddress, GlobalDefinitions.port, 0, out error) <= 0)
            GlobalDefinitions.guiUpdateStatusMessage("Connection Failed");
        else
            GlobalDefinitions.guiUpdateStatusMessage("Connection Established");
    }
}
