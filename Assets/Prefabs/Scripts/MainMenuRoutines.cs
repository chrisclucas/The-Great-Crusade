using UnityEngine;
using UnityEngine.UI;

public class MainMenuRoutines : MonoBehaviour
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

    public static GameObject hotseatToggle;
    public static GameObject AIToggle;
    public static GameObject peerToPeerNetworkToggle;
    public static GameObject clientServerNetworkToggle;
    public static GameObject serverNetworkToggle;
    public static GameObject emailToggle;

    /// <summary>
    /// Pulls up a gui for the user to select the type of game mode
    /// </summary>
    public static void GetGameModeUI()
    {
        Button okButton;
        GameObject tempText;

        float panelWidth = 6 * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 7 * GlobalDefinitions.GUIUNITIMAGESIZE; // If email game is an option this needs to change to 7
        Canvas getGameModeCanvas = new Canvas();
        GlobalDefinitions.CreateGUICanvas("GameModeCanvas", 
                panelWidth, 
                panelHeight, 
                ref getGameModeCanvas);

        // This gui has two columns, selection toggles and desription
        tempText = GlobalDefinitions.CreateText("Select", "gameModeSelectText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                6.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        tempText = GlobalDefinitions.CreateText("Game Mode", "gameModeDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                6.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        // Now list the four game modes
        hotseatToggle = GlobalDefinitions.CreateToggle("hostseatToggle",
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                5.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);

        tempText = GlobalDefinitions.CreateText("Hot-seat", "hotseatDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                5.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        hotseatToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        hotseatToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => hotseatToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());

        AIToggle = GlobalDefinitions.CreateToggle("AIToggle",
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                4.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);

        tempText = GlobalDefinitions.CreateText("Play against Computer", "AIDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                4.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        AIToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        AIToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => AIToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());


        peerToPeerNetworkToggle = GlobalDefinitions.CreateToggle("PeerToPeerNetworkToggle",
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText = GlobalDefinitions.CreateText("Peer to Peer network play", "PeerToPeerNetworkDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        peerToPeerNetworkToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        peerToPeerNetworkToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => peerToPeerNetworkToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());

        //peerToPeerNetworkToggle.GetComponent<Toggle>().interactable = false;

        clientServerNetworkToggle = GlobalDefinitions.CreateToggle("ClientServerNetworkToggle",
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText = GlobalDefinitions.CreateText("Client-Server network play", "ClientServerNetworkDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        clientServerNetworkToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        clientServerNetworkToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => clientServerNetworkToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());

        serverNetworkToggle = GlobalDefinitions.CreateToggle("ServerNetworkToggle",
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText = GlobalDefinitions.CreateText("Server", "ServerDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        clientServerNetworkToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        clientServerNetworkToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => clientServerNetworkToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());


        //emailToggle = GlobalDefinitions.createToggle("emailToggle",
        //        GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
        //        1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //        getGameModeCanvas);
        //tempText = GlobalDefinitions.createText("Play against e-Mail opponent", "emailDescriptionString",
        //        4 * GlobalDefinitions.GUIUNITIMAGESIZE,
        //        GlobalDefinitions.GUIUNITIMAGESIZE,
        //        GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
        //        1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //        getGameModeCanvas);
        //tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        //emailToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        //emailToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => emailToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().toggleChange());

        // Add an OK button
        okButton = GlobalDefinitions.CreateButton("getGameModeOKButton", "OK",
                GlobalDefinitions.GUIUNITIMAGESIZE * 3 - (0.5f * panelWidth),
                0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        okButton.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        okButton.onClick.AddListener(okButton.GetComponent<GameModeSelectionButtonRoutines>().OkGameMode);
    }

    /// <summary>
    /// This routine pulls up a gui to allow the user to set the network settings
    /// </summary>
    public static void NetworkSettingsUI()
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
