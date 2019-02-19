using UnityEngine.UI;
using UnityEngine;
using System.IO;

public class GameTypeSelectionButtonRoutines : MonoBehaviour
{
    public void ToggleChange()
    {
        if (GetComponent<Toggle>().isOn == true)
        {
            if (GetComponent<Toggle>() != GlobalDefinitions.newGameToggle.GetComponent<Toggle>())
                GlobalDefinitions.newGameToggle.GetComponent<Toggle>().isOn = false;
            if (GetComponent<Toggle>() != GlobalDefinitions.savedGameToggle.GetComponent<Toggle>())
                GlobalDefinitions.savedGameToggle.GetComponent<Toggle>().isOn = false;
            if (GetComponent<Toggle>() != GlobalDefinitions.commandFileToggle.GetComponent<Toggle>())
                GlobalDefinitions.commandFileToggle.GetComponent<Toggle>().isOn = false;
        }
    }

    public void NewSavedGameOK()
    {
        if (GlobalDefinitions.newGameToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToLogFile("newSavedGameOK: Starting new game");

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            GameControl.setUpStateInstance.GetComponent<SetUpState>().ExecuteNewGame();
        }
        else if (GlobalDefinitions.savedGameToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToLogFile("newSavedGameOK: Starting saved game");

            // Since at this point we know we are starting a new game and not running the command file, remove the command file
            if (!GlobalDefinitions.commandFileBeingRead)
                GlobalDefinitions.DeleteCommandFile();

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            GameControl.setUpStateInstance.GetComponent<SetUpState>().ExecuteSavedGame();
        }
        else if (GlobalDefinitions.commandFileToggle.GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToLogFile("newSavedGameOK: Executing command file");

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            GameControl.setUpStateInstance.GetComponent<SetUpState>().ReadCommandFile();            
        }
    }

}
