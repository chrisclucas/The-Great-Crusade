using UnityEngine;
using UnityEngine.UI;

public class NetworkSettingsButtonRoutines : MonoBehaviour
{

    /// <summary>
    /// This is the routine that executes when the player is initiating the game
    /// </summary>
    public void yesInitiate()
    {
        MainMenuRoutines.alliedToggle.GetComponent<Toggle>().interactable = true;
        MainMenuRoutines.germanToggle.GetComponent<Toggle>().interactable = true;
        MainMenuRoutines.newGameToggle.GetComponent<Toggle>().interactable = true;
        MainMenuRoutines.savedGameToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.userIsIntiating = true;
        if (TransportScript.channelEstablished)
        {
            // This executes when the channel is established but the two computers have the same intiating state
            GlobalDefinitions.userIsIntiating = true;
        }
    }

    /// <summary>
    /// This is the routine that executes when the player is not initiating the game
    /// The only thing the player has to enter is the opponent ip addr
    /// </summary>
    public void noInitiate()
    {
        MainMenuRoutines.alliedToggle.GetComponent<Toggle>().interactable = false;
        MainMenuRoutines.germanToggle.GetComponent<Toggle>().interactable = false;
        MainMenuRoutines.newGameToggle.GetComponent<Toggle>().interactable = false;
        MainMenuRoutines.savedGameToggle.GetComponent<Toggle>().interactable = false;
        MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().interactable = true;
        if (TransportScript.channelEstablished)
        {
            // This executes when the channel is established but the two computers have the same intiating state
            GlobalDefinitions.userIsIntiating = false;
        }
    }

    /// <summary>
    /// Executes when player toggles the German selection
    /// </summary>
    public void germanSelection()
    {
        if (MainMenuRoutines.germanToggle.GetComponent<Toggle>().isOn == true)
        {
            MainMenuRoutines.alliedToggle.GetComponent<Toggle>().isOn = false;
            GlobalDefinitions.sideControled = GlobalDefinitions.Nationality.German;

            if (MainMenuRoutines.newGameToggle.GetComponent<Toggle>().isOn || MainMenuRoutines.savedGameToggle.GetComponent<Toggle>().isOn)
                MainMenuRoutines.opponentIPaddr.interactable = true;
        }
        else
            MainMenuRoutines.opponentIPaddr.interactable = false;
    }

    /// <summary>
    /// Executes when player toggles the Allied selection
    /// </summary>
    public void alliedSelection()
    {
        if (MainMenuRoutines.alliedToggle.GetComponent<Toggle>().isOn == true)
        {
            MainMenuRoutines.germanToggle.GetComponent<Toggle>().isOn = false;
            GlobalDefinitions.sideControled = GlobalDefinitions.Nationality.Allied;

            if (MainMenuRoutines.newGameToggle.GetComponent<Toggle>().isOn || MainMenuRoutines.savedGameToggle.GetComponent<Toggle>().isOn)
                MainMenuRoutines.opponentIPaddr.interactable = true;
        }
        else
            MainMenuRoutines.opponentIPaddr.interactable = false;
    }

    /// <summary>
    /// Executes when player selects to play a new game
    /// </summary>
    public void newGameSelection()
    {
        if (MainMenuRoutines.newGameToggle.GetComponent<Toggle>().isOn == true)
        {
            MainMenuRoutines.savedGameToggle.GetComponent<Toggle>().isOn = false;
            MainMenuRoutines.playNewGame = true;
            MainMenuRoutines.playSavedGame = false;

            if (MainMenuRoutines.germanToggle.GetComponent<Toggle>().isOn || MainMenuRoutines.alliedToggle.GetComponent<Toggle>().isOn)
                MainMenuRoutines.opponentIPaddr.interactable = true;
        }
        else
        {
            MainMenuRoutines.playNewGame = false;
            MainMenuRoutines.opponentIPaddr.interactable = false;
        }
    }

    public void savedGameSelection()
    {
        if (MainMenuRoutines.savedGameToggle.GetComponent<Toggle>().isOn == true)
        {
            MainMenuRoutines.newGameToggle.GetComponent<Toggle>().isOn = false;
            MainMenuRoutines.playSavedGame = true;
            MainMenuRoutines.playNewGame = false;

            if (MainMenuRoutines.germanToggle.GetComponent<Toggle>().isOn || MainMenuRoutines.alliedToggle.GetComponent<Toggle>()  .isOn)
                MainMenuRoutines.opponentIPaddr.interactable = true;
        }
        else
        {
            MainMenuRoutines.playSavedGame = false;
            MainMenuRoutines.opponentIPaddr.interactable = false;
        }
    }

    public void okNetworkSettings()
    {
        GlobalDefinitions.opponentIPAddress = MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text;
        if (TransportScript.channelEstablished)
        {
            // This executes when the channel is established but the two computers have the same intiating state
            if (GlobalDefinitions.userIsIntiating)
            {
                GlobalDefinitions.writeToLogFile("okNetworkSettings: sending message InControl");
                TransportScript.SendSocketMessage("InControl");
                GlobalDefinitions.userIsIntiating = true;
                GlobalDefinitions.writeToLogFile("okNetworkSettings: checkForHandshakeReceipt(NotInControl)");
                TransportScript.checkForHandshakeReceipt("NotInControl");
            }
            else
            {
                GlobalDefinitions.writeToLogFile("okNetworkSettings: sending message NotInControl");
                TransportScript.SendSocketMessage("NotInControl");
                GlobalDefinitions.userIsIntiating = false;
                GlobalDefinitions.writeToLogFile("okNetworkSettings: checkForHandshakeReceipt(InControl)");
                TransportScript.checkForHandshakeReceipt("InControl");
            }
        }
        else
        {
            if (MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text.Length > 0)
            {
                if (TransportScript.Connect(MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text))
                {
                    TransportScript.channelEstablished = true;
                    GlobalDefinitions.writeToLogFile("okNetworkSettings: Channel Established");
                    Debug.Log("Channel Established");
                    GlobalDefinitions.guiUpdateStatusMessage("Channel Established");
                }
                else
                    GlobalDefinitions.guiUpdateStatusMessage("Connection Failed");
            }
            else
                GlobalDefinitions.guiUpdateStatusMessage("No IP address entered");
        }
    }

    /// <summary>
    /// This is the routine executes on clicking Cancel which brings up the Game Selection UI again
    /// </summary>
    public void cancelNetworkSettings()
    {
        GlobalDefinitions.removeGUI(transform.parent.gameObject);
        MainMenuRoutines.getGameModeUI();
    }

    /// <summary>
    /// IP address entered
    /// </summary>
    public static void executeConnect()
    {
        if (MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text.Length > 0)
        {
            GlobalDefinitions.opponentIPAddress = MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text;
            if (TransportScript.Connect(MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text))
            {
                TransportScript.channelEstablished = true;
                GlobalDefinitions.guiUpdateStatusMessage("Channel Established");
            }
            else
                GlobalDefinitions.guiUpdateStatusMessage("Connection Failed");
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("No IP address entered");
    }
}
