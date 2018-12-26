using UnityEngine;

public class ChooseSideButtonRoutines : MonoBehaviour
{
    /// <summary>
    /// This routine executes when the player hits the button to indicate that they will be playing the Allied side
    /// </summary>
    public void allyButtonSelected()
    {
        GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.Allied;
        GlobalDefinitions.commandFileHeader += " Allied";
        GameControl.createStatesForAI(GlobalDefinitions.Nationality.Allied);
        GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.AI;
        GlobalDefinitions.gameStarted = true;
        GlobalDefinitions.localControl = true;
        GlobalDefinitions.removeGUI(transform.parent.gameObject);

        // Call the setup routine.  The user will indicate whether they are playing a saved or new game there.
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = GameControl.germanAISetupStateInstance.GetComponent<GermanAISetupState>();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
        // Note there is no execute method for the German AI state
    }

    /// <summary>
    /// This routine executes when the player hits the button to indicate that they will be playing the German side
    /// </summary>
    public void germanButtonSelected()
    {
        GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.German;
        GlobalDefinitions.commandFileHeader += " German";
        GameControl.createStatesForAI(GlobalDefinitions.Nationality.German);
        GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.AI;
        GlobalDefinitions.gameStarted = true;
        GlobalDefinitions.localControl = true;
        GlobalDefinitions.removeGUI(transform.parent.gameObject);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(GameControl.inputMessage.GetComponent<InputMessage>());
    }
}
