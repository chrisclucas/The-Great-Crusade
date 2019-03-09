using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class TransportScript : MonoBehaviour
{
    public const int BUFFERSIZE = 1024; // started with 512
    public static int reliableChannelId;
    public static int unreliableChannelId;
    public static int gamePort = 5016;

    public static int connectionId = -1;

    public static int serverSocket = -1;
    public static int remoteComputerId = -1;

    public static bool channelRequested = false;
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
        if (serverSocket != -1)
        {
            NetworkTransport.Disconnect(serverSocket, connectionId, out error);
            serverSocket = -1;
        }
        if (remoteComputerId != -1)
        {
            NetworkTransport.Disconnect(remoteComputerId, connectionId, out error);
            remoteComputerId = -1;
        }

        serverSocket = NetworkTransport.AddHost(topology, gamePort);
        remoteComputerId = NetworkTransport.AddHost(topology);

        return (remoteComputerId);

    }

    void Start()
    {

    }

    void Update()
    {
        // This update() executes up until the game data is loaded and everything is set up.  Then the GameControl update() takes over.
        if (!GlobalDefinitions.gameStarted)
        {
            // This is needed by the computer that is not intiating.  It will wait for a connection event.
            if (!channelRequested && GlobalDefinitions.userIsNotInitiating)
            {
                NetworkEventType recNetworkEvent;
                // Check if there is a network event

                // This try is needed since this will start executing once the user indicates that he isn't initiating the game but the
                // init of the network doesn't take place until OK is selected
                try
                {
                    recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);
                }
                catch
                {
                    return;
                }
                processNetworkEvent(recNetworkEvent);
            }

            // This goes from the intial connect attempt to the confirmation from the remote computer
            else if (channelRequested && !opponentComputerConfirmsSync)
            {
                // Check if there is a network event
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

                processNetworkEvent(recNetworkEvent);
            }

            // This executes until the two computers agree on who is intiating the game
            else if (opponentComputerConfirmsSync && !handshakeConfirmed)
            {
                // Check if there is a network event
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

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
                        }
                        else
                        {
                            // Pass control to the remote computer
                            SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " German");
                            SendMessageToRemoteComputer(GlobalDefinitions.PASSCONTROLKEYWORK);
                            GlobalDefinitions.SwitchLocalControl(false);
                        }

                        SendMessageToRemoteComputer(GlobalDefinitions.PLAYNEWGAMEKEYWORD + " " + GlobalDefinitions.germanSetupFileUsed);
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

                        GlobalDefinitions.GuiUpdateStatusMessage("TransportScript Update()3: Waiting on remote data load...");

                        // Tell the remote computer what file to load.  It will then turn around and request it
                        SendMessageToRemoteComputer(GlobalDefinitions.SENDTURNFILENAMEWORD + " " + savedFileName);

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
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

                switch (recNetworkEvent)
                {
                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                        ResetConnection(recHostId);
                        break;

                    case NetworkEventType.DataEvent:
                        char[] delimiterChars = { ' ' };

                        Stream stream = new MemoryStream(recBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);
                        string[] switchEntries = message.Split(delimiterChars);

                        if (switchEntries[0] == GlobalDefinitions.GAMEDATALOADEDKEYWORD)
                        {
                            GlobalDefinitions.GuiUpdateStatusMessage("TransportScript Update()4:Remote data load complete");

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
                            GlobalDefinitions.WriteToLogFile("ERROR - TransportScript update()4: Checking for data load complete - unknown message - " + message);

                        break;

                    case NetworkEventType.Nothing:
                        break;

                    default:
                        GlobalDefinitions.WriteToLogFile("ERROR - TransportScript update() 4: Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
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
        if (!channelRequested)
        {
            byte error;

            NetworkTransport.Init();
            connectionId = NetworkTransport.Connect(remoteComputerId, opponentIPaddr, gamePort, 0, out error);

            if (connectionId <= 0)
                return (false);
            else
                return (true);
        }
        return (true); // Connection already established
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
            NetworkTransport.Send(recHostId, recConnectionId, reliableChannelId, sendBuffer, BUFFERSIZE, out sendError);
            GlobalDefinitions.WriteToLogFile("Sending message - " + message + " hostId=" + recHostId + "  communicationChannel=" + recConnectionId + " Error: " + (NetworkError)sendError);

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
        GlobalDefinitions.opponentIPAddress = "";
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

        NetworkTransport.Disconnect(hostId, connectionId, out error);

        if ((hostId != serverSocket) && (hostId != remoteComputerId))
            GlobalDefinitions.WriteToLogFile("ERROR - resetConnecti0n: Request recieved to disconnect unknown host id - " + hostId);
    }

    private static void processNetworkEvent(NetworkEventType currentNetworkEvent)
    {
        switch (currentNetworkEvent)
        {
            case NetworkEventType.ConnectEvent:
                connectionConfirmed = true;
                channelRequested = true;    // Since this is can be executed by the computer that isn't requesting a channel it isn't symantically correct 
                                            // but it needs to be set

                // This code executes when the non-initiating computer gets a connection request.
                // The other computer doesn't have the ip address of this computer so send it since it is needed if a saved game is going to be played
                SendMessageToRemoteComputer("RemoteIPAddress " + Network.player.ipAddress);
                SendMessageToRemoteComputer("ConfirmSync");

                break;

            case NetworkEventType.DataEvent:
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;
                OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);

                // Check for confirmation
                if (message == "ConfirmSync")
                {
                    GlobalDefinitions.WriteToLogFile("Update: remote computer confirms sync through data message");
                    opponentComputerConfirmsSync = true;

                    // Send out the handshake message
                    if (GlobalDefinitions.userIsIntiating)
                        SendMessageToRemoteComputer("InControl");
                    else
                        SendMessageToRemoteComputer("NotInControl");
                }
                else if ((message == "InControl") || (message == "NotInControl"))
                {
                    checkForHandshakeReceipt(message);
                }
                else
                    GlobalDefinitions.WriteToLogFile("ERROR - TransportScript update()1: Expecting ConfirmSync and received = " + message);
                break;

            case NetworkEventType.DisconnectEvent:
                GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                ResetConnection(recHostId);
                break;
            case NetworkEventType.Nothing:
                break;
            default:
                GlobalDefinitions.WriteToLogFile("ERROR - TransportScript update()1: Unknown network event type received - " + currentNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                break;
        }
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
