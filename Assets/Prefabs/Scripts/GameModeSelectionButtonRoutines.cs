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
            if (GetComponent<Toggle>() != MainMenuRoutines.networkToggle.GetComponent<Toggle>())
                MainMenuRoutines.networkToggle.GetComponent<Toggle>().isOn = false;
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
            GameControl.createStatesForHotSeatOrNetwork();
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize(GameControl.inputMessage.GetComponent<InputMessage>());
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(GameControl.inputMessage.GetComponent<InputMessage>());
            GlobalDefinitions.gameStarted = true;
            GlobalDefinitions.localControl = true;
            GlobalDefinitions.removeGUI(transform.parent.gameObject);

        }
        else if (MainMenuRoutines.AIToggle.GetComponent<Toggle>().isOn)
        {
            // Note: unlike hotseat or network the state transitions in AI are determined by the side being played so the state creation is called in the button routines
            // that are invoked by the user selecting the side to play
            GlobalDefinitions.writeToLogFile("okGameMode: Setting up AI mode");
            GlobalDefinitions.askUserWhichSideToPlay();
            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
        else if (MainMenuRoutines.networkToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.writeToLogFile("okGameMode: Setting up Network mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.Network;
            GameControl.createStatesForHotSeatOrNetwork();
            GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().initiateFileTransferServer();
            MainMenuRoutines.networkSettingsUI();

            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
        else if (MainMenuRoutines.emailToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.writeToLogFile("okGameMode: Setting up eMail mode");
            GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.EMail;
            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
    }
}
