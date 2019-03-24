using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class TransportScript : MonoBehaviour
{
    public const string defaultRemoteComputerIPAddress = "45.30.112.248";
    //public const string defaultRemoteComputerIPAddress = "192.168.1.73";

    public static string remoteComputerIPAddress;
    public static string localComputerIPAddress;

    public const int defaultGamePort = 5016;
    public const int defaultFileTransferPort = 5017;

    public static int remoteGamePort;
    public static int localGamePort;
    public static int remoteFileTransferPort;
    public static int localFileTransferPort;

    public static int gameConnectionId = -1;
    public static int fileTransferConnectionId = -1;

    public static int localGameComputerId = -1;
    public static int remoteGameComputerId = -1;
    public static int localFileTransferComputerId = -1;
    public static int remoteFileTransferComputerId = -1;

    public static bool channelRequested = false;
    public static bool connectionConfirmed = false;
    public static bool handshakeConfirmed = false;
    public static bool opponentComputerConfirmsSync = false;
    public static bool gameDataSent = false;


    public const int BUFFERSIZE = 1024; // started with 512
    public static int reliableChannelId;
    public static int unreliableChannelId;

    static byte sendError;
    static byte[] sendBuffer = new byte[BUFFERSIZE];

    private static int recievedHostId;
    private static int recievedConnectionId;
    private static int recievedChannelId;
    private static byte[] recievedBuffer = new byte[BUFFERSIZE];
    private static int recievedDataSize;
    private static byte recievedError;

    /// <summary>
    /// This routine sets up the parameters for network communication.  Called when initially setting up a connection or resetting an existing connection
    /// </summary>
    public static int NetworkInit()
    {
        byte error;

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
        if (localGameComputerId != -1)
        {
            GlobalDefinitions.WriteToLogFile("NetworkInit: sending disconnect serverSocket = -1");
            NetworkTransport.Disconnect(localGameComputerId, gameConnectionId, out error);
            localGameComputerId = -1;
        }
        if (remoteGameComputerId != -1)
        {
            GlobalDefinitions.WriteToLogFile("NetworkInit: sending disconnect remoteComputerId = -1");
            NetworkTransport.Disconnect(remoteGameComputerId, gameConnectionId, out error);
            remoteGameComputerId = -1;
        }

        localGameComputerId = NetworkTransport.AddHost(topology, localGamePort);
        remoteGameComputerId = NetworkTransport.AddHost(topology);

        return (remoteGameComputerId);

    }

    public static int ConfigureFileTransferConnection()
    {
        byte error;

        GlobalDefinitions.WriteToLogFile("ConfigureFileTransferConnection: localFileTransferPort = " + localFileTransferPort + " remoteFileTransferPort = " + remoteFileTransferPort);

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
        if (localFileTransferComputerId != -1)
        {
            NetworkTransport.Disconnect(localFileTransferComputerId, fileTransferConnectionId, out error);
            localFileTransferComputerId = -1;
        }
        if (remoteFileTransferComputerId != -1)
        {
            NetworkTransport.Disconnect(remoteFileTransferComputerId, fileTransferConnectionId, out error);
            remoteFileTransferComputerId = -1;
        }

        localFileTransferComputerId = NetworkTransport.AddHost(topology, localFileTransferPort);
        remoteFileTransferComputerId = NetworkTransport.AddHost(topology);

        return (remoteFileTransferComputerId);

    }

    void Start()
    {

    }

    void Update()
    {
        // This update() executes up until the game data is loaded and everything is set up.  Then the GameControl update() takes over.
        if (!GlobalDefinitions.gameStarted)
        {
            // Walk through the three statges of connection - channel requested, confirming sync, and handshake
            if ((GlobalDefinitions.userIsNotInitiating && !channelRequested) ||
                    (channelRequested && !opponentComputerConfirmsSync) ||
                    (opponentComputerConfirmsSync && !handshakeConfirmed))
            {
                NetworkEventType recNetworkEvent;

                // This try is needed since this will start executing once the user indicates that he isn't initiating the game but the
                // init of the network doesn't take place until OK is selected
                try
                {
                    recNetworkEvent = NetworkTransport.Receive(out recievedHostId, out recievedConnectionId, out recievedChannelId, recievedBuffer, BUFFERSIZE, out recievedDataSize, out recievedError);

                    // Need to trap the connection here since I need to distingush between the game connection and the file transfer connection
                    if (recNetworkEvent == NetworkEventType.ConnectEvent)
                    {
                        gameConnectionId = recievedHostId;
                        remoteGameComputerId = recievedHostId;
                    }
                }
                catch
                {
                    return;
                }

                processNetworkEvent(recNetworkEvent);
            }

            // Executes from confirmation of handshake to sending of game data
            else if (handshakeConfirmed && !gameDataSent)
            {
                GlobalDefinitions.chatPanel.SetActive(true);
                GameObject.Find("ChatInputField").SetActive(true);
                GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));  // Get rid of the gui, we don't need it if we got here.
                GlobalDefinitions.GuiUpdateStatusMessage("Waiting on intial data load");

                gameDataSent = true;
                if (GlobalDefinitions.userIsIntiating)
                {
                    // Playing a new game
                    if (MainMenuRoutines.playNewGame)
                    {

                        // Since at this point we know we are starting a new game and not running the command file, remove the command file
                        if (!GlobalDefinitions.commandFileBeingRead)
                            if (File.Exists(GameControl.path + GlobalDefinitions.commandFile))
                            {
                                GlobalDefinitions.DeleteCommandFile();
                                GlobalDefinitions.DeleteFullCommandFile();
                            }

                        // Set the game state to Setup 
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
                        GameControl.setUpStateInstance.GetComponent<SetUpState>().ExecuteNewGame();
                        GlobalDefinitions.gameStarted = true;

                        if (GlobalDefinitions.sideControled == GlobalDefinitions.Nationality.German)
                        {
                            GlobalDefinitions.SwitchLocalControl(true);
                            SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " Allied");
                            SendMessageToRemoteComputer(GlobalDefinitions.PLAYNEWGAMEKEYWORD + " " + GlobalDefinitions.germanSetupFileUsed);
                        }
                        else
                        {
                            // Pass control to the remote computer
                            SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " German");
                            SendMessageToRemoteComputer(GlobalDefinitions.PLAYNEWGAMEKEYWORD + " " + GlobalDefinitions.germanSetupFileUsed);
                            SendMessageToRemoteComputer(GlobalDefinitions.PASSCONTROLKEYWORK);
                            GlobalDefinitions.SwitchLocalControl(false);
                        }

                    }

                    // Playing a saved game
                    else
                    {
                        string savedFileName = "";
                        savedFileName = GlobalDefinitions.GuiFileDialog();

                        if (GlobalDefinitions.sideControled == GlobalDefinitions.Nationality.German)
                            SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " Allied");
                        else
                            SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " German");

                        // Call the routine to read a saved file note that this call will set the localControl variable
                        GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadTurnFile(savedFileName); // Note this will set the currentState based on the saved file

                        GlobalDefinitions.GuiUpdateStatusMessage("TransportScript Update(): Waiting on remote data load...");

                        // Tell the remote computer what file to load.  It will then turn around and request it
                        SendMessageToRemoteComputer(GlobalDefinitions.SENDTURNFILENAMEWORD + " " + savedFileName);

                        // Now initiate file transfer setup
                        ConfigureFileTransferConnection();
                        FileTransferConnect(remoteComputerIPAddress);
                        GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().InitiateFileTransferServer();

                        gameDataSent = true;
                    }
                }
                else
                {
                    // The non-initiating computer will move on to game mode since the read of the game data is conducted with gameStarted set
                    GlobalDefinitions.gameStarted = true;
                    GlobalDefinitions.SwitchLocalControl(false);
                }
            }

            // Last section before turning over to game play
            else if (gameDataSent)
            {
                // Check if there is a network event
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recievedHostId, out recievedConnectionId, out recievedChannelId, recievedBuffer, BUFFERSIZE, out recievedDataSize, out recievedError);

                switch (recNetworkEvent)
                {
                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                        ResetConnection(recievedHostId);
                        break;

                    case NetworkEventType.ConnectEvent:
                        // This connection event traps the connection on the file transfer port.  Send a message back
                        remoteFileTransferComputerId = recievedHostId;
                        fileTransferConnectionId = recievedConnectionId;
                        SendFileTransferMessageToRemoteComputer("ConnectionEventReceived");
                        break;

                    case NetworkEventType.DataEvent:
                        char[] delimiterChars = { ' ' };

                        Stream stream = new MemoryStream(recievedBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        OnData(recievedHostId, recievedConnectionId, recievedChannelId, message, recievedDataSize, (NetworkError)recievedError);
                        string[] switchEntries = message.Split(delimiterChars);

                        if (switchEntries[0] == GlobalDefinitions.GAMEDATALOADEDKEYWORD)
                        {
                            GlobalDefinitions.GuiUpdateStatusMessage("Remote data load complete");
                            GlobalDefinitions.WriteToLogFile("Calling File Transfer disconnect");
                            NetworkTransport.Disconnect(remoteFileTransferComputerId, fileTransferConnectionId, out recievedError);

                            GlobalDefinitions.gameStarted = true;
                            if (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.sideControled)
                            {
                                GlobalDefinitions.SwitchLocalControl(true);
                            }
                            else
                            {
                                SendMessageToRemoteComputer(GlobalDefinitions.PASSCONTROLKEYWORK);
                                GlobalDefinitions.SwitchLocalControl(false);
                            }
                        }
                        else
                            GlobalDefinitions.WriteToLogFile("ERROR - TransportScript update(): Checking for data load complete - unknown message - " + message);

                        break;

                    case NetworkEventType.Nothing:
                        break;

                    default:
                        GlobalDefinitions.WriteToLogFile("ERROR - TransportScript update(): Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        break;
                }
            }
        }
    }

    /// <summary>
    /// This is the routine that runs when the connect button on the gui is clicked
    /// </summary>
    /// <param name="opponentIPaddr"></param>
    /// <returns></returns>
    public static bool Connect(string opponentIPaddr)
    {
        NetworkTransport.Init();
        gameConnectionId = NetworkTransport.Connect(remoteGameComputerId, opponentIPaddr, remoteGamePort, 0, out recievedError);
        GlobalDefinitions.WriteToLogFile("Connect: gameConnectionId = " + gameConnectionId);

        if (gameConnectionId <= 0)
            return (false);
        else
            return (true);
    }

    public static bool FileTransferConnect(string ipAddress)
    {
        //NetworkTransport.Init();
        fileTransferConnectionId = NetworkTransport.Connect(remoteGameComputerId, ipAddress, remoteGamePort, 0, out recievedError);
        GlobalDefinitions.WriteToLogFile("FileTransferConnect: fileTransferConnectionId = " + fileTransferConnectionId);

        if (fileTransferConnectionId <= 0)
            return (false);
        else
            return (true);
    }

    public static void SendFileTransferMessageToRemoteComputer(string message)
    {
        Stream stream = new MemoryStream(sendBuffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, message);
        NetworkTransport.Send(remoteFileTransferComputerId, fileTransferConnectionId, reliableChannelId, sendBuffer, BUFFERSIZE, out sendError);
        GlobalDefinitions.WriteToLogFile("SendFileTransferMessageToRemoteComputer message - " + message + " hostId=" + recievedHostId + "  communicationChannel=" + recievedConnectionId + " Error: " + (NetworkError)sendError);

        if ((NetworkError)sendError != NetworkError.Ok)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("ERROR IN TRANSMISSION - Network Error returned = " + (NetworkError)sendError);
        }
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
            //NetworkTransport.Send(recievedHostId, recievedConnectionId, reliableChannelId, sendBuffer, BUFFERSIZE, out sendError);
            NetworkTransport.Send(remoteGameComputerId, gameConnectionId, reliableChannelId, sendBuffer, BUFFERSIZE, out sendError);
            GlobalDefinitions.WriteToLogFile("Sending message - " + message + " hostId=" + remoteGameComputerId + "  communicationChannel=" + gameConnectionId + " Error: " + (NetworkError)sendError);

            if ((NetworkError)sendError != NetworkError.Ok)
            {
                GlobalDefinitions.GuiUpdateStatusMessage("ERROR IN TRANSMISSION - Network Error returned = " + (NetworkError)sendError);
            }
        }
        else
        {
            GlobalDefinitions.WriteToLogFile("ERROR - SendSocketMessage - Connection hasn't been confirmed message not sent: " + message + "  " + DateTime.Now.ToString("h:mm:ss tt"));
        }
    }

    /// <summary>
    /// This routine checks if the two computers are in agreement about who is initiating the game
    /// </summary>
    /// <param name="message"></param>
    public static void checkForHandshakeReceipt(string message)
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
                handshakeConfirmed = true;
            }
            else
            {
                handshakeConfirmed = false;
            }
        }
    }

    /// <summary>
    /// This executes when a disconnect event is received
    /// </summary>
    /// <param name="hostId"></param>
    public static void ResetConnection(int hostId)
    {
        byte error;

        GlobalDefinitions.SwitchLocalControl(false);
        TransportScript.remoteComputerIPAddress = "";
        GlobalDefinitions.userIsIntiating = false;
        GlobalDefinitions.userIsNotInitiating = false;
        GlobalDefinitions.isServer = false;
        GlobalDefinitions.hasReceivedConfirmation = false;
        GlobalDefinitions.gameStarted = false;
        channelRequested = false;
        connectionConfirmed = false;
        handshakeConfirmed = false;
        opponentComputerConfirmsSync = false;
        gameDataSent = false;

        GlobalDefinitions.WriteToLogFile("ResetConnection: sending disconnect");
        NetworkTransport.Disconnect(hostId, gameConnectionId, out error);

        if ((hostId != localGameComputerId) && (hostId != remoteGameComputerId))
            GlobalDefinitions.WriteToLogFile("ERROR - resetConnection: Request recieved to disconnect unknown host id - " + hostId);
    }

    private static void processNetworkEvent(NetworkEventType currentNetworkEvent)
    {
        switch (currentNetworkEvent)
        {
            case NetworkEventType.ConnectEvent:
                connectionConfirmed = true;
                channelRequested = true;    // Since this is can be executed by the computer that isn't requesting a channel it isn't symantically correct 
                                            // but it needs to be set

                // In the case of the server, this is needed to know the address and ports of the client computer.
                SendMessageToRemoteComputer("RemoteIPAddress " + localComputerIPAddress + " " + localGamePort + " " + localFileTransferPort);
                SendMessageToRemoteComputer("ConfirmSync");

                break;

            case NetworkEventType.DataEvent:
                char[] delimiterChars = { ' ' };
                Stream stream = new MemoryStream(recievedBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;
                OnData(recievedHostId, recievedConnectionId, recievedChannelId, message, recievedDataSize, (NetworkError)recievedError);
                string[] switchEntries = message.Split(delimiterChars);

                // Check for confirmation
                switch (switchEntries[0])
                {
                    case "ConfirmSync":
                        GlobalDefinitions.WriteToLogFile("Update: remote computer confirms sync through data message");
                        opponentComputerConfirmsSync = true;

                        // Send out the handshake message
                        if (GlobalDefinitions.userIsIntiating)
                            SendMessageToRemoteComputer("InControl");
                        else
                            SendMessageToRemoteComputer("NotInControl");
                        break;

                    case "InControl":
                    case "NotInControl":
                        checkForHandshakeReceipt(message);
                        break;

                    case "RemoteIPAddress":
                        remoteComputerIPAddress = switchEntries[1];
                        remoteGamePort = Convert.ToInt32(switchEntries[2]);
                        remoteFileTransferPort = Convert.ToInt32(switchEntries[3]);
                        break;

                    default:
                        GlobalDefinitions.WriteToLogFile("ERROR - TransportScript update(): unknown data message recevied - " + message);
                        break;
                }
                break;

            case NetworkEventType.DisconnectEvent:
                GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                ResetConnection(recievedHostId);
                break;
            case NetworkEventType.Nothing:
                break;
            default:
                GlobalDefinitions.WriteToLogFile("ERROR - TransportScript update()1: Unknown network event type received - " + currentNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                break;
        }
    }

    /// <summary>
    /// Checks for an incoming message and returns the event type of the message.  For data the message parameter gets set to the message data.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static NetworkEventType checkForNetworkEvent(out string message)
    {
        message = null;
        NetworkEventType receivedNetworkEvent = NetworkTransport.Receive(out recievedHostId, out recievedConnectionId, out recievedChannelId, recievedBuffer, BUFFERSIZE, out recievedDataSize, out recievedError);

        switch (receivedNetworkEvent)
        {
            case NetworkEventType.DisconnectEvent:
                {
                    GlobalDefinitions.WriteToLogFile("checkForNetworkEvent: Disconnect event received");
                    GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                    ResetConnection(recievedHostId);

                    // Since the connetion has been broken, quit the game and go back to the main menu
                    GameObject guiButtonInstance = new GameObject("GUIButtonInstance");
                    guiButtonInstance.AddComponent<GUIButtonRoutines>();
                    guiButtonInstance.GetComponent<GUIButtonRoutines>().YesMain();

                    break;
                }

            case NetworkEventType.DataEvent:
                {
                    Stream stream = new MemoryStream(TransportScript.recievedBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    message = formatter.Deserialize(stream) as string;
                    GlobalDefinitions.WriteToLogFile("Date Event Received: (hostId = " + recievedHostId + ", connectionId = "
                            + recievedConnectionId + ", channelId = " + recievedChannelId + ", data = "
                            + message + ", size = " + recievedDataSize + ", error = " + recievedError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

                    break;
                }

            case NetworkEventType.Nothing:
                {
                    break;
                }

            default:
                {
                    GlobalDefinitions.WriteToLogFile("GameControl Update(): Unknown network message type received: " + receivedNetworkEvent);
                    break;
                }
        }

        return (receivedNetworkEvent);

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
        GlobalDefinitions.WriteToLogFile("Date Event Received: (hostId = " + hostId + ", connectionId = "
            + connectionId + ", channelId = " + channelId + ", data = "
            + message + ", size = " + size + ", error = " + error.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
    }
}
