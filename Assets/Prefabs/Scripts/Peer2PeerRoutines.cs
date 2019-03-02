using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

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

    public void InitiatePeerConnection()
    {
        NetworkRoutines.remoteComputerIPAddress = opponentIPaddr.GetComponent<InputField>().text;
        GlobalDefinitions.WriteToLogFile("InitiatePeerConnection: executing  remote compter id = " + NetworkRoutines.remoteComputerId);

        NetworkRoutines.remoteComputerId = NetworkRoutines.NetworkInit();

        if (ClientServerRoutines.ConnectToServer())
        {
            NetworkRoutines.channelEstablished = true;
            GlobalDefinitions.GuiUpdateStatusMessage("Channel Established");
            
        }
        else
            GlobalDefinitions.GuiUpdateStatusMessage("Connection Failed");
    }

    public static bool PeerConnect()
    {

        byte error;

        NetworkTransport.Init();
        NetworkRoutines.remoteConnectionId = NetworkTransport.Connect(NetworkRoutines.remoteComputerId, NetworkRoutines.remoteComputerIPAddress, NetworkRoutines.gamePort, 0, out error);

        GlobalDefinitions.WriteToLogFile("Connect: Remote Computer Id = " + NetworkRoutines.remoteComputerId + ", Remote Connection Id = " + NetworkRoutines.remoteConnectionId + ", Port = " + NetworkRoutines.gamePort + ", error = " + error.ToString() + "  " + DateTime.Now.ToString("h:mm:ss tt"));

        if (NetworkRoutines.remoteConnectionId <= 0)
            return (false);
        else
            return (true);
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
