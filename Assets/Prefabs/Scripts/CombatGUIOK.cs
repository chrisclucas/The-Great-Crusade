using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatGUIOK : MonoBehaviour
{
    public GameObject singleCombat;

    /// <summary>
    /// This executes when the OK button on the combat assignment gui is pressed
    /// </summary>
    public void okCombatGUISelection()
    {
        Button yesButton = null;
        Button noButton = null;
        List<GameObject> removeUnit = new List<GameObject>();

        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.COMBATGUIOKKEYWORD + " " + name);

        removeUnit.Clear();
        foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
            if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                removeUnit.Add(unit);

        // Now go through the defenders and remove non-committed units from the combat
        foreach (GameObject unit in removeUnit)
            singleCombat.GetComponent<Combat>().defendingUnits.Remove(unit);

        removeUnit.Clear();
        foreach (GameObject unit in singleCombat.GetComponent<Combat>().attackingUnits)
            if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                removeUnit.Add(unit);

        // Now go through the attackers and remove non-committed units from the combat
        foreach (GameObject unit in removeUnit)
            singleCombat.GetComponent<Combat>().attackingUnits.Remove(unit);

        // Need to check if the user has selected both attackers and defenders.  If not then nothing should be changed
        if ((singleCombat.GetComponent<Combat>().attackingUnits.Count == 0) || (singleCombat.GetComponent<Combat>().defendingUnits.Count == 0))
            noAbort();

        // Check for if a combat is being selected that is less than 1:6 odds - this is useless but need to check just in case
        else if (GlobalDefinitions.convertOddsToString(GlobalDefinitions.returnCombatGUICombatOdds(
                singleCombat.GetComponent<Combat>().defendingUnits, singleCombat.GetComponent<Combat>().attackingUnits)) == "1:7")
        {
            // If the odds or worse than 1:6 then the attackers are eliminated and no battle takes place.  It does not
            // count as an attack on the defending units
            GlobalDefinitions.askUserYesNoQuestion("Attacking at odds less than 1:6 is useless: do you want to continue?", ref yesButton, ref noButton, yesContinue, noAbort);
        }
        
        else
        {

            //GlobalDefinitions.writeToLogFile("okCombatGUISelection: ok button selected");
            foreach (Transform childTransform in transform.parent.transform)
                if ((childTransform.GetComponent<CombatToggleRoutines>() != null) &&
                        (childTransform.GetComponent<CombatToggleRoutines>().unit != null))
                    if (childTransform.GetComponent<Toggle>().isOn)
                    {
                        //GlobalDefinitions.writeToLogFile("okCombatGUISelection: committing and unhighlighting unit = " + childTransform.GetComponent<CombatToggleRoutines>().unit.name);
                        GlobalDefinitions.unhighlightUnit(childTransform.GetComponent<CombatToggleRoutines>().unit);
                        childTransform.GetComponent<CombatToggleRoutines>().unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
                    }

            if (GlobalDefinitions.combatAirSupportToggle != null)
            {
                if (GlobalDefinitions.combatAirSupportToggle.GetComponent<Toggle>().isOn)
                    singleCombat.GetComponent<Combat>().attackAirSupport = true;
                else
                    singleCombat.GetComponent<Combat>().attackAirSupport = false;
            }

            if (GlobalDefinitions.combatCarpetBombingToggle != null)
            {
                if (GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn)
                {
                    bool sameHex = true;
                    singleCombat.GetComponent<Combat>().carpetBombing = false;
                    // Need to make sure that all of the defenders are on the same hex
                    if (singleCombat.GetComponent<Combat>().defendingUnits.Count > 1)
                        for (int index = 1; index < singleCombat.GetComponent<Combat>().defendingUnits.Count; index++)
                            if (singleCombat.GetComponent<Combat>().defendingUnits[index].GetComponent<UnitDatabaseFields>().occupiedHex !=
                                    singleCombat.GetComponent<Combat>().defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex)
                                sameHex = false;

                    if (sameHex)
                    {
                        // Make sure the attack was attacked last turn
                        if (!GlobalDefinitions.hexesAttackedLastTurn.Contains(singleCombat.GetComponent<Combat>().defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex))
                        {
                            GlobalDefinitions.guiUpdateStatusMessage("Carpet bombing not allowed on hex - it was not attacked last turn");
                        }
                        else
                        {
                            GlobalDefinitions.numberOfCarpetBombingsUsed++;
                            singleCombat.GetComponent<Combat>().carpetBombing = true;
                            singleCombat.GetComponent<Combat>().defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().carpetBombingActive = true;
                        }
                    }
                    else
                        GlobalDefinitions.guiUpdateStatusMessage("Carpet bombing not allowed on hex - all units do not occupy a single hex");
                }
                else
                    singleCombat.GetComponent<Combat>().carpetBombing = false;
            }

            GlobalDefinitions.allCombats.Add(singleCombat);
            GlobalDefinitions.removeGUI(GlobalDefinitions.combatGUIInstance);

            if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance") ||
                    GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn)
                CombatRoutines.checkIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality, true);

            // Determine what state we are in and set the next executeMethod
            if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanMovementStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().executeSelectUnit;
            if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<CombatState>().executeSelectUnit;
            if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedInvasionStateInstance")
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedInvasionState>().executeSelectUnit;
            if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedAirborneStateInstance")
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedAirborneState>().executeSelectUnit;
        }
    }

    // Cancels the combat assignment
    public void cancelCombatGUISelection()
    {
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.COMBATGUICANCELKEYWORD + " " + name);

        foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
        {
            GlobalDefinitions.unhighlightUnit(unit);
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
        }
        foreach (GameObject unit in singleCombat.GetComponent<Combat>().attackingUnits)
        {
            GlobalDefinitions.unhighlightUnit(unit);
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
        }

        Destroy(singleCombat);
        GlobalDefinitions.removeGUI(GlobalDefinitions.combatGUIInstance);

        if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance") ||
                GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn)
            CombatRoutines.checkIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality, true);

        // Determine what state we are in and set the next executeMethod
        if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanMovementStateInstance"))
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().executeSelectUnit;
        if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance"))
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<CombatState>().executeSelectUnit;
        if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedInvasionStateInstance")
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedInvasionState>().executeSelectUnit;
        if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedAirborneStateInstance")
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedAirborneState>().executeSelectUnit;

    }

    private void yesContinue()
    {
        for (int index = 0; index < singleCombat.GetComponent<Combat>().attackingUnits.Count; index++)
            GlobalDefinitions.moveUnitToDeadPile(singleCombat.GetComponent<Combat>().attackingUnits[index]);

        foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
            if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
            {
                unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                GlobalDefinitions.highlightUnit(unit);
            }

        Destroy(singleCombat);
        GlobalDefinitions.removeGUI(GlobalDefinitions.combatGUIInstance);

        if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance") ||
        (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance"))
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<CombatState>().executeSelectUnit;
        else
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().executeSelectUnit;
    }
    private void noAbort()
    {
        foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
            if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;

        foreach (GameObject unit in singleCombat.GetComponent<Combat>().attackingUnits)
            if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;

        Destroy(singleCombat);
        GlobalDefinitions.removeGUI(GlobalDefinitions.combatGUIInstance);

        if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance"))
        {
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<CombatState>().executeSelectUnit;
            CombatRoutines.checkIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality, true);
        }
        else
        {
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().executeSelectUnit;
            CombatRoutines.checkIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality, false);
        }
    }
}
