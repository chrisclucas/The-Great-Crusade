using UnityEngine;
using UnityEngine.UI;

public class NetworkSettingsButtonRoutines : MonoBehaviour
{

    /// <summary>
    /// This is the routine that executes when the player is initiating the game
    /// </summary>
    public void YesInitiate()
    {
        MainMenuRoutines.alliedToggle.GetComponent<Toggle>().interactable = true;
        MainMenuRoutines.germanToggle.GetComponent<Toggle>().interactable = true;
        MainMenuRoutines.newGameToggle.GetComponent<Toggle>().interactable = true;
        MainMenuRoutines.savedGameToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.userIsIntiating = true;
        if (NetworkRoutines.channelEstablished)
        {
            // This executes when the channel is established but the two computers have the same intiating state
            GlobalDefinitions.userIsIntiating = true;
        }
    }

    /// <summary>
    /// This is the routine that executes when the player is not initiating the game
    /// The only thing the player has to enter is the opponent ip addr
    /// </summary>
    public void NoInitiate()
    {
        MainMenuRoutines.alliedToggle.GetComponent<Toggle>().interactable = false;
        MainMenuRoutines.germanToggle.GetComponent<Toggle>().interactable = false;
        MainMenuRoutines.newGameToggle.GetComponent<Toggle>().interactable = false;
        MainMenuRoutines.savedGameToggle.GetComponent<Toggle>().interactable = false;
        MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().interactable = true;
        if (NetworkRoutines.channelEstablished)
        {
            // This executes when the channel is established but the two computers have the same intiating state
            GlobalDefinitions.userIsIntiating = false;
        }
    }

    /// <summary>
    /// Executes when player toggles the German selection
    /// </summary>
    public void GermanSelection()
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
    public void AlliedSelection()
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
    public void NewGameSelection()
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

    /// <summary>
    /// Executes when the player selects to play a saved game
    /// </summary>
    public void SavedGameSelection()
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

    /// <summary>
    /// Executes when the OK button is selected
    /// </summary>
    public void OkNetworkSettings()
    {
        NetworkRoutines.NetworkInit();
        GlobalDefinitions.opponentIPAddress = MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text;

        GlobalDefinitions.WriteToLogFile("okNetworkSettings: executing");
        GlobalDefinitions.WriteToLogFile("okNetworkSettings:    channelEstablished - " + NetworkRoutines.channelEstablished);
        GlobalDefinitions.WriteToLogFile("okNetworkSettings:    gameStarted - " + GlobalDefinitions.gameStarted);
        GlobalDefinitions.WriteToLogFile("okNetworkSettings:    opponentComputerConfirmsSync - " + NetworkRoutines.opponentComputerConfirmsSync);
        GlobalDefinitions.WriteToLogFile("okNetworkSettings:    handshakeConfirmed - " + NetworkRoutines.handshakeConfirmed);
        GlobalDefinitions.WriteToLogFile("okNetworkSettings:    gameDataSent - " + NetworkRoutines.gameDataSent);


        if (NetworkRoutines.channelEstablished)
        {
            // This executes when the channel is established but the two computers have the same intiating state
            if (GlobalDefinitions.userIsIntiating)
            {
                GlobalDefinitions.WriteToLogFile("okNetworkSettings: sending message InControl");
                NetworkRoutines.SendSocketMessage("InControl");
                GlobalDefinitions.userIsIntiating = true;
                GlobalDefinitions.WriteToLogFile("okNetworkSettings: checkForHandshakeReceipt(NotInControl)");
                NetworkRoutines.CheckForHandshakeReceipt("NotInControl");
            }
            else
            {
                GlobalDefinitions.WriteToLogFile("okNetworkSettings: sending message NotInControl");
                NetworkRoutines.SendSocketMessage("NotInControl");
                GlobalDefinitions.userIsIntiating = false;
                GlobalDefinitions.WriteToLogFile("okNetworkSettings: checkForHandshakeReceipt(InControl)");
                NetworkRoutines.CheckForHandshakeReceipt("InControl");
            }
        }
        else
        {
            if (MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text.Length > 0)
            {
                if (NetworkRoutines.Connect(MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text))
                {
                    NetworkRoutines.channelEstablished = true;
                    GlobalDefinitions.WriteToLogFile("okNetworkSettings: Channel Established");
                    GlobalDefinitions.GuiUpdateStatusMessage("Channel Established");
                }
                else
                    GlobalDefinitions.GuiUpdateStatusMessage("Connection Failed");
            }
            else
                GlobalDefinitions.GuiUpdateStatusMessage("No IP address entered");
        }
    }

    /// <summary>
    /// This is the routine executes on clicking Cancel which brings up the Game Selection UI again
    /// </summary>
    public void CancelNetworkSettings()
    {
        GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        MainMenuRoutines.GetGameModeUI();
    }

    /// <summary>
    /// IP address entered
    /// </summary>
    public static void ExecuteConnect()
    {
        if (MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text.Length > 0)
            GlobalDefinitions.opponentIPAddress = MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text;
        //if (MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text.Length > 0)
        //{
        //    GlobalDefinitions.opponentIPAddress = MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text;
        //    if (TransportScript.Connect(MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text))
        //    {
        //        TransportScript.channelEstablished = true;
        //        GlobalDefinitions.guiUpdateStatusMessage("Channel Established");
        //    }
        //    else
        //        GlobalDefinitions.guiUpdateStatusMessage("Connection Failed");
        //}
        //else
        //    GlobalDefinitions.guiUpdateStatusMessage("No IP address entered");
    }
}
