using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class InvasionRoutines : MonoBehaviour
{
    /// <summary>
    /// This routine resets all the counters associated with the units used in the invasion area for the turn
    /// </summary>
    public void initializeAreaCounters()
    {
        foreach (InvasionArea targetArea in GlobalDefinitions.invasionAreas)
        {
            targetArea.airborneUnitsUsedThisTurn = 0;
            targetArea.armorUnitsUsedThisTurn = 0;
            targetArea.infantryUnitsUsedThisTurn = 0;
            targetArea.totalUnitsUsedThisTurn = 0;
        }

        GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn = 0;

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            GlobalDefinitions.unhighlightHex(hex.gameObject);
        }
    }

    /// <summary>
    /// Routine pulls up a list of the invasion areas for the user to select from.
    /// </summary>
    /// <returns></returns>
    public void selectInvasionArea()
    {
        GlobalDefinitions.writeToLogFile("selectInvasionArea: executing");
        Canvas invasionAreaSelectionCanvasInstance = new Canvas();

        float panelWidth = 3 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 9 * GlobalDefinitions.GUIUNITIMAGESIZE;
        GlobalDefinitions.invasionAreaSelectionGUIInstance = GlobalDefinitions.createGUICanvas("InvasionAreaSelectionGUIInstance",
                panelWidth,
                panelHeight,
                ref invasionAreaSelectionCanvasInstance);
        GlobalDefinitions.createText("Select invasion area", "InvasionAreaSelectionText",
                (3) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE,
                1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                (8) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                invasionAreaSelectionCanvasInstance);
        for (int index = 0; index < 7; index++)
        {
            Toggle tempToggle;
            GlobalDefinitions.createText(GlobalDefinitions.invasionAreas[index].name,
                    "InvasionAreaSelectionText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1.25f - 0.5f * panelWidth,
                    (index + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    invasionAreaSelectionCanvasInstance);
            tempToggle = GlobalDefinitions.createToggle("InvasionAreaSelectionToggle" + index,
                        2 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        (index + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        invasionAreaSelectionCanvasInstance).GetComponent<Toggle>();
            tempToggle.gameObject.AddComponent<InvasionSelectionToggleRoutines>();
            tempToggle.GetComponent<InvasionSelectionToggleRoutines>().index = index;
            tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<InvasionSelectionToggleRoutines>().invadedAreaSelected());
        }
    }

    /// <summary>
    /// Sets the global variables related to an invasion site
    /// </summary>
    /// <param name="invadedAreaSectionIndex"></param>
    public void setInvasionArea(int invadedAreaSectionIndex)
    {
        GlobalDefinitions.writeToLogFile("setInvasionArea: executing with index " + invadedAreaSectionIndex);
        GlobalDefinitions.numberInvasionsExecuted++;
        if (GlobalDefinitions.turnNumber == 1)
        {
            GlobalDefinitions.firstInvasionAreaIndex = invadedAreaSectionIndex;
            GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].invaded = true;
            GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].turn = 1;
        }
        else
        {
            GlobalDefinitions.secondInvasionAreaIndex = invadedAreaSectionIndex;
            GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].invaded = true;
            GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].turn = 1;
        }
    }

    /// <summary>
    /// This routine gets an invading unit from Britain
    /// </summary>
    /// <returns></returns>
    public GameObject getInvadingUnit(GameObject selectedUnit)
    {

        //  Check for valid unit
        if (selectedUnit == null)
        {
            GlobalDefinitions.guiUpdateStatusMessage("No unit selected");
        }

        // Check if the unit is on a sea hex, this would make it a unit that has already been deployed for an invasion
        // The user may be picking it in order to undo the selection.
        if ((selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null) && 
                (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea))
        {
            // Hghlight the unit
            GlobalDefinitions.highlightUnit(selectedUnit);
            return (selectedUnit);
        }

        // If the unit selected isn't in Britain than display the units in the gui
        else if (!selectedUnit.GetComponent<UnitDatabaseFields>().inBritain)
        {
            if (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
                GlobalDefinitions.guiDisplayUnitsOnHex(selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex);
        }
        else if (selectedUnit.GetComponent<UnitDatabaseFields>().HQ)
        {
            GlobalDefinitions.guiUpdateStatusMessage("HQ units are not allowed to invade");
        }
        else if (selectedUnit.GetComponent<UnitDatabaseFields>().turnAvailable > GlobalDefinitions.turnNumber)
        {
            GlobalDefinitions.guiUpdateStatusMessage("Unit selected is not available until turn " + selectedUnit.GetComponent<UnitDatabaseFields>().turnAvailable);
        }
        else
        {
            // Check if the unit is available for the first invasion area

            if (GlobalDefinitions.turnNumber == 1)
            {
                //GlobalDefinitions.writeToLogFile("getInvadingUnit: total units used this turn = " + GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].totalUnitsUsedThisTurn);
                //GlobalDefinitions.writeToLogFile("getInvadingUnit: max total unit for invasion area this turn = " + returnMaxTotalUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]));

                if (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].totalUnitsUsedThisTurn <
                        returnMaxTotalUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]))
                {
                    if (selectedUnit.GetComponent<UnitDatabaseFields>().armor)
                    {
                        if (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].armorUnitsUsedThisTurn <
                                returnMaxArmorUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]))
                        {
                            // Valid unit so highlight the hexes available in the invasion area
                            GlobalDefinitions.highlightUnit(selectedUnit);
                            highlightAvailableInvasionHexes(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]);
                            return (selectedUnit);
                        }
                    }
                    else if (selectedUnit.GetComponent<UnitDatabaseFields>().infantry || selectedUnit.GetComponent<UnitDatabaseFields>().airborne)
                    {
                        //GlobalDefinitions.writeToLogFile("getInvadingUnit: infantry units used this turn = " + GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].infantryUnitsUsedThisTurn);
                        //GlobalDefinitions.writeToLogFile("getInvadingUnit: max infantry units this turn = " + returnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]));
                        // Need to check for using infantry against the armor limit
                        if ((GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].infantryUnitsUsedThisTurn <
                                returnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex])) ||
                                (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].armorUnitsUsedThisTurn <
                                returnMaxArmorUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex])))
                        {
                            // Valid unit so highlight the hexes available in the invasion area
                            GlobalDefinitions.highlightUnit(selectedUnit);
                            highlightAvailableInvasionHexes(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]);
                            return (selectedUnit);
                        }
                    }
                    else
                    {
                        // Don't know why we would ever get here but if we do return a null
                        GlobalDefinitions.guiUpdateStatusMessage("Selected unit is not recognized as armor, infantry, or airborne");
                    }
                }
            }
            else
            {
                // Need to check the second invasion area
                if (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].totalUnitsUsedThisTurn <
                        returnMaxTotalUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex]))
                {
                    if (selectedUnit.GetComponent<UnitDatabaseFields>().armor)
                    {
                        if (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].armorUnitsUsedThisTurn <
                                returnMaxArmorUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex]))
                        {
                            // Valid unit so highlight the hexes available in the invasion area
                            GlobalDefinitions.highlightUnit(selectedUnit);
                            highlightAvailableInvasionHexes(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex]);
                            return (selectedUnit);
                        }
                    }
                    else if (selectedUnit.GetComponent<UnitDatabaseFields>().infantry || selectedUnit.GetComponent<UnitDatabaseFields>().airborne)
                    {
                        // Need to check for using infantry against the armor limit
                        if ((GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].infantryUnitsUsedThisTurn <
                                returnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex])) ||
                                (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].armorUnitsUsedThisTurn <
                                returnMaxArmorUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex])))
                        {
                            // Valid unit so highlight the hexes available in the invasion area
                            GlobalDefinitions.highlightUnit(selectedUnit);
                            highlightAvailableInvasionHexes(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex]);
                            return (selectedUnit);
                        }
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Determines the hex that the selected unit will invade from.
    /// </summary>
    /// <param name="selectedUnit"></param>
    public void getUnitInvasionHex(GameObject selectedUnit, GameObject selectedHex)
    {
        if (selectedHex != null)
        {
            if (selectedHex.GetComponent<HexDatabaseFields>().availableForMovement)
            {
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().landAlliedUnitFromOffBoard(selectedUnit, selectedHex, false);
            }
            else
            {
                GlobalDefinitions.guiDisplayUnitsOnHex(selectedHex);
                GlobalDefinitions.guiUpdateStatusMessage("Hex selected is not available");
            }
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("No hex selected");

        if (GlobalDefinitions.selectedUnit != null)
            GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
        GlobalDefinitions.selectedUnit = null;

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            GlobalDefinitions.unhighlightHex(hex.gameObject);
            hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
        }
    }

    private int returnMaxTotalUnitsForInvasionAreaThisTurn(InvasionArea targetArea)
    {
        if (targetArea.turn == 0)
        {
            GlobalDefinitions.writeToLogFile("returnMaxTotalUnitsForInvasionAreaThisTurn: Something is wrong - invasion area with turn set to 0");
            return (0);
        }
        else if (targetArea.turn == 1)
            return (targetArea.firstTurnArmor + targetArea.firstTurnInfantry);
        else if (targetArea.turn == 2)
            return (targetArea.secondTurnArmor + targetArea.secondTurnInfantry);
        else
            return (targetArea.divisionsPerTurn);
    }

    private int returnMaxArmorUnitsForInvasionAreaThisTurn(InvasionArea targetArea)
    {
        if (targetArea.turn == 0)
        {
            GlobalDefinitions.writeToLogFile("returnMaxArmorUnitsForInvasionAreaThisTurn: Something is wrong - invasion area with turn set to 0");
            return (0);
        }
        else if (targetArea.turn == 1)
            return (targetArea.firstTurnArmor);
        else if (targetArea.turn == 2)
            return (targetArea.secondTurnArmor);
        else
            return (targetArea.divisionsPerTurn);
    }

    private int returnMaxInfantryUnitsForInvasionAreaThisTurn(InvasionArea targetArea)
    {
        if (targetArea.turn == 0)
        {
            GlobalDefinitions.writeToLogFile("returnMaxInfantryUnitsForInvasionAreaThisTurn: Something is wrong - invasion area with turn set to 0");
            return (0);
        }
        else if (targetArea.turn == 1)
            return (targetArea.firstTurnInfantry);
        else if (targetArea.turn == 2)
            return (targetArea.secondTurnInfantry);
        else
            return (targetArea.divisionsPerTurn);
    }

    /// <summary>
    /// Highlights the hexes for the invasion area passed
    /// </summary>
    /// <param name="targetArea"></param>
    private void highlightAvailableInvasionHexes(InvasionArea targetArea)
    {
        foreach (GameObject hex in targetArea.invasionHexes)
        {
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count < 2)
            {
                GlobalDefinitions.highlightHexForMovement(hex);
                hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
            }
        }
    }

    /// <summary>
    /// This routine is used to adjust the unit limits when a unit is landed from Britain
    /// </summary>
    /// <param name="unit"></param>
    public void incrementInvasionUnitLimits(GameObject unit)
    {
        int invasionAreaIndex = unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionAreaIndex;
        if ((GlobalDefinitions.invasionAreas[invasionAreaIndex].invaded) &&
                ((GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 1) || (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 2)))
        {
            if (unit.GetComponent<UnitDatabaseFields>().armor)
            {
                GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn++;
                GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn++;
                GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn++;
            }
            // Note airborne counts against the infantry limit if brought in through the beach
            else if (unit.GetComponent<UnitDatabaseFields>().infantry || unit.GetComponent<UnitDatabaseFields>().airborne)
            {
                // Check if infantry is using the armor limit on this turn
                if (GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn ==
                        returnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[invasionAreaIndex]))
                {
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn++;
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn++;
                    GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn++;
                }
                else
                {
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn++;
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn++;
                    GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn++;
                }
            }
            else
            {
                GlobalDefinitions.writeToLogFile("incrementInvasionLimits: ERROR - unit = " + unit.name + " Most likely due to an HQ being landed during the first two turns.. This should never be executed");
            }
        }
        else
        {
            // Note that a unit being landed in a non-invasion area so use the turn three limits
            GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn++;
            GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn++;
        }
    }

    /// <summary>
    /// This routine is called to add back to limits when a unit returns to Britain by an undo action
    /// </summary>
    /// <param name="unit"></param>
    public void decrementInvasionUnitLimits(GameObject unit)
    {
        int invasionAreaIndex = unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex;

        // Haven't figured out how to set the invasion index on an airborne drop yet.  The invasion index should be set
        // based on the unit that the airborne unit is dropping from... that's a lot of code for this one specific issue.
        // For the time being I'll ignore it.
        if (!unit.GetComponent<UnitDatabaseFields>().airborne)
        {
            if ((GlobalDefinitions.invasionAreas[invasionAreaIndex].invaded) &&
                    ((GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 1) || (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 2)))
            {
                if (unit.GetComponent<UnitDatabaseFields>().armor)
                {
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn--;
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn--;
                    GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn--;
                }
                // Note airborne counts against the infantry limit if brought in through the beach
                else if (unit.GetComponent<UnitDatabaseFields>().infantry || unit.GetComponent<UnitDatabaseFields>().airborne)
                {
                    // Check if infantry is using the armor limit on this turn
                    if (GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn ==
                            returnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[invasionAreaIndex]))
                    {
                        GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn--;
                        GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn--;
                        GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn--;
                    }
                    else
                    {
                        GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn--;
                        GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn--;
                        GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn--;
                    }
                }
                else
                {
                    GlobalDefinitions.writeToLogFile("decrementInvasionUnitLimits: ERROR - Most likely due to an HQ being landed during the first two turns.. This should never be executed");
                }
            }
            else
            {
                // This is a unit being landed in a non-invasion area so use the turn three limits
                GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn--;
                GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn--;
            }
        }
    }

    /// <summary>
    /// Takes all units on sea hexes that don't have any opposition and moves them onto land
    /// </summary>
    public void moveUnopposedSeaUnits()
    {
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
        {
            if (((unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea) &&
                    (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)) ||
                    ((unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea) &&
                    (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                    (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)))
            {
                // Set the flag on the invasion target hex to allow for reinforcement landing to take place
                unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().successfullyInvaded = true;
                unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().alliedControl = true;
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnit(
                        unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionTarget,
                        unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
            }
        }
    }
}
