using UnityEngine;
using UnityEngine.UI;

public class NetworkSettingsButtonRoutines : MonoBehaviour
{

    /// <summary>
    /// This is the routine that executes when the player is initiating the game
    /// </summary>
    public void YesInitiate()
    {
        Peer2PeerRoutines.alliedToggle.GetComponent<Toggle>().interactable = true;
        Peer2PeerRoutines.germanToggle.GetComponent<Toggle>().interactable = true;
        Peer2PeerRoutines.newGameToggle.GetComponent<Toggle>().interactable = true;
        Peer2PeerRoutines.savedGameToggle.GetComponent<Toggle>().interactable = true;
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
        Peer2PeerRoutines.alliedToggle.GetComponent<Toggle>().interactable = false;
        Peer2PeerRoutines.germanToggle.GetComponent<Toggle>().interactable = false;
        Peer2PeerRoutines.newGameToggle.GetComponent<Toggle>().interactable = false;
        Peer2PeerRoutines.savedGameToggle.GetComponent<Toggle>().interactable = false;
        Peer2PeerRoutines.opponentIPaddr.GetComponent<InputField>().interactable = true;
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
        if (Peer2PeerRoutines.germanToggle.GetComponent<Toggle>().isOn == true)
        {
            Peer2PeerRoutines.alliedToggle.GetComponent<Toggle>().isOn = false;
            GlobalDefinitions.sideControled = GlobalDefinitions.Nationality.German;

            if (Peer2PeerRoutines.newGameToggle.GetComponent<Toggle>().isOn || Peer2PeerRoutines.savedGameToggle.GetComponent<Toggle>().isOn)
                Peer2PeerRoutines.opponentIPaddr.interactable = true;
        }
        else
            Peer2PeerRoutines.opponentIPaddr.interactable = false;
    }

    /// <summary>
    /// Executes when player toggles the Allied selection
    /// </summary>
    public void AlliedSelection()
    {
        if (Peer2PeerRoutines.alliedToggle.GetComponent<Toggle>().isOn == true)
        {
            Peer2PeerRoutines.germanToggle.GetComponent<Toggle>().isOn = false;
            GlobalDefinitions.sideControled = GlobalDefinitions.Nationality.Allied;

            if (Peer2PeerRoutines.newGameToggle.GetComponent<Toggle>().isOn || Peer2PeerRoutines.savedGameToggle.GetComponent<Toggle>().isOn)
                Peer2PeerRoutines.opponentIPaddr.interactable = true;
        }
        else
            Peer2PeerRoutines.opponentIPaddr.interactable = false;
    }

    /// <summary>
    /// Executes when player selects to play a new game
    /// </summary>
    public void NewGameSelection()
    {
        if (Peer2PeerRoutines.newGameToggle.GetComponent<Toggle>().isOn == true)
        {
            Peer2PeerRoutines.savedGameToggle.GetComponent<Toggle>().isOn = false;
            Peer2PeerRoutines.playNewGame = true;
            Peer2PeerRoutines.playSavedGame = false;

            if (Peer2PeerRoutines.germanToggle.GetComponent<Toggle>().isOn || Peer2PeerRoutines.alliedToggle.GetComponent<Toggle>().isOn)
                Peer2PeerRoutines.opponentIPaddr.interactable = true;
        }
        else
        {
            Peer2PeerRoutines.playNewGame = false;
            Peer2PeerRoutines.opponentIPaddr.interactable = false;
        }
    }

    /// <summary>
    /// Executes when the player selects to play a saved game
    /// </summary>
    public void SavedGameSelection()
    {
        if (Peer2PeerRoutines.savedGameToggle.GetComponent<Toggle>().isOn == true)
        {
            Peer2PeerRoutines.newGameToggle.GetComponent<Toggle>().isOn = false;
            Peer2PeerRoutines.playSavedGame = true;
            Peer2PeerRoutines.playNewGame = false;

            if (Peer2PeerRoutines.germanToggle.GetComponent<Toggle>().isOn || Peer2PeerRoutines.alliedToggle.GetComponent<Toggle>().isOn)
                Peer2PeerRoutines.opponentIPaddr.interactable = true;
        }
        else
        {
            Peer2PeerRoutines.playSavedGame = false;
            Peer2PeerRoutines.opponentIPaddr.interactable = false;
        }
    }

    /// <summary>
    /// Executes when the OK button is selected
    /// </summary>
    public void OkNetworkSettings()
    {
        if (Peer2PeerRoutines.opponentIPaddr.GetComponent<InputField>().text.Length == 0)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("No IP address entered");
            return;
        }
        GlobalDefinitions.opponentIPAddress = Peer2PeerRoutines.opponentIPaddr.GetComponent<InputField>().text;
        GameControl.peer2PeerRoutinesInstance.GetComponent<Peer2PeerRoutines>().InitiatePeerConnection();

        GlobalDefinitions.WriteToLogFile("okNetworkSettings: executing  remote compter id = " + NetworkRoutines.remoteComputerId);

        if (NetworkRoutines.Connect(GlobalDefinitions.opponentIPAddress))
        {
            NetworkRoutines.channelEstablished = true;
            GlobalDefinitions.GuiUpdateStatusMessage("Channel Established");
        }
        else
            GlobalDefinitions.GuiUpdateStatusMessage("Connection Failed");
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
        if (Peer2PeerRoutines.opponentIPaddr.GetComponent<InputField>().text.Length > 0)
            GlobalDefinitions.opponentIPAddress = Peer2PeerRoutines.opponentIPaddr.GetComponent<InputField>().text;
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
