using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetreatToggleRoutines : MonoBehaviour
{
    public GameObject unit;

    /// <summary>
    /// Called when a unit is selected from the gui to retreat when there are multiple units avaialble
    /// </summary>
    public void selectUnitsToMove()
    {
        if (GetComponent<Toggle>().isOn)
        {
            if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
                TransportScript.SendSocketMessage(GlobalDefinitions.RETREATSELECTIONKEYWORD + " " + name);
            
            // The unit has been selected so move it to the zero position in the list since that is what will be moved
            GlobalDefinitions.retreatingUnits.Remove(GetComponent<RetreatToggleRoutines>().unit);
            GlobalDefinitions.retreatingUnits.Insert(0, GetComponent<RetreatToggleRoutines>().unit);

            List<GameObject> retreatHexes = CombatResolutionRoutines.returnRetreatHexes(GetComponent<RetreatToggleRoutines>().unit);
            if (retreatHexes.Count > 0)
            {
                GlobalDefinitions.highlightUnit(unit);
                foreach (GameObject hex in retreatHexes)
                    GlobalDefinitions.highlightHexForMovement(hex);
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<CombatState>().executeRetreatMovement;
            }

            // This executes when there is no retreat available for the unit.  While the units without retreat available is checked early on,
            // this is a case where there was more than one unit that needed to retreat but there wasn't room for all of them
            else
            {
                GlobalDefinitions.guiUpdateStatusMessage("No retreat available - eliminating unit" + unit.name);
                GlobalDefinitions.moveUnitToDeadPile(unit);
                GlobalDefinitions.retreatingUnits.RemoveAt(0);

                // Need to call selection routines in case there are more units that cannot retreat
                CombatResolutionRoutines.selectUnitsForRetreat();
            }
            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
    }
}
