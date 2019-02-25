﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Peer2PeerRoutines : MonoBehaviour
{
    public static GameObject germanToggle;
    public static GameObject alliedToggle;
    public static GameObject newGameToggle;
    public static GameObject savedGameToggle;
    public static Button yesInitiateButton;
    public static Button noInitiateButton;
    public static Button connectButton;
    public static Button okButton;
    public static InputField opponentIPaddr;
    public static bool playNewGame = false;
    public static bool playSavedGame = false;

    public const int BUFFERSIZE = 1024; // started with 512
    private static int reliableChannelId;
    private static int unreliableChannelId;
    private static int socketPort = 5016;

    private static int connectionId = -1;

    private static int serverSocket = -1;
    private static int clientSocket = -1;

    public static bool connectionConfirmed = false;
    public static bool handshakeConfirmed = false;
    public static bool opponentComputerConfirmsSync = false;
    public static bool gameDataSent = false;

    private static byte sendError;
    private static byte[] sendBuffer = new byte[BUFFERSIZE];

    public static int recHostId;
    public static int recConnectionId;
    public static int recChannelId;
    public static byte[] recBuffer = new byte[BUFFERSIZE];
    public static int dataSize;
    public static byte recError;

    private static string fileName;

    private void Start()
    {
        GlobalDefinitions.WriteToLogFile("Executing Peer2PeerRoutines Start()");
    }

    void Update()
    {
        // This update() executes up until the game data is loaded and everything is set up.  Then the GameControl update() takes over.
        if ((NetworkRoutines.channelEstablished) && (!GlobalDefinitions.gameStarted) && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork))
        {
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);
            // This goes from the intial connect attempt to the confirmation from the remote computer
            if (NetworkRoutines.channelEstablished && !opponentComputerConfirmsSync)
            {
                //GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1: executing");
                //GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1:    gameStarted - " + GlobalDefinitions.gameStarted);
                //GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1:    channelEstablished - " + NetworkRoutines.channelEstablished);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1:    opponentComputerConfirmsSync - " + opponentComputerConfirmsSync);
                //GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1:    handshakeConfirmed - " + handshakeConfirmed);
                //GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1:    gameDataSent - " + gameDataSent);
                // Check if there is a network event

                switch (recNetworkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1: OnConnect: (hostId = " + recHostId + ", connectionId = " + recConnectionId + ", error = " + recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1: Setting connectionConfirmed to true");
                        connectionConfirmed = true;
                        NetworkRoutines.remoteComputerId = recHostId;
                        NetworkRoutines.remoteConnectionId = recConnectionId;

                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1: connect event, sending message - ConfirmSync");
                        NetworkRoutines.SendMessageToRemoteComputer("ConfirmSync");

                        break;

                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                        NetworkRoutines.ResetConnection(recHostId);
                        break;

                    case NetworkEventType.DataEvent:
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1: data event");
                        Stream stream = new MemoryStream(recBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        NetworkRoutines.OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);

                        // The fact that we have received a message is a sync
                        if (message == "ConfirmSync")
                        {
                            opponentComputerConfirmsSync = true;
                            GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1: Confirmed sync with remote computer = " + message);

                            // Send out the handshake message
                            if (GlobalDefinitions.userIsIntiating)
                                NetworkRoutines.SendMessageToRemoteComputer("InControl");
                            else
                                NetworkRoutines.SendMessageToRemoteComputer("NotInControl");
                        }
                        else
                            GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1: Expecting ConfirmSync and received = " + message);
                        break;

                    case NetworkEventType.Nothing:
                        break;
                    default:
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()1: Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        break;
                }
            }

            // This executes until the two computers agree on who is intiating the game
            else if (opponentComputerConfirmsSync && !handshakeConfirmed)
            {
                // Check if there is a network event
                //NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()2: executing");
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()2:    channelEstablished - " + NetworkRoutines.channelEstablished);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()2:    opponentComputerConfirmsSync - " + NetworkRoutines.opponentComputerConfirmsSync);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()2:    handshakeConfirmed - " + NetworkRoutines.handshakeConfirmed);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()2:    gameDataSent - " + NetworkRoutines.gameDataSent);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()2:    gameStarted - " + GlobalDefinitions.gameStarted);

                switch (recNetworkEvent)
                {
                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                        NetworkRoutines.ResetConnection(recHostId);
                        break;

                    case NetworkEventType.DataEvent:
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()2: data event");
                        Stream stream = new MemoryStream(recBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        NetworkRoutines.OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);
                        NetworkRoutines.CheckForHandshakeReceipt(message);
                        break;

                    case NetworkEventType.Nothing:
                        break;

                    default:
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()2:Checking for handshake: Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        break;
                }
            }

            // Executes from confirmation of handshake to sending of game data
            else if (handshakeConfirmed && !gameDataSent)
            {

                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()3: executing");
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()3:    channelEstablished - " + NetworkRoutines.channelEstablished);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()3:    opponentComputerConfirmsSync - " + NetworkRoutines.opponentComputerConfirmsSync);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()3:    handshakeConfirmed - " + NetworkRoutines.handshakeConfirmed);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()3:    gameDataSent - " + NetworkRoutines.gameDataSent);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()3:    gameStarted - " + GlobalDefinitions.gameStarted);

                GlobalDefinitions.chatPanel.SetActive(true);
                GameObject.Find("ChatInputField").SetActive(true);
                GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));  // Get rid of the gui, we don't need it if we got here.
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()3: Computers in sync - Waiting on intial data load");
                GlobalDefinitions.GuiUpdateStatusMessage("Waiting on intial data load");

                gameDataSent = true;
                if (GlobalDefinitions.userIsIntiating)
                {
                    // Playing a new game
                    if (Peer2PeerRoutines.playNewGame)
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
                            NetworkRoutines.SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " Allied");
                        }
                        else
                        {
                            // Pass control to the remote computer
                            NetworkRoutines.SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " German");
                            GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()3: passing control to remote computer");
                            NetworkRoutines.SendMessageToRemoteComputer(GlobalDefinitions.PASSCONTROLKEYWORK);
                            GlobalDefinitions.SwitchLocalControl(false);
                        }

                        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.PLAYNEWGAMEKEYWORD + " " + GlobalDefinitions.germanSetupFileUsed);
                    }
                    // Playing a saved game
                    else
                    {
                        string savedFileName = "";
                        GlobalDefinitions.SwitchLocalControl(true);
                        savedFileName = GlobalDefinitions.GuiFileDialog();
                        fileName = savedFileName;

                        if (GlobalDefinitions.sideControled == GlobalDefinitions.Nationality.German)
                            NetworkRoutines.SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " Allied");
                        else
                            NetworkRoutines.SendMessageToRemoteComputer(GlobalDefinitions.PLAYSIDEKEYWORD + " German");

                        // Call the routine to read a saved file
                        //GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().readTurnFile(savedFileName); // Note this will set the currentState based on the saved file

                        GlobalDefinitions.GuiUpdateStatusMessage("Peer2PeerRoutines Update()3: Waiting on remote data load...");

                        // If this is a network game send the file name to the remote computer so it can be requested through the file transfer routines.  It's silly that 
                        // I have to tell it what to ask for but I bought the code and that is how it works
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines Update()3: GameMode = " + GlobalDefinitions.gameMode + " localControl" + GlobalDefinitions.localControl);
                        if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork))
                        {
                            GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines Update()3: Sending file name to remote computer");
                            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.SENDTURNFILENAMEWORD + " " + savedFileName);
                        }

                        //GlobalDefinitions.writeToLogFile("TranportScript: setting gameDataSent to ture");
                        //gameDataSent = true;
                    }
                }
                else
                {
                    GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines Update()3:Computer is not initiating game - setting gameStarted to true and localControl to false");
                    // The non-initiating computer will move on to game mode since the read of the game data is conducted with gameStarted set
                    GlobalDefinitions.gameStarted = true;
                    GlobalDefinitions.SwitchLocalControl(false);
                }
            }

            // Last section before turning over to game play
            else if (gameDataSent)
            {

                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()4: executing");
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()4:    channelEstablished - " + NetworkRoutines.channelEstablished);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()4:    opponentComputerConfirmsSync - " + NetworkRoutines.opponentComputerConfirmsSync);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()4:    handshakeConfirmed - " + NetworkRoutines.handshakeConfirmed);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()4:    gameDataSent - " + NetworkRoutines.gameDataSent);
                GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()4:    gameStarted - " + GlobalDefinitions.gameStarted);

                // Check if there is a network event
                //NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, BUFFERSIZE, out dataSize, out recError);

                switch (recNetworkEvent)
                {
                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.GuiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        GlobalDefinitions.RemoveGUI(GameObject.Find("NetworkSettingsCanvas"));
                        NetworkRoutines.ResetConnection(recHostId);
                        break;

                    case NetworkEventType.DataEvent:
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()4: data event");
                        char[] delimiterChars = { ' ' };

                        Stream stream = new MemoryStream(recBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        NetworkRoutines.OnData(recHostId, recConnectionId, recChannelId, message, dataSize, (NetworkError)recError);
                        string[] switchEntries = message.Split(delimiterChars);

                        if (switchEntries[0] == GlobalDefinitions.GAMEDATALOADEDKEYWORD)
                        {
                            GlobalDefinitions.GuiUpdateStatusMessage("Peer2PeerRoutines Update()4:Remote data load complete - read the file sent = " + fileName);

                            // Call the routine to read a saved file
                            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadTurnFile(fileName); // Note this will set the currentState based on the saved file

                            GlobalDefinitions.gameStarted = true;
                            if (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.sideControled)
                            {
                                GlobalDefinitions.SwitchLocalControl(true);
                            }
                            else
                            {
                                NetworkRoutines.SendMessageToRemoteComputer(GlobalDefinitions.PASSCONTROLKEYWORK);
                                GlobalDefinitions.SwitchLocalControl(false);
                            }
                        }
                        else
                            GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update()4: Checking for data load complete - unknown message - " + message);

                        break;

                    case NetworkEventType.Nothing:
                        break;

                    default:
                        GlobalDefinitions.WriteToLogFile("Peer2PeerRoutines update() 4: Unknown network event type received - " + recNetworkEvent + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        break;
                }
            }
        }
    }

    public void InitiatePeerConnection()
    {
        NetworkRoutines.remoteComputerId = NetworkRoutines.NetworkInit();
    }


    /// <summary>
    /// This routine pulls up a gui to allow the user to set the network settings
    /// </summary>
    public void PeerToPeerNetworkSettingsUI()
    {
        Button cancelButton;
        GameObject tempText;

        float panelWidth = 10 * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 7 * GlobalDefinitions.GUIUNITIMAGESIZE;
        Canvas networkSettingsCanvas = new Canvas();
        GlobalDefinitions.CreateGUICanvas("NetworkSettingsCanvas",
                panelWidth,
                panelHeight,
                ref networkSettingsCanvas);

        // Add an OK button
        okButton = GlobalDefinitions.CreateButton("networkSettingsOKButton", "OK",
                8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        okButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        okButton.onClick.AddListener(okButton.GetComponent<NetworkSettingsButtonRoutines>().OkNetworkSettings);

        // Add a Cancel button
        cancelButton = GlobalDefinitions.CreateButton("networkSettingsCancelButton", "Cancel",
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        cancelButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        cancelButton.onClick.AddListener(cancelButton.GetComponent<NetworkSettingsButtonRoutines>().CancelNetworkSettings);

        // Get opponent ip address from the user
        tempText = GlobalDefinitions.CreateText("Enter opponents IP address", "opponentIPAddrLabelText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                2 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        opponentIPaddr = GlobalDefinitions.CreateInputField("opponentIPAddrText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        opponentIPaddr.onEndEdit.AddListener(delegate { NetworkSettingsButtonRoutines.ExecuteConnect(); });
        opponentIPaddr.interactable = false;

        // Display the local ip address
        tempText = GlobalDefinitions.CreateText("Local IP address = " + GlobalDefinitions.GetLocalPublicIPAddress(), "localIPAddrText",
                5 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        // Determine if new or saved game
        GlobalDefinitions.CreateText("Game:", "GameLabelText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        newGameToggle = GlobalDefinitions.CreateToggle("NewGameToggle",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        newGameToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        newGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        newGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => newGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().NewGameSelection());

        GlobalDefinitions.CreateText("New", "NewGameText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        savedGameToggle = GlobalDefinitions.CreateToggle("SavedGameToggle",
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        savedGameToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        savedGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        savedGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => savedGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().SavedGameSelection());

        GlobalDefinitions.CreateText("Saved", "SavedGameText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                7 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        // Set which side the player will play
        GlobalDefinitions.CreateText("Side:", "SideLabelText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        germanToggle = GlobalDefinitions.CreateToggle("GermanSideToggle",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        germanToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        germanToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        germanToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => germanToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().GermanSelection());

        GlobalDefinitions.CreateText("German", "GermanSideLabelText",
                1.1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        alliedToggle = GlobalDefinitions.CreateToggle("AlliedSideToggle",
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        alliedToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        alliedToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        alliedToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => alliedToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().AlliedSelection());

        GlobalDefinitions.CreateText("Allied", "AlliedSideLabelText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                7 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        // Ask the user if he is initiating the game
        tempText = GlobalDefinitions.CreateText("Are you initiating the game?", "initiatingGameYesNoText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        yesInitiateButton = GlobalDefinitions.CreateButton("initiatingGameYesButton", "Yes",
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        yesInitiateButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        yesInitiateButton.onClick.AddListener(yesInitiateButton.GetComponent<NetworkSettingsButtonRoutines>().YesInitiate);
        noInitiateButton = GlobalDefinitions.CreateButton("initiatingGameNoButton", "No",
                8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        noInitiateButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        noInitiateButton.onClick.AddListener(yesInitiateButton.GetComponent<NetworkSettingsButtonRoutines>().NoInitiate);
    }
}