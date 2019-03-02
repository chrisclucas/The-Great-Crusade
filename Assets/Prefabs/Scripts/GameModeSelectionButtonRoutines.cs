using UnityEngine.UI;
using UnityEngine;

public class GameModeSelectionButtonRoutines : MonoBehaviour
{
    /// <summary>
    /// Executes when the toggle is changed on the gui
    /// </summary>
    public void ToggleChange()
    {
        if (GetComponent<Toggle>().isOn == true)
        {
            if (GetComponent<Toggle>() != MainMenuRoutines.hotseatToggle.GetComponent<Toggle>())
                MainMenuRoutines.hotseatToggle.GetComponent<Toggle>().isOn = false;
            if (GetComponent<Toggle>() != MainMenuRoutines.AIToggle.GetComponent<Toggle>())
                MainMenuRoutines.AIToggle.GetComponent<Toggle>().isOn = false;
            if (GetComponent<Toggle>() != MainMenuRoutines.peerToPeerNetworkToggle.GetComponent<Toggle>())
                MainMenuRoutines.peerToPeerNetworkToggle.GetComponent<Toggle>().isOn = false;
            if (GetComponent<Toggle>() != MainMenuRoutines.clientServerNetworkToggle.GetComponent<Toggle>())
                MainMenuRoutines.clientServerNetworkToggle.GetComponent<Toggle>().isOn = false;
            if (GetComponent<Toggle>() != MainMenuRoutines.serverNetworkToggle.GetComponent<Toggle>())
                MainMenuRoutines.serverNetworkToggle.GetComponent<Toggle>().isOn = false;
            //if (GetComponent<Toggle>() != MainMenuRoutines.emailToggle.GetComponent<Toggle>())
            //    MainMenuRoutines.emailToggle.GetComponent<Toggle>().isOn = false;
        }
    }

    /// <summary>
    /// Executes when the OK button is selected
    /// </summary>
    public void OkGameMode()
    {
        if (MainMenuRoutines.hotseatToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToLogFile("okGameMode: Setting up hotseat mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.Hotseat;
            GlobalDefinitions.commandFileHeader = "Hotseat";
            GameControl.CreateStatesForHotSeatOrNetwork();
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod(GameControl.inputMessage.GetComponent<InputMessage>());
            GlobalDefinitions.gameStarted = true;
            GlobalDefinitions.SwitchLocalControl(true);
            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);

        }
        else if (MainMenuRoutines.AIToggle.GetComponent<Toggle>().isOn)
        {
            // Note: unlike hotseat or network the state transitions in AI are determined by the side being played so the state creation is called in the button routines
            // that are invoked by the user selecting the side to play
            GlobalDefinitions.WriteToLogFile("okGameMode: Setting up AI mode");
            GlobalDefinitions.commandFileHeader = "AI";
            GlobalDefinitions.AskUserWhichSideToPlay();
            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        }
        else if (MainMenuRoutines.peerToPeerNetworkToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToLogFile("okGameMode: Setting up Peer to Peer Network mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.Peer2PeerNetwork;
            GlobalDefinitions.commandFileHeader = "Peer2PeerNetwork";
            GameControl.CreateStatesForHotSeatOrNetwork();
            GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().initiateFileTransferServer();
            MainMenuRoutines.NetworkSettingsUI();

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        }
        else if (MainMenuRoutines.clientServerNetworkToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToLogFile("okGameMode: Setting up Client Server Network mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.ClientServerNetwork;
            GlobalDefinitions.commandFileHeader = "ClientServerNetwork";
            GameControl.CreateStatesForHotSeatOrNetwork();
            GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().initiateFileTransferServer();

            GameControl.clientServerRoutinesInstance.GetComponent<ClientServerRoutines>().InitiateServerConnection();

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        }
        else if (MainMenuRoutines.serverNetworkToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToLogFile("okGameMode: Setting up Server Network mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.Server;
            GlobalDefinitions.commandFileHeader = "Server";
            GameControl.CreateStatesForHotSeatOrNetwork();
            GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().initiateFileTransferServer();

            GameControl.serverRoutinesInstance.GetComponent<ServerRoutines>().StartListening();

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        }
    }
}
