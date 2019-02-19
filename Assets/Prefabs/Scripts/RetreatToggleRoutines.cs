using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetreatToggleRoutines : MonoBehaviour
{
    public GameObject unit;

    /// <summary>
    /// Called when a unit is selected from the gui to retreat when there are multiple units avaialble
    /// </summary>
    public void SelectUnitsToMove()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.RETREATSELECTIONKEYWORD + " " + name);
            
            // The unit has been selected so move it to the zero position in the list since that is what will be moved
            GlobalDefinitions.retreatingUnits.Remove(GetComponent<RetreatToggleRoutines>().unit);
            GlobalDefinitions.retreatingUnits.Insert(0, GetComponent<RetreatToggleRoutines>().unit);

            List<GameObject> retreatHexes = CombatResolutionRoutines.ReturnRetreatHexes(GetComponent<RetreatToggleRoutines>().unit);
            if (retreatHexes.Count > 0)
            {
                GlobalDefinitions.HighlightUnit(unit);
                foreach (GameObject hex in retreatHexes)
                    GlobalDefinitions.HighlightHexForMovement(hex);
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteRetreatMovement;
            }

            // This executes when there is no retreat available for the unit.  While the units without retreat available is checked early on,
            // this is a case where there was more than one unit that needed to retreat but there wasn't room for all of them
            else
            {
                GlobalDefinitions.GuiUpdateStatusMessage("No retreat available - eliminating unit" + unit.name);
                GlobalDefinitions.MoveUnitToDeadPile(unit);
                GlobalDefinitions.retreatingUnits.RemoveAt(0);

                // Need to call selection routines in case there are more units that cannot retreat
                CombatResolutionRoutines.SelectUnitsForRetreat();
            }
            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        }
    }
}
