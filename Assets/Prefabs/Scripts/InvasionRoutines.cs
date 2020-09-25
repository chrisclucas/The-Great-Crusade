﻿using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class InvasionRoutines : MonoBehaviour
{
    /// <summary>
    /// This routine resets all the counters associated with the units used in the invasion area for the turn
    /// </summary>
    public void InitializeAreaCounters()
    {
        foreach (InvasionArea targetArea in GlobalDefinitions.invasionAreas)
        {
            targetArea.airborneUnitsUsedThisTurn = 0;
            targetArea.armorUnitsUsedThisTurn = 0;
            targetArea.infantryUnitsUsedThisTurn = 0;
            targetArea.totalUnitsUsedThisTurn = 0;
            targetArea.infantryUsedAsArmorThisTurn = 0;
            targetArea.airborneUsedAsInfantryThisTurn = 0;
        }

        GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn = 0;

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            GlobalDefinitions.UnhighlightHex(hex.gameObject);
        }
    }

    /// <summary>
    /// Routine pulls up a list of the invasion areas for the user to select from.
    /// </summary>
    /// <returns></returns>
    public void SelectInvasionArea()
    {
        GlobalDefinitions.WriteToLogFile("selectInvasionArea: executing");
        Canvas invasionAreaSelectionCanvasInstance = new Canvas();

        float panelWidth = 3 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 9 * GlobalDefinitions.GUIUNITIMAGESIZE;
        GlobalDefinitions.invasionAreaSelectionGUIInstance = GlobalDefinitions.CreateGUICanvas("InvasionAreaSelectionGUIInstance",
                panelWidth,
                panelHeight,
                ref invasionAreaSelectionCanvasInstance, 0.16f, 0.16f);
        GlobalDefinitions.invasionAreaSelectionGUIInstance.GetComponent<RectTransform>().anchorMin = new Vector2(0.16f, 0.5f);

        GlobalDefinitions.CreateUIText("Select invasion area", "InvasionAreaSelectionText",
                (3) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE,
                1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                (8) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                Color.white,
                invasionAreaSelectionCanvasInstance, 0.16f, 0.16f, 0.5f, 0.5f);
        for (int index = 0; index < 7; index++)
        {
            Toggle tempToggle;
            GlobalDefinitions.CreateUIText(GlobalDefinitions.invasionAreas[index].name,
                    "InvasionAreaSelectionText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 1.25f - 0.5f * panelWidth,
                    (index + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    Color.white,
                    invasionAreaSelectionCanvasInstance, 0.16f, 0.16f, 0.5f, 0.5f);
            tempToggle = GlobalDefinitions.CreateToggle("InvasionAreaSelectionToggle" + index,
                        2 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        (index + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        invasionAreaSelectionCanvasInstance, 0.16f, 0.16f, 0.5f, 0.5f).GetComponent<Toggle>();
            tempToggle.gameObject.AddComponent<InvasionSelectionToggleRoutines>();
            tempToggle.GetComponent<InvasionSelectionToggleRoutines>().index = index;
            tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<InvasionSelectionToggleRoutines>().InvadedAreaSelected());
        }
    }

    /// <summary>
    /// Sets the global variables related to an invasion site
    /// </summary>
    /// <param name="invadedAreaSectionIndex"></param>
    public void SetInvasionArea(int invadedAreaSectionIndex)
    {
        GlobalDefinitions.WriteToLogFile("setInvasionArea: executing with index " + invadedAreaSectionIndex);
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
    public GameObject GetInvadingUnit(GameObject selectedUnit)
    {
        GlobalDefinitions.WriteToLogFile("getInvadingUnit: executing for unit = " + selectedUnit.name);

        //  Check for valid unit
        if (selectedUnit == null)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("No unit selected; select a unit in Britain that is available to invade this turn");
        }

        // Check if the unit is on a sea hex, this would make it a unit that has already been deployed for an invasion
        // The user may be picking it in order to undo the selection.
        else if ((selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null) &&
                (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea))
        {
            // Hghlight the unit
            GlobalDefinitions.HighlightUnit(selectedUnit);
            return (selectedUnit);
        }

        // If the unit selected isn't in Britain than display the units in the gui
        else if (!selectedUnit.GetComponent<UnitDatabaseFields>().inBritain)
        {
            if (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
                GlobalDefinitions.GuiDisplayUnitsOnHex(selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex);
        }
        else if (selectedUnit.GetComponent<UnitDatabaseFields>().HQ)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("HQ units are not allowed to invade");
        }
        else if (selectedUnit.GetComponent<UnitDatabaseFields>().turnAvailable > GlobalDefinitions.turnNumber)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("Unit selected is not available until turn " + selectedUnit.GetComponent<UnitDatabaseFields>().turnAvailable);
        }
        else
        {
            // Check if the unit is available for the first invasion area

            if (GlobalDefinitions.turnNumber == 1)
            {
                if (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].totalUnitsUsedThisTurn <
                        ReturnMaxTotalUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]))
                {
                    if (selectedUnit.GetComponent<UnitDatabaseFields>().armor)
                    {
                        if (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].armorUnitsUsedThisTurn <
                                ReturnMaxArmorUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]))
                        {
                            // Valid unit so highlight the hexes available in the invasion area
                            GlobalDefinitions.HighlightUnit(selectedUnit);
                            HighlightAvailableInvasionHexes(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]);
                            return (selectedUnit);
                        }
                    }
                    else if (selectedUnit.GetComponent<UnitDatabaseFields>().infantry || selectedUnit.GetComponent<UnitDatabaseFields>().airborne)
                    {
                        //GlobalDefinitions.writeToLogFile("getInvadingUnit: infantry units used this turn = " + GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].infantryUnitsUsedThisTurn);
                        //GlobalDefinitions.writeToLogFile("getInvadingUnit: max infantry units this turn = " + returnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]));
                        // Need to check for using infantry against the armor limit
                        if ((GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].infantryUnitsUsedThisTurn <
                                ReturnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex])) ||
                                (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].armorUnitsUsedThisTurn <
                                ReturnMaxArmorUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex])))
                        {
                            // Valid unit so highlight the hexes available in the invasion area
                            GlobalDefinitions.HighlightUnit(selectedUnit);
                            HighlightAvailableInvasionHexes(GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex]);
                            return (selectedUnit);
                        }
                    }
                    else
                    {
                        // Don't know why we would ever get here but if we do return a null
                        GlobalDefinitions.GuiUpdateStatusMessage("Internal Error - Selected unit is not recognized as armor, infantry, or airborne");
                    }
                }
            }
            else
            {
                // Need to check the second invasion area
                if (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].totalUnitsUsedThisTurn <
                        ReturnMaxTotalUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex]))
                {
                    if (selectedUnit.GetComponent<UnitDatabaseFields>().armor)
                    {
                        if (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].armorUnitsUsedThisTurn <
                                ReturnMaxArmorUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex]))
                        {
                            // Valid unit so highlight the hexes available in the invasion area
                            GlobalDefinitions.HighlightUnit(selectedUnit);
                            HighlightAvailableInvasionHexes(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex]);
                            return (selectedUnit);
                        }
                    }
                    else if (selectedUnit.GetComponent<UnitDatabaseFields>().infantry || selectedUnit.GetComponent<UnitDatabaseFields>().airborne)
                    {
                        // Need to check for using infantry against the armor limit
                        if ((GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].infantryUnitsUsedThisTurn <
                                ReturnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex])) ||
                                (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].armorUnitsUsedThisTurn <
                                ReturnMaxArmorUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex])))
                        {
                            // Valid unit so highlight the hexes available in the invasion area
                            GlobalDefinitions.HighlightUnit(selectedUnit);
                            HighlightAvailableInvasionHexes(GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex]);
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
    public void GetUnitInvasionHex(GameObject selectedUnit, GameObject selectedHex)
    {
        if (selectedHex != null)
        {
            if (selectedHex.GetComponent<HexDatabaseFields>().availableForMovement)
            {
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(selectedUnit, selectedHex, false);
            }
            else
            {
                GlobalDefinitions.GuiDisplayUnitsOnHex(selectedHex);
                GlobalDefinitions.GuiUpdateStatusMessage("Hex selected is not available for invasion, must select a highlighted hex");
            }
        }
        else
            GlobalDefinitions.GuiUpdateStatusMessage("No hex selected, must select a highlighted hex");

        if (GlobalDefinitions.selectedUnit != null)
            GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
        GlobalDefinitions.selectedUnit = null;

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            GlobalDefinitions.UnhighlightHex(hex.gameObject);
            hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
        }
    }

    private int ReturnMaxTotalUnitsForInvasionAreaThisTurn(InvasionArea targetArea)
    {
        if (targetArea.turn == 0)
        {
            GlobalDefinitions.WriteToLogFile("returnMaxTotalUnitsForInvasionAreaThisTurn: Something is wrong - invasion area with turn set to 0");
            return (0);
        }
        else if (targetArea.turn == 1)
            return (targetArea.firstTurnArmor + targetArea.firstTurnInfantry);
        else if (targetArea.turn == 2)
            return (targetArea.secondTurnArmor + targetArea.secondTurnInfantry);
        else
            return (targetArea.divisionsPerTurn);
    }

    private int ReturnMaxArmorUnitsForInvasionAreaThisTurn(InvasionArea targetArea)
    {
        if (targetArea.turn == 0)
        {
            GlobalDefinitions.WriteToLogFile("returnMaxArmorUnitsForInvasionAreaThisTurn: Something is wrong - invasion area with turn set to 0");
            return (0);
        }
        else if (targetArea.turn == 1)
            return (targetArea.firstTurnArmor);
        else if (targetArea.turn == 2)
            return (targetArea.secondTurnArmor);
        else
            return (targetArea.divisionsPerTurn);
    }

    private int ReturnMaxInfantryUnitsForInvasionAreaThisTurn(InvasionArea targetArea)
    {
        if (targetArea.turn == 0)
        {
            GlobalDefinitions.WriteToLogFile("returnMaxInfantryUnitsForInvasionAreaThisTurn: Something is wrong - invasion area with turn set to 0");
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
    private void HighlightAvailableInvasionHexes(InvasionArea targetArea)
    {
        foreach (GameObject hex in targetArea.invasionHexes)
        {
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count < 2)
            {
                GlobalDefinitions.HighlightHexForMovement(hex);
                hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
            }
        }
    }

    /// <summary>
    /// This routine is used to adjust the unit limits when a unit is landed from Britain
    /// </summary>
    /// <param name="unit"></param>
    public void IncrementInvasionUnitLimits(GameObject unit)
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
            else if (unit.GetComponent<UnitDatabaseFields>().infantry)
            {
                //  First check if infantry should be used against the armor limits
                if (GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn ==
                            ReturnMaxInfantryUnitsForInvasionAreaThisTurn(GlobalDefinitions.invasionAreas[invasionAreaIndex]))
                {
                    // If so then increment the armor and the decrement the infantry used as armor
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn++;
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUsedAsArmorThisTurn++;
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
            else if (unit.GetComponent<UnitDatabaseFields>().airborne)
            {
                //  Airborne landed over a beach counts against the infantry limits
                GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn++;
                GlobalDefinitions.invasionAreas[invasionAreaIndex].airborneUsedAsInfantryThisTurn++;
                GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn++;
                GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn++;
            }
            else
            {
                GlobalDefinitions.WriteToLogFile("incrementInvasionLimits: ERROR - unit = " + unit.name + " Most likely due to an HQ being landed during the first two turns.. This should never be executed");
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
    public void DecrementInvasionUnitLimits(GameObject unit)
    {
        // When returning a unit back to Britain, it only impacts the limits if it is being returned on the turn that it was landed.
        // Otherwise sending units back will allow the player to replace them with other units and still bring in the full 
        // complement of units available for that turn.  The way that I can determine if the unit is being returned on the same turn
        // that it was landed is whetehr or not it has a beginning hex set.  If it does that means it started the turn on the board.
        if (unit.GetComponent<UnitDatabaseFields>().beginningTurnHex != null)
            // No limits should be adjusted for landing this turn since the unit started the turn on the board
            return;

        // The otehr special case I have to check for is an airborne unit being returned to Britain that did an airborne drop this turn.
        // The way that I can tell this is the case is that the airborne unit will not have an invasion index set to -1
        if (unit.GetComponent<UnitDatabaseFields>().airborne && (unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex == -1))
        {
            GlobalDefinitions.currentAirborneDropsThisTurn--;
            return;
        }

        // Now the only units left are ones that were landed by land this turn

        int invasionAreaIndex = unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex;

        if ((GlobalDefinitions.invasionAreas[invasionAreaIndex].invaded) &&
                ((GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 1) || (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 2)))
        {
            if (unit.GetComponent<UnitDatabaseFields>().armor)
            {
                GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn--;
                GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn--;
                GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn--;
            }
            // Note airborne counts against the infantry limit if brought in through the beach - which is silly because I've weeded out airborne units before this point
            else if (unit.GetComponent<UnitDatabaseFields>().infantry)
            {
                // First check if the infantry used armor limits to land this turn
                if (GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUsedAsArmorThisTurn > 0)
                {
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn--;
                    GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUsedAsArmorThisTurn--;
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
            else if (unit.GetComponent<UnitDatabaseFields>().airborne)
            {
                // We've already taken care of airborne units that were dropped this turn so this unit has to have been landed
                GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn--;
                GlobalDefinitions.invasionAreas[invasionAreaIndex].airborneUsedAsInfantryThisTurn--;
                GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn--;
                GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn--;
            }
            else
            {
                GlobalDefinitions.WriteToLogFile("decrementInvasionUnitLimits: ERROR - Most likely due to an HQ being landed during the first two turns.. This should never be executed");
            }
        }
        else
        {
            // This is a unit being landed in a non-invasion area so use the turn three limits
            GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn--;
            GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn--;
        }
    }

    /// <summary>
    /// Takes all units on sea hexes that don't have any opposition and moves them onto land
    /// </summary>
    public void MoveUnopposedSeaUnits()
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
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(
                        unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionTarget,
                        unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
            }
        }
    }
}
