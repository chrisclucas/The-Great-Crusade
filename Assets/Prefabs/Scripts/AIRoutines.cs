//#define OUTPUTDEBUG

using System.Collections.Generic;
using System;
using UnityEngine;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class AIRoutines : MonoBehaviour
    {
        // This variable holds the average location of the enemy units
        public static Vector2 targetLocation = new Vector2();

        /// <summary>
        /// This routine moves Allied units that are not near enemy units
        /// </summary>
        /// <param name="unitList"></param>
        public static void MakeAlliedStrategicMoves(List<GameObject> unitList)
        {
            if (unitList.Count > 0)
            {
                // Set the target location for reinforcement movement
                targetLocation = GetAverageEnemyLocation(GlobalDefinitions.Nationality.German);
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeAlliedStrategicMoves: target reinforcement location " + targetLocation.x + " " + targetLocation.y);
#endif
            }

            foreach (GameObject unit in unitList)
            {
                // Reinforcement movement is used for units that are far enough away from enemy units that their main criteria is to move as 
                // far as possible in a specific direction.  The distance that needs to be checked for enemy units is dependent on the type 
                // of unit.  All Allied units can move strategically so they should be checked for twice their movement factor plus 4.  The plus
                // 4 is the movement that the enemy can make in the next turn to attack the unit.  This would work out to 12 hexes for most units.
                if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                {
                    // if (findNearbyEnemyUnits(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.GetComponent<UnitDatabaseFields>().nationality, unit.GetComponent<UnitDatabaseFields>().movementFactor * 2 + GlobalDefinitions.attackRange).Count == 0)
                    // Note that I've changed the distance to check for Allied units to be 4.  This makes it more effective in getting 
                    // units moving west.
                    if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack && !unit.GetComponent<UnitDatabaseFields>().hasMoved &&
                            (FindNearbyEnemyUnits(unit.GetComponent<UnitDatabaseFields>().occupiedHex, GlobalDefinitions.Nationality.Allied, 4).Count == 0))
                        ExecuteAlliedStrategicMovement(unit);
                }
            }
        }

        /// <summary>
        /// Executes reinforcement movement for the German unit passed
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="targetLocation"></param>
        public static void ExecuteGermanReinforcementMovement(GameObject unit, GameObject targetHex)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("executeGermanReinforcementMovement: moving unit - " + unit.name + " target location = " + targetHex.name);
#endif
            float closestDistance = float.MaxValue;
            float furthestDistance = 0f;
            GameObject moveHex = null;

            // We have the target location to move to and the list of available hexes so we can exeute movement now
            // Go through each of the available hexes and determine which is closest to the target location
            GeneralHexRoutines.ResetMovementAvailableFields();
            unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Clear();
            unit.GetComponent<UnitDatabaseFields>().availableMovementHexes
                    = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
            foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("executeGermanReinforcementMovement:     available hex - " + hex.name);
#endif
                if (!hex.GetComponent<HexDatabaseFields>().sea &&
                        !hex.GetComponent<HexDatabaseFields>().bridge &&
                        !hex.GetComponent<HexDatabaseFields>().neutralCountry &&
                        //!hex.GetComponent<HexDatabaseFields>().mountain &&
                        !hex.GetComponent<HexDatabaseFields>().impassible &&
                        !hex.GetComponent<HexDatabaseFields>().inAlliedZOC &&
                        (CalculateDistance(hex, targetHex) < closestDistance) &&
                        (GlobalDefinitions.HexUnderStackingLimit(hex, unit.GetComponent<UnitDatabaseFields>().nationality)))
                {
                    closestDistance = CalculateDistance(hex, targetHex);
                    moveHex = hex;
                }
            }

            if (moveHex == unit.GetComponent<UnitDatabaseFields>().occupiedHex)
            {
                // This is a case where the unit is stuck, so at this point move the hex as far away from the current position
                foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                {
                    if (!hex.GetComponent<HexDatabaseFields>().sea &&
                            !hex.GetComponent<HexDatabaseFields>().bridge &&
                            !hex.GetComponent<HexDatabaseFields>().neutralCountry &&
                            //!hex.GetComponent<HexDatabaseFields>().mountain &&
                            !hex.GetComponent<HexDatabaseFields>().impassible &&
                            !hex.GetComponent<HexDatabaseFields>().inAlliedZOC &&
                            (CalculateDistance(hex, unit.GetComponent<UnitDatabaseFields>().occupiedHex) > furthestDistance) &&
                            (GlobalDefinitions.HexUnderStackingLimit(hex, unit.GetComponent<UnitDatabaseFields>().nationality)))
                    {
                        furthestDistance = CalculateDistance(hex, unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                        moveHex = hex;
                    }
                }
            }

            // Move the unit to the hex stored in closestHex
            if ((moveHex == null) || (moveHex == unit.GetComponent<UnitDatabaseFields>().occupiedHex))
                // At this point just move it to the first available hex
                moveHex = unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0];
            else
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(moveHex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);

        }

        /// <summary>
        /// Moves Allied units that aren't attacking as far East or North as possible
        /// </summary>
        /// <param name="unit"></param>
        public static void ExecuteAlliedStrategicMovement(GameObject unit)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("executeAlliedStrategicMovement: executing for unit " + unit.name);
#endif
            // If the unit is far enough North then move it east
            if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().xMapCoor < 28)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("executeAlliedStrategicMovement: looking to move unit east");
#endif
                // For Allied units the goal is to get to Germany, so the unit will move as far west as possible.  This is the hex with the highest y value.
                int maxY = 0;
                GeneralHexRoutines.ResetMovementAvailableFields();
                unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Clear();
                unit.GetComponent<UnitDatabaseFields>().availableMovementHexes
                        = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
                RemoveEnemyZOCHexes(unit.GetComponent<UnitDatabaseFields>().availableMovementHexes, GlobalDefinitions.Nationality.Allied);
                RemoveBridgeHexes(unit.GetComponent<UnitDatabaseFields>().availableMovementHexes);
                RemoveOutOfSupplyHexes(unit.GetComponent<UnitDatabaseFields>().availableMovementHexes);
                foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                    if ((hex.GetComponent<HexDatabaseFields>().yMapCoor > maxY) && GlobalDefinitions.HexUnderStackingLimit(hex, GlobalDefinitions.Nationality.Allied) &&
                            !hex.GetComponent<HexDatabaseFields>().sea && !unit.GetComponent<UnitDatabaseFields>().hasMoved && (hex.GetComponent<HexDatabaseFields>().supplySources.Count > 0))
                        maxY = hex.GetComponent<HexDatabaseFields>().yMapCoor;
                foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                    if ((hex.GetComponent<HexDatabaseFields>().yMapCoor == maxY) && GlobalDefinitions.HexUnderStackingLimit(hex, GlobalDefinitions.Nationality.Allied) &&
                            !hex.GetComponent<HexDatabaseFields>().sea && !unit.GetComponent<UnitDatabaseFields>().hasMoved && (hex.GetComponent<HexDatabaseFields>().supplySources.Count > 0))
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(hex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
            }
            // Otherwise move it North.  This is mainly for units landing in South France
            else
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("executeAlliedStrategicMovement: looking to move unit north");
#endif
                int minX = int.MaxValue;
                GeneralHexRoutines.ResetMovementAvailableFields();
                unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Clear();
                unit.GetComponent<UnitDatabaseFields>().availableMovementHexes
                        = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
                RemoveEnemyZOCHexes(unit.GetComponent<UnitDatabaseFields>().availableMovementHexes, GlobalDefinitions.Nationality.Allied);
                RemoveBridgeHexes(unit.GetComponent<UnitDatabaseFields>().availableMovementHexes);
                RemoveOutOfSupplyHexes(unit.GetComponent<UnitDatabaseFields>().availableMovementHexes);
                foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                    if ((hex.GetComponent<HexDatabaseFields>().xMapCoor < minX) &&
                            GlobalDefinitions.HexUnderStackingLimit(hex, GlobalDefinitions.Nationality.Allied) &&
                            !hex.GetComponent<HexDatabaseFields>().sea &&
                            !hex.GetComponent<HexDatabaseFields>().mountain &&
                            !unit.GetComponent<UnitDatabaseFields>().hasMoved &&
                            (hex.GetComponent<HexDatabaseFields>().supplySources.Count > 0))
                        minX = hex.GetComponent<HexDatabaseFields>().xMapCoor;
                foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                    if ((hex.GetComponent<HexDatabaseFields>().xMapCoor == minX) &&
                            GlobalDefinitions.HexUnderStackingLimit(hex, GlobalDefinitions.Nationality.Allied) &&
                            !hex.GetComponent<HexDatabaseFields>().sea &&
                            !hex.GetComponent<HexDatabaseFields>().mountain &&
                            !unit.GetComponent<UnitDatabaseFields>().hasMoved &&
                            (hex.GetComponent<HexDatabaseFields>().supplySources.Count > 0))
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(hex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
            }
        }

        /// <summary>
        /// This routine returns the hexside that points to the average location of enemy units
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static Vector2 GetAverageEnemyLocation(GlobalDefinitions.Nationality enemyNationality)
        {
            List<GameObject> enemyUnits;

            if (enemyNationality == GlobalDefinitions.Nationality.German)
                enemyUnits = GlobalDefinitions.germanUnitsOnBoard;
            else
                enemyUnits = GlobalDefinitions.alliedUnitsOnBoard;

            int totalX = 0, totalY = 0, totalNumber = 0;

            // Add up the x's and y's of each unit and then average to get the direction
            // The location will be weighted by the attack factor of the unit
            foreach (GameObject tempUnit in enemyUnits)
            {
                totalX += tempUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().xMapCoor * tempUnit.GetComponent<UnitDatabaseFields>().attackFactor;
                totalY += tempUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor * tempUnit.GetComponent<UnitDatabaseFields>().attackFactor;
                totalNumber += tempUnit.GetComponent<UnitDatabaseFields>().attackFactor;
            }

            if (totalNumber == 0)
                // There aren't any enemy units on the map
                return (new Vector2(0, 0));
            else
                return (new Vector2(totalX / totalNumber, totalY / totalNumber));

        }

        /// <summary>
        /// Removes all hexes that are in an enemy ZOC from the list passed
        /// </summary>
        /// <param name="availableMovementHexes"></param>
        private static void RemoveEnemyZOCHexes(List<GameObject> availableMovementHexes, GlobalDefinitions.Nationality friendlyNationality)
        {
            List<GameObject> hexesToRemove = new List<GameObject>();
            foreach (GameObject hex in availableMovementHexes)
                if (GlobalDefinitions.HexInEnemyZOC(hex, friendlyNationality))
                    hexesToRemove.Add(hex);
            foreach (GameObject hex in hexesToRemove)
                if (availableMovementHexes.Contains(hex))
                    availableMovementHexes.Remove(hex);
        }

        /// <summary>
        /// Removes bridges from possible movement hexes
        /// </summary>
        /// <param name="availableMovementHexes"></param>
        private static void RemoveBridgeHexes(List<GameObject> availableMovementHexes)
        {
            List<GameObject> hexesToRemove = new List<GameObject>();
            foreach (GameObject hex in availableMovementHexes)
                if (hex.GetComponent<HexDatabaseFields>().bridge)
                    hexesToRemove.Add(hex);
            foreach (GameObject hex in hexesToRemove)
                if (availableMovementHexes.Contains(hex))
                    availableMovementHexes.Remove(hex);
        }

        /// <summary>
        /// Remove hexes that don't have supply sources
        /// </summary>
        /// <param name="availableMovementHexes"></param>
        private static void RemoveOutOfSupplyHexes(List<GameObject> availableMovementHexes)
        {
            List<GameObject> hexesToRemove = new List<GameObject>();
            foreach (GameObject hex in availableMovementHexes)
                if (hex.GetComponent<HexDatabaseFields>().supplySources.Count == 0)
                    hexesToRemove.Add(hex);
            foreach (GameObject hex in hexesToRemove)
                if (availableMovementHexes.Contains(hex))
                    availableMovementHexes.Remove(hex);
        }

        /// <summary>
        /// This routine goes through all the units on the board and moves any that have not yet moved or attacked
        /// </summary>
        /// <param name="nationality"></param>
        public static void MoveAllUnits(GlobalDefinitions.Nationality nationality)
        {
            //GameObject unit = new GameObject("moveAllUnits");
            GameObject unit;

            unit = ReturnNextUnitToMove(nationality);

            if (unit != null)
                SetUnitMovementValues(unit);

            while (unit != null)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("moveAllUnits: unit " + unit.name + " was returned as the next unit to move");
#endif
                RemoveEnemyZOCHexes(unit.GetComponent<UnitDatabaseFields>().availableMovementHexes, nationality);
                RemoveBridgeHexes(unit.GetComponent<UnitDatabaseFields>().availableMovementHexes);
                unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Sort((b, a) => a.GetComponent<HexDatabaseFields>().hexValue.CompareTo(b.GetComponent<HexDatabaseFields>().hexValue));

#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("moveAllUnits:     available movement hexes = " + unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Count);
#endif
                if (unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Count == 0)
                    unit.GetComponent<UnitDatabaseFields>().hasMoved = true;

                // The available movement hexes are sorted from highest value hex to lowest.  Keep trying to move to a hex until it is successful.
                foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                    if (!unit.GetComponent<UnitDatabaseFields>().hasMoved)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("moveAllUnits:     evaluating hex " + hex.name + " hex value = " + hex.GetComponent<HexDatabaseFields>().hexValue);
#endif
                        // If the highest value hex to move to is the hex the unit is currently occupying leave it where it is
                        if (hex == unit.GetComponent<UnitDatabaseFields>().occupiedHex)
                            unit.GetComponent<UnitDatabaseFields>().hasMoved = true;
                        else if (!hex.GetComponent<HexDatabaseFields>().sea && !hex.GetComponent<HexDatabaseFields>().impassible && !hex.GetComponent<HexDatabaseFields>().neutralCountry &&
                                GlobalDefinitions.HexUnderStackingLimit(hex, unit.GetComponent<UnitDatabaseFields>().nationality))
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(hex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
                    }
                unit.GetComponent<UnitDatabaseFields>().hasMoved = true; // If the unit has moved this is already set but if it hasn't moved, leave it where it is.
                unit = ReturnNextUnitToMove(nationality);
            }
        }

        /// <summary>
        /// This routine will set the value of all highest value movement option for each unit of the nationality passed.
        /// It returns the unit with the highest value hex
        /// </summary>
        /// <param name="nationality"></param>
        /// <returns></returns>
        public static GameObject ReturnNextUnitToMove(GlobalDefinitions.Nationality nationality)
        {
            List<GameObject> unitList;
            //GameObject maxValueUnit = new GameObject("returnNextUnitToMove");
            GameObject maxValueUnit;
            int globalMaxValue = 0;
            int maxValue = 0;
            int globalMaxDiffernce = 0;
            int maxDiffernce = 0;

            List<GameObject> listOfGlobalMaxValueUnits = new List<GameObject>();
            List<GameObject> listOfGlobalMaxDifferenceUnits = new List<GameObject>();

            // Load a list of the units of the right nationality
            if (nationality == GlobalDefinitions.Nationality.Allied)
                unitList = GlobalDefinitions.alliedUnitsOnBoard;
            else
                unitList = GlobalDefinitions.germanUnitsOnBoard;

            // Loop through each unit on the board
            foreach (GameObject unit in unitList)
            {
                // Make sure the unit hasn't already moved and isn't committed to an attack
                if (!unit.GetComponent<UnitDatabaseFields>().hasMoved && !unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnNextUnitToMove: processing unit - " + unit.name);
#endif
                    SetUnitMovementValues(unit.gameObject);
                    if (unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Count > 0)
                        maxValue = unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0].GetComponent<HexDatabaseFields>().hexValue;
                    else
                        maxValue = 0;
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("      max value = " + maxValue);
                GlobalDefinitions.WriteToLogFile("          current hex value = " + unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().hexValue);
#endif
                    maxDiffernce = maxValue - unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().hexValue;
                    // Add the defense factor of the unit to the differnce to allow for influence of stronger units
                    maxDiffernce += unit.GetComponent<UnitDatabaseFields>().defenseFactor;
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("      max difference = " + maxDiffernce);
#endif
                    // If the current max is the new global maximum then store the information
                    if (maxValue > globalMaxValue)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("      new max value = " + maxValue);
#endif
                        listOfGlobalMaxValueUnits.Clear();
                        globalMaxValue = maxValue;
                        listOfGlobalMaxValueUnits.Add(unit.gameObject);
                    }
                    else if (maxValue == globalMaxValue)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("      same as current max value");
#endif
                        listOfGlobalMaxValueUnits.Add(unit.gameObject);
                    }

                    if (maxDiffernce > globalMaxDiffernce)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("      new max difference" + maxDiffernce);
#endif
                        listOfGlobalMaxDifferenceUnits.Clear();
                        globalMaxDiffernce = maxDiffernce;
                        listOfGlobalMaxDifferenceUnits.Add(unit.gameObject);
                    }
                    else if (maxDiffernce == globalMaxDiffernce)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("      same as current max difference");
#endif
                        listOfGlobalMaxDifferenceUnits.Add(unit.gameObject);
                    }

                    // If a unit is currently sitting in an enemy ZOC and is not committed to an attack it has to be moved first so it doesn't
                    // end up getting boxed in and then it causes a problem because it is in a ZOC but it is not part of an attack.
                    if (GlobalDefinitions.HexInEnemyZOC(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.GetComponent<UnitDatabaseFields>().nationality))
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("returnNextUnitToMove: unit " + unit.name + " being returned because it is in enemy ZOC");
#endif
                        return (unit);
                    }
                }
            }

            // This is an alternate way to determine the unit that should move - look at the difference between what their current value is to their potential

            if (listOfGlobalMaxDifferenceUnits.Count == 0)
                return (null);
            else if (listOfGlobalMaxDifferenceUnits.Count == 1)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnNextUnitToMove: only one unit with max difference - " + listOfGlobalMaxDifferenceUnits[0].name);
#endif
                maxValueUnit = listOfGlobalMaxDifferenceUnits[0];
            }
            else
            {
                // This executes when there is more than one unit with the same difference.  Need to break the tie
                // Get the unit with the highest value hex to move to
                maxValueUnit = ReturnUnitWithHighestValueHex(listOfGlobalMaxDifferenceUnits);
#if OUTPUTDEBUG
            if (maxValueUnit != null)
                GlobalDefinitions.WriteToLogFile("returnNextUnitToMove: breaking max difference tie - " + maxValueUnit.name);
#endif
                if (maxValueUnit == null)
                {
                    // Return the unit with the highest defense factor
                    maxValueUnit = ReturnUnitWithHightestDefenseFactor(listOfGlobalMaxDifferenceUnits);
#if OUTPUTDEBUG
                if (maxValueUnit != null)
                    GlobalDefinitions.WriteToLogFile("retrunNextUnitToMove: breaking max difference tie defender with highest defense factor - " + maxValueUnit.name);
#endif
                }
                if (maxValueUnit == null)
                {
                    // At this point punt ... return the first unit in the list
                    maxValueUnit = listOfGlobalMaxDifferenceUnits[0];
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("retrunNextUnitToMove: breaking max difference tie punting to first unit in list - " + maxValueUnit.name);
#endif
                }
            }
            return (maxValueUnit);
        }

        /// <summary>
        /// Sets the hex values for all the hexes the passed unit can move to and returns the value of the highest hex
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static void SetUnitMovementValues(GameObject unit)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("setUnitMovementValues: processing unit " + unit.name + " supply status = " + unit.GetComponent<UnitDatabaseFields>().inSupply + "  remaining movement = " + unit.GetComponent<UnitDatabaseFields>().remainingMovement);
#endif
            // This is a blunt approach but tying to see if this is why I'm getting inconsistent results with returnAvaialbleMovementHexes
            foreach (GameObject tempHex in HexDefinitions.allHexesOnBoard)
            {
                tempHex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                tempHex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
                tempHex.GetComponent<HexDatabaseFields>().hexValue = tempHex.GetComponent<HexDatabaseFields>().intrinsicHexValue;
                tempHex.GetComponent<HexDatabaseFields>().adjacentUnitModifier = 0;
                tempHex.GetComponent<HexDatabaseFields>().sharedZOCModifier = 0;
                tempHex.GetComponent<HexDatabaseFields>().abuttingZOCModifier = 0;
                tempHex.GetComponent<HexDatabaseFields>().stackedUnitModfier = 0;
                tempHex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                tempHex.GetComponent<HexDatabaseFields>().enemyDistanceModifier = 0;
                tempHex.GetComponent<HexDatabaseFields>().supplyModifier = 0;

                // AI TESTING
                //GlobalDefinitions.updateHexValueText(tempHex.gameObject);
            }

            GeneralHexRoutines.ResetMovementAvailableFields();
            unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Clear();
            unit.GetComponent<UnitDatabaseFields>().availableMovementHexes =
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.gameObject);

            // Calculate the value for all available moves
            foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
            {

                hex.GetComponent<HexDatabaseFields>().hexValue = ReturnHexValue(hex, unit);
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("setUnitMovementValues: hex value for hex " + hex.name + " with value = " + hex.GetComponent<HexDatabaseFields>().hexValue);
#endif
                // AI TESTING
                //GlobalDefinitions.updateHexValueText(hex);
            }

            unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Sort((b, a) => a.GetComponent<HexDatabaseFields>().hexValue.CompareTo(b.GetComponent<HexDatabaseFields>().hexValue));
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("setUnitMovementValues: Available movement hexes returned - count = " + unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Count);
        foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
            GlobalDefinitions.WriteToLogFile("setUnitMovementValues:        " + hex.name + " value = " + hex.GetComponent<HexDatabaseFields>().hexValue);
#endif
        }

        /// <summary>
        /// Returns the value of the hex passed for the nationality of the unit passed
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        private static int ReturnHexValue(GameObject hex, GameObject unit)
        {
            int returnValue = hex.GetComponent<HexDatabaseFields>().intrinsicHexValue;
            if (hex.GetComponent<HexDatabaseFields>().successfullyInvaded)
                returnValue += GlobalDefinitions.successfullInvasionHexValue;

            // Set values differently based on nationality.  The allies need to worry about supply and direction of attack (to get to Germany)
            if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("      setUnitMovementValues: Calculating hex value for hex - " + hex.name);
#endif
                returnValue += ReturnUnitContextHexValueModifier(hex, unit);
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("            setUnitMovementValues: Executed returnUnitContextHexValueModifier hexValue = " + returnValue);
#endif
                returnValue += ReturnEnemyDistanceHexModifierValue(hex, unit);
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("            setUnitMovementValues: Executed returnEnemyDistanceHexModifierValue hexValue = " + returnValue);
#endif
                returnValue += ReturnRiverContextHexValueModifier(hex, unit);
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("            setUnitMovementValues: Executed returnRiverContextHexValueModifier hexValue = " + returnValue);
#endif
                returnValue += ReturnSupplyContextHexValueModifier(hex, unit);
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("            setUnitMovementValues: Executed returnSupplyContextHexValueModifier hexValue = " + returnValue);
#endif
            }
            else
            {
#if OUTPUTDEBUG
            //GlobalDefinitions.WriteToLogFile("      setUnitMovementValues: Calculating hex value for hex - " + hex.name);
#endif
                returnValue += ReturnUnitContextHexValueModifier(hex, unit);
#if OUTPUTDEBUG
            //GlobalDefinitions.WriteToLogFile("            setUnitMovementValues: Executed returnUnitContextHexValueModifier hexValue = " + returnValue);
#endif
                returnValue += ReturnEnemyDistanceHexModifierValue(hex, unit);
#if OUTPUTDEBUG
            //GlobalDefinitions.WriteToLogFile("            setUnitMovementValues: Executed returnEnemyDistanceHexModifierValue hexValue = " + returnValue);
#endif
                returnValue += ReturnRiverContextHexValueModifier(hex, unit);
#if OUTPUTDEBUG
            //GlobalDefinitions.WriteToLogFile("            setUnitMovementValues: Executed returnRiverContextHexValueModifier hexValue = " + returnValue);
#endif
                returnValue += ReturnSupplyContextHexValueModifier(hex, unit);
#if OUTPUTDEBUG
            //GlobalDefinitions.WriteToLogFile("            setUnitMovementValues: Executed returnSupplyContextHexValueModifier hexValue = " + returnValue);
#endif
            }

            return (returnValue);
        }

        /// <summary>
        /// This routine returns the hex value modifier for adjacency to friendly units
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static int ReturnUnitContextHexValueModifier(GameObject hex, GameObject unit)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnUnitContextHexValueModifier: Evaluating hex - " + hex.name);
#endif
            bool modifierAdded = false;
            int returnValue = 0;
            bool matchUnit = false;
            bool adjacentZOCModiferAdded = true;

            // Only check if the unit is already on the hex or there is availability on the hex
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Contains(unit) ||
                    GlobalDefinitions.HexUnderStackingLimit(hex, unit.GetComponent<UnitDatabaseFields>().nationality))
            {
                // Check for stacked unit bonus
                if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                {
                    // Make sure the unit being stacked with is not the unit that we're looking to move
                    if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 1) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0] == unit))
                    {

                    }
                    else
                    {
                        modifierAdded = true;
                        hex.GetComponent<HexDatabaseFields>().stackedUnitModfier += GlobalDefinitions.stackedUnitHexModifier;
                        returnValue += GlobalDefinitions.stackedUnitHexModifier;
                    }
                }

                // Check for an adjacent unit
                modifierAdded = false;  // Reset this to false.  We will add only one modifier regardless of how many adjacent units there are
                foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                    if (!modifierAdded)
                    {
                        if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                                && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                                && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality
                                == unit.GetComponent<UnitDatabaseFields>().nationality))
                        {
                            // Need to make sure the unit we're planning to move isn't the unit being counted as the adjacent unit.  This happens when scoring around the original hex
                            matchUnit = false;
                            if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count == 1) &&
                                    (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0] == unit))
                                matchUnit = true;

                            if (!matchUnit)
                            {
                                modifierAdded = true;
                                hex.GetComponent<HexDatabaseFields>().adjacentUnitModifier += GlobalDefinitions.adjacentUnitHexModifier;
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnUnitContextHexValueModifier:    adding adjacent modifier for hex - " + hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].name);
#endif
                                returnValue += GlobalDefinitions.adjacentUnitHexModifier;
                            }
                        }
                    }

                // Check for adding a modifier for being adjacent to a friendly ZOC
                modifierAdded = false; // Reset to false we will add a adjacent ZOC modifier once regardless of how many there are
                foreach (GameObject tempHex in ReturnHexesWithinDistance(hex, 1))
                    if (!modifierAdded)
                    {
                        if (GlobalDefinitions.HexInFriendlyZOC(tempHex, unit.GetComponent<UnitDatabaseFields>().nationality))
                        {
                            // Set the modifier if there is ZOC in the hex and no friendly units (otherwise we would be adding two modifiers for an adjacent unit)
                            if ((tempHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0) || ((tempHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                                    (tempHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality != unit.GetComponent<UnitDatabaseFields>().nationality)))
                            {
                                // The modifier will not be added if it is the unit that being evaluated is causing the ZOC.  This is an issue when scoring hexes around the starting location
                                if (!CheckIfUnitIsOnlyFriendlyUnitExertingZOC(tempHex, unit))
                                {
                                    hex.GetComponent<HexDatabaseFields>().sharedZOCModifier += GlobalDefinitions.adjacentZOCHexModifier;
                                    returnValue += GlobalDefinitions.adjacentZOCHexModifier;
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("returnUnitContextHexValueModifier:    adding adjacent ZOC modifier for hex - " + tempHex.name);
#endif
                                    modifierAdded = true;
                                    adjacentZOCModiferAdded = true;
                                }
                            }
                        }
                    }

                // I'm not going to check for abutting if I have an adjacent ZOC because that means whenever there is a unit two hexes away it will always be counted twice 
                // since the abutted doesn't distinguish from a ZOC being projected or there because a unit is in the hex.
                if (!adjacentZOCModiferAdded)
                {
                    // Check for abutting ZOC.  Note this adds a modifier if the hex two hexes away is in ZOC regardless of whether that is
                    // because it is projected or has a friendly unit on it.

                    // In order to get the list of hexes two hexes away I will get the list of hexes 1 away and take these away from the list of hexes 2 away
                    List<GameObject> hexesOneAway = ReturnHexesWithinDistance(hex, 1);
                    List<GameObject> hexesTwoAway = ReturnHexesWithinDistance(hex, 2);
                    List<GameObject> hexesOnlyTwoAway = new List<GameObject>();
                    List<GameObject> zocHexes = new List<GameObject>();

                    foreach (GameObject tempHex in hexesTwoAway)
                        if (!hexesOneAway.Contains(tempHex))
                            hexesOnlyTwoAway.Add(tempHex);

                    // Load a list of the hexes one hex away that the current hex projects ZOC into
                    foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                        if (hex.GetComponent<BooleanArrayData>().exertsZOC[(int)hexSide])
                            zocHexes.Add(hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);

                    modifierAdded = false;
                    foreach (GameObject zocHex in zocHexes)
                    {
                        // Only add one modifier for each ZOC hex.  There could be multiple hexes with ZOC abutting
                        if (!modifierAdded)
                        {
                            foreach (GameObject tempHex in hexesOnlyTwoAway)
                            {
                                if (!modifierAdded && HexInFriendlyZOC(tempHex, unit))
                                {
                                    // At this point the tempHex is two hexes away and is in friendly ZOC
                                    foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                                    {
                                        // Make sure we're not seeing an abutting ZOC only due to the hex the unit is originally in
                                        matchUnit = false;
                                        if (((tempHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 1) && (tempHex.GetComponent<HexDatabaseFields>().occupyingUnit[0] == unit)))
                                            matchUnit = true;
                                        // Check that the ZOC of control isn't there only because of the unit that we're checking
                                        if (!matchUnit && !CheckIfUnitIsOnlyFriendlyUnitExertingZOC(tempHex, unit))

                                            // Check to see if the zoc's one hex away are adjacent to the tempHex
                                            if (!modifierAdded && (zocHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] == tempHex))
                                            {
                                                modifierAdded = true;
                                                hex.GetComponent<HexDatabaseFields>().abuttingZOCModifier += GlobalDefinitions.abuttedZOCHexModifier;
#if OUTPUTDEBUG
                                            GlobalDefinitions.WriteToLogFile("returnUnitContextHexValueModifier:    adding abutted ZOC modifier for hex - " + tempHex.name);
#endif
                                                returnValue += GlobalDefinitions.abuttedZOCHexModifier;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (returnValue);
        }

        /// <summary>
        /// This routine returns the hex value modifier for distance to the nearest enemy unit
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static int ReturnEnemyDistanceHexModifierValue(GameObject hex, GameObject unit)
        {
            List<GameObject> hexList;

            // Since I'm going with the approach that a unit will move into attack position during the movement allocation
            // I have to check here for adding a distance modifier when the unit is in an enemy ZOC.  During testing 
            // it was clear that having an incresed modifier just for being next to an enemy unit caused movement that 
            // was based on nothing other than it was next to an enemy unit rather than there being any intrinsic value
            // in the hex.

            for (int distance = 1; distance <= GlobalDefinitions.enemyUnitModiferDistance; distance++)
            {
                // Get the hexes within the current distance of the hex
                hexList = ReturnHexesWithinDistance(hex, distance);
                // Search to see if an enemy is on any of the hexes
                // The check for the hex not being in enemy ZOC is to keep units moving to attack for no other reason than they can
                foreach (GameObject tempHex in hexList)
                    if ((tempHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (tempHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality != unit.GetComponent<UnitDatabaseFields>().nationality) &&
                            !GlobalDefinitions.HexInEnemyZOC(hex, unit.GetComponent<UnitDatabaseFields>().nationality))
                    {
                        hex.GetComponent<HexDatabaseFields>().enemyDistanceModifier = ReturnDistanceToEnemyHexValueModifier(distance);
                        return (hex.GetComponent<HexDatabaseFields>().enemyDistanceModifier);
                    }
            }
            hex.GetComponent<HexDatabaseFields>().enemyDistanceModifier = 0;
            return (0);
        }

        /// <summary>
        /// This routine will return the hex value modifier for a river.  It will look for enemy units within four hexes and determine
        /// if there is a river between the unit passed and the enemy unit.  If there is it will modify the hex(es) behind the river.
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static int ReturnRiverContextHexValueModifier(GameObject hex, GameObject unit)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier: evaluating hex " + hex.name);
#endif
            // If the hex doesn't have a river abutting it there is not reason to do any checks
            bool riverPresent = false;
            for (int index = 0; index < 6; index++)
                if (hex.GetComponent<BooleanArrayData>().riverSides[index] == true)
                    riverPresent = true;
            if (!riverPresent)
                return (0);

            // If the hex is a city or fortress there is no river bonus
            if (hex.GetComponent<HexDatabaseFields>().fortress || hex.GetComponent<HexDatabaseFields>().city)
                return (0);

            List<GameObject> nearbyEnemyUnits = FindNearbyEnemyUnits(hex, unit.GetComponent<UnitDatabaseFields>().nationality, GlobalDefinitions.riverModifierDistance);

            // We have all the nearby enemy units.  Now we just need to go through all of them and determine if 
            // there is a river edge on the hex between the unit passed and the enemy unit being checked.
            // First see if the hex has a river bordering it.
            // I originally returned a true if I found a unit across a river hex.  I need to check if there are enemy 
            // units on both sides of the river before I return true since if there are units on both sides the 
            // river modifier should not be added.  The way I will do it is to see if there are units without a 
            // river between and return a false, if I get through all the checks then I will return a true as default
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   number of nearby enemy units = " + nearbyEnemyUnits.Count);
#endif
            if (nearbyEnemyUnits.Count > 0)
            {
                foreach (GameObject enemyUnit in nearbyEnemyUnits)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   evaluating river modifier for enemy unit " + enemyUnit.name);
#endif
                    if (hex.transform.position.x < enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.x)
                    {
                        // The enemy is to the right
                        if (hex.transform.position.y < enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.y)
                        //if (hex.GetComponent<HexDatabaseFields>().yMapCoor < enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor)
                        {
                            // The enemy is to the upper right so no modification to the hex value if there is no river on the North or NorthEast sides
                            if (!hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.North] ||
                                    !hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.NorthEast])
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   returning 0 modifier - " + enemyUnit.name + " is to the upper right");
#endif
                                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                                return (0);
                            }
                        }
                        else if (hex.transform.position.y > enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.y)
                        {
                            // The enemy is to the bottom right so no modification to the hex value if there is no river on the South or SouthEast sides
                            if (!hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.South] ||
                                    !hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.SouthEast])
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   returning 0 modifier - " + enemyUnit.name + " is to the bottom right");
#endif
                                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                                return (0);
                            }
                        }
                        else
                        {
                            // The enemy is directly to the right so no modification to the hex value if there is no river on the NorthEast and SouthEast sides
                            if (hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.NorthEast] ||
                                    hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.SouthEast])
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   returning 0 modifier - " + enemyUnit.name + " is to the right");
#endif
                                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                                return (0);
                            }
                        }
                    }

                    if (hex.transform.position.x > enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.x)
                    {
                        // The enemy is to the left
                        if (hex.transform.position.y < enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.y)
                        {
                            // The enemy is to the upper left so no modification to the hex value if there is no river on the North and NorthWest sides
                            if (!hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.North] ||
                                    !hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.NorthWest])
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   returning 0 modifier - " + enemyUnit.name + " is to the upper left");
#endif
                                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                                return (0);
                            }
                        }
                        else if (hex.transform.position.y > enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.y)
                        {
                            // The enemy is to the bottom left so no modification to the hex value if there is no river on the South and SouthWest sides
                            if (!hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.South] ||
                                    !hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.SouthWest])
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   returning 0 modifier - " + enemyUnit.name + " is to the bottom left");
#endif
                                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                                return (0);
                            }
                        }
                        else
                        {
                            // The enemy is directly to the left so no modification to the hex value if there is no river on the NorthWest and SouthWest sides
                            if (!hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.NorthWest] ||
                                    !hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.SouthWest])
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   returning 0 modifier - " + enemyUnit.name + " is to the left");
#endif
                                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                                return (0);
                            }
                        }
                    }

                    if (hex.transform.position.x == enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.x)
                    {
                        // The enemy is on the same x coordinate

                        if (hex.transform.position.y < enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.y)
                        {
                            // The enemy is directly above so no modification to the hex value if there is no river on the North side
                            if (!hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.North])
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   returning 0 modifier - " + enemyUnit.name + " is directly above");
#endif
                                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                                return (0);
                            }
                        }
                        else if (hex.transform.position.y > enemyUnit.GetComponent<UnitDatabaseFields>().occupiedHex.transform.position.y)
                        {
                            // The enemy is directly below so no modification to the hex value if there is no river on the South side
                            if (!hex.GetComponent<BooleanArrayData>().riverSides[(int)HexDefinitions.HexSides.South])
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   returning 0 modifier - " + enemyUnit.name + " is directly below");
#endif
                                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                                return (0);
                            }
                        }

                        // Note I don't need another check since the enemy unit cannot be on the hex
                    }
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnRiverContextHexValueModifier:   setting modifier");
#endif
                    hex.GetComponent<HexDatabaseFields>().riverModifier = GlobalDefinitions.riverHexModifier;
                }
            }
            else
                // No nearby enemy units so not modifier
                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
            return (hex.GetComponent<HexDatabaseFields>().riverModifier);
        }

        /// <summary>
        /// Returns the supply modifier for the hex value
        /// This is determined by whether the hex has a supply source or not
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static int ReturnSupplyContextHexValueModifier(GameObject hex, GameObject unit)
        {
            if (hex.GetComponent<HexDatabaseFields>().supplySources.Count == 0)
            {
                // German's in fortress don't have a supply source but they are in supply
                if (hex.GetComponent<HexDatabaseFields>().fortress && (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German))
                    return (0);
                else
                {
                    hex.GetComponent<HexDatabaseFields>().supplyModifier = GlobalDefinitions.supplyHexModifier;
                    return (GlobalDefinitions.supplyHexModifier);
                }
            }
            else
                return (0);
        }

        /// <summary>
        /// Returns the modifier for a hex being a supply source.  Used for Allies.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static int ReturnSupplySourceHexValueModifier(GameObject hex, GlobalDefinitions.Nationality nationality)
        {
            if (SupplyNeeded() && hex.GetComponent<HexDatabaseFields>().supplyCapacity > 0)
                return (GlobalDefinitions.supplySourceValue);

            return (0);
        }

        /// <summary>
        /// This routine resets the context hex value modifiers for all hexes
        /// </summary>
        public static void ResetContextHexValueModifiers(List<GameObject> hexList)
        {
            foreach (GameObject hex in hexList)
            {
                hex.GetComponent<HexDatabaseFields>().hexValue = hex.GetComponent<HexDatabaseFields>().intrinsicHexValue;
                hex.GetComponent<HexDatabaseFields>().adjacentUnitModifier = 0;
                hex.GetComponent<HexDatabaseFields>().sharedZOCModifier = 0;
                hex.GetComponent<HexDatabaseFields>().abuttingZOCModifier = 0;
                hex.GetComponent<HexDatabaseFields>().stackedUnitModfier = 0;
                hex.GetComponent<HexDatabaseFields>().riverModifier = 0;
                hex.GetComponent<HexDatabaseFields>().supplyModifier = 0;
                hex.GetComponent<HexDatabaseFields>().enemyDistanceModifier = 0;
                // AI TESTING
                //GlobalDefinitions.updateHexValueText(hex);
            }
        }


        /// <summary>
        /// This routine returns true if the passed hex is in a friendly ZOC
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool HexInFriendlyZOC(GameObject hex, GameObject unit)
        {
            if ((unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) && hex.GetComponent<HexDatabaseFields>().inAlliedZOC)
            {
                // Make sure the unit we're looking to move isn't the only unit projecting this ZOC
                if ((hex.GetComponent<HexDatabaseFields>().unitsExertingZOC.Count == 1) && (hex.GetComponent<HexDatabaseFields>().unitsExertingZOC[0] == unit))
                    return (false);
                // Also check that the unit we're looking to move isn't the only unit on the hex and no other friendly unit is projecting ZOC
                else if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 1) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0] == unit) && !CheckIfFriendyUnitProjectingZOC(hex, unit))
                    return (false);
                else
                    return (true);
            }
            if ((unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German) &&
                    hex.GetComponent<HexDatabaseFields>().inGermanZOC)
            {
                // Make sure the unit we're looking to move isn't the only unit projecting this ZOC
                if ((hex.GetComponent<HexDatabaseFields>().unitsExertingZOC.Count == 1) && (hex.GetComponent<HexDatabaseFields>().unitsExertingZOC[0] == unit))
                    return (false);
                else
                    return (true);
            }
            return (false);

        }

        /// <summary>
        /// This routine returns true if a friendly unit is projecting ZOC into the hex
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool CheckIfFriendyUnitProjectingZOC(GameObject hex, GameObject unit)
        {
            foreach (GameObject tempUnit in hex.GetComponent<HexDatabaseFields>().unitsExertingZOC)
                if (tempUnit.GetComponent<UnitDatabaseFields>().nationality == unit.GetComponent<UnitDatabaseFields>().nationality)
                    return (true);
            return (false);

        }

        /// <summary>
        /// This routine will return a list of enemy units that are within the distance passed of the hex passed
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static List<GameObject> FindNearbyEnemyUnits(GameObject hex, GlobalDefinitions.Nationality friendlyNationality, int distance)
        {
            List<GameObject> nearbyEnemyUnits = new List<GameObject>();
            List<GameObject> hexesToCheck = ReturnHexesWithinDistance(hex, distance);
            foreach (GameObject tempHex in hexesToCheck)
                if ((tempHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                        (tempHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality != friendlyNationality))
                    foreach (GameObject tempUnit in tempHex.GetComponent<HexDatabaseFields>().occupyingUnit)
                        nearbyEnemyUnits.Add(tempUnit);

#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("findNearbyEnemyUnits: returning number of enemy units = " + nearbyEnemyUnits.Count);
#endif
            return (nearbyEnemyUnits);
        }

        /// <summary>
        /// Returns a list of unit of the nationality passed within the passed distance of the hex passed
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="nationality"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static List<GameObject> FindNearbyUnits(GameObject hex, GlobalDefinitions.Nationality nationality, int distance)
        {
            List<GameObject> nearbyEnemyUnits = new List<GameObject>();
            List<GameObject> hexesToCheck = ReturnHexesWithinDistance(hex, distance);
            foreach (GameObject tempHex in hexesToCheck)
                if ((tempHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                        (tempHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == nationality))
                    foreach (GameObject tempUnit in tempHex.GetComponent<HexDatabaseFields>().occupyingUnit)
                        nearbyEnemyUnits.Add(tempUnit);

#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("findNearbyEnemyUnits: returning number of enemy units = " + nearbyEnemyUnits.Count);
#endif
            return (nearbyEnemyUnits);
        }

        /// <summary>
        /// Returns the number of armor units available through this invasion area passed for this turn
        /// </summary>
        /// <param name="invasionAreaIndex"></param>
        private static int ReturnNumberOfArmorUnitsAvailable(int invasionAreaIndex)
        {
            if ((GlobalDefinitions.firstInvasionAreaIndex == invasionAreaIndex) || (GlobalDefinitions.secondInvasionAreaIndex == invasionAreaIndex))
            {
                if (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 1)
                    return (GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn - GlobalDefinitions.invasionAreas[invasionAreaIndex].firstTurnArmor);
                else if (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 2)
                    return (GlobalDefinitions.invasionAreas[invasionAreaIndex].armorUnitsUsedThisTurn - GlobalDefinitions.invasionAreas[invasionAreaIndex].secondTurnArmor);
            }
            return (GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn - GlobalDefinitions.invasionAreas[invasionAreaIndex].divisionsPerTurn);
        }

        /// <summary>
        /// Returns the number of infantry units available through this invasion area passed for this turn
        /// </summary>
        /// <param name="invasionAreaIndex"></param>
        private static int ReturnNumberOfInfantryUnitsAvailable(int invasionAreaIndex)
        {
            if ((GlobalDefinitions.firstInvasionAreaIndex == invasionAreaIndex) || (GlobalDefinitions.secondInvasionAreaIndex == invasionAreaIndex))
            {
                if (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 1)
                    return (GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn - GlobalDefinitions.invasionAreas[invasionAreaIndex].firstTurnInfantry);
                else if (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 2)
                    return (GlobalDefinitions.invasionAreas[invasionAreaIndex].infantryUnitsUsedThisTurn - GlobalDefinitions.invasionAreas[invasionAreaIndex].secondTurnInfantry);
            }
            return (GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn - GlobalDefinitions.invasionAreas[invasionAreaIndex].divisionsPerTurn);
        }

        /// <summary>
        /// Returns the number of infantry units available through this invasion area passed for this turn
        /// </summary>
        /// <param name="invasionAreaIndex"></param>
        private static int ReturnNumberOfHQUnitsAvailable(int invasionAreaIndex)
        {
            if ((GlobalDefinitions.firstInvasionAreaIndex == invasionAreaIndex) || (GlobalDefinitions.secondInvasionAreaIndex == invasionAreaIndex))
            {
                if (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 1)
                    return (0);
                else if (GlobalDefinitions.invasionAreas[invasionAreaIndex].turn == 2)
                    return (0);
            }
            return (GlobalDefinitions.invasionAreas[invasionAreaIndex].totalUnitsUsedThisTurn - GlobalDefinitions.invasionAreas[invasionAreaIndex].divisionsPerTurn);
        }

        /// <summary>
        /// This routine will return all hexes within the passed distance of the hex passed
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static List<GameObject> ReturnHexesWithinDistance(GameObject hex, int distance)
        {
            List<GameObject> hexList = new List<GameObject>();
            List<GameObject> addList = new List<GameObject>();
            hexList.Add(hex);
            for (int index = 1; index <= distance; index++)
            {
                // Go through the hexes in the list and add all neighbors
                foreach (GameObject tempHex in hexList)
                    foreach (HexDefinitions.HexSides hexSides in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                        if (tempHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] != null &&
                                !tempHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().sea &&
                                !tempHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().neutralCountry &&
                                !tempHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().impassible &&
                                !hexList.Contains(tempHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides]))
                            addList.Add(tempHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides]);

                foreach (GameObject tempHex in addList)
                    if (!hexList.Contains(tempHex))
                        hexList.Add(tempHex);
            }
            return (hexList);
        }

        /// <summary>
        /// Returns the best movement option for the unit from those stored in the unit's maxMovementValueHexes List
        /// </summary>
        /// <param name="unit"></param>
        private static GameObject ReturnBestMovementOptionForUnit(GameObject unit)
        {
            List<GameObject> bestHexes = new List<GameObject>();

            // The best option will be for the unit to stay in place if that is one of the hexes
            //if (unit.GetComponent<UnitDatabaseFields>().maxMovementValueHexes.Contains(unit.GetComponent<UnitDatabaseFields>().occupiedHex))
            //{
            //    //GlobalDefinitions.WriteToLogFile("returnBestMovementOptionForUnit: best option is to remain in place " + unit.GetComponent<UnitDatabaseFields>().occupiedHex.name + "  Hex Value = " + unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().hexValue);
            //    return (unit.GetComponent<UnitDatabaseFields>().occupiedHex);
            //}

            // Return the hex that is closest to the enemy
            float closestDistance = float.MaxValue;
            float distance = float.MaxValue;

            foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
            {
                distance = (float)Math.Sqrt(Math.Pow(Math.Abs(targetLocation.x - hex.GetComponent<HexDatabaseFields>().xMapCoor), 2) + Math.Pow(Math.Abs(targetLocation.y - hex.GetComponent<HexDatabaseFields>().yMapCoor), 2));
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestHexes.Clear();
                    bestHexes.Add(hex);
                }
                else if (distance == closestDistance)
                {
                    bestHexes.Add(hex);
                }
            }

            if (bestHexes.Count == 0)
            {
                // This shouldn't happen but ...
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnBestMovementOptionForUnit: ERROR - check for distance to enemy resulted in no result - returning first listed hex by default " + unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0].name + "  Hex Value = " + unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0].GetComponent<HexDatabaseFields>().hexValue);
#endif
                // punt
                return (unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0]);
            }
            else if (bestHexes.Count == 1)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnBestMovementOptionForUnit: best option based on closest hex to enemy " + bestHexes[0].name + "  Hex Value = " + bestHexes[0].GetComponent<HexDatabaseFields>().hexValue);
#endif
                return (bestHexes[0]);
            }
            else
            {
                // More than one of the options was the "closest" to the enemy
                // Will break this tie by using the one that is closest to the current position of the unit
                closestDistance = float.MaxValue;
                distance = float.MaxValue;
                List<GameObject> closestToUnit = new List<GameObject>();

                foreach (GameObject hex in bestHexes)
                {
                    distance = (float)Math.Sqrt(Math.Pow(Math.Abs(targetLocation.x - hex.GetComponent<HexDatabaseFields>().xMapCoor), 2) + Math.Pow(Math.Abs(targetLocation.y - hex.GetComponent<HexDatabaseFields>().yMapCoor), 2));
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestToUnit.Clear();
                        closestToUnit.Add(hex);
                    }
                    else if (distance == closestDistance)
                    {
                        closestToUnit.Add(hex);
                    }
                }

                if (closestToUnit.Count == 0)
                {
                    // This shouldn't happen but ...
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnBestMovementOptionForUnit: ERROR - check for distance to unit resulted in no result - returning first listed hex by default " + bestHexes[0].name + "  Hex Value = " + bestHexes[0].GetComponent<HexDatabaseFields>().hexValue);
#endif
                    // punt
                    return (bestHexes[0]);
                }
                else if (closestToUnit.Count == 1)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnBestMovementOptionForUnit: best option based on closest hex to enemy " + closestToUnit[0].name + "  Hex Value = " + closestToUnit[0].GetComponent<HexDatabaseFields>().hexValue);
#endif
                    return (closestToUnit[0]);
                }
                else
                {
                    // I give up
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnBestMovementOptionForUnit: ERROR - check for distance to unit resulted in no result - returning first listed hex by default " + bestHexes[0].name + "  Hex Value = " + bestHexes[0].GetComponent<HexDatabaseFields>().hexValue);
#endif
                    return (unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0]);
                }
            }
        }

        /// <summary>
        /// This routine sets the intrinsic value of each hex on the board
        /// </summary>
        public static void SetIntrinsicHexValues()
        {
            foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
            {
                if (hex.GetComponent<HexDatabaseFields>().fortress)
                    hex.GetComponent<HexDatabaseFields>().intrinsicHexValue = GlobalDefinitions.fortressIntrinsicValue;
                else if (hex.GetComponent<HexDatabaseFields>().city)
                    hex.GetComponent<HexDatabaseFields>().intrinsicHexValue = GlobalDefinitions.cityIntrinsicValue;
                else if (hex.GetComponent<HexDatabaseFields>().fortifiedZone)
                    hex.GetComponent<HexDatabaseFields>().intrinsicHexValue = GlobalDefinitions.fortifiedZoneIntrinsicValue;
                else if (hex.GetComponent<HexDatabaseFields>().mountain)
                    hex.GetComponent<HexDatabaseFields>().intrinsicHexValue = GlobalDefinitions.mountainIntrinsicValue;
                else if (hex.GetComponent<HexDatabaseFields>().impassible || hex.GetComponent<HexDatabaseFields>().neutralCountry || hex.GetComponent<HexDatabaseFields>().sea)
                    hex.GetComponent<HexDatabaseFields>().intrinsicHexValue = 0;
                else
                    hex.GetComponent<HexDatabaseFields>().intrinsicHexValue = GlobalDefinitions.landIntrinsicValue;
                hex.GetComponent<HexDatabaseFields>().hexValue = hex.GetComponent<HexDatabaseFields>().intrinsicHexValue;
            }
        }

        /// <summary>
        /// This routine is used to break a tie with units.  It returns the unit with the highest value available movement hex.  If there is still a tie it returns null.
        /// </summary>
        /// <param name="unitList"></param>
        /// <returns></returns>
        private static GameObject ReturnUnitWithHighestValueHex(List<GameObject> unitList)
        {
            int maxValue = 0;

            foreach (GameObject unit in unitList)
            {
                if ((unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Count > 0)
                        && (unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0].GetComponent<HexDatabaseFields>().hexValue > maxValue))
                {
                    maxValue = unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0].GetComponent<HexDatabaseFields>().hexValue;
                }
                else if ((unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Count > 0) && (maxValue > 0)
                        && (unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0].GetComponent<HexDatabaseFields>().hexValue == maxValue))
                {
                    // This means that there is more than one unit with the maximum value hex
                    return (null);
                }
            }

            // If we get here that means that there is one unit with the highest hex movement value, find it and return it
            foreach (GameObject unit in unitList)
            {
                if ((unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Count > 0) && (maxValue > 0)
                        && (unit.GetComponent<UnitDatabaseFields>().availableMovementHexes[0].GetComponent<HexDatabaseFields>().hexValue == maxValue))
                {
                    return (unit);
                }
            }

            // Should never get here but need a return to satisfy the compiler
            return (null);
        }

        /// <summary>
        /// This routine is used to break a tie with units.  It returns the unit with the highest defense factor.  If there is still a tie it returns null.
        /// </summary>
        /// <param name="unitList"></param>
        /// <returns></returns>
        private static GameObject ReturnUnitWithHightestDefenseFactor(List<GameObject> unitList)
        {
            int maxValue = 0;
            foreach (GameObject unit in unitList)
            {
                if (unit.GetComponent<UnitDatabaseFields>().defenseFactor > maxValue)
                {
                    maxValue = unit.GetComponent<UnitDatabaseFields>().defenseFactor;
                }
                else if ((maxValue > 0) && (unit.GetComponent<UnitDatabaseFields>().defenseFactor == maxValue))
                {
                    // This means that there is another unit with the same defense factor so return null
                    return (null);
                }
            }

            foreach (GameObject unit in unitList)
            {
                if ((maxValue > 0) && (unit.GetComponent<UnitDatabaseFields>().defenseFactor == maxValue))
                    return (unit);
            }

            // Should never get here but need a return to satisfy the compiler
            return (null);
        }

        /// <summary>
        /// Returns true if the unit passed is the only unit projecting friendly ZOC onto the hex
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        private static bool CheckIfUnitIsOnlyFriendlyUnitExertingZOC(GameObject hex, GameObject unit)
        {
            List<GameObject> friendlyUnitsExertingZOC = new List<GameObject>();
            // This routine is needed because I was just checking if the number of exerting units was 1 and if it was the unit than I wouldn't add the modifier.
            // The problem with this is that enemy units could also be exerting ZOC so they would be included so that means I can't just check the unit in the 0 position
            foreach (GameObject tempUnit in hex.GetComponent<HexDatabaseFields>().unitsExertingZOC)
                if (tempUnit.GetComponent<UnitDatabaseFields>().nationality == unit.GetComponent<UnitDatabaseFields>().nationality)
                    friendlyUnitsExertingZOC.Add(tempUnit);

            if ((friendlyUnitsExertingZOC.Count == 1) && (friendlyUnitsExertingZOC[0] == unit))
                return (true);
            else
                return (false);
        }

        //=============================================================================================================================================================================================
        // THE FOLLOWING ARE THE AI COMBAT ROUTINES

        /// <summary>
        /// This routine sets the value of the hexs for German units
        /// </summary>
        /// <param name="defendingHexes"></param>
        /// <param name="defendingUnitsOnBoard"></param>
        public static void SetGermanAttackHexValues(List<GameObject> defendingHexes)
        {
            // Get all hexes with defenders on them
            foreach (GameObject defendingUnit in GlobalDefinitions.alliedUnitsOnBoard)
                if (!defendingHexes.Contains(defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex))
                    defendingHexes.Add(defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex);

            // Now sort the defending hexes by intrinsic hex value
            GameObject tempHex;
            for (int index1 = 0; index1 < defendingHexes.Count; index1++)
                for (int index2 = (index1 + 1); index2 < defendingHexes.Count; index2++)
                    if (defendingHexes[index1].GetComponent<HexDatabaseFields>().intrinsicHexValue < defendingHexes[index2].GetComponent<HexDatabaseFields>().intrinsicHexValue)
                    {
                        tempHex = defendingHexes[index1];
                        defendingHexes[index1] = defendingHexes[index2];
                        defendingHexes[index2] = tempHex;
                    }
        }

        /// <summary>
        /// This routine sets the value of the hexs for Allied units
        /// </summary>
        /// <param name="defendingHexes"></param>
        /// <param name="defendingUnitsOnBoard"></param>
        public static void SetAlliedAttackHexValues(List<GameObject> defendingHexes)
        {
            // Get all hexes with defenders on them
            foreach (GameObject defendingUnit in GlobalDefinitions.germanUnitsOnBoard)
                if (!defendingHexes.Contains(defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex))
                    defendingHexes.Add(defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex);

            // Now sort the defending hexes by intrinsic hex value
            GameObject tempHex;
            for (int index1 = 0; index1 < defendingHexes.Count; index1++)
                for (int index2 = (index1 + 1); index2 < defendingHexes.Count; index2++)
                    if (defendingHexes[index1].GetComponent<HexDatabaseFields>().intrinsicHexValue < defendingHexes[index2].GetComponent<HexDatabaseFields>().intrinsicHexValue)
                    {
                        tempHex = defendingHexes[index1];
                        defendingHexes[index1] = defendingHexes[index2];
                        defendingHexes[index2] = tempHex;
                    }
        }

        /// <summary>
        /// This routine determines what attacks should be attempted.
        /// </summary>
        /// <param name="attackingNationality"></param>
        public static void CheckForAICombat(GlobalDefinitions.Nationality attackingNationality, List<GameObject> defendingHexes, List<GameObject> defendingUnitsOnBoard)
        {
            // Evaluate each hex.  These have been sorted from the most to least valuable
            bool defendersNonCommitted = false;
            int targetOdds = GlobalDefinitions.maximumAIOdds;
            int minimumOdds = GlobalDefinitions.minimumAIOdds;

            //#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: number of turns without successfulcombat = " + GlobalDefinitions.numberOfTurnsWithoutSuccessfulAttack);
            //#endif
            // Germans don't need to worry about attacking
            if (attackingNationality == GlobalDefinitions.Nationality.Allied)
                switch (GlobalDefinitions.numberOfTurnsWithoutSuccessfulAttack)
                {
                    case 0:
                        targetOdds = GlobalDefinitions.maximumAIOdds;
                        minimumOdds = GlobalDefinitions.minimumAIOdds;
                        break;
                    case 1:
                        targetOdds = 1;
                        minimumOdds = 1;
                        break;
                    case 2:
                        targetOdds = 1;
                        minimumOdds = -2;
                        break;
                    case 3:
                        targetOdds = 1;
                        // Anything less than 1:2 odds has no chance of victory so save these attacks for after the second invasion
                        if (GlobalDefinitions.turnNumber > 9)
                            minimumOdds = -3;
                        else
                            minimumOdds = -2;
                        break;
                    case 4:
                        targetOdds = -1;
                        // Anything less than 1:2 odds has no chance of victory so save these attacks for after the second invasion
                        if (GlobalDefinitions.turnNumber > 9)
                            minimumOdds = -4;
                        else
                            minimumOdds = -2;
                        break;
                    case 5:
                        targetOdds = -2;
                        // Anything less than 1:2 odds has no chance of victory so save these attacks for after the second invasion
                        if (GlobalDefinitions.turnNumber > 9)
                            minimumOdds = -5;
                        else
                            minimumOdds = -2;
                        break;
                    case 6:
                        targetOdds = -3;
                        // Anything less than 1:2 odds has no chance of victory so save these attacks for after the second invasion
                        if (GlobalDefinitions.turnNumber > 9)
                            minimumOdds = -6;
                        else
                            minimumOdds = -2;
                        break;
                    default:
                        targetOdds = 3;
                        // Anything less than 1:2 odds has no chance of victory so save these attacks for after the second invasion
                        if (GlobalDefinitions.turnNumber > 9)
                            minimumOdds = -6;
                        else
                            minimumOdds = -2;
                        break;
                }

            // The first three turns for the Allied side is critical so make sure the odds are minimal
            if ((GlobalDefinitions.turnNumber < 4) && (attackingNationality == GlobalDefinitions.Nationality.Allied))
            {
                targetOdds = 1;
                minimumOdds = 1;
            }

            foreach (GameObject defendingHex in defendingHexes)
            {
                // See if there are units on the defending hex that haven't been attacked.
                defendersNonCommitted = false;
                foreach (GameObject defendingUnit in defendingHex.GetComponent<HexDatabaseFields>().occupyingUnit)
                    if (!defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                        defendersNonCommitted = true;

                if (defendersNonCommitted)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("checkForAICombat: Checking attacks against defending hex " + defendingHex.name);
#endif
                    MakeAllCombatAssignments(defendingHex, attackingNationality, targetOdds, minimumOdds);
                }
            }
        }

        /// <summary>
        /// Adds unit passed to the attack passed and returns true if odds are met
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        private static bool MakeAnAttack(AIPotentialAttack attack, GameObject unit, int targetOdds)
        {
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
            attack.attackingUnits.Add(unit);
            // NEED FIX - the false being passed in the if statement below is for attack air support, need to deal with this for Allies
            attack.odds = CalculateBattleOddsRoutines.ReturnCombatOdds(attack.defendingUnits, attack.attackingUnits, false);
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeAnAttack:         checking attacker " + unit.name + " attack odds = " + attack.odds + "  target odds = " + targetOdds);
#endif
            if (attack.odds >= targetOdds)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeAnAttack:         attack meets odds  odds = " + attack.odds + "  target odds = " + targetOdds);
#endif
                return (true);
            }
            else
                return (false);
        }

        /// <summary>
        /// Determines whether the attack on the hex can take place.  Units are moved to attack if odds are met.
        /// It factors in all spawned attacks that could result from this one attack
        /// </summary>
        /// <param name="defendingHex"></param>
        /// <param name="attackingNationality"></param>
        /// <param name="targetOdds"></param>
        /// <param name="minimumOdds"></param>
        private static void MakeAllCombatAssignments(GameObject defendingHex, GlobalDefinitions.Nationality attackingNationality, int targetOdds, int minimumOdds)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: initiating");
#endif
            List<AIPotentialAttack> listPotentialAttacks = new List<AIPotentialAttack>();
            List<GameObject> listOfHexesToBeAttacked = new List<GameObject>();
            List<GameObject> listOfNewAttackHexesAdded = new List<GameObject>();
            bool oddsMet = false;
            bool hexBeingAttacked = false;

            // We will add hexes to the list and continue processing until all of the hexes in the list have been moved to potential attacks
            listOfHexesToBeAttacked.Add(defendingHex);
            while (listOfHexesToBeAttacked.Count > 0)
            {
                hexBeingAttacked = false;
                oddsMet = false;
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: checking hex = " + defendingHex.name + " hex being evaluated = " + listOfHexesToBeAttacked[0].name);
#endif
                // Create a new potential attack structure
                AIPotentialAttack newPotentialAttack = new AIPotentialAttack();
                AIDefendHex newAIDefendingHex = new AIDefendHex();
                LoadHexesAndUnitsForPotentialAttack(listOfHexesToBeAttacked[0], newAIDefendingHex, attackingNationality);
                newPotentialAttack.defendingHexes.Add(newAIDefendingHex);

                // Load up the defending structure with all of the units on the current defending hex
                foreach (GameObject defendingUnit in listOfHexesToBeAttacked[0].GetComponent<HexDatabaseFields>().occupyingUnit)
                {
                    defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
                    newPotentialAttack.defendingUnits.Add(defendingUnit);
                }

#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: checking single attack hexes  defendingHexes.Count = " + newPotentialAttack.defendingHexes.Count);
#endif
                // First we will add an attacker one by one until we either get to the maximum odds or we run out of hexes or units
                if (newPotentialAttack.defendingHexes.Count > 0)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: checking defending hex = " + newPotentialAttack.defendingHexes[0].defendingHex.name + " single attack hex count = " + newPotentialAttack.defendingHexes[0].singleAttackHexes.Count);
#endif
                    // Note that newPotentialAttack.defendingHexes[0] is the original defending hex
                    foreach (AISingleAttackHex singleAttackHex in newPotentialAttack.defendingHexes[0].singleAttackHexes)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:     checking single attack hex = " + singleAttackHex.attackHex.name);

                    // The reinforcement possibilities are landed first.  They have less movement options that units already on the board so if I don't 
                    // move them first the units already on the board may use hexes that the reinforcements need but that the on board units don't.
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:              checking reinforcement - oddsMet = " + oddsMet + " under stacking limit = " + GlobalDefinitions.HexUnderStackingLimit(singleAttackHex.attackHex, GlobalDefinitions.Nationality.Allied) + " Attacking nationality = " + attackingNationality);
#endif
                        if (!oddsMet && (attackingNationality == GlobalDefinitions.Nationality.Allied) && GlobalDefinitions.HexUnderStackingLimit(singleAttackHex.attackHex, GlobalDefinitions.Nationality.Allied))
                        {
                            GameObject reinforcementUnit = ReturnReinforcementUnitForAttackHex(singleAttackHex.attackHex);
                            if (reinforcementUnit != null)
                            {
                                hexBeingAttacked = true;
                                oddsMet = MakeAnAttack(newPotentialAttack, reinforcementUnit, targetOdds);
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:              adding reinforcement " + reinforcementUnit.name + " to the attack  odds met = " + oddsMet);
#endif
                            }

                            // Check for another reinforcement unit if the odds aren't met and there is still room on the hex
                            if (!oddsMet && GlobalDefinitions.HexUnderStackingLimit(singleAttackHex.attackHex, GlobalDefinitions.Nationality.Allied))
                            {
                                reinforcementUnit = ReturnReinforcementUnitForAttackHex(singleAttackHex.attackHex);
                                if (reinforcementUnit != null)
                                {
                                    hexBeingAttacked = true;
                                    oddsMet = MakeAnAttack(newPotentialAttack, reinforcementUnit, targetOdds);
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:              adding another reinforcement " + reinforcementUnit.name + " to the attack  odds met = " + oddsMet);
#endif
                                }
                            }
                        }

                        // We have checked the current single attack hex with reinforcements.  
                        // If odds aren't met check units on the board already
                        if (!oddsMet)
                            foreach (GameObject attackingUnit in singleAttackHex.potentialAttackers)
                                // Check to see if the unit is available
                                if (!oddsMet && !attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack &&
                                    (singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().occupyingUnit.Contains(attackingUnit) ||
                                    GlobalDefinitions.HexUnderStackingLimit(singleAttackHex.attackHex, attackingUnit.GetComponent<UnitDatabaseFields>().nationality)))
                                {
                                    // The unit and the hex are available so move the unit to the hex if it isn't already on the hex
                                    if (attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex != singleAttackHex.attackHex)
                                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(singleAttackHex.attackHex, attackingUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex, attackingUnit);

                                    hexBeingAttacked = true;
                                    oddsMet = MakeAnAttack(newPotentialAttack, attackingUnit, targetOdds);
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:              adding " + attackingUnit.name + " to the attack  odds met = " + oddsMet);
#endif
                                }
                    }
                }

                // If the odds haven't been met check if it meets minumum odds before adding in other hexes
                if (!oddsMet)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: combat did not meet odds = " + newPotentialAttack.odds + " minumum AI odds = " + GlobalDefinitions.minimumAIOdds + " passed minimum odd = " + minimumOdds);
#endif
                    if ((newPotentialAttack.odds != 0) && (newPotentialAttack.odds >= minimumOdds))
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: combat met minimum odds = " + newPotentialAttack.odds + " minumum passed odds = " + minimumOdds);
#endif
                        oddsMet = true;
                    }
                    // This executes because the attack was not able to make it to the maximum odds.  Determine if the lower odds are acceptable for the target attack
                    //if ((newPotentialAttack.odds >= (GlobalDefinitions.minimumAIOdds)) && (listOfHexesToBeAttacked[0].GetComponent<HexDatabaseFields>().intrinsicHexValue >= 5))
                    //    oddsMet = true;
                    //else if ((newPotentialAttack.odds >= GlobalDefinitions.minimumAIOdds) && (listOfHexesToBeAttacked[0].GetComponent<HexDatabaseFields>().intrinsicHexValue >= 10))
                    //    oddsMet = true;
                }

                // The single attack didn't work so now start adding more units that would cause additional hexes to be brought in
                if (!oddsMet && (newPotentialAttack.defendingHexes.Count > 0))
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:         checking multiple attack hexes - multiple attack hex count = " + newPotentialAttack.defendingHexes[0].multipleAttackHexes.Count);
#endif
                    // Note that newPotentialAttack.defendingHexes[0] is the original defending hex
                    foreach (AIMultipleAttackHex multipleAttackHex in newPotentialAttack.defendingHexes[0].multipleAttackHexes)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:             checking hex " + multipleAttackHex.attackHex.name);
                    // The reinforcement possibilities are landed first.  They have less movement options than units already on the board so if I don't 
                    // move them first the units already on the board may use hexes that the reinforcements need but that the on board units don't.
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:              checking reinforcement - oddsMet = " + oddsMet + " under stacking limit = " + GlobalDefinitions.HexUnderStackingLimit(multipleAttackHex.attackHex, GlobalDefinitions.Nationality.Allied) + " Attacking nationality = " + attackingNationality);
#endif
                        if (!oddsMet && (attackingNationality == GlobalDefinitions.Nationality.Allied) && GlobalDefinitions.HexUnderStackingLimit(multipleAttackHex.attackHex, GlobalDefinitions.Nationality.Allied))
                        {
                            GameObject reinforcementUnit = ReturnReinforcementUnitForAttackHex(multipleAttackHex.attackHex);
                            if (reinforcementUnit != null)
                            {
                                hexBeingAttacked = true;
                                oddsMet = MakeAnAttack(newPotentialAttack, reinforcementUnit, targetOdds);
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:              adding reinforcement " + reinforcementUnit.name + " to the attack  odds met = " + oddsMet);
#endif
                                // Note that the additional hexes that are brought in battle are not factored in the current battle.  They are pushed to the stack and will be dealt with separately.
                                foreach (GameObject hex in multipleAttackHex.additionalDefendingHexes)
                                    if (!listOfHexesToBeAttacked.Contains(hex))
                                    {
#if OUTPUTDEBUG
                                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:             additional defending hex being added " + hex.name);
#endif
                                        listOfHexesToBeAttacked.Add(hex);
                                        // Store these hexes in case the attack is called off
                                        listOfNewAttackHexesAdded.Add(hex);
                                    }
                            }

                            // Check for another reinforcement unit if the odds aren't met and there is still room on the hex
                            if (!oddsMet && GlobalDefinitions.HexUnderStackingLimit(multipleAttackHex.attackHex, GlobalDefinitions.Nationality.Allied))
                            {
                                reinforcementUnit = ReturnReinforcementUnitForAttackHex(multipleAttackHex.attackHex);
                                if (reinforcementUnit != null)
                                {
                                    hexBeingAttacked = true;
                                    oddsMet = MakeAnAttack(newPotentialAttack, reinforcementUnit, targetOdds);
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:              adding another reinforcement " + reinforcementUnit.name + " to the attack  odds met = " + oddsMet);
#endif
                                }
                            }
                        }

                        // Reinforcements have been checked for the hex
                        // If the odds still aren't met and there is room on the hex check for units that are already on the board
                        if (!oddsMet)
                            foreach (GameObject attackingUnit in multipleAttackHex.potentialAttackers)
                                // Check to see if the hex is at the stacking limit
                                if (!oddsMet && !attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack &&
                                        (multipleAttackHex.attackHex.GetComponent<HexDatabaseFields>().occupyingUnit.Contains(attackingUnit) ||
                                        GlobalDefinitions.HexUnderStackingLimit(multipleAttackHex.attackHex, attackingUnit.GetComponent<UnitDatabaseFields>().nationality)))
                                {
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:         checking attacker " + attackingUnit.name);
#endif
                                    // The unit and the hex are available so move the unit to the hex if it isn't already on the hex
                                    if (attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex != multipleAttackHex.attackHex)
                                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(multipleAttackHex.attackHex, attackingUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex, attackingUnit);

                                    hexBeingAttacked = true;
                                    oddsMet = MakeAnAttack(newPotentialAttack, attackingUnit, targetOdds);
                                    attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;

                                    // Note that the additional hexes that are brought in battle are not factored in the current battle.  They are pushed to the stack and will be dealt with separately.
                                    foreach (GameObject hex in multipleAttackHex.additionalDefendingHexes)
                                        if (!listOfHexesToBeAttacked.Contains(hex))
                                        {
#if OUTPUTDEBUG
                                        GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:             additional defending hex being added " + hex.name);
#endif
                                            listOfHexesToBeAttacked.Add(hex);
                                            // Store these hexes in case the attack is called off
                                            listOfNewAttackHexesAdded.Add(hex);
                                        }
                                }
                    }
                }

                // If the odds haven't been met, check if airborne can be added
                // Only use airborne units if there were no successful attacks last turn.  While not definitive it is a sign that the Allied units might be stalled.
                // Note that hexBeingAttacked is being checked to avoid having airborne units attack on their own.  While this doesn't violate rules, the AI doesn't
                // have a strategic sense to know what random hex should be attacked.
                if (attackingNationality == GlobalDefinitions.Nationality.Allied && !oddsMet && hexBeingAttacked && !GlobalDefinitions.SuccessfulAttacksLastTurn() &&
                        (GlobalDefinitions.currentAirborneDropsThisTurn < GlobalDefinitions.maxNumberAirborneDropsThisTurn))
                {
                    List<GameObject> airborneUnits = new List<GameObject>();
                    GameObject airborneUnit;
                    foreach (AISingleAttackHex singleAttackHex in newPotentialAttack.defendingHexes[0].singleAttackHexes)
                        if (!oddsMet && GlobalDefinitions.HexUnderStackingLimit(singleAttackHex.attackHex, GlobalDefinitions.Nationality.Allied) &&
                                (GlobalDefinitions.maxNumberAirborneDropsThisTurn > 0) && (GlobalDefinitions.currentAirborneDropsThisTurn < GlobalDefinitions.maxNumberAirborneDropsThisTurn))
                        {
                            airborneUnit = ReturnAirborneFromBritain(airborneUnits);
                            if (airborneUnit != null)
                            {
                                airborneUnits.Add(airborneUnit);

                                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitFromBritain(singleAttackHex.attackHex, airborneUnit);
                                singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().alliedControl = true;
                                airborneUnit.GetComponent<UnitDatabaseFields>().remainingMovement = 0;

                                oddsMet = MakeAnAttack(newPotentialAttack, airborneUnit, targetOdds);
                                GlobalDefinitions.currentAirborneDropsThisTurn++;

                                if (!oddsMet && GlobalDefinitions.HexUnderStackingLimit(singleAttackHex.attackHex, GlobalDefinitions.Nationality.Allied) &&
                                        (GlobalDefinitions.currentAirborneDropsThisTurn < GlobalDefinitions.maxNumberAirborneDropsThisTurn))
                                {
                                    airborneUnit = ReturnAirborneFromBritain(airborneUnits);
                                    if (airborneUnit != null)
                                    {
                                        airborneUnits.Add(airborneUnit);

                                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitFromBritain(singleAttackHex.attackHex, airborneUnit);
                                        singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().alliedControl = true;
                                        airborneUnit.GetComponent<UnitDatabaseFields>().remainingMovement = 0;

                                        oddsMet = MakeAnAttack(newPotentialAttack, airborneUnit, targetOdds);
                                        GlobalDefinitions.currentAirborneDropsThisTurn++;
                                    }
                                }
                            }
                        }

                    //  Not going to add additional hexes to attack for airborne attacks...
                }

                if (!oddsMet)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: combat did not meet odds = " + newPotentialAttack.odds + " minumum AI odds = " + GlobalDefinitions.minimumAIOdds);
#endif
                    if ((newPotentialAttack.odds != 0) && (newPotentialAttack.odds >= minimumOdds))
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: combat met odds = " + newPotentialAttack.odds + " minumum AI odds = " + GlobalDefinitions.minimumAIOdds);
#endif
                        oddsMet = true;
                    }
                }

                if (!oddsMet)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:         calling off attack cannot meet the odds");
#endif
                    // If we get here and the odds still aren't met then all the attacks need to be called off
                    foreach (GameObject attacker in newPotentialAttack.attackingUnits)
                    {
                        if (attacker.GetComponent<UnitDatabaseFields>().airborne && (attacker.GetComponent<UnitDatabaseFields>().beginningTurnHex == null))
                        {
                            GlobalDefinitions.currentAirborneDropsThisTurn--;
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(attacker.GetComponent<UnitDatabaseFields>().occupiedHex, attacker, false);
                        }
                        else if (attacker.GetComponent<UnitDatabaseFields>().beginningTurnHex == null)
                        {
                            GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().DecrementInvasionUnitLimits(attacker);
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(attacker.GetComponent<UnitDatabaseFields>().occupiedHex, attacker, false);
                        }
                        else
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(attacker.GetComponent<UnitDatabaseFields>().beginningTurnHex, attacker.GetComponent<UnitDatabaseFields>().occupiedHex, attacker);
                        if (attacker.GetComponent<UnitDatabaseFields>().inSupply)
                            attacker.GetComponent<UnitDatabaseFields>().remainingMovement = attacker.GetComponent<UnitDatabaseFields>().movementFactor;
                        else
                            attacker.GetComponent<UnitDatabaseFields>().remainingMovement = 1;
                        attacker.GetComponent<UnitDatabaseFields>().hasMoved = false;
                        attacker.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                    }
                    foreach (GameObject defender in newPotentialAttack.defendingUnits)
                        defender.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;

                    // Now go back and remove the units from the attacks that met the odds
                    foreach (AIPotentialAttack attack in listPotentialAttacks)
                    {
                        foreach (GameObject attacker in attack.attackingUnits)
                        {
                            if (attacker.GetComponent<UnitDatabaseFields>().airborne && (attacker.GetComponent<UnitDatabaseFields>().beginningTurnHex == null))
                            {
                                GlobalDefinitions.currentAirborneDropsThisTurn--;
                                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(attacker.GetComponent<UnitDatabaseFields>().occupiedHex, attacker, false);
                            }
                            else if (attacker.GetComponent<UnitDatabaseFields>().beginningTurnHex == null)
                            {
                                GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().DecrementInvasionUnitLimits(attacker);
                                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(attacker.GetComponent<UnitDatabaseFields>().occupiedHex, attacker, false);
#if OUTPUTDEBUG
                            //GlobalDefinitions.WriteToLogFile("hexAvailableForUnitTypeReinforcements: invasion area index = " + attacker.GetComponent<UnitDatabaseFields>().invasionAreaIndex + " invasion turn = " + GlobalDefinitions.invasionAreas[attacker.GetComponent<UnitDatabaseFields>().invasionAreaIndex].turn + " armor units used this turn = " + GlobalDefinitions.invasionAreas[attacker.GetComponent<UnitDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn + " infantry units used this turn = " + GlobalDefinitions.invasionAreas[attacker.GetComponent<UnitDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn);
#endif
                            }
                            else
                                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(attacker.GetComponent<UnitDatabaseFields>().beginningTurnHex, attacker.GetComponent<UnitDatabaseFields>().occupiedHex, attacker);

                            if (attacker.GetComponent<UnitDatabaseFields>().inSupply)
                                attacker.GetComponent<UnitDatabaseFields>().remainingMovement = attacker.GetComponent<UnitDatabaseFields>().movementFactor;
                            else
                                attacker.GetComponent<UnitDatabaseFields>().remainingMovement = 1;
                            attacker.GetComponent<UnitDatabaseFields>().hasMoved = false;
                            attacker.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                        }
                        foreach (GameObject defender in attack.defendingUnits)
                            defender.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                    }

                    listPotentialAttacks.Clear();

                    // Remove all the hexes that were added for this hex
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:             Removing hexes due to canceled attack:");
#endif
                    foreach (GameObject hex in listOfNewAttackHexesAdded)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:                 " + hex.name);
#endif
                        listOfHexesToBeAttacked.Remove(hex);
                    }
                }
                else
                {
                    // The attack has met the mimum needed odds
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:             Adding attack to listPotentialAttacks defending hex count = " + newPotentialAttack.defendingHexes.Count);
#endif
                    if (newPotentialAttack.defendingHexes.Count > 0)
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:             Adding attack to listPotentialAttacks defending hex = " + newPotentialAttack.defendingHexes[0].defendingHex.name);
#endif
                        listPotentialAttacks.Add(newPotentialAttack);
                }

                // It is possible for the cancel of the attack to remove all the hexes so check this before trying to remove anything
                if (listOfHexesToBeAttacked.Count > 0)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: Done evaluating hex " + listOfHexesToBeAttacked[0].name + " Count = " + listOfHexesToBeAttacked.Count);
#endif
                    listOfHexesToBeAttacked.RemoveAt(0);
                }
            }

            // All attacks related to the passed initial hex attack have been evaluated.  Move all the stored attacks to the GlobalDefinition variables
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: Loading all attacks - attack number = " + listPotentialAttacks.Count + " global combat count = " + GlobalDefinitions.allCombats.Count);
#endif
            foreach (AIPotentialAttack newAttack in listPotentialAttacks)
            {
                GameObject singleCombat = new GameObject("singleConbat");
                singleCombat.AddComponent<Combat>();

#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:         Adding Attack");
#endif
                foreach (GameObject unit in newAttack.defendingUnits)
                    if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:             Defender " + unit.name);
#endif
                        singleCombat.GetComponent<Combat>().defendingUnits.Add(unit);
                    }

                foreach (GameObject unit in newAttack.attackingUnits)
                    if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments:             Attacker " + unit.name);
#endif
                        singleCombat.GetComponent<Combat>().attackingUnits.Add(unit);
                    }
                GlobalDefinitions.allCombats.Add(singleCombat);
            }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeAllCombatAssignments: exiting");
#endif
        }

        /// <summary>
        /// Returns a reinforcement unit if available for the hex passed
        /// </summary>
        /// <param name="attackHex"></param>
        /// <returns></returns>
        private static GameObject ReturnReinforcementUnitForAttackHex(GameObject attackHex)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnReinforcementUnit: executing for attack hex = " + attackHex.name);
        GlobalDefinitions.WriteToLogFile("returnReinforcementUnit:      number of reinforcement hexes = " + GlobalDefinitions.availableReinforcementPorts.Count);
        foreach (GameObject hex in GlobalDefinitions.availableReinforcementPorts)
            GlobalDefinitions.WriteToLogFile("returnReinforcementUnit:              " + hex.name);
#endif
            // Get the list of hexes that are up to four hexes away, this will be within distance of a unit to land and move to the hex
            List<GameObject> hexList = ReturnHexesWithinDistance(attackHex, (GlobalDefinitions.attackRange - 1));
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnReinforcementUnit:  found " + hexList.Count + " hexes to check for ports");
#endif
            // Check if the list contains a landing hex
            foreach (GameObject hex in hexList)
            {
                if (GlobalDefinitions.availableReinforcementPorts.Contains(hex))
                {
                    bool reinforcementAvailable;
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnReinforcementUnit: found a port = " + hex.name);
#endif
                    // hex contains a reinforcement port within range

                    // Check if an armor unit can be landed
                    GameObject reinforcementUnit = ReturnAvailableArmorUnit();

                    if ((reinforcementUnit != null) &&
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnReinforcementLandingHexes(reinforcementUnit).Contains(hex))
                        reinforcementAvailable = true;
                    else
                        reinforcementAvailable = false;

                    // If an armor unit isn't available check for an infantry unit
                    if (!reinforcementAvailable)
                    {
                        reinforcementUnit = ReturnAvailableInfantryUnit();
                        if ((reinforcementUnit != null) &&
                                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnReinforcementLandingHexes(reinforcementUnit).Contains(hex))
                            reinforcementAvailable = true;
                        else
                            reinforcementAvailable = false;
                    }

                    if (reinforcementAvailable)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("returnReinforcementUnit: reinforcement unit available = " + reinforcementUnit.name);
#endif
                        // Check if the unit can be landed at the port

                        hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(reinforcementUnit, hex, false);
                        reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(hex, reinforcementUnit);
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("returnReinforcementUnit: available movement hexes:");
                    foreach (GameObject tempHex in reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                        GlobalDefinitions.WriteToLogFile("returnReinforcementUnit:          " + tempHex.name);
#endif
                        // The loadAlliedUnitFromOffBoard will have loaded available movement hexes
                        if (reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Contains(attackHex))
                        {
                            // The unit can move to the attack hex so move it
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(attackHex, hex, reinforcementUnit);
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("returnReinforcementUnit: unit " + reinforcementUnit.name + " is available returning the unit");
#endif
                            return (reinforcementUnit);
                        }
                        else
                        {
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("returnReinforcementUnit: moving unit " + reinforcementUnit.name + " back to Britain");
#endif
                            reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Clear();
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("returnReinforcementUnit: invasion area index = " + hex.GetComponent<HexDatabaseFields>().invasionAreaIndex + " invasion turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn + " armor units used this turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn + " infantry units used this turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn);
#endif
                            GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().DecrementInvasionUnitLimits(reinforcementUnit);
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(hex, reinforcementUnit, false);
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("returnReinforcementUnit: invasion area index = " + hex.GetComponent<HexDatabaseFields>().invasionAreaIndex + " invasion turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].turn + " armor units used this turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].armorUnitsUsedThisTurn + " infantry units used this turn = " + GlobalDefinitions.invasionAreas[hex.GetComponent<HexDatabaseFields>().invasionAreaIndex].infantryUnitsUsedThisTurn);
#endif
                        }
                    }
                }
            }
            return (null);
        }

        /// <summary>
        /// Loads hexes and units for the potential attacks related to the attack on the passed hex
        /// </summary>
        /// <param name="defendingHex"></param>
        private static void LoadHexesAndUnitsForPotentialAttack(GameObject defendingHex, AIDefendHex aiDefendHex, GlobalDefinitions.Nationality attackingNationality)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("loadHexesAndUnitsForPotentialAttack:        executing with parameters defending hex = " + defendingHex.name);
#endif
            // The assumption in this routine is that there is at least one defending unit on the hex passed
            List<GameObject> attackHexes = new List<GameObject>();

            aiDefendHex.defendingHex = defendingHex;
            attackHexes = DetermineAttackHexesForTargetDefendingHex(defendingHex, attackingNationality);
            aiDefendHex.singleAttackHexes = ReturnSingleAttackHexes(defendingHex, attackHexes, attackingNationality);
            aiDefendHex.multipleAttackHexes = ReturnMultipleAttackHexes(defendingHex, attackHexes, attackingNationality);
            LoadPotentialAttackingUnits(defendingHex, attackingNationality, aiDefendHex.singleAttackHexes);
            LoadPotentialAttackingUnits(defendingHex, attackingNationality, aiDefendHex.multipleAttackHexes);
        }

        /// <summary>
        /// Returns a list of hexes that are adjacent to the hex and able to attack
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="attackingNationality"></param>
        /// <returns></returns>
        private static List<GameObject> DetermineAttackHexesForTargetDefendingHex(GameObject hex, GlobalDefinitions.Nationality attackingNationality)
        {
            List<GameObject> openAttackHexes = new List<GameObject>();

            // Check for empty adjacent hexes
            foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null) &&
                        !hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().sea &&
                        !hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().impassible &&
                        !hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().bridge &&
                        (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0))
                    // Hex is open so it's available
                    openAttackHexes.Add(hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);

                else if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null) &&
                        (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                        !hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().bridge &&
                        (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == attackingNationality) &&
                        !GlobalDefinitions.StackingLimitExceeded(hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide],
                        hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality))
                    // Hex has units but they are attacking units so add the hex
                    openAttackHexes.Add(hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);

            return (openAttackHexes);
        }

        /// <summary>
        /// Loads the list of units that can attack the defending unit from the passed list of attack hexes
        /// </summary>
        /// <param name="defendingHex"></param>
        /// <param name="attackingNationality"></param>
        /// <param name="attackingHexes"></param>
        private static void LoadPotentialAttackingUnits(GameObject defendingHex, GlobalDefinitions.Nationality attackingNationality, List<AISingleAttackHex> attackingHexes)
        {
            // Get all the units within five hexes and then check this list since the max any unit can move and still attack is four hexes
            List<GameObject> potentialAttackUnits = new List<GameObject>();

            potentialAttackUnits = FindNearbyEnemyUnits(defendingHex, defendingHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality, GlobalDefinitions.attackRange);

            // Go through all of the potential attackers and see if the attacking hexes are contained in their available movement
            // If so then they are a potential attack unit
            foreach (GameObject tempUnit in potentialAttackUnits)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("loadPotentialAttackingUnits:              single attack hex evaluating unit " + tempUnit.name + " is committed to attack " + tempUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack);
#endif
                if (!tempUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack && !tempUnit.GetComponent<UnitDatabaseFields>().hasMoved &&
                        !tempUnit.GetComponent<UnitDatabaseFields>().HQ)
                {
                    tempUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(tempUnit.GetComponent<UnitDatabaseFields>().occupiedHex, tempUnit);
                    foreach (AISingleAttackHex tempHex in attackingHexes)
                        if (tempUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Contains(tempHex.attackHex))
                        {
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("loadPotentialAttackingUnits:              single attack hex adding unit " + tempUnit.name + " to hex " + tempHex.attackHex.name);
#endif
                            tempHex.potentialAttackers.Add(tempUnit);
                        }
                }
            }

            foreach (AISingleAttackHex tempAttackHex in attackingHexes)
            {
                // Sort the units in the list from highest attack factor to lowest
                tempAttackHex.potentialAttackers.Sort((b, a) => a.GetComponent<UnitDatabaseFields>().attackFactor.CompareTo(b.GetComponent<UnitDatabaseFields>().attackFactor));
            }

        }

        /// <summary>
        /// Loads the list of units that can attack the defending unit from the passed list of attack hexes 
        /// </summary>
        /// <param name="defendingHex"></param>
        /// <param name="attackingNationality"></param>
        /// <param name="attackingHexes"></param>
        private static void LoadPotentialAttackingUnits(GameObject defendingHex, GlobalDefinitions.Nationality attackingNationality, List<AIMultipleAttackHex> attackingHexes)
        {
            // I am not going to go through all of the attacking units.  I think it would be quicker to get all the units within
            // five hexes and then check this list since the max any unit can move and still attack is four hexes
            List<GameObject> potentialAttackUnits = new List<GameObject>();

            // Go through all of the potential attackers and see if the attacking hexes are contained in their available movement
            // If so then they are a potential attack unit
            potentialAttackUnits = FindNearbyEnemyUnits(defendingHex, defendingHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality, GlobalDefinitions.attackRange);
            foreach (GameObject tempUnit in potentialAttackUnits)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("loadPotentialAttackingUnits:              multiple attack hex evaluating unit " + tempUnit.name + " is committed to attack " + tempUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack);
#endif
                if (!tempUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack && !tempUnit.GetComponent<UnitDatabaseFields>().hasMoved &&
                !tempUnit.GetComponent<UnitDatabaseFields>().HQ)
                {
                    tempUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(tempUnit.GetComponent<UnitDatabaseFields>().occupiedHex, tempUnit);
                    foreach (AIMultipleAttackHex tempHex in attackingHexes)
                        if (tempUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Contains(tempHex.attackHex))
                        {
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("loadPotentialAttackingUnits:              multiple attack hex adding unit " + tempUnit.name + " to hex " + tempHex.attackHex.name);
#endif
                            tempHex.potentialAttackers.Add(tempUnit);
                        }
                }
            }
        }

        /// <summary>
        /// Takes the hex passed and returns hexes that can be used to attack the defending hex without including other hexes
        /// </summary>
        /// <param name="defendingHex"></param>
        /// <param name="attackHexes"></param>
        /// <param name="attackingNationality"></param>
        /// <returns></returns>
        private static List<AISingleAttackHex> ReturnSingleAttackHexes(GameObject defendingHex, List<GameObject> attackHexes, GlobalDefinitions.Nationality attackingNationality)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnSingleAttackHexes: executing for defending hex " + defendingHex.name);
#endif
            List<GameObject> singleAttackHexes = new List<GameObject>();
            List<AISingleAttackHex> returnList = new List<AISingleAttackHex>();
            bool hexIncludesAdditionalDefenders = false;
            foreach (GameObject attackHex in attackHexes)
            {
                hexIncludesAdditionalDefenders = false;
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnSingleAttackHexes: checking for attack hex " + attackHex.name);
#endif
                // Go through and see if there are other units projecting ZOC into the attackHex which would prompt another attack
                foreach (GameObject unit in attackHex.GetComponent<HexDatabaseFields>().unitsExertingZOC)
                {
                    if ((unit.GetComponent<UnitDatabaseFields>().occupiedHex != defendingHex)
                            && (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.ReturnOppositeNationality(attackingNationality)))
                        hexIncludesAdditionalDefenders = true;
                }

                // Need to check if there is a river
                if (!hexIncludesAdditionalDefenders && GeneralHexRoutines.CheckForRiverBetweenTwoHexes(attackHex, defendingHex))
                {
                    // Since this would be a cross river attack check to see if it would add additional defenders
                    // We need to go through all the additional potential defenders and make sure they are not all already being attacked
                    foreach (GameObject hex in ReturnAdditionalDefendersForCrossRiverAttack(attackHex, defendingHex, attackingNationality))
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("returnSingleAttackHexes:          checking single hex attack for river adjacent hex " + hex.name);
#endif
                        foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().occupyingUnit)
                            if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnSingleAttackHexes:              hex would bring additional hexes");
#endif
                                hexIncludesAdditionalDefenders = true;
                            }
                    }
                }
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnSingleAttackHexes: check complete for attack hex " + attackHex.name + " hexIncludesAdditionalDefenders = " + hexIncludesAdditionalDefenders);
#endif
                if (!hexIncludesAdditionalDefenders && !singleAttackHexes.Contains(attackHex))
                    singleAttackHexes.Add(attackHex);
            }

            // Sort the hexes so that hexes that don't have rivers between the defending hex and the attack hex come first.  When assigning combat 
            // they are tested in order so we want to make sure that the hexes that do not attack across a river are done first.
            List<GameObject> sortedSingleAttackHexes = new List<GameObject>();
            foreach (GameObject hex in singleAttackHexes)
                if (!GeneralHexRoutines.CheckForRiverBetweenTwoHexes(hex, defendingHex))
                    sortedSingleAttackHexes.Add(hex);
            foreach (GameObject hex in singleAttackHexes)
                if (!sortedSingleAttackHexes.Contains(hex))
                    sortedSingleAttackHexes.Add(hex);

            // Add the single attack hexes to the return list
            foreach (GameObject hex in sortedSingleAttackHexes)
            {
                AISingleAttackHex newAttackHex = new AISingleAttackHex
                {
                    attackHex = hex
                };
                returnList.Add(newAttackHex);
            }

            return (returnList);
        }

        /// <summary>
        /// Takes the hex passed and returns hexes that can be used to attack the defending hex but bring in other defenders
        /// </summary>
        /// <param name="defendingHex"></param>
        /// <param name="attackHexes"></param>
        /// <param name="attackingNationality"></param>
        /// <returns></returns>
        private static List<AIMultipleAttackHex> ReturnMultipleAttackHexes(GameObject defendingHex, List<GameObject> attackHexes, GlobalDefinitions.Nationality attackingNationality)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnMultipleAttackHexes:    defending hex = " + defendingHex.name);
#endif
            List<GameObject> multipleAttackHexes = new List<GameObject>();
            List<AIMultipleAttackHex> returnList = new List<AIMultipleAttackHex>();
            AIMultipleAttackHex newAttackHex = new AIMultipleAttackHex();
            foreach (GameObject attackHex in attackHexes)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnMultipleAttackHexes:        checking attack hex = " + attackHex.name);
#endif
                // Go through and see if there are other enemy units projecting ZOC into the attackHex which would prompt another attack
                foreach (GameObject unit in attackHex.GetComponent<HexDatabaseFields>().unitsExertingZOC)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnMultipleAttackHexes:            hex exerting ZOC - " + unit.GetComponent<UnitDatabaseFields>().occupiedHex.name + " unit = " + unit.name);
#endif
                    // I was originally including a check if the attack hex was already in multipleAttackHexes I would skip the following code.  This caused a hex that brought in two 
                    // additional defending hexes from loading both.  Only one would be loaded.
                    if ((unit.GetComponent<UnitDatabaseFields>().occupiedHex != defendingHex) &&
                            (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.ReturnOppositeNationality(attackingNationality)) &&
                            !unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("returnMultipleAttackHexes:                    adding hex exerting ZOC - " + unit.GetComponent<UnitDatabaseFields>().occupiedHex.name + " unit = " + unit.name);
#endif
                        if (!multipleAttackHexes.Contains(attackHex))
                        {
                            multipleAttackHexes.Add(attackHex);
                            newAttackHex.attackHex = attackHex;
                            returnList.Add(newAttackHex);
                        }
                        if (!newAttackHex.additionalDefendingHexes.Contains(unit.GetComponent<UnitDatabaseFields>().occupiedHex))
                            newAttackHex.additionalDefendingHexes.Add(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                    }
                }

                // Need to check if there is a river
                if (GeneralHexRoutines.CheckForRiverBetweenTwoHexes(attackHex, defendingHex))
                    // Since this would be a cross river attack check to see if it would add additional defenders
                    foreach (GameObject newHex in ReturnAdditionalDefendersForCrossRiverAttack(attackHex, defendingHex, attackingNationality))
                        foreach (GameObject unit in newHex.GetComponent<HexDatabaseFields>().occupyingUnit)
                            if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                            {
                                if (!multipleAttackHexes.Contains(attackHex))
                                {
                                    multipleAttackHexes.Add(attackHex);
                                    newAttackHex.attackHex = attackHex;
                                    returnList.Add(newAttackHex);
                                }
                                if (!newAttackHex.additionalDefendingHexes.Contains(newHex))
                                    newAttackHex.additionalDefendingHexes.Add(newHex);
                            }
            }

            AIMultipleAttackHex tempStore;
            int index1Total, index2Total;
            // Sort the hexes in descending order of how many additional defense factors the hex brings into the attack
            // When assigning units to combat, by having it in this order it will make things easer
            // Note that this is sorting by defense factors scaled by the hex it is on, it isn't accounting for cross river attacks since I won't
            // know if rivers double defense until all the units are allocated
            for (int index1 = 0; index1 < returnList.Count; index1++)
            {
                index1Total = 0;
                foreach (GameObject hex in returnList[index1].additionalDefendingHexes)
                    foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().occupyingUnit)
                        index1Total += ReturnBaseDefenseFactor(unit);

                for (int index2 = 0; index2 < returnList.Count; index2++)
                {
                    index2Total = 0;
                    foreach (GameObject hex in returnList[index2].additionalDefendingHexes)
                        foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().occupyingUnit)
                            index2Total += ReturnBaseDefenseFactor(unit);


                    if (index2Total < index1Total)
                    {
                        // Need to update the index1Total with the index2Total since we're swapping
                        index1Total = index2Total;

                        tempStore = returnList[index1];
                        returnList[index1] = returnList[index2];
                        returnList[index2] = tempStore;
                    }
                }
            }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnMultipleAttackHexes:      returning " + returnList.Count + " multiple attack hexes for " + defendingHex.name);
#endif
            return (returnList);
        }

        /// <summary>
        /// Returns defense factor scaled by the occupied hex
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private static int ReturnBaseDefenseFactor(GameObject unit)
        {
            if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().city)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * GlobalDefinitions.defenseFactorScalingForCity);
            else if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().mountain)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * GlobalDefinitions.defenseFactorScalingForMountain);
            else if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortifiedZone)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * GlobalDefinitions.defenseFactorScalingForFortifiedZone);
            else if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * GlobalDefinitions.defenseFactorScalingForFortress);
            else
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor);
        }

        // This routine returns the hex modifier based on the distance to an enemy
        public static int ReturnDistanceToEnemyHexValueModifier(int distanceToEnemy)
        {
            // The only reason this routine should be called with a 1 distance is if the enemy is in a fortress 
            // or across a river so that it doesn't exert ZOX to the hex
            if (distanceToEnemy < GlobalDefinitions.enemyUnitModiferDistance)
                return (GlobalDefinitions.baseEnemyDistanceHexModifier - distanceToEnemy);

            // The default is no modification
            return (0);
        }

        /// <summary>
        /// Returns the additional hexes that would need to be attacked due to being in the ZOC of a defender being attacked cross river
        /// </summary>
        /// <param name="attackHex"></param>
        /// <param name="defendHex"></param>
        /// <param name="attackingNationality"></param>
        /// <returns></returns>
        private static List<GameObject> ReturnAdditionalDefendersForCrossRiverAttack(GameObject attackHex, GameObject defendHex, GlobalDefinitions.Nationality attackingNationality)
        {
            // The assumption here is that the calling code already checked for a river between the hexes passed
            List<GameObject> returnList = new List<GameObject>();
            // There is a river so check the neighbors of the defending hex and see if there are defending units in the ZOC of the defending hex and adjacent to the attack hex
            // If so this hex must also be attacked
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnAdditionalDefendersForCrossRiverAttack: checking attack hex = " + attackHex.name + "  defendHex = " + defendHex.name);
#endif
            foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
            {
                // Check that the neighbor hex for the defender exists
                if (defendHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                {
                    // Check if the defending hex is exerting ZOC to the neighbor
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnAdditionalDefendersForCrossRiverAttack:         checking hex = " + defendHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].name);
#endif
                    if (defendHex.GetComponent<BooleanArrayData>().exertsZOC[(int)hexSide])
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("returnAdditionalDefendersForCrossRiverAttack:         defender exerts ZOC");
#endif
                        // Check if there are defending units on the neighbor
                        if (defendHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                        {
                            if (defendHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.ReturnOppositeNationality(attackingNationality))
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("returnAdditionalDefendersForCrossRiverAttack:         neighbor has defenders");
#endif
                                // And finally check if there is a river between the attack hex and the neighbor hex
                                if (GeneralHexRoutines.CheckForRiverBetweenTwoHexes(attackHex, defendHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]))
                                {
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("returnAdditionalDefendersForCrossRiverAttack:         river between neighbor and attack hex");
#endif
                                    // Only add the hex is there is a uncommitted unit on the hex
                                    foreach (GameObject unit in defendHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit)
                                    {
                                        if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                                        {
#if OUTPUTDEBUG
                                        GlobalDefinitions.WriteToLogFile("returnAdditionalDefendersForCrossRiverAttack:         ADDING THE NEIGHBOR TO NEW DEFENDING HEX");
#endif
                                            returnList.Add(defendHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (returnList);
        }

        /// <summary>
        /// Selects the allied replacement units and places them in Britain
        /// </summary>
        public static void SelectAlliedAIReplacementUnits()
        {
            GameObject replacementUnit = new GameObject("selectAlliedAIReplacementUnits");
            List<GameObject> armorReplacements = new List<GameObject>();
            List<GameObject> infantryReplacements = new List<GameObject>();

            // Get the Allied units that have been eliminated but not HQ's
            foreach (Transform unit in GameObject.Find("Units Eliminated").transform)
                if ((unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
                {
                    if (unit.GetComponent<UnitDatabaseFields>().armor)
                        armorReplacements.Add(unit.gameObject);
                    else if (unit.GetComponent<UnitDatabaseFields>().infantry)
                        infantryReplacements.Add(unit.gameObject);
                }

            // The Allied replacement is pretty straight forward since all infantry and armor units are the same and they just get placed back in Britain
            while ((GlobalDefinitions.alliedReplacementsRemaining > 3) &&
                    (armorReplacements.Count > 0) && (infantryReplacements.Count > 0))
            {
                if ((GlobalDefinitions.alliedReplacementsRemaining > 4) && (armorReplacements.Count > 0))
                {
                    replacementUnit = armorReplacements[0];
                    armorReplacements.RemoveAt(0);
                }
                else if ((GlobalDefinitions.alliedReplacementsRemaining > 3) && (infantryReplacements.Count > 0))
                {
                    replacementUnit = infantryReplacements[0];
                    infantryReplacements.RemoveAt(0);
                }

                if (replacementUnit != null)
                {
                    GlobalDefinitions.alliedReplacementsRemaining -= replacementUnit.GetComponent<UnitDatabaseFields>().attackFactor;
                    replacementUnit.transform.position = replacementUnit.GetComponent<UnitDatabaseFields>().locationInBritain;
                    replacementUnit.GetComponent<UnitDatabaseFields>().unitEliminated = false;
                    replacementUnit.transform.parent = GameObject.Find("Units In Britain").transform;
                    replacementUnit.GetComponent<UnitDatabaseFields>().inBritain = true;
                }
            }
        }

        /// <summary>
        /// Places German replacement units on the board
        /// </summary>
        public static void GermanAIReplacementUnits()
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("germanAIReplacementUnits: starting German Replacement Remaining = " + GlobalDefinitions.germanReplacementsRemaining);
#endif
            List<GameObject> replacementHexes = new List<GameObject>();
            GameObject tempUnit;
            GameObject tempHex;

            List<GameObject> armorReplacements = new List<GameObject>();
            List<GameObject> airborneReplacements = new List<GameObject>();
            List<GameObject> infantryReplacements = new List<GameObject>();

            // Check for the condition where there are no Allied units on the board.  This means the German has won but that won't be decided until the end of combat.
            if (GlobalDefinitions.alliedUnitsOnBoard.Count == 0)
                return;

            // The target location will be used when determining which replacement hex to use
            targetLocation = GetAverageEnemyLocation(GlobalDefinitions.Nationality.Allied);

            // Get a list of the replacement hexes that aren't fully stacked, not in Allied ZOC (I don't want to have to figure an attack out here) and it isn't in Allied control
            foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                if (hex.GetComponent<HexDatabaseFields>().germanRepalcement && !hex.GetComponent<HexDatabaseFields>().alliedControl && !hex.GetComponent<HexDatabaseFields>().inAlliedZOC)
                    if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0) ||
                            ((hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German) &&
                            (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count < GlobalDefinitions.GermanStackingLimit)))
                        if (!replacementHexes.Contains(hex.gameObject))
                        {
                            hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                            replacementHexes.Add(hex.gameObject);
                        }

            // If there are no replacement hexes available there is no reason to continue
            if (replacementHexes.Count == 0)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("germanAIReplacementUnits: no replacement hexes available - exiting");
#endif
                return;
            }

            // Find the unit that is the futhest west on the board (highest y coordinate)
            int furthestWest = 0;
            GameObject furthestWestHex = null;
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor > furthestWest)
                {
                    furthestWestHex = unit.GetComponent<UnitDatabaseFields>().occupiedHex;
                    furthestWest = unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor;
                }

            // Now sort the replacement hexes from nearest to furthest from the furthest west unit
            for (int index1 = 0; index1 < replacementHexes.Count; index1++)
                for (int index2 = (index1 + 1); index2 < replacementHexes.Count; index2++)
                    if (CalculateDistance(replacementHexes[index1], furthestWestHex) > CalculateDistance(replacementHexes[index2], furthestWestHex))
                    {
                        // Swap the two hexes
                        tempHex = replacementHexes[index1];
                        replacementHexes[index1] = replacementHexes[index2];
                        replacementHexes[index2] = tempHex;
                    }

            // Get the German units that have been eliminated but not HQ's
            foreach (Transform unit in GameObject.Find("Units Eliminated").transform)
                if ((unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German) && !unit.GetComponent<UnitDatabaseFields>().HQ)
                {
                    if (unit.GetComponent<UnitDatabaseFields>().armor)
                        armorReplacements.Add(unit.gameObject);
                    else if (unit.GetComponent<UnitDatabaseFields>().airborne)
                        airborneReplacements.Add(unit.gameObject);
                    else
                        infantryReplacements.Add(unit.gameObject);
                }

            // Sort the units from highest to lowest attack factor
            if (armorReplacements.Count > 1)
                for (int index1 = 0; index1 < armorReplacements.Count; index1++)
                    for (int index2 = (index1 + 1); index2 < armorReplacements.Count; index2++)
                        if (armorReplacements[index1].GetComponent<UnitDatabaseFields>().attackFactor < armorReplacements[index2].GetComponent<UnitDatabaseFields>().attackFactor)
                        {
                            // Swap the two units
                            tempUnit = armorReplacements[index1];
                            armorReplacements[index1] = armorReplacements[index2];
                            armorReplacements[index2] = tempUnit;
                        }

            if (airborneReplacements.Count > 1)
                for (int index1 = 0; index1 < airborneReplacements.Count; index1++)
                    for (int index2 = (index1 + 1); index2 < airborneReplacements.Count; index2++)
                        if (airborneReplacements[index1].GetComponent<UnitDatabaseFields>().attackFactor < airborneReplacements[index2].GetComponent<UnitDatabaseFields>().attackFactor)
                        {
                            // Swap the two units
                            tempUnit = airborneReplacements[index1];
                            airborneReplacements[index1] = airborneReplacements[index2];
                            airborneReplacements[index2] = tempUnit;
                        }

            if (infantryReplacements.Count > 1)
                for (int index1 = 0; index1 < infantryReplacements.Count; index1++)
                    for (int index2 = (index1 + 1); index2 < infantryReplacements.Count; index2++)
                        if (infantryReplacements[index1].GetComponent<UnitDatabaseFields>().attackFactor < infantryReplacements[index2].GetComponent<UnitDatabaseFields>().attackFactor)
                        {
                            // Swap the two units
                            tempUnit = infantryReplacements[index1];
                            infantryReplacements[index1] = infantryReplacements[index2];
                            infantryReplacements[index2] = tempUnit;
                        }
            tempUnit = ReturnReplacementUnit(armorReplacements, airborneReplacements, infantryReplacements);

            while (tempUnit != null)
            {
                GlobalDefinitions.GuiUpdateStatusMessage("German replacement " + tempUnit.GetComponent<UnitDatabaseFields>().unitDesignation + " being placed on board");
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("      replacement being moved to hex " + replacementHexes[0].name + "available for movement flag = " + replacementHexes[0].GetComponent<HexDatabaseFields>().availableForMovement);
#endif
                // The unit at the index position needs to be placed on a replacement hex

                tempUnit.transform.parent = GlobalDefinitions.allUnitsOnBoard.transform;
                tempUnit.GetComponent<UnitDatabaseFields>().unitEliminated = false;
                // Add the unit to the OnBoard list
                GlobalDefinitions.germanUnitsOnBoard.Add(tempUnit);

                // Change the unit's location to the target hex
                GeneralHexRoutines.PutUnitOnHex(tempUnit, replacementHexes[0]);

                tempUnit.GetComponent<UnitDatabaseFields>().hasMoved = false;
                tempUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex = replacementHexes[0];
                if (!GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().CheckForAdjacentEnemy(replacementHexes[0], tempUnit) &&
                        (tempUnit.GetComponent<UnitDatabaseFields>().armor || tempUnit.GetComponent<UnitDatabaseFields>().airborne))
                    tempUnit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = true;

                // If this is the only unit in the target hex then update ZOC's
                if (replacementHexes[0].GetComponent<HexDatabaseFields>().occupyingUnit.Count < 2)
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().UpdateZOC(replacementHexes[0]);

                GlobalDefinitions.germanReplacementsRemaining -= tempUnit.GetComponent<UnitDatabaseFields>().attackFactor;

                // If the replacement unit is placed in an enemy ZOC then it must attack and cannot move
                if (replacementHexes[0].GetComponent<HexDatabaseFields>().inAlliedZOC)
                    tempUnit.GetComponent<UnitDatabaseFields>().hasMoved = true;

                // See if the current replacement hex is at the stacking limit.  If it is remove it from the list
                if (replacementHexes[0].GetComponent<HexDatabaseFields>().occupyingUnit.Count == GlobalDefinitions.GermanStackingLimit)
                    replacementHexes.RemoveAt(0);
                //else
                // The available for movement flag is reset in the landGermanUnitFromOffBoard routine (since it is used interactively also). Set it back to true.
                //replacementHexes[0].GetComponent<HexDatabaseFields>().availableForMovement = true;

                if (tempUnit.GetComponent<UnitDatabaseFields>().armor)
                    armorReplacements.Remove(tempUnit);

                else if (tempUnit.GetComponent<UnitDatabaseFields>().airborne)
                    airborneReplacements.Remove(tempUnit);

                else
                    infantryReplacements.Remove(tempUnit);

                tempUnit = ReturnReplacementUnit(armorReplacements, airborneReplacements, infantryReplacements);
            }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("germanAIReplacementUnits: exiting German Replacement Remaining = " + GlobalDefinitions.germanReplacementsRemaining);
#endif
        }

        /// <summary>
        /// Returns the next replacement available in order of armor, airborne, infantry
        /// </summary>
        /// <param name="armorList"></param>
        /// <param name="airborneList"></param>
        /// <param name="infantryList"></param>
        /// <returns></returns>
        private static GameObject ReturnReplacementUnit(List<GameObject> armorList, List<GameObject> airborneList, List<GameObject> infantryList)
        {
            // First check if there is an armor unit that there is enough factors for
            foreach (GameObject unit in armorList)
                if (unit.GetComponent<UnitDatabaseFields>().attackFactor <= GlobalDefinitions.germanReplacementsRemaining)
                    return (unit);

            // If we get here but there are still units in the armor list it means that we need more replacement factors to get the armor so return a null
            if (armorList.Count > 0)
                return (null);

            foreach (GameObject unit in airborneList)
                if (unit.GetComponent<UnitDatabaseFields>().attackFactor <= GlobalDefinitions.germanReplacementsRemaining)
                    return (unit);

            foreach (GameObject unit in infantryList)
                if (unit.GetComponent<UnitDatabaseFields>().attackFactor <= GlobalDefinitions.germanReplacementsRemaining)
                    return (unit);

            return (null);
        }

        /// <summary>
        /// Returns the distance between a hex and the position passed
        /// </summary>
        /// <param name="hex1"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static float CalculateDistance(GameObject hex1, Vector2 target)
        {
            return ((float)Math.Sqrt(Math.Pow(Math.Abs(target.x - hex1.GetComponent<HexDatabaseFields>().xMapCoor), 2) + Math.Pow(Math.Abs(target.y - hex1.GetComponent<HexDatabaseFields>().yMapCoor), 2)));
        }

        /// <summary>
        /// Returns the distance from a hex to another hex
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="targetLocation"></param>
        /// <returns></returns>
        private static float CalculateDistance(GameObject hex1, GameObject hex2)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("calcualteDistanec: hex = " + hex1.name + " coord = " + hex1.GetComponent<HexDatabaseFields>().xMapCoor + " , " + hex1.GetComponent<HexDatabaseFields>().yMapCoor);
        GlobalDefinitions.WriteToLogFile("calcualteDistanec: hex = " + hex2.name + " coord = " + hex2.GetComponent<HexDatabaseFields>().xMapCoor + " , " + hex2.GetComponent<HexDatabaseFields>().yMapCoor);
#endif
            return ((float)Math.Sqrt(Math.Pow(Math.Abs(hex2.GetComponent<HexDatabaseFields>().xMapCoor - hex1.GetComponent<HexDatabaseFields>().xMapCoor), 2) + Math.Pow(Math.Abs(hex2.GetComponent<HexDatabaseFields>().yMapCoor - hex1.GetComponent<HexDatabaseFields>().yMapCoor), 2)));
        }

        /// <summary>
        /// Sort the invasion hexes from best to worst
        /// </summary>
        /// <param name="invasionArea"></param>
        private static void SortInvasionHexes(InvasionArea invasionArea)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("sortInvasionHexes: starting order of invasion hexes");
        foreach (GameObject hex in invasionArea.invasionHexes)
            GlobalDefinitions.WriteToLogFile("      " + hex.name);
#endif

            // Note that the scoring used is to take the supply available from a hex and divide by the defense of the hex.  Even after I fixed the fact that the 
            // "float" comparison was only using integer values (because if you don't cast one of the oprands of an integer division to (float) you end up with an
            // integer result even if you assign the result to a float) there are a lot of ties.  I have reseeded the invasion hexes for the invasion areas when a 
            // game is restarting to avoid different results but this still brings up the issue that at some point I probably want a more detailed scoring for the 
            // best hex in order to avoid having so many ties.

            GameObject tempHex;
            // Since the algorithm is going to attempt to invade the hexes in the order they are stored, they need
            // to be stored in order that lists the best hexes first.  This way units aren't wasted on poor hexes 
            // listed first.
            for (int index1 = 0; index1 < invasionArea.invasionHexes.Count; index1++)
                for (int index2 = (index1 + 1); index2 < invasionArea.invasionHexes.Count; index2++)
                {
                    int defense1 = 1, defense2 = 1;

                    // The best hexes are determined by dividing the supply capacity of the hex by the defense factors

                    foreach (GameObject unit in invasionArea.invasionHexes[index1].GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit)
                        defense1 += ReturnBaseDefenseFactor(unit);
                    foreach (GameObject unit in invasionArea.invasionHexes[index2].GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit)
                        defense2 += ReturnBaseDefenseFactor(unit);
                    float score1 = (float)invasionArea.invasionHexes[index1].GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().supplyCapacity / defense1;
                    float score2 = (float)invasionArea.invasionHexes[index2].GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().supplyCapacity / defense2;
                    if (score1 < score2)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("sortInvasionHexes: swapping hexes " + invasionArea.invasionHexes[index1].name + " " + score1.ToString("0.00") + " and " + invasionArea.invasionHexes[index2].name + " " + score2.ToString("0.00"));
#endif
                        tempHex = invasionArea.invasionHexes[index1];
                        invasionArea.invasionHexes[index1] = invasionArea.invasionHexes[index2];
                        invasionArea.invasionHexes[index2] = tempHex;
                    }
                }
        }

        /// <summary>
        /// Determines what beach to invade and assigns attacks
        /// </summary>
        public static void DetermineInvasionSite()
        {
            List<GameObject> invadingUnits = new List<GameObject>();
            int maxInvasionAreaScore = 0;

            List<AIPotentialAttack> listPotentialAttacks = new List<AIPotentialAttack>();
            List<AIPotentialAttack> tempListPotentialAttacks = new List<AIPotentialAttack>();
            List<GameObject> listOfHexesToBeAttacked = new List<GameObject>();
            List<GameObject> listOfNewAttackHexesAdded = new List<GameObject>();
            List<GameObject> tempListOfNewAttackHexesAdded = new List<GameObject>();
            bool oddsMet = false;

            GlobalDefinitions.invasionsTookPlaceThisTurn = true;
            foreach (InvasionArea invasionArea in GlobalDefinitions.invasionAreas)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("determineInvasionSite: evaluating invasion area " + invasionArea.name);
#endif
                SortInvasionHexes(invasionArea);
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("determineInvasionSite: sorted list of invasion hexes");
            foreach (GameObject hex in invasionArea.invasionHexes)
                GlobalDefinitions.WriteToLogFile("determineInvasionSite:    invasion hex = " + hex.name + " invasion target = " + hex.GetComponent<HexDatabaseFields>().invasionTarget.name);
#endif
                // Reset the list of invading units and then reload
                invadingUnits.Clear();
                LoadInvadingUnits(invadingUnits, invasionArea.invasionHexes[0].GetComponent<HexDatabaseFields>().invasionAreaIndex);

                // The invasion score will be the accumulated score for each invasion hex in an area
                int invasionAreaScore = 0;
                int tempInvasionAreaScore = 0;

                // Now go through and score each invasion hex
                foreach (GameObject invasionHex in invasionArea.invasionHexes)
                {
                    int odds = 0;
                    // Make sure that the hex hasn't been occupied as a result of a previous attack
                    if (!((invasionHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (invasionHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)))
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("determineInvasionSite: execute for invading hex = " + invasionHex.name + " defending hex = " + invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.name + " number of potential invaders = " + invadingUnits.Count);
#endif
                        // An invasion target can be attacked as part of the invasion of an adjacent hex.  It is unlikely that this would not use the invasion hex but it is possible.
                        // So check that the target hex isn't already being attacked.
                        bool uncommittedUnitAvailable = false;
                        foreach (GameObject unit in invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit)
                            if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                                uncommittedUnitAvailable = true;
                        if (invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                            uncommittedUnitAvailable = true;

                        if (uncommittedUnitAvailable)
                        {
                            // We will add hexes to the list and continue processing until all of the hexes in the list have been moved to potential attacks or canceled.
                            // This is the same stack structure that is used in the combat routines
                            listOfHexesToBeAttacked.Add(invasionHex.GetComponent<HexDatabaseFields>().invasionTarget);
                            while (listOfHexesToBeAttacked.Count > 0)
                            {
                                odds = InvasionOdds(invasionHex, invadingUnits, tempListPotentialAttacks, listOfHexesToBeAttacked, tempListOfNewAttackHexesAdded, ref oddsMet);

                                // If the odds returned is 0 that means that a valid battle can't be made so cancel the attacks loaded
                                if (odds == 0)
                                {
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("determineInvasionSite: odds returned is 0 so canceling attacks attackNumber = " + tempListPotentialAttacks.Count);
#endif
                                    RemovePotentialInvasions(tempListPotentialAttacks);
                                    tempListPotentialAttacks.Clear();
                                    foreach (GameObject hex in tempListOfNewAttackHexesAdded)
                                        if (listOfHexesToBeAttacked.Contains(hex))
                                            listOfHexesToBeAttacked.Remove(hex);
                                    tempInvasionAreaScore = 0;
                                }

                                else
                                {
                                    // Get the supply capacity of the entire attack (not just the invading hex).  Additional defending hexes can be brought in.
                                    int supplyCapacity = 0;
                                    List<GameObject> attackingHexes = new List<GameObject>();
                                    foreach (AIPotentialAttack potentialAttack in tempListPotentialAttacks)
                                    {
                                        foreach (AIDefendHex aihex in potentialAttack.defendingHexes)
                                            supplyCapacity += aihex.defendingHex.GetComponent<HexDatabaseFields>().supplyCapacity;

                                        foreach (GameObject unit in potentialAttack.attackingUnits)
                                            if (!attackingHexes.Contains(unit.GetComponent<UnitDatabaseFields>().occupiedHex))
                                            {
                                                supplyCapacity += unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplyCapacity;
                                                attackingHexes.Add(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                                            }
                                    }

                                    //tempInvasionAreaScore += odds * supplyCapacity
                                    //        * (invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().invasionAreaIndex + 1);
                                    tempInvasionAreaScore = odds * supplyCapacity;
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("determineInvasionSite: Updating tempInvasionScore to " + tempInvasionAreaScore + "   odds = " + odds + "  supplyCapacity = " + supplyCapacity);
#endif
                                }

                                // It is possible for the cancel of an attack to remove all the hexes so check this before trying to remove anything
                                if (listOfHexesToBeAttacked.Count > 0)
                                    listOfHexesToBeAttacked.RemoveAt(0);
                            }

                            // Update the invasion score if there were attacks added
                            if (tempListPotentialAttacks.Count > 0)
                                invasionAreaScore += tempInvasionAreaScore * (invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().invasionAreaIndex + 1);
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("determineInvasionSite: Updating invasionScore to " + invasionAreaScore + "   tempInvasionScore = " + tempInvasionAreaScore);
#endif
                            // Transfer the tempLists to the permanent lists and then reset the temp lists for the next invasion target
                            if (tempListPotentialAttacks.Count > 0)
                                foreach (AIPotentialAttack tempAttack in tempListPotentialAttacks)
                                    listPotentialAttacks.Add(tempAttack);
                            tempListPotentialAttacks.Clear();

                            if (tempListOfNewAttackHexesAdded.Count > 0)
                                foreach (GameObject hex in tempListOfNewAttackHexesAdded)
                                    listOfNewAttackHexesAdded.Add(hex);
                            tempListOfNewAttackHexesAdded.Clear();
                        }
                    }
                }

                if (invasionAreaScore >= maxInvasionAreaScore)
                {
                    maxInvasionAreaScore = invasionAreaScore;
                    if (GlobalDefinitions.turnNumber == 1)
                        GlobalDefinitions.firstInvasionAreaIndex = invasionArea.invasionHexes[0].GetComponent<HexDatabaseFields>().invasionAreaIndex;
                    else
                        GlobalDefinitions.secondInvasionAreaIndex = invasionArea.invasionHexes[0].GetComponent<HexDatabaseFields>().invasionAreaIndex;
                }

                // Clear out the list of attacks before moving to the next area
                RemovePotentialInvasions(listPotentialAttacks);

                // Remove all the hexes that were added for this hex
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("determineInvasionSite: Removing hexes due to canceled attack:");
#endif
                foreach (GameObject hex in listOfNewAttackHexesAdded)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("determineInvasionSite:                 " + hex.name);
#endif
                    listOfHexesToBeAttacked.Remove(hex);
                }

                listPotentialAttacks.Clear();
                listOfNewAttackHexesAdded.Clear();
                listOfHexesToBeAttacked.Clear();
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("determineInvasionSite:    score = " + invasionAreaScore + " for invasion area " + invasionArea.name);
            GlobalDefinitions.WriteToLogFile("");
            GlobalDefinitions.WriteToLogFile("");
#endif
            }

            // All the areas have been evaluated at this point and the max scoring area is stored.  Go back and execute the attacks on the highest scoring area.
            invadingUnits.Clear();

            int invasionAreaIndex;
            if (GlobalDefinitions.turnNumber == 1)
                invasionAreaIndex = GlobalDefinitions.firstInvasionAreaIndex;
            else
                invasionAreaIndex = GlobalDefinitions.secondInvasionAreaIndex;
            //invasionAreaIndex = 6; // TESTING
            GlobalDefinitions.WriteToLogFile("determineInvasionSite: top scoring invasion site is " + GlobalDefinitions.invasionAreas[invasionAreaIndex].name);
            // Set the global variables for the invasion site
            GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().SetInvasionArea(invasionAreaIndex);

            SortInvasionHexes(GlobalDefinitions.invasionAreas[invasionAreaIndex]);
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("determineInvasionSite: sorted list of invasion hexes");
        foreach (GameObject hex in GlobalDefinitions.invasionAreas[invasionAreaIndex].invasionHexes)
            GlobalDefinitions.WriteToLogFile("determineInvasionSite:    " + hex.GetComponent<HexDatabaseFields>().invasionTarget.name);
#endif
            LoadInvadingUnits(invadingUnits, invasionAreaIndex);

            foreach (GameObject invasionHex in GlobalDefinitions.invasionAreas[invasionAreaIndex].invasionHexes)
            {
                // Check that units aren't already on the hex because of another attack
                if (!((invasionHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                        (invasionHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)))
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("determineInvasionSite: execute for invading hex = " + invasionHex.name + " defending hex = " + invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.name + " number of potential invaders = " + invadingUnits.Count);
#endif
                    // An invasion target can be attacked as part of the invasion of an adjacent hex.  It is unlikely that this would not use the invasion hex but it is possible.
                    // So check that the target hex isn't already being attacked.
                    bool uncommittedUnitAvailable = false;
                    foreach (GameObject unit in invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit)
                        if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                            uncommittedUnitAvailable = true;
                    if (invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                        uncommittedUnitAvailable = true;

#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("determineInvasionSite: uncommittedUnitAvailable = " + uncommittedUnitAvailable);
#endif
                    if (uncommittedUnitAvailable)
                    {
                        // We will add hexes to the list and continue processing until all of the hexes in the list have been moved to potential attacks or canceled.
                        // This is the same stack structure that is used in the combat routines
                        listOfHexesToBeAttacked.Add(invasionHex.GetComponent<HexDatabaseFields>().invasionTarget);
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("determineInvasionSite: adding initial hex invasion target = " + invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.name);
#endif
                        while (listOfHexesToBeAttacked.Count > 0)
                        {
                            AIInvasion(invasionHex, invadingUnits, tempListPotentialAttacks, tempListOfNewAttackHexesAdded, listOfNewAttackHexesAdded, ref oddsMet);
                            if (tempListPotentialAttacks.Count > 0)
                                foreach (AIPotentialAttack tempAttack in tempListPotentialAttacks)
                                    listPotentialAttacks.Add(tempAttack);
                            tempListPotentialAttacks.Clear();

                            if (tempListOfNewAttackHexesAdded.Count > 0)
                                foreach (GameObject hex in tempListOfNewAttackHexesAdded)
                                    listOfNewAttackHexesAdded.Add(hex);
                            tempListOfNewAttackHexesAdded.Clear();

                            // It is possible for the cancel of an attack to remove all the hexes so check this before trying to remove anything
                            if (listOfHexesToBeAttacked.Count > 0)
                                listOfHexesToBeAttacked.RemoveAt(0);

                        }
                    }
                }

#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("determineInvasionSite: Checking if hex " + invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.name + " was unopposed so should be flagged as successfully invaded");
#endif
                // In cases where there is no defense on an invasion target and the unit moves but has to attack, the code does not have awareness that it 
                // is an invasion attack.  Therefore I will check here if there are units on the invasion target and set the flag for successfully invased hexes.
                if ((invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                        (invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("determineInvasionSite:        hex is being flagged as successfully invaded");
#endif
                    invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().successfullyInvaded = true;
                    invasionHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().alliedControl = true;
                }
            }

            // This moves unopposed units to the invasion hexes
            //GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().moveUnopposedSeaUnits();

            // For some reason the units on board list is being double loaded, I'm being lazy and I'll just reinitialize it here
            GlobalDefinitions.alliedUnitsOnBoard = GlobalDefinitions.ReturnNationUnitsOnBoard(GlobalDefinitions.Nationality.Allied);
        }

        /// <summary>
        /// Loads the lists of units with up to the limits in place on the invasion area passed depending on availability
        /// </summary>
        /// <param name="invadingUnits"></param>
        /// <param name="invasionAreaIndex"></param>
        private static void LoadInvadingUnits(List<GameObject> invadingUnits, int invasionAreaIndex)
        {
            bool allUnitsLoaded;
            GameObject tempUnit;
            int armorUnitsLoaded = 0;
            int infantryLimit;

            allUnitsLoaded = false;
            invadingUnits.Clear();
            // Load up the invading list with units available for this invasion site
            for (int index = 0; !allUnitsLoaded && (index < GlobalDefinitions.invasionAreas[invasionAreaIndex].firstTurnArmor); index++)
            {
                tempUnit = ReturnArmorFromBritain(invadingUnits);
                if ((tempUnit != null) && (!invadingUnits.Contains(tempUnit)))
                {
                    invadingUnits.Add(tempUnit);
                    armorUnitsLoaded++;
                }
                else if (tempUnit == null)
                    allUnitsLoaded = true;
            }

            // If there aren't enough armor units available to reach the limit then infantry units can be used instead
            infantryLimit = GlobalDefinitions.invasionAreas[invasionAreaIndex].firstTurnInfantry + (GlobalDefinitions.invasionAreas[invasionAreaIndex].firstTurnArmor - armorUnitsLoaded);

            allUnitsLoaded = false;
            for (int index = 0; !allUnitsLoaded && (index < infantryLimit); index++)
            {
                tempUnit = ReturnInfantryFromBritain(invadingUnits);
                if ((tempUnit != null) && (!invadingUnits.Contains(tempUnit)))
                    invadingUnits.Add(tempUnit);
                else if (tempUnit == null)
                    allUnitsLoaded = true;
            }

            allUnitsLoaded = false;
            for (int index = 0; !allUnitsLoaded && (index < GlobalDefinitions.invasionAreas[invasionAreaIndex].firstTurnAirborne); index++)
            {
                tempUnit = ReturnAirborneFromBritain(invadingUnits);
                if ((tempUnit != null) && (!invadingUnits.Contains(tempUnit)))
                    invadingUnits.Add(tempUnit);
                else if (tempUnit == null)
                    allUnitsLoaded = true;
            }
        }

        /// <summary>
        /// Loads an armor unit from Britain to the list passed
        /// I'm using the list passed to keep from returning the same unit every time the routine is called.
        /// </summary>
        /// <param name="invadingUnits"></param>
        /// <returns></returns>
        private static GameObject ReturnArmorFromBritain(List<GameObject> invadingUnits)
        {
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                if (unit.GetComponent<UnitDatabaseFields>().armor && !invadingUnits.Contains(unit.gameObject) && (unit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber))
                    return (unit.gameObject);
            return (null);
        }

        /// <summary>
        /// Loads an infantry unit from Britain to the list passed
        /// I'm using the list passed to keep from returning the same unit every time the routine is called.
        /// </summary>
        /// <param name="invadingUnits"></param>
        /// <returns></returns>
        private static GameObject ReturnInfantryFromBritain(List<GameObject> invadingUnits)
        {
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                if (unit.GetComponent<UnitDatabaseFields>().infantry && !invadingUnits.Contains(unit.gameObject) && (unit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber))
                    return (unit.gameObject);
            return (null);
        }

        /// <summary>
        /// Loads an airborne unit from Britain to the list passed
        /// </summary>
        /// <param name="invadingUnits"></param>
        /// <returns></returns>
        private static GameObject ReturnAirborneFromBritain(List<GameObject> invadingUnits)
        {
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                if (unit.GetComponent<UnitDatabaseFields>().airborne && !invadingUnits.Contains(unit.gameObject) && (unit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber))
                    return (unit.gameObject);
            return (null);
        }

        /// <summary>
        /// Assigns the units to invade the passed hex
        /// </summary>
        /// <param name="invadingHex"></param>
        /// <param name="potentialInvadingUnitsList"></param>
        /// <param name="listPotentialAttacks"></param>
        /// <param name="listOfHexesToBeAttacked"></param>
        /// <param name="listOfNewAttackHexesAdded"></param>
        /// <param name="oddsMet"></param>
        private static void AIInvasion(GameObject invadingHex, List<GameObject> potentialInvadingUnitsList, List<AIPotentialAttack> listPotentialAttacks,
                List<GameObject> listOfHexesToBeAttacked, List<GameObject> listOfNewAttackHexesAdded, ref bool oddsMet)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("AIInvasion: Executing for invading hex " + invadingHex.name);
#endif
            // An invasion target can be attacked as part of the invasion of an adjacent hex.  It is unlikely that this would not use the invasion hex but it is possible.
            // So check that the target hex isn't already being attacked.
            bool uncommittedUnitAvailable = false;
            foreach (GameObject unit in invadingHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit)
                if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    uncommittedUnitAvailable = true;
            if (invadingHex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                uncommittedUnitAvailable = true;

            if (uncommittedUnitAvailable)
            {
                // We will add hexes to the list and continue processing until all of the hexes in the list have been moved to potential attacks.
                // This is the same stack structure that is used in the combat routines
                listOfHexesToBeAttacked.Add(invadingHex.GetComponent<HexDatabaseFields>().invasionTarget);
                while (listOfHexesToBeAttacked.Count > 0)
                {
                    if (InvasionOdds(invadingHex, potentialInvadingUnitsList, listPotentialAttacks, listOfHexesToBeAttacked, listOfNewAttackHexesAdded, ref oddsMet) == 0)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("AIInvasion: odds returned is 0 so canceling attacks attackNumber = " + listPotentialAttacks.Count);
#endif
                        RemovePotentialInvasions(listPotentialAttacks);
                        listPotentialAttacks.Clear();
                        foreach (GameObject hex in listOfNewAttackHexesAdded)
                            if (listOfHexesToBeAttacked.Contains(hex))
                                listOfHexesToBeAttacked.Remove(hex);
                    }

                    // It is possible for the cancel of an attack to remove all the hexes so check this before trying to remove anything
                    if (listOfHexesToBeAttacked.Count > 0)
                        listOfHexesToBeAttacked.RemoveAt(0);
                }

                // All attacks related to the passed initial hex attack have been evaluated.  Move all the stored attacks to the GlobalDefinition variables
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("AIInvasion: Loading all attacks - attack number = " + listPotentialAttacks.Count);
#endif
                foreach (AIPotentialAttack newAttack in listPotentialAttacks)
                {
                    // If there are no defenders (unopposed invasion don't create a battle)
                    if (newAttack.defendingUnits.Count > 0)
                    {
                        GameObject singleCombat = new GameObject("SingleCombat");
                        singleCombat.AddComponent<Combat>();
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("AIInvasion:         Adding Attack");
                    GlobalDefinitions.WriteToLogFile("AIInvasion:             adding air support to combat = " + newAttack.addAirSupport);
#endif
                        singleCombat.GetComponent<Combat>().attackAirSupport = newAttack.addAirSupport;
                        foreach (GameObject unit in newAttack.defendingUnits)
                            if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("AIInvasion:             Defender " + unit.name);
#endif
                                singleCombat.GetComponent<Combat>().defendingUnits.Add(unit);
                            }

                        foreach (GameObject unit in newAttack.attackingUnits)
                            if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("AIInvasion:             Attacker " + unit.name);
#endif
                                singleCombat.GetComponent<Combat>().attackingUnits.Add(unit);
                            }
                        GlobalDefinitions.allCombats.Add(singleCombat);
                    }
                    else
                    {
                        // This executes if there are units on an invasion hex with no opposition on the defending hex.
                        // I was just leaving them on the invasion hex and the routine that starts the combat phase would
                        // move them ashore but there are scenarios where a fortress is being attacked in an adjacent hex
                        // that loads units to the defending hex for the attack.  Then when the units are moved ashore it
                        // it causes an overstack.  In order to avoid this move the units ashore now so that in the following
                        // attacks the units know that they aren't able to use the hex for an attack.
                        foreach (GameObject unit in newAttack.attackingUnits)
                        {

                            // I had a insidious bug because by moving the units ashore it didn't remove the unit from the occupyingUnit list on the sea hex
                            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().occupyingUnit.Remove(unit);
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("AIInvasion: moving unopposed unit ashore " + unit.name + " to hex " + newAttack.defendingHexes[0].defendingHex.name + " successfully invaded flag set");
#endif
                            if (newAttack.defendingHexes[0].defendingHex.GetComponent<HexDatabaseFields>().coast || newAttack.defendingHexes[0].defendingHex.GetComponent<HexDatabaseFields>().coastalPort)
                            {
                                // Set the flag on the invasion target hex to allow for reinforcement landing to take place
                                newAttack.defendingHexes[0].defendingHex.GetComponent<HexDatabaseFields>().successfullyInvaded = true;
                                newAttack.defendingHexes[0].defendingHex.GetComponent<HexDatabaseFields>().alliedControl = true;
                            }

                            newAttack.defendingHexes[0].defendingHex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(unit, newAttack.defendingHexes[0].defendingHex, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the loaded attacks that are passed.  It moves the units back to Britain and resets any status indicators.
        /// </summary>
        /// <param name="listPotentialAttacks"></param>
        private static void RemovePotentialInvasions(List<AIPotentialAttack> listPotentialAttacks)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("removePotentialAttacks: canceling attacks number of attacks = " + listPotentialAttacks.Count);
#endif
            foreach (AIPotentialAttack attack in listPotentialAttacks)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("removePotentialAttacks:       number attacking units = " + attack.attackingUnits.Count);
            GlobalDefinitions.WriteToLogFile("removePotentialAttacks:       number defending units = " + attack.defendingUnits.Count);
#endif
                foreach (GameObject attacker in attack.attackingUnits)
                {
                    //GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().decrementInvasionUnitLimits(attacker);
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(attacker.GetComponent<UnitDatabaseFields>().occupiedHex, attacker, false);
                    if (attacker.GetComponent<UnitDatabaseFields>().inSupply)
                        attacker.GetComponent<UnitDatabaseFields>().remainingMovement = attacker.GetComponent<UnitDatabaseFields>().movementFactor;
                    else
                        attacker.GetComponent<UnitDatabaseFields>().remainingMovement = 1;
                    attacker.GetComponent<UnitDatabaseFields>().hasMoved = false;
                    attacker.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                }
                foreach (GameObject defender in attack.defendingUnits)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("removePotentialAttacks: resetting defender " + defender.name + " nationality = " + defender.GetComponent<UnitDatabaseFields>().nationality);
#endif
                    // This shouldn't happen but... check that the units aren't friendly that were moved in a different attack
                    if (defender.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
                        defender.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("removePotentialAttacks:           isCommittedToAnAttack = " + defender.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack);
#endif
                }
            }

            listPotentialAttacks.Clear();
        }

        /// <summary>
        /// This routine returns the odds for the passed invasion attack
        /// </summary>
        /// <param name="invadingHex"></param>
        /// <param name="potentialInvadingUnitsList"></param>
        /// <param name="listPotentialAttacks"></param>
        /// <param name="listOfHexesToBeAttacked"></param>
        /// <param name="listOfNewAttackHexesAdded"></param>
        /// <param name="oddsMet"></param>
        /// <returns></returns>
        private static int InvasionOdds(GameObject invadingHex, List<GameObject> potentialInvadingUnitsList, List<AIPotentialAttack> listPotentialAttacks, List<GameObject> listOfHexesToBeAttacked,
                List<GameObject> listOfNewAttackHexesAdded, ref bool oddsMet)
        {
            int odds = 0;

            GameObject defendingHex = invadingHex.GetComponent<HexDatabaseFields>().invasionTarget;
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("invasionOdds: invading hex = " + invadingHex.name);
        GlobalDefinitions.WriteToLogFile("invasionOdds: potential invading units list count = " + potentialInvadingUnitsList.Count);
        GlobalDefinitions.WriteToLogFile("invasionOdds: list of potential attacks count = " + listPotentialAttacks.Count);
        GlobalDefinitions.WriteToLogFile("invasionOdds: list of hexes to be attacked count = " + listOfHexesToBeAttacked.Count);
        GlobalDefinitions.WriteToLogFile("invasionOdds: list of new attack hexes added count = " + listOfNewAttackHexesAdded.Count);
        GlobalDefinitions.WriteToLogFile("invasionOdds: Processing attack on hex = " + listOfHexesToBeAttacked[0].name);
#endif
            // Create a new potential attack structure
            AIPotentialAttack newPotentialAttack = new AIPotentialAttack();
            AIDefendHex newAIDefendingHex = new AIDefendHex();

            // List to store the single attack hexes for this attack
            List<AISingleAttackHex> singleAttackHexes = new List<AISingleAttackHex>();
            // List to store the multiple attack hexes for this attack
            List<AIMultipleAttackHex> multipleAttackHexes = new List<AIMultipleAttackHex>();

            // If this is the original defending hex the invading hex will not be loaded as a single attack hex (since it is a sea hex) so it needs to be loaded here.
            // This is the only time that a sea hex is a valid attack hex which is why this has to be done here.
            if (listOfHexesToBeAttacked[0] == defendingHex)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("invasionOdds: Executing check for unopposed invasion");
#endif
                // If there are no units on the defending hex then I need to check if it is an uncontested attack.
                // If no enemy units are exerting ZOC on the defending hex then add it as a single attack hex and move on.
                // If units are exerting ZOC it will have to be dealt with as a multi unit attack but I have to ensure there are no 
                // single attack hexes loaded since.
                if (defendingHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                {
                    if (!GlobalDefinitions.HexInEnemyZOC(listOfHexesToBeAttacked[0], GlobalDefinitions.Nationality.Allied))
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("invasionOdds:     Unopposed invasion with no other attacks");
#endif
                        // This is an unopposed invasion
                        // Need to add the invasion hex as a single attack hex.  The returnSingleAttackHexes function won't add it (and it needs to be first).
                        AISingleAttackHex newAttackHex = new AISingleAttackHex
                        {
                            attackHex = invadingHex
                        };
                        singleAttackHexes.Add(newAttackHex);
                        newAIDefendingHex.defendingHex = listOfHexesToBeAttacked[0];
                    }
                    else
                    {
                        // This executes when the invasion is unopposed but will result in an attack
                        List<GameObject> tempList = new List<GameObject>();
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("invasionOdds:     Unopposed invasion but need to make attack(s) once landed");
                    GlobalDefinitions.WriteToLogFile("invasionOdds: units exerting ZOC to hex " + listOfHexesToBeAttacked[0].name + " count = " + listOfHexesToBeAttacked[0].GetComponent<HexDatabaseFields>().unitsExertingZOC.Count);

#endif

                        // If there is more than one hex that has to be attacked after landing, I'll take the first hex and make it the invasion battle
                        // and move the other hexes to the must be attacked hexes.  Note I can only do this if the hexes aren't already under attack.

                        foreach (GameObject unit in listOfHexesToBeAttacked[0].GetComponent<HexDatabaseFields>().unitsExertingZOC)
                        {
                            if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack && !tempList.Contains(unit.GetComponent<UnitDatabaseFields>().occupiedHex))
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("invasionOdds:             adding hex to be attacked" + unit.GetComponent<UnitDatabaseFields>().occupiedHex.name);
#endif
                                tempList.Add(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                            }
                        }
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("invasionOdds:         number of additional hexes that need to be attacked = " + tempList.Count);
#endif
                        if (tempList.Count > 0)
                        {
                            AISingleAttackHex newAttackHex = new AISingleAttackHex
                            {
                                attackHex = listOfHexesToBeAttacked[0]
                            };
                            singleAttackHexes.Add(newAttackHex);

                            // I'm moving the defending hex and the hex that needs to be attacked to the first hex listed as exerting ZOC onto the invasion hex
                            newAIDefendingHex.defendingHex = tempList[0];
                            listOfHexesToBeAttacked[0] = tempList[0];

                            // If the new hex is an invasion site then add the invasion hex if it is empty and is in the targeted invasion beach
                            if ((ReturnInvasionHexForTarget(tempList[0]) != null) &&
                                    (ReturnInvasionHexForTarget(tempList[0]).GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0) &&
                                    (defendingHex.GetComponent<HexDatabaseFields>().invasionAreaIndex == tempList[0].GetComponent<HexDatabaseFields>().invasionAreaIndex))
                            {
                                newAttackHex.attackHex = ReturnInvasionHexForTarget(tempList[0]);
                                singleAttackHexes.Add(newAttackHex);
                            }
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("invasionOdds:     making the hex " + tempList[0].name + " the new defending hex");
#endif
                            // Push any other hexes that are exerting ZOC to the list of hexes that must be attacked
                            for (int index = 1; index < tempList.Count; index++)
                                if (!listOfHexesToBeAttacked.Contains(tempList[index]))
                                    listOfHexesToBeAttacked.Add(tempList[index]);
                        }
                        else
                            // In this scenario all the units are under attack already.  I can't add to the attack because it could involve other hexes that adding this
                            // hex would make it illegal.  Also, since it is in place that meant it was able to reach odds without using this hex.
                            // The bottom line is that this hex should be ignored.  I'm just going to return a 0 here.  Since this is the initial invasion hex it should be okay.
                            return (0);
                    }
                }
                else
                {
                    // This is an opposed invasion, add the invasion hex as the first single hex attack
                    AISingleAttackHex newAttackHex = new AISingleAttackHex
                    {
                        attackHex = invadingHex
                    };
                    singleAttackHexes.Add(newAttackHex);
                    newAIDefendingHex.defendingHex = listOfHexesToBeAttacked[0];
                }
            }
            else
                // This is not the original invasion hex so just add the current hex as the defending hex
                newAIDefendingHex.defendingHex = listOfHexesToBeAttacked[0];

            List<GameObject> attackHexes = new List<GameObject>();

            // Load the single and multiple attack hex lists
            attackHexes = DetermineAttackHexesForTargetDefendingHex(listOfHexesToBeAttacked[0], GlobalDefinitions.Nationality.Allied);
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("invasionOdds: number of attack hexes returned = " + attackHexes.Count);
        foreach (GameObject hex in attackHexes)
            GlobalDefinitions.WriteToLogFile("invasionOdds:      " + hex.name);
#endif
            foreach (AISingleAttackHex aiSingleAttackHex in ReturnSingleAttackHexes(listOfHexesToBeAttacked[0], attackHexes, GlobalDefinitions.Nationality.Allied))
                if (!singleAttackHexes.Contains(aiSingleAttackHex))
                    singleAttackHexes.Add(aiSingleAttackHex);
            foreach (AIMultipleAttackHex aiMultipleAttackHex in ReturnMultipleAttackHexes(listOfHexesToBeAttacked[0], attackHexes, GlobalDefinitions.Nationality.Allied))
                if (!multipleAttackHexes.Contains(aiMultipleAttackHex))
                    multipleAttackHexes.Add(aiMultipleAttackHex);
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("invasionOdds: for defending hex " + listOfHexesToBeAttacked[0].name);
        GlobalDefinitions.WriteToLogFile("  Single Attack Hexes:");
        foreach (AISingleAttackHex attackHex in singleAttackHexes)
            GlobalDefinitions.WriteToLogFile("invasionOdds:      " + attackHex.attackHex.name);
        GlobalDefinitions.WriteToLogFile("  Multiple Attack Hexes:");
        foreach (AIMultipleAttackHex attackHex in multipleAttackHexes)
            GlobalDefinitions.WriteToLogFile("invasionOdds:      " + attackHex.attackHex.name);
#endif
            // There is a scenario that can happen that when I have an unopposed landing and move the defending hex to be a hex other than the landing hex, it could have invasion hexes that are
            // not part of the current invasion area.  Need to go through the coastal hexes loaded and remove any hexes that are not part of the current invasion area.
            RemoveOutOfInvasionAreaHexes(singleAttackHexes, multipleAttackHexes, invadingHex.GetComponent<HexDatabaseFields>().invasionAreaIndex);

            // If we don't generate hexes then add 0 count lists so we don't have to keep checking throughout this routine.
            if (newAIDefendingHex.singleAttackHexes == null)
                newAIDefendingHex.singleAttackHexes = new List<AISingleAttackHex>();
            if (newAIDefendingHex.multipleAttackHexes == null)
                newAIDefendingHex.multipleAttackHexes = new List<AIMultipleAttackHex>();

            // Load the single and multiple attack hex lists to the defending hex structure
            newAIDefendingHex.singleAttackHexes = singleAttackHexes;
            newAIDefendingHex.multipleAttackHexes = multipleAttackHexes;

            //  Before we go on, check that there is a land unit left.   Otherwise we can get airborne attacks without ground support.
            bool landUnitAvailable = false;
            foreach (GameObject unit in potentialInvadingUnitsList)
                if (!unit.GetComponent<UnitDatabaseFields>().airborne && !unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    landUnitAvailable = true;
            if (!landUnitAvailable)
                return (0);

            LoadAndSortPotentialAttackersToHexes(potentialInvadingUnitsList, newAIDefendingHex, newPotentialAttack, listOfHexesToBeAttacked[0]);

            foreach (GameObject defendingUnit in listOfHexesToBeAttacked[0].GetComponent<HexDatabaseFields>().occupyingUnit)
            {
                defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("invasionOdds: Adding unit to defendingUnits list " + defendingUnit.name);
#endif
                newPotentialAttack.defendingUnits.Add(defendingUnit);
            }

            // Up to this point we have created the defending hex struture, now it's time to create the attack structure for this attack
            newPotentialAttack.defendingHexes.Add(newAIDefendingHex);

            oddsMet = false;

            // We will work down from the maximum to the minimum odds.  I was originally just checking if the attack met the minimum odds if it didn't
            // meet the target but this results in over allcoated units on an attack.
            for (int oddsTarget = GlobalDefinitions.maximumAIInvasionOdds; (!oddsMet && (listOfHexesToBeAttacked.Count > 0) && (oddsTarget >= GlobalDefinitions.minimumAIInvasionOdds)); oddsTarget--)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("invasionOdds: Executing defense hex " + listOfHexesToBeAttacked[0].name + "with odds target = " + oddsTarget);
#endif
                //List<AIPotentialAttack> oddsTargetListPotentialAttacks = new List<AIPotentialAttack>();
                List<GameObject> oddsTargetListOfNewAttackHexesAdded = new List<GameObject>();
                AIPotentialAttack oddsTargetNewPotentailAttack = new AIPotentialAttack();

                foreach (GameObject defendingUnit in listOfHexesToBeAttacked[0].GetComponent<HexDatabaseFields>().occupyingUnit)
                {
                    defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("invasionOdds: Adding unit to oddsTargetDefendingUnits list " + defendingUnit.name);
#endif
                    oddsTargetNewPotentailAttack.defendingUnits.Add(defendingUnit);
                }

                oddsTargetNewPotentailAttack.defendingHexes = newPotentialAttack.defendingHexes;

                odds = 0;
                //newPotentialAttack.attackingUnits.Clear();

#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("invasionOdds:         checking single attack hexes");
#endif
                // First we will add an attacker one by one until we either get to the maximum odds or we run out of hexes or units
                if (oddsTargetNewPotentailAttack.defendingHexes.Count > 0)
                    // Note that newPotentialAttack.defendingHexes[0] is the original defending hex
                    foreach (AISingleAttackHex singleAttackHex in oddsTargetNewPotentailAttack.defendingHexes[0].singleAttackHexes)
                        foreach (GameObject attackingUnit in singleAttackHex.potentialAttackers)
                            AssignInvasionUnitsToAttack(attackingUnit, singleAttackHex.attackHex, invadingHex, ref oddsMet, ref odds, oddsTarget, oddsTargetNewPotentailAttack);

                // The single attack didn't work so now start adding more units that would cause additional hexes to be brought in
                if (!oddsMet)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("invasionOdds:         checking multiple attack hexes");
#endif
                    if (oddsTargetNewPotentailAttack.defendingHexes.Count > 0)
                    {
                        // Note that newPotentialAttack.defendingHexes[0] is the original defending hex
                        foreach (AIMultipleAttackHex multipleAttackHex in oddsTargetNewPotentailAttack.defendingHexes[0].multipleAttackHexes)
                            foreach (GameObject attackingUnit in multipleAttackHex.potentialAttackers)
                            {
                                AssignInvasionUnitsToAttack(attackingUnit, multipleAttackHex.attackHex, invadingHex, ref oddsMet, ref odds, oddsTarget, oddsTargetNewPotentailAttack);

                                // If the attack hex was used add the additional hexes that this brings into the battle
                                if (attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                                    // Note that the additional hexes that are brought in battle are not factored in the current battle.  They are pushed to the stack and will be dealt with separately.
                                    foreach (GameObject hex in multipleAttackHex.additionalDefendingHexes)
                                        if (!listOfHexesToBeAttacked.Contains(hex))
                                        {
#if OUTPUTDEBUG
                                        GlobalDefinitions.WriteToLogFile("invasionOdds:             additional defending hex being added " + hex.name);
#endif
                                            listOfHexesToBeAttacked.Add(hex);
                                            // Store these hexes in case the attack is called off
                                            oddsTargetListOfNewAttackHexesAdded.Add(hex);
                                        }
                            }
                    }
                }

                // Odds were not met so reset the units
                if (!oddsMet)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("invasionOdds:         calling off attack odds = " + odds);
#endif
                    foreach (GameObject attacker in oddsTargetNewPotentailAttack.attackingUnits)
                    {
                        //GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().decrementInvasionUnitLimits(attacker);
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(attacker.GetComponent<UnitDatabaseFields>().occupiedHex, attacker, false);
                        if (attacker.GetComponent<UnitDatabaseFields>().inSupply)
                            attacker.GetComponent<UnitDatabaseFields>().remainingMovement = attacker.GetComponent<UnitDatabaseFields>().movementFactor;
                        else
                            attacker.GetComponent<UnitDatabaseFields>().remainingMovement = 1;
                        attacker.GetComponent<UnitDatabaseFields>().hasMoved = false;
                        attacker.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                    }
                    foreach (GameObject defender in oddsTargetNewPotentailAttack.defendingUnits)
                    {
                        // This shouldn't happen but... check that the units aren't friendly that were moved in a different attack
                        if (defender.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
                            defender.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                    }

                    // Remove all the hexes that were added for this attempt
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("removePotentialAttacks: Removing hexes due to due to odds target not met:");
#endif
                    foreach (GameObject hex in oddsTargetListOfNewAttackHexesAdded)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("removePotentialAttacks:       " + hex.name);
#endif
                        listOfHexesToBeAttacked.Remove(hex);
                    }
                }

                if (oddsMet)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("invasionOdds:         saving attack odds = " + odds);
#endif
                    listPotentialAttacks.Add(oddsTargetNewPotentailAttack);
                    foreach (GameObject newHex in oddsTargetListOfNewAttackHexesAdded)
                    {
                        if (!listOfHexesToBeAttacked.Contains(newHex))
                            listOfHexesToBeAttacked.Add(newHex);
                        if (!listOfNewAttackHexesAdded.Contains(newHex))
                            listOfNewAttackHexesAdded.Add(newHex);
                    }
                }
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("InvasionOdds: completed check with oddsTarget = " + oddsTarget + " oddsMet = " + oddsMet + " listOfHexesToBeAttacked.Count = " + listOfHexesToBeAttacked.Count + " minimum odds target = " + GlobalDefinitions.minimumAIInvasionOdds);
#endif
            }

            if (!oddsMet)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("invasionOdds: odds target not met, returning odds = 0");
#endif
                return (0);
            }
            else
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("invasionOdds: odds target met, returning odds = " + odds);
#endif
                return (odds);
            }
        }

        /// <summary>
        /// Assigns units to the invasion attack
        /// </summary>
        /// <param name="attackingUnit"></param>
        /// <param name="attackHex"></param>
        /// <param name="invadingHex"></param>
        /// <param name="oddsMet"></param>
        /// <param name="odds"></param>
        /// <param name="attackingUnits"></param>
        /// <param name="newPotentialAttack"></param>
        private static void AssignInvasionUnitsToAttack(GameObject attackingUnit, GameObject attackHex, GameObject invadingHex, ref bool oddsMet, ref int odds, int oddsTarget, AIPotentialAttack newPotentialAttack)
        {
            if (!oddsMet && !attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack
                                   && GlobalDefinitions.HexUnderStackingLimit(attackHex, attackingUnit.GetComponent<UnitDatabaseFields>().nationality))
            {
                // The unit and the hex are available so move the unit to the hex
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("assignInvasionUnitsToAttack: moving unit " + attackingUnit.name + " to hex " + attackHex.name);
#endif
                //if (attackHex.GetComponent<HexDatabaseFields>().coast || attackHex.GetComponent<HexDatabaseFields>().coastalPort)
                //{
                //    // Set the flag on the invasion target hex to allow for reinforcement landing to take place
                //    attackHex.GetComponent<HexDatabaseFields>().successfullyInvaded = true;
                //    attackHex.GetComponent<HexDatabaseFields>().alliedControl = true;
                //}

                attackHex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                if (attackHex == invadingHex)
                    GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().GetUnitInvasionHex(attackingUnit, attackHex);
                else
                {
                    // Need to check if this is an airborne drop since it is brought in differently
                    if (attackHex.GetComponent<HexDatabaseFields>().coast || attackHex.GetComponent<HexDatabaseFields>().coastalPort)
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(attackingUnit, attackHex, false);
                    else
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitFromBritain(attackHex, attackingUnit);
                }
                attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
                newPotentialAttack.attackingUnits.Add(attackingUnit);
                if (newPotentialAttack.defendingUnits.Count == 0)
                    odds = oddsTarget;
                else
                {
                    odds = CalculateBattleOddsRoutines.ReturnCombatOdds(newPotentialAttack.defendingUnits, newPotentialAttack.attackingUnits, false);
                    newPotentialAttack.addAirSupport = false;
                }
                if (odds >= oddsTarget)
                    oddsMet = true;

                // See if adding air support will meet the odds
                if (!oddsMet)
                {
                    odds = CalculateBattleOddsRoutines.ReturnCombatOdds(newPotentialAttack.defendingUnits, newPotentialAttack.attackingUnits, true);
                    newPotentialAttack.addAirSupport = true;
                    if (odds >= oddsTarget)
                        oddsMet = true;
                }
            }
        }

        /// <summary>
        /// Loads and sorts the potential units to the correct hexes
        /// </summary>
        /// <param name="potentialInvadingUnitsList"></param>
        /// <param name="newAIDefendingHex"></param>
        /// <param name="newPotentialAttack"></param>
        /// <param name="defendingHex"></param>
        private static void LoadAndSortPotentialAttackersToHexes(List<GameObject> potentialInvadingUnitsList, AIDefendHex newAIDefendingHex, AIPotentialAttack newPotentialAttack, GameObject defendingHex)
        {
            foreach (GameObject unit in potentialInvadingUnitsList)
                if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                {
                    foreach (AISingleAttackHex singleAttackHex in newAIDefendingHex.singleAttackHexes)
                        // The following code isn't exactly correct.  It is not allowing airborne units to invade only land by air.  This is an isolated case that I'm not
                        // spending the time to check for in the AI.  Maybe later...

                        // Add if it is an invasion or coast hex only if it isn't an airborne unit
                        if (!unit.GetComponent<UnitDatabaseFields>().airborne)
                        {
                            if (singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().sea || singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().coast || singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().coastalPort)
                                singleAttackHex.potentialAttackers.Add(unit);
                        }
                        else
                        {
                            if (!singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().sea && !singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().coast && !singleAttackHex.attackHex.GetComponent<HexDatabaseFields>().coastalPort)
                                singleAttackHex.potentialAttackers.Add(unit);
                        }

                    foreach (AIMultipleAttackHex multipleAttackHex in newAIDefendingHex.multipleAttackHexes)
                        // The following code isn't exactly correct.  It is not allowing airborne units to invade.  This is an isolated case that I'm not
                        // spending the time to check for in the AI.  Maybe later...

                        // Add if it is an invasion or coast hex only if it isn't an airborne unit
                        if (!unit.GetComponent<UnitDatabaseFields>().airborne)
                        {
                            if (multipleAttackHex.attackHex.GetComponent<HexDatabaseFields>().sea || multipleAttackHex.attackHex.GetComponent<HexDatabaseFields>().coast || multipleAttackHex.attackHex.GetComponent<HexDatabaseFields>().coastalPort)
                                multipleAttackHex.potentialAttackers.Add(unit);
                        }
                        else
                        {
                            if (!multipleAttackHex.attackHex.GetComponent<HexDatabaseFields>().sea && !multipleAttackHex.attackHex.GetComponent<HexDatabaseFields>().coast && !multipleAttackHex.attackHex.GetComponent<HexDatabaseFields>().coastalPort)
                                multipleAttackHex.potentialAttackers.Add(unit);
                        }
                }

            // The units need to be sorted from highest to lowest attack factor since the allocation is counting on this.
            // Note that this will put airborne units to the end of the list

            foreach (AISingleAttackHex singleAttackHex in newAIDefendingHex.singleAttackHexes)
                SortListByAttackFactor(singleAttackHex.potentialAttackers);
            foreach (AIMultipleAttackHex multipleAttackHex in newAIDefendingHex.multipleAttackHexes)
                SortListByAttackFactor(multipleAttackHex.potentialAttackers);

            //newPotentialAttack.defendingHexes.Add(newAIDefendingHex);

            //foreach (GameObject defendingUnit in defendingHex.GetComponent<HexDatabaseFields>().occupyingUnit)
            //{
            //    defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
            //    newPotentialAttack.defendingUnits.Add(defendingUnit);
            //}
        }

        /// <summary>
        /// Goes through the hexes passed and removes invasion hexes that are not in the invasion area passed
        /// </summary>
        /// <param name="singleAttackHexes"></param>
        /// <param name="multipleAttackHexes"></param>
        /// <param name="invasionAreaIndex"></param>
        private static void RemoveOutOfInvasionAreaHexes(List<AISingleAttackHex> singleAttackHexes, List<AIMultipleAttackHex> multipleAttackHexes, int invasionAreaIndex)
        {

            List<AISingleAttackHex> hexesToRemove = new List<AISingleAttackHex>();
            foreach (AISingleAttackHex aiSingleHex in singleAttackHexes)
                if ((aiSingleHex.attackHex.GetComponent<HexDatabaseFields>().coast || aiSingleHex.attackHex.GetComponent<HexDatabaseFields>().coastalPort) &&
                        (aiSingleHex.attackHex.GetComponent<HexDatabaseFields>().invasionAreaIndex != invasionAreaIndex))
                    hexesToRemove.Add(aiSingleHex);

            foreach (AISingleAttackHex aiSingleHex in hexesToRemove)
                singleAttackHexes.Remove(aiSingleHex);

            List<AIMultipleAttackHex> multipleHexesToRemove = new List<AIMultipleAttackHex>();
            foreach (AIMultipleAttackHex aiMultipleHex in multipleAttackHexes)
                if ((aiMultipleHex.attackHex.GetComponent<HexDatabaseFields>().coast || aiMultipleHex.attackHex.GetComponent<HexDatabaseFields>().coastalPort) &&
                        (aiMultipleHex.attackHex.GetComponent<HexDatabaseFields>().invasionAreaIndex != invasionAreaIndex))
                    multipleHexesToRemove.Add(aiMultipleHex);

            foreach (AIMultipleAttackHex aiMultipleHex in multipleHexesToRemove)
                multipleAttackHexes.Remove(aiMultipleHex);

        }

        /// <summary>
        /// Returns the hex that targets the passed hex as an invasion site
        /// </summary>
        /// <param name="targetHex"></param>
        /// <returns></returns>
        private static GameObject ReturnInvasionHexForTarget(GameObject targetHex)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnInvasionHexForTarget: processing for target hex " + targetHex.name);
#endif
            foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                if ((hex.GetComponent<HexDatabaseFields>().invasionTarget == targetHex) && !targetHex.GetComponent<HexDatabaseFields>().inlandPort)
                    return (hex.gameObject);
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnInvasionHexForTarget: ERROR - did not find an invasion hex for the target passed");
#endif
            return (null);
        }

        /// <summary>
        /// Reorders the passed list from highest attack factor to least attack factor
        /// </summary>
        /// <param name="unitList"></param>
        public static void SortListByAttackFactor(List<GameObject> unitList)
        {
            GameObject tempUnit;
            for (int index1 = 0; index1 < unitList.Count; index1++)
                for (int index2 = (index1 + 1); index2 < unitList.Count; index2++)
                    if (unitList[index1].GetComponent<UnitDatabaseFields>().attackFactor < unitList[index2].GetComponent<UnitDatabaseFields>().attackFactor)
                    {
                        tempUnit = unitList[index1];
                        unitList[index1] = unitList[index2];
                        unitList[index2] = tempUnit;
                    }
        }

        /// <summary>
        /// This routine will assign the Allied air missions
        /// </summary>
        public static void AssignAlliedAirMissions()
        {

            // Check how many missions are remaining
            int missionsRemaining = GlobalDefinitions.maxNumberOfTacticalAirMissions - GlobalDefinitions.tacticalAirMissionsThisTurn;
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("assignAlliedAirMissions: executing number of missions remaining = " + missionsRemaining);
#endif
            List<GameObject> alliedHexes = new List<GameObject>();
            List<GameObject> enemyUnits = new List<GameObject>();
            List<GameObject> closeEnemyUnits = new List<GameObject>();
            int enemyAttackFactorTotal, defenseFactorTotal;

            // Go through each of the allied units on the board and collect hexes with Allied units on them
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                if (!alliedHexes.Contains(unit.GetComponent<UnitDatabaseFields>().occupiedHex) &&
                        !unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().city &&
                        !unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
                {
                    // The hex should be considered for air defense if there are enemy units within attack range
                    enemyUnits = FindNearbyEnemyUnits(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.GetComponent<UnitDatabaseFields>().nationality, GlobalDefinitions.attackRange);

                    if (enemyUnits.Count > 0)
                    {
                        // If the attack factors within reach of the hex is greater than the defense factor than assign air defense
                        enemyAttackFactorTotal = 0;
                        foreach (GameObject enemyUnit in enemyUnits)
                            enemyAttackFactorTotal += enemyUnit.GetComponent<UnitDatabaseFields>().attackFactor;
                        defenseFactorTotal = 0;
                        foreach (GameObject defendUnit in unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().occupyingUnit)
                            defenseFactorTotal += defendUnit.GetComponent<UnitDatabaseFields>().defenseFactor;
                        if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().city ||
                                unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortifiedZone ||
                                unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().mountain)
                            defenseFactorTotal = defenseFactorTotal * 2;
                        if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
                            defenseFactorTotal = defenseFactorTotal * 3;

                        if (enemyAttackFactorTotal > defenseFactorTotal)
                            alliedHexes.Add(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                    }
                }

            // Note that this could be improved by actually seeing what the potential combat odds are for each hex and basing the decision off of 
            // whether the air defense would make a difference.  There could also be a sorting alorithm added to determine which hexes are more 
            // important.  Not doing any of that right now.

            foreach (GameObject hex in alliedHexes)
                if (missionsRemaining > 0)
                {
                    missionsRemaining--;
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("assignAlliedAirMissions: assigning close air defense for hex - " + hex.name + "  missions remaining = " + missionsRemaining);
#endif
                    CombatResolutionRoutines.SetCloseDefenseHex(hex);
                }

            // Look for enemy units to interdict with any remaining missions.  
            if (missionsRemaining > 0)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("assignAlliedAirMissions: checking for units to interdict");
#endif
                List<GameObject> potentialInterdictUnits = new List<GameObject>();
                // Units must be available for strategic movement and be more than attack range away for it to make sense to use interdiction
                //foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
                //    if (!potentialInterdictUnits.Contains(unit))
                //    {
                //        enemyUnits = findNearbyEnemyUnits(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.GetComponent<UnitDatabaseFields>().nationality, 20);
                //        if ((enemyUnits.Count == 0) && unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement)
                //            potentialInterdictUnits.Add(unit);
                //    }
                foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
                    if (unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement)
                        potentialInterdictUnits.Add(unit);
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("assignAlliedAirMissions: number of German units = " + potentialInterdictUnits.Count);
#endif
                // At this point the potentialInterdictUnits are German units that are avaialble for strategic movement and are within 20 hexes of an Allied unit
                // I'll now check for units that are within 6 hexes

                foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                {
                    enemyUnits = FindNearbyEnemyUnits(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.GetComponent<UnitDatabaseFields>().nationality, 6);
                    foreach (GameObject enemyUnit in enemyUnits)
                        closeEnemyUnits.Add(enemyUnit);
                }
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("assignAlliedAirMissions: number of close German units = " + closeEnemyUnits.Count);
#endif
                // Remove the close enemy units from the potential list
                foreach (GameObject unit in closeEnemyUnits)
                    if (potentialInterdictUnits.Contains(unit))
                        potentialInterdictUnits.Remove(unit);

                // The potential list now contains German units from 7 to 20 hexes of Allied units
                // Sort them by attack factor
                potentialInterdictUnits.Sort((b, a) => a.GetComponent<UnitDatabaseFields>().attackFactor.CompareTo(b.GetComponent<UnitDatabaseFields>().attackFactor));

                // Now assign interdiction starting from the first unit (highest attack factor) down unitl we run out of missions
                foreach (GameObject unit in potentialInterdictUnits)
                    if (missionsRemaining > 0)
                    {
                        missionsRemaining--;
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("assignAlliedAirMissions: assigning air interdiction for unit - " + unit.name);
#endif
                        GlobalDefinitions.interdictedUnits.Add(unit);
                        GlobalDefinitions.tacticalAirMissionsThisTurn++;
                        unit.GetComponent<UnitDatabaseFields>().unitInterdiction = true;
                    }
            }
        }

        /// <summary>
        /// Returns up to the number of armor units passed from Britain if available
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static GameObject ReturnAvailableArmorUnit()
        {
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                if ((unit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber) &&
                        (unit.GetComponent<UnitDatabaseFields>().armor))
                    return (unit.gameObject);
            return (null);
        }

        /// <summary>
        /// Returns up to the number of infantry units passed from Britain if available
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static GameObject ReturnAvailableInfantryUnit()
        {
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                if ((unit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber) &&
                        (unit.GetComponent<UnitDatabaseFields>().infantry))
                    return (unit.gameObject);
            return (null);
        }

        /// <summary>
        /// Returns up to the number of airborne units passed from Britain if available
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static List<GameObject> ReturnAvailableAirborneUnits(int number)
        {
            List<GameObject> unitList = new List<GameObject>();
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                if ((unit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber) && (unit.GetComponent<UnitDatabaseFields>().airborne) && (unitList.Count < number))
                    unitList.Add(unit.gameObject);
            return (unitList);
        }

        /// <summary>
        /// Returns up to the number of HQ units passed from Britain if available
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static GameObject ReturnAvailableHQUnit(int number)
        {
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                if ((unit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber) &&
                        (unit.GetComponent<UnitDatabaseFields>().HQ))
                    return (unit.gameObject);
            return (null);
        }

        /// <summary>
        /// Returns an available unit from Britain for the hex passed
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private static GameObject ReturnReinforcementUnit(GameObject hex)
        {
            if (GlobalDefinitions.numberAlliedReinforcementsLandedThisTurn < GlobalDefinitions.maxNumberAlliedReinforcementPerTurn)
            {
                GameObject reinforcementUnit = new GameObject("returnReinforcementUnit");

                reinforcementUnit = ReturnAvailableArmorUnit();
                if ((reinforcementUnit != null) &&
                        (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnReinforcementLandingHexes(reinforcementUnit).Contains(hex)) &&
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().HexAvailableForUnitTypeReinforcements(hex, reinforcementUnit))
                    return (reinforcementUnit);

                reinforcementUnit = ReturnAvailableInfantryUnit();

                if ((reinforcementUnit != null) &&
                        (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnReinforcementLandingHexes(reinforcementUnit).Contains(hex)) &&
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().HexAvailableForUnitTypeReinforcements(hex, reinforcementUnit))
                    return (reinforcementUnit);
                else
                    return (null);
            }
            else
                return (null);
        }


        /// <summary>
        /// Land any reinforcements that are still available
        /// </summary>
        public static void LandAllAlliedReinforcementUnits()
        {
            GameObject reinforcementUnit;

            //GlobalDefinitions.availableReinforcementPorts.Sort((b, a) => a.GetComponent<HexDatabaseFields>().hexValue.CompareTo(b.GetComponent<HexDatabaseFields>().hexValue));

            // A possible improvement might be to make the sort on available supply rather than supply capacity.  Not really seeing this as an issue right now.
            GlobalDefinitions.availableReinforcementPorts.Sort((b, a) => a.GetComponent<HexDatabaseFields>().supplyCapacity.CompareTo(b.GetComponent<HexDatabaseFields>().supplyCapacity));

            foreach (GameObject hex in GlobalDefinitions.availableReinforcementPorts)
                if (!GlobalDefinitions.HexInEnemyZOC(hex, GlobalDefinitions.Nationality.Allied))
                {
                    reinforcementUnit = ReturnReinforcementUnit(hex);

                    bool movementAvailable = true;
                    while ((reinforcementUnit != null) && (movementAvailable))
                    {
                        if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(reinforcementUnit, hex, false))
                        {
                            SetUnitMovementValues(reinforcementUnit);

                            // Remove any hexes that are in enemy ZOC since we have already made all combat moves
                            RemoveEnemyZOCHexes(reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes, reinforcementUnit.GetComponent<UnitDatabaseFields>().nationality);
                            RemoveBridgeHexes(reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes);

                            // The hexes in availableMovementHexes are sorted from highest value to lowest.  Go through them and move to the first one available from the beginning.
                            bool moved = false;
                            foreach (GameObject tempHex in reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                            {
                                if (!moved && (tempHex.GetComponent<HexDatabaseFields>().supplySources.Count > 0))
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("landAllAlliedReinforcementsUnits:     checking for movement from " + reinforcementUnit.GetComponent<UnitDatabaseFields>().occupiedHex.name + " to " + tempHex.name);
#endif
                                    if (!moved && GlobalDefinitions.HexUnderStackingLimit(tempHex, GlobalDefinitions.Nationality.Allied) &&
                                            !tempHex.GetComponent<HexDatabaseFields>().sea && !tempHex.GetComponent<HexDatabaseFields>().impassible && !tempHex.GetComponent<HexDatabaseFields>().impassible &&
                                            (tempHex.GetComponent<HexDatabaseFields>().supplySources.Count > 0))
                                    {
                                        moved = true;
                                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(tempHex, reinforcementUnit.GetComponent<UnitDatabaseFields>().occupiedHex, reinforcementUnit);
                                    }
                            }

                            // Check if the unit can stay on the landing hex
                            if (!moved && GlobalDefinitions.HexUnderStackingLimit(reinforcementUnit.GetComponent<UnitDatabaseFields>().occupiedHex, GlobalDefinitions.Nationality.Allied))
                                moved = true;

                            if (!moved)
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("landAllAlliedReinforcementsUnits: couldn't move, return reinforcement back to Britain unit = " + reinforcementUnit.name);
#endif
                                GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().DecrementInvasionUnitLimits(reinforcementUnit);
                                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(reinforcementUnit.GetComponent<UnitDatabaseFields>().occupiedHex, reinforcementUnit, false);

                                // If one unit can't move than no units can moce.  Set the flag to move on to the next hex
                                movementAvailable = false;
                            }
                        }
                        reinforcementUnit = ReturnReinforcementUnit(hex);
                    }
                }
        }

        /// <summary>
        /// Returns true if additional supply capacity is needed by the Allies
        /// </summary>
        /// <returns></returns>
        public static bool SupplyNeeded()
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("supplyNeeded: allied excess supply = " + SupplyRoutines.ReturnAlliedExcessSupply());
        GlobalDefinitions.WriteToLogFile("supplyNeeded: number of allied units available  = " + ReturnNumberAlliedUnitsAvailable());
        GlobalDefinitions.WriteToLogFile("supplyNeeded: available units each turn  = " + ReturnNumberOfReinforcementsThatCanLandEachTurn());
#endif
            int excessSupply = SupplyRoutines.ReturnAlliedExcessSupply();

            if (excessSupply <= 0)
                return (true);
            else if (SupplyRoutines.ReturnAlliedExcessSupply() > ReturnNumberAlliedUnitsAvailable())
                return (false);
            else if (SupplyRoutines.ReturnAlliedExcessSupply() < ReturnNumberOfReinforcementsThatCanLandEachTurn())
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Returns the number of units that are currently available in Britain
        /// </summary>
        /// <returns></returns>
        private static int ReturnNumberAlliedUnitsAvailable()
        {
            int unitsAvailable = 0;
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                if (unit.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber)
                    unitsAvailable++;
            return (unitsAvailable);
        }

        /// <summary>
        /// Returns how many units each turn can be landed from all beaches that have an Allied controled port
        /// </summary>
        /// <returns></returns>
        public static int ReturnNumberOfReinforcementsThatCanLandEachTurn()
        {
            List<int> invasionAreasChecked = new List<int>();
            int totalUnitsAvailablePerTurn = 0;

            foreach (GameObject port in GlobalDefinitions.availableReinforcementPorts)
            {
                if (!invasionAreasChecked.Contains(port.GetComponent<HexDatabaseFields>().invasionAreaIndex))
                {
                    totalUnitsAvailablePerTurn += GlobalDefinitions.invasionAreas[port.GetComponent<HexDatabaseFields>().invasionAreaIndex].divisionsPerTurn;
                    invasionAreasChecked.Add(port.GetComponent<HexDatabaseFields>().invasionAreaIndex);
                }
            }
            return (totalUnitsAvailablePerTurn);
        }

        /// <summary>
        /// Makes all moves associated with securing and defending supply hexes
        /// </summary>
        public static void MakeSupplyMovements()
        {
            List<GameObject> availablePorts = new List<GameObject>();
            List<GameObject> availableHQs = new List<GameObject>();
            List<GameObject> unitList = new List<GameObject>();

            // The way I'm going to approach supply sources is that I am not going to have HQ units travel around the board to capture supply.
            // I'm going to check for reinfocement ports to see if there are HQ units on them.  If not I will land an HQ unit to make it a supply source.
            // This means that theoretically there should not be any HQ units wandering around the board.  There is no need for them to.

            // The first thing we're going to do is to check if there is an HQ on a supply source that is in enemy ZOC.  If I find one, move it back to Britain.
            foreach (GameObject hex in GlobalDefinitions.availableReinforcementPorts)
                if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                    foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().occupyingUnit)
                        if (unit.GetComponent<UnitDatabaseFields>().HQ && GlobalDefinitions.HexInEnemyZOC(hex, GlobalDefinitions.Nationality.Allied) &&
                                !unitList.Contains(unit))
                            unitList.Add(unit);

            foreach (GameObject unit in unitList)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeSupplyMovements: sending HQ " + unit.name + "back to Britain due to being in German ZOC");
#endif
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit, true);
            }

            foreach (GameObject hex in GlobalDefinitions.availableReinforcementPorts)
            {
                // Get ports that have no HQ unit on it and it isn't a successfully invaded hex.  Don't count hexes that are in 
                // enemy ZOC, combat movement will come later.
                if ((hex.GetComponent<HexDatabaseFields>().coastalPort || hex.GetComponent<HexDatabaseFields>().inlandPort) &&
                    !hex.GetComponent<HexDatabaseFields>().successfullyInvaded &&
                    !hex.GetComponent<HexDatabaseFields>().inGermanZOC)
                {
                    if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                        availablePorts.Add(hex.gameObject);
                    else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                    {
                        if (GlobalDefinitions.NumberHQOnHex(hex.gameObject) > 0)
                        {
                            foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().occupyingUnit)
                                if (unit.GetComponent<UnitDatabaseFields>().HQ)
                                    if (!GlobalDefinitions.HexInEnemyZOC(hex, GlobalDefinitions.Nationality.Allied))
                                        unit.GetComponent<UnitDatabaseFields>().hasMoved = true;
                        }
                        else
                            availablePorts.Add(hex.gameObject);
                    }
                }
            }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeSupplyMovements: number of empty ports = " + availablePorts.Count);
#endif
            // Sort the available ports by from highest supply capacity to lowest
            availablePorts.Sort((b, a) => a.GetComponent<HexDatabaseFields>().supplyCapacity.CompareTo(b.GetComponent<HexDatabaseFields>().supplyCapacity));

            // Now get all HQ's that are on the board that aren't on a port that wasn't invaded.  This shouldn't happen but I will check anyhow.
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                if (unit.GetComponent<UnitDatabaseFields>().HQ && (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().supplyCapacity == 0))
                    availableHQs.Add(unit);

            bool stillNeedSupply = true;
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeSupplyMovements: number of available HQs = " + availableHQs.Count);
#endif
            // Check if any of the available HQ's can move to any of the available ports
            foreach (GameObject unit in availableHQs)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeSupplyMovement: executing for HQ on board " + unit.name);
#endif
                // Get the available movement for the unit
                unit.GetComponent<UnitDatabaseFields>().availableMovementHexes =
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);

                bool unitMoved = false;
                if (stillNeedSupply)
                    foreach (GameObject hex in availablePorts)
                        if (!unitMoved && unit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Contains(hex))
                        {
                            unitMoved = true;
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(hex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
                        }

                // If the unit moved remove the port that it moved to from the available list since it is now covered
                if (unitMoved)
                {
                    availablePorts.Remove(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                    stillNeedSupply = SupplyNeeded();
                }
                else
                {
                    // The unit wasn't able to make it to an unoccupied port or wasn't needed.  See if it can move back to Britain.
                    foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                        if (!unitMoved && hex.GetComponent<HexDatabaseFields>().sea)
                        {
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("makeSupplyMovement:       sending HQ back to Britain " + unit.name);
#endif
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit, true);
                            unitMoved = true;
                        }

                    if (!unitMoved)
                    {
                        // The unit wasn't able to make it back to Britain.  Move it towards the port with the most supply
                        float closestDistance = 0;
                        foreach (GameObject availableHex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                            foreach (GameObject port in availablePorts)
                                if (closestDistance > CalculateDistance(availableHex, port))
                                    closestDistance = CalculateDistance(availableHex, port);

                        if (closestDistance > 0)
                            foreach (GameObject availableHex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                                foreach (GameObject port in availablePorts)
                                    if (closestDistance == CalculateDistance(availableHex, port))
                                    {
                                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(availableHex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
                                        unitMoved = true;
                                    }

                        // I could do more here if it still hasn't moved, but I'm not right now.  If it becomes an issue I'll expand the checks.
                    }
                }
            }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeSupplyMovements: supplyNeeded = " + SupplyNeeded());
#endif
            // If supply is still needed at this point and there are no available HQ's check to see if any HQ's can be landed
            if (SupplyNeeded())
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeSupplyMovements: all reinforcement ports - turn = " + GlobalDefinitions.turnNumber);
            foreach (GameObject port in GlobalDefinitions.availableReinforcementPorts)
                GlobalDefinitions.WriteToLogFile("makeSupplyMovements:      port - " + port.name);
#endif
                // The approach I'm going to use is to only land HQs on ports.  I'm not going to land them somewhere and try to get them to the port.
                foreach (GameObject port in GlobalDefinitions.availableReinforcementPorts)
                {
                    if (SupplyNeeded() && !port.GetComponent<HexDatabaseFields>().inGermanZOC)
                    {
                        if ((GlobalDefinitions.NumberHQOnHex(port) == 0) && !port.GetComponent<HexDatabaseFields>().successfullyInvaded)
                        {
                            GameObject hqUnit = ReturnAvailableHQUnit(1);
                            if (hqUnit != null)
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("makeSupplyMovements: landing hq unit " + hqUnit.name + " landing on port " + port.name);
#endif
                                port.GetComponent<HexDatabaseFields>().availableForMovement = true;
                                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(hqUnit, port, false);
                                hqUnit.GetComponent<UnitDatabaseFields>().hasMoved = true; // Don't want the unit wandering off during combat or movement
                            }
                        }
                    }
                }
            }

            // Check for adding to the current supply range
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeSupplyMovements: no supply needed, checking if supply range needs to be extended");
#endif
            float furthestUnit;
            // Loop through each of the supply sources and determine the furthest unit that it is supplying
            foreach (GameObject supplySource in GlobalDefinitions.supplySources)
            {
                furthestUnit = 0;
                foreach (GameObject unit in ReturnUnitsSupplied(supplySource))
                    if (CalculateDistance(supplySource, unit.GetComponent<UnitDatabaseFields>().occupiedHex) > furthestUnit)
                        furthestUnit = CalculateDistance(supplySource, unit.GetComponent<UnitDatabaseFields>().occupiedHex);

                // At this point we know how far away the furtheset unit is that this hex is supplying.  If it is less than 4 
                // hexes away from the current range add additional HQ units to extend the range

                int supplyRange = GlobalDefinitions.NumberHQOnHex(supplySource) * GlobalDefinitions.supplyRangeIncrement;
                if ((supplyRange == 0) && supplySource.GetComponent<HexDatabaseFields>().successfullyInvaded)
                    // If a hex is successfully invaded it automatically gets a range of GlobalDefinitions.supplyRangeIncrement hexes even without an HQ unit
                    supplyRange = GlobalDefinitions.supplyRangeIncrement;
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeSupplyMovements: supply source " + supplySource.name + " supply range = " + supplyRange + " furthest unit = " + furthestUnit);
#endif
                if ((int)furthestUnit > (supplyRange - 4))
                {
                    if (GlobalDefinitions.NumberHQOnHex(supplySource) < 3)
                    {
                        if (GlobalDefinitions.HexUnderStackingLimit(supplySource, GlobalDefinitions.Nationality.Allied) &&
                        (supplySource.GetComponent<HexDatabaseFields>().supplyCapacity > 4))
                        {
                            // Land an HQ on the hex
                            GameObject hqUnit = ReturnAvailableHQUnit(1);
                            if (hqUnit != null)
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("makeSupplyMovements: extending supply range, landing hq unit " + hqUnit.name + " landing on port " + supplySource.name);
#endif
                                supplySource.GetComponent<HexDatabaseFields>().availableForMovement = true;
                                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(hqUnit, supplySource, false);
                                hqUnit.GetComponent<UnitDatabaseFields>().hasMoved = true; // Don't want the unit wandering off during combat or movement
                            }
                        }
                    }
                    // This executes when the units are at the limit of the range of the supply source and there are already 3 HQ's on the hex
                    else
                    {

                    }
                }
            }

            // Go through all the ports and check to see if there needs to be a defender on the port other than the HQ unit
            // Defenders will only be added to hexes that have a supply capacity of at least 7 
            foreach (GameObject port in GlobalDefinitions.availableReinforcementPorts)
                if (port.GetComponent<HexDatabaseFields>().supplyCapacity > 6)
                    // Only check if not in a German ZOC.  Combat movement comes later.
                    if (!port.GetComponent<HexDatabaseFields>().inGermanZOC)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeSupplyMovements: checking if port " + port.name + " needs defenders added");
#endif

                        List<GameObject> nearbyEnemyUnits = new List<GameObject>();
                        int enemyAttackFactors = 0;
                        int numberDefendersNeeded = 1;
                        int numberDefendersPresent = 0;

                        nearbyEnemyUnits = FindNearbyEnemyUnits(port, GlobalDefinitions.Nationality.Allied, GlobalDefinitions.attackRange); // Nothing can attack if more than four hexes away

#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeSupplyMovements:      nearby enemy units count = " + nearbyEnemyUnits.Count);
#endif

                        // Successfully invaded hexes don't need a unit on them to be a reinforcement port
                        if (nearbyEnemyUnits.Count > 0)
                        {
                            // Check how many defenders are needed
                            foreach (GameObject enemyUnit in nearbyEnemyUnits)
                                enemyAttackFactors += enemyUnit.GetComponent<UnitDatabaseFields>().attackFactor;
                            if (enemyAttackFactors > 4)
                                numberDefendersNeeded = 2; // If more than four total factors need to put two units on the hex (if possible)

#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("makeSupplyMovements:      enemy attack factors = " + enemyAttackFactors);
#endif

                            foreach (GameObject tempUnit in port.GetComponent<HexDatabaseFields>().occupyingUnit)
                            {
                                // Set the hasMoved flag so the unit doesn't move off the hex
                                tempUnit.GetComponent<UnitDatabaseFields>().hasMoved = true;
                                if (!tempUnit.GetComponent<UnitDatabaseFields>().HQ)
                                    numberDefendersPresent++;
                            }

#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("makeSupplyMovements:      number of defenders present = " + numberDefendersPresent);
#endif

                            while (numberDefendersNeeded > numberDefendersPresent)
                            {
                                // The hex needs more defense on it and there are enemy units nearby, land an infantry unit
                                GameObject reinforcementUnit = ReturnAvailableInfantryUnit();
                                if (reinforcementUnit == null)
                                    reinforcementUnit = ReturnAvailableArmorUnit();

                                if (reinforcementUnit != null)
                                {
                                    if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnReinforcementLandingHexes(reinforcementUnit).Contains(port))
                                    {
                                        // Check if the hex is at its stacking limit
                                        if (GlobalDefinitions.HexUnderStackingLimit(port, GlobalDefinitions.Nationality.Allied))
                                        {
#if OUTPUTDEBUG
                                        GlobalDefinitions.WriteToLogFile("makeSupplyMovements: landing infantry unit " + reinforcementUnit.name + " for defense on port " + port.name);
#endif

                                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(reinforcementUnit, port, false);
                                            GlobalDefinitions.WriteToLogFile("makeSupplyMovements:  unit count on port = " + port.GetComponent<HexDatabaseFields>().occupyingUnit.Count);
                                            reinforcementUnit.GetComponent<UnitDatabaseFields>().hasMoved = true; // Don't want the unit wandering off during combat or movement

                                            GlobalDefinitions.WriteToLogFile("MakeSupplyMovements: port " + port.name + " under stacking limit, occupying unit count = " + port.GetComponent<HexDatabaseFields>().occupyingUnit.Count);
                                            numberDefendersPresent++;
                                        }
                                        else
                                        {
                                            GlobalDefinitions.WriteToLogFile("MakeSupplyMovements: port " + port.name + " over or at stacking limit, occupying unit count = " + port.GetComponent<HexDatabaseFields>().occupyingUnit.Count);

                                            numberDefendersPresent = numberDefendersNeeded;
                                        }
                                    }
                                    else
                                    {
#if OUTPUTDEBUG
                                    GlobalDefinitions.WriteToLogFile("makeSupplyMovements:      no reinforcement units available");
#endif

                                        numberDefendersPresent = numberDefendersNeeded; // No units are available so exit out
                                    }
                                }
                                else
                                {
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("makeSupplyMovements:      no reinforcement units available");
#endif

                                    numberDefendersPresent = numberDefendersNeeded; // No units are available so exit out
                                }
                            }
                        }
                    }

            // If supply is needed, need to see if there are open ports (not included in reinforcement ports) that can be occupied to increase supply.
            // If I don't do this here, early on in an invasion the units will be allocated to attacks and there won't be any to capture unoccupied hexes
            // when movement comes along.
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeSupplyMovements: supplyNeeded = " + SupplyNeeded());
#endif
            if (SupplyNeeded())
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeSupplyMovements: Check for additional supply ports available");
#endif
                // We've already checked all the ports with allied units on them so now check ports that don't have any units on them
                foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                    if ((hex.GetComponent<HexDatabaseFields>().coastalPort || hex.GetComponent<HexDatabaseFields>().inlandPort) &&
                        !hex.GetComponent<HexDatabaseFields>().inGermanZOC && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0))
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeSupplyMovements: Found an empty port " + hex.name);
#endif
                        List<GameObject> nearbyHexes = new List<GameObject>();
                        // This is an empty port.  Check to see if there is a reinforcement port within 7 hexes
                        // It is 7 hexes because all allied units move four hexes and it counts one to land so there are only 7 more moves remaining in strategic movement
                        nearbyHexes = ReturnHexesWithinDistance(hex.gameObject, 7);
                        foreach (GameObject reinforcementPort in GlobalDefinitions.availableReinforcementPorts)
                            if (nearbyHexes.Contains(reinforcementPort))
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("makeSupplyMovements: " + reinforcementPort.name + "is within range of " + hex.gameObject.name);
#endif
                                // There is a reinforcement port within movement range.  Land a unit and then see if movement is available

                                GameObject reinforcementUnit = ReturnAvailableInfantryUnit();
                                if (reinforcementUnit == null)
                                    reinforcementUnit = ReturnAvailableArmorUnit();

                                if (reinforcementUnit != null)
                                {
#if OUTPUTDEBUG
                                GlobalDefinitions.WriteToLogFile("makeSupplyMovements: landing unit " + reinforcementUnit.name);
#endif
                                    // We have a unit, now land it
                                    if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(reinforcementUnit, reinforcementPort, false))
                                    {
#if OUTPUTDEBUG
                                    GlobalDefinitions.WriteToLogFile("makeSupplyMovements:      unit landed");
#endif
                                        reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes =
                                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(reinforcementPort, reinforcementUnit);
                                        if (reinforcementUnit.GetComponent<UnitDatabaseFields>().availableMovementHexes.Contains(hex.gameObject))
                                            // The unit is available to move the unoccupied port
                                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(hex.gameObject, reinforcementPort, reinforcementUnit);
                                        else
                                        {
                                            // The unit can't make it to the port so send it back to Britain
                                            GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().DecrementInvasionUnitLimits(reinforcementUnit);
                                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(reinforcementPort, reinforcementUnit, false);
                                        }
                                    }
                                }
                            }
                    }
            }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeSupplyMovements: executing  number of Allied units on board = " + GlobalDefinitions.alliedUnitsOnBoard.Count);
        GlobalDefinitions.WriteToLogFile("makeSupplyMovements:      number of supply sources = " + GlobalDefinitions.supplySources.Count);
        foreach (GameObject hex in GlobalDefinitions.supplySources)
        {
            GlobalDefinitions.WriteToLogFile("makeSupplyMovements:          " + hex.name + " supply capacity = " + hex.GetComponent<HexDatabaseFields>().supplyCapacity + " supply excess = " + hex.GetComponent<HexDatabaseFields>().unassignedSupply);
            GlobalDefinitions.WriteToLogFile("makeSupplyMovements:              number of units = " + hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count + "  supply range = " + hex.GetComponent<HexDatabaseFields>().supplyRange);
        }
#endif
        }

        /// <summary>
        /// Used to assign combat for units that have been left in enemy ZOC
        /// </summary>
        /// <param name="nationality"></param>
        public static void SetDefaultAttacks(GlobalDefinitions.Nationality nationality)
        {
            List<GameObject> unitList;
            List<GameObject> unitsThatMustAttack = new List<GameObject>();
            List<GameObject> unitsThatMoved = new List<GameObject>();
            List<GameObject> movementHexes = new List<GameObject>();
            GlobalDefinitions.Nationality opposingNationality;
            bool unitHasMoved;

            if (nationality == GlobalDefinitions.Nationality.Allied)
            {
                unitList = GlobalDefinitions.alliedUnitsOnBoard;
                opposingNationality = GlobalDefinitions.Nationality.German;
            }
            else
            {
                unitList = GlobalDefinitions.germanUnitsOnBoard;
                opposingNationality = GlobalDefinitions.Nationality.Allied;
            }

            // Separate the units that are in enemy ZOC.  I will need them gathered together in order to see
            // if attacks should be combined.
            foreach (GameObject unit in unitList)
                if (GlobalDefinitions.HexInEnemyZOC(unit.GetComponent<UnitDatabaseFields>().occupiedHex, nationality)
                        && !unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    unitsThatMustAttack.Add(unit);

#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("setDefaultAttacks: units that either must move or attack");
        foreach (GameObject unit in unitsThatMustAttack)
            GlobalDefinitions.WriteToLogFile("setDefaultAttacks:    " + unit.name);
#endif
            // The first thing we will do is determine if the units can move away since the assumption is that the attacks were
            // already attempted and they did not meet minimum odds.
            foreach (GameObject unit in unitsThatMustAttack)
            {
                unitHasMoved = false;
                movementHexes = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
                foreach (GameObject hex in movementHexes)
                {
                    if (!unitHasMoved && GlobalDefinitions.HexUnderStackingLimit(hex, nationality) && !GlobalDefinitions.HexInEnemyZOC(hex, nationality))
                    {
                        if (hex.GetComponent<HexDatabaseFields>().sea)
                        {
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("setDefaultAttacks: " + unit.name + " Moving back to Britain");
#endif
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit, true);
                        }
                        else
                        {
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("setDefaultAttacks: " + unit.name + " Moving  to " + hex.name);
#endif
                            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(hex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
                        }
                        unitHasMoved = true;
                        unitsThatMoved.Add(unit);
                    }
                }
            }

            // Now remove all of the units that were able to move from the list if units that must attack
            foreach (GameObject unit in unitsThatMoved)
                unitsThatMustAttack.Remove(unit);

#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("setDefaultAttacks: units that must attack");
        foreach (GameObject unit in unitsThatMustAttack)
            GlobalDefinitions.WriteToLogFile("setDefaultAttacks:    " + unit.name);
#endif

            foreach (GameObject unit in unitsThatMustAttack)
            {
                // This executes if units are in a ZOC, they can't make a minimum odds attack and they can't move away.
                // I hit an issue, after literally years of testing, that the assignment of combats by attacking hex
                // against all hexes within which the hex lies in an enemy ZOC, caused the second hex to be evaluated 
                // without any defenders because the hex was already assigned combat by the first hex.  There are all
                // kinds of permutations that I can think of that could theoretically be an issue, but I don't think it's
                // not worth coding for since I have hit this once in years of testing (and the specific case I hit was resolved
                // by the code above that moves units away), so I'm just going to code that an attack with no defenders eliminates 
                // the attackers.  

                GameObject singleCombat = new GameObject("SingleCombat");
                singleCombat.AddComponent<Combat>();
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("setDefaultAttacks: adding a default attack");
#endif
                foreach (GameObject defendingUnit in unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().unitsExertingZOC)
                    if ((defendingUnit.GetComponent<UnitDatabaseFields>().nationality == opposingNationality)
                                && !defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("setDefaultAttacks:             Defender " + defendingUnit.name);
#endif
                        singleCombat.GetComponent<Combat>().defendingUnits.Add(defendingUnit);
                        defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
                    }

                foreach (GameObject attackingUnit in unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().occupyingUnit)
                    if (!attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("setDefaultAttacks:             Attacker " + attackingUnit.name);
#endif
                        singleCombat.GetComponent<Combat>().attackingUnits.Add(attackingUnit);
                        attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = true;
                    }
                // Check if there are defenders, if not, eliminate the attackers
                if (singleCombat.GetComponent<Combat>().defendingUnits.Count == 0)
                {
                    foreach (GameObject eliminateUnit in singleCombat.GetComponent<Combat>().attackingUnits)
                        GlobalDefinitions.MoveUnitToDeadPile(eliminateUnit);
                }
                else
                    GlobalDefinitions.allCombats.Add(singleCombat);
            }
        }

        /// <summary>
        /// Returns a list of units that is supplied by the source passed
        /// </summary>
        /// <param name="supppySource"></param>
        /// <returns></returns>
        public static List<GameObject> ReturnUnitsSupplied(GameObject supplySource)
        {
            List<GameObject> returnList = new List<GameObject>();
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                if (unit.GetComponent<UnitDatabaseFields>().supplySource == supplySource)
                    returnList.Add(unit);
            return (returnList);
        }

        /// <summary>
        /// Executes retreat options when the AI is defending and the attacker has to retreat
        /// </summary>
        /// <param name="retreatingUnits"></param>
        public static void ExecuteAIAback2(List<GameObject> retreatingUnits)
        {
            foreach (GameObject unitToRetreat in retreatingUnits)
            {
                List<GameObject> retreatHexes = CombatResolutionRoutines.ReturnRetreatHexes(unitToRetreat);
                int highScore = 0;
                GameObject bestRetreatHex = null;

                if (retreatHexes.Count == 0)
                {
                    // There are no hexes the unit can retreat to so it is eliminated
                    GlobalDefinitions.GuiUpdateStatusMessage("No retreat available - eliminating " + unitToRetreat.name);
                    GlobalDefinitions.MoveUnitToDeadPile(unitToRetreat);
                }
                else
                {
                    // Need to determine the lowest value hex to move the retreating unit to.  I'll rank them by hex value, number of units, and distance from current hex
                    foreach (GameObject hex in retreatHexes)
                    {
                        int score = 0;
                        // One point for being an open hex
                        if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                            score++;
                        // One point for being a land hex
                        if (hex.GetComponent<HexDatabaseFields>().hexValue == 1)
                            score++;
                        // One point for being two hexes away
                        if (!GlobalDefinitions.TwoHexesAdjacent(unitToRetreat.GetComponent<UnitDatabaseFields>().occupiedHex, hex))
                            score++;
                        if (score >= highScore)
                        {
                            highScore = score;
                            bestRetreatHex = hex;
                        }
                    }
                    // Move the unit to the best hex
                    if (bestRetreatHex != null)
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(bestRetreatHex, unitToRetreat.GetComponent<UnitDatabaseFields>().occupiedHex, unitToRetreat);
                    else
                        // Not sure why this would ever happen
                        GlobalDefinitions.GuiUpdateStatusMessage("Encounted a problem with executing retreat for unit " + unitToRetreat.name);
                }
            }
            GlobalDefinitions.dback2Attackers.Clear();
            GlobalDefinitions.dback2Defenders.Clear();
            GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
        }

        /// <summary>
        /// Executes retreat options when the AI is attacking and the defender has to retreat
        /// </summary>
        /// <param name="retreatingUnits"></param>
        public static void ExecuteAIDback2(List<GameObject> retreatingUnits)
        {
            foreach (GameObject unitToRetreat in retreatingUnits)
            {
                List<GameObject> retreatHexes = CombatResolutionRoutines.ReturnRetreatHexes(unitToRetreat);
                int highScore = 0;
                GameObject bestRetreatHex = null;
                if (retreatHexes.Count == 0)
                {
                    // There are no hexes the unit can retreat to so it is eliminated
                    GlobalDefinitions.GuiUpdateStatusMessage("No retreat available - eliminating " + unitToRetreat.name);
                    GlobalDefinitions.MoveUnitToDeadPile(unitToRetreat);
                }
                else
                {
                    // Need to determine the lowest value hex to move the retreating unit to.  I'll rank them by hex value, number of units, and distance from current hex
                    foreach (GameObject hex in retreatHexes)
                    {
                        int score = 0;
                        // One point for being an open hex
                        if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                            score++;
                        // One point for being a land hex
                        if (hex.GetComponent<HexDatabaseFields>().hexValue == 1)
                            score++;
                        // One point for being two hexes away
                        if (!GlobalDefinitions.TwoHexesAdjacent(unitToRetreat.GetComponent<UnitDatabaseFields>().occupiedHex, hex))
                            score++;
                        if (score >= highScore)
                        {
                            highScore = score;
                            bestRetreatHex = hex;
                        }
                    }
                    // Move the unit to the best hex
                    if (bestRetreatHex != null)
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(bestRetreatHex, unitToRetreat.GetComponent<UnitDatabaseFields>().occupiedHex, unitToRetreat);
                    else
                        // Not sure why this would ever happen
                        GlobalDefinitions.GuiUpdateStatusMessage("Encounted a problem with executing retreat for unit " + unitToRetreat.name);
                }
            }

            // Check if the attacking units can occupy the defending hex
            if (GlobalDefinitions.hexesAvailableForPostCombatMovement.Count > 0)
                ExecuteAIPostCombatMovement(GlobalDefinitions.dback2Attackers);

            GlobalDefinitions.dback2Attackers.Clear();
            GlobalDefinitions.dback2Defenders.Clear();
            GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
        }

        /// <summary>
        /// Determines if any of the units passed should occupy hexes available for post combat advance
        /// </summary>
        /// <param name="unitList"></param>
        public static void ExecuteAIPostCombatMovement(List<GameObject> unitList)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("executeAIPostCombatMovement: executing unit count = " + unitList.Count);
        foreach (GameObject unit in unitList)
            GlobalDefinitions.WriteToLogFile("executeAIPostCombatMovement:      " + unit.name);
#endif
            GameObject tempUnit;
            // Sort the unit list so that the higher defense factored units are moved first
            for (int i = 0; i < unitList.Count; i++)
                for (int j = (i + 1); j < unitList.Count; j++)
                    if (unitList[i].GetComponent<UnitDatabaseFields>().defenseFactor < unitList[j].GetComponent<UnitDatabaseFields>().defenseFactor)
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }

            foreach (GameObject hex in GlobalDefinitions.hexesAvailableForPostCombatMovement)
                foreach (GameObject unit in unitList)
                {
                    // If the hex has at least the same intrinsic value as what the unit is occupying then the bias is to take advantage of post combat movement
                    // Breaking through a defensive line on a river is dependent on getting units across the river
                    if ((GlobalDefinitions.HexUnderStackingLimit(hex, unitList[0].GetComponent<UnitDatabaseFields>().nationality)) &&
                            (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().intrinsicHexValue <= hex.GetComponent<HexDatabaseFields>().intrinsicHexValue))
                    {
                        // Check if the unit is moving off a sea hex which indicates that the hex being occupied should be flagged as a successful invasion
                        if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea)
                            hex.GetComponent<HexDatabaseFields>().successfullyInvaded = true;
                        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(hex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
                    }
                }
        }

        /// <summary>
        /// Decides which attacking units will be exchanged
        /// </summary>
        /// <param name="attackingUnits"></param>
        /// <param name="defendingUnits"></param>
        public static void ExecuteAIExchangeForAttackingUnits(List<GameObject> attackingUnits, List<GameObject> defendingUnits)
        {
            int exchangeFactorsToLose = CalculateBattleOddsRoutines.CalculateDefenseFactorWithoutAirSupport(defendingUnits, attackingUnits);
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("executeAIExchangeForAttackingUnits: need to eliminate factors total = " + exchangeFactorsToLose);
#endif
            float exchangeFactorsLost = 0f;
            List<GameObject> unitsToDelete = new List<GameObject>();

            // Which units I want to exchange is dependent on the nationality so sort based on nationality
            if (attackingUnits[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
                SortForExchangeGerman(attackingUnits);
            else
                SortForExchangeAllied(attackingUnits);

            foreach (GameObject unit in attackingUnits)
                if (exchangeFactorsLost < exchangeFactorsToLose)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("executeAIExchangeForAttackingUnits: adding unit " + unit.name + " with attack factor " + GlobalDefinitions.ReturnAttackFactor(unit) + " total lost factors = " + exchangeFactorsLost);
#endif
                    exchangeFactorsLost += CalculateBattleOddsRoutines.ReturnAttackFactor(unit);
                    unitsToDelete.Add(unit);
                }

            foreach (GameObject unit in unitsToDelete)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("executeAIExchangeForAttackingUnits: eliminating attacking unit " + unit.name + " attack factor = " + unit.GetComponent<UnitDatabaseFields>().attackFactor);
#endif
                GlobalDefinitions.MoveUnitToDeadPile(unit);
                attackingUnits.Remove(unit);
            }

            foreach (GameObject unit in defendingUnits)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("executeAIExchangeForAttackingUnits: eliminating defending unit " + unit.name + " defense factor = " + unit.GetComponent<UnitDatabaseFields>().defenseFactor);
#endif
                GlobalDefinitions.MoveUnitToDeadPile(unit);
            }

            if ((GlobalDefinitions.hexesAvailableForPostCombatMovement.Count > 0) && (attackingUnits.Count > 0))
                ExecuteAIPostCombatMovement(attackingUnits);
        }

        /// <summary>
        /// Decides which attacking units will be exchanged
        /// </summary>
        /// <param name="attackingUnits"></param>
        /// <param name="defendingUnits"></param>
        public static void ExecuteAIExchangeForDefendingUnits(List<GameObject> attackingUnits, List<GameObject> defendingUnits)
        {
            int exchangeFactorsToLose = (int)CalculateBattleOddsRoutines.CalculateAttackFactorWithoutAirSupport(attackingUnits);
            int exchangeFactorsLost = 0;
            List<GameObject> unitsToDelete = new List<GameObject>();

            // Which units I want to exchange is dependent on the nationality so sort based on nationality
            if (defendingUnits[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
                SortForExchangeGerman(defendingUnits);
            else
                SortForExchangeAllied(defendingUnits);

            foreach (GameObject unit in defendingUnits)
                if (exchangeFactorsLost < exchangeFactorsToLose)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("executeAIExchangeForDefendingUnits: adding unit " + unit.name + " with defense factor " + GlobalDefinitions.CalculateUnitDefendingFactor(unit, attackingUnits) + " total lost factors = " + exchangeFactorsLost);
#endif
                    exchangeFactorsLost += CalculateBattleOddsRoutines.CalculateUnitDefendingFactor(unit, attackingUnits);
                    unitsToDelete.Add(unit);
                }

            foreach (GameObject unit in unitsToDelete)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("executeAIExchangeForDefendingUnits: eliminating defending unit " + unit.name + " defense factor = " + unit.GetComponent<UnitDatabaseFields>().defenseFactor);
#endif
                GlobalDefinitions.MoveUnitToDeadPile(unit);
            }

            foreach (GameObject unit in attackingUnits)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("executeAIExchangeForDefendingUnits: eliminating attacking unit " + unit.name + " attack factor = " + unit.GetComponent<UnitDatabaseFields>().attackFactor);
#endif
                GlobalDefinitions.MoveUnitToDeadPile(unit);
            }
        }

        /// <summary>
        /// Sort the unit list passed based on which units should be exchanged first for German units
        /// </summary>
        /// <param name="unitList"></param>
        private static void SortForExchangeGerman(List<GameObject> unitList)
        {
            // Sort the most desirable units for exchange to be at the beginning of the list
            //  HQ, Static, Infantry, Airborne, Armor
            // Within each type of unit sort by lowes to highest attack factor

            GameObject tempUnit;
            int index1 = 0;
            int index2 = 0;
            int startIndex = 0;

            // Move HQ units to the front of the list
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().HQ))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().HQ)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Next are static units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().germanStatic))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().germanStatic)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            startIndex = index1;
            // Next are infantry units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().infantry))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().infantry)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the infantry units from lowest factor to highest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                    if (unitList[i].GetComponent<UnitDatabaseFields>().attackFactor > unitList[j].GetComponent<UnitDatabaseFields>().attackFactor)
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }

            startIndex = index1;
            // Next are airborne units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().airborne))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().airborne)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the airborne units from lowest factor to highest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                    if (unitList[i].GetComponent<UnitDatabaseFields>().attackFactor > unitList[j].GetComponent<UnitDatabaseFields>().attackFactor)
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }

            startIndex = index1;

            //Armor units should be at the end of the list by now

            // Sort the armor units from lowest factor to highest
            for (int i = startIndex; i < unitList.Count; i++)
                for (int j = i + 1; j < unitList.Count; j++)
                    if (unitList[i].GetComponent<UnitDatabaseFields>().attackFactor > unitList[j].GetComponent<UnitDatabaseFields>().attackFactor)
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("sortForExchangeGerman: list sorted - count = " + unitList.Count);
        for (int index = 0; index < unitList.Count; index++)
            GlobalDefinitions.WriteToLogFile("sortForExchangeGerman:    index = " + index + " " + unitList[index].name + " attack factor = " + unitList[index].GetComponent<UnitDatabaseFields>().attackFactor);
#endif
        }

        /// <summary>
        /// Sort the unit list passed based on which units should be exchanged first for Allied units
        /// </summary>
        /// <param name="unitList"></param>
        private static void SortForExchangeAllied(List<GameObject> unitList)
        {
            // Sort the most desirable units for exchange to be at the beginning of the list
            //  Infantry, Armor, Airborne, HQ
            // Within each type of unit sort by lowes to highest attack factor

            GameObject tempUnit;
            int index1 = 0;
            int index2 = 0;
            int startIndex = 0;

            // First get infantry units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().infantry))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().infantry)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the infantry units from lowest factor to highest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                    if (unitList[i].GetComponent<UnitDatabaseFields>().attackFactor > unitList[j].GetComponent<UnitDatabaseFields>().attackFactor)
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }

            startIndex = index1;
            // Next are armor units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().armor))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().armor)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the armor units from lowest factor to highest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                    if (unitList[i].GetComponent<UnitDatabaseFields>().attackFactor > unitList[j].GetComponent<UnitDatabaseFields>().attackFactor)
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }

            startIndex = index1;
            // Next are airborne units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().airborne))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().airborne)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the airborne units from lowest factor to highest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                    if (unitList[i].GetComponent<UnitDatabaseFields>().attackFactor > unitList[j].GetComponent<UnitDatabaseFields>().attackFactor)
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }

            // HQ units should be at the end of the list by now, they don't need to be sorted
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("sortForExchangeAllied: list sorted - count = " + unitList.Count);
        for (int index = 0; index < unitList.Count; index++)
            GlobalDefinitions.WriteToLogFile("sortForExchangeAllied:    index = " + index + " " + unitList[index].name);
#endif
        }

        /// <summary>
        /// Makes German reinforcement moves
        /// </summary>
        public static void MakeGermanReinforcementMoves()
        {
            List<GameObject> closeUnits = new List<GameObject>();

            // Reset the group lists
            foreach (List<GameObject> list in GlobalDefinitions.alliedGroups)
                list.Clear();
            GlobalDefinitions.alliedGroups.Clear();

            foreach (List<GameObject> list in GlobalDefinitions.germanGroups)
                list.Clear();
            GlobalDefinitions.germanGroups.Clear();
            GlobalDefinitions.germanReserves.Clear();

            // Reset the group numbers on all of the units
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                unit.GetComponent<UnitDatabaseFields>().groupNumber = -1;
            foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
                unit.GetComponent<UnitDatabaseFields>().groupNumber = -1;

            int groupNumber = -1;
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            {
                if (unit.GetComponent<UnitDatabaseFields>().groupNumber == -1)
                {
                    groupNumber++;
                    unit.GetComponent<UnitDatabaseFields>().groupNumber = groupNumber;
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: adding new group " + groupNumber + " unit = " + unit.name);
#endif
                    GlobalDefinitions.alliedGroups.Add(new List<GameObject>());
                    GlobalDefinitions.germanGroups.Add(new List<GameObject>());
                    GlobalDefinitions.alliedGroups[groupNumber].Add(unit);

                    for (int index = 0; index < GlobalDefinitions.alliedGroups[groupNumber].Count; index++)
                    {
                        closeUnits = FindNearbyUnits(GlobalDefinitions.alliedGroups[groupNumber][index].GetComponent<UnitDatabaseFields>().occupiedHex, GlobalDefinitions.Nationality.Allied, GlobalDefinitions.groupRange);
                        foreach (GameObject closeUnit in closeUnits)
                        {
                            closeUnit.GetComponent<UnitDatabaseFields>().groupNumber = groupNumber;
                            if (!GlobalDefinitions.alliedGroups[groupNumber].Contains(closeUnit))
                            {
#if OUTPUTDEBUG
                            GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: Unit " + closeUnit.name + " being added to group " + groupNumber);
#endif
                                GlobalDefinitions.alliedGroups[groupNumber].Add(closeUnit);
                            }
                        }

                    }
                }
            }
#if OUTPUTDEBUG
        foreach (List<GameObject> list in GlobalDefinitions.alliedGroups)
        {
            GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: Allied Group count = " + list.Count);
            foreach (GameObject unit in list)
                GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves:         " + unit.name + " group " + unit.GetComponent<UnitDatabaseFields>().groupNumber);
        }
#endif
            // At this point all of the Allied units on the board have been assigned a group number

            // Now go through all of the German units and assign group numbers based on the Allied group they can attack
            foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
            {
                closeUnits = FindNearbyEnemyUnits(unit.GetComponent<UnitDatabaseFields>().occupiedHex, GlobalDefinitions.Nationality.German, (unit.GetComponent<UnitDatabaseFields>().movementFactor + 1));

                List<int> differentGroups = new List<int>();

                foreach (GameObject closeUnit in closeUnits)
                    if (!differentGroups.Contains(closeUnit.GetComponent<UnitDatabaseFields>().groupNumber))
                        differentGroups.Add(closeUnit.GetComponent<UnitDatabaseFields>().groupNumber);

                // The unit isn't near an enemy unit
                if (differentGroups.Count == 0)
                {
                    // Check that if a second invasion has taken place.  If not, any static or infantry unit on invasion hex that is west
                    // of the first invasion point shouldn't move so it will be not be added to the reserves
                    if ((GlobalDefinitions.secondInvasionAreaIndex == -1) &&
                            (unit.GetComponent<UnitDatabaseFields>().germanStatic || unit.GetComponent<UnitDatabaseFields>().HQ ||
                            unit.GetComponent<UnitDatabaseFields>().infantry) &&
                            (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().coast ||
                            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().inlandPort ||
                            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().coastalPort) &&
                            (GlobalDefinitions.firstInvasionAreaIndex < unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionAreaIndex))
                        unit.GetComponent<UnitDatabaseFields>().hasMoved = true;
                    else if (!GlobalDefinitions.germanReserves.Contains(unit))
                        GlobalDefinitions.germanReserves.Add(unit);
                }

                // The unit is near enemy units of only one group
                else if (differentGroups.Count == 1)
                {
                    if (!GlobalDefinitions.germanGroups[differentGroups[0]].Contains(unit))
                        GlobalDefinitions.germanGroups[differentGroups[0]].Add(unit);
                    unit.GetComponent<UnitDatabaseFields>().groupNumber = differentGroups[0];
                }

                // The unit can attack units from more than one group
                else
                {
                    // Add it to each of the groups that it can attack
                    foreach (int index in differentGroups)
                    {
                        if (GlobalDefinitions.germanGroups[index].Contains(unit))
                            GlobalDefinitions.germanGroups[index].Add(unit);
                        unit.GetComponent<UnitDatabaseFields>().groupNumber = index;
                    }

                }

            }
#if OUTPUTDEBUG
        foreach (List<GameObject> list in GlobalDefinitions.germanGroups)
        {
            GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: German Group count = " + list.Count);
            foreach (GameObject unit in list)
                GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves:         " + unit.name + " group " + unit.GetComponent<UnitDatabaseFields>().groupNumber);
        }

        GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: German Reserve Group count = " + GlobalDefinitions.germanReserves.Count);
        foreach (GameObject unit in GlobalDefinitions.germanReserves)
            GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves:         " + unit.name);
#endif
            // At this point all of the German units have been assigned to a group(s) or to the reserve if they aren't within attack range

            List<int> attackFactors = new List<int>();
            List<int> defenseFactors = new List<int>();

            for (int groupIndex = 0; groupIndex < GlobalDefinitions.alliedGroups.Count; groupIndex++)
            {
                int totalAttackFactors = 0;
                foreach (GameObject unit in GlobalDefinitions.alliedGroups[groupIndex])
                    totalAttackFactors += unit.GetComponent<UnitDatabaseFields>().attackFactor;
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: group " + groupIndex + " attack factors = " + totalAttackFactors);
#endif
                attackFactors.Add(totalAttackFactors);
            }

            for (int groupIndex = 0; groupIndex < GlobalDefinitions.germanGroups.Count; groupIndex++)
            {
                int totalDefenseFactors = 0;
                foreach (GameObject unit in GlobalDefinitions.germanGroups[groupIndex])
                    totalDefenseFactors += unit.GetComponent<UnitDatabaseFields>().defenseFactor;
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: group " + groupIndex + " defense factors = " + totalDefenseFactors);
#endif
                defenseFactors.Add(totalDefenseFactors);
            }

            // Go through all of the groups and move any excess factors in any group to the reserve
            for (int index = 0; index < GlobalDefinitions.germanGroups.Count; index++)
            {
                // This is a case where there are excess defenders.  Add the excess to the reserve list
                if ((attackFactors[index] - defenseFactors[index]) < 0)
                {
                    List<GameObject> removeList = new List<GameObject>();
                    int factorsMoved = 0;

                    // Sort the units in the group by largest defense factor to smallest
                    GlobalDefinitions.germanGroups[index].Sort((b, a) => a.GetComponent<UnitDatabaseFields>().defenseFactor.CompareTo(b.GetComponent<UnitDatabaseFields>().defenseFactor));

                    // Keep moving units to the reserve list until factors are even
                    foreach (GameObject unit in GlobalDefinitions.germanGroups[index])
                        if ((factorsMoved < (defenseFactors[index] - attackFactors[index])) && !GlobalDefinitions.germanReserves.Contains(unit))
                        {
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: Adding " + unit.name + " to reserve group");
#endif
                            GlobalDefinitions.germanReserves.Add(unit);
                            factorsMoved += unit.GetComponent<UnitDatabaseFields>().defenseFactor;
                            removeList.Add(unit); // Note that I'm not doing anything with this because I don't think it is important to remove from the group but ...
                        }
                }
            }

            // At this point all the excess factors in any group have been moved to the reserve group.
            // Now go through each of the groups and assign units in the reserve those areas needing additional factors
            for (int index = 0; index < GlobalDefinitions.germanGroups.Count; index++)
            {
                if ((attackFactors[index] - defenseFactors[index]) > 0)
                {
                    int totalFactorsMoved = 0;
                    GameObject targetHex = ReturnGermanReinforcementTarget(groupNumber);
                    // Sort the units
                    SortGermanReinforcementUnits(GlobalDefinitions.germanReserves, index);
                    foreach (GameObject unit in GlobalDefinitions.germanReserves)
                        if (totalFactorsMoved < (attackFactors[index] - defenseFactors[index]))
                        {
#if OUTPUTDEBUG
                        GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: Reserve unit " + unit.name + " being moved to target " + targetHex.name);
#endif
                            ExecuteGermanReinforcementMovement(unit, targetHex);
                            totalFactorsMoved += unit.GetComponent<UnitDatabaseFields>().defenseFactor;
                        }

                }
            }

            // Movements for the groups has been done.  Need to look at the remaining strategic units and determine if they should move

            // First remove any unit with a group assigned since these are not really strategic they were placed here because they were excess in their group
            List<GameObject> unitsToRemove = new List<GameObject>();

            foreach (GameObject unit in GlobalDefinitions.germanReserves)
                if (unit.GetComponent<UnitDatabaseFields>().groupNumber != -1)
                    unitsToRemove.Add(unit);

#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: removing committed units in reserve - count = " + unitsToRemove.Count);
#endif
            foreach (GameObject unit in unitsToRemove)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves:         " + unit.name);
#endif
                GlobalDefinitions.germanReserves.Remove(unit);
            }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: German reinforcement status after group reinforcement - count = " + GlobalDefinitions.germanReserves.Count);
        foreach (GameObject unit in GlobalDefinitions.germanReserves)
            GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves:     " + unit.name + " hasMoved = " + unit.GetComponent<UnitDatabaseFields>().hasMoved);

        GlobalDefinitions.WriteToLogFile("");
        GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: Making strategic moves");
#endif
            foreach (GameObject unit in GlobalDefinitions.germanReserves)
                if (!unit.GetComponent<UnitDatabaseFields>().hasMoved)
                {
                    // Note that the list is sorted by unit type
                    GameObject tempHex, minHex = null;
                    float minDistance = float.MaxValue;
                    float distance;

#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: Moving unit " + unit.name);
#endif
                    // Determine which group is closest and move the unit to that target
                    for (int index = 0; index < GlobalDefinitions.germanGroups.Count; index++)
                    {
                        tempHex = ReturnGermanReinforcementTarget(index);
                        distance = CalculateDistance(unit.GetComponent<UnitDatabaseFields>().occupiedHex, tempHex);
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves:     index = " + index + " distance = " + distance + " hex = " + tempHex.name);
#endif
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            minHex = tempHex;
                        }
                    }

                    // If minHex is null that means that there aren't any Allied units on the board.  Move north???
                    if (minHex == null)
                    {
                        MoveGermanReinforcementWithNoAlliedUnits(unit);
                    }
                    else
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("makeGermanReinforcementMoves: moving to hex " + minHex.name);
#endif
                        ExecuteGermanReinforcementMovement(unit, minHex);
                    }
                }
        }

        /// <summary>
        /// Executes when there are no Allied units on the board so moves reinforement units directly behind Pas De Calais
        /// </summary>
        /// <param name="unit"></param>
        private static void MoveGermanReinforcementWithNoAlliedUnits(GameObject unit)
        {
            unit.GetComponent<UnitDatabaseFields>().availableMovementHexes =
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);

            // Find the available hex that is the closest to hex (19,17)
            GameObject targetHex = GeneralHexRoutines.GetHexAtXY(12, 22);
            float closestDistance = float.MaxValue;
            GameObject closestHex = null;

            foreach (GameObject hex in unit.GetComponent<UnitDatabaseFields>().availableMovementHexes)
                if (GlobalDefinitions.HexUnderStackingLimit(hex, GlobalDefinitions.Nationality.German) && (closestDistance > CalculateDistance(hex, targetHex)))
                {
                    closestDistance = CalculateDistance(hex, targetHex);
                    closestHex = hex;
                }

            if (closestHex == null)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("moveGermanReinforcementWithNoAlliedUnits: unit " + unit.name + " null furthest north hex");
#endif
                unit.GetComponent<UnitDatabaseFields>().hasMoved = true; // It can't move any closer leave it alone
            }
            else
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("moveGermanReinforcementWithNoAlliedUnits: unit " + unit.name + " moving to " + closestHex.name);
#endif
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(closestHex, unit.GetComponent<UnitDatabaseFields>().occupiedHex, unit);
            }

        }

        /// <summary>
        /// Sorts the passed list by armor, airborne, infantry, static and then hq
        /// It sorts each type by nearness to the group passed
        /// </summary>
        /// <param name="unitList"></param>
        /// <param name="groupNumber"></param>
        private static void SortGermanReinforcementUnits(List<GameObject> unitList, int groupNumber)
        {
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits: unitList count = " + unitList.Count);
        foreach (GameObject unit in unitList)
            GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits:         " + unit.name);
#endif
            GameObject tempUnit;
            GameObject targetHex = ReturnGermanReinforcementTarget(groupNumber);
            int index1 = 0;
            int index2 = 0;
            int startIndex = 0;
#if OUTPUTDEBUG
        //GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits: target hex = " + targetHex.name);
#endif
            // First are armor units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().armor))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().armor)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the armor units from closest to furthest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits: sorting armor units i = " + i + " j = " + j);
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits:     i unit = " + unitList[i].name + " j unit = " + unitList[j].name);
#endif
                    if (CalculateDistance(unitList[i].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex) > CalculateDistance(unitList[j].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex))
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }
                }

            startIndex = index1;
            // Next are airborne units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().airborne))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().airborne)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the airborne units from closest to furthest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits: sorting airborne units i = " + i + " j = " + j);
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits:     i unit = " + unitList[i].name + " j unit = " + unitList[j].name);
#endif
                    if (CalculateDistance(unitList[i].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex) > CalculateDistance(unitList[j].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex))
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }
                }

            startIndex = index1;
            // Next are infantry units
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().infantry))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().infantry)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the infantry units from closest to furthest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits: sorting infantry units i = " + i + " j = " + j);
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits:     i unit = " + unitList[i].name + " j unit = " + unitList[j].name);
#endif
                    if (CalculateDistance(unitList[i].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex) > CalculateDistance(unitList[j].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex))
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }
                }

            // Next are static units
            startIndex = index1;
            while ((index1 < unitList.Count) && (unitList[index1].GetComponent<UnitDatabaseFields>().germanStatic))
                index1++;
            index2 = index1 + 1;
            while (index2 < unitList.Count)
            {
                if (unitList[index2].GetComponent<UnitDatabaseFields>().germanStatic)
                {
                    tempUnit = unitList[index1];
                    unitList[index1] = unitList[index2];
                    unitList[index2] = tempUnit;
                    index1++;
                }
                index2++;
            }

            // Sort the static units from closest to furthest
            for (int i = startIndex; i < (index1 - 1); i++)
                for (int j = i + 1; j < (index1 - 1); j++)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits: sorting static units i = " + i + " j = " + j);
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits:     i unit = " + unitList[i].name + " j unit = " + unitList[j].name);
#endif
                    if (CalculateDistance(unitList[i].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex) > CalculateDistance(unitList[j].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex))
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }
                }


            // What should be left are HQ units, sort them
            startIndex = index1;
            for (int i = startIndex; i < unitList.Count; i++)
                for (int j = i + 1; j < unitList.Count; j++)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits: sorting hq units i = " + i + " j = " + j);
                GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits:     i unit = " + unitList[i].name + " j unit = " + unitList[j].name);
#endif
                    if (CalculateDistance(unitList[i].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex) > CalculateDistance(unitList[j].GetComponent<UnitDatabaseFields>().occupiedHex, targetHex))
                    {
                        tempUnit = unitList[i];
                        unitList[i] = unitList[j];
                        unitList[j] = tempUnit;
                    }
                }

#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits: list sorted - count = " + unitList.Count);
        for (int index = 0; index < unitList.Count; index++)
            GlobalDefinitions.WriteToLogFile("sortGermanReinforcementUnits:    index = " + index + " " + unitList[index].name + " attack factor = " + unitList[index].GetComponent<UnitDatabaseFields>().attackFactor);
#endif
        }

        /// <summary>
        /// Returns the target hex for the group number passed
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <returns></returns>
        private static GameObject ReturnGermanReinforcementTarget(int groupNumber)
        {
            List<GameObject> targets = new List<GameObject>();
            int furthestWest = 0;

            // The target is determined by the unit in the group that is the furthest west (which equates to the highest y coordinate)
            // I am assuming that for South France the reinforcement units will run into advancing units before they end up in the mountains
            // In case of ties I will use the furtest north (which is the lowest x coordinate)
            foreach (GameObject unit in GlobalDefinitions.alliedGroups[groupNumber])
            {
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor > furthestWest)
                {
                    targets.Clear();
                    targets.Add(unit);
                    furthestWest = unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor;
                }
                else if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor == furthestWest)
                {
                    targets.Add(unit);
                }
            }

            if (targets.Count == 0)
            {
                // This should never happen
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnGermanReinforcementTarget: ERROR target count = 0");
#endif
                return (null);
            }
            else if (targets.Count == 1)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnGermanReinforcementTarget: single choice - group number = " + groupNumber + "  target = " + targets[0].name + " hex = " + targets[0].GetComponent<UnitDatabaseFields>().occupiedHex.name);
#endif
                return (targets[0].GetComponent<UnitDatabaseFields>().occupiedHex);
            }
            else
            {
                // Determine which of the hexes is the furthese north (the smallest x coordinate)
                int furthestNorth = 100;
                foreach (GameObject unit in targets)
                    if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().xMapCoor < furthestNorth)
                        furthestNorth = unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().xMapCoor;

                foreach (GameObject unit in targets)
                    if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().xMapCoor == furthestNorth)
                    {
#if OUTPUTDEBUG
                    GlobalDefinitions.WriteToLogFile("returnGermanReinforcementTarget: multiple choice - group number = " + groupNumber + "  target = " + unit.name + " hex = " + unit.GetComponent<UnitDatabaseFields>().occupiedHex.name);
#endif
                        return (unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                    }
            }
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnGermanReinforcementTarget: ERROR returning null for group " + groupNumber);
#endif
            return (null);
        }

        public static Vector2 ReturnAlliedAverageLocationForInvasionArea(int invasionAreaIndex)
        {
            int totalX = 0;
            int totalY = 0;
            int totalNumber = 0;
#if OUTPUTDEBUG
        GlobalDefinitions.WriteToLogFile("returnAlliedAverageLocationForInvasionArea: number of allied units on board = " + GlobalDefinitions.alliedUnitsOnBoard.Count);
#endif
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            {
#if OUTPUTDEBUG
            GlobalDefinitions.WriteToLogFile("returnAlliedAverageLocationForInvasionArea: unit " + unit.name + " invasion area index = " + unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex);
#endif
                if (unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex == invasionAreaIndex)
                {
#if OUTPUTDEBUG
                GlobalDefinitions.WriteToLogFile("returnAlliedAverageLocationForInvasionArea: unit " + unit.name);
#endif
                    totalX += unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().xMapCoor;
                    totalY += unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().yMapCoor;
                    totalNumber++;
                }
            }
            if (totalNumber == 0)
                // There aren't any enemy units on the map
                return (new Vector2(0, 0));
            else
                return (new Vector2(totalX / totalNumber, totalY / totalNumber));
        }

        /// <summary>
        /// This routine adds carpet bombing to AI combats
        /// </summary>
        public static void CheckForAICarpetBombingShouldBeAdded()
        {
            // Go through each of the combats that are pending and see if there are 1:2 combats in place that carpet bombing can be added to
            // I can make it better but I'm going to stop at the first one I find for now instead of weighing them off
            foreach (GameObject combat in GlobalDefinitions.allCombats)
            {
                if ((CalculateBattleOddsRoutines.ReturnCombatOdds(combat.GetComponent<Combat>().defendingUnits, combat.GetComponent<Combat>().attackingUnits, combat.GetComponent<Combat>().attackAirSupport) == -2) &&
                        CombatRoutines.CheckIfCarpetBombingIsAvailable(combat))
                {
                    GlobalDefinitions.carpetBombingUsedThisTurn = true;
                    GlobalDefinitions.numberOfCarpetBombingsUsed++;
                    combat.GetComponent<Combat>().defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().carpetBombingActive = true;
                    combat.GetComponent<Combat>().carpetBombing = true;
                    return;
                }
            }
        }
    }

    public class AIPotentialAttack
    {
        public List<GameObject> defendingUnits = new List<GameObject>();
        public List<GameObject> attackingUnits = new List<GameObject>();
        public List<AIDefendHex> defendingHexes = new List<AIDefendHex>();
        public bool addAirSupport = false;
        public int odds;
    }

    public class AIDefendHex
    {
        // The defending hex
        //public GameObject defendingHex = new GameObject("AIDefendHex");
        public GameObject defendingHex;

        // Hexes that can attack the hex without involving other defending hexes
        public List<AISingleAttackHex> singleAttackHexes;

        // Hexes that can attack the hex but involved other defending units
        public List<AIMultipleAttackHex> multipleAttackHexes;
    }

    // This class is used by the AI to keep track of the potential attackers for a hex
    public class AISingleAttackHex
    {
        //public GameObject attackHex = new GameObject("AISingleAttackHex");
        public GameObject attackHex;
        public List<GameObject> potentialAttackers = new List<GameObject>();
    }

    // This class expands on the single attack hex to include the other hexes it would add
    public class AIMultipleAttackHex : AISingleAttackHex
    {
        public List<GameObject> additionalDefendingHexes = new List<GameObject>();
    }

    public class AIInvasionDefendHex : AIDefendHex
    {
        public int supplyFactors;
        public int odds;
    }
}