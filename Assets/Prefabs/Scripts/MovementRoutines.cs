
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MovementRoutines : MonoBehaviour
{
    /// <summary>
    /// Returns the unit that is going to be moved of the correct nationality
    /// </summary>
    /// <param name="nationality"></param>
    public GameObject getUnitToMove(GameObject selectedUnit)
    {
        List<GameObject> movementHexes = new List<GameObject>();
        // Note that the check for a Null unit has already been done
        if (selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality)
        {
            GlobalDefinitions.highlightUnit(selectedUnit);
            movementHexes = returnAvailableMovementHexes(selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex, selectedUnit);
            foreach (GameObject hex in movementHexes)
                GlobalDefinitions.highlightHexForMovement(hex);

            return (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex);
        }
        else
        {
            // If the hex selected has enemy units on it display them in the gui
            if (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
                GlobalDefinitions.guiDisplayUnitsOnHex(selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex);
            return (null);
        }
    }

    /// <summary>
    /// Takes the passed unit and moves it to the passed hex
    /// </summary>
    /// <param name="selectedUnit"></param>
    /// <param name="startHex"></param>
    /// <param name="destinationHex"></param>
    public void getUnitMoveDestination(GameObject selectedUnit, GameObject startHex, GameObject destinationHex)
    {
        // I have an issue where an invading unit on a sea hex is selected during movement, in order to deselect it
        // the invading hex is selected which is a sea hex and sends the unit back to Britain.  While I'm not sure why
        // an invading unit would be selected in movement mode (since it can't move) if I don't account for this the 
        // invading unit is sent back to Britain and it's too late to bring it back (we're in movement mode).
        // The way I will deal wiht this is to check if start and destination are the same and just unselect the unit.
        // This will apply to more than just the scenario listed above but is still valid.

        // If the user selects a bridge the logic won't let him move there even though it is highlighted.
        // Write out a message so the user knows what is going on
        if ((destinationHex != null) && (destinationHex.GetComponent<HexDatabaseFields>().bridge))
            GlobalDefinitions.guiUpdateStatusMessage("Cannot stop movement on the dyke");

        if ((destinationHex != null) && (startHex != destinationHex))
        {
            if (destinationHex.GetComponent<HexDatabaseFields>().availableForMovement)
            {
                // If the hex selected is a sea hex than that means the unit is going back to Britain
                if (destinationHex.GetComponent<HexDatabaseFields>().sea)
                    moveUnitBackToBritain(startHex, selectedUnit, true);
                // Otherwise it is a normal move
                else
                    moveUnit(destinationHex, startHex, selectedUnit);
            }
            else
            {
                // if an invalid hex is selected "move" the object to where it already is (resets all hexes)
                moveUnit(startHex, startHex, selectedUnit);
            }
        }
        else
        {
            if (startHex != null)
                // If a hex isn't hit, leave the unit where it is
                moveUnit(startHex, startHex, selectedUnit);
        }

        GlobalDefinitions.unhighlightUnit(selectedUnit);
    }

    /// <summary>
    /// Moves the selected unit from off board to the passed hex
    /// The allied routine is separate since have to check for unit limits
    /// Returns true if the unit is brought onto the board
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="hex"></param>
    /// <returns></returns>
    public bool landAlliedUnitFromOffBoard(GameObject unit, GameObject hex, bool highlight)
    {
        List<GameObject> movementHexes = new List<GameObject>();
        if (hex != null)
        {
            if (hex.GetComponent<HexDatabaseFields>().availableForMovement)
            {
                GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().assignAvailableSupplyCapacity(hex, unit);
                moveUnitFromBritain(hex, unit);
                GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().incrementInvasionUnitLimits(unit);

                foreach (GameObject tempHex in GlobalDefinitions.allHexesOnBoard)
                {
                    GlobalDefinitions.unhighlightHex(tempHex);
                    tempHex.GetComponent<HexDatabaseFields>().availableForMovement = false;
                }

                // If the unit did not land in a German ZOC remove one movement factor and highlight the hexes that it can move from here
                if (!hex.GetComponent<HexDatabaseFields>().inGermanZOC)
                {
                    unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor - 1;
                    movementHexes = returnAvailableMovementHexes(hex, unit);
                    if (highlight)
                        foreach (GameObject tempHex in movementHexes)
                            GlobalDefinitions.highlightHexForMovement(tempHex);
                    else
                        unit.GetComponent<UnitDatabaseFields>().availableMovementHexes = movementHexes;
                }
                else
                {
                    GlobalDefinitions.unhighlightUnit(unit);
                    unit = null;
                    foreach (GameObject tempHex in GlobalDefinitions.allHexesOnBoard)
                    {
                        GlobalDefinitions.unhighlightHex(tempHex);
                        tempHex.GetComponent<HexDatabaseFields>().availableForMovement = false;
                    }
                }
                return (true);
            }
            else
            {
                GlobalDefinitions.unhighlightUnit(unit);
                unit = null;
                GlobalDefinitions.guiUpdateStatusMessage("Hex selected is not avaiable");
                return (false);
            }
        }
        else
        {
            GlobalDefinitions.unhighlightUnit(unit);
            unit = null;
            foreach (GameObject tempHex in GlobalDefinitions.allHexesOnBoard)
            {
                GlobalDefinitions.unhighlightHex(tempHex);
                tempHex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            }
            GlobalDefinitions.guiUpdateStatusMessage("No hex selected");
            return (false);
        }
    }

    /// <summary>
    /// This unit places a German replacement unit
    /// </summary>
    /// <param name="selectedUnit"></param>
    /// <param name="selectedHex"></param>
    /// <returns></returns>
    public bool landGermanUnitFromOffBoard(GameObject selectedUnit, GameObject selectedHex)
    {
        if (selectedHex != null)
        {
            if (selectedHex.GetComponent<HexDatabaseFields>().availableForMovement)
            {

                selectedUnit.transform.parent = GlobalDefinitions.allUnitsOnBoard.transform;
                // Add the unit to the OnBoard list
                GlobalDefinitions.germanUnitsOnBoard.Add(selectedUnit);

                // Change the unit's location to the target hex
                GlobalDefinitions.putUnitOnHex(selectedUnit, selectedHex);
                selectedUnit.GetComponent<UnitDatabaseFields>().unitEliminated = false;

                if (!checkForAdjacentEnemy(selectedHex, selectedUnit) && (selectedUnit.GetComponent<UnitDatabaseFields>().armor || selectedUnit.GetComponent<UnitDatabaseFields>().airborne))
                    selectedUnit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = true;

                // If this is the only unit in the target hex then update ZOC's
                if (selectedHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count < 2)
                    updateZOC(selectedHex);

                // Finally remove all of the highlighting from hexes that were availabe for movement and reset the availableForMovement and remainingMovement fields
                foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
                {
                    GlobalDefinitions.unhighlightHex(hex.gameObject);
                    hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                    hex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
                    hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
                }
                GlobalDefinitions.germanReplacementsRemaining -= selectedUnit.GetComponent<UnitDatabaseFields>().attackFactor;
                return (true);
            }
            else
            {
                GlobalDefinitions.unhighlightUnit(selectedUnit);
                selectedUnit = null;
                GlobalDefinitions.guiUpdateStatusMessage("Hex selected is not avaiable");
                return (false);
            }
        }
        else
        {
            GlobalDefinitions.unhighlightUnit(selectedUnit);
            selectedUnit = null;
            foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            {
                GlobalDefinitions.unhighlightHex(hex);
                hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            }
            GlobalDefinitions.guiUpdateStatusMessage("No hex selected");
            return (false);
        }
    }

    /// <summary>
    /// This routine checks the unit limits for the Allied reinforcement area.  If limit isn't reached it returns true, otherwise false
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private bool checkUnitLimits(GameObject hex, GameObject unit)
    {
        //GlobalDefinitions.writeToLogFile("MovementRoutines.checkUnitLimits: checking limits for index = " + hex.GetComponent<HexDatabaseFields>().invasionAreaIndex);
        //GlobalDefinitions.writeToLogFile("MovementRoutines.checkUnitLimits:     turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn);
        //GlobalDefinitions.writeToLogFile("MovementRoutines.checkUnitLimits:     totalUnitsUsedThisTurn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].totalUnitsUsedThisTurn);
        //GlobalDefinitions.writeToLogFile("MovementRoutines.checkUnitLimits:     GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn = " + GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn);

        if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].invaded)
        {
            if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn == 1)
            {
                if (unit.GetComponent<UnitDatabaseFields>().armor)
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnArmor)
                        if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                            return (true);

                if (unit.GetComponent<UnitDatabaseFields>().infantry)
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnInfantry)
                        if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                            return (true);
                        // Before we return false see if the infantry unit can be applied to the armor limit
                        else if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <
                                GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnArmor)
                            if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                                return (true);
                }
                if (unit.GetComponent<UnitDatabaseFields>().airborne)
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].airborneUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnAirborne)
                        if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                            return (true);
                        // Before we return false see if the infantry unit can be applied to the armor limit
                        else if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn <=
                                GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnInfantry)
                            if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                                return (true);
                            else if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <=
                                    GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnArmor)
                                if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                                    return (true);
                }
            }
            if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn == 2)
            {
                if (unit.GetComponent<UnitDatabaseFields>().armor)
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnArmor)
                        if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                            return (true);
                if (unit.GetComponent<UnitDatabaseFields>().infantry)
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnInfantry)
                        if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                            return (true);
                        // Before we return false see if the infantry unit can be applied to the armor limit
                        else if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <
                                GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnArmor)
                            if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                                return (true);
                }
                if (unit.GetComponent<UnitDatabaseFields>().airborne)
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].airborneUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnAirborne)
                        if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                            return (true);
                        // Before we return false see if the infantry unit can be applied to the armor limit
                        else if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn <=
                                GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnInfantry)
                            if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                                return (true);
                            else if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <=
                                    GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnArmor)
                                if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
                                    return (true);
                }
            }
            if ((GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].totalUnitsUsedThisTurn <
                    GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].divisionsPerTurn)
                    && (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn))
                return (true);
        }
        else
            if ((GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].totalUnitsUsedThisTurn <
                    GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].divisionsPerTurn)
                    && (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn))
            return (true);

        return (false);
    }

    /// <summary>
    /// This routine checks if the target hex passed is available for movement of the unit passed
    /// </summary>
    /// <param name="beginningHex"></param>
    /// <param name="destinationHex"></param>
    /// <param name="selectedUnit"></param>
    /// <returns></returns>
    private bool checkForMovementAvailable(GameObject beginningHex, GameObject destinationHex, GameObject selectedUnit)
    {
        // First check if there is any remaining movement cost available from the start hex
        if (beginningHex.GetComponent<HexDatabaseFields>().remainingMovement == 0)
        {
            //GlobalDefinitions.writeToLogFile("checkForMovementAvailable: movement not available - no remaining movement");
            return (false);
        }

        // Check if the destination is occupied by an enemy unit
        if ((destinationHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                (selectedUnit.GetComponent<UnitDatabaseFields>().nationality != destinationHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality))
        {
            //GlobalDefinitions.writeToLogFile("checkForMovementAvailable: movement not available - enemy unit on hex");
            return (false);
        }

        // This check needs to be done before the true check below since it is for hexes which can't be entered
        if (destinationHex.GetComponent<HexDatabaseFields>().impassible || destinationHex.GetComponent<HexDatabaseFields>().neutralCountry ||
                destinationHex.GetComponent<HexDatabaseFields>().sea)
        {
            //GlobalDefinitions.writeToLogFile("checkForMovementAvailable: movement not available - hex not available for unit");
            return (false);
        }

        // During interactive play the user can over-stack with the idea that they would fix the over-stack before moving to the next phase
        // If the AI is moving it needs to not be allowed to over-stack since there is no process to resolve issues before going to the next phase
        if (!GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI) &&
                !GlobalDefinitions.hexUnderStackingLimit(destinationHex, selectedUnit.GetComponent<UnitDatabaseFields>().nationality) &&
                (destinationHex.GetComponent<HexDatabaseFields>().remainingMovement == 0))
        {
            //GlobalDefinitions.writeToLogFile("checkForMovementAvailable: movement not available - overstacked");
            //return (false);
        }

        // Need to add a check that prohibits a unit from moving from the ZOC of an enemy to another hex that is in the same units ZOC.  This is an issue when
        // a unit starts its turn in an enemy ZOC because of post combat movement by the enemy at the end of the last turn.  It is allowed for a unit to move
        // from one ZOC hex to another enemy ZOC hex as long as the same unit isn't exerting ZOC to both hexes.

        // Need to add a special check for HQ units.  They cannot enter an enemy ZOC
        if (selectedUnit.GetComponent<UnitDatabaseFields>().HQ)
        {
            if ((selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German) && destinationHex.GetComponent<HexDatabaseFields>().inAlliedZOC)
            {
                //GlobalDefinitions.writeToLogFile("checkForMovementAvailable: movement not available - HQ cannot enter enemy ZOC");
                return (false);
            }
            else if ((selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) && destinationHex.GetComponent<HexDatabaseFields>().inGermanZOC)
            {
                //GlobalDefinitions.writeToLogFile("checkForMovementAvailable: movement not available - HQ cannot enter enemy ZOC");
                return (false);
            }
        }

        if ((movementCost(beginningHex, destinationHex, selectedUnit) <= beginningHex.GetComponent<HexDatabaseFields>().remainingMovement))
        {
            // A unit can pass over a hex that has reached it's stacking limit but it can't end there.  I can't check it here because theoretically I could have enough movement available to get
            // to the current overstacked hex with movement remaining but I can't be gauranteed that the movement remaining is enough to allow the unit to move off the overstacked hex.  My original
            // implementation just removed overstacked hexes from being avialable for movement, but in a tight beach situation this could artifiially keep a unit stuck.  But there is no good option
            // and the reality if that if the max stacked hex doesn't have any options to move it is probably because movement is constrained by enemy ZOC anyhow.  So I'm going with the intial
            // implementation for now.

            // Bridges are not available for movement, they can pass over but they can't stop
            if (!destinationHex.GetComponent<HexDatabaseFields>().bridge)
            {
                destinationHex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                //GlobalDefinitions.highlightHexForMovement(destinationHex);
            }

            // The if statement below is needed to see if the current path takes less moves to get to the target hex
            if ((beginningHex.GetComponent<HexDatabaseFields>().remainingMovement - movementCost(beginningHex, destinationHex, selectedUnit)) > destinationHex.GetComponent<HexDatabaseFields>().remainingMovement)
                destinationHex.GetComponent<HexDatabaseFields>().remainingMovement = beginningHex.GetComponent<HexDatabaseFields>().remainingMovement - movementCost(beginningHex, destinationHex, selectedUnit);
            //GlobalDefinitions.writeToLogFile("checkForMovementAvailable: movement available");
            return (true);
        }
        else
        {
            //GlobalDefinitions.writeToLogFile("checkForMovementAvailable: movement not available - not enough remaining movement");
            return (false);
        }
    }

    /// <summary>
    /// This routine checks to see if the movement from the beginning hex to the destination hex is available for strategic movement
    /// </summary>
    /// <param name="beginningHex"></param>
    /// <param name="destinationHex"></param>
    /// <param name="selectedUnit"></param>
    /// <returns></returns>
    private bool checkForStrategicMovementAvailable(GameObject beginningHex, GameObject destinationHex, GameObject selectedUnit)
    {
        // Note that in checking for strategic movement I don't have to do a special check for HQs like in normal movement to make sure they don't enter an enemy ZOC since strategic is more restrictive

        // First check if there is any remaining movement cost available from the start hex
        if (beginningHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement == 0)
            return (false);

        // Check if the unit is occupied by an enemy unit
        if ((destinationHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                (selectedUnit.GetComponent<UnitDatabaseFields>().nationality != destinationHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality))
            return (false);

        // This check needs to be done before the true check below since it is for mountains or netral countries - which can't be entered
        if ((destinationHex.GetComponent<HexDatabaseFields>().impassible) || (destinationHex.GetComponent<HexDatabaseFields>().neutralCountry) || (destinationHex.GetComponent<HexDatabaseFields>().sea))
            return (false);

        // Need to check if the starting hex (this is really only useful for the intial check only) and the destination hex are not adjacent to an enemy unit
        if (checkForAdjacentEnemy(beginningHex, selectedUnit) || checkForAdjacentEnemy(destinationHex, selectedUnit))
            return (false);

        // During interactive play the user can over-stack with the idea that they would fix the over-stack before moving to the next phase
        // If the AI is moving it needs to not be allowed to over-stack since there is no process to resolve issues before going to the next phase
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI) && !GlobalDefinitions.localControl && !GlobalDefinitions.hexUnderStackingLimit(destinationHex, selectedUnit.GetComponent<UnitDatabaseFields>().nationality))
            return (false);

        if ((strategicMovementCost(beginningHex, destinationHex, selectedUnit) <= beginningHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement))
        {
            // A unit can pass over a hex that has reached it's stacking limit but it can't end there.  I can't check it here because theoretically I could have enough movement available to get
            // to the current overstacked hex with movement remaining but I can't be gauranteed that the movement remaining is enough to allow the unit to move off the overstacked hex.  My original
            // implementation just removed overstacked hexes from being avialable for movement, but in a tight beach situation this could artifiially keep a unit stuck.  But there is no good option
            // and the reality if that if the max stacked hex doesn't have any options to move it is probably because movement is constrained by enemy ZOC anyhow.  So I'm going with the intial
            // implementation for now.

            // Bridges are not available for movement, they can pass over but they can't stop
            if (!destinationHex.GetComponent<HexDatabaseFields>().bridge)
                destinationHex.GetComponent<HexDatabaseFields>().availableForMovement = true;

            // The if statement below is needed to see if the current path takes less moves to get to the target hex
            if ((beginningHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement - strategicMovementCost(beginningHex, destinationHex, selectedUnit)) > destinationHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement)
                destinationHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = beginningHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement - strategicMovementCost(beginningHex, destinationHex, selectedUnit);

            return (true);
        }
        else
            return (false);
    }

    /// <summary>
    /// This routine checks to see if the hex passed to it has any enemies in adjacent hexes.  Used for strategic movement.
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="selectedUnit"></param>
    /// <returns></returns>
    public bool checkForAdjacentEnemy(GameObject hex, GameObject selectedUnit)
    {
        foreach (GlobalDefinitions.HexSides hexSides in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
        {
            if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] != null) && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                    (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality != selectedUnit.GetComponent<UnitDatabaseFields>().nationality))
            {
                return (true);
            }
        }
        return (false);
    }

    /// <summary>
    /// This routine returns the cost to move from the beginning hex passed to the desitnation hex passed for the selected unit
    /// </summary>
    /// <param name="beginningHex"></param>
    /// <param name="destinationHex"></param>
    /// <param name="selectedUnit"></param>
    /// <returns></returns>
    private int movementCost(GameObject beginningHex, GameObject destinationHex, GameObject selectedUnit)
    {
        bool checkNeeded = false;
        GlobalDefinitions.Nationality nationalityToCheck = new GlobalDefinitions.Nationality();

        // The first check is to determine if the selectedUnit is starting in an enemy ZOC it cannot move
        // into a hex that is in that same unit's ZOC.  I know it is starting in an enemy ZOC if the 
        // remainingMovement of the beginning hex is equal to the movementFactor of the unit
        if (beginningHex.GetComponent<HexDatabaseFields>().remainingMovement == selectedUnit.GetComponent<UnitDatabaseFields>().movementFactor)
        {
            // nationalityToCheck will be set by the enemy nationality.  By setting this I don't have to write the same code twice
            if ((selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) &&
                    (beginningHex.GetComponent<HexDatabaseFields>().inGermanZOC))
            {
                nationalityToCheck = GlobalDefinitions.Nationality.German;
                checkNeeded = true;
            }
            else if ((selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German) &&
                    (beginningHex.GetComponent<HexDatabaseFields>().inAlliedZOC))
            {
                nationalityToCheck = GlobalDefinitions.Nationality.Allied;
                checkNeeded = true;
            }

            if (checkNeeded)
            {
                // Check if any of the 6 neighbors are exerting ZOC to the beginning hex
                foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                {
                    if ((beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null) &&
                        (beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                        (beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == nationalityToCheck) &&
                        (beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<BoolArrayData>().exertsZOC[GlobalDefinitions.returnHexSideOpposide((int)hexSide)]))
                    {
                        // The current hexSide exerts ZOC into the beginning hex

                        // Check if the hex exerting ZOC also projects ZOC into the hexSide + 2 hex or the hexSide + 4 hex
                        if ((((beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().Neighbors[GlobalDefinitions.returnHex2SideClockwise((int)hexSide)] != null) &&
                                (beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<BoolArrayData>().exertsZOC[GlobalDefinitions.returnHex2SideClockwise((int)hexSide)]) &&
                                (beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().Neighbors[GlobalDefinitions.returnHex2SideClockwise((int)hexSide)] == destinationHex))) ||
                                ((beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().Neighbors[GlobalDefinitions.returnHex4SideClockwise((int)hexSide)] != null) &&
                                (beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<BoolArrayData>().exertsZOC[GlobalDefinitions.returnHex4SideClockwise((int)hexSide)]) &&
                                (beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().Neighbors[GlobalDefinitions.returnHex4SideClockwise((int)hexSide)] == destinationHex)))
                        {
                            //  The same unit has ZOC in both hexes so return a movement cost of the unit's movementFactor + 1
                            //  Note, I don't have to keep checking since it doesn't matter if there are more units with the same condition so I return immediately
                            return ((int)selectedUnit.GetComponent<UnitDatabaseFields>().movementFactor + 1);
                        }
                    }
                }
            }
        }

        // Check if the target hex is an enemy ZOC.  If so then the cost to enter will be all remaining movement
        // That is the unit can move into a ZOC but then must stop
        if (selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
        {
            if (destinationHex.GetComponent<HexDatabaseFields>().inAlliedZOC)
            {
                return (beginningHex.GetComponent<HexDatabaseFields>().remainingMovement);
            }
        }
        else
        {
            if (destinationHex.GetComponent<HexDatabaseFields>().inGermanZOC)
            {
                return (beginningHex.GetComponent<HexDatabaseFields>().remainingMovement);
            }
        }

        // If the destination hex is a mountain hex it will take all of the remaining movement to move there
        if (destinationHex.GetComponent<HexDatabaseFields>().mountain)
        {
            return (beginningHex.GetComponent<HexDatabaseFields>().remainingMovement);
        }

        // If the beginning hex is an river interdicted hex, and the unit is German, and the destination hex is across a river
        // it will take all the remaining movement to move there.
        if ((selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German) &&
                (destinationHex.GetComponent<HexDatabaseFields>().riverInterdiction))
        {
            // The hex is interdicted, now check if movement crosses a river
            foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            {
                if ((beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] == destinationHex) &&
                            beginningHex.GetComponent<BoolArrayData>().riverSides[(int)hexSide])
                    return (beginningHex.GetComponent<HexDatabaseFields>().remainingMovement);
            }
        }

        return (1); // The default movement cost is 1
    }

    /// <summary>
    /// This routine returns the movement cost between the two hexes passed to it in strategic movement mode
    /// </summary>
    /// <param name="beginningHex"></param>
    /// <param name="destinationHex"></param>
    /// <param name="selectedUnit"></param>
    /// <returns></returns>
    private int strategicMovementCost(GameObject beginningHex, GameObject destinationHex, GameObject selectedUnit)
    {

        // Check if the target hex is an enemy ZOC.  If so then the cost to enter will be all remaining movement + 1
        // A unit using strategic movement cannot enter an enemy ZOC
        if (selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
        {
            if (destinationHex.GetComponent<HexDatabaseFields>().inAlliedZOC)
            {
                return (beginningHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement + 1);
            }
        }
        else
        {
            if (destinationHex.GetComponent<HexDatabaseFields>().inGermanZOC)
            {
                return (beginningHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement + 1);
            }
        }

        // All units must stop when entering a mountain hex
        if (destinationHex.GetComponent<HexDatabaseFields>().mountain)
        {
            return (beginningHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement);
        }

        // If the beginning hex is an river interdicted hex, and the unit is German, and the destination hex is across a river
        // it will take all the remaining movement to move there.
        if ((selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German) &&
                (destinationHex.GetComponent<HexDatabaseFields>().riverInterdiction))
        {
            // The hex is interdicted, now check if movement crosses a river
            foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            {
                if ((beginningHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] == destinationHex) &&
                            beginningHex.GetComponent<BoolArrayData>().riverSides[(int)hexSide])
                {
                    return (beginningHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement);
                }
            }
        }

        return (1); // The default movement cost is 1
    }

    /// <summary>
    /// This routine takes the unit and hex passed to it and returns all hexes available for movement
    /// </summary>
    /// <param name="intialHexToCheck"></param>
    /// <param name="selectedUnit"></param>
    /// <returns></returns>
    public List<GameObject> returnAvailableMovementHexes(GameObject initialHexToCheck, GameObject selectedUnit)
    {
        // Reset hex values
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
            hex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
        }

        //GlobalDefinitions.writeToLogFile("returnAvailableMovementHexes: initial hex to check = " + initialHexToCheck.name + "  unit = " + selectedUnit.name + "  unit remaining movement = " + selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement);
        List<GameObject> availableMovementHexes = new List<GameObject>();
        List<GameObject> hexesToCheck = new List<GameObject>();
        bool storeHex;

        // Set the remaining movement available on the selected unit to the remaining movement on the initial hex.  Do this for both regular and strategic movement
        initialHexToCheck.GetComponent<HexDatabaseFields>().remainingMovement = selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement;
        if (selectedUnit.GetComponent<UnitDatabaseFields>().inSupply)
            initialHexToCheck.GetComponent<HexDatabaseFields>().strategicRemainingMovement = selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement * 2; // strategic movement is twice the regular movement

        hexesToCheck.Add(initialHexToCheck);
        //GlobalDefinitions.writeToLogFile("returnAvailableMovementHexes:     storing hex = " + initialHexToCheck.name + " initial hex to check");
        availableMovementHexes.Add(initialHexToCheck);

        // Need to set the intial hex to check as being available for movement.  This seems silly but the unhighlight is based on this being set.
        initialHexToCheck.GetComponent<HexDatabaseFields>().availableForMovement = true;

        // The first thing to do is to determine if the unit is an allied unit starting on a hex that allows for a return to Britain
        // If it is the sea hex associated with the hex will be highlighted
        if ((selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) && !selectedUnit.GetComponent<UnitDatabaseFields>().hasMoved)
            if (checkForUnitReturnToBritainBeginningOfTurn(initialHexToCheck))
                if ((getBritainReturnHex(initialHexToCheck) != null) && !availableMovementHexes.Contains(getBritainReturnHex(initialHexToCheck)))
                    availableMovementHexes.Add(getBritainReturnHex(initialHexToCheck));

        while (hexesToCheck.Count > 0)
        {
            //GlobalDefinitions.writeToLogFile("returnAvailableMovementHexes:     hexesToCheck.Count = " + hexesToCheck.Count + "  checking hex - " + hexesToCheck[0].name + " remaining movement = " + hexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement);
            foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            {
                if (hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                {
                    //GlobalDefinitions.writeToLogFile("returnAvailableMovementHexes:          checking hex = " + hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].name);
                    storeHex = false;

                    // Check for normal movement
                    if (checkForMovementAvailable(hexesToCheck[0], hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide], selectedUnit))
                    {
                        // See if this hex will allow the unit to return to Britain (for allied units only obviously)
                        if (selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied &&
                                checkForUnitReturnToBritain(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]))
                            if ((getBritainReturnHex(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]) != null) && !availableMovementHexes.Contains(getBritainReturnHex(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide])))
                                availableMovementHexes.Add(getBritainReturnHex(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]));
                        storeHex = true;
                    }

                    // If the selected unit is available for strategic movement, check for it
                    if (selectedUnit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement &&
                            !selectedUnit.GetComponent<UnitDatabaseFields>().unitInterdiction &&
                            !selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().mountain &&
                            selectedUnit.GetComponent<UnitDatabaseFields>().inSupply &&
                            checkForStrategicMovementAvailable(hexesToCheck[0], hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide], selectedUnit))
                    {
                        // See if this hex will allow the unit to return to Britain (for allied units only obviously)
                        if (selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied &&
                                checkForUnitReturnToBritain(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]))
                            if ((getBritainReturnHex(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]) != null) && !availableMovementHexes.Contains(getBritainReturnHex(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide])))
                                availableMovementHexes.Add(getBritainReturnHex(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]));
                        storeHex = true;
                    }

                    // See of the current neighbor needs to be popped to the stack for checking
                    if (storeHex && !availableMovementHexes.Contains(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]))
                    {
                        //GlobalDefinitions.writeToLogFile("returnAvailableMovementHexes:     storing hex = " + hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].name);
                        hexesToCheck.Add(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                        availableMovementHexes.Add(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                    }
                }
            }
            hexesToCheck.RemoveAt(0);
        }
        //GlobalDefinitions.writeToLogFile("returnAvailableMovementHexes:    number of movement hexes being returned = " + availableMovementHexes.Count);
        return (availableMovementHexes);
    }

    /// <summary>
    /// This routine takes the unit pased to it and moves it from the beginningHex to the destinationHex
    /// </summary>
    /// <param name="destinationHex"></param>
    /// <param name="beginningHex"></param>
    /// <param name="unit"></param>
    public void moveUnit(GameObject destinationHex, GameObject beginningHex, GameObject unit)
    {
        //GlobalDefinitions.writeToLogFile("moveUnit: moving unit = " + unit.name + " beginning hex = " + beginningHex.name + " destination hex = " + destinationHex.name);

        // Need to check if the AI is overstacking units
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI) && !GlobalDefinitions.hexUnderStackingLimit(destinationHex, unit.GetComponent<UnitDatabaseFields>().nationality) &&
                (beginningHex != destinationHex))
        {
            GlobalDefinitions.writeToLogFile("moveUnit: ERROR AI is trying to overstack a hex - moving unit " + unit.name + " being moved from " + beginningHex.name + " to " + destinationHex.name);
        }


        // Check if a unit is capturing a hex that yields Allied replacement points
        checkForAlliedCaptureOfStrategicInstallations(destinationHex, unit);

        // Indcate that the unit has moved this turn
        unit.GetComponent<UnitDatabaseFields>().hasMoved = true;

        // Take the unit out of the occupyingUnits field of the current hex
        GlobalDefinitions.removeUnitFromHex(unit, beginningHex);

        // If this is the AI moving units don't change control of hexes since the AI moves units all over the place to test odds for attacking and then moves them back
        // NEED FIX: Note I need to fix the fact that someone who manually moves a unit to a hex and then performs an undo will result in a false setting
        if ((GlobalDefinitions.gameMode != GlobalDefinitions.GameModeValues.AI) || ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI) && GlobalDefinitions.localControl))
        {
            // Set the control of the hex
            if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                destinationHex.GetComponent<HexDatabaseFields>().alliedControl = true;
            else
            {
                destinationHex.GetComponent<HexDatabaseFields>().alliedControl = false;
                destinationHex.GetComponent<HexDatabaseFields>().successfullyInvaded = false;
            }
        }

        // Change the unit's location to the target hex
        GlobalDefinitions.putUnitOnHex(unit, destinationHex);
        unit.GetComponent<UnitDatabaseFields>().remainingMovement = destinationHex.GetComponent<HexDatabaseFields>().remainingMovement;

        // If this is the only unit in the target hex then update ZOC's
        if (destinationHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count < 2)
        {
            updateZOC(destinationHex);
        }

        // Finally remove all of the highlighting from hexes that were availabe for movement and reset the availableForMovement and remainingMovement fields
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (hex.GetComponent<HexDatabaseFields>().availableForMovement)
            {
                GlobalDefinitions.unhighlightHex(hex.gameObject);
                hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            }
        // By default we will always clear the bridge hex since is not available for movement but it is highlighted
        GameObject bridgeHex = GameObject.Find("Bridge_x5_y24");
        GlobalDefinitions.unhighlightHex(bridgeHex.gameObject);
        bridgeHex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
        bridgeHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
        bridgeHex.GetComponent<HexDatabaseFields>().availableForMovement = false;
    }

    /// <summary>
    /// This routine is for moving from Britain to the board.  There is no beginning hex.
    /// </summary>
    /// <param name="destinationHex"></param>
    /// <param name="beginningHex"></param>
    public void moveUnitFromBritain(GameObject destinationHex, GameObject selectedUnit)
    {
        //GlobalDefinitions.writeToLogFile("moveUnitFromBritain: executing with unit = " + selectedUnit.name + " hex = " + destinationHex.name);
        // Indcate that the unit has moved this turn
        selectedUnit.GetComponent<UnitDatabaseFields>().hasMoved = true;
        checkForAlliedCaptureOfStrategicInstallations(destinationHex, selectedUnit);

        selectedUnit.transform.parent = GlobalDefinitions.allUnitsOnBoard.transform;
        GlobalDefinitions.alliedUnitsOnBoard.Add(selectedUnit); // Add the unit to the OnBoardList
        selectedUnit.GetComponent<UnitDatabaseFields>().inBritain = false;
        selectedUnit.GetComponent<UnitDatabaseFields>().invasionAreaIndex = destinationHex.GetComponent<HexDatabaseFields>().invasionAreaIndex;

        // Change the unit's location to the target hex
        GlobalDefinitions.putUnitOnHex(selectedUnit, destinationHex);
        selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement = destinationHex.GetComponent<HexDatabaseFields>().remainingMovement;

        // If this is the only unit in the target hex then update ZOC's
        if (destinationHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count < 2)
        {
            updateZOC(destinationHex);
        }

        // Finally remove all of the highlighting from hexes that were availabe for movement and reset the availableForMovement and remainingMovement fields
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (hex.GetComponent<HexDatabaseFields>().availableForMovement)
            {
                GlobalDefinitions.unhighlightHex(hex.gameObject);
                hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            }
    }

    /// <summary>
    /// Udpates the ZOC flags on the hex passed and its neighbors
    /// </summary>
    /// <param name="hex"></param>
    public void updateZOC(GameObject hex)
    {
        if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
        {
            // This executes when the routine is called when it is the first unit to be placed on the hex.
            // This assumes that previous true ZOC values don't need to be changed to false.  It is only looking at add true ZOC flags.

            // Set the current hex to the zone of control of the occupying unit
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
            {
                hex.GetComponent<HexDatabaseFields>().inGermanZOC = true;

                //  Set the ZOC field if exerting to a neighbor and add any units that aren't already there
                foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                    if ((hex.GetComponent<BoolArrayData>().exertsZOC[(int)hexSide]) && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null))
                        hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().inGermanZOC = true;
            }
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
            {
                hex.GetComponent<HexDatabaseFields>().inAlliedZOC = true;

                //  Set the ZOC field if exerting to a neighbor and add any units that aren't already there
                foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                    if ((hex.GetComponent<BoolArrayData>().exertsZOC[(int)hexSide]) && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null))
                        hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().inAlliedZOC = true;
            }
        }

        else
        {
            // This executes when the routine is called because the last unit was removed from the hex
            // Check if the adjacent hexes exert ZOC onto this hex

            // Reset the ZOC flags on this hex.  Since the unit is being taken off, if it was taken off because of an exchange and there are no enemy units remaining either, the
            // flag for the current nationality will never be reset here.
            hex.GetComponent<HexDatabaseFields>().inGermanZOC = false;
            hex.GetComponent<HexDatabaseFields>().inAlliedZOC = false;

            checkAdjacentHexZOCImpact(hex);
            foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                if (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                    checkAdjacentHexZOCImpact(hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
        }
    }

    /// <summary>
    /// This is called when a hex has been vacated, it is used to reset the ZOC on the adjacent hexes from the hex that was vacated.
    /// </summary>
    /// <param name="hex"></param>
    public static void  checkAdjacentHexZOCImpact(GameObject hex)
    {
        hex.GetComponent<HexDatabaseFields>().inGermanZOC = false;
        hex.GetComponent<HexDatabaseFields>().inAlliedZOC = false;

        // Need to check if the hex is occupied
        if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                hex.GetComponent<HexDatabaseFields>().inAlliedZOC = true;
            else
                hex.GetComponent<HexDatabaseFields>().inGermanZOC = true;

        foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
        {
            if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                    && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<BoolArrayData>().exertsZOC[GlobalDefinitions.returnHexSideOpposide((int)hexSide)])
                    && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0))
            {
                if (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
                {
                    hex.GetComponent<HexDatabaseFields>().inGermanZOC = true;
                }
                else
                {
                    hex.GetComponent<HexDatabaseFields>().inAlliedZOC = true;
                }
            }
        }
    }

    /// <summary>
    /// Used to display multi-unit selection
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="nationality"></param>
    public void callMultiUnitDisplay(GameObject hex, GlobalDefinitions.Nationality nationality)
    {
        // Check if the hex has units of the right nationality on it
        if ((hex != null) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                && (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == nationality))
        {
            Canvas movementCanvas = new Canvas();
            Button cancelButton;
            float panelWidth = (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE;
            float panelHeight = 4 * GlobalDefinitions.GUIUNITIMAGESIZE;
            GlobalDefinitions.createGUICanvas("MultiUnitMovementGUIInstance",
                    panelWidth,
                    panelHeight,
                    ref movementCanvas);
            GlobalDefinitions.createText("Select a unit", "multiUnitMovement",
                    (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    0.5f * (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    movementCanvas);

            float xSeperation = (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE / hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count;
            float xOffset = xSeperation / 2;
            for (int index = 0; index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
            {
                Toggle tempToggle;

                tempToggle = GlobalDefinitions.createUnitTogglePair("multiUnitMovement" + index,
                    index * xSeperation + xOffset - 0.5f * panelWidth,
                    2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    movementCanvas,
                    hex.GetComponent<HexDatabaseFields>().occupyingUnit[index]);

                tempToggle.gameObject.AddComponent<MultiUnitMovementToggleRoutines>();
                tempToggle.GetComponent<MultiUnitMovementToggleRoutines>().unit = hex.GetComponent<HexDatabaseFields>().occupyingUnit[index];
                tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<MultiUnitMovementToggleRoutines>().selectUnitToMove());
            }
            cancelButton = GlobalDefinitions.createButton("CancelButton", "Cancel",
                    (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE / 2 - 0.5f * panelWidth,
                    0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    movementCanvas);
            cancelButton.gameObject.AddComponent<MultiUnitMovementToggleRoutines>();
            cancelButton.onClick.AddListener(cancelButton.GetComponent<MultiUnitMovementToggleRoutines>().cancelGui);
        }
        else
        {
            GlobalDefinitions.guiUpdateStatusMessage("No unit found on hex selected");
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().executeSelectUnit;
        }
    }

    /// <summary>
    /// Executes the step to reset unit fields for the start of a new turn
    /// </summary>
    public void initializeUnits()
    {
        // At first I only initialized a singe nationality at a time.  This doesn't work for isCommittedToAnAttack since at the end of one sides
        // turn they all need to be reset otherwise units that attacked in their turn would be committed in the opposing players turn.
        // The reality is that there is no reason to limit this to a nationality.

        // Load the units on the board into their respective global lists
        GlobalDefinitions.alliedUnitsOnBoard = GlobalDefinitions.returnNationUnitsOnBoard(GlobalDefinitions.Nationality.Allied);
        GlobalDefinitions.germanUnitsOnBoard = GlobalDefinitions.returnNationUnitsOnBoard(GlobalDefinitions.Nationality.German);

        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
        {
            unit.GetComponent<UnitDatabaseFields>().beginningTurnHex = unit.GetComponent<UnitDatabaseFields>().occupiedHex;
            unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor;
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
            unit.GetComponent<UnitDatabaseFields>().hasMoved = false;

            // Since I'm going to allow a user to perform his moves in multiple sections, I can't use a static flag for strategic movement available.  If I do this,
            // a unit that starts off adjacent to an enemy unit could move 1 unit and then the next section would make strategic movement available which is wrong.
            // So I will check every unit here if it is available for strategic movement in this turn.
            if (checkForAdjacentEnemy(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit))
                unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = false;
            else
                unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = true;
        }

        foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
        {
            unit.GetComponent<UnitDatabaseFields>().beginningTurnHex = unit.GetComponent<UnitDatabaseFields>().occupiedHex;
            unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor;
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
            unit.GetComponent<UnitDatabaseFields>().hasMoved = false;

            if (unit.GetComponent<UnitDatabaseFields>().armor || unit.GetComponent<UnitDatabaseFields>().airborne)
                if (checkForAdjacentEnemy(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit) ||
                        unit.GetComponent<UnitDatabaseFields>().unitInterdiction)
                    unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = false;
                else
                    unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = true;
            else
                unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = false;
        }

        foreach (Transform unit in GameObject.Find("Units In Britain").transform)
        {
            // Set the start hex to null in case any units moved back to Britain last turn
            unit.GetComponent<UnitDatabaseFields>().beginningTurnHex = null;
        }
    }

    public static bool checkIfMovementDone(GlobalDefinitions.Nationality nationality)
    {
        bool returnState = true;
        List<GameObject> onBoardList;

        if (nationality == GlobalDefinitions.Nationality.Allied)
            onBoardList = GlobalDefinitions.alliedUnitsOnBoard;
        else
            onBoardList = GlobalDefinitions.germanUnitsOnBoard;

        // Note: executing for all units so that messages will be displayed for all overstacked hexes.  That is why I'm waiting to return after the loop.
        foreach (GameObject unit in onBoardList)
            if (GlobalDefinitions.stackingLimitExceeded(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.GetComponent<UnitDatabaseFields>().nationality))
            {
                GlobalDefinitions.guiUpdateStatusMessage("Hex at(" + unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().xMapCoor + "," +
                        unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor + ") is overstacked");
                returnState = false;
            }

        return (returnState);
    }

    /// <summary>
    /// Returns the hexes that are currently available for airborne drops
    /// </summary>
    /// <returns></returns>
    public List<GameObject> getAirborneDropHexes()
    {
        List<GameObject> dropHexes = new List<GameObject>();
        List<GameObject> airborneHexesToCheck = new List<GameObject>();
        bool storeHex;

        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
        {
            // Only Allied units are able to drop.  Can only drop based on infantry or armor units
            if (unit.GetComponent<UnitDatabaseFields>().armor || unit.GetComponent<UnitDatabaseFields>().infantry)
            {

                airborneHexesToCheck.Add(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                // Set the remaining movement available for the hex at the airborne drop limit
                unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().remainingMovement = GlobalDefinitions.AirborneDropHexLimit;

                while (airborneHexesToCheck.Count > 0)
                {
                    if (airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement > 0)
                    {
                        foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                        {
                            if (airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                            {
                                // Note that unlike movement we don't need a path to get to a hex; the airborne unit is dropping onto the hex.
                                // Therefore all hexes will be added to hexesToCheck until we run get to the maximum drop distance
                                if (airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().remainingMovement <
                                        (airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement - 1))
                                {
                                    // This executes if the current hex is the shortest path to this hex.  Otherwise it means that a previous path got here
                                    // with more remaining movement so we don't need to check this hex again
                                    airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().remainingMovement =
                                            airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement - 1;
                                    airborneHexesToCheck.Add(airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                                }

                                storeHex = false;

                                if (!airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().mountain
                                        && !airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().neutralCountry
                                        && !airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().sea
                                        && !airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().bridge
                                        && (airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count < 2))
                                {

                                    // Check that if the hex has friendly units that it doesn't have two units on it
                                    if ((airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count == 1)
                                                && (airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
                                        storeHex = true;

                                    // The only other condition is if the hex is empty since we can drop in an enemy ZOC
                                    if (airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                                        storeHex = true;

                                    // See if the current neighbor needs to be saved for highlight for avaiable movement
                                    if (storeHex && !dropHexes.Contains(airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]))
                                    {
                                        // Note that bridge hexes are a special case.  They can be traversed for finding a drop but they can't be dropped on
                                        if (!airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().bridge)
                                            //GlobalDefinitions.highlightHexForMovement(airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                                            airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().availableForMovement = true;

                                        dropHexes.Add(airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                                    }
                                }
                            }
                        }
                    }
                    airborneHexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement = 0;
                    airborneHexesToCheck.RemoveAt(0);
                }
            }
        }
        return (dropHexes);
    }

    public void removeHexHighlighting()
    {
        // Finally remove all of the highlighting from hexes that were availabe for movement and reset the availableForMovement and remainingMovement fields
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (hex.GetComponent<HexDatabaseFields>().availableForMovement)
            {
                GlobalDefinitions.unhighlightHex(hex.gameObject);
                hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            }
        // And remove the hex references
        //while (storedHexes.Count > 0)
        //    storedHexes.RemoveAt(0);
        //while (airborneHexesToCheck.Count > 0)
        //    airborneHexesToCheck.RemoveAt(0);
    }

    /// <summary>
    /// This routine loads the availableReinforcementPorts list with ports and inland ports that are occucpied by Allied units.
    /// These can be used for landing reinforcements.
    /// </summary>
    public void determineAvailableReinforcementPorts()
    {
        GlobalDefinitions.availableReinforcementPorts.Clear();
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            if (hex.GetComponent<HexDatabaseFields>().inlandPort && GlobalDefinitions.checkIfInlandPortClear(hex.gameObject) &&
                    (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                    (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
                GlobalDefinitions.availableReinforcementPorts.Add(hex.gameObject);
            else if (hex.GetComponent<HexDatabaseFields>().coastalPort && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                    (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
                GlobalDefinitions.availableReinforcementPorts.Add(hex.gameObject);
            else if (hex.GetComponent<HexDatabaseFields>().successfullyInvaded && hex.GetComponent<HexDatabaseFields>().alliedControl)
                GlobalDefinitions.availableReinforcementPorts.Add(hex.gameObject);
        }

        //GlobalDefinitions.writeToLogFile("determineAvailableReinforcementPorts: number of reinforcement ports available = " + GlobalDefinitions.availableReinforcementPorts.Count + " turn = " + GlobalDefinitions.turnNumber);
        //foreach (GameObject port in GlobalDefinitions.availableReinforcementPorts)
        //    GlobalDefinitions.writeToLogFile("determineAvailableReinforcementPorts:     " + port.name);
    }

    /// <summary>
    /// This routine returns hexes that are avaiable for landing reinforcement of the unit type passed.  It also checks for supply capacity available.
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public List<GameObject> returnReinforcementLandingHexes(GameObject unit)
    {
        List<GameObject> landingHexes = new List<GameObject>();

        if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
        {
            // There are available units to be landed this turn.  Go through and highlight all ports and invasion beaches that 
            // can still accept the type of unit selected this turn and has supply capacity available
            foreach (GameObject hex in GlobalDefinitions.availableReinforcementPorts)
                if (GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().checkForAvailableSupplyCapacity(hex))
                {
                    // Invasion hexes don't need to be free of enemy ZOC and they don't need to be occupied.
                    if (hex.GetComponent<HexDatabaseFields>().successfullyInvaded)
                    {
                        //GlobalDefinitions.writeToLogFile("returnReinforcementLandingHexes: unit " + unit.name + " hex " + hex.name + " allied control = " + hex.GetComponent<HexDatabaseFields>().alliedControl + " hex available = " + hexAvailableForUnitTypeReinforcements(hex, unit)); 
                        // The hex cannot have been in German hands in order to be able to land
                        if ((hex.GetComponent<HexDatabaseFields>().alliedControl) && (hexAvailableForUnitTypeReinforcements(hex, unit)))
                        {
                            landingHexes.Add(hex);
                            //GlobalDefinitions.highlightHexForMovement(hex.gameObject);
                            hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                        }
                    }
                    //  Coastal ports have to be occupied and free of German ZOC
                    else if (hex.GetComponent<HexDatabaseFields>().coastalPort)
                    {
                        if ((!hex.GetComponent<HexDatabaseFields>().inGermanZOC) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                                (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) &&
                                (hexAvailableForUnitTypeReinforcements(hex, unit)))
                        {
                            landingHexes.Add(hex);
                            //GlobalDefinitions.highlightHexForMovement(hex);
                            hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                        }
                    }
                    // In addition to the coastal port requirements, inland ports must have their dependent hexes free from German occupation
                    else if (hex.GetComponent<HexDatabaseFields>().inlandPort)
                    {
                        if ((!hex.GetComponent<HexDatabaseFields>().inGermanZOC) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                                (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) &&
                                (hexAvailableForUnitTypeReinforcements(hex, unit)))
                        {
                            landingHexes.Add(hex);
                            //GlobalDefinitions.highlightHexForMovement(hex);
                            hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                        }
                    }
                }
        }
        return (landingHexes);
    }

    /// <summary>
    /// Returns true if the unit passed can land at the hex passed
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="hex"></param>
    /// <returns></returns>
    public bool returnHexAvailabilityForLanding(GameObject unit, GameObject hex)
    {
        if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
        {
            // There are available units to be landed this turn.  
            // Check if the hex can still accept the type of unit selected this turn and has supply capacity available
            if (GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().checkForAvailableSupplyCapacity(hex))
            {
                // Invasion hexes don't need to be free of enemy ZOC and they don't need to be occupied.
                if (hex.GetComponent<HexDatabaseFields>().successfullyInvaded)
                {
                    //GlobalDefinitions.writeToLogFile("returnHexAvailabilityForLanding: unit " + unit.name + " hex " + hex.name + " allied control = " + hex.GetComponent<HexDatabaseFields>().alliedControl + " hex available = " + hexAvailableForUnitTypeReinforcements(hex, unit)); 
                    // The hex cannot have been in German hands in order to be able to land
                    if ((hex.GetComponent<HexDatabaseFields>().alliedControl) && (hexAvailableForUnitTypeReinforcements(hex, unit)))
                        return (true);
                }

                //  Coastal ports have to be occupied and free of German ZOC
                else if (hex.GetComponent<HexDatabaseFields>().coastalPort)
                {
                    if ((!hex.GetComponent<HexDatabaseFields>().inGermanZOC) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) &&
                            (hexAvailableForUnitTypeReinforcements(hex, unit)))
                        return (true);
                }
                // In addition to the coastal port requirements, inland ports must have their dependent hexes free from German occupation
                else if (hex.GetComponent<HexDatabaseFields>().inlandPort)
                {
                    if ((!hex.GetComponent<HexDatabaseFields>().inGermanZOC) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) &&
                            (hexAvailableForUnitTypeReinforcements(hex, unit)))
                        return (true);
                }
            }
        }
        return (false);
    }

    /// <summary>
    /// This routine returns true if it is availalble for the type of reinforcement unit on the hex passed
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool hexAvailableForUnitTypeReinforcements(GameObject hex, GameObject unit)
    {
        //GlobalDefinitions.writeToLogFile("hexAvailableForUnitTypeReinforcements: invasion area index = " + hex.GetComponent<HexDatabaseFields>().invasionAreaIndex + " invasion turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn + " armor units used this turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn + " infantry units used this turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn);
        // Need to check for a special case; can't land at the two ports in Germany
        if ((hex.name != "InlandPort_x2_y29") && (hex.name != "InlandPort_x3_y32"))
        {
            if (unit.GetComponent<UnitDatabaseFields>().armor)
            {
                if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn == 1)
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnArmor)
                        return (true);
                }
                else if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn == 2)
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnArmor)
                        return (true);
                }
                else
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].totalUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].divisionsPerTurn)
                        return (true);
                }
            }
            // Note that airborne units landing by sea count against the infantry limit
            else if (unit.GetComponent<UnitDatabaseFields>().infantry || unit.GetComponent<UnitDatabaseFields>().airborne)
            {
                if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn == 1)
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnInfantry)
                        return (true);
                    // This will execute if the infantry limit is reached, check to see if there is the armor limit can be used
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].firstTurnArmor)
                        return (true);
                }
                else if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn == 2)
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnInfantry)
                        return (true);
                    // This will execute if the infantry limit is reached, check to see if there is the armor limit can be used
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].secondTurnArmor)
                        return (true);
                }
                else
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].totalUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].divisionsPerTurn)
                        return (true);
                }
            }
            else
            {
                // This is for HQ units and they are only avaialable on the third turn or later
                if ((GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn != 1) &&
                            (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn != 2))
                {
                    if (GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].totalUnitsUsedThisTurn <
                            GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].divisionsPerTurn)
                        return (true);
                }
            }
        }
        return (false);
    }

    /// <summary>
    /// This is used to determine if an Allied unit that can move to the right type of hex can return to Britain
    /// This is he check used at the end of a turn because in the beginning of a turn a unit on a coastal invasion hex can return even if it is in German ZOC
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private bool checkForUnitReturnToBritain(GameObject targetHex)
    {
        if (targetHex.GetComponent<HexDatabaseFields>().successfullyInvaded ||
                targetHex.GetComponent<HexDatabaseFields>().coastalPort ||
                targetHex.GetComponent<HexDatabaseFields>().inlandPort)
        {
            if (targetHex.GetComponent<HexDatabaseFields>().inGermanZOC)
                return (false);
            else if (targetHex.GetComponent<HexDatabaseFields>().successfullyInvaded ||
                targetHex.GetComponent<HexDatabaseFields>().coastalPort)
                return (true);
            else if (targetHex.GetComponent<HexDatabaseFields>().inlandPort)
            {
                foreach (GameObject hex in targetHex.GetComponent<HexDatabaseFields>().controlHexes)
                {
                    if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German))
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }
        else
            return (false);
    }

    /// <summary>
    /// This is used to determine if an Allied unit that begins its move on the right type of hex can return to Britain
    /// This is the check used at the beginning of a turn because in the beginning of a turn a unit on a coastal invasion hex can return even if it is in German ZOC
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private bool checkForUnitReturnToBritainBeginningOfTurn(GameObject targetHex)
    {
        if (targetHex.GetComponent<HexDatabaseFields>().successfullyInvaded ||
                targetHex.GetComponent<HexDatabaseFields>().coastalPort ||
                targetHex.GetComponent<HexDatabaseFields>().inlandPort)
        {
            if (targetHex.GetComponent<HexDatabaseFields>().successfullyInvaded)
                return (true); // do this check first since it isn't in impact being in ZOC at the beginning of turn if it's on an invasion hex
            else if (targetHex.GetComponent<HexDatabaseFields>().inGermanZOC)
                return (false);
            else if (targetHex.GetComponent<HexDatabaseFields>().coastalPort)
                return (true);
            else if (targetHex.GetComponent<HexDatabaseFields>().inlandPort)
            {
                foreach (GameObject hex in targetHex.GetComponent<HexDatabaseFields>().controlHexes)
                {
                    if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German))
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }
        else
            return (false);
    }

    /// <summary>
    /// This routine returns the sea hex that a unit can use to go back to Britain
    /// </summary>
    /// <param name="targetHex"></param>
    /// <returns></returns>
    private GameObject getBritainReturnHex(GameObject targetHex)
    {
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (hex.GetComponent<HexDatabaseFields>().invasionTarget == targetHex)
            {
                hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                return (hex.gameObject);
            }

        // The port of Rotterdam is a special case in that there is no sea hex associated with it.  Will highligh the sea hex at (7 21)
        if ((targetHex.GetComponent<HexDatabaseFields>().xMapCoor == 7) &&
                (targetHex.GetComponent<HexDatabaseFields>().xMapCoor == 21))
        {
            GlobalDefinitions.getHexAtXY(7, 21).GetComponent<HexDatabaseFields>().availableForMovement = true;
            return (GlobalDefinitions.getHexAtXY(7, 21));
        }

        // If we get here return a null since the hex isn't available to return to Britain
        return null;
    }

    /// <summary>
    ///  This routine is called when a unit is selected that can return back to Britain.  To signify this the sea hex for the hex will be highlighted.
    /// </summary>
    /// <param name="hex"></param>
    private void highlightBritainReturnHex(GameObject targetHex)
    {
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (hex.GetComponent<HexDatabaseFields>().invasionTarget == targetHex)
            {
                //storedHexes.Add(hex.gameObject);
                GlobalDefinitions.highlightHexForMovement(hex.gameObject);
                hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
            }

        // The port of Rotterdam is a special case in that there is no sea hex associated with it.  Will highligh the sea hex at (7 21)
        if ((targetHex.GetComponent<HexDatabaseFields>().xMapCoor == 7) &&
                (targetHex.GetComponent<HexDatabaseFields>().xMapCoor == 21))
        {
            GlobalDefinitions.highlightHexForMovement(GlobalDefinitions.getHexAtXY(7, 21));
            GlobalDefinitions.getHexAtXY(7, 21).GetComponent<HexDatabaseFields>().availableForMovement = true;
            //storedHexes.Add(GlobalDefinitions.getHexAtXY(7, 21));
        }
    }

    /// <summary>
    /// This routine takes the unit passed and moves it back to Britain
    /// </summary>
    /// <param name="beginningHex"></param>
    /// <param name="unit"></param>
    /// <param name="resultOfMovement"></param>
    public void moveUnitBackToBritain(GameObject beginningHex, GameObject unit, bool resultOfMovement)
    {
        //GlobalDefinitions.writeToLogFile("moveUnitBackToBritain: unit = " + unit.name + " hex = " + beginningHex.name + " result of movement = " + resultOfMovement);
        // If the unit is being placed back in Britain by an undo then the turn available shouldn't change.  If it's
        // moving back as a result of movement then it needs to wait until the next turn before it can be used.
        if (resultOfMovement)
            unit.GetComponent<UnitDatabaseFields>().turnAvailable = GlobalDefinitions.turnNumber + 1;

        // Take the unit out of the occupyingUnits field of the current hex
        GlobalDefinitions.removeUnitFromHex(unit, beginningHex);

        // Check if the unit was on a sea hex.  If not then it needs to give back it's supply
        if (!unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea)
        {
            // Since this unit is moving back to Britain is needs to give back its supply to its source.  Need to check that it is in supply
            // since an out of supply unit won't have a supply source
            if ((unit.GetComponent<UnitDatabaseFields>().inSupply) && (unit.GetComponent<UnitDatabaseFields>().supplySource != null))
                unit.GetComponent<UnitDatabaseFields>().supplySource.GetComponent<HexDatabaseFields>().unassignedSupply++;
            unit.GetComponent<UnitDatabaseFields>().supplySource = null;
            unit.GetComponent<UnitDatabaseFields>().inSupply = true;
        }

        // Change the unit's location to its Britain location 
        unit.GetComponent<UnitDatabaseFields>().occupiedHex = null;
        // Remove it from the OnBoard list
        GlobalDefinitions.alliedUnitsOnBoard.Remove(unit);
        // Reset the invasion index
        unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex = -1;
        // If the unit started its turn adjacent to a German unit the strategic movement flag would be off - reset it
        unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = true;

        unit.transform.position = unit.GetComponent<UnitDatabaseFields>().locationInBritain;
        unit.GetComponent<SpriteRenderer>().sortingOrder = 0;
        unit.transform.parent = GameObject.Find("Units In Britain").transform;
        unit.GetComponent<UnitDatabaseFields>().inBritain = true;

        // There is no case where a unit in Britain should be highlighted
        unit.GetComponent<SpriteRenderer>().material.color = Color.white;

        // Finally remove all of the highlighting from hexes that were availabe for movement and reset the availableForMovement and remainingMovement fields
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (hex.GetComponent<HexDatabaseFields>().availableForMovement)
            {
                GlobalDefinitions.unhighlightHex(hex.gameObject);
                hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            }
    }

    /// <summary>
    /// This routine is used to determine if an Allied unit has ended its movement on a strategic installation
    /// </summary>
    /// <param name="hex"></param>
    public void checkForAlliedCaptureOfStrategicInstallations(GameObject hex, GameObject unit)
    {
        // Note that once a hex is captured it counts for replacements regardless if it is recaptured
        // Brest
        if (hex == GlobalDefinitions.getHexAtXY(22, 1))
            if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
            {
                GlobalDefinitions.alliedCapturedBrest = true;
                GlobalDefinitions.unhighlightHex(hex);
            }

        // Rotterdam
        if (hex == GlobalDefinitions.getHexAtXY(8, 23))
            if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
            {
                GlobalDefinitions.alliedCapturedRotterdam = true;
                GlobalDefinitions.unhighlightHex(hex);
            }

        // Boulogne
        if (hex == GlobalDefinitions.getHexAtXY(14, 16))
            if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
            {
                GlobalDefinitions.alliedCapturedBoulogne = true;
                GlobalDefinitions.unhighlightHex(hex);
            }
    }

    /// <summary>
    /// This routine will return true if there are infantry or armor units in the dead pile
    /// </summary>
    /// <returns></returns>
    public bool checkIfAlliedReplacementsAvailable()
    {
        foreach (Transform unit in GameObject.Find("Units Eliminated").transform)
            if (unit.GetComponent<UnitDatabaseFields>().armor || unit.GetComponent<UnitDatabaseFields>().infantry)
                return true;
        return false;
    }

    /// <summary>
    /// This routine will select an allied unit from the dead pile and move it to its location in Britain
    /// </summary>
    /// <returns></returns>
    public void selectAlliedReplacementUnit()
    {
        GameObject selectedUnit;
        selectedUnit = GlobalDefinitions.getUnitWithoutHex(Input.mousePosition);

        //  Check for valid unit
        if (selectedUnit == null)
        {
            GlobalDefinitions.guiUpdateStatusMessage("No unit selected");
        }
        // The unit must be in the dead pile and can only select armor or infantry units
        else if (selectedUnit.transform.parent.gameObject.name == "Units Eliminated")
        {
            if (selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
            {
                if (selectedUnit.GetComponent<UnitDatabaseFields>().armor || selectedUnit.GetComponent<UnitDatabaseFields>().infantry)
                {
                    if (selectedUnit.GetComponent<UnitDatabaseFields>().attackFactor <= GlobalDefinitions.alliedReplacementsRemaining)
                    {
                        GlobalDefinitions.alliedReplacementsRemaining -= selectedUnit.GetComponent<UnitDatabaseFields>().attackFactor;
                        selectedUnit.transform.position = selectedUnit.GetComponent<UnitDatabaseFields>().locationInBritain;
                        selectedUnit.GetComponent<UnitDatabaseFields>().unitEliminated = false;
                        selectedUnit.transform.parent = GameObject.Find("Units In Britain").transform;
                        selectedUnit.GetComponent<UnitDatabaseFields>().inBritain = true;
                    }
                    else
                        GlobalDefinitions.guiUpdateStatusMessage("Not enough factors remaining for selected unit");
                }
                else
                    GlobalDefinitions.guiUpdateStatusMessage("Can only select infantry or armor for replacement");
            }
            else
                GlobalDefinitions.guiUpdateStatusMessage("Must select an Allied unit");
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Unit not on OOB sheet");
    }

    /// <summary>
    /// This routine will select an German unit from the dead pile
    /// </summary>
    /// <returns></returns>
    public bool selectGermanReplacementUnit(GameObject selectedUnit)
    {
        //GameObject selectedUnit;
        //selectedUnit = GlobalDefinitions.getUnitWithoutHex();

        //  Check for valid unit
        if (selectedUnit == null)
        {
            GlobalDefinitions.guiUpdateStatusMessage("No unit selected");
            return false;
        }
        // The unit must be in the dead pile and can only select armor or infantry units
        else if (selectedUnit.transform.parent.gameObject.name == "Units Eliminated")
        {
            if (selectedUnit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
            {
                if (!selectedUnit.GetComponent<UnitDatabaseFields>().HQ)
                {
                    if (selectedUnit.GetComponent<UnitDatabaseFields>().attackFactor <= GlobalDefinitions.germanReplacementsRemaining)
                    {
                        selectedUnit.GetComponent<UnitDatabaseFields>().unitEliminated = false;
                        GlobalDefinitions.selectedUnit = selectedUnit;
                        return true;
                    }
                    else
                    {
                        GlobalDefinitions.guiUpdateStatusMessage("Not enough factors remaining for selected unit");
                        return false;
                    }
                }
                else
                {
                    GlobalDefinitions.guiUpdateStatusMessage("Cannot select HQ units for replacement");
                    return false;
                }
            }
            else
            {
                GlobalDefinitions.guiUpdateStatusMessage("Must select a German unit");
                return false;
            }
        }
        else
        {
            GlobalDefinitions.guiUpdateStatusMessage("Unit not on OOB sheet");
            return false;
        }
    }

    /// <summary>
    /// Used during turn initialization to add any additional factors to the number of allied replacement factors
    /// </summary>
    public void calculateAlliedRelacementFactors()
    {
        if (GlobalDefinitions.alliedCapturedBoulogne)
            GlobalDefinitions.alliedReplacementsRemaining++;
        if (GlobalDefinitions.alliedCapturedBrest)
            GlobalDefinitions.alliedReplacementsRemaining++;
        if (GlobalDefinitions.alliedCapturedRotterdam)
            GlobalDefinitions.alliedReplacementsRemaining++;
        GlobalDefinitions.writeToLogFile("calculateAlliedReplacementFactors: Turn " + GlobalDefinitions.turnNumber + "  Allied Replacement Factor Total = " + GlobalDefinitions.alliedReplacementsRemaining);
    }

    /// <summary>
    /// This routine will highlight the German replacment hexes that aren't in Allied control
    /// </summary>
    public void highlightGermanReplacementHexes()
    {
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (!hex.GetComponent<HexDatabaseFields>().alliedControl && hex.GetComponent<HexDatabaseFields>().germanRepalcement)
            {
                GlobalDefinitions.highlightHexForMovement(hex.gameObject);
                hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
            }
    }

    /// <summary>
    /// This routine is called at the end of a movement phase.  Any HQ units in an enemy ZOC are eliminated
    /// </summary>
    /// <param name="nationality"></param>
    public void removeHQInEnemyZOC(GlobalDefinitions.Nationality nationality)
    {
        // I can't remove units from the list while checking so strore them away and remove later
        // Can't get a count fromt the GameObject.Find result so can't index the search
        List<GameObject> unitsToDelete = new List<GameObject>();

        foreach (Transform unitTransform in GlobalDefinitions.allUnitsOnBoard.transform)
            if ((unitTransform.GetComponent<UnitDatabaseFields>().nationality == nationality) &&
                    unitTransform.GetComponent<UnitDatabaseFields>().HQ &&
                    (GlobalDefinitions.hexInEnemyZOC(unitTransform.GetComponent<UnitDatabaseFields>().occupiedHex, nationality)))
                unitsToDelete.Add(unitTransform.gameObject);

        for (int index = 0; index < unitsToDelete.Count; index++)
        {
            GlobalDefinitions.guiUpdateStatusMessage("HQ unit " + unitsToDelete[index].name + " ended its turn in enemy ZOC so it is eliminated");
            GlobalDefinitions.moveUnitToDeadPile(unitsToDelete[index]);
        }

    }

    /// <summary>
    /// This routine will set the number of airborne drops that can be executed this turn.  It sets the GlobalDefinitions.maxNumberAirborneDropsThisTurn variable.
    /// </summary>
    /// <returns></returns>
    /// </summary>
    public void setAirborneLimits()
    {
        // Set the maximum number of airborne drops available this turn.
        // The maximum is usually 3, but this can be limited if this is the first or second turn after an invasion.
        // Note that for the second invasion the number of airborne drops will be limited by the TIC limits of the 
        // second invasion area.  In other words, if the second invasion areas has a limit of 1 I am not making 2
        // drops availabe outside of the second invasion area.
        // This routine sets the airborne limits for the current turn
        GlobalDefinitions.writeToLogFile("Executing setAirborneLimits: firstInvasionIndex - " + GlobalDefinitions.firstInvasionAreaIndex);
        if (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].invaded && (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].turn < 3))
        {
            if (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].turn == 1)
                GlobalDefinitions.maxNumberAirborneDropsThisTurn = GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].firstTurnAirborne;
            else
                GlobalDefinitions.maxNumberAirborneDropsThisTurn = GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].secondTurnAirborne;
        }
        else if ((GlobalDefinitions.secondInvasionAreaIndex != -1) && GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].invaded &&
                (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].turn < 3))
        {
            if (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].turn == 1)
                GlobalDefinitions.maxNumberAirborneDropsThisTurn = GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].firstTurnAirborne;
            else
                GlobalDefinitions.maxNumberAirborneDropsThisTurn = GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].secondTurnAirborne;
        }
        else
            GlobalDefinitions.maxNumberAirborneDropsThisTurn = GlobalDefinitions.NormalAirborneDropLimit;
        GlobalDefinitions.guiUpdateStatusMessage("Max number of airborne drops this turn = " + GlobalDefinitions.maxNumberAirborneDropsThisTurn);
    }

    /// <summary>
    /// This routine returns true if airborne units are available in Britain for the current turn
    /// </summary>
    /// <returns></returns>
    public bool airborneUnitsAvaialbleInBritain()
    {
        foreach (Transform unitTransform in GameObject.Find("Units In Britain").transform)
            if ((unitTransform.GetComponent<UnitDatabaseFields>().airborne) && (unitTransform.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber))
                return (true);
        return (false);
    }

    /// <summary>
    /// This routine takes the passed unit and determies if it is a valid unit for airborne drop and then processes it
    /// </summary>
    /// <param name="selectedUnit"></param>
    public void processAirborneUnitSelection(GameObject selectedUnit)
    {
        List<GameObject> dropHexes = new List<GameObject>();

        // Set the GlobalDefinitions.selectedUnit to null.  It will get set below if a valid unit has been selected
        GlobalDefinitions.selectedUnit = null;

        if (selectedUnit != null)
        {
            // Check if there we have already dropped the max this turn
            if (GlobalDefinitions.currentAirborneDropsThisTurn == GlobalDefinitions.maxNumberAirborneDropsThisTurn)
            {
                // The only available option at this point it to select a unit to perform an undo
                if (selectedUnit.GetComponent<UnitDatabaseFields>().airborne &&
                        GlobalDefinitions.alliedUnitsOnBoard.Contains(selectedUnit) &&
                        (selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex == null))
                {
                    // This is an aiborne unit already on the board that was just dropped (because the beginning turn hex is null).
                    // Highight this in case the user wants to undo the drop
                    GlobalDefinitions.highlightUnit(selectedUnit);
                    GlobalDefinitions.selectedUnit = selectedUnit;
                }
                else
                    GlobalDefinitions.guiUpdateStatusMessage("No more airborne drops are available this turn");
            }

            else if (!selectedUnit.GetComponent<UnitDatabaseFields>().airborne)
            {
                if (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
                    GlobalDefinitions.guiDisplayUnitsOnHex(selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex);
                GlobalDefinitions.guiUpdateStatusMessage("The unit selected is not an airborne unit.  Please select an airborne unit");
            }

            else if (GlobalDefinitions.alliedUnitsOnBoard.Contains(selectedUnit) && (selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex == null))
            {
                // This is an aiborne unit already on the board that was just dropped (because the beginning turn hex is null).
                // Highight this in case the user wants to undo the drop
                GlobalDefinitions.highlightUnit(selectedUnit);
                GlobalDefinitions.selectedUnit = selectedUnit;
            }

            else if (!selectedUnit.GetComponent<UnitDatabaseFields>().inBritain)
            {
                if (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
                    GlobalDefinitions.guiDisplayUnitsOnHex(selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex);
                GlobalDefinitions.guiUpdateStatusMessage("Only airborne units in Britain can use airborne drop");
            }

            // Check if it is available this turn
            else if (selectedUnit.GetComponent<UnitDatabaseFields>().turnAvailable > GlobalDefinitions.turnNumber)
            {
                GlobalDefinitions.guiUpdateStatusMessage("Airborne unit not availabe until turn " + selectedUnit.GetComponent<UnitDatabaseFields>().turnAvailable);
            }

            else
            {
                GlobalDefinitions.highlightUnit(selectedUnit);
                GlobalDefinitions.selectedUnit = selectedUnit;
                dropHexes = getAirborneDropHexes();  // Note that the selected hex doesn't have any impact on drop hexes
                foreach (GameObject hex in dropHexes)
                    GlobalDefinitions.highlightHexForMovement(hex);

            }
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("No unit selected");
    }

    /// <summary>
    /// This routine processes the hex selected to drop an airborne unit on
    /// </summary>
    /// <param name="selectedHex"></param>
    public void processAirborneDrop(GameObject selectedHex)
    {
        if (selectedHex == null)
        {
            GlobalDefinitions.guiUpdateStatusMessage("No valid hex selected");

            // In movement mode getUnitMoveDestination takes case of a unit not moving (unhighlighting, ect...) 
            // But I can't use it here because I have to not count it against the airborne drop limit
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().removeHexHighlighting();
            GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
            GlobalDefinitions.selectedUnit = null;
        }

        else if (selectedHex.GetComponent<HexDatabaseFields>().availableForMovement)
        {
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnitFromBritain(selectedHex, GlobalDefinitions.selectedUnit);
            selectedHex.GetComponent<HexDatabaseFields>().alliedControl = true;
            GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().removeHexHighlighting();
            //GlobalDefinitions.airborneDropsTookPlaceThisTurn = true;
            GlobalDefinitions.currentAirborneDropsThisTurn++;
            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement = 0;
        }
        else
        {
            // In movement mode getUnitMoveDestination takes case of a unit not moving (unhighlighting, ect...) 
            // But I can't use it here because I have to not count it against the airborne drop limit
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().removeHexHighlighting();
            GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
            GlobalDefinitions.selectedUnit = null;
        }
    }

    /// <summary>
    /// Wrapper for processing a unit selected for movement
    /// </summary>
    /// <param name="selectedUnit"></param>
    public void processUnitSelectionForMovement(GameObject selectedUnit, GlobalDefinitions.Nationality currentNationality)
    {
        if (selectedUnit != null)
            // Selecting a reinforcing unit from Britain
            if ((currentNationality == GlobalDefinitions.Nationality.Allied) && selectedUnit.GetComponent<UnitDatabaseFields>().inBritain)
                // Check that the unit selected is available for the current turn
                if (selectedUnit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber)
                {
                    GlobalDefinitions.highlightUnit(selectedUnit);
                    // Get the landing hexes and highlight them
                    foreach (GameObject hex in returnReinforcementLandingHexes(selectedUnit))
                        GlobalDefinitions.highlightHexForMovement(hex);
                }
                else
                {
                    GlobalDefinitions.guiUpdateStatusMessage("Unit selected is not available until turn " + selectedUnit.GetComponent<UnitDatabaseFields>().turnAvailable);
                }
            // Check if the unit doesn't occupy a hex (we've already checked for a reinforcement unit)
            else if (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex == null)
                GlobalDefinitions.guiUpdateStatusMessage("Unit selected must be on the board");

            // Selecting a unit on the board
            else
            {

                // AI TESTING: For debugging the AI algorithm I am setting the hex movement values here for the selected unit
                //AIRoutines.setUnitMovementValues(selectedUnit);

                GlobalDefinitions.startHex = getUnitToMove(selectedUnit);
            }
        else
            GlobalDefinitions.guiUpdateStatusMessage("No unit selected");
    }
}
