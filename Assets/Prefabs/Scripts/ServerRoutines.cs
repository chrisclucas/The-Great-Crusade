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
                GlobalDefinitions.writeToLogFile("TransportScript update()1: Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                break;
        }
    }
}
