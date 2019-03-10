using UnityEngine;
using UnityEngine.UI;

public class InvasionSelectionToggleRoutines : MonoBehaviour
{
    public int index;

    public void InvadedAreaSelected()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.INVASIONAREASELECTIONKEYWORD + " " + name);
            GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().SetInvasionArea(index);

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedInvasionState>().ExecuteSelectUnit;

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        }
    }
}
