using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CombatRoutines : MonoBehaviour
{
    /// <summary>
    /// Returns true if there are units that should be invovled in combat but aren't committed to an attack
    /// Will highlight all units that are uncommitted but should be involved in an attack if the shouldHighlight flag is true
    /// </summary>
    /// <param name="attackingNationality"></param>
    /// <param name="shouldHighlight"></param>
    public static bool checkIfRequiredUnitsAreUncommitted(GlobalDefinitions.Nationality attackingNationality, bool shouldHighlight)
    {
        //GlobalDefinitions.writeToLogFile("checkIfRequiredUnitsAreUncommitted: executing");
        bool unitFound = false;
        List<GameObject> attackingUnits;

        // I'm going to turn off all unit highlighting here.  It will be added back on when the gui is dismissed if needed
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            GlobalDefinitions.unhighlightUnit(unit);
        foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
            GlobalDefinitions.unhighlightUnit(unit);

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
                            GlobalDefinitions.highlightUnit(unit);
                    }

                    // Get the German units that exert ZOC to this unit
                    if (highlightUnitsThatMustBeAttacked(GlobalDefinitions.Nationality.Allied, unit.GetComponent<UnitDatabaseFields>().occupiedHex, shouldHighlight))
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
                            GlobalDefinitions.highlightUnit(unit);
                    }

                    // Get the Allied units that exert ZOC to this unit
                    if (highlightUnitsThatMustBeAttacked(GlobalDefinitions.Nationality.German, unit.GetComponent<UnitDatabaseFields>().occupiedHex, shouldHighlight))
                        unitFound = true;
                }
            }
        }

        // Need to check all existing attacks and see if they are cross river and bring in additional defenders that aren't being attacked
        foreach (GameObject combat in GlobalDefinitions.allCombats)
        {
            if (checkIfDefenderToBeAddedDueToCrossRiverAttack(combat.GetComponent<Combat>().attackingUnits, combat.GetComponent<Combat>().defendingUnits, shouldHighlight))
                unitFound = true;
        }
        return (unitFound);
    }

    /// <summary>
    /// This routine looks through the attackers and defenders and determines if there are defenders that need to be added to the mustBeAttackedUnits due to being in the ZOC of a defender being attacked cross river
    /// </summary>
    public static bool checkIfDefenderToBeAddedDueToCrossRiverAttack(List<GameObject> attackingUnits, List<GameObject> defendingUnits, bool shouldHighlight)
    {
        bool foundUnit = false;
        List<GameObject> adjacentUnits = new List<GameObject>();

        if (attackingUnits.Count > 0)
        {
            // Get all adjacent defenders to the attacking units
            foreach (GameObject attackingUnit in attackingUnits)
                foreach (GameObject defendingUnit in GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().returnAdjacentEnemyUnits(attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex, GlobalDefinitions.returnOppositeNationality(attackingUnits[0].GetComponent<UnitDatabaseFields>().nationality)))
                    if (!adjacentUnits.Contains(defendingUnit))
                        adjacentUnits.Add(defendingUnit);

            foreach (GameObject defendingUnit in defendingUnits)
                if (defendingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    //check if the defender is across a river from a committed attacker
                    foreach (GameObject attackingUnit in attackingUnits)
                        if (attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                            // Go through each of the hexsides on the defending hexes and see if there is a river between the defender and the attacker
                            foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                                if ((defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                                        && (defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] == attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex)
                                        && defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<BoolArrayData>().riverSides[(int)hexSide])
                                {
                                    // At this point we know that the defender is across the river from a committed attacker

                                    // Now check if there is a friendly unit in the ZOC of the defender and is adjacent to the attacker.  If so, add it to must be attacked

                                    // Get all friendly units that are in the ZOC of the defender
                                    foreach (GameObject unit in returnFriendlyUnitsInZOC(defendingUnit))
                                        // Now check if any of the units are adjacent to the attacker with a river between them.  If so, add it to must be attacked.
                                        if (adjacentUnits.Contains(unit) && 
                                                GlobalDefinitions.checkForRiverBetweenTwoHexes(attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex, unit.GetComponent<UnitDatabaseFields>().occupiedHex) &&
                                                !unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                                        {
                                            foundUnit = true;
                                            if (shouldHighlight)
                                                GlobalDefinitions.highlightUnit(unit);
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
    private static List<GameObject> returnFriendlyUnitsInZOC(GameObject defendingUnit)
    {
        List<GameObject> returnList = new List<GameObject>();
        foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            if (defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<BoolArrayData>().exertsZOC[(int)hexSide])
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
    private static bool highlightUnitsThatMustBeAttacked(GlobalDefinitions.Nationality attackingNationality, GameObject hex, bool shouldHighlight)
    {
        bool foundUnit = false;

        foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            if ((hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                    && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<BoolArrayData>().exertsZOC[GlobalDefinitions.returnHexSideOpposide((int)hexSide)] == true)
                    && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                    && (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality != attackingNationality))
                foreach (GameObject unit in hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide].GetComponent<HexDatabaseFields>().occupyingUnit)
                {
                    if (!unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    {
                        foundUnit = true;
                        if (shouldHighlight)
                            GlobalDefinitions.highlightUnit(unit);
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
                        GlobalDefinitions.highlightUnit(unit);
                }

        return (foundUnit);
    }


    /// <summary>
    /// This routine returns true if there are units available to defend the hex passed to it
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    private bool nonCommittedDefendersAvailable(GameObject hex)
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
    public void prepForCombatDisplay(GameObject hex, GlobalDefinitions.Nationality defendingNationality)
    {
        // First thing we need to do is check that the combat assignment gui isn't already active.  If it is do not load anoether one.
        if (GameObject.Find("CombatGUIInstance") != null)
        {
            GlobalDefinitions.guiUpdateStatusMessage("Resolve current combat assignment before assigning another");
            return;
        }

        GameObject singleCombat = new GameObject();
        singleCombat.AddComponent<Combat>();
        // Check if the hex has uncommittted units of the right nationality on it and there are adjacent enemies
        if ((hex != null) &&
                (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == defendingNationality) &&
                nonCommittedDefendersAvailable(hex) &&
                (returnUncommittedUnits(returnAdjacentEnemyUnits(hex, GlobalDefinitions.returnOppositeNationality(defendingNationality))).Count > 0))
        {
            // Get all the available defending units
            singleCombat.GetComponent<Combat>().defendingUnits = returnUncommittedUnits(hex.GetComponent<HexDatabaseFields>().occupyingUnit);

            singleCombat.GetComponent<Combat>().attackingUnits = returnUncommittedUnits(returnAdjacentEnemyUnits(hex, GlobalDefinitions.returnOppositeNationality(defendingNationality)));

            // Fianlly we have to get all potential defenders that are adjacent to the attackers
            foreach (GameObject defender in returnUncommittedUnits(returnAdjacentDefenders(hex, singleCombat)))
                if (!singleCombat.GetComponent<Combat>().defendingUnits.Contains(defender))
                    singleCombat.GetComponent<Combat>().defendingUnits.Add(defender);

            callCombatDisplay(singleCombat);
        }
        else
        {
            if (hex == null)
                GlobalDefinitions.guiUpdateStatusMessage("No valid hex selected");
            else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                GlobalDefinitions.guiUpdateStatusMessage("No units found on hex selected");
            else if (!nonCommittedDefendersAvailable(hex))
                GlobalDefinitions.guiUpdateStatusMessage("No uncommitted defenders found on hex selected");
            else if (returnUncommittedUnits(returnAdjacentEnemyUnits(hex, GlobalDefinitions.returnOppositeNationality(defendingNationality))).Count == 0)
                GlobalDefinitions.guiUpdateStatusMessage("No available units available to attack the hex selected");
            else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality != defendingNationality)
                GlobalDefinitions.guiDisplayUnitsOnHex(hex);

            if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanMovementStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().executeSelectUnit;
            else if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedInvasionStateInstance")
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedInvasionState>().executeSelectUnit;
            else if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedAirborneStateInstance")
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedAirborneState>().executeSelectUnit;
            else
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<CombatState>().executeSelectUnit;
        }
    }

    /// <summary>
    /// Returns the units passed that are not committed to an attack
    /// </summary>
    /// <param name="unitList"></param>
    /// <returns></returns>
    public List<GameObject> returnUncommittedUnits(List<GameObject> unitList)
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
    public void callCombatDisplay(GameObject singleCombat)
    {
        Button okButton;
        Button cancelButton;
        Canvas combatCanvas = new Canvas();

        // I'm going to unhighight all the units potentially involved in this attack.  When selected for inclusion in the attack they
        // will be highlighted.  I will restore the must-attack and must-be-attacked highlighting when leaving the gui

        foreach (GameObject unit in singleCombat.GetComponent<Combat>().defendingUnits)
            GlobalDefinitions.unhighlightUnit(unit);
        foreach (GameObject unit in singleCombat.GetComponent<Combat>().attackingUnits)
            GlobalDefinitions.unhighlightUnit(unit);

        // The panel needs to be at least the width for four units to fit everything
        int maxUnits = 5;
        if (singleCombat.GetComponent<Combat>().defendingUnits.Count > maxUnits)
            maxUnits = singleCombat.GetComponent<Combat>().defendingUnits.Count;
        if (singleCombat.GetComponent<Combat>().attackingUnits.Count > maxUnits)
            maxUnits = singleCombat.GetComponent<Combat>().attackingUnits.Count;
        float panelWidth = (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 7 * GlobalDefinitions.GUIUNITIMAGESIZE;
        GlobalDefinitions.combatGUIInstance = GlobalDefinitions.createGUICanvas("CombatGUIInstance",
                panelWidth,
                panelHeight,
                ref combatCanvas);

        GlobalDefinitions.createText("Combat Odds", "OddsText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                (0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE) - 0.5f * panelWidth,
                6.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas);

        float xSeperation = (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE / maxUnits;
        float xOffset = xSeperation / 2;
        for (int index = 0; index < singleCombat.GetComponent<Combat>().defendingUnits.Count; index++)
        {
            Toggle tempToggle;
            tempToggle = GlobalDefinitions.createUnitTogglePair("unitToggleDefendingPair" + index,
                    index * xSeperation + xOffset - 0.5f * panelWidth,
                    5.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas,
                    singleCombat.GetComponent<Combat>().defendingUnits[index]);
            tempToggle.gameObject.AddComponent<CombatToggleRoutines>();
            tempToggle.gameObject.GetComponent<CombatToggleRoutines>().currentCombat = singleCombat;
            tempToggle.gameObject.GetComponent<CombatToggleRoutines>().unit = singleCombat.GetComponent<Combat>().defendingUnits[index];
            tempToggle.gameObject.GetComponent<CombatToggleRoutines>().attackingUnitFlag = false;
            tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<CombatToggleRoutines>().addOrDeleteSelectedUnit());

            if (checkForInvasionDefense(singleCombat.GetComponent<Combat>().defendingUnits[index], singleCombat))
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
            tempToggle = GlobalDefinitions.createUnitTogglePair("unitToggleAttackingPair" + index,
                    index * xSeperation + xOffset - 0.5f * panelWidth,
                    3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas,
                    singleCombat.GetComponent<Combat>().attackingUnits[index]);
            tempToggle.gameObject.AddComponent<CombatToggleRoutines>();
            tempToggle.gameObject.GetComponent<CombatToggleRoutines>().currentCombat = singleCombat;
            tempToggle.gameObject.GetComponent<CombatToggleRoutines>().unit = singleCombat.GetComponent<Combat>().attackingUnits[index];
            tempToggle.gameObject.GetComponent<CombatToggleRoutines>().attackingUnitFlag = true;
            tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<CombatToggleRoutines>().addOrDeleteSelectedUnit());

            if (checkForInvadingAttacker(singleCombat.GetComponent<Combat>().attackingUnits[index]))
            {
                // This executes if the attacker is invading this turn. The unit will be selected and the toggle routines will not allow it to be turned off 
                tempToggle.isOn = true;
                tempToggle.interactable = false;
            }
        }

        GlobalDefinitions.combatAirSupportToggle = null;
        GlobalDefinitions.combatCarpetBombingToggle = null;
        // If there are air missions left present the user with the option of adding air support to the attack
        if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
        {
            Toggle airToggle;
            airToggle = GlobalDefinitions.createToggle("CombatAirSupportToggle",
                    0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth - 2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas).GetComponent<Toggle>();
            GlobalDefinitions.combatAirSupportToggle = airToggle.gameObject;
            airToggle.gameObject.AddComponent<CombatToggleRoutines>();
            airToggle.gameObject.GetComponent<CombatToggleRoutines>().currentCombat = singleCombat;
            airToggle.onValueChanged.AddListener((bool value) => airToggle.GetComponent<CombatToggleRoutines>().toggleAirSupport());
            if (GlobalDefinitions.tacticalAirMissionsThisTurn <= GlobalDefinitions.maxNumberOfTacticalAirMissions)
                airToggle.GetComponent<Toggle>().interactable = true;
            else
                airToggle.GetComponent<Toggle>().interactable = false;

            GlobalDefinitions.createText("Air Support", "CombatAirSupportText",
                    1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth - 1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas);

            Toggle carpetToggle;
            carpetToggle = GlobalDefinitions.createToggle("CombatCarpetBombingToggle",
                    0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth + 1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas).GetComponent<Toggle>();
            GlobalDefinitions.combatCarpetBombingToggle = carpetToggle.gameObject;
            carpetToggle.gameObject.AddComponent<CombatToggleRoutines>();
            carpetToggle.gameObject.GetComponent<CombatToggleRoutines>().currentCombat = singleCombat;
            carpetToggle.onValueChanged.AddListener((bool value) => carpetToggle.GetComponent<CombatToggleRoutines>().toggleCarpetBombing());
            if ((GlobalDefinitions.currentAirborneDropsThisTurn == 0) && checkForCarpetBombing())
                carpetToggle.GetComponent<Toggle>().interactable = true;
            else
                carpetToggle.GetComponent<Toggle>().interactable = false;

            GlobalDefinitions.createText("Carpet Bombing", "CarpetBombingSupportText",
                    1.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth + 2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas);
        }

        // OK button
        okButton = GlobalDefinitions.createButton("combatOKButton", "OK",
                0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth - 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas);
        okButton.gameObject.AddComponent<CombatGUIOK>();
        okButton.gameObject.GetComponent<CombatGUIOK>().singleCombat = singleCombat;
        okButton.onClick.AddListener(okButton.GetComponent<CombatGUIOK>().okCombatGUISelection);

        // Cancel button
        cancelButton = GlobalDefinitions.createButton("combatCancelButton", "Cancel",
                0.5f * (maxUnits + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas);
        cancelButton.gameObject.AddComponent<CombatGUIOK>();
        cancelButton.gameObject.GetComponent<CombatGUIOK>().singleCombat = singleCombat;
        cancelButton.onClick.AddListener(cancelButton.GetComponent<CombatGUIOK>().cancelCombatGUISelection);

    }

    /// <summary>
    /// Checks if there are carpet bombing hexes available.  Returns false if there are no hexes available
    /// </summary>
    /// <returns></returns>
    public static bool checkForCarpetBombing()
    {
        // This list is used to determine if there are hexes that were attacked last turn that are not available for carpet bombing this turn
        List<GameObject> removeHexes = new List<GameObject>();
        if ((GlobalDefinitions.numberOfCarpetBombingsUsed < GlobalDefinitions.maxNumberOfCarpetBombings) && !GlobalDefinitions.invasionsTookPlaceThisTurn && (GlobalDefinitions.currentAirborneDropsThisTurn == 0))
        {
            // See what hexes are not valid for carpet bombing from the list of hexes attacked last turn
            foreach (GameObject hex in GlobalDefinitions.hexesAttackedLastTurn)
                if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0)
                    removeHexes.Add(hex);
                else if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                    removeHexes.Add(hex);

            // This is the only routine using the hexesAttackedLastTurn list so it will be updated here to represent the hexes that are valid
            foreach (GameObject hex in removeHexes)
                if (GlobalDefinitions.hexesAttackedLastTurn.Contains(hex))
                    GlobalDefinitions.hexesAttackedLastTurn.Remove(hex);

            if (GlobalDefinitions.hexesAttackedLastTurn.Count > 0)
                return (true);
            else
                return (false);
        }

        return (false);
    }

    /// <summary>
    /// Display a GUI that shows all of the available carpet bombing hexes available
    /// </summary>
    public static void displayCarpetBombingHexesAvailable()
    {
        Canvas bombingCanvas = new Canvas();
        Button tempOKButton;
        int widthSeed = 6;  // Just to make the development easier since position is based on width
        int heightSeed = GlobalDefinitions.hexesAttackedLastTurn.Count + 2;

        float panelWidth = widthSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = heightSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;

        // The avaialble carpet bombing hexes are stored in the hexesAttackedLastTurn list.  All non valid
        // hexes were removed already (i.e. hexes attacked last turn but do not have Germans on them this turn)
        GlobalDefinitions.createGUICanvas("CarpetBombingGUI",
                panelWidth,
                panelHeight,
                ref bombingCanvas);

        for (int index = 0; index < GlobalDefinitions.hexesAttackedLastTurn.Count; index++)
        {
            Toggle tempToggle;
            Button tempLocateButton;

            GlobalDefinitions.createText("Carpet bombing available - you may select a hex", "CarpetBombingAvailableText",
                    widthSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    (widthSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE) / 2 - 0.5f * panelWidth,
                    heightSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    bombingCanvas);

            for (int index2 = 0; index2 < GlobalDefinitions.hexesAttackedLastTurn[index].GetComponent<HexDatabaseFields>().occupyingUnit.Count; index2++)
            {
                GlobalDefinitions.createUnitImage(GlobalDefinitions.hexesAttackedLastTurn[index].GetComponent<HexDatabaseFields>().occupyingUnit[index2], "UnitImage",
                        index2 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        (index + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        bombingCanvas);
            }

            tempLocateButton = GlobalDefinitions.createButton("BombingLocateButton" + index, "Locate",
                    3 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    (index + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    bombingCanvas);
            tempLocateButton.gameObject.AddComponent<CarpetBombingToggleRoutines>();
            tempLocateButton.GetComponent<CarpetBombingToggleRoutines>().hex = GlobalDefinitions.hexesAttackedLastTurn[index];
            tempLocateButton.onClick.AddListener(tempLocateButton.GetComponent<CarpetBombingToggleRoutines>().locateCarpetBombingHex);
            tempToggle = GlobalDefinitions.createToggle("BombingToggle" + index,
                    4 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    (index + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    bombingCanvas).GetComponent<Toggle>();
            tempToggle.gameObject.AddComponent<CarpetBombingToggleRoutines>();
            tempToggle.GetComponent<CarpetBombingToggleRoutines>().hex = GlobalDefinitions.hexesAttackedLastTurn[index];
            tempToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => tempToggle.GetComponent<CarpetBombingToggleRoutines>().selectHex());
        }
        tempOKButton = GlobalDefinitions.createButton("BombingOKButton", "OK",
                widthSeed * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE / 2 - 0.5f * panelWidth,
                0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                bombingCanvas);
        tempOKButton.gameObject.AddComponent<CarpetBombingOKRoutines>();
        tempOKButton.onClick.AddListener(tempOKButton.GetComponent<CarpetBombingOKRoutines>().carpetBombingOK);
    }

    /// <summary>
    /// This routine returns true if the unit passed to it is defending against a seaborne invasion
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool checkForInvasionDefense(GameObject unit, GameObject singleCombat)
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
    public bool checkForInvadingAttacker(GameObject unit)
    {
        // The assumption here is that if there is an attacker on a sea hex it and it is in the combatAssignmentAttackingUnits list than it is invading
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
    public List<GameObject> returnAdjacentEnemyUnits(GameObject hex, GlobalDefinitions.Nationality enemyNationality)
    {
        //GlobalDefinitions.writeToLogFile("returnAdjacentEnemyUnits: hex = " + hex.name);
        List<GameObject> returnList = new List<GameObject>();
        if (hex != null)
            foreach (GlobalDefinitions.HexSides hexSides in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
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
    public List<GameObject> returnAdjacentDefenders(GameObject hex, GameObject singleCombat)
    {
        // This should never happen but make sure that the hex passed has defending units on it in order to check for an invasion.
        // If the hex doesn't have a defender then the routine will return null
        if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0) ||
                ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                ((hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality ==
                singleCombat.GetComponent<Combat>().attackingUnits[0].GetComponent<UnitDatabaseFields>().nationality))))
            return (null);

        List<GameObject> returnList = new List<GameObject>();
        if (!checkForInvasionDefense(hex.GetComponent<HexDatabaseFields>().occupyingUnit[0], singleCombat))
            foreach (GameObject attacker in singleCombat.GetComponent<Combat>().attackingUnits)
                foreach (GlobalDefinitions.HexSides hexSides in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
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