using UnityEngine.UI;
using UnityEngine;

public class GameModeSelectionButtonRoutines : MonoBehaviour
{
    public void toggleChange()
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

    public void okGameMode()
    {
        if (MainMenuRoutines.hotseatToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.writeToLogFile("okGameMode: Setting up hotseat mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.Hotseat;
            GlobalDefinitions.commandFileHeader = "Hotseat";
            GameControl.createStatesForHotSeatOrNetwork();
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(GameControl.inputMessage.GetComponent<InputMessage>());
            GlobalDefinitions.gameStarted = true;
            GlobalDefinitions.switchLocalControl(true);
            GlobalDefinitions.removeGUI(transform.parent.gameObject);

        }
        else if (MainMenuRoutines.AIToggle.GetComponent<Toggle>().isOn)
        {
            // Note: unlike hotseat or network the state transitions in AI are determined by the side being played so the state creation is called in the button routines
            // that are invoked by the user selecting the side to play
            GlobalDefinitions.writeToLogFile("okGameMode: Setting up AI mode");
            GlobalDefinitions.commandFileHeader = "AI";
            GlobalDefinitions.askUserWhichSideToPlay();
            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
        else if (MainMenuRoutines.peerToPeerNetworkToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.writeToLogFile("okGameMode: Setting up Peer to Peer Network mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.Peer2PeerNetwork;
            GlobalDefinitions.commandFileHeader = "Peer2PeerNetwork";
            GameControl.createStatesForHotSeatOrNetwork();
            GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().initiateFileTransferServer();
            MainMenuRoutines.PeerToPeerNetworkSettingsUI();

            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
        else if (MainMenuRoutines.clientServerNetworkToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.writeToLogFile("okGameMode: Setting up Client Server Network mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.ClientServerNetwork;
            GlobalDefinitions.commandFileHeader = "ClientServerNetwork";
            GameControl.createStatesForHotSeatOrNetwork();
            GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().initiateFileTransferServer();
            ClientServerRoutines.initiateServerConnection();

            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
        else if (MainMenuRoutines.serverNetworkToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.writeToLogFile("okGameMode: Setting up Server Network mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.Server;
            GlobalDefinitions.commandFileHeader = "Server";
            GameControl.createStatesForHotSeatOrNetwork();
            GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().initiateFileTransferServer();
            ServerRoutines.

            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
    }
}
