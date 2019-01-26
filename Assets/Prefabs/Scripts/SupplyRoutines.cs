using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SupplyRoutines : MonoBehaviour
{
    /// <summary>
    /// This routine will go through all the Allied units on the board and set their supply status.
    /// It returns true if the user needs to make decisions about what units are out of supply
    /// </summary>
    /// <param name="endOfTurn"></param>
    /// <returns></returns>
    public bool setAlliedSupplyStatus(bool endOfTurn)
    {
        int supplyRange = 0;
        bool userIntervention = false;

        // This is the routine that sets GlobalDefinitions.supplySources 
        GlobalDefinitions.supplySources.Clear();

        // First reset all the supply sources on hexes since it has the sources from the last check
        // Also reset unassigned supply to the supply capacity

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().supplySources.Clear();
            hex.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied.Clear();
            hex.GetComponent<HexDatabaseFields>().unassignedSupply = hex.GetComponent<HexDatabaseFields>().supplyCapacity;
        }

        foreach (GameObject supplySource in GlobalDefinitions.availableReinforcementPorts)
        {
            // Successfully invaded hexes are supply sources
            if (supplySource.GetComponent<HexDatabaseFields>().successfullyInvaded)
            {
                GlobalDefinitions.supplySources.Add(supplySource);
                supplyRange = GlobalDefinitions.numberHQOnHex(supplySource) * GlobalDefinitions.supplyRangeIncrement;
                // An invasion site has a minimum range of GlobalDefinitions.supplyRangeIncrement
                if (supplyRange == 0)
                    supplyRange = GlobalDefinitions.supplyRangeIncrement;
                supplySource.GetComponent<HexDatabaseFields>().supplyRange = supplyRange;
                setHexAsSupplySource(supplySource, supplyRange, GlobalDefinitions.Nationality.Allied);
            }
            // Ports that have Allied units on them are supply sources
            else
            {
                GlobalDefinitions.supplySources.Add(supplySource);
                // Note that if there are no HQ units on the port the range will be 0 which means it can only supply the units on the port
                supplyRange = GlobalDefinitions.numberHQOnHex(supplySource) * GlobalDefinitions.supplyRangeIncrement;
                supplySource.GetComponent<HexDatabaseFields>().supplyRange = supplyRange;
                setHexAsSupplySource(supplySource, supplyRange, GlobalDefinitions.Nationality.Allied);
            }
        }

        // At this point all the hexes have their supply sources set.  Go through and set the bit to show that the hex is in supply for the supply status gui
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().alliedInSupply = false;
            if (hex.GetComponent<HexDatabaseFields>().supplySources.Count > 0)
                hex.GetComponent<HexDatabaseFields>().alliedInSupply = true;
        }

        // Go through all the allied units and flag them as unsupplied.
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
        {
            // Store this unit to all of its potential supply sources
            foreach (GameObject supplySource in unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources)
                supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied.Add(unit);

            unit.GetComponent<UnitDatabaseFields>().inSupply = false; // Note they aren't being highlighted here
                                                                      // When it comes time to move, it is important that the remaining movement be set to 1 for unsupplied units
            unit.GetComponent<UnitDatabaseFields>().remainingMovement = 1;
        }

        // First check to see if units are in coastal hexes since they are in supply by being on the coast.  Note, do not need to worry about invsion hexes.
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            if (!unit.GetComponent<UnitDatabaseFields>().inSupply &&
                    unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().coast &&
                    !unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().successfullyInvaded)
            {
                unit.GetComponent<UnitDatabaseFields>().supplySource = unit.GetComponent<UnitDatabaseFields>().occupiedHex;
                unit.GetComponent<UnitDatabaseFields>().inSupply = true;
                unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor;
                GlobalDefinitions.unhighlightUnit(unit);
            }

        // Store all of the units that are out of supply with a supply source
        GameObject storedUnit;
        List<GameObject> outOfSupplyUnitsWithSupplySources = new List<GameObject>();

        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            if (!unit.GetComponent<UnitDatabaseFields>().inSupply &&
                    (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources.Count > 0))
                outOfSupplyUnitsWithSupplySources.Add(unit);

        // Sort the list by the lowest supply available to the unit
        for (int firstIndex = 0; firstIndex < outOfSupplyUnitsWithSupplySources.Count; firstIndex++)
            for (int secondIndex = (firstIndex + 1); secondIndex < outOfSupplyUnitsWithSupplySources.Count; secondIndex++)
                if (totalSupplyAvailable(outOfSupplyUnitsWithSupplySources[firstIndex]) > totalSupplyAvailable(outOfSupplyUnitsWithSupplySources[secondIndex]))
                {
                    storedUnit = outOfSupplyUnitsWithSupplySources[firstIndex];
                    outOfSupplyUnitsWithSupplySources[firstIndex] = outOfSupplyUnitsWithSupplySources[secondIndex];
                    outOfSupplyUnitsWithSupplySources[secondIndex] = storedUnit;
                }

        // Now go through and assign supply
        for (int index = 0; index < outOfSupplyUnitsWithSupplySources.Count; index++)
            assignAlliedSupply(outOfSupplyUnitsWithSupplySources[index]);

        // If it is the end of a turn and the unit just went out of supply, ignore it
        if (endOfTurn)
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                if (!unit.GetComponent<UnitDatabaseFields>().inSupply && (unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply == 0))
                    unit.GetComponent<UnitDatabaseFields>().inSupply = true;

        // Check to see if there are unsupplied units with supply sources listed.  This will trigger user intervention
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            if (!unit.GetComponent<UnitDatabaseFields>().inSupply &&
                    (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources.Count > 0))
                userIntervention = true;

        //Go through each of the supply sources and see if supply can be switched from units that have been out of supply with those that are in supply
        foreach (GameObject supplySource in GlobalDefinitions.supplySources)
        {
            for (int i = 0; i < supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied.Count; i++)
            {
                // Note I'm checking increments out of suppply here because you don't want a unit that was out of supply last turn get supply set above and then 
                // swapped here.  If you do that you're actually resetting two units out of supply time from a single source.
                if (supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied[i].GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply == 0)
                {
                    bool supplyStatusSwapped = false;
                    for (int j = 0; (!supplyStatusSwapped && (j < supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied.Count)); j++)
                    {
                        if ((j != i) &&
                                    (supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied[j].GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply > 2))
                        {
                            // Swap the supply status to the 'j' unit since it has been out of supply for at least one turn
                            supplyStatusSwapped = true;
                            supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied[j].GetComponent<UnitDatabaseFields>().inSupply = true;
                            supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied[j].GetComponent<UnitDatabaseFields>().remainingMovement =
                                    supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied[j].GetComponent<UnitDatabaseFields>().movementFactor;
                            supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied[i].GetComponent<UnitDatabaseFields>().inSupply = false;
                            supplySource.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied[i].GetComponent<UnitDatabaseFields>().remainingMovement = 1;
                        }
                    }
                }
            }
        }

        if (!userIntervention)
            checkIfAlliedUnsuppliedUnitsShouldBeEliminated(endOfTurn);

        //GlobalDefinitions.writeToLogFile("setAlliedSupplyStatus: Allied Supply Sources - ");
        //foreach (GameObject hex in GlobalDefinitions.supplySources)
        //{
        //    GlobalDefinitions.writeToLogFile("setAlliedSupplyStatus:    " + hex.name + " supply capacity = " + hex.GetComponent<HexDatabaseFields>().supplyCapacity + " unassigned supply = " + hex.GetComponent<HexDatabaseFields>().unassignedSupply);
        //}

        //GlobalDefinitions.writeToLogFile("setAlliedSupplyStatus: Allied Unit Supply Status");
        //foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
        //{
        //    GlobalDefinitions.writeToLogFile("setAlliedSupplyStatus:    " + unit.name + " supply status = " + unit.GetComponent<UnitDatabaseFields>().inSupply + " increments out of supply = " + unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply);
        //}

        // Now reset all the remainingMovement values
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;

        return (userIntervention);
    }

    /// <summary>
    /// This routine goes through the allied units and detemines if they need to be eliminated
    /// </summary>
    /// <param name="endOfTurn"></param> units are only deleted at the end of a turn
    public void checkIfAlliedUnsuppliedUnitsShouldBeEliminated(bool endOfTurn)
    {
        List<GameObject> unitsToRemove = new List<GameObject>();
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            if (!unit.GetComponent<UnitDatabaseFields>().inSupply)
            {
                unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply++;
                GlobalDefinitions.writeToLogFile("Turn " + GlobalDefinitions.turnNumber + " Allied unit " + unit.name + " is out of supply for " + unit.gameObject.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply + " checks");
                unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = false;
                if (unit.GetComponent<UnitDatabaseFields>().remainingMovement > 0)
                    unit.GetComponent<UnitDatabaseFields>().remainingMovement = 1;

                // If a unit is out of supply for six straight checks it is eliminated
                if (endOfTurn && (unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply > 5))
                {
                    // Can't remove the unit in a foreach loop so store for later
                    GlobalDefinitions.writeToLogFile("      unit being eliminated");
                    unitsToRemove.Add(unit);
                }

                unit.GetComponent<SpriteRenderer>().material.color = Color.gray;
            }
            else
                unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;

        for (int index = 0; index < unitsToRemove.Count; index++)
        {
            GlobalDefinitions.guiUpdateStatusMessage("Unit " + unitsToRemove[index].name + " has been eliminated due to supply");
            // Reset the flags in case this unit is used later as a replacement
            unitsToRemove[index].GetComponent<UnitDatabaseFields>().inSupply = true;
            if (unitsToRemove[index].GetComponent<UnitDatabaseFields>().armor || unitsToRemove[index].GetComponent<UnitDatabaseFields>().airborne)
                unitsToRemove[index].GetComponent<UnitDatabaseFields>().availableForStrategicMovement = true;
            unitsToRemove[index].gameObject.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;
            GlobalDefinitions.moveUnitToDeadPile(unitsToRemove[index]);
        }
    }

    /// <summary>
    /// This routine will go through all of the German units on the board and set their supply status
    /// </summary>
    /// <param name="endOfTurn"></param>  units are only deleted at the end of a turn but supply check happens in the beginning and the end
    public void setGermanSupplyStatus(bool endOfTurn)
    {
        List<GameObject> unitsToRemove = new List<GameObject>();

        // Need to remove all the supply sources on hexes from the last check
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            hex.GetComponent<HexDatabaseFields>().supplySources.Clear();

        // There are 23 supply sources for the Germans.  They are the hexes on the eastern board edge north of Switzerland
        // I'm sure there is a better way to do this but I'm going to call the set routine 23 times.
        // The board limits are 46 hexes in the x plane and 33 in the y plane.  To account for lines that have to meander I will set range to 100 - no specific reason for 100
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(1, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(2, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(3, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(4, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(5, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(6, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(7, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(8, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(9, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(10, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(11, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(12, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(13, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(14, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(15, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(16, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(17, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(18, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(19, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(20, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(21, 32), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(22, 33), 100, GlobalDefinitions.Nationality.German);
        setHexAsSupplySource(GlobalDefinitions.getHexAtXY(23, 32), 100, GlobalDefinitions.Nationality.German);

        // Go through all the hexes and set the flag for displaying supply status in the gui
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().germanInSupply = false;
            if (hex.GetComponent<HexDatabaseFields>().supplySources.Count > 0)
                hex.GetComponent<HexDatabaseFields>().germanInSupply = true;
        }

        // At this point, each hex on the board that has a source of German supply should be set
        foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
        {
            // Germans are easy, if the hex they are on has a supply source  or if they are in a fortress then they are in supply
            if ((unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources.Count > 0) ||
                    unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
            {
                unit.GetComponent<UnitDatabaseFields>().inSupply = true;
                // Remaining movement needs to be set here in preparation for movement
                unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor;
                unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;
                if (unit.GetComponent<UnitDatabaseFields>().armor || unit.GetComponent<UnitDatabaseFields>().airborne)
                    unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = true;
                GlobalDefinitions.unhighlightUnit(unit.gameObject.gameObject); ;
            }
            else
            {
                unit.GetComponent<UnitDatabaseFields>().inSupply = false;
                unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply++;
                GlobalDefinitions.writeToLogFile("Turn " + GlobalDefinitions.turnNumber + " German unit" + unit.name + "out of supply for " + unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply + " checks");
                unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = false;
                // Remaining movement needs to be set here in preparation for movement
                if (unit.GetComponent<UnitDatabaseFields>().remainingMovement > 0)
                    unit.GetComponent<UnitDatabaseFields>().remainingMovement = 1;

                // If a unit is out of supply for six straight checks it is eliminated
                if (endOfTurn && (unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply > 5))
                {
                    // Can't remove the unit from Units On Board while in a foreach loop so store for later
                    unitsToRemove.Add(unit);
                }

                unit.GetComponent<SpriteRenderer>().material.color = Color.gray;
            }

        }

        for (int index = 0; index < unitsToRemove.Count; index++)
        {
            GlobalDefinitions.guiUpdateStatusMessage("Unit " + unitsToRemove[index].name + " has been eliminated due to supply");
            // Reset the flags in case this unit is used later as a replacement
            unitsToRemove[index].GetComponent<UnitDatabaseFields>().inSupply = true;
            if (unitsToRemove[index].GetComponent<UnitDatabaseFields>().armor || unitsToRemove[index].GetComponent<UnitDatabaseFields>().airborne)
                unitsToRemove[index].GetComponent<UnitDatabaseFields>().availableForStrategicMovement = true;
            unitsToRemove[index].gameObject.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;
            GlobalDefinitions.moveUnitToDeadPile(unitsToRemove[index]);
        }

        // Now reset all the remainingMovement values
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
    }

    /// <summary>
    /// This routine will take the hex passed to it and add it as a supply source for all hexes within range
    /// </summary>
    /// <param name="supplySourceHex"></param>
    /// <param name="rangeOfSupply"></param>
    public void setHexAsSupplySource(GameObject supplySourceHex, int rangeOfSupply, GlobalDefinitions.Nationality nationality)
    {
        List<GameObject> hexesToCheck = new List<GameObject>();
        supplySourceHex.GetComponent<HexDatabaseFields>().remainingMovement = rangeOfSupply;
        hexesToCheck.Add(supplySourceHex);

        while (hexesToCheck.Count > 0)
        {
            hexesToCheck[0].GetComponent<HexDatabaseFields>().supplySources.Add(supplySourceHex);
            // The hex is a stopping point if it is in enemy ZOC or no more movement remaining
            if (!GlobalDefinitions.hexInEnemyZOC(hexesToCheck[0], nationality) && (hexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement > 0))
            {
                foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                {
                    // Supply can't pass through neutral, sea, or impassible hexes
                    if ((hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null) &&
                        !hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().neutralCountry &&
                        !hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().sea &&
                        !hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().impassible)
                    {
                        // Add the nieghbor if it doesn't already have the supply source listed and it already isn't in the stack
                        if ((!hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().supplySources.Contains(supplySourceHex)) &&
                                !hexesToCheck.Contains(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]))
                        {
                            hexesToCheck.Add(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                            hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().remainingMovement =
                                        hexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement - 1;
                        }
                        // If the hex already has this supply source listed, check to see if this is a shorter path to the hex
                        if (hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().supplySources.Contains(supplySourceHex) &&
                                ((hexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement - 1) > hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().remainingMovement))
                        {
                            hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().remainingMovement = hexesToCheck[0].GetComponent<HexDatabaseFields>().remainingMovement - 1;
                        }
                    }
                }
            }
            hexesToCheck.RemoveAt(0);
        }
    }


    /// <summary>
    /// Return the Allied unit with the least available supply
    /// </summary>
    /// <returns></returns>
    private GameObject returnLeastSuppliedAlliedUnit()
    {
        int leastAvailableSupply = 1000;
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            if ((totalSupplyAvailable(unit) > 0)
                    && (totalSupplyAvailable(unit) < leastAvailableSupply))
                leastAvailableSupply = totalSupplyAvailable(unit);

        if (leastAvailableSupply == 1000)
            // Everthing is out of supply
            return (null);
        else
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                if ((unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                        && (totalSupplyAvailable(unit) == leastAvailableSupply))
                    return (unit);
        // Should never get here
        return (null);
    }

    /// <summary>
    /// Returns the total supply avaiable to the unit passed
    /// </summary>
    /// <param name="unit"></param>
    private int totalSupplyAvailable(GameObject unit)
    {
        int total = 0;

        foreach (GameObject supplySource in unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources)
            total += supplySource.GetComponent<HexDatabaseFields>().unassignedSupply;
        return (total);
    }

    /// <summary>
    /// Assigns supply to the unit passed using the source with the most unassigned supply
    /// </summary>
    /// <param name="unit"></param>
    private void assignAlliedSupply(GameObject unit)
    {
        int mostUnassignedSuppply = 0;
        bool assignmentMade = false;

        // Loop through the sources and see what the most unassigned supply value is
        foreach (GameObject supplySource in unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources)
            if (supplySource.GetComponent<HexDatabaseFields>().unassignedSupply > mostUnassignedSuppply)
                mostUnassignedSuppply = supplySource.GetComponent<HexDatabaseFields>().unassignedSupply;

        if (mostUnassignedSuppply > 0)
        {
            // Now go back and assign supply from the source with the most unassigned
            foreach (GameObject supplySource in unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources)
                if (!assignmentMade && (supplySource.GetComponent<HexDatabaseFields>().unassignedSupply == mostUnassignedSuppply))
                {
                    assignmentMade = true;
                    supplySource.GetComponent<HexDatabaseFields>().unassignedSupply--;
                    unit.GetComponent<UnitDatabaseFields>().inSupply = true;
                    unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;
                    // Remaining movement needs to be set here in preparation for movement
                    unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor;
                    unit.GetComponent<UnitDatabaseFields>().supplySource = supplySource;
                    GlobalDefinitions.unhighlightUnit(unit);
                }
        }
        //else
        //    GlobalDefinitions.writeToLogFile("assignAlliedSupply: mostUnassignedSupply = 0 all supply has been allocated");

    }

    public void highlightUnsuppliedUnits()
    {
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            GlobalDefinitions.unhighlightUnit(unit);
    }

    /// <summary>
    /// This routine will take the supply source passed and highlight all units that are being supplied by the
    /// source in yellow, use gray to denote units that could be supplied by the source but are out of supply, and
    /// leave units that could be supplied but are being supplied by a different source with white.  Note, all other units will
    /// be highlighted in red
    /// </summary>
    /// <param name="supplySource"></param>
    public void highlightUnitsAvailableForSupply(GameObject supplySource)
    {
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
        {
            // If a unit is being supplied by the supply source passed it will highlight yellow
            // if it is out of supply it will be gray
            // if it is out of range of the supply hex it will be red
            if ((unit.GetComponent<UnitDatabaseFields>().inSupply) && (unit.GetComponent<UnitDatabaseFields>().supplySource == supplySource))
            {
                GlobalDefinitions.writeToLogFile("highlightUnitsAvailableForSupply: unit " + unit.name + " being supplied by current source - highlight yellow");
                //GlobalDefinitions.highlightUnit(unit);
                unit.GetComponent<SpriteRenderer>().material.color = Color.yellow;
            }
            else if (!unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources.Contains(supplySource))
            {
                GlobalDefinitions.writeToLogFile("highlightUnitsAvailableForSupply: unit " + unit.name + " could be supplied by current source - highlight red");
                unit.GetComponent<SpriteRenderer>().material.color = Color.red;
            }
            else if (!unit.GetComponent<UnitDatabaseFields>().inSupply)
            {
                GlobalDefinitions.writeToLogFile("highlightUnitsAvailableForSupply: unit " + unit.name + " out of supply - highlight gray");
                unit.GetComponent<SpriteRenderer>().material.color = Color.gray;
            }
        }
    }


    /// <summary>
    /// Serves as an entery point for the gui to call createSupplySourceGUI since the gui can't pass parameters
    /// </summary>
    public void displaySupplySourceGUI()
    {
        // Turn off the button
        GameObject.Find("SupplySourcesButton").GetComponent<Button>().interactable = false;

        createSupplySourceGUI(true);
    }

    /// <summary>
    /// This routine will create a GUI that displays all the current supply sources
    /// </summary>
    public void createSupplySourceGUI(bool displayOnly)
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.DISPLAYALLIEDSUPPLYKEYWORD + " " + displayOnly);

        GameObject unassignedTextGameObject;

        // Only create the gui if there isn't already one active
        if (GlobalDefinitions.guiList.Count > 0)
        {
            GlobalDefinitions.guiUpdateStatusMessage("Resolve currently displayed menu before invoking another - gui list count = " + GlobalDefinitions.guiList.Count + " name[0] = " + GlobalDefinitions.guiList[0].name);
            return;
        }

        if (GlobalDefinitions.supplySources.Count == 0)
        {
            // When starting off with a saved game the supply status isn't set do regenerate in case we're in this situation.
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().determineAvailableReinforcementPorts();
            GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().setAlliedSupplyStatus(true);

            if (GlobalDefinitions.supplySources.Count == 0) {
                GlobalDefinitions.guiUpdateStatusMessage("No Allied supply sources have been assigned");

                // Turn the button back on
                GameObject.Find("SupplySourcesButton").GetComponent<Button>().interactable = true;

                return;
            }
        }

        // Clear out the global variables related to the supply gui
        float yPosition = 0;
        Button okButton;
        GlobalDefinitions.unassignedTextObejcts.Clear();
        GlobalDefinitions.supplyGUI.Clear();

        float panelWidth = 11 * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight;
        if (GlobalDefinitions.supplySources.Count == 0)
            panelHeight = (GlobalDefinitions.supplySources.Count * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE) + 4 * GlobalDefinitions.GUIUNITIMAGESIZE;
        else
            panelHeight = (GlobalDefinitions.supplySources.Count * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE) + 2 * GlobalDefinitions.GUIUNITIMAGESIZE;

        Canvas supplyCanvas = new Canvas();

        // In case a scrolling window is needed for the supply sources need to create a content panel
        GameObject supplyContentPanel = new GameObject("SupplyContentPanel");
        Image panelImage = supplyContentPanel.AddComponent<Image>();

        panelImage.color = new Color32(0, 44, 255, 220);
        panelImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panelImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panelImage.rectTransform.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelImage.rectTransform.anchoredPosition = new Vector2(0, 0);

        if (panelHeight > (UnityEngine.Screen.height - 50))
            GlobalDefinitions.supplySourceGUIInstance =
                    GlobalDefinitions.createScrollingGUICanvas("SupplyGUICanvas", panelWidth, panelHeight, ref supplyContentPanel, ref supplyCanvas);
        else
        {
            GlobalDefinitions.supplySourceGUIInstance = GlobalDefinitions.createGUICanvas(name, panelWidth, panelHeight, ref supplyCanvas);
            supplyContentPanel.transform.SetParent(GlobalDefinitions.supplySourceGUIInstance.transform, false);
        }

        yPosition = 0.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight;

        // Need an OK button to get out of the gui
        okButton = GlobalDefinitions.createButton("SupplySourcesOKButton", "OK",
                5.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                yPosition,
                supplyCanvas);

        okButton.gameObject.AddComponent<SupplyButtonRoutines>();
        if (!displayOnly)
            okButton.onClick.AddListener(okButton.GetComponent<SupplyButtonRoutines>().okSupplyWithEndPhase);
        else
            okButton.onClick.AddListener(okButton.GetComponent<SupplyButtonRoutines>().okSupply);
        GlobalDefinitions.combatResolutionOKButton = okButton.gameObject;
        GlobalDefinitions.combatResolutionOKButton.SetActive(true);
        okButton.transform.SetParent(supplyContentPanel.transform, false);

        for (int index = 0; index < GlobalDefinitions.supplySources.Count; index++)
        {
            GameObject singleSupplyGUI = new GameObject("singleSupplyGUI" + index);
            singleSupplyGUI.AddComponent<SupplyGUIObject>();

            yPosition += 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;

            // This creates a text box with the name of the source
            (GlobalDefinitions.createText(GlobalDefinitions.supplySources[index].GetComponent<HexDatabaseFields>().hexName,
                    "SourceNameText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    supplyCanvas)).transform.SetParent(supplyContentPanel.transform, false);

            if (!displayOnly)
            {
                // In column three a toggle will be displayed to select the supply source
                singleSupplyGUI.GetComponent<SupplyGUIObject>().supplyToggle = GlobalDefinitions.createToggle("SupplySourceSelectToggle" + index,
                        GlobalDefinitions.GUIUNITIMAGESIZE * 3 * 1.25f - 0.5f * panelWidth,
                        yPosition,
                        supplyCanvas).GetComponent<Toggle>();
                singleSupplyGUI.GetComponent<SupplyGUIObject>().supplyToggle.transform.SetParent(supplyContentPanel.transform, false);
                singleSupplyGUI.GetComponent<SupplyGUIObject>().supplyToggle.gameObject.AddComponent<SupplyButtonRoutines>();
                singleSupplyGUI.GetComponent<SupplyGUIObject>().supplyToggle.GetComponent<SupplyButtonRoutines>().supplySource = GlobalDefinitions.supplySources[index];
                singleSupplyGUI.GetComponent<SupplyGUIObject>().supplySource = GlobalDefinitions.supplySources[index];
                // A separate Toggle object is needed otherwise the Listener won't work without it
                Toggle tempToggle;
                tempToggle = singleSupplyGUI.GetComponent<SupplyGUIObject>().supplyToggle.GetComponent<Toggle>();
                tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<SupplyButtonRoutines>().checkToggle());
            }

            // In column four the range of the source will be displayed
            (GlobalDefinitions.createText((GlobalDefinitions.supplySources[index].GetComponent<HexDatabaseFields>().supplyRange).ToString(),
                    "supplySourceRangeText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    supplyCanvas)).transform.SetParent(supplyContentPanel.transform, false);

            // In column six the total supply capacity will be listed
            (GlobalDefinitions.createText((GlobalDefinitions.supplySources[index].GetComponent<HexDatabaseFields>().supplyCapacity).ToString(),
                    "SupplySourceCapacityText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 5 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    supplyCanvas)).transform.SetParent(supplyContentPanel.transform, false);

            // In column eight the unassigned capacity will be listed
            unassignedTextGameObject = GlobalDefinitions.createText((GlobalDefinitions.supplySources[index].GetComponent<HexDatabaseFields>().unassignedSupply).ToString(),
                    "SupplySourceUnassignedText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 6.5f * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    supplyCanvas);
            unassignedTextGameObject.transform.SetParent(supplyContentPanel.transform, false);
            GlobalDefinitions.unassignedTextObejcts.Add(unassignedTextGameObject);
            GlobalDefinitions.writeToLogFile("createSupplySourceGUI: adding unassignedTextGameObject count = " + GlobalDefinitions.unassignedTextObejcts.Count);

            // In column 10 add a button to locate the supply source
            singleSupplyGUI.GetComponent<SupplyGUIObject>().locateButton = GlobalDefinitions.createButton("CombatResolutionLocateButton", "Locate",
                   GlobalDefinitions.GUIUNITIMAGESIZE * 8 * 1.25f - 0.5f * panelWidth,
                   yPosition,
                   supplyCanvas);
            singleSupplyGUI.GetComponent<SupplyGUIObject>().locateButton.transform.SetParent(supplyContentPanel.transform, false);
            singleSupplyGUI.GetComponent<SupplyGUIObject>().locateButton.gameObject.AddComponent<SupplyButtonRoutines>();
            singleSupplyGUI.GetComponent<SupplyGUIObject>().locateButton.gameObject.GetComponent<SupplyButtonRoutines>().supplySource = GlobalDefinitions.supplySources[index];
            singleSupplyGUI.GetComponent<SupplyGUIObject>().locateButton.onClick.AddListener(singleSupplyGUI.GetComponent<SupplyGUIObject>().locateButton.GetComponent<SupplyButtonRoutines>().locateSupplySource);

            GlobalDefinitions.supplyGUI.Add(singleSupplyGUI);
        }

        // Put a series of text boxes along the top row to serve as the header
        yPosition += 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
        // The first column contains the names of the supply sources
        (GlobalDefinitions.createText("Supply Source", "SupplySourceNameHeaderText",
                2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 * 1.25f - 0.5f * panelWidth,
                yPosition,
                supplyCanvas)).transform.SetParent(supplyContentPanel.transform, false);

        if (!displayOnly)
        {
            // In column three a toggle for selection will be listed
            (GlobalDefinitions.createText("Select", "SupplySelectionText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 3 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    supplyCanvas)).transform.SetParent(supplyContentPanel.transform, false);
        }

            // In column four the range of the source will be listed
            (GlobalDefinitions.createText("Range", "SupplyRangeText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    supplyCanvas)).transform.SetParent(supplyContentPanel.transform, false);

        // In column five the Total Supply Capacity will be listed
        (GlobalDefinitions.createText("Total  Capacity", "SuppplyCapacityText",
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 5 * 1.25f - 0.5f * panelWidth,
                yPosition,
                supplyCanvas)).transform.SetParent(supplyContentPanel.transform, false);

        // In column six the Unassigned Capacity will be listed
        (GlobalDefinitions.createText("Unassigned Supply", "UnassignedSupplyText",
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 6.5f * 1.25f - 0.5f * panelWidth,
                yPosition,
                supplyCanvas)).transform.SetParent(supplyContentPanel.transform, false);

    }

    /// <summary>
    /// This routine will get the hex from the user input and swap the supply status of the unit on the hex
    /// </summary>
    /// <param name="unit"></param>
    public void changeUnitSupplyStatus(GameObject hex)
    {
        if (GlobalDefinitions.currentSupplySource == null)
            GlobalDefinitions.guiUpdateStatusMessage("No supply source selected");


        if (hex != null)
        {
            // Count how many units on the hex are supplied by the current supply source or are out of supply
            int number = 0;
            for (int index = 0; index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                if (!hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().inSupply ||
                        (hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().supplySource == GlobalDefinitions.currentSupplySource))
                    number++;

            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
            {
                GlobalDefinitions.guiUpdateStatusMessage("Hex selected doesn't have any units");
            }
            else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
            {
                GlobalDefinitions.guiUpdateStatusMessage("Allies cannot supply German units");
            }
            else if (number == 0)
            {
                GlobalDefinitions.guiUpdateStatusMessage("No units on the hex are out of supply or supplied by the current supply source");
            }
            else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 1)
            {
                // Only swap the supply status if the unit is out of supply or supplied by the current supply source
                if (!hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().inSupply)
                    swapSupplyStatus(hex.GetComponent<HexDatabaseFields>().occupyingUnit[0]);
                else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().supplySource == GlobalDefinitions.currentSupplySource)
                    swapSupplyStatus(hex.GetComponent<HexDatabaseFields>().occupyingUnit[0]);
                else
                {
                    GlobalDefinitions.guiUpdateStatusMessage("Single unit on the hex is not being supplied by the selected source");
                }
            }
            else if (number == 1)
            {
                for (int index = 0; index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                {
                    if (!hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().inSupply ||
                            (hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().supplySource == GlobalDefinitions.currentSupplySource))
                    {
                        swapSupplyStatus(hex.GetComponent<HexDatabaseFields>().occupyingUnit[index]);
                    }
                }
            }
            else
            {
                callSupplyMultiUnitDisplay(hex, number);
            }
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Valid hex not selected");
    }

    /// <summary>
    /// This routine switches the supply status of the unit passed
    /// </summary>
    /// <param name="unit"></param>
    public void swapSupplyStatus(GameObject unit)
    {
        if (unit != null)
        {
            if (unit.GetComponent<UnitDatabaseFields>().inSupply)
            {
                // This unit is in supply so switch it to being out of supply
                unit.GetComponent<UnitDatabaseFields>().supplySource.GetComponent<HexDatabaseFields>().unassignedSupply++;
                updateUnassignedText(unit.GetComponent<UnitDatabaseFields>().supplySource);
                unit.GetComponent<UnitDatabaseFields>().supplySource = null;
                unit.GetComponent<UnitDatabaseFields>().inSupply = false;
                // Remaining movement needs to be set here in preparation for movement
                unit.GetComponent<UnitDatabaseFields>().remainingMovement = 1;
                unit.GetComponent<SpriteRenderer>().material.color = Color.gray;
            }
            else
            {
                if (GlobalDefinitions.currentSupplySource != null)
                {
                    // This unit is out of supply so set it into supply as long as there is unassigned capacity from the source
                    if (GlobalDefinitions.currentSupplySource.GetComponent<HexDatabaseFields>().unassignedSupply > 0)
                    {
                        // Now make sure the supply is a source for the hex that the unit occupies
                        if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplySources.Contains(GlobalDefinitions.currentSupplySource))
                        {
                            unit.GetComponent<UnitDatabaseFields>().inSupply = true;
                            // Remaining movement needs to be set here in preparation for movement
                            unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor;
                            unit.GetComponent<UnitDatabaseFields>().supplySource = GlobalDefinitions.currentSupplySource;
                            GlobalDefinitions.currentSupplySource.GetComponent<HexDatabaseFields>().unassignedSupply--;
                            updateUnassignedText(GlobalDefinitions.currentSupplySource);
                            // Set the unit to a yellow highlight to indicate it is in supply
                            unit.GetComponent<SpriteRenderer>().material.color = Color.yellow;
                        }
                        else
                            GlobalDefinitions.guiUpdateStatusMessage("The currently selected supply source is not a supply source for this unit");
                    }
                    else
                        GlobalDefinitions.guiUpdateStatusMessage("The supply source does not have unassigned supply capacity");
                }
                else
                    GlobalDefinitions.guiUpdateStatusMessage("No supply source selected.  Select a supply source on the display before selecting a unit.");
            }
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<SupplyState>().executeSelectUnit;
        }
    }

    /// <summary>
    /// This routine determines which object represents the unassigned text to change due to a change in supply
    /// </summary>
    /// <param name="supplySource"></param>
    public static void updateUnassignedText(GameObject supplySource)
    {
        for (int index = 0; index < GlobalDefinitions.supplySources.Count; index++)
            if (supplySource == GlobalDefinitions.supplySources[index])
            {
                GlobalDefinitions.writeToLogFile("updateUnassignedText: debug for out of range index  index = " + index + "  supplySources.Count = " + GlobalDefinitions.supplySources.Count);
                GlobalDefinitions.unassignedTextObejcts[index].GetComponent<Text>().text = supplySource.GetComponent<HexDatabaseFields>().unassignedSupply.ToString();
            }
    }

    /// <summary>
    /// Invoked when a hex is selected for supply assignment that has more than one unit on it
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="numberUnitsToDisplay"></param>
    public void callSupplyMultiUnitDisplay(GameObject hex, int numberUnitsToDisplay)
    {
        GlobalDefinitions.supplySourceGUIInstance.SetActive(false);
        Canvas supplyCanvas = new Canvas();
        float panelWidth = (numberUnitsToDisplay + 1) * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 4 * GlobalDefinitions.GUIUNITIMAGESIZE;
        GlobalDefinitions.createGUICanvas("MultiUnitSupplyGUIInstance",
                panelWidth,
                panelHeight,
                ref supplyCanvas);
        GlobalDefinitions.createText("Select a unit", "multiUnitSupplyText",
                (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                //0.5f * (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                0.5f * (numberUnitsToDisplay + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                supplyCanvas);

        //float xSeperation = (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE / hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count;
        float xSeperation = (numberUnitsToDisplay + 1) * GlobalDefinitions.GUIUNITIMAGESIZE / numberUnitsToDisplay;
        float xOffset = xSeperation / 2;
        int index = 0;
        foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().occupyingUnit)
            if (!unit.GetComponent<UnitDatabaseFields>().inSupply ||
                    (unit.GetComponent<UnitDatabaseFields>().supplySource == GlobalDefinitions.currentSupplySource))
            {
                // My original intent was to have the units displayed in the gui with the same highlighting that they have on the board
                // I don't think this is possible without a lot of extra assets added so I am moving forward with a text box added to 
                // display the supply status of the units displayed.  The status will be in-supply, out-of-supply.  If a unit is 
                // supplied by a different supply source it will not be displayed here because there is nothing that should be
                // done with that unit here since it would have to be removed from the other supply source before it could be
                // assigned here

                Toggle tempToggle;

                tempToggle = GlobalDefinitions.createUnitTogglePair("multiUnitSupplyUnitToggle" + index,
                        index * xSeperation + xOffset - 0.5f * panelWidth,
                        2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        supplyCanvas,
                        unit);

                tempToggle.gameObject.AddComponent<SupplyButtonRoutines>();
                tempToggle.GetComponent<SupplyButtonRoutines>().unit = unit;
                tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<SupplyButtonRoutines>().selectFromMultiUnits());

                if (!unit.GetComponent<UnitDatabaseFields>().inSupply)
                    GlobalDefinitions.createText("Out of Supply",
                            "OutOfSupplyText",
                            GlobalDefinitions.GUIUNITIMAGESIZE,
                            GlobalDefinitions.GUIUNITIMAGESIZE,
                            index * xSeperation + xOffset - 0.5f * panelWidth,
                            0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                            supplyCanvas);
                else
                    GlobalDefinitions.createText("In Supply",
                            "InSupplyText",
                            GlobalDefinitions.GUIUNITIMAGESIZE,
                            GlobalDefinitions.GUIUNITIMAGESIZE,
                            index * xSeperation + xOffset - 0.5f * panelWidth,
                            0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                            supplyCanvas);
                index++;
            }
    }

    /// <summary>
    /// This routine will return true if the hex passed has available supply capacity
    /// </summary>
    /// <param name="selectedHex"></param>
    /// <param name="selectedUnit"></param>
    /// <returns></returns>
    public bool checkForAvailableSupplyCapacity(GameObject selectedHex)
    {
        // This routine will check all the supply sources listed on the hex, not just the hex itself.  It will assign the unit to the source found
        for (int index = 0; index < selectedHex.GetComponent<HexDatabaseFields>().supplySources.Count; index++)
            if (selectedHex.GetComponent<HexDatabaseFields>().supplySources[index].GetComponent<HexDatabaseFields>().unassignedSupply > 0)
                return (true);

        return (false);
    }

    /// <summary>
    /// This routine is called when landing a reinforcement, it assigns unassigned supply capacity to the unit
    /// </summary>
    /// <param name="selectedHex"></param>
    /// <param name="selectedUnit"></param>
    public bool assignAvailableSupplyCapacity(GameObject selectedHex, GameObject selectedUnit)
    {
        // This routine will check all the supply sources listed on the hex, not just the hex itself.  It will assign the unit to the source found
        for (int index = 0; index < selectedHex.GetComponent<HexDatabaseFields>().supplySources.Count; index++)
            if (selectedHex.GetComponent<HexDatabaseFields>().supplySources[index].GetComponent<HexDatabaseFields>().unassignedSupply > 0)
            {
                selectedUnit.GetComponent<UnitDatabaseFields>().supplySource = selectedHex.GetComponent<HexDatabaseFields>().supplySources[index];
                selectedHex.GetComponent<HexDatabaseFields>().supplySources[index].GetComponent<HexDatabaseFields>().unassignedSupply--;
                return (true);
            }

        return (false);
    }

    /// <summary>
    /// Returns a list of all supply sources that are available to Allied units.  For hexes that aren't invasion sites the hex must have
    /// an Allied hq unit on it in order to be able to provide supply.
    /// </summary>
    /// <returns></returns>
    static public List<GameObject> returnAllAlliedSupplySources()
    {
        List<GameObject> returnList = new List<GameObject>();

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (hex.GetComponent<HexDatabaseFields>().successfullyInvaded)
            {
                //GlobalDefinitions.writeToLogFile("returnAllAlliedSupplySources: adding supply source " + hex.name + " with capacity = " + hex.GetComponent<HexDatabaseFields>().supplyCapacity);
                returnList.Add(hex.gameObject);
            }

            else if ((hex.GetComponent<HexDatabaseFields>().supplyCapacity > 0)
                    && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                    && (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
            {
                bool hqPresent = false;
                foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().occupyingUnit)
                    if (unit.GetComponent<UnitDatabaseFields>().HQ)
                        hqPresent = true;
                if (hqPresent)
                {
                    //GlobalDefinitions.writeToLogFile("returnAllAlliedSupplySources: adding supply source " + hex.name + " with capacity = " + hex.GetComponent<HexDatabaseFields>().supplyCapacity);
                    returnList.Add(hex.gameObject);
                }
            }

        return (returnList);
    }

    /// <summary>
    /// Returns the total supply available for Allied units
    /// </summary>
    /// <returns></returns>
    static public int returnAlliedSupplyCapacity()
    {
        int supplyCapacity = 0;
        foreach (GameObject hex in GlobalDefinitions.supplySources)
            supplyCapacity += hex.GetComponent<HexDatabaseFields>().supplyCapacity;
        return (supplyCapacity);
    }

    /// <summary>
    /// Returns a positive number for the additional units that can be supplied.  Negative number if not enough supply.
    /// </summary>
    /// <returns></returns>
    static public int returnAlliedExcessSupply()
    {
        //GlobalDefinitions.writeToLogFile("returnAlliedExcessSupply: allied units on board = " + GlobalDefinitions.alliedUnitsOnBoard.Count);
        return (returnAlliedSupplyCapacity() - GlobalDefinitions.alliedUnitsOnBoard.Count);
    }
}
