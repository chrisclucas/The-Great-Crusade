using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatToggleRoutines : MonoBehaviour
{
    public GameObject unit;
    public bool attackingUnitFlag = false;

    public GameObject currentCombat;

    /// <summary>
    /// This routine is called whenever an unit selection toggle on the combat gui is changed
    /// </summary>
    public void AddOrDeleteSelectedUnit()
    {
        if (GetComponent<Toggle>().isOn)
        {
            // Turn on the toggle on the remote computer
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.SETCOMBATTOGGLEKEYWORD + " " + name);

            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
            GlobalDefinitions.HighlightUnit(unit);
            if (attackingUnitFlag)
            {
                // An attacking unit was added

                // Need to check for adding an attack from a fortress.
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
                    AddDefendersOfFortressAttack(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                else
                    RefreshDefendersBasedOnAttackers();
            }
            else
            {
                // A defending unit was added

                // If the defender that was just added is a unit in a fortress, go through and turn on all other units that 
                // are in the fortress since attacking into a fortress means you have to attack all defenders

                // Loop through the other toggles
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
                    foreach (Transform childTransform in transform.parent.transform)
                        if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) &&
                                (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit != null) &&
                                (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit != unit) &&
                                (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit.GetComponent<UnitDatabaseFields>().occupiedHex ==
                                unit.GetComponent<UnitDatabaseFields>().occupiedHex) &&
                                !childTransform.GetComponent<Toggle>().isOn)
                            // This is another unit in the fotress, turn it on also.  Note this doesn't add to the checks that need 
                            // to be done because it is on the same hex as the unit being checked already
                            childTransform.GetComponent<Toggle>().isOn = true;

                RefreshAttackersBasedOnDefenders();
            }
        }
        else
        {
            // Turn off the toggle on the remote computer
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.RESETCOMBATTOGGLEKEYWORD + " " + name);

            GlobalDefinitions.UnhighlightUnit(unit);
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
            if (attackingUnitFlag)
            {
                // An attacking unit was removed

                // Need to check for removing an attack from a fortress.  If there are no more units attacking from a fortress than remove the adjacent units.
                // They still may need to be attacked due to other units but that will be set by the process below.
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
                {
                    bool attackStillTakingPlace = false;
                    foreach (Transform childTransform in transform.parent.transform)
                    {
                        if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) &&
                                (childTransform.GetComponent<CombatToggleRoutines>().unit != null) &&
                                (childTransform.GetComponent<CombatToggleRoutines>().unit != unit) &&
                                (childTransform.GetComponent<CombatToggleRoutines>().unit.GetComponent<UnitDatabaseFields>().occupiedHex ==
                                unit.GetComponent<UnitDatabaseFields>().occupiedHex) &&
                                childTransform.GetComponent<Toggle>().isOn)
                            attackStillTakingPlace = true;
                    }

                    if (!attackStillTakingPlace)
                        RemoveDefendersOfFortressAttack(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                }
                else
                    RefreshDefendersBasedOnAttackers();
            }
            else
            {
                // A defending unit was removed

                // If the defender that was just removed is a unit in a fortress, go through and turn on all other units that 
                // are in the fortress and turn them off also since you can't pick and choose what units to attack in a fortress

                // Loop through the other toggles
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
                    foreach (Transform childTransform in transform.parent.transform)
                        if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) &&
                                (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit != null) &&
                                (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit != unit) &&
                                (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit.GetComponent<UnitDatabaseFields>().occupiedHex ==
                                unit.GetComponent<UnitDatabaseFields>().occupiedHex) &&
                                childTransform.GetComponent<Toggle>().isOn)
                            // This is another unit in the fotress, turn it off also.  Note this doesn't add to the checks that need 
                            // to be done because it is on the same hex as the unit being checked already
                            childTransform.GetComponent<Toggle>().isOn = false;

                RefreshAttackersBasedOnDefenders();
            }
        }

        // Check if carpet bombing is available with the current set of defenders
        // When the game sets the state of a defender or an attacker it is executing this before the gui is finished and the code 
        // below will cause an exception.  Need to check that the toggle is there.
        if (GlobalDefinitions.combatCarpetBombingToggle != null)
        {
            if (CombatRoutines.CheckIfCarpetBombingIsAvailable(currentCombat))
                GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().interactable = true;
            else
            {
                GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn = false;
                GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().interactable = false;
            }
        }

        UpdateOddsText();
    }

    /// <summary>
    /// This routine executes when a new attacker is committed to attacking
    /// </summary>
    private void RefreshDefendersBasedOnAttackers()
    {
        // First go through and enable all the defending units since if I don't, once a unit is disabled it will never be enabled again and this way 
        // I don't have to explicitly check for adjacency to all attackers

        foreach (GameObject defendingUnit in currentCombat.GetComponent<Combat>().defendingUnits)
            // This is how the other toggles in the display are accessed
            foreach (Transform childTransform in transform.parent.transform)
                if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) &&
                        (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit == defendingUnit))
                    // if an invasion is taking place don't turn the toogle back on
                    if (!GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().CheckForInvasionDefense(
                            childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit, currentCombat))
                        childTransform.gameObject.GetComponent<Toggle>().interactable = true;

        // Go through each of the attackers and gray out any defender that isn't adjacent.  By going through each of the attackers I am left
        // with only defenders that are adjacent to all attacking units
        foreach (GameObject attackingUnit in currentCombat.GetComponent<Combat>().attackingUnits)
        {
            // Only check for adjacency if the attaker is selected

            // When this is called because of the code changing a toggle status, the routine is called before the committed variable is set (since it is event triggered
            // I used to check the isCommittedToAttack variable but this doesn't work in complex situations.  Therefore I will check if the unit is committed to the attack by
            // checking the unit's toggle status

            bool attackerIsCommitted = false;
            foreach (Transform attackerChildTransform in transform.parent.transform)
                if ((attackerChildTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) &&
                        (attackerChildTransform.gameObject.GetComponent<CombatToggleRoutines>().unit == attackingUnit))
                    if (attackerChildTransform.gameObject.GetComponent<Toggle>().isOn)
                        attackerIsCommitted = true;

            if (attackerIsCommitted)
                foreach (GameObject defendingUnit in currentCombat.GetComponent<Combat>().defendingUnits)
                    if (!GlobalDefinitions.TwoUnitsAdjacent(attackingUnit, defendingUnit))
                        // The defending unit is not adjacent to the attacking unit so it needs to be greyed out and disabled in the display
                        foreach (Transform childTransform in transform.parent.transform)
                            if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) &&
                                    (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit == defendingUnit))
                            {
                                GlobalDefinitions.UnhighlightUnit(defendingUnit);
                                defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                                childTransform.gameObject.GetComponent<Toggle>().isOn = false;
                                childTransform.gameObject.GetComponent<Toggle>().interactable = false;
                            }
        }
    }

    /// <summary>
    /// This routine is called when a new defender is added to the attack and updates the toggles on the combat gui
    /// </summary>
    private void RefreshAttackersBasedOnDefenders()
    {
        // First go through and enable all the attacking units since if I don't once a unit is disabled it will never be enabled again and this way 
        // I don't have to explicitly check for adjacency to all defenders

        foreach (GameObject attackingUnit in currentCombat.GetComponent<Combat>().attackingUnits)
        {
            foreach (Transform childTransform in transform.parent.transform)
            {
                if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) && (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit == attackingUnit))
                {
                    // check if there is an invasion taking place and if so do not turn the check back on.
                    if (!GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().CheckForInvadingAttacker(childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit))
                        childTransform.gameObject.GetComponent<Toggle>().interactable = true;
                }
            }
        }

        // Go through each of the defenders and gray out any attacker that isn't adjacent.  By going through each of the defenders I am left
        // with only attackers that are adjacent to all defending units
        foreach (GameObject defendingUnit in currentCombat.GetComponent<Combat>().defendingUnits)
        {
            // Only check for adjacency if the defnder is selected

            // When this is called because of the code changing a toggle status, the routine is called before the committed variable is set (since it is event triggered
            // I used to check the isCommittedToAttack variable but this doesn't work in complex situations.  Therefore I will check if the unit is committed to the attack by
            // checking the unit's toggle status

            bool defendererIsCommitted = false;
            foreach (Transform defenderChildTransform in transform.parent.transform)
                if ((defenderChildTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) &&
                        (defenderChildTransform.gameObject.GetComponent<CombatToggleRoutines>().unit == defendingUnit))
                    if (defenderChildTransform.gameObject.GetComponent<Toggle>().isOn)
                        defendererIsCommitted = true;

            if (defendererIsCommitted)
            {
                foreach (GameObject attackingUnit in currentCombat.GetComponent<Combat>().attackingUnits)
                {
                    if (!GlobalDefinitions.TwoUnitsAdjacent(attackingUnit, defendingUnit))
                    {
                        // The attacking unit is not adjacent to the defending unit so it needs to be greyed out and disabled in the display
                        foreach (Transform childTransform in transform.parent.transform)
                        {
                            if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) && (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit == attackingUnit))
                            {
                                GlobalDefinitions.WriteToLogFile("refreshAttackersBasedOnDefenders: decommitting attacker and making non-interactable " + attackingUnit.name);
                                GlobalDefinitions.UnhighlightUnit(attackingUnit);
                                attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                                childTransform.gameObject.GetComponent<Toggle>().isOn = false;
                                childTransform.gameObject.GetComponent<Toggle>().interactable = false;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// This routine looks through the attackers and defenders and determines if there are defenders that need to be added to the mustBeAttackedUnits due to being in the ZOC of a defender being attacked cross river
    /// </summary>
    //private void checkIfDefenderToBeAdded()
    //{
    //    foreach (GameObject defendingUnit in currentCombat.GetComponent<Combat>().defendingUnits)
    //    {
    //        //check if the defender is across a river from a committed attacker
    //        foreach (GameObject attackingUnit in currentCombat.GetComponent<Combat>().attackingUnits)
    //        {
    //            if (attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
    //            {
    //                foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
    //                {
    //                    if ((defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
    //                            && (defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] == attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex)
    //                            && defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<BoolArrayData>().riverSides[(int)hexSide])
    //                    {
    //                        // If the defender is committed to the attack, make sure it is on the mustBeAttackedUnits list
    //                        if (defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack && !GlobalDefinitions.mustBeAttackedUnits.Contains(defendingUnit))
    //                        {
    //                            GlobalDefinitions.mustBeAttackedUnits.Add(defendingUnit);
    //                        }
    //                        else
    //                        {
    //                            // Check if the defendingUnit is in the ZOC of another defender that is being attacked cross river
    //                            foreach (GameObject committedDefender in currentCombat.GetComponent<Combat>().defendingUnits)
    //                            {
    //                                if (committedDefender.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack && (committedDefender != defendingUnit))
    //                                {
    //                                    foreach (GlobalDefinitions.HexSides hexSide2 in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
    //                                    {
    //                                        if ((committedDefender.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide2] != null)
    //                                                && (committedDefender.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide2] == defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex)
    //                                                && committedDefender.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<BoolArrayData>().exertsZOC[(int)hexSide2]
    //                                                && GlobalDefinitions.checkForRiverBetweenTwoHexes(attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex, committedDefender.GetComponent<UnitDatabaseFields>().occupiedHex))
    //                                        {
    //                                            // If we get here the defending unit needs to get added to the mustBeAttackedUnits list
    //                                            if (!GlobalDefinitions.mustBeAttackedUnits.Contains(defendingUnit))
    //                                            {
    //                                                GlobalDefinitions.mustBeAttackedUnits.Add(defendingUnit);
    //                                            //    if (!defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
    //                                            //        GlobalDefinitions.highlightUnit(unit);
    //                                            }
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// this routine will reset the defenders that are in the mustBeAttackedUnits list
    /// </summary>
    public void CheckDefenderToBeRemoved()
    {
        // The easiest thing to do is to remove all current defenders from the mustBeAttackedUnts list and then go through and add the units that need to be added
        //foreach (GameObject defendingUnit in currentCombat.GetComponent<Combat>().defendingUnits)
        //    if (GlobalDefinitions.mustBeAttackedUnits.Contains(defendingUnit))
        //    {
        //        GlobalDefinitions.mustBeAttackedUnits.Remove(defendingUnit);
        //        //GlobalDefinitions.unhighlightUnit(defendingUnit);
        //    }
        if (currentCombat.GetComponent<Combat>().attackingUnits.Count > 0)
        {
            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance"))
                CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, true);
            else
                CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, false);

            //checkIfDefenderToBeAdded();
        }
    }

    /// <summary>
    /// This routine is called when a unit decides to attack from a fortress.   It toggles all units that would be in the unit's ZOC if it wasn't in a fortress.
    /// </summary>
    /// <param name="fortressHex"></param>
    public void AddDefendersOfFortressAttack(GameObject fortressHex)
    {
        foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            if (fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
            {
                if (!fortressHex.GetComponent<BoolArrayData>().riverSides[(int)hexSide] &&
                        !fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().fortress)
                    if ((fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality !=
                            fortressHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality))
                        // If we get here then the unit should be highlighted since it is adjacent and isn't separated by a river or it isn't a fortress
                        foreach (GameObject unit in fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit)
                        {
                            GlobalDefinitions.HighlightUnit(unit);
                            foreach (Transform childTransform in transform.parent.transform)
                                if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) && (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit == unit))
                                {
                                    // Turn on the defenders toggle to show that the unit must be attacked
                                    GlobalDefinitions.WriteToLogFile("addDefendersOfFortressAttack: adding unit " + unit.name + " as defender of attack from fortress");
                                    childTransform.GetComponent<Toggle>().isOn = true;
                                    // The only way to turn the toggle off will be to click off the attacking unit
                                    childTransform.GetComponent<Toggle>().interactable = false;
                                }
                        }
            }
    }

    /// <summary>
    /// This routine is called when attacking from a fortress is removed.  It toggles off the adjacent units
    /// </summary>
    /// <param name="fortressHex"></param>
    public void RemoveDefendersOfFortressAttack(GameObject fortressHex)
    {
        foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            if (fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                if (!fortressHex.GetComponent<BoolArrayData>().riverSides[(int)hexSide])
                    // No need to check the type of hex.  If there are enemy units on it turn the toggle off
                    if ((fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality !=
                            fortressHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality))
                        foreach (GameObject unit in fortressHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit)
                            foreach (Transform childTransform in transform.parent.transform)
                                if ((childTransform.gameObject.GetComponent<CombatToggleRoutines>() != null) && (childTransform.gameObject.GetComponent<CombatToggleRoutines>().unit == unit))
                                {
                                    GlobalDefinitions.UnhighlightUnit(unit);
                                    unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                                    // Turn off the selection of the unit
                                    childTransform.GetComponent<Toggle>().isOn = false;
                                    // Allow the user to select the unit
                                    childTransform.GetComponent<Toggle>().interactable = true;
                                }
    }

    /// <summary>
    /// Used to toggle the air support option on the combat gui in network games
    /// </summary>
    public void ToggleAirSupport()
    {
        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.TOGGLEAIRSUPPORTCOMBATTOGGLE + " " + name);

        if (this.GetComponent<Toggle>().isOn)
        {
            if (GlobalDefinitions.tacticalAirMissionsThisTurn < GlobalDefinitions.maxNumberOfTacticalAirMissions)
            {
                currentCombat.GetComponent<Combat>().attackAirSupport = true;
                GlobalDefinitions.WriteToLogFile("toggleAirSupport: incrementing GlobalDefinitions.tacticalAirMissionsThisTurn");
                GlobalDefinitions.tacticalAirMissionsThisTurn++;
            }
            else
            {
                GlobalDefinitions.WriteToLogFile("No more air missions available");
                this.GetComponent<Toggle>().isOn = false;
            }
        }
        else
        {
            currentCombat.GetComponent<Combat>().attackAirSupport = false;
            GlobalDefinitions.tacticalAirMissionsThisTurn--;
        }
        UpdateOddsText();
    }

    public void ToggleCarpetBombing()
    {
        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.TOGGLECARPETBOMBINGCOMBATTOGGLE + " " + name);

        if (this.GetComponent<Toggle>().isOn)
        {
            currentCombat.GetComponent<Combat>().carpetBombing = true;
        }
        else
        {
            currentCombat.GetComponent<Combat>().carpetBombing = false;
        }
    }

    private void UpdateOddsText()
    {
        // Update the text displaying the combat odds
        GameObject.Find("OddsText").GetComponent<TextMeshProUGUI>().text = "Combat Odds " +
                GlobalDefinitions.ConvertOddsToString(
                GlobalDefinitions.ReturnCombatOdds(
                currentCombat.GetComponent<Combat>().defendingUnits,
                currentCombat.GetComponent<Combat>().attackingUnits, currentCombat.GetComponent<Combat>().attackAirSupport)) + "\nDefense = " +
                GlobalDefinitions.CalculateDefenseFactor(
                currentCombat.GetComponent<Combat>().defendingUnits,
                currentCombat.GetComponent<Combat>().attackingUnits) + "\nAttack = " +
                GlobalDefinitions.CalculateAttackFactor(
                currentCombat.GetComponent<Combat>().attackingUnits, currentCombat.GetComponent<Combat>().attackAirSupport);
    }
}
