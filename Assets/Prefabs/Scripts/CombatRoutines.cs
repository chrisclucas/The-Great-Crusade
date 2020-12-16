using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class CombatRoutines : MonoBehaviour
    {
        /// <summary>
        /// Returns true if there are units that should be invovled in combat but aren't committed to an attack
        /// Will highlight all units that are uncommitted but should be involved in an attack if the shouldHighlight flag is true
        /// </summary>
        /// <param name="attackingNationality"></param>
        /// <param name="shouldHighlight"></param>
        public static bool CheckIfRequiredUnitsAreUncommitted(GlobalDefinitions.Nationality attackingNationality, bool shouldHighlight)
        {
            //GlobalDefinitions.writeToLogFile("checkIfRequiredUnitsAreUncommitted: executing");
            bool unitFound = false;
            List<GameObject> attackingUnits;

            // I'm going to turn off all unit highlighting here.  It will be added back on when the gui is dismissed if needed
            GlobalDefinitions.UnhighlightAllUnits();

            if (attackingNationality == GlobalDefinitions.Nationality.German)
                attackingUnits = GlobalDefinitions.germanUnitsOnBoard;
            else
                attackingUnits = GlobalDefinitions.alliedUnitsOnBoard;

            foreach (GameObject unit in attackingUnits)
            {
                if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                {
                    // Add the clause below to check for a unit on a sea hex.  This comes into play when a fortress is being attacked from 
                    // the sea since it isn't in a ZOC.
                    if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().inGermanZOC ||
                            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea)
                    {
                        // This is an allied unit that must perform an attack this turn
                        if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                        {
                            unitFound = true;
                            if (shouldHighlight)
                                GlobalDefinitions.HighlightUnit(unit);
                        }

                        // Get the German units that exert ZOC to this unit
                        if (HighlightUnitsThatMustBeAttacked(GlobalDefinitions.Nationality.Allied, unit.GetComponent<UnitDatabaseFields>().occupiedHex, shouldHighlight))
                            unitFound = true;
                    }
                }
                else
                {
                    if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().inAlliedZOC)
                    {
                        // This is a German unit that must perform an attack this turn
                        if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                        {
                            unitFound = true;
                            if (shouldHighlight)
                                GlobalDefinitions.HighlightUnit(unit);
                        }

                        // Get the Allied units that exert ZOC to this unit
                        if (HighlightUnitsThatMustBeAttacked(GlobalDefinitions.Nationality.German, unit.GetComponent<UnitDatabaseFields>().occupiedHex, shouldHighlight))
                            unitFound = true;
                    }
                }
            }

            // Need to check all existing attacks and see if they are cross river and bring in additional defenders that aren't being attacked
            foreach (GameObject combat in GlobalDefinitions.allCombats)
            {
                if (CheckIfDefenderToBeAddedDueToCrossRiverAttack(combat.GetComponent<Combat>().attackingUnits, combat.GetComponent<Combat>().defendingUnits, shouldHighlight))
                    unitFound = true;
            }
            return (unitFound);
        }

        /// <summary>
        /// This routine looks through the attackers and defenders and determines if there are defenders that need to be added to the mustBeAttackedUnits due to being in the ZOC of a defender being attacked cross river
        /// </summary>
        public static bool CheckIfDefenderToBeAddedDueToCrossRiverAttack(List<GameObject> attackingUnits, List<GameObject> defendingUnits, bool shouldHighlight)
        {
            bool foundUnit = false;
            List<GameObject> adjacentUnits = new List<GameObject>();

            if (attackingUnits.Count > 0)
            {
                // Get all adjacent defenders to the attacking units
                foreach (GameObject attackingUnit in attackingUnits)
                    foreach (GameObject defendingUnit in GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().ReturnAdjacentEnemyUnits(attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex, GlobalDefinitions.ReturnOppositeNationality(attackingUnits[0].GetComponent<UnitDatabaseFields>().nationality)))
                        if (!adjacentUnits.Contains(defendingUnit))
                            adjacentUnits.Add(defendingUnit);

                foreach (GameObject defendingUnit in defendingUnits)
                    if (defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                        //check if the defender is across a river from a committed attacker
                        foreach (GameObject attackingUnit in attackingUnits)
                            if (attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                                // Go through each of the hexsides on the defending hexes and see if there is a river between the defender and the attacker
                                foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                                    if ((defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                                            && (defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] == attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex)
                                            && defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<BooleanArrayData>().riverSides[(int)hexSide])
                                    {
                                        // At this point we know that the defender is across the river from a committed attacker

                                        // Now check if there is a friendly unit in the ZOC of the defender and is adjacent to the attacker.  If so, add it to must be attacked

                                        // Get all friendly units that are in the ZOC of the defender
                                        foreach (GameObject unit in ReturnFriendlyUnitsInZOC(defendingUnit))
                                            // Now check if any of the units are adjacent to the attacker with a river between them.  If so, add it to must be attacked.
                                            if (adjacentUnits.Contains(unit) &&
                                                    GeneralHexRoutines.CheckForRiverBetweenTwoHexes(attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.GetComponent<UnitDatabaseFields>().occupiedHex) &&
                                                    !unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                                            {
                                                foundUnit = true;
                                                if (shouldHighlight)
                                                    GlobalDefinitions.HighlightUnit(unit);
                                            }
                                    }
            }

            return (foundUnit);
        }

        /// <summary>
        /// Returns all friendly units that are in the ZOC of the unit passed
        /// </summary>
        /// <param name="defendingUnit"></param>
        /// <returns></returns>
        private static List<GameObject> ReturnFriendlyUnitsInZOC(GameObject defendingUnit)
        {
            List<GameObject> returnList = new List<GameObject>();
            foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                if (defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<BooleanArrayData>().exertsZOC[(int)hexSide])
                    foreach (GameObject unit in defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit)
                        returnList.Add(unit);
            return (returnList);
        }

        /// <summary>
        /// Highlights any units that must be attacked this turn.  Returns true if it finds units
        /// The shouldHighlight flag is needed because if being called by the AI units shouldn't be highlighted
        /// </summary>
        /// <param name="attackingNationality"></param>
        /// <param name="hex"></param>
        /// <param name="shouldHighlight"></param>
        private static bool HighlightUnitsThatMustBeAttacked(GlobalDefinitions.Nationality attackingNationality, GameObject hex, bool shouldHighlight)
        {
            bool foundUnit = false;

            foreach (HexDefinitions.HexSides hexSide in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                        && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<BooleanArrayData>().exertsZOC[GlobalDefinitions.ReturnHexSideOpposide((int)hexSide)] == true)
                        && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                        && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality != attackingNationality))
                    foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit)
                    {
                        if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                        {
                            foundUnit = true;
                            if (shouldHighlight)
                                GlobalDefinitions.HighlightUnit(unit);
                        }
                    }

            // Need to do a special case check here for being on a sea hex, the invasion target will be a mustBeAttacked even if it is a fortress
            if (hex.GetComponent<HexDatabaseFields>().sea && hex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().fortress)
                // Any units in a fortress that has units attacking from the sea will should be attacked
                foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().occupyingUnit)
                    if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    {
                        foundUnit = true;
                        if (shouldHighlight)
                            GlobalDefinitions.HighlightUnit(unit);
                    }

            return (foundUnit);
        }


        /// <summary>
        /// This routine returns true if there are units available to defend the hex passed to it
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private bool NonCommittedDefendersAvailable(GameObject hex)
        {
            bool allUnitsCommitted = true;
            foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().occupyingUnit)
                if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    allUnitsCommitted = false;

            // We're returning if there are units available which is the opposite of what we captured
            return (!allUnitsCommitted);
        }

        /// <summary>
        /// This routine pulls all the defenders and attackers based on the hex passed and then calls the combat selection GUI
        /// </summary>
        /// <param name="defendingNationality"></param>
        public void PrepForCombatDisplay(GameObject hex, GlobalDefinitions.Nationality defendingNationality)
        {
            // First thing we need to do is check that the combat assignment gui isn't already active.  If it is do not load anoether one.
            if (GameObject.Find("CombatGUIInstance") != null)
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Resolve current combat assignment before assigning another");
                return;
            }

            GameObject singleCombat = new GameObject("prepForCombatDisplay");
            singleCombat.AddComponent<Combat>();
            // Check if the hex has uncommittted units of the right nationality on it and there are adjacent enemies
            if ((hex != null) &&
                    (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                    (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == defendingNationality) &&
                    NonCommittedDefendersAvailable(hex) &&
                    (ReturnUncommittedUnits(ReturnAdjacentEnemyUnits(hex, GlobalDefinitions.ReturnOppositeNationality(defendingNationality))).Count > 0))
            {
                // Get all the available defending units
                singleCombat.GetComponent<Combat>().defendingUnits = ReturnUncommittedUnits(hex.GetComponent<HexDatabaseFields>().occupyingUnit);

                singleCombat.GetComponent<Combat>().attackingUnits = ReturnUncommittedUnits(ReturnAdjacentEnemyUnits(hex, GlobalDefinitions.ReturnOppositeNationality(defendingNationality)));

                // Fianlly we have to get all potential defenders that are adjacent to the attackers
                foreach (GameObject defender in ReturnUncommittedUnits(ReturnAdjacentDefenders(hex, singleCombat)))
                    if (!singleCombat.GetComponent<Combat>().defendingUnits.Contains(defender))
                        singleCombat.GetComponent<Combat>().defendingUnits.Add(defender);

                CallCombatDisplay(singleCombat);
            }
            else
            {
                if (hex == null)
                    GlobalDefinitions.GuiUpdateStatusMessage("No valid hex selected for combat; hex selected must contain enemy units");
                else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                    GlobalDefinitions.GuiUpdateStatusMessage("No units found on hex selected for combat; hex selected must contain enemy units");
                else if (!NonCommittedDefendersAvailable(hex))
                    GlobalDefinitions.GuiUpdateStatusMessage("No uncommitted defenders found on hex selected; all units on hex are already assigned to combat.  Cancel attacks and reassign combat to add additional attacking units.");
                else if (ReturnUncommittedUnits(ReturnAdjacentEnemyUnits(hex, GlobalDefinitions.ReturnOppositeNationality(defendingNationality))).Count == 0)
                    GlobalDefinitions.GuiUpdateStatusMessage("No units are available to attack the hex selected");
                //else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality != defendingNationality)
                else
                    GlobalDefinitions.GuiDisplayUnitsOnHex(hex);

                if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                        (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanMovementStateInstance"))
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().ExecuteSelectUnit;
                else if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedInvasionStateInstance")
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedInvasionState>().ExecuteSelectUnit;
                else if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedAirborneStateInstance")
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedAirborneState>().ExecuteSelectUnit;
                else
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteSelectUnit;
            }
        }

        /// <summary>
        /// Returns the units passed that are not committed to an attack
        /// </summary>
        /// <param name="unitList"></param>
        /// <returns></returns>
        public List<GameObject> ReturnUncommittedUnits(List<GameObject> unitList)
        {
            List<GameObject> returnList = new List<GameObject>();
            foreach (GameObject unit in unitList)
                if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    returnList.Add(unit);
            return (returnList);
        }

        /// <summary>
        /// This routine is what pulls up the GUI to assign units to a single combat
        /// </summary>
        /// <param name="singleCombat"></param>
        public void CallCombatDisplay(GameObject singleCombat)
        {
            Button okButton;
            Button cancelButton;
            Canvas combatCanvas = new Canvas();

            // I'm going to unhighight all the units potentially involved in this attack.  When selected for inclusion in the attack they
            // will be highlighted.  I will restore the must-attack and must-be-attacked highlighting when leaving the gui

            foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
                GlobalDefinitions.UnhighlightUnit(unit);
            foreach (GameObject unit in singleCombat.GetComponent<Combat>().attackingUnits)
                GlobalDefinitions.UnhighlightUnit(unit);

            // The panel needs to be at least the width for four units to fit everything
            int maxUnits = 5;
            if (singleCombat.GetComponent<Combat>().defendingUnits.Count > maxUnits)
                maxUnits = singleCombat.GetComponent<Combat>().defendingUnits.Count;
            if (singleCombat.GetComponent<Combat>().attackingUnits.Count > maxUnits)
                maxUnits = singleCombat.GetComponent<Combat>().attackingUnits.Count;
            float panelWidth = (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE;
            float panelHeight = 7 * GlobalDefinitions.GUIUNITIMAGESIZE;
            GlobalDefinitions.combatGUIInstance = GUIRoutines.CreateGUICanvas("CombatGUIInstance",
                    panelWidth,
                    panelHeight,
                    ref combatCanvas);

            GUIRoutines.CreateUIText("Combat Odds", "OddsText",
                    3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    (0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE) - 0.5f * panelWidth,
                    6.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    Color.white, combatCanvas);

            float xSeperation = (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE / maxUnits;
            float xOffset = xSeperation / 2;
            for (int index = 0; index < singleCombat.GetComponent<Combat>().defendingUnits.Count; index++)
            {
                Toggle tempToggle;
                tempToggle = GUIRoutines.CreateUnitTogglePair("unitToggleDefendingPair" + index,
                        index * xSeperation + xOffset - 0.5f * panelWidth,
                        5.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        combatCanvas,
                        singleCombat.GetComponent<Combat>().defendingUnits[index]);
                tempToggle.gameObject.AddComponent<CombatToggleRoutines>();
                tempToggle.gameObject.GetComponent<CombatToggleRoutines>().currentCombat = singleCombat;
                tempToggle.gameObject.GetComponent<CombatToggleRoutines>().unit = singleCombat.GetComponent<Combat>().defendingUnits[index];
                tempToggle.gameObject.GetComponent<CombatToggleRoutines>().attackingUnitFlag = false;
                tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<CombatToggleRoutines>().AddOrDeleteSelectedUnit());

                if (CheckForInvasionDefense(singleCombat.GetComponent<Combat>().defendingUnits[index], singleCombat))
                {
                    // This executes if the defender is on an invasion hex of at least one of the attackers.  If it is the toggle will be set and there are checks in the toggle routines from allowing
                    // it to be turned off.  If the defenders are being invaded they can't be attacked separately.
                    tempToggle.isOn = true;
                    tempToggle.interactable = false;
                }
            }

            for (int index = 0; index < singleCombat.GetComponent<Combat>().attackingUnits.Count; index++)
            {
                Toggle tempToggle;
                tempToggle = GUIRoutines.CreateUnitTogglePair("unitToggleAttackingPair" + index,
                        index * xSeperation + xOffset - 0.5f * panelWidth,
                        3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        combatCanvas,
                        singleCombat.GetComponent<Combat>().attackingUnits[index]);
                tempToggle.gameObject.AddComponent<CombatToggleRoutines>();
                tempToggle.gameObject.GetComponent<CombatToggleRoutines>().currentCombat = singleCombat;
                tempToggle.gameObject.GetComponent<CombatToggleRoutines>().unit = singleCombat.GetComponent<Combat>().attackingUnits[index];
                tempToggle.gameObject.GetComponent<CombatToggleRoutines>().attackingUnitFlag = true;
                tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<CombatToggleRoutines>().AddOrDeleteSelectedUnit());

                if (CheckForInvadingAttacker(singleCombat.GetComponent<Combat>().attackingUnits[index]))
                {
                    // This executes if the attacker is invading this turn. The unit will be selected and the toggle routines will not allow it to be turned off 
                    tempToggle.isOn = true;
                    tempToggle.interactable = false;
                }
            }

            GlobalDefinitions.combatAirSupportToggle = null;
            GlobalDefinitions.combatCarpetBombingToggle = null;
            // If there are air missions left present the user with the option of adding air support to the attack
            if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
            {
                Toggle airToggle;
                airToggle = GUIRoutines.CreateToggle("CombatAirSupportToggle",
                        0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth - 2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        combatCanvas).GetComponent<Toggle>();
                GlobalDefinitions.combatAirSupportToggle = airToggle.gameObject;
                airToggle.gameObject.AddComponent<CombatToggleRoutines>();
                airToggle.gameObject.GetComponent<CombatToggleRoutines>().currentCombat = singleCombat;
                airToggle.onValueChanged.AddListener((bool value) => airToggle.GetComponent<CombatToggleRoutines>().ToggleAirSupport());
                if (GlobalDefinitions.tacticalAirMissionsThisTurn <= GlobalDefinitions.maxNumberOfTacticalAirMissions)
                    airToggle.GetComponent<Toggle>().interactable = true;
                else
                    airToggle.GetComponent<Toggle>().interactable = false;

                GUIRoutines.CreateUIText("Air Support", "CombatAirSupportText",
                        1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth - 1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        Color.white, combatCanvas);

                Toggle carpetToggle;
                carpetToggle = GUIRoutines.CreateToggle("CombatCarpetBombingToggle",
                        0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth + 1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        combatCanvas).GetComponent<Toggle>();
                GlobalDefinitions.combatCarpetBombingToggle = carpetToggle.gameObject;
                carpetToggle.gameObject.AddComponent<CombatToggleRoutines>();
                carpetToggle.gameObject.GetComponent<CombatToggleRoutines>().currentCombat = singleCombat;
                carpetToggle.onValueChanged.AddListener((bool value) => carpetToggle.GetComponent<CombatToggleRoutines>().ToggleCarpetBombing());
                if (CheckIfCarpetBombingIsAvailable(singleCombat))
                    carpetToggle.GetComponent<Toggle>().interactable = true;
                else
                    carpetToggle.GetComponent<Toggle>().interactable = false;

                GUIRoutines.CreateUIText("Carpet Bombing", "CarpetBombingSupportText",
                        1.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                        1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth + 2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        Color.white, combatCanvas);
            }

            // OK button
            okButton = GUIRoutines.CreateButton("combatOKButton", "OK",
                    0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth - 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas);
            okButton.gameObject.AddComponent<CombatGUIOK>();
            okButton.gameObject.GetComponent<CombatGUIOK>().singleCombat = singleCombat;
            okButton.onClick.AddListener(okButton.GetComponent<CombatGUIOK>().OkCombatGUISelection);

            // Cancel button
            cancelButton = GUIRoutines.CreateButton("combatCancelButton", "Cancel",
                    0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas);
            cancelButton.gameObject.AddComponent<CombatGUIOK>();
            cancelButton.gameObject.GetComponent<CombatGUIOK>().singleCombat = singleCombat;
            cancelButton.onClick.AddListener(cancelButton.GetComponent<CombatGUIOK>().CancelCombatGUISelection);

        }

        /// <summary>
        /// Routine checks if carpet bombing is avaibale for the combat passed and returns true if it is
        /// </summary>
        /// <param name="GameObject"></param>
        /// <returns></returns>
        public static bool CheckIfCarpetBombingIsAvailable(GameObject singleCombat)
        {
            // The criteria for having carpet bombing available on a combat is that:
            //      * only four in a game
            //      * can only execute a maximum of one per turn
            //      * can't execute on a turn an invasion is taking place
            //      * can't execute on a turn in which ariborne units have dropped
            //      * all defenders must be on a single hex
            //      * the hex must have been attacked last turn
            //      * carpet bombing is in effect for all attacks on the single hex

            if (GlobalDefinitions.numberOfCarpetBombingsUsed == GlobalDefinitions.maxNumberOfCarpetBombings)
                return false;

            if (GlobalDefinitions.carpetBombingUsedThisTurn)
                return false;

            if (GlobalDefinitions.currentAirborneDropsThisTurn > 0)
                return false;

            if (GlobalDefinitions.invasionsTookPlaceThisTurn)
                return false;

            // Check that all defenders are on a single hex.  Since this is called from the combat gui, only count defenders if they are committed to an attack
            List<GameObject> defenders = new List<GameObject>();

            foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    defenders.Add(unit);

            if (defenders.Count == 0)
                return false;

            GameObject defendingHex = defenders[0].GetComponent<UnitDatabaseFields>().occupiedHex;
            foreach (GameObject unit in defenders)
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex != defendingHex)
                    return false;

            // The last check is if the defending hex was attacked last turn
            if (!GlobalDefinitions.hexesAttackedLastTurn.Contains(defendingHex))
                return false;

            // All checks have been met if the code reaches here so return a true
            return true;
        }

        /// <summary>
        /// Display a GUI that shows all of the available carpet bombing hexes available
        /// </summary>
        public static void DisplayCarpetBombingHexesAvailable()
        {
            Canvas bombingCanvas = new Canvas();
            Button tempOKButton;
            int widthSeed = 6;  // Just to make the development easier since position is based on width
            int heightSeed = GlobalDefinitions.hexesAttackedLastTurn.Count + 2;

            float panelWidth = widthSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
            float panelHeight = heightSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;

            // The avaialble carpet bombing hexes are stored in the hexesAttackedLastTurn list.  All non valid
            // hexes were removed already (i.e. hexes attacked last turn but do not have Germans on them this turn)
            GUIRoutines.CreateGUICanvas("CarpetBombingGUI",
                    panelWidth,
                    panelHeight,
                    ref bombingCanvas);

            for (int index = 0; index < GlobalDefinitions.hexesAttackedLastTurn.Count; index++)
            {
                Toggle tempToggle;
                Button tempLocateButton;

                GUIRoutines.CreateUIText("Carpet bombing available - you may select a hex", "CarpetBombingAvailableText",
                        widthSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE,
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        (widthSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE) / 2 - 0.5f * panelWidth,
                        heightSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        Color.white, bombingCanvas);

                for (int index2 = 0; index2 < GlobalDefinitions.hexesAttackedLastTurn[index].GetComponent<HexDatabaseFields>().occupyingUnit.Count; index2++)
                {
                    GUIRoutines.CreateUnitImage(GlobalDefinitions.hexesAttackedLastTurn[index].GetComponent<HexDatabaseFields>().occupyingUnit[index2], "UnitImage",
                            index2 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                            (index + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                            bombingCanvas);
                }

                tempLocateButton = GUIRoutines.CreateButton("BombingLocateButton" + index, "Locate",
                        3 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        (index + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        bombingCanvas);
                tempLocateButton.gameObject.AddComponent<CarpetBombingToggleRoutines>();
                tempLocateButton.GetComponent<CarpetBombingToggleRoutines>().hex = GlobalDefinitions.hexesAttackedLastTurn[index];
                tempLocateButton.onClick.AddListener(tempLocateButton.GetComponent<CarpetBombingToggleRoutines>().LocateCarpetBombingHex);
                tempToggle = GUIRoutines.CreateToggle("BombingToggle" + index,
                        4 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        (index + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        bombingCanvas).GetComponent<Toggle>();
                tempToggle.gameObject.AddComponent<CarpetBombingToggleRoutines>();
                tempToggle.GetComponent<CarpetBombingToggleRoutines>().hex = GlobalDefinitions.hexesAttackedLastTurn[index];
                tempToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => tempToggle.GetComponent<CarpetBombingToggleRoutines>().SelectHex());
            }
            tempOKButton = GUIRoutines.CreateButton("BombingOKButton", "OK",
                    widthSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE / 2 - 0.5f * panelWidth,
                    0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    bombingCanvas);
            tempOKButton.gameObject.AddComponent<CarpetBombingOKRoutines>();
            tempOKButton.onClick.AddListener(tempOKButton.GetComponent<CarpetBombingOKRoutines>().CarpetBombingOK);
        }

        /// <summary>
        /// This routine returns true if the unit passed to it is defending against a seaborne invasion
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public bool CheckForInvasionDefense(GameObject unit, GameObject singleCombat)
        {
            // Check the attacking units if they are on a sea hex with an invasion target of the defender
            foreach (GameObject attackUnit in singleCombat.GetComponent<Combat>().attackingUnits)
                if ((attackUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea) &&
                        (attackUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().invasionTarget == unit.GetComponent<UnitDatabaseFields>().occupiedHex))
                    return true;
            return false;
        }

        /// <summary>
        /// This routine returns true is the unit passed to it is attempting a seaborne invasion
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public bool CheckForInvadingAttacker(GameObject unit)
        {
            // The assumption here is that if there is an attacker on a sea hex and it is in the combatAssignmentAttackingUnits list than it is invading
            if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns a list of enemy units adjacent to the hex passed
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="defendingNationality"></param>
        /// <returns></returns>
        public List<GameObject> ReturnAdjacentEnemyUnits(GameObject hex, GlobalDefinitions.Nationality enemyNationality)
        {
            //GlobalDefinitions.writeToLogFile("returnAdjacentEnemyUnits: hex = " + hex.name);
            List<GameObject> returnList = new List<GameObject>();
            if (hex != null)
                foreach (HexDefinitions.HexSides hexSides in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                {
                    if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] != null) &&
                            (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                            (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == enemyNationality))
                        // Need to check if an enemy is on an adjacent sea hex or that it is invading the hex selected
                        if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().sea == false) ||
                                ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().sea == true) &&
                                (hex == hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().invasionTarget)))
                            foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit)
                                returnList.Add(unit);
                }
            return (returnList);
        }

        /// <summary>
        /// Takes the list of attackers passed to it and returns all adjacent defenders not committed to an attack
        /// </summary>
        public List<GameObject> ReturnAdjacentDefenders(GameObject hex, GameObject singleCombat)
        {
            // This should never happen but make sure that the hex passed has defending units on it in order to check for an invasion.
            // If the hex doesn't have a defender then the routine will return null
            if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0) ||
                    ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                    ((hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality ==
                    singleCombat.GetComponent<Combat>().attackingUnits[0].GetComponent<UnitDatabaseFields>().nationality))))
                return (null);

            List<GameObject> returnList = new List<GameObject>();
            if (!CheckForInvasionDefense(hex.GetComponent<HexDatabaseFields>().occupyingUnit[0], singleCombat))
                foreach (GameObject attacker in singleCombat.GetComponent<Combat>().attackingUnits)
                    foreach (HexDefinitions.HexSides hexSides in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                        if ((attacker.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] != null) &&
                                (attacker.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                                (attacker.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality !=
                                singleCombat.GetComponent<Combat>().attackingUnits[0].GetComponent<UnitDatabaseFields>().nationality))
                            foreach (GameObject defender in attacker.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit)
                                if (!defender.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                                    returnList.Add(defender);
            return (returnList);
        }
    }
}