using System;
using TheGreatCrusade;
using UnityEngine;

namespace CommonRoutines
{
    public class GeneralHexRoutines : MonoBehaviour
    {

        /// <summary>
        /// This routine returns true if there is a river between the two hexes passed
        /// </summary>
        /// <param name="attackingHex"></param>
        /// <param name="defendingHex"></param>
        /// <returns></returns>
        public static bool CheckForRiverBetweenTwoHexes(GameObject attackingHex, GameObject defendingHex)
        {
            // Note that this routine does not assume that the two hexes are neighbors but if they aren't a false will be returned
            foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                if ((attackingHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null) &&
                        (attackingHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] == defendingHex) &&
                        (attackingHex.GetComponent<BooleanArrayData>().riverSides[(int)hexSide] == true))
                    return (true);

            return (false);
        }

        /// <summary>
        /// This routine will get a hex selected by the user
        /// </summary>
        /// <returns></returns>
        public static GameObject GetHexFromUserInput(Vector2 mousePosition)
        {
            RaycastHit selectionHit = new RaycastHit();

            if (Input.GetMouseButtonDown(0))
            {
                //  Note in the call below, 256 is used to only hit hexes on layer 8 (the 9th bit)
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out selectionHit, 10000f, 256, QueryTriggerInteraction.Ignore);
                if (hit && (selectionHit.collider.gameObject != null))
                    return (selectionHit.collider.gameObject);
                else
                    return (null);
            }
            else return (null);
        }

        /// <summary>
        /// Not used - used to pull up muti-hex selection
        /// </summary>
        /// <param name="inputSelection"></param>
        /// <returns></returns>
        public static GameObject GetHexFromRightButtonUserInput(Vector3 inputSelection)
        {
            RaycastHit selectionHit = new RaycastHit();

            if (Input.GetMouseButtonDown(1))
            {
                //  Note in the call below, 256 is used to only hit hexes on layer 8 (the 9th bit)
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(inputSelection), out selectionHit, 10000f, 256, QueryTriggerInteraction.Ignore);
                if (hit)
                {
                    return (selectionHit.collider.gameObject);
                }
                else
                {
                    return (null);
                }
            }
            else
            {
                return (null);
            }
        }

        /// <summary>
        /// This routine returns a hex based on the user input
        /// </summary>
        /// <param name="inputSelection"></param>
        /// <returns></returns>
        public static GameObject GetHexFromUserInput(Vector3 inputSelection)
        {
            RaycastHit selectionHit = new RaycastHit();

            if (Input.GetMouseButtonDown(0))
            {
                //  Note in the call below, 256 is used to only hit hexes on layer 8 (the 9th bit)
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(inputSelection), out selectionHit, 10000f, 256, QueryTriggerInteraction.Ignore);
                if (hit)
                {
                    return (selectionHit.collider.gameObject);
                }
                else
                {
                    return (null);
                }
            }
            else
            {
                return (null);
            }
        }

        /// <summary>
        /// Based on the hex passed to it this routine will return the unit on the hex
        /// In the case where there is more than one unit it will return the top unit
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static GameObject GetUnitOnHex(GameObject hex)
        {
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                return (hex.GetComponent<HexDatabaseFields>().occupyingUnit[hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count - 1]);
            else
                return (null);
        }

        /// <summary>
        /// This routine is the standard routine for selecting a unit since many of the times the user has the option of chosing off board units
        /// </summary>
        /// <returns></returns>
        public static GameObject GetUnitWithoutHex(Vector2 mousePosition)
        {
            RaycastHit selectionHit = new RaycastHit();
            GameObject unit;

            if (Input.GetMouseButtonDown(0))
            {
                //  Note in the call below, 512 is used to only hit counters on layer 9 (the 10th bit)
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out selectionHit, 10000f, 512, QueryTriggerInteraction.Ignore);
                if (hit)
                    unit = selectionHit.collider.gameObject;
                else
                    return (null);

                if ((unit != null) && (unit.GetComponent<UnitDatabaseFields>().occupiedHex != null) && (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 1))
                    // The unit selected is on a hex with multiple units.  Make sure that the topmost unit is returned
                    unit = unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().occupyingUnit[unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count - 1];
                return (unit);
            }
            else
                return (null);
        }

        /// <summary>
        /// This routine returns the hex with the given map coordinates
        /// </summary>
        /// <param name="xCoor"></param>
        /// <param name="yCoor"></param>
        /// <returns></returns>
        public static GameObject GetHexAtXY(int xCoor, int yCoor)
        {
            foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                if ((hex.GetComponent<HexDatabaseFields>().xMapCoor == xCoor) &&
                    (hex.GetComponent<HexDatabaseFields>().yMapCoor == yCoor))
                    return (hex);

            // This executes when called by a hex neighbor search for sides with no hexes
            return (null);
        }

        /// <summary>
        /// Zero's out the remaining movement fields
        /// </summary>
        public static void ResetMovementAvailableFields()
        {
            foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
            {
                hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
            }
        }

        /// <summary>
        /// When placing a unit on a hex, this routine needs to be used in order to offset multiple units on a hex in order to make them visible
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="hex"></param>
        public static void PutUnitOnHex(GameObject unit, GameObject hex)
        {
            float offset = 0.5f;

            unit.GetComponent<UnitDatabaseFields>().occupiedHex = hex;
            hex.GetComponent<HexDatabaseFields>().occupyingUnit.Add(unit);

            // If the hex exerts ZOC to neighbors, need to add the unit to the list of exerting units
            foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                if ((hex.GetComponent<BooleanArrayData>().exertsZOC[(int)hexSide]) && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null))
                    if (!hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().unitsExertingZOC.Contains(unit))
                        hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().unitsExertingZOC.Add(unit);

            // The new unit is on the top of the stack right now.  In order to allow for a user to cycle through the units on a hex I will swap
            // the order.  This way when a user selects a unit it selects the top unit, and if he clicks the same hex it will move to the bottom
            // and the next unit will be on top.

            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 1)
            {
                // There is more than one unit on the hex
                for (int index = hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index > 1; index--)
                {
                    hex.GetComponent<HexDatabaseFields>().occupyingUnit[index - 1] = hex.GetComponent<HexDatabaseFields>().occupyingUnit[index - 2];
                    hex.GetComponent<HexDatabaseFields>().occupyingUnit[index - 1].transform.position = hex.transform.position + new Vector3(offset * (index - 1), offset * (index - 1));
                    hex.GetComponent<HexDatabaseFields>().occupyingUnit[index - 1].GetComponent<SpriteRenderer>().sortingOrder = index - 1;
                }
            }
            hex.GetComponent<HexDatabaseFields>().occupyingUnit[0] = unit;
            unit.transform.position = hex.transform.position;
            hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<SpriteRenderer>().sortingOrder = 0;
        }

        /// <summary>
        /// This routine is used to adjust the offsets when a unit is removed from a hex that has more than 1 unit
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="hex"></param>
        public static void RemoveUnitFromHex(GameObject unit, GameObject hex)
        {
            if ((unit != null) && (hex != null))
            {
                float offset = 0.5f;
                unit.GetComponent<SpriteRenderer>().sortingOrder = 0;
                hex.GetComponent<HexDatabaseFields>().occupyingUnit.Remove(unit);

                // If the hex exerts ZOC to neighbors, need to remove the unit to the list of exerting units
                foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                    if ((hex.GetComponent<BooleanArrayData>().exertsZOC[(int)hexSide]) && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null))
                        if (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().unitsExertingZOC.Contains(unit))
                            hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().unitsExertingZOC.Remove(unit);

                // Remove the unit from exerting ZOC on the hex it is being removed from
                if (hex.GetComponent<HexDatabaseFields>().unitsExertingZOC.Contains(unit))
                    hex.GetComponent<HexDatabaseFields>().unitsExertingZOC.Remove(unit);

                if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                    for (int index = 0; index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                    {
                        hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].transform.position = hex.transform.position + new Vector3(index * offset, index * offset);
                        hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<SpriteRenderer>().sortingOrder = index + 1;
                    }

                // Need to update the ZOC of the hex since the unit that was removed was the last unit on the hex
                else
                    // Before I created the common routines, I was calling UpdateZOC through a singleton.  I don't think I need this anymore so I'm making it a direct call.
                    // The original call is below
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().UpdateZOC(hex);
            }
            else
            {
                // Not sure why but I keep getting null exceptions in this routine
                if (unit == null)
                    IORoutines.WriteToLogFile("removeUnitFromHex: unit passed is null");
                if (hex == null)
                    IORoutines.WriteToLogFile("removeUnitFromHex: hex passed is null");
            }
        }
    }
}
