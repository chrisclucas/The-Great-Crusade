using UnityEngine;
using UnityEngine.UI;

namespace TheGreatCrusade
{
    public class InvasionSelectionToggleRoutines : MonoBehaviour
    {
        public int index;

        public void InvadedAreaSelected()
        {
            if (GetComponent<Toggle>().isOn)
            {
                GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.INVASIONAREASELECTIONKEYWORD + " " + name);
                GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().SetInvasionArea(index);

                // Turn on the gui buttons

                GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = true;
                GlobalDefinitions.undoButton.GetComponent<Button>().interactable = true;
                GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = true;
                GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = true;
                GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = true;
                GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
                GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
                GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;

                if (!GlobalDefinitions.localControl)
                    GlobalDefinitions.SetGUIForNonLocalControl();

                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedInvasionState>().ExecuteSelectUnit;

                GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            }
        }
    }
}