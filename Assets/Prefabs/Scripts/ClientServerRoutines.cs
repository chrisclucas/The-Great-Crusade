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

    //private static int connectionId = -1;

    private static bool channelRequested = false;

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
                    GlobalDefinitions.GuiUpdateStatusMessage("ClientServerRoutines update: ConnectEvent (hostId = " + recHostId + ", connectionId = " + recConnectionId + ", error = " + recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

                    TransportScript.channelRequested = true;

                    // At this point I need to pass off control to the normal game flow depending on whether this client in control or waiting on the other computer

                    break;

                case NetworkEventType.DisconnectEvent:
                    GlobalDefinitions.GuiUpdateStatusMessage("ClientServerRoutines update: Disconnect event received from remote computer - resetting connection");
                    TransportScript.ResetConnection(recHostId);
                    break;

                case NetworkEventType.DataEvent:
                    GlobalDefinitions.GuiUpdateStatusMessage("ClientServerRoutines update: data event");
                    Stream stream = new MemoryStream(recBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    TransportScript.OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);

                    break;

                case NetworkEventType.Nothing:
                    break;
                default:
                    GlobalDefinitions.GuiUpdateStatusMessage("ClientServerRoutines update(): Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
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

        // Set the ip address of the server
        TransportScript.remoteComputerIPAddress = "192.168.1.67";

        //TransportScript.computerId = TransportScript.NetworkInit();

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
        byte error;

        //NetworkRoutines.remoteConnectionId = NetworkTransport.Connect(NetworkRoutines.remoteComputerId, GlobalDefinitions.opponentIPAddress, NetworkRoutines.gamePort, 0, out error);
        TransportScript.gameConnectionId = NetworkTransport.Connect(TransportScript.computerId, TransportScript.remoteComputerIPAddress, TransportScript.defaultGamePort, 0, out error);

        if (TransportScript.gameConnectionId <= 0)
            return (false);
        else
            return (true);
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
        NetworkTransport.Send(TransportScript.computerId, TransportScript.gameConnectionId, allCostDeliveryChannelId, sendBuffer, BUFFERSIZE, out sendError);
        GlobalDefinitions.GuiUpdateStatusMessage("Sending message - " + message + " HostID=" + TransportScript.computerId + "  ConnectionID=" + TransportScript.gameConnectionId + " ChannelID=" + allCostDeliveryChannelId + " Error: " + (NetworkError)sendError);

        if ((NetworkError)sendError != NetworkError.Ok)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("ERROR IN TRANSMISSION - Network Error returned = " + (NetworkError)sendError);
        }
    }
}