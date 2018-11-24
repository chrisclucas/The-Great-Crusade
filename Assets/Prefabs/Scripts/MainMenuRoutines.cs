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

    public static GameObject hotseatToggle;
    public static GameObject AIToggle;
    public static GameObject networkToggle;
    public static GameObject emailToggle;

    public static bool playNewGame = false;
    public static bool playSavedGame = false;

    /// <summary>
    /// Pulls up a gui for the user to select the type of game mode
    /// </summary>
    public static void getGameModeUI()
    {
        Button okButton;
        GameObject tempText;

        float panelWidth = 6 * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 5 * GlobalDefinitions.GUIUNITIMAGESIZE; // Is email game is an option this needs to change to 6
        Canvas getGameModeCanvas = new Canvas();
        GlobalDefinitions.createGUICanvas("GameModeCanvas", 
                panelWidth, 
                panelHeight, 
                ref getGameModeCanvas);

        // This gui has two columns, selection toggles and desription
        tempText = GlobalDefinitions.createText("Select", "gameModeSelectText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                4.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        tempText = GlobalDefinitions.createText("Game Mode", "gameModeDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                4.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        // Now list the four game modes
        hotseatToggle = GlobalDefinitions.createToggle("hostseatToggle",
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);

        tempText = GlobalDefinitions.createText("Hot-seat", "hotseatDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        hotseatToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        hotseatToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => hotseatToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().toggleChange());

        AIToggle = GlobalDefinitions.createToggle("AIToggle",
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);

        tempText = GlobalDefinitions.createText("Play against Computer", "AIDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        AIToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        AIToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => AIToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().toggleChange());


        networkToggle = GlobalDefinitions.createToggle("networkToggle",
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText = GlobalDefinitions.createText("Play against Internet opponent", "networkDescriptionText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        networkToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        networkToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => networkToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().toggleChange());

        networkToggle.GetComponent<Toggle>().interactable = false;

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
        okButton = GlobalDefinitions.createButton("getGameModeOKButton", "OK",
                GlobalDefinitions.GUIUNITIMAGESIZE * 3 - (0.5f * panelWidth),
                0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                getGameModeCanvas);
        okButton.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
        okButton.onClick.AddListener(okButton.GetComponent<GameModeSelectionButtonRoutines>().okGameMode);
    }

    /// <summary>
    /// This routine pulls up a gui to allow the user to set the network settings
    /// </summary>
    public static void networkSettingsUI()
    {
        Button cancelButton;
        GameObject tempText;

        float panelWidth = 10 * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 7 * GlobalDefinitions.GUIUNITIMAGESIZE;
        Canvas networkSettingsCanvas = new Canvas();
        GlobalDefinitions.createGUICanvas("NetworkSettingsCanvas",
                panelWidth,
                panelHeight,
                ref networkSettingsCanvas);

        // Add an OK button
        okButton = GlobalDefinitions.createButton("networkSettingsOKButton", "OK",
                8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        okButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        okButton.onClick.AddListener(okButton.GetComponent<NetworkSettingsButtonRoutines>().okNetworkSettings);

        // Add a Cancel button
        cancelButton = GlobalDefinitions.createButton("networkSettingsCancelButton", "Cancel",
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        cancelButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        cancelButton.onClick.AddListener(cancelButton.GetComponent<NetworkSettingsButtonRoutines>().cancelNetworkSettings);

        // Get opponent ip address from the user
        tempText = GlobalDefinitions.createText("Enter opponents IP address", "opponentIPAddrLabelText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                2 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        opponentIPaddr = GlobalDefinitions.createInputField("opponentIPAddrText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        opponentIPaddr.onEndEdit.AddListener(delegate { NetworkSettingsButtonRoutines.executeConnect(); });
        opponentIPaddr.interactable = false;

        // Display the local ip address
        tempText = GlobalDefinitions.createText("Local IP address = " + GlobalDefinitions.getLocalPublicIPAddress(), "localIPAddrText",
                5 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        // Determine if new or saved game
        GlobalDefinitions.createText("Game:", "GameLabelText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        newGameToggle = GlobalDefinitions.createToggle("NewGameToggle",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        newGameToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        newGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        newGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => newGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().newGameSelection());

        GlobalDefinitions.createText("New", "NewGameText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        savedGameToggle = GlobalDefinitions.createToggle("SavedGameToggle",
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        savedGameToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        savedGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        savedGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => savedGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().savedGameSelection());

        GlobalDefinitions.createText("Saved", "SavedGameText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                7 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        // Set which side the player will play
        GlobalDefinitions.createText("Side:", "SideLabelText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        germanToggle = GlobalDefinitions.createToggle("GermanSideToggle",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        germanToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        germanToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        germanToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => germanToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().germanSelection());

        GlobalDefinitions.createText("German", "GermanSideLabelText",
                1.1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        alliedToggle = GlobalDefinitions.createToggle("AlliedSideToggle",
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        alliedToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        alliedToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        alliedToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => alliedToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().alliedSelection());

        GlobalDefinitions.createText("Allied", "AlliedSideLabelText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                7 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);

        // Ask the user if he is initiating the game
        tempText = GlobalDefinitions.createText("Are you initiating the game?", "initiatingGameYesNoText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        yesInitiateButton = GlobalDefinitions.createButton("initiatingGameYesButton", "Yes",
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        yesInitiateButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        yesInitiateButton.onClick.AddListener(yesInitiateButton.GetComponent<NetworkSettingsButtonRoutines>().yesInitiate);
        noInitiateButton = GlobalDefinitions.createButton("initiatingGameNoButton", "No",
                8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
                6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                networkSettingsCanvas);
        noInitiateButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        noInitiateButton.onClick.AddListener(yesInitiateButton.GetComponent<NetworkSettingsButtonRoutines>().noInitiate);
    }
}
