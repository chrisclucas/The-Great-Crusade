
using System;
using UnityEngine;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class SetupRoutines : MonoBehaviour
    {
        /// <summary>
        /// This routine selects the unit to be setup
        /// </summary>
        public GameObject GetUnitToSetup(GlobalDefinitions.Nationality nationality, GameObject selectedUnit)
        {
            if ((selectedUnit != null) && (selectedUnit.GetComponent<UnitDatabaseFields>().nationality == nationality))
            {
                // Change the color of the unit to yellow
                GlobalDefinitions.HighlightUnit(selectedUnit);
            }
            else
                selectedUnit = null;

            return (selectedUnit);
        }

        /// <summary>
        /// This routine takes the selectedUnit and places it at the destination hex selected
        /// </summary>
        public void GetUnitSetupDestination(GameObject selectedUnit, GameObject targetHex)
        {
            if ((targetHex != null) && (VerifyValidHex(selectedUnit, targetHex)))
            {
                // If the unit already occupies a hex (not gauranteed since this is setup) need
                // to remove it from the hex so it isn't counting against stacking when it isn't there
                if (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
                    GeneralHexRoutines.RemoveUnitFromHex(selectedUnit, selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex);

                GeneralHexRoutines.PutUnitOnHex(selectedUnit, targetHex);
            }
            GlobalDefinitions.UnhighlightUnit(selectedUnit);

            if (targetHex != null)
            {
                // Move the unit to be on the board
                selectedUnit.transform.parent = GlobalDefinitions.allUnitsOnBoard.transform;
                selectedUnit.GetComponent<UnitDatabaseFields>().inBritain = false;
                selectedUnit = null;
            }
        }

        /// <summary>
        /// This routine takes the selectedUnit and places it on the hex at the location passed
        /// </summary>
        /// <param name="selectedUnit"></param>
        /// <param name="xCoord"></param>
        /// <param name="yCoord"></param>
        public void GetUnitSetupDestination(GameObject selectedUnit, int xCoord, int yCoord)
        {
            GameObject targetHex;

            targetHex = GeneralHexRoutines.GetHexAtXY(xCoord, yCoord);
            if ((targetHex != null) && (VerifyValidHex(selectedUnit, targetHex)))
            {
                // If the unit already occupies a hex (not gauranteed since this is setup) need
                // to remove it from the hex so it isn't counting against stacking when it isn't there
                if (selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
                    GeneralHexRoutines.RemoveUnitFromHex(selectedUnit, selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex);

                GeneralHexRoutines.PutUnitOnHex(selectedUnit, targetHex);
            }
            GlobalDefinitions.UnhighlightUnit(selectedUnit);
            selectedUnit.transform.parent = GlobalDefinitions.allUnitsOnBoard.transform;
            selectedUnit.GetComponent<UnitDatabaseFields>().inBritain = false;
            selectedUnit = null;
        }

        /// <summary>
        /// This routine determines if the hex passed is valid it checks stacking limits and hex type
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="hex"></param>
        /// <returns></returns>
        bool VerifyValidHex(GameObject unit, GameObject hex)
        {
            // Make sure the unit isn't stacking on a hex with an enemy unit
            if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                   (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality !=
                   unit.GetComponent<UnitDatabaseFields>().nationality))
                return (false);

            // Check for hex types that can't be used: impassible, invasion, neutral country
            if ((hex.GetComponent<HexDatabaseFields>().impassible) || (hex.GetComponent<HexDatabaseFields>().neutralCountry))
                return (false);

            // All the non-valid conditions have been checked so if we get here the hex is ok
            return (true);
        }

        /// <summary>
        /// This routine goes through a series of checks to see if the units are setup properly.
        /// </summary>
        /// <returns></returns>
        public static bool UpdateHexFields()
        {
            bool returnState = true;

            // Do an initial load of the GermanUnitsOnBoardList
            GlobalDefinitions.germanUnitsOnBoard.Clear(); // Clear it out first in case multiple passes are made.
            foreach (Transform unit in GameObject.Find("Units On Board").transform)
                if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
                    GlobalDefinitions.germanUnitsOnBoard.Add(unit.gameObject);

            GlobalDefinitions.WriteToLogFile("updateHexFields: executing ... number of German units on board = " + GlobalDefinitions.germanUnitsOnBoard.Count);

            foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
            {
                // Unhighlight the unit so that if all units pass they will be unhighlighted
                GlobalDefinitions.UnhighlightUnit(unit);

                // Not sure if this is needed.  Can't really be in "Units On Board" without a hex assignment
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex == null)
                {
                    GlobalDefinitions.GuiUpdateStatusMessage("Internal Error - Unit " + unit.GetComponent<UnitDatabaseFields>().name + " is not assigned a hex locunit.GetComponent<UnitDatabaseFields>().ation");
                    GlobalDefinitions.HighlightUnit(unit);
                    returnState = false;
                }

                // German static units must be on a coast or inland port hex
                else if (unit.GetComponent<UnitDatabaseFields>().germanStatic &&
                        !unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().coast &&
                        !unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().inlandPort &&
                        !unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().coastalPort)
                {
                    GlobalDefinitions.GuiUpdateStatusMessage(unit.GetComponent<UnitDatabaseFields>().unitDesignation + " is a static unit and must start on a coast hex, a port, or an inland port");
                    GlobalDefinitions.HighlightUnit(unit);
                    returnState = false;
                }

                else if (((unit.name == "Armor-German-3SS") ||
                        (unit.name == "Armor-German-9SS") ||
                        (unit.name == "Armor-German-25SS") ||
                        (unit.name == "Armor-German-49SS") ||
                        (unit.name == "Armor-German-51SS") ||
                        (unit.name == "Armor-German-106") ||
                        (unit.name == "Armor-German-15SS")) &&
                        (!unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().germanRepalcement))
                {
                    GlobalDefinitions.GuiUpdateStatusMessage(unit.GetComponent<UnitDatabaseFields>().unitDesignation + " must start on a replacement hex (hexes in Germany with a star on them");
                    GlobalDefinitions.HighlightUnit(unit);
                    returnState = false;
                }

                // Setup the ZOC for the hex and its neighbors.  Note that I did not bother to do this while the user was moving
                // units around since I think it makes sense to just do it once when he is done.
                unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().inGermanZOC = true;
                foreach (HexDefinitions.HexSides hexSides in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                {
                    if ((unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] != null)
                            && (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<BooleanArrayData>().exertsZOC[(int)hexSides]))
                        unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().inGermanZOC = true;
                }
            }
            if (!returnState)
                GlobalDefinitions.GuiUpdateStatusMessage("Cannot exit setup until all issues are resolved");
            return (returnState);
        }
    }
}