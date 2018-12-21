using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarpetBombingSelectionToggleRoutines : MonoBehaviour
{
    public List<GameObject> defendingUnits;
    public List<GameObject> attackingUnits;
    public string combatOdds;
    public int dieRollResult;
    public GlobalDefinitions.CombatResults combatResults;
    public Vector2 buttonLocation;

    public void carpetBombingResultsSelected()
    {
        if (gameObject.GetComponent<Toggle>().isOn)
        {
            if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (!GlobalDefinitions.localControl))
            {
                GlobalDefinitions.removeGUI(transform.parent.gameObject);
                CombatResolutionRoutines.executeCombatResults(defendingUnits, attackingUnits, combatOdds, dieRollResult, combatResults, buttonLocation);
            }
            else
            {
                GlobalDefinitions.writeToCommandFile(GlobalDefinitions.COMBATRESOLUTIONSELECTEDKEYWORD + " " + GlobalDefinitions.CombatResultToggleName);
                GlobalDefinitions.writeToCommandFile(GlobalDefinitions.CARPETBOMBINGRESULTSSELECTEDKEYWORD + " " + name + " " + dieRollResult);
                GlobalDefinitions.removeGUI(transform.parent.gameObject);
                CombatResolutionRoutines.executeCombatResults(defendingUnits, attackingUnits, combatOdds, dieRollResult, combatResults, buttonLocation);

            }
        }
    }
}
