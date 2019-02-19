using UnityEngine;

public class ChooseSideButtonRoutines : MonoBehaviour
{
    /// <summary>
    /// This routine executes when the player hits the button to indicate that they will be playing the Allied side
    /// </summary>
    public void AllyButtonSelected()
    {
        GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.Allied;
        GlobalDefinitions.commandFileHeader += " Allied";
        GameControl.CreateStatesForAI(GlobalDefinitions.Nationality.Allied);
        GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.AI;
        GlobalDefinitions.gameStarted = true;
        GlobalDefinitions.SwitchLocalControl(true);
        GlobalDefinitions.RemoveGUI(transform.parent.gameObject);

        // Call the setup routine.  The user will indicate whether they are playing a saved or new game there.
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.germanAISetupStateInstance.GetComponent<GermanAISetupState>();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
        // Note there is no execute method for the German AI state
    }

    /// <summary>
    /// This routine executes when the player hits the button to indicate that they will be playing the German side
    /// </summary>
    public void GermanButtonSelected()
    {
        GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.German;
        GlobalDefinitions.commandFileHeader += " German";
        GameControl.CreateStatesForAI(GlobalDefinitions.Nationality.German);
        GlobalDefinitions.gameMode = GlobalDefinitions.GameModeValues.AI;
        GlobalDefinitions.gameStarted = true;
        GlobalDefinitions.SwitchLocalControl(true);
        GlobalDefinitions.RemoveGUI(transform.parent.gameObject);

        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod(GameControl.inputMessage.GetComponent<InputMessage>());
    }
}
