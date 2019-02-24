using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class ServerRoutines : MonoBehaviour
{
    private const int BUFFERSIZE = 1024; // started with 512
    private int clientHostId;
    private int clientConnectionId;
    private int clientChannelId;
    private byte[] clientBuffer = new byte[BUFFERSIZE];
    private int receivedDataSize;
    private byte receivedError;

    private int hostId;
    private int allCostDeliveryChannelId;

    private byte sendError;
    private byte[] sendBuffer = new byte[BUFFERSIZE];

    private class ClientCommunicationInformationStructure
    {
        public int hostId;
        public int connectionId;
        public int channelId;
        public byte[] buffer;
    }

    private class GameCommunicationInformationStructure
    {
        ClientCommunicationInformationStructure player1;
        ClientCommunicationInformationStructure player2;
    }

    List<GameCommunicationInformationStructure> clientList = new List<GameCommunicationInformationStructure>();

    // Update is called once per frame
    void Update ()
    {
        if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Server)
        {
            NetworkEventType recieveNetworkEvent = NetworkTransport.Receive(out clientHostId, out clientConnectionId, out clientChannelId, clientBuffer, BUFFERSIZE, out receivedDataSize, out receivedError);

            switch (recieveNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    GlobalDefinitions.WriteToLogFile("ServerRoutines update: ConnectEvent (hostId = " + clientHostId + ", connectionId = " + clientConnectionId + ", error = " + receivedError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                    ClientCommunicationInformationStructure clientInfo = new ClientCommunicationInformationStructure
                    {
                        hostId = clientHostId,
                        connectionId = clientConnectionId,
                        channelId = clientChannelId,
                        buffer = clientBuffer
                    };


                    SendMessageToClient("Connection Confirmed", clientHostId, clientConnectionId, clientChannelId);
                    break;

                case NetworkEventType.DisconnectEvent:
                    GlobalDefinitions.GuiUpdateStatusMessage("ServerRoutines update: Disconnect event received from remote computer - resetting connection");
                    GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                    // Need to add code here to drop the specific client that is sending a disconnect event
                    break;

                case NetworkEventType.DataEvent:
                    GlobalDefinitions.WriteToLogFile("ServerRoutines update: data event");
                    Stream stream = new MemoryStream(clientBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    NetworkRoutines.OnData(clientHostId, clientConnectionId, clientChannelId, message, receivedDataSize, (NetworkError)receivedError);

                    break;

                case NetworkEventType.Nothing:
                    break;
                default:
                    GlobalDefinitions.WriteToLogFile("ServerRoutines update(): Unknown network event type received - " + recieveNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                    break;
            }
        }
    }

    /// <summary>
    /// Sets-up the network parameters
    /// </summary>
    public void StartListening()
    {
        GlobalDefinitions.WriteToLogFile("StartListening: executing");

        GlobalConfig globalConfig = new GlobalConfig
        {
            ReactorModel = ReactorModel.SelectReactor, // Process messages as soon as they come in (not good for mobile)
            MaxPacketSize = 1500
        };

        ConnectionConfig config = new ConnectionConfig();

        allCostDeliveryChannelId = config.AddChannel(QosType.AllCostDelivery);

        config.PacketSize = 1400;
        config.MaxConnectionAttempt = Byte.MaxValue;

        int maxConnections = 2; // Need to change this because this is the server
        HostTopology topology = new HostTopology(config, maxConnections)
        {
            ReceivedMessagePoolSize = 128,
            SentMessagePoolSize = 1024 // Default 128
        };

        NetworkTransport.Init(globalConfig);

        // Note, while the hostId below doesn't get used anywhere the line is needed to start the server listening, I know I should probably not assign the result, but ...
        hostId = NetworkTransport.AddHost(topology, NetworkRoutines.gamePort);
    }

    private void SendMessageToClient(string message, int clientHostID, int clientConnectionID, int clientChannelID)
    {
        Stream stream = new MemoryStream(sendBuffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, message);
        NetworkTransport.Send(clientHostID, clientConnectionID, clientChannelID, sendBuffer, BUFFERSIZE, out sendError);
        GlobalDefinitions.WriteToLogFile("Sending client message - " + message + " HostID=" + clientHostID + "  ConnectionID=" + clientConnectionID + " ChannelID=" + clientChannelID + " Error: " + (NetworkError)sendError);

        if ((NetworkError)sendError != NetworkError.Ok)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("ERROR IN TRANSMISSION - Network Error returned = " + (NetworkError)sendError);
        }
    }
}
