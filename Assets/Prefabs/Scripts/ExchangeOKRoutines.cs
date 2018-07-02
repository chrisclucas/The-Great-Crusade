using System.Collections.Generic;
using UnityEngine;

public class ExchangeOKRoutines : MonoBehaviour
{
    public List<GameObject> defendingUnits;
    public List<GameObject> attackingUnits;
    public bool attackerHadMostFactors;

    public void exchangeOKSelected()
    {
        List<GameObject> unitsToDelete = new List<GameObject>();
        // Determine if the user has selected enough factors
        if (GlobalDefinitions.exchangeFactorsSelected >= GlobalDefinitions.exchangeFactorsToLose)
        {
            if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
                TransportScript.SendSocketMessage(GlobalDefinitions.OKEXCHANGEKEYWORD + " " + name);
            
            GlobalDefinitions.writeToLogFile("exchangeOKSelected: attackerHadMostFactors = " + attackerHadMostFactors + " Units selected for exchange:");
            foreach (GameObject unit in GlobalDefinitions.unitsToExchange)
            {
                GlobalDefinitions.writeToLogFile("    unit " + unit.name);
                unitsToDelete.Add(unit);
            }

            if (attackerHadMostFactors)
            {
                foreach (GameObject unit in defendingUnits)
                {
                    GlobalDefinitions.unhighlightUnit(unit);
                    GlobalDefinitions.moveUnitToDeadPile(unit);
                }
                defendingUnits.Clear();

                foreach (GameObject unit in unitsToDelete)
                {
                    attackingUnits.Remove(unit); // This is needed to see if there are any attackers left at the end for post-combat movement
                    GlobalDefinitions.unhighlightUnit(unit);
                    GlobalDefinitions.moveUnitToDeadPile(unit);
                }
            }
            else
            {
                foreach (GameObject unit in attackingUnits)
                {
                    GlobalDefinitions.unhighlightUnit(unit);
                    GlobalDefinitions.moveUnitToDeadPile(unit);
                }
                attackingUnits.Clear();

                foreach (GameObject unit in unitsToDelete)
                {
                    GlobalDefinitions.unhighlightUnit(unit);
                    GlobalDefinitions.moveUnitToDeadPile(unit);
                }
            }

            GlobalDefinitions.removeGUI(GlobalDefinitions.ExchangeGUIInstance);

            if (attackerHadMostFactors && (GlobalDefinitions.hexesAvailableForPostCombatMovement.Count > 0) && (attackingUnits.Count > 0))
                CombatResolutionRoutines.selectUnitsForPostCombatMovement(attackingUnits);
            else
                // The CombatResolution table will be activated after the post movement combat units are selected which is why this check is needed.
                GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);

            GlobalDefinitions.unitsToExchange.Clear();
        }
        else
            GlobalDefinitions.writeToLogFile("exchangeOKSelected: ERROR - Not enough factors selected");
    }
}
