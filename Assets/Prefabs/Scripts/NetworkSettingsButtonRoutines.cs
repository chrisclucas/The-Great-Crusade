using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;

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
        GlobalDefinitions.userIsNotInitiating = false;

        // Since this computer is intiating, I need to find an open port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        GlobalDefinitions.WriteToLogFile("YesInitiate: open port found = " + port);
        TransportScript.localGamePort = port;
        TransportScript.remoteGamePort = TransportScript.defaultGamePort;
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
        MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().interactable = false;
        GlobalDefinitions.userIsNotInitiating = true;
        GlobalDefinitions.userIsIntiating = false;
        MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text = "";
        GameObject.Find("initiatingGameNoButton").GetComponent<Button>().interactable = false;
        GameObject.Find("initiatingGameYesButton").GetComponent<Button>().interactable = false;

        TransportScript.localGamePort = TransportScript.defaultGamePort;
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

            if (MainMenuRoutines.germanToggle.GetComponent<Toggle>().isOn || MainMenuRoutines.alliedToggle.GetComponent<Toggle>().isOn)
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
        // If the user is not initiating, then just exit out since the next step is to wait for a connection request
        if (GlobalDefinitions.userIsNotInitiating)
        {
            TransportScript.NetworkInit();
            GlobalDefinitions.GuiUpdateStatusMessage("Waiting on connection request");
            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        }

        else if (!TransportScript.channelRequested)
        {
            GlobalDefinitions.opponentIPAddress = MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text;
            if (MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text.Length > 0)
            {
                TransportScript.NetworkInit();
                if (TransportScript.Connect(MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text))
                {
                    TransportScript.channelRequested = true;
                    GlobalDefinitions.GuiUpdateStatusMessage("Connection with Remote Computer Requested");
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
        {
            GlobalDefinitions.opponentIPAddress = MainMenuRoutines.opponentIPaddr.GetComponent<InputField>().text;
        }
    }
}
