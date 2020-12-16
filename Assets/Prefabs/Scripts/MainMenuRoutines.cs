using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonRoutines;

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
            GUIRoutines.CreateGUICanvas("GameModeCanvas",
                    panelWidth,
                    panelHeight,
                    ref getGameModeCanvas);

            // This gui has two columns, selection toggles and desription
            tempText = GUIRoutines.CreateUIText("Select", "gameModeSelectText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    panelHeight - 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            tempText = GUIRoutines.CreateUIText("Game Mode", "gameModeDescriptionText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    panelHeight - 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // Now list the three game modes
            hotseatToggle = GUIRoutines.CreateToggle("hostseatToggle",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    panelHeight - 1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getGameModeCanvas);

            tempText = GUIRoutines.CreateUIText("Hot-seat", "hotseatDescriptionText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    panelHeight - 1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            hotseatToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
            hotseatToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => hotseatToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());

            AIToggle = GUIRoutines.CreateToggle("AIToggle",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    panelHeight - 2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getGameModeCanvas);

            tempText = GUIRoutines.CreateUIText("Play against Computer", "AIDescriptionText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    panelHeight - 2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            AIToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
            AIToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => AIToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());

            peerToPeerNetworkToggle = GUIRoutines.CreateToggle("PeerToPeerNetworkToggle",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    panelHeight - 3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getGameModeCanvas);
            tempText = GUIRoutines.CreateUIText("Peer to Peer network play", "PeerToPeerNetworkDescriptionText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    panelHeight - 3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white, getGameModeCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            peerToPeerNetworkToggle.gameObject.AddComponent<GameModeSelectionButtonRoutines>();
            peerToPeerNetworkToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => peerToPeerNetworkToggle.gameObject.GetComponent<GameModeSelectionButtonRoutines>().ToggleChange());

            // Add an OK button
            okButton = GUIRoutines.CreateButton("getGameModeOKButton", "OK",
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
        //    GUIRoutines.CreateGUICanvas("NetworkSettingsCanvas",
        //            panelWidth,
        //            panelHeight,
        //            ref networkSettingsCanvas);

        //    // Add an OK button
        //    okButton = GUIRoutines.CreateButton("networkSettingsOKButton", "OK",
        //            8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    okButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    okButton.onClick.AddListener(okButton.GetComponent<NetworkSettingsButtonRoutines>().OkNetworkSettings);

        //    // Add a Cancel button
        //    cancelButton = GUIRoutines.CreateButton("networkSettingsCancelButton", "Cancel",
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    cancelButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    cancelButton.onClick.AddListener(cancelButton.GetComponent<NetworkSettingsButtonRoutines>().CancelNetworkSettings);

        //    // Get opponent ip address from the user
        //    tempText = GUIRoutines.CreateUIText("Enter opponents IP address", "opponentIPAddrLabelText",
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
        //    GUIRoutines.CreateUIText("LAN", "LANGameText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            1 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    LANGameToggle = GUIRoutines.CreateToggle("LANGameToggle",
        //    2 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //    3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //    networkSettingsCanvas);
        //    LANGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    LANGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => LANGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().LANGameSelection());

        //    GUIRoutines.CreateUIText("WWW", "WWWGameText",
        //    GlobalDefinitions.GUIUNITIMAGESIZE,
        //    GlobalDefinitions.GUIUNITIMAGESIZE,
        //    3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //    3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //    networkSettingsCanvas);

        //    WWWGameToggle = GUIRoutines.CreateToggle("WWWGameToggle",
        //    4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //    3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //    networkSettingsCanvas);
        //    WWWGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    WWWGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => WWWGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().WWWGameSelection());

        //    tempText = GUIRoutines.CreateUIText("This computer IP address = " + TransportScript.localComputerIPAddress, "localIPAddrText",
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        //    // Determine if new or saved game
        //    GUIRoutines.CreateUIText("Game:", "GameLabelText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    newGameToggle = GUIRoutines.CreateToggle("NewGameToggle",
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    newGameToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        //    newGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    newGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => newGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().NewGameSelection());

        //    GUIRoutines.CreateUIText("New", "NewGameText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    savedGameToggle = GUIRoutines.CreateToggle("SavedGameToggle",
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    savedGameToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        //    savedGameToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    savedGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => savedGameToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().SavedGameSelection());

        //    GUIRoutines.CreateUIText("Saved", "SavedGameText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            7 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    // Set which side the player will play
        //    GUIRoutines.CreateUIText("Side:", "SideLabelText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    germanToggle = GUIRoutines.CreateToggle("GermanSideToggle",
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    germanToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        //    germanToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    germanToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => germanToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().GermanSelection());

        //    GUIRoutines.CreateUIText("German", "GermanSideLabelText",
        //            1.1f * GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    alliedToggle = GUIRoutines.CreateToggle("AlliedSideToggle",
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    alliedToggle.GetComponent<Toggle>().interactable = false; // The default is to turn this off until the user indicates that he is intiating
        //    alliedToggle.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    alliedToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => alliedToggle.gameObject.GetComponent<NetworkSettingsButtonRoutines>().AlliedSelection());

        //    GUIRoutines.CreateUIText("Allied", "AlliedSideLabelText",
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            7 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            5 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);

        //    // Ask the user if he is initiating the game
        //    tempText = GUIRoutines.CreateUIText("Are you initiating the game?", "initiatingGameYesNoText",
        //            4 * GlobalDefinitions.GUIUNITIMAGESIZE,
        //            GlobalDefinitions.GUIUNITIMAGESIZE,
        //            3 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    tempText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        //    yesInitiateButton = GUIRoutines.CreateButton("initiatingGameYesButton", "Yes",
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    yesInitiateButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    yesInitiateButton.onClick.AddListener(yesInitiateButton.GetComponent<NetworkSettingsButtonRoutines>().YesInitiate);
        //    noInitiateButton = GUIRoutines.CreateButton("initiatingGameNoButton", "No",
        //            8 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelWidth),
        //            6 * GlobalDefinitions.GUIUNITIMAGESIZE - (0.5f * panelHeight),
        //            networkSettingsCanvas);
        //    noInitiateButton.gameObject.AddComponent<NetworkSettingsButtonRoutines>();
        //    noInitiateButton.onClick.AddListener(yesInitiateButton.GetComponent<NetworkSettingsButtonRoutines>().NoInitiate);
        //}
    }
}