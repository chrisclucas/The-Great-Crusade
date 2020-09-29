using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheGreatCrusade
{
    public class MainMenuRoutines : MonoBehaviour
    {
        public static GameObject germanToggle;
        public static GameObject alliedToggle;
        public static GameObject LANGameToggle;
        public static GameObject WWWGameToggle;
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

        public static GameObject emailToggle;

        /// <summary>
        /// Pulls up a gui for the user to select the type of game mode
        /// </summary>
        public static void GetGameModeUI()
        {
            Button okButton;
            GameObject tempText;

            // Set the localControl to true in order to enable the buttons and toggles
            GlobalDefinitions.localControl = true;

            float panelWidth = 6 * GlobalDefinitions.GUIUNITIMAGESIZE;
            float panelHeight = 5 * GlobalDefinitions.GUIUNITIMAGESIZE;
            Canvas getGameModeCanvas = new Canvas();
            GlobalDefinitions.CreateGUICanvas("GameModeCanvas",
                    panelWidth,
                    panelHeight,
                    ref getGameModeCanvas);

            // This gui has two columns, selection toggles and desription
            tempText = GlobalDefinitions.CreateUIText("Select", "gameModeSelectText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    panelHeight - 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            tempText = GlobalDefinitions.CreateUIText("Game Mode", "gameModeDescriptionText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    panelHeight - 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // Now list the four game modes
            hotseatToggle = GlobalDefinitions.CreateToggle("hostseatToggle",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    panelHeight - 1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getGameModeCanvas);

            tempText = GlobalDefinitions.CreateUIText("Hot-seat", "hotseatDescriptionText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    panelHeight - 1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            hotseatToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
            hotseatToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => hotseatToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());

            AIToggle = GlobalDefinitions.CreateToggle("AIToggle",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    panelHeight - 2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getGameModeCanvas);

            tempText = GlobalDefinitions.CreateUIText("Play against Computer", "AIDescriptionText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    panelHeight - 2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            AIToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
            AIToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => AIToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());


            peerToPeerNetworkToggle = GlobalDefinitions.CreateToggle("PeerToPeerNetworkToggle",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    panelHeight - 3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getGameModeCanvas);
            tempText = GlobalDefinitions.CreateUIText("Peer to Peer network play", "PeerToPeerNetworkDescriptionText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    panelHeight - 3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            peerToPeerNetworkToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
            peerToPeerNetworkToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => peerToPeerNetworkToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());

            // Add an OK button
            okButton = GlobalDefinitions.CreateButton("getGameModeOKButton", "OK",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 3 - (0.5f * panelWidth),
                    panelHeight - 4.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getGameModeCanvas);
            okButton.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
            okButton.onClick.AddListener(okButton.GetComponent<GameModeSelectionButtonRoutines>().OkGameMode);
        }

        /// <summary>
        /// This routine pulls up a gui to allow the user to set the network settings
        /// </summary>
        //public static void NetworkSettingsUI()
        //{
        //    Button cancelButton;
        //    GameObject tempText;

        //    float panelWidth = 10 * GlobalDefinitions.GUIUNITIMAGESIZE;
        //    float panelHeight = 7 * GlobalDefinitions.GUIUNITIMAGESIZE;
        //    Canvas networkSettingsCanvas = new Canvas();
        //    GlobalDefinitions.CreateGUICanvas("NetworkSettingsCanvas",
        //            panelWidth,
        //            panelHeight,
        //            ref networkSettingsCanvas);

        //    // Add an OK button
        //    okButton = GlobalDefinitions.CreateButton("networkSettingsOKButton", "OK",
        //            8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    okButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    okButton.onClick.AddListener(okButton.GetComponent<NetworkSettingsButtonRoutines>().OkNetworkSettings);

        //    // Add a Cancel button
        //    cancelButton = GlobalDefinitions.CreateButton("networkSettingsCancelButton", "Cancel",
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    cancelButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    cancelButton.onClick.AddListener(cancelButton.GetComponent<NetworkSettingsButtonRoutines>().CancelNetworkSettings);

        //    // Get opponent ip address from the user
        //    tempText = GlobalDefinitions.CreateUIText("Enter opponents IP address", "opponentIPAddrLabelText",
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            2 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        //    opponentIPaddr = GlobalDefinitions.CreateInputField("opponentIPAddrText",
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    opponentIPaddr.onEndEdit.AddListener(delegate { NetworkSettingsButtonRoutines.ExecuteConnect(); });
        //    opponentIPaddr.interactable = false;

        //    // Display the local ip address
        //    GlobalDefinitions.CreateUIText("LAN", "LANGameText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    LANGameToggle = GlobalDefinitions.CreateToggle("LANGameToggle",
        //    2 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //    3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //    networkSettingsCanvas);
        //    LANGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    LANGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => LANGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().LANGameSelection());

        //    GlobalDefinitions.CreateUIText("WWW", "WWWGameText",
        //    GlobalDefinitions.GUIUNITIMAGESIZE,
        //    GlobalDefinitions.GUIUNITIMAGESIZE,
        //    3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //    3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //    networkSettingsCanvas);

        //    WWWGameToggle = GlobalDefinitions.CreateToggle("WWWGameToggle",
        //    4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //    3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //    networkSettingsCanvas);
        //    WWWGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    WWWGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => WWWGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().WWWGameSelection());

        //    tempText = GlobalDefinitions.CreateUIText("This computer IP address = " + TransportScript.localComputerIPAddress, "localIPAddrText",
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        //    // Determine if new or saved game
        //    GlobalDefinitions.CreateUIText("Game:", "GameLabelText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    newGameToggle = GlobalDefinitions.CreateToggle("NewGameToggle",
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    newGameToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        //    newGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    newGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => newGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().NewGameSelection());

        //    GlobalDefinitions.CreateUIText("New", "NewGameText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    savedGameToggle = GlobalDefinitions.CreateToggle("SavedGameToggle",
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    savedGameToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        //    savedGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    savedGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => savedGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().SavedGameSelection());

        //    GlobalDefinitions.CreateUIText("Saved", "SavedGameText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            7 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    // Set which side the player will play
        //    GlobalDefinitions.CreateUIText("Side:", "SideLabelText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    germanToggle = GlobalDefinitions.CreateToggle("GermanSideToggle",
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    germanToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        //    germanToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    germanToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => germanToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().GermanSelection());

        //    GlobalDefinitions.CreateUIText("German", "GermanSideLabelText",
        //            1.1f * GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    alliedToggle = GlobalDefinitions.CreateToggle("AlliedSideToggle",
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    alliedToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        //    alliedToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    alliedToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => alliedToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().AlliedSelection());

        //    GlobalDefinitions.CreateUIText("Allied", "AlliedSideLabelText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            7 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    // Ask the user if he is initiating the game
        //    tempText = GlobalDefinitions.CreateUIText("Are you initiating the game?", "initiatingGameYesNoText",
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        //    yesInitiateButton = GlobalDefinitions.CreateButton("initiatingGameYesButton", "Yes",
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    yesInitiateButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    yesInitiateButton.onClick.AddListener(yesInitiateButton.GetComponent<NetworkSettingsButtonRoutines>().YesInitiate);
        //    noInitiateButton = GlobalDefinitions.CreateButton("initiatingGameNoButton", "No",
        //            8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    noInitiateButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    noInitiateButton.onClick.AddListener(yesInitiateButton.GetComponent<NetworkSettingsButtonRoutines>().NoInitiate);
        //}
    }
}