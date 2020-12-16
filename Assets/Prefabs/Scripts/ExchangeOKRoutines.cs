using System.Collections.Generic;
using UnityEngine;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class ExchangeOKRoutines : MonoBehaviour
    {
        public List<GameObject> defendingUnits;
        public List<GameObject> attackingUnits;
        public bool attackerHadMostFactors;

        public void ExchangeOKSelected()
        {
            List<GameObject> unitsToDelete = new List<GameObject>();
            // Determine if the user has selected enough factors
            if (GlobalDefinitions.exchangeFactorsSelected >= GlobalDefinitions.exchangeFactorsToLose)
            {
                GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.OKEXCHANGEKEYWORD + " " + name);

                GlobalDefinitions.WriteToLogFile("exchangeOKSelected: attackerHadMostFactors = " + attackerHadMostFactors + " Units selected for exchange:");
                foreach (GameObject unit in GlobalDefinitions.unitsToExchange)
                {
                    GlobalDefinitions.WriteToLogFile("    unit " + unit.name);
                    unitsToDelete.Add(unit);
                }

                if (attackerHadMostFactors)
                {
                    foreach (GameObject unit in defendingUnits)
                    {
                        GlobalDefinitions.UnhighlightUnit(unit);
                        GlobalDefinitions.MoveUnitToDeadPile(unit);
                    }
                    defendingUnits.Clear();

                    foreach (GameObject unit in unitsToDelete)
                    {
                        attackingUnits.Remove(unit); // This is needed to see if there are any attackers left at the end for post-combat movement
                        GlobalDefinitions.UnhighlightUnit(unit);
                        GlobalDefinitions.MoveUnitToDeadPile(unit);
                    }
                }
                else
                {
                    foreach (GameObject unit in attackingUnits)
                    {
                        GlobalDefinitions.UnhighlightUnit(unit);
                        GlobalDefinitions.MoveUnitToDeadPile(unit);
                    }
                    attackingUnits.Clear();

                    foreach (GameObject unit in unitsToDelete)
                    {
                        GlobalDefinitions.UnhighlightUnit(unit);
                        GlobalDefinitions.MoveUnitToDeadPile(unit);
                    }
                }

                GUIRoutines.RemoveGUI(GlobalDefinitions.ExchangeGUIInstance);

                if (attackerHadMostFactors && (GlobalDefinitions.hexesAvailableForPostCombatMovement.Count > 0) && (attackingUnits.Count > 0))
                    CombatResolutionRoutines.SelectUnitsForPostCombatMovement(attackingUnits);
                else
                    // The CombatResolution table will be activated after the post movement combat units are selected which is why this check is needed.
                    GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);

                GlobalDefinitions.unitsToExchange.Clear();
            }
            else
                GlobalDefinitions.WriteToLogFile("exchangeOKSelected: ERROR - Not enough factors selected");
        }
    }
}