using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;


public class TransportScript : MonoBehaviour
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
    public static bool opponentComputerConfirmsSync = false;
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
    public static void networkInit()
    {
        byte error;

        GlobalDefinitions.WriteToLogFile("TransportScript networkInit(): executing");
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
            GlobalDefinitions.WriteToLogFile("networkInit: server socket set to " + serverSocket + " - disconnecting and resetting to -1");
            NetworkTransport.Disconnect(serverSocket, connectionId, out error);
            serverSocket = -1;
        }
        if (clientSocket != -1)
        {
            GlobalDefinitions.WriteToLogFile("networkInit: client socket set to " + clientSocket + " - disconnecting and resetting to -1");
            NetworkTransport.Disconnect(clientSocket, connectionId, out error);
            clientSocket = -1;
        }

        serverSocket = NetworkTransport.AddHost(topology, socketPort);
        clientSocket = NetworkTransport.AddHost(topology);
        GlobalDefinitions.WriteToLogFile("networkInit: set server socket = " + serverSocket);
        GlobalDefinitions.WriteToLogFile("networkInit: set client socket = " + clientSocket);
    }

    void Start()
    {
        //GlobalDefinitions.writeToLogFile("TransportScript executing start()");
        //networkInit();
    }

    void Update()
    {
        // This update() executes up until the game data is loaded and everything is set up.  Then the GameControl update() takes over.
        if (!GlobalDefinitions.gameStarted)
        {
            // This goes from the intial connect attempt to the confirmation from the remote computer
            if (channelEstablished && !opponentComputerConfirmsSync)
            {
                // Check if there is a network event
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

                GlobalDefinitions.WriteToLogFile("TransportScript update()1: executing");
                GlobalDefinitions.WriteToLogFile("TransportScript update()1:    channelEstablished - " + TransportScript.channelEstablished);
                GlobalDefinitions.WriteToLogFile("TransportScript update()1:    opponentComputerConfirmsSync - " + TransportScript.opponentComputerConfirmsSync);
                GlobalDefinitions.WriteToLogFile("TransportScript update()1:    handshakeConfirmed - " + TransportScript.handshakeConfirmed);
                GlobalDefinitions.WriteToLogFile("TransportScript update()1:    gameDataSent - " + TransportScript.gameDataSent);
                GlobalDefinitions.WriteToLogFile("TransportScript update()1:    gameStarted - " + GlobalDefinitions.gameStarted);

                switch (recNetworkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                        GlobalDefinitions.WriteToLogFile("TransportScript update()1: OnConnect: (hostId = " + recHostId + ", connectionId = " + recConnectionId + ", error = " + recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        GlobalDefinitions.WriteToLogFile("TransportScript update()1: Setting connectionConfirmed to true");
                        connectionConfirmed = true;
                        GlobalDefinitions.communicationSocket = recHostId;
                        GlobalDefinitions.communicationChannel = recConnectionId;

                        GlobalDefinitions.WriteToLogFile("TransportScript update()1: connect event, sending message - ConfirmSync");
                        SendSocketMessage("ConfirmSync");

                        break;

                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                        resetConnection(recHostId);
                        break;

                    case NetworkEventType.DataEvent:
                        GlobalDefinitions.WriteToLogFile("TransportScript update()1: data event");
                        Stream stream = new MemoryStream(recBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);

                        // The fact that we have received a message is a sync
                        if (message == "ConfirmSync")
                        {
                            opponentComputerConfirmsSync = true;
                            GlobalDefinitions.WriteToLogFile("TransportScript update()1: Confirmed sync with remote computer = " + message);

                            // Send out the handshake message
                            if (GlobalDefinitions.userIsIntiating)
                                SendSocketMessage("InControl");
                            else
                                SendSocketMessage("NotInControl");
                        }
                        else
                            GlobalDefinitions.WriteToLogFile("TransportScript update()1: Expecting ConfirmSync and received = " + message);
                        break;

                    case NetworkEventType.Nothing:
                        break;
                    default:
                        GlobalDefinitions.WriteToLogFile("TransportScript update()1: Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        break;
                }
            }

            // This executes until the two computers agree on who is intiating the game
            else if (opponentComputerConfirmsSync && !handshakeConfirmed)
            {
                // Check if there is a network event
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

                GlobalDefinitions.WriteToLogFile("TransportScript update()2: executing");
                GlobalDefinitions.WriteToLogFile("TransportScript update()2:    channelEstablished - " + TransportScript.channelEstablished);
                GlobalDefinitions.WriteToLogFile("TransportScript update()2:    opponentComputerConfirmsSync - " + TransportScript.opponentComputerConfirmsSync);
                GlobalDefinitions.WriteToLogFile("TransportScript update()2:    handshakeConfirmed - " + TransportScript.handshakeConfirmed);
                GlobalDefinitions.WriteToLogFile("TransportScript update()2:    gameDataSent - " + TransportScript.gameDataSent);
                GlobalDefinitions.WriteToLogFile("TransportScript update()2:    gameStarted - " + GlobalDefinitions.gameStarted);

                switch (recNetworkEvent)
                {
                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                        resetConnection(recHostId);
                        break;

                    case NetworkEventType.DataEvent:
                        GlobalDefinitions.WriteToLogFile("TransportScript update()2: data event");
                        Stream stream = new MemoryStream(recBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);
                        checkForHandshakeReceipt(message);
                        break;

                    case NetworkEventType.Nothing:
                        break;

                    default:
                        GlobalDefinitions.WriteToLogFile("TransportScript update()2:Checking for handshake: Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        break;
                }
            }

            // Executes from confirmation of handshake to sending of game data
            else if (handshakeConfirmed && !gameDataSent)
            {

                GlobalDefinitions.WriteToLogFile("TransportScript update()3: executing");
                GlobalDefinitions.WriteToLogFile("TransportScript update()3:    channelEstablished - " + TransportScript.channelEstablished);
                GlobalDefinitions.WriteToLogFile("TransportScript update()3:    opponentComputerConfirmsSync - " + TransportScript.opponentComputerConfirmsSync);
                GlobalDefinitions.WriteToLogFile("TransportScript update()3:    handshakeConfirmed - " + TransportScript.handshakeConfirmed);
                GlobalDefinitions.WriteToLogFile("TransportScript update()3:    gameDataSent - " + TransportScript.gameDataSent);
                GlobalDefinitions.WriteToLogFile("TransportScript update()3:    gameStarted - " + GlobalDefinitions.gameStarted);

                GlobalDefinitions.chatPanel.SetActive(true);
                GameObject.Find("ChatInputField").SetActive(true);
                GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));  // Get rid of the gui, we don't need it if we got here.
                GlobalDefinitions.WriteToLogFile("TransportScript update()3: Computers in sync - Waiting on intial data load");
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
                            SendSocketMessage(GlobalDefinitions.PLAYSIDEKEYWORD + " Allied");
                        }
                        else
                        {
                            // Pass control to the remote computer
                            SendSocketMessage(GlobalDefinitions.PLAYSIDEKEYWORD + " German");
                            GlobalDefinitions.WriteToLogFile("TransportScript update()3: passing control to remote computer");
                            SendSocketMessage(GlobalDefinitions.PASSCONTROLKEYWORK);
                            GlobalDefinitions.SwitchLocalControl(false);
                        }

                        SendSocketMessage(GlobalDefinitions.PLAYNEWGAMEKEYWORD + " " + GlobalDefinitions.germanSetupFileUsed);
                    }
                    // Playing a saved game
                    else
                    {
                        string savedFileName = "";
                        GlobalDefinitions.SwitchLocalControl(true);
                        savedFileName = GlobalDefinitions.GuiFileDialog();
                        fileName = savedFileName;

                        if (GlobalDefinitions.sideControled == GlobalDefinitions.Nationality.German)
                            SendSocketMessage(GlobalDefinitions.PLAYSIDEKEYWORD + " Allied");
                        else
                            SendSocketMessage(GlobalDefinitions.PLAYSIDEKEYWORD + " German");

                        // Call the routine to read a saved file
                        GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadTurnFile(savedFileName); // Note this will set the currentState based on the saved file

                        GlobalDefinitions.GuiUpdateStatusMessage("TransportScript Update()3: Waiting on remote data load...");

                        // If this is a network game send the file name to the remote computer so it can be requested through the file transfer routines.  It's silly that 
                        // I have to tell it what to ask for but I bought the code and that is how it works
                        GlobalDefinitions.WriteToLogFile("TransportScript Update()3: GameMode = " + GlobalDefinitions.gameMode + " localControl" + GlobalDefinitions.localControl);
                        if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
                        {
                            GlobalDefinitions.WriteToLogFile("TransportScript Update()3: Sending file name to remote computer");
                            SendSocketMessage(GlobalDefinitions.SENDTURNFILENAMEWORD + " " + savedFileName);
                        }

                        GlobalDefinitions.WriteToLogFile("TranportScript: setting gameDataSent to ture");
                        gameDataSent = true;
                    }
                }
                else
                {
                    GlobalDefinitions.WriteToLogFile("TransportScript Update()3:Computer is not initiating game - setting gameStarted to true and localControl to false");
                    // The non-initiating computer will move on to game mode since the read of the game data is conducted with gameStarted set
                    GlobalDefinitions.gameStarted = true;
                    GlobalDefinitions.SwitchLocalControl(false);
                }
            }

            // Last section before turning over to game play
            else if (gameDataSent)
            {

                GlobalDefinitions.WriteToLogFile("TransportScript update()4: executing");
                GlobalDefinitions.WriteToLogFile("TransportScript update()4:    channelEstablished - " + TransportScript.channelEstablished);
                GlobalDefinitions.WriteToLogFile("TransportScript update()4:    opponentComputerConfirmsSync - " + TransportScript.opponentComputerConfirmsSync);
                GlobalDefinitions.WriteToLogFile("TransportScript update()4:    handshakeConfirmed - " + TransportScript.handshakeConfirmed);
                GlobalDefinitions.WriteToLogFile("TransportScript update()4:    gameDataSent - " + TransportScript.gameDataSent);
                GlobalDefinitions.WriteToLogFile("TransportScript update()4:    gameStarted - " + GlobalDefinitions.gameStarted);

                // Check if there is a network event
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

                switch (recNetworkEvent)
                {
                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                        resetConnection(recHostId);
                        break;

                    case NetworkEventType.DataEvent:
                        GlobalDefinitions.WriteToLogFile("TransportScript update()4: data event");
                        char[] delimiterChars = { ' ' };

                        Stream stream = new MemoryStream(recBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);
                        string[] switchEntries = message.Split(delimiterChars);

                        if (switchEntries[0] == GlobalDefinitions.GAMEDATALOADEDKEYWORD)
                        {
                            GlobalDefinitions.GuiUpdateStatusMessage("TransportScript Update()4:Remote data load complete - read the file sent = " + fileName);

                            // Call the routine to read a saved file
                            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadTurnFile(fileName); // Note this will set the currentState based on the saved file

                            GlobalDefinitions.gameStarted = true;
                            if (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.sideControled)
                            {
                                GlobalDefinitions.SwitchLocalControl(true);
                            }
                            else
                            {
                                SendSocketMessage(GlobalDefinitions.PASSCONTROLKEYWORK);
                                GlobalDefinitions.SwitchLocalControl(false);
                            }
                        }
                        else
                            GlobalDefinitions.WriteToLogFile("TransportScript update()4: Checking for data load complete - unknown message - " + message);

                        break;

                    case NetworkEventType.Nothing:
                        break;

                    default:
                        GlobalDefinitions.WriteToLogFile("TransportScript update() 4: Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
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
        //if (!FirewallRoutines.IsPortOpen(socketPort))
        //    FirewallRoutines.OpenPort(socketPort, "TGC Network Communication");
        if (!channelEstablished)
        {
            byte error;

            NetworkTransport.Init();

            GlobalDefinitions.WriteToLogFile("Initial Connection(clientSocket (hostId) = " + clientSocket + ", IP addr = " + opponentIPaddr + ", socketPort = " + socketPort + ", error = )" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

            connectionId = NetworkTransport.Connect(clientSocket, opponentIPaddr, socketPort, 0, out error);

            GlobalDefinitions.WriteToLogFile("Initial Connection(clientSocket (hostId) = " + clientSocket + ", IP addr = " + opponentIPaddr + ", socketPort = " + socketPort + ", error = " + error.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

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
    public static void SendSocketMessage(string message)
    {
        if (connectionConfirmed)
        {
            Stream stream = new MemoryStream(sendBuffer);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);
            NetworkTransport.Send(GlobalDefinitions.communicationSocket, GlobalDefinitions.communicationChannel, reliableChannelId, sendBuffer, BUFFERSIZE, out sendError);
            GlobalDefinitions.WriteToLogFile("Sending message - " + message + " serverSocket=" + GlobalDefinitions.communicationSocket + "  communicationChannel=" + GlobalDefinitions.communicationChannel + " Error: " + (NetworkError)sendError);

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

    public void sendHandshakeMessage()
    {
        if (GlobalDefinitions.userIsIntiating)
        {
            GlobalDefinitions.WriteToLogFile("sendHandshakeMessage: sending InControl");
            SendSocketMessage("InControl");
        }
        else
        {
            GlobalDefinitions.WriteToLogFile("sendHandshakeMessage: sending NotInControl");
            SendSocketMessage("NotInControl");
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

    public static void resetConnection(int hostId)
    {
        byte error;

        GlobalDefinitions.WriteToLogFile("TransportScript.resetConnection: (hostId = " + hostId + ", connectionId = "
                + recConnectionId + ", error = " + recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));

        // Send a disconnect command to the remote computer
        SendSocketMessage(GlobalDefinitions.DISCONNECTFROMREMOTECOMPUTER);

        GlobalDefinitions.SwitchLocalControl(false);
        GlobalDefinitions.opponentIPAddress = "";
        GlobalDefinitions.userIsIntiating = false;
        GlobalDefinitions.isServer = false;
        GlobalDefinitions.hasReceivedConfirmation = false;
        GlobalDefinitions.gameStarted = false;
        TransportScript.channelEstablished = false;
        TransportScript.connectionConfirmed = false;
        TransportScript.handshakeConfirmed = false;
        TransportScript.opponentComputerConfirmsSync = false;
        TransportScript.gameDataSent = false;

        if (hostId == serverSocket)
        {
            NetworkTransport.Disconnect(serverSocket, connectionId, out error);
            GlobalDefinitions.WriteToLogFile("resetConnection: NetworkTransport.Disconnect(serverSocket=" + serverSocket + ", connectionId=" + connectionId + ", error = " + ((NetworkError)error).ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
        }
        else if (hostId == clientSocket)
        {
            NetworkTransport.Disconnect(clientSocket, connectionId, out error);
            GlobalDefinitions.WriteToLogFile("resetConnection: NetworkTransport.Disconnect(clientSocket=" + clientSocket + ", connectionId=" + connectionId + ", error = " + ((NetworkError)error).ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
            MainMenuRoutines.NetworkSettingsUI();
        }
        else
            GlobalDefinitions.WriteToLogFile("resetConnectin: Request recieved to disconnect unknown host - " + hostId);

        networkInit();
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
