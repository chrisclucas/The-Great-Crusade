using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class CombatGUIOK : MonoBehaviour
    {
        public GameObject singleCombat;

        /// <summary>
        /// This executes when the OK button on the combat assignment gui is pressed
        /// </summary>
        public void OkCombatGUISelection()
        {
            Button yesButton = null;
            Button noButton = null;
            List<GameObject> removeUnit = new List<GameObject>();

            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.COMBATGUIOKKEYWORD + " " + name);

            removeUnit.Clear();

            // Get a list of the defending units that were not added to the combat
            foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
                if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    removeUnit.Add(unit);

            // Now go through the defenders and remove non-committed units from the combat
            foreach (GameObject unit in removeUnit)
                singleCombat.GetComponent<Combat>().defendingUnits.Remove(unit);

            removeUnit.Clear();

            // Get a list of the attacking units that were not added to the combat
            foreach (GameObject unit in singleCombat.GetComponent<Combat>().attackingUnits)
                if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    removeUnit.Add(unit);

            // Now go through the attackers and remove non-committed units from the combat
            foreach (GameObject unit in removeUnit)
                singleCombat.GetComponent<Combat>().attackingUnits.Remove(unit);

            // Need to check if the user has selected both attackers and defenders.  If not then nothing should be changed
            if ((singleCombat.GetComponent<Combat>().attackingUnits.Count == 0) || (singleCombat.GetComponent<Combat>().defendingUnits.Count == 0))
                NoAbort();

            // Check for if a combat is being selected that is less than 1:6 odds - this is useless but need to check just in case
            else if (GlobalDefinitions.ConvertOddsToString(CalculateBattleOddsRoutines.ReturnCombatGUICombatOdds(
                    singleCombat.GetComponent<Combat>().defendingUnits, singleCombat.GetComponent<Combat>().attackingUnits)) == "1:7")
            {
                // If the odds or worse than 1:6 then the attackers are eliminated and no battle takes place.  It does not
                // count as an attack on the defending units
                GlobalDefinitions.AskUserYesNoQuestion("Attacking at odds less than 1:6 is useless: do you want to continue?/nNote that the attackers will be eliminated and this will not count as a combat", ref yesButton, ref noButton, YesContinue, NoAbort);
            }

            else
            {
                foreach (Transform childTransform in transform.parent.transform)
                    if ((childTransform.GetComponent<CombatToggleRoutines>() != null) &&
                            (childTransform.GetComponent<CombatToggleRoutines>().unit != null))
                        //if (childTransform.GetComponent<Toggle>().isOn && GlobalDefinitions.localControl)  I removed the local control check here because units on the remote computer and not being reset.  The local control check was added for a reason, though, and I don't know why which is why I'm leaving this here as a comment
                        if (childTransform.GetComponent<Toggle>().isOn)
                        {
                            GlobalDefinitions.UnhighlightUnit(childTransform.GetComponent<CombatToggleRoutines>().unit);
                            childTransform.GetComponent<CombatToggleRoutines>().unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
                        }

                // Check whether air support is to be used in this attack
                if (GlobalDefinitions.combatAirSupportToggle != null)
                {
                    if (GlobalDefinitions.combatAirSupportToggle.GetComponent<Toggle>().isOn)
                        singleCombat.GetComponent<Combat>().attackAirSupport = true;
                    else
                        singleCombat.GetComponent<Combat>().attackAirSupport = false;
                }

                // Check if carpet bombing is to be used in this attack
                if (GlobalDefinitions.combatCarpetBombingToggle != null)
                {
                    if (GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn)
                    {
                        GlobalDefinitions.carpetBombingUsedThisTurn = true;
                        GlobalDefinitions.numberOfCarpetBombingsUsed++;
                        singleCombat.GetComponent<Combat>().carpetBombing = true;
                        singleCombat.GetComponent<Combat>().defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().carpetBombingActive = true;
                    }
                    else
                        singleCombat.GetComponent<Combat>().carpetBombing = false;
                }

                GlobalDefinitions.allCombats.Add(singleCombat);
                GlobalDefinitions.RemoveGUI(GlobalDefinitions.combatGUIInstance);

                // Check if the Must Attack toggle is on and if it is highlight uncommitted units that must participate in an attack
                if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                        (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance") ||
                        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn)
                    CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, true);

                // Determine what state we are in and set the next executeMethod
                if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                        (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanMovementStateInstance"))
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().ExecuteSelectUnit;
                if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                        (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance"))
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteSelectUnit;
                if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedInvasionStateInstance")
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedInvasionState>().ExecuteSelectUnit;
                if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedAirborneStateInstance")
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedAirborneState>().ExecuteSelectUnit;
            }
        }

        // Cancels the combat assignment
        public void CancelCombatGUISelection()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.COMBATGUICANCELKEYWORD + " " + name);

            foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
            {
                GlobalDefinitions.UnhighlightUnit(unit);
                unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
            }

            foreach (GameObject unit in singleCombat.GetComponent<Combat>().attackingUnits)
            {
                GlobalDefinitions.UnhighlightUnit(unit);
                unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
            }

            Destroy(singleCombat);
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.combatGUIInstance);

            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance") ||
                    GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn)
                CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, true);

            // Determine what state we are in and set the next executeMethod
            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanMovementStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().ExecuteSelectUnit;
            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteSelectUnit;
            if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedInvasionStateInstance")
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedInvasionState>().ExecuteSelectUnit;
            if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedAirborneStateInstance")
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedAirborneState>().ExecuteSelectUnit;

        }

        private void YesContinue()
        {
            for (int index = 0; index < singleCombat.GetComponent<Combat>().attackingUnits.Count; index++)
                GlobalDefinitions.MoveUnitToDeadPile(singleCombat.GetComponent<Combat>().attackingUnits[index]);

            foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                {
                    unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                    GlobalDefinitions.HighlightUnit(unit);
                }

            Destroy(singleCombat);
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.combatGUIInstance);

            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
            (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteSelectUnit;
            else
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().ExecuteSelectUnit;
        }

        private void NoAbort()
        {
            foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;

            foreach (GameObject unit in singleCombat.GetComponent<Combat>().attackingUnits)
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;

            Destroy(singleCombat);
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.combatGUIInstance);

            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance"))
            {
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteSelectUnit;
                CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, true);
            }
            else
            {
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().ExecuteSelectUnit;
                CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, false);
            }
        }
    }
}