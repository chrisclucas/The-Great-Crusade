using UnityEngine;
using UnityEngine.UI;

public class MainMenuRoutines : MonoBehaviour
{
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
}
