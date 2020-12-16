using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class GlobalDefinitions : MonoBehaviour
    {
        public const string releaseVersion = "2.0";

        public static void InitializeFileNames()
        {
            GlobalGameFields.logfile = "TGCOutputFiles\\TGCLogFile.txt";
            GlobalGameFields.hexSetupFile = "TGCBoardSetup\\TGCHexSetup.txt";
            GlobalGameFields.riverSetupFile = "TGCBoardSetup\\TGCRiverSetup.txt";
            GlobalGameFields.hexSettingsFile = "TGCBoardSetup\\TGCHexSettingsSetup.txt";
            GlobalGameFields.mapGraphicsFile = "TGCBoardSetup\\TGCMapGraphicsSetup.txt";
            GlobalGameFields.britainUnitLocationFile = "TGCBoardSetup\\TGCBritainUnitLocation.txt";
            GlobalGameFields.settingsFile = "TGCSettingsFile.txt";
            GlobalGameFields.commandFile = "TGCOutputFiles\\TGCCommandFile.txt";
            GlobalGameFields.fullCommandFile = "TGCOutputFiles\\TGCFullCommandFile.txt";
        }

        // I'm creating a canvas for putting text onto the map
        public static Canvas mapGraphicCanvas;
        public static GameObject mapText;

        // Configuration settings
        public static int difficultySetting;
        public static int aggressiveSetting;

        // Indicates when a command file is being read so that commands are not written
        public static bool commandFileBeingRead = false;

        // Used to adjust strength of victory
        public static int easiestDifficultySettingUsed = 5;

        // Odds used to determine AI attacks
        public static int minimumAIOdds = 2;
        public static int maximumAIOdds = 3;
        public static int maximumAIInvasionOdds = 2;
        public static int minimumAIInvasionOdds = 1;

        // These are the intrinsic hex values used by the AI
        public const int fortressIntrinsicValue = 5;
        public const int cityIntrinsicValue = 5;
        public const int fortifiedZoneIntrinsicValue = 5;
        public const int mountainIntrinsicValue = 3;
        public const int landIntrinsicValue = 1;
        public const int supplySourceValue = 20;
        public const int successfullInvasionHexValue = 20;
        public const int outOfSupplyValue = -20;

        // Determines how much range a HQ unit adds to the supply range.
        // The original rules use 8 but I'm using 11 in order to allow for all major ports to support attacks on Germany
        public const int supplyRangeIncrement = 11;

        // Range to check for attacks
        public const int attackRange = 5;

        // Used by the AI for the distance to use when grouping Allied units
        public const int groupRange = 4;
        public static List<List<GameObject>> alliedGroups = new List<List<GameObject>>();
        public static List<List<GameObject>> germanGroups = new List<List<GameObject>>();
        public static List<GameObject> germanReserves = new List<GameObject>();

        // Context modifiers to the hex value used by the AI
        public const int adjacentZOCHexModifier = 4;
        public const int adjacentUnitHexModifier = adjacentZOCHexModifier - 1;
        public const int abuttedZOCHexModifier = adjacentZOCHexModifier - 1;
        public const int stackedUnitHexModifier = adjacentZOCHexModifier - 2;
        public const int riverHexModifier = cityIntrinsicValue;
        public const int baseEnemyDistanceHexModifier = 6;
        public const int supplyHexModifier = -20;

        // Indicates if either side has met victory conditions
        public static bool alliedVictory = false;
        public static bool germanVictory = false;

        // Keep track of how many factors are lost for strength of victory calculation
        public static int alliedFactorsEliminated = 0;
        public static int germanFactorsEliminated = 0;

        // This determines how far to look for enemy units to determine if a river modifier should be used
        public const int riverModifierDistance = 4;

        // This determines how far to look for an enemy unit when adding a distance to enemy hex modifier
        public const int enemyUnitModiferDistance = 4;

        public const int defenseFactorScalingForFortress = 3;
        public const int defenseFactorScalingForCity = 2;
        public const int defenseFactorScalingForFortifiedZone = 2;
        public const int defenseFactorScalingForRiver = 2;
        public const int defenseFactorScalingForMountain = 2;

        public static int turnsAlliedMetVictoryCondition = 0;

        // Contains the current game mode
        public enum GameModeValues { Hotseat, AI, Peer2PeerNetwork, ClientServerNetwork, Server, EMail }
        public static GameModeValues gameMode;
        public static string commandFileHeader; // Used to log what type of game the command file was generated with
        public static bool gameStarted = false;

        //  Use to determine if Combat Assignment is available
        public static bool AICombat = false;

        // During network games this determines when the player is in control, in AI games it signifies whether the player is in control (true) or the AI (false)
        public static bool localControl = false;
        public static bool userIsIntiating = false;
        public static bool userIsNotInitiating = false;
        public static bool isServer = false;
        public static bool hasReceivedConfirmation = false;
        public static Nationality sideControled; // Used for network play

        // Buttons and Toggles - each UI element that is created that can accept a user input must be referenced in a global since this is what will
        // be used for network games to execute the gui elements
        public static UnityEngine.UI.Button TypeOfGameYesButton;
        public static UnityEngine.UI.Button TypeOfGameNoButton;
        public static UnityEngine.UI.Button SecondInvasionYesButton;
        public static UnityEngine.UI.Button SecondInvasionNoButton;

        // These are set by the player
        public static GameObject difficultySettingText;
        public static GameObject aggressivenessSettingText;

        // Buttons and Toggles on the static gui
        // Status displays
        public static GameObject nextPhaseButton = GameObject.Find("NextPhaseButton");
        public static GameObject mainMenuButton = GameObject.Find("MainMenuButton");
        public static GameObject quitButton = GameObject.Find("QuitButton");
        public static GameObject undoButton = GameObject.Find("UndoButton");

        public static GameObject MustAttackToggle = GameObject.Find("MustAttackToggle");
        public static GameObject AssignCombatButton = GameObject.Find("LoadCombatButton");
        public static GameObject DisplayAllCombatsButton = GameObject.Find("ResolveCombatButton");
        public static GameObject AlliedSupplyRangeToggle = GameObject.Find("AlliedSupplyToggle");
        public static GameObject GermanSupplyRangeToggle = GameObject.Find("GermanSupplyToggle");
        public static GameObject AlliedSupplySourcesButton = GameObject.Find("SupplySourcesButton");
        public static GameObject SettingsButton = GameObject.Find("SettingsButton");
        public static GameObject HideUnitsToggle = GameObject.Find("HideUnitsToggle");
        public static GameObject HistoricalProgressToggle = GameObject.Find("ShowHistoryToggle");


        public const int AirborneDropHexLimit = 5;
        public const int NormalAirborneDropLimit = 3;
        public const int GUIUNITIMAGESIZE = 50;
        public const float GUIXOFFSET = GUIUNITIMAGESIZE;
        public const float GUIYOFFSET = GUIUNITIMAGESIZE;
        public const int maxNumberOfCarpetBombings = 4;
        public const int maxNumberOfTacticalAirMissions = 6;
        public const int maxNumberAlliedReinforcementPerTurn = 12;
        public const int germanReplacementsPerTurn = 5;

        // Settings empirically derived to position the map properly
        public const float boardOffsetX = 20.52489f;
        public const float boardOffsetY = 16.1363f;

        // These are the colors of the hex highlights
        public static Vector4 TacticalAirCloseDefenseHighlightColor = new Vector4(1f, 0, 0, 0.5f); // red
        public static Vector4 TacticalAirRiverInterdictionHighlightColor = new Vector4(1f, 0, 1f, 0.5f); // magenta
        public static Vector4 SuccessfulInvasionSiteColor = new Vector4(0.5f, 0.5f, 0.5f, 0.5f); // cyan
        public static Vector4 StrategicInstallationHexColor = new Vector4(0, 1f, 0, 0.5f); //green
        public static Vector4 HexInSupplyHighlightColor = new Vector4(0f, 0f, 1f, 0.15f); // blue
        public static Vector4 HexAvailableForMovmentColor = new Vector4(1f, 0.922f, 0.016f, 0.5f); // yellow
        public static Vector4 HexOverstackedColor = new Vector4(1f, 0.922f, 0.016f, 0.5f); // yellow

        public enum Nationality { German, Allied }
        public static Nationality nationalityUserIsPlaying;

        public const int GermanStackingLimit = 3;
        public const int AlliedStackingLimit = 2;

        public enum CombatResults { Aelim, Aback2, Exchange, Dback2, Delim }
        public static List<CombatResults> combatResultsFromLastTurn = new List<CombatResults>();

        // Note that the second demension is determined by the die roll (0-5)
        public static CombatResults[,] combatResultsTable = new CombatResults[,] {
            {CombatResults.Aelim, CombatResults.Aelim, CombatResults.Aelim, CombatResults.Aelim, CombatResults.Aelim, CombatResults.Aelim },
            {CombatResults.Aelim, CombatResults.Aelim, CombatResults.Aback2, CombatResults.Aelim, CombatResults.Aelim, CombatResults.Aelim },
            {CombatResults.Aelim, CombatResults.Aelim, CombatResults.Aback2, CombatResults.Aback2, CombatResults.Aelim, CombatResults.Aelim },
            {CombatResults.Aback2, CombatResults.Aelim, CombatResults.Aback2, CombatResults.Aback2, CombatResults.Aelim, CombatResults.Aelim },
            {CombatResults.Aback2, CombatResults.Aback2, CombatResults.Aback2, CombatResults.Aback2, CombatResults.Aelim, CombatResults.Aelim },
            {CombatResults.Dback2, CombatResults.Exchange, CombatResults.Aback2, CombatResults.Aback2, CombatResults.Aelim, CombatResults.Aelim },
            {CombatResults.Delim, CombatResults.Exchange, CombatResults.Dback2, CombatResults.Aback2, CombatResults.Aelim, CombatResults.Aelim },
            {CombatResults.Delim, CombatResults.Exchange, CombatResults.Dback2, CombatResults.Aback2, CombatResults.Exchange, CombatResults.Aelim },
            {CombatResults.Delim, CombatResults.Exchange, CombatResults.Dback2, CombatResults.Dback2, CombatResults.Exchange, CombatResults.Delim },
            {CombatResults.Delim, CombatResults.Exchange, CombatResults.Delim, CombatResults.Dback2, CombatResults.Dback2, CombatResults.Delim },
            {CombatResults.Delim, CombatResults.Dback2, CombatResults.Delim, CombatResults.Dback2, CombatResults.Delim, CombatResults.Delim },
            {CombatResults.Delim, CombatResults.Dback2, CombatResults.Delim, CombatResults.Delim, CombatResults.Delim, CombatResults.Delim },
            {CombatResults.Delim, CombatResults.Delim, CombatResults.Delim, CombatResults.Delim, CombatResults.Delim, CombatResults.Delim }
            };

        public static System.Random dieRoll = new System.Random();
        public static int dieRollResult1, dieRollResult2;
        public static string CombatResultToggleName;
        public static float exchangeFactorsSelected = 0;
        public static int exchangeFactorsToLose = 0;

        public static GameObject combatContentPanel;
        public static GameObject getGameModeGuiInstance;
        public static GameObject combatResolutionGUIInstance;
        public static GameObject combatGUIInstance;
        public static GameObject ExchangeGUIInstance;
        public static GameObject combatResolutionOKButton;
        public static GameObject postCombatMovementGuiInstance;
        public static GameObject tacticalAirGUIInstance;
        public static GameObject invasionAreaSelectionGUIInstance;
        public static GameObject supplySourceGUIInstance;
        public static GameObject combatAirSupportToggle;
        public static GameObject combatCarpetBombingToggle;
        public static GameObject newGameToggle;
        public static GameObject savedGameToggle;
        public static GameObject commandFileToggle;

        public static bool combatResolutionStarted = false;

        public static int turnNumber = 0;

        // The following is set to the Unity static object that contains all units placed on the board
        public static GameObject allUnitsOnBoard;

        // The following lists are loaded at the start of each turn and hold all units of each nationality on the board
        public static List<GameObject> alliedUnitsOnBoard = new List<GameObject>();
        public static List<GameObject> germanUnitsOnBoard = new List<GameObject>();

        // Stores the combats that are going to be made
        public static List<GameObject> allCombats = new List<GameObject>();

        public static GameObject currentSupplySource;

        public static List<GameObject> supplyGUI = new List<GameObject>();

        public static List<GameObject> retreatingUnits = new List<GameObject>();
        public static List<GameObject> retreatingUnitsBeginningHexes = new List<GameObject>();

        public static List<GameObject> dback2Defenders = new List<GameObject>();
        public static List<GameObject> dback2Attackers = new List<GameObject>();

        public static List<GameObject> hexesAvailableForPostCombatMovement = new List<GameObject>();
        public static GameObject unitSelectedForPostCombatMovement;

        public static GameObject selectedUnit = new GameObject("selectedUnit1");
        public static GameObject startHex = new GameObject("selectedUnit2");

        public static List<GameObject> unitsToExchange = new List<GameObject>();

        // Used to determine hexes that are available for carpet bombing
        public static List<GameObject> hexesAttackedLastTurn = new List<GameObject>();
        public static int numberOfCarpetBombingsUsed = 0;
        public static bool carpetBombingUsedThisTurn = false;

        // Used by the AI to determine if it is stuck
        public static int numberOfTurnsWithoutSuccessfulAttack = 0;

        public static bool invasionsTookPlaceThisTurn = false;
        public static int maxNumberAirborneDropsThisTurn = 3;
        public static int numberAlliedReinforcementsLandedThisTurn = 0;
        public static int currentAirborneDropsThisTurn = 0;

        public static int numberOfHexesInAlliedControl = 0;

        public static int tacticalAirMissionsThisTurn = 0;
        public static GameObject numberTacticalAirFactorsRemainingText;
        public static List<GameObject> riverInderdictedHexes = new List<GameObject>();
        public static List<GameObject> interdictedUnits = new List<GameObject>();
        public static List<GameObject> closeDefenseHexes = new List<GameObject>();

        public static InvasionArea[] invasionAreas = new InvasionArea[7];
        public static int numberInvasionsExecuted = 0;
        public static int firstInvasionAreaIndex = -1;
        public static int secondInvasionAreaIndex = -1;

        public static int germanReplacementsRemaining = 0;
        public static int alliedReplacementsRemaining = 0;

        // The following booleans are used to determine the number of allied reinforments
        public static bool alliedCapturedBrest = false;
        public static bool alliedCapturedBoulogne = false;
        public static bool alliedCapturedRotterdam = false;

        // Set during supply phase - shows all the avialable ports and inland ports available for landing reinforcements in the turn
        public static List<GameObject> availableReinforcementPorts = new List<GameObject>();
        public static List<GameObject> supplySources = new List<GameObject>();
        public static List<GameObject> unassignedTextObejcts = new List<GameObject>();

        public static GameObject chatPanel; // Need to grab the chat panel and store it since the default will have it turned off and I can't find it with it off

        public static InputField fileBrowserSelectionLabel;

        public static bool displayAlliedSupplyStatus = false;
        public static bool displayGermanSupplyStatus = false;

        public static bool AIExecuting;

        public static int germanSetupFileUsed = 100;

        /// <summary>
        /// Goes through and resets all variables 
        /// </summary>
        public static void ResetAllGlobalDefinitions()
        {
            alliedGroups.Clear();
            germanGroups.Clear();
            germanReserves.Clear();

            turnsAlliedMetVictoryCondition = 0;

            TypeOfGameYesButton = null;
            TypeOfGameNoButton = null;
            SecondInvasionYesButton = null;
            SecondInvasionNoButton = null;

            combatResultsFromLastTurn.Clear();

            exchangeFactorsSelected = 0;
            exchangeFactorsToLose = 0;

            combatResolutionStarted = false;

            turnNumber = 0;

            alliedUnitsOnBoard.Clear();
            germanUnitsOnBoard.Clear();

            allCombats.Clear();

            currentSupplySource = null;
            supplyGUI.Clear();

            retreatingUnits.Clear();
            retreatingUnitsBeginningHexes.Clear();

            dback2Defenders.Clear();
            dback2Attackers.Clear();

            hexesAvailableForPostCombatMovement.Clear();
            unitSelectedForPostCombatMovement = null;

            selectedUnit = null;
            startHex = null;

            unitsToExchange.Clear();

            hexesAttackedLastTurn.Clear();
            combatResultsFromLastTurn.Clear();
            numberOfCarpetBombingsUsed = 0;
            carpetBombingUsedThisTurn = false;

            numberOfTurnsWithoutSuccessfulAttack = 0;

            invasionsTookPlaceThisTurn = false;
            maxNumberAirborneDropsThisTurn = 3;
            numberAlliedReinforcementsLandedThisTurn = 0;
            currentAirborneDropsThisTurn = 0;

            tacticalAirMissionsThisTurn = 0;
            numberTacticalAirFactorsRemainingText = null;
            riverInderdictedHexes.Clear();
            interdictedUnits.Clear();
            closeDefenseHexes.Clear();

            numberInvasionsExecuted = 0;
            firstInvasionAreaIndex = -1;
            secondInvasionAreaIndex = -1;

            germanReplacementsRemaining = 0;
            alliedReplacementsRemaining = 0;

            alliedCapturedBrest = false;
            alliedCapturedBoulogne = false;
            alliedCapturedRotterdam = false;

            availableReinforcementPorts.Clear();
            supplySources.Clear();
            unassignedTextObejcts.Clear();

            displayAlliedSupplyStatus = false;
            displayGermanSupplyStatus = false;

            alliedVictory = false;
            germanVictory = false;

            alliedFactorsEliminated = 0;
            germanFactorsEliminated = 0;

            foreach (InvasionArea area in invasionAreas)
            {
                area.airborneUnitsUsedThisTurn = 0;
                area.airborneUnitsUsedThisTurn = 0;
                area.infantryUnitsUsedThisTurn = 0;
                area.infantryUsedAsArmorThisTurn = 0;
                area.airborneUsedAsInfantryThisTurn = 0;
                area.invaded = false;
                area.secondInvasionArea = false;
                area.totalUnitsUsedThisTurn = 0;
                area.turn = 0;
            }

            germanSetupFileUsed = 100;

            // The following is for resetting the variables associated with a network game
            //TransportScript.remoteComputerIPAddress = "";
            //userIsIntiating = false;
            //isServer = false;
            //hasReceivedConfirmation = false;
            //gameStarted = false;
            //TransportScript.channelRequested = false;
            //TransportScript.connectionConfirmed = false;
            //TransportScript.handshakeConfirmed = false;
            //TransportScript.opponentComputerConfirmsSync = false;
            //TransportScript.gameDataSent = false;

            // When resetting I am going to regenerate the invasion areas.  If I don't the AI will come up with different results based on the arrays being seeded diferently
            GameControl.createBoardInstance.GetComponent<CreateBoard>().SetupInvasionAreas();

        }

        /// <summary>
        /// Draws a blue line representing a river between the two points passed
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        public static void DrawBlueLineBetweenTwoPoints(Vector3 point1, Vector3 point2)
        {
            Material lineMaterial = Resources.Load("LineMaterial", typeof(Material)) as Material;
            GameObject river = new GameObject("DrawBlueLineBetweenTwoPoints");

            if (lineMaterial == null)
                WriteToLogFile("DrawBlueLineBetweenTwoPoints: ERROR - Material returned null from Resources");

            river.layer = LayerMask.NameToLayer("River");
            river.name = "River";
            river.transform.SetParent(GameObject.Find("Rivers").transform);

            Vector3[] linePositions = new Vector3[2];
            linePositions[0] = point1;
            linePositions[1] = point2;

            river.AddComponent<LineRenderer>();
            river.GetComponent<LineRenderer>().useWorldSpace = true;
            river.GetComponent<LineRenderer>().startColor = Color.blue;
            river.GetComponent<LineRenderer>().endColor = Color.blue;
            river.GetComponent<LineRenderer>().positionCount = 2;
            river.GetComponent<LineRenderer>().startWidth = 0.5f;
            river.GetComponent<LineRenderer>().endWidth = 0.5f;
            river.GetComponent<LineRenderer>().numCapVertices = 10;
            river.GetComponent<LineRenderer>().material = lineMaterial;
            river.GetComponent<LineRenderer>().SetPositions(linePositions);
            river.GetComponent<LineRenderer>().sortingLayerName = "River";
        }

        /// <summary>
        /// This routine will return the index that corresponds to one hexside counter clockwise
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static int ReturnHexSideCounterClockwise(int side)
        {
            if (side == 0)
                return (5);
            else
                return (side - 1);
        }

        /// <summary>
        /// This routine will return the index that corresponds to 2 hexsides counter clockwise
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static int ReturnHexSide2CounterClockwise(int side)
        {
            if (side == 0)
                return (4);
            else if (side == 1)
                return (5);
            else
                return (side - 2);
        }

        /// <summary>
        /// This routine will return the index that corresponds to one hexside clockwise
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static int ReturnHexSideClockwise(int side)
        {
            if (side == 5)
                return (0);
            else
                return (side + 1);
        }

        /// <summary>
        /// This routine will return the index that corresponds to 2 hexsides clockwise
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static int ReturnHex2SideClockwise(int side)
        {
            if (side == 5)
                return (1);
            else if (side == 4)
                return (0);
            else
                return (side + 2);
        }

        /// <summary>
        /// This routine will return the index that corresponds to the opposite hexside 
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static int ReturnHexSideOpposide(int side)
        {
            if (side < 3)
                return (side + 3);
            else
                return (side - 3);
        }

        /// <summary>
        /// This routine will return the index that corresponds to 4 hexsides clockwise
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public static int ReturnHex4SideClockwise(int side)
        {
            if (side == 5)
                return (3);
            else if (side == 4)
                return (2);
            else if (side == 3)
                return (1);
            else if (side == 2)
                return (0);
            else
                return (side + 4);
        }

        /// <summary>
        /// Returns true if the two hexes passed are adjacent
        /// </summary>
        /// <param name="hex1"></param>
        /// <param name="hex2"></param>
        /// <returns></returns>
        public static bool TwoHexesAdjacent(GameObject hex1, GameObject hex2)
        {
            foreach (HexDefinitions.HexSides hexSides in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                if (hex1.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] != null)
                    if (hex2 == hex1.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides])
                        return (true);
            return (false);
        }

        /// <summary>
        /// Returns true if the two units passed are on adjacent hexes
        /// </summary>
        /// <param name="unit1"></param>
        /// <param name="unit2"></param>
        /// <returns></returns>
        public static bool TwoUnitsAdjacent(GameObject unit1, GameObject unit2)
        {
            foreach (HexDefinitions.HexSides hexSides in Enum.GetValues(typeof(HexDefinitions.HexSides)))
            {
                if (unit1.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] != null)
                {
                    for (int index = 0; index < unit1.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                    {
                        if (unit1.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides].GetComponent<HexDatabaseFields>().occupyingUnit[index] == unit2)
                        {
                            return (true);
                        }
                    }
                }
            }
            return (false);
        }

        /// <summary>
        /// Returns a string with the odds formatted correctly
        /// </summary>
        /// <param name="odds"></param>
        /// <returns></returns>
        public static string ConvertOddsToString(int odds)
        {
            if (odds == 0)
                return (" ");
            // Positive numbers indicate that the attacker is stronger than the defender
            if (odds > 0)
                return (odds.ToString() + ":1");

            // Negative numbers indicate that the defender is stronger than the attacker
            else
            {
                odds = -odds;
                return ("1:" + odds.ToString());
            }
        }

        /// <summary>
        /// Returns true if there were successful attacks last turn
        /// </summary>
        /// <returns></returns>
        public static bool SuccessfulAttacksLastTurn()
        {
            if (combatResultsFromLastTurn.Count == 0)
                return (false);

            foreach (CombatResults combatResult in combatResultsFromLastTurn)
                if ((combatResult == CombatResults.Dback2) || (combatResult == CombatResults.Delim) || (combatResult == CombatResults.Exchange))
                    return (true);

            return (false);
        }

        /// <summary>
        /// Returns the opposite nationality passed
        /// </summary>
        /// <param name="nationality"></param>
        /// <returns></returns>
        public static Nationality ReturnOppositeNationality(Nationality nationality)
        {
            if (nationality == Nationality.Allied)
                return (Nationality.German);
            else
                return (Nationality.Allied);
        }

        /// <summary>
        /// Takes a unit from the board and moves it to the OOB sheet (the dead pile)
        /// </summary>
        /// <param name="unit"></param>
        public static void MoveUnitToDeadPile(GameObject unit)
        {
            // Remove the unit from the hex
            GeneralHexRoutines.RemoveUnitFromHex(unit, unit.GetComponent<UnitDatabaseFields>().occupiedHex);

            // The removeUnitFromHex routine takes care of a lot of the fields but we need to remove others because
            // through testing I have encountered issues when the fields are not reset.
            unit.GetComponent<UnitDatabaseFields>().beginningTurnHex = null;
            unit.GetComponent<UnitDatabaseFields>().supplySource = null;
            if (unit.GetComponent<UnitDatabaseFields>().supplySource != null)
                unit.GetComponent<UnitDatabaseFields>().supplySource.GetComponent<HexDatabaseFields>().unassignedSupply++;

            // Remove the unit from the OnBoard list
            if (unit.GetComponent<UnitDatabaseFields>().nationality == Nationality.Allied)
                alliedUnitsOnBoard.Remove(unit);
            else
                germanUnitsOnBoard.Remove(unit);

            // Keep track of the number of factors lost
            if (unit.GetComponent<UnitDatabaseFields>().nationality == Nationality.Allied)
                alliedFactorsEliminated += (unit.GetComponent<UnitDatabaseFields>().attackFactor + unit.GetComponent<UnitDatabaseFields>().defenseFactor);
            else
                germanFactorsEliminated += (unit.GetComponent<UnitDatabaseFields>().attackFactor + unit.GetComponent<UnitDatabaseFields>().defenseFactor);

            // Move the unit to the order of battle sheet
            ReturnUnitToOOBShet(unit);

            // Reset flags
            unit.GetComponent<UnitDatabaseFields>().inSupply = true;
            unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex = -1;
            unit.GetComponent<UnitDatabaseFields>().hasMoved = false;
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;

            // Make sure highlighting is turned off on the unit
            UnhighlightUnit(unit);

            // Set the flag to indicate the unit is eliminated
            unit.GetComponent<UnitDatabaseFields>().unitEliminated = true;
        }

        /// <summary>
        /// Highlights the hex yellow - used for movement
        /// </summary>
        /// <param name="hex"></param>
        public static void HighlightHexForMovement(GameObject hex)
        {
            Renderer targetRenderer = hex.GetComponent(typeof(SpriteRenderer)) as Renderer;
            //hex.transform.localScale = new Vector2(0.75f, 0.75f);
            targetRenderer.sortingLayerName = "Highlight";
            targetRenderer.material.color = HexAvailableForMovmentColor;
            targetRenderer.sortingOrder = 2;
        }

        /// <summary>
        /// Used to highlight a hex passed that is overstacked
        /// </summary>
        /// <param name="hex"></param>
        public static void HighlightOverstackedHex(GameObject hex)
        {
            Renderer targetRenderer = hex.GetComponent(typeof(SpriteRenderer)) as Renderer;
            //hex.transform.localScale = new Vector2(0.75f, 0.75f);
            targetRenderer.sortingLayerName = "Highlight";
            targetRenderer.material.color = HexOverstackedColor;
            targetRenderer.sortingOrder = 2;
        }

        /// <summary>
        /// Highlights the hex blue - used for supply
        /// </summary>
        /// <param name="hex"></param>
        public static void HighlightHexInSupply(GameObject hex)
        {
            // Don't highlight a hex as in supply if a unit is on the hex - looks bad and isn't needed
            // If a unit is on a hex and it's out of supply it is highlighted black
            // Also check if the hex is already highlighted yellow (meaning the user is in the middle of moving a unit)
            if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0) &&
                (Convert.ToString(hex.GetComponent<SpriteRenderer>().material.color) != "RGBA(1.000, 0.922, 0.016, 0.500)"))
            {
                Renderer targetRenderer = hex.GetComponent(typeof(SpriteRenderer)) as Renderer;
                //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                targetRenderer.sortingLayerName = "Highlight";
                targetRenderer.material.color = HexInSupplyHighlightColor;
                targetRenderer.sortingOrder = 2;
            }
        }

        /// <summary>
        /// Resets all hexes on the board
        /// </summary>
        public static void UnhighlightAllHexes()
        {
            foreach (GameObject cleanHex in HexDefinitions.allHexesOnBoard)
            {
                cleanHex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                cleanHex.GetComponent<HexDatabaseFields>().availableForMovement = false;
                UnhighlightHex(cleanHex.gameObject);
            }
        }

        /// <summary>
        /// Takes away all the highlights that should be removed of a hex
        /// </summary>
        /// <param name="hex"></param>
        public static void UnhighlightHex(GameObject hex)
        {
            Renderer targetRenderer = hex.GetComponent(typeof(SpriteRenderer)) as Renderer;

            hex.transform.localScale = new Vector2(1f, 1f);
            targetRenderer.sortingLayerName = "Hex";
            targetRenderer.material.color = Color.white;
            targetRenderer.sortingOrder = 0;

            // Check for highlights that need to remain - air targets and supply

            if (displayAlliedSupplyStatus && HexInAlliedSupply(hex))
                HighlightHexInSupply(hex);

            else if (hex.GetComponent<HexDatabaseFields>().successfullyInvaded)
            {
                //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                targetRenderer.sortingLayerName = "Hex";
                targetRenderer.material.color = SuccessfulInvasionSiteColor;
                targetRenderer.sortingOrder = 2;
            }
            else if (hex.GetComponent<HexDatabaseFields>().closeDefenseSupport)
            {
                //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                targetRenderer.sortingLayerName = "Hex";
                targetRenderer.material.color = TacticalAirCloseDefenseHighlightColor;
                targetRenderer.sortingOrder = 2;
            }
            else if (hex.GetComponent<HexDatabaseFields>().riverInterdiction)
            {
                //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                targetRenderer.sortingLayerName = "Hex";
                targetRenderer.material.color = TacticalAirRiverInterdictionHighlightColor;
                targetRenderer.sortingOrder = 2;
            }
            // If it is an Allied replacement hex it is highlighted green - Rotterdam 8,23 Boulogne 14,16 Brest 22,1
            else if (((hex == GeneralHexRoutines.GetHexAtXY(22, 1)) && !alliedCapturedBrest) ||
                    ((hex == GeneralHexRoutines.GetHexAtXY(14, 16)) && !alliedCapturedBoulogne) ||
                    ((hex == GeneralHexRoutines.GetHexAtXY(8, 23)) && !alliedCapturedRotterdam))
            {
                //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                targetRenderer.sortingLayerName = "Hex";
                targetRenderer.material.color = StrategicInstallationHexColor;
                targetRenderer.sortingOrder = 2;
            }
        }

        /// <summary>
        /// This routine unhighlights any hexes that are showing supply range.  This is needed for the scenario where
        /// a user turns off the supply highlight in the middle of a move.
        /// </summary>
        /// <param name="hex"></param>
        public static void UnhighlightHexSupplyRange(GameObject hex)
        {

            // Only change the highlighting if the hex is highlighted for supply
            if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count == 0) && (Convert.ToString(hex.GetComponent<SpriteRenderer>().material.color) == "RGBA(0.000, 0.000, 1.000, 0.150)"))
            {
                Renderer targetRenderer = hex.GetComponent(typeof(SpriteRenderer)) as Renderer;

                hex.transform.localScale = new Vector2(1f, 1f);
                targetRenderer.sortingLayerName = "Hex";
                targetRenderer.material.color = Color.white;
                targetRenderer.sortingOrder = 0;

                // Check for highlights that need to remain - air targets and supply

                if (displayAlliedSupplyStatus && HexInAlliedSupply(hex))
                    HighlightHexInSupply(hex);

                else if (hex.GetComponent<HexDatabaseFields>().successfullyInvaded)
                {
                    //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                    targetRenderer.sortingLayerName = "Hex";
                    targetRenderer.material.color = SuccessfulInvasionSiteColor;
                    targetRenderer.sortingOrder = 2;
                }
                else if (hex.GetComponent<HexDatabaseFields>().closeDefenseSupport)
                {
                    //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                    targetRenderer.sortingLayerName = "Hex";
                    targetRenderer.material.color = TacticalAirCloseDefenseHighlightColor;
                    targetRenderer.sortingOrder = 2;
                }
                else if (hex.GetComponent<HexDatabaseFields>().riverInterdiction)
                {
                    //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                    targetRenderer.sortingLayerName = "Hex";
                    targetRenderer.material.color = TacticalAirRiverInterdictionHighlightColor;
                    targetRenderer.sortingOrder = 2;
                }
                // If it is an Allied repalcement hex it is highlighted green - Rotterdam 8,23 Boulogne 14,16 Brest 22,1
                else if (((hex == GeneralHexRoutines.GetHexAtXY(22, 1)) && !alliedCapturedBrest) ||
                    ((hex == GeneralHexRoutines.GetHexAtXY(14, 16)) && !alliedCapturedBoulogne) ||
                    ((hex == GeneralHexRoutines.GetHexAtXY(8, 23)) && !alliedCapturedRotterdam))
                {
                    //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                    targetRenderer.sortingLayerName = "Hex";
                    targetRenderer.material.color = StrategicInstallationHexColor;
                    targetRenderer.sortingOrder = 2;
                }
            }
        }

        /// <summary>
        /// This routine will return true if the hex is in the ZOC of the opposite nationality passed
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="friendlyNationality"></param>
        /// <returns></returns>
        public static bool HexInEnemyZOC(GameObject hex, Nationality friendlyNationality)
        {
            if (friendlyNationality == Nationality.Allied)
            {
                if (hex.GetComponent<HexDatabaseFields>().inGermanZOC)
                    return true;
            }
            else
            {
                if (hex.GetComponent<HexDatabaseFields>().inAlliedZOC)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// This routine will return true if the hex is in the ZOC of the nationality passed
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="nationality"></param>
        /// <returns></returns>
        public static bool HexInFriendlyZOC(GameObject hex, Nationality nationality)
        {
            if (nationality == Nationality.Allied)
            {
                if (hex.GetComponent<HexDatabaseFields>().inAlliedZOC)
                    return true;
            }
            else
            {
                if (hex.GetComponent<HexDatabaseFields>().inGermanZOC)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if Allied supply available on the hex passed
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static bool HexInAlliedSupply(GameObject hex)
        {
            if (hex.GetComponent<HexDatabaseFields>().supplySources.Count > 0)
                return true;
            else
                return false;
        }

        ///// <summary>
        ///// This routine is used to return the attacker factor of the unit passed
        ///// It checks if the unit is out of supply
        ///// </summary>
        ///// <param name="unit"></param>
        ///// <returns></returns>
        //public static float ReturnAttackFactor(GameObject unit)
        //{
        //    if (unit.GetComponent<UnitDatabaseFields>().inSupply)
        //    {
        //        //writeToLogFile("returnAttackFactor: unit " + unit.name + " returning attack factor = " + unit.GetComponent<UnitDatabaseFields>().attackFactor);
        //        return (unit.GetComponent<UnitDatabaseFields>().attackFactor);
        //    }
        //    else
        //    {
        //        //writeToLogFile("returnAttackFactor: unit " + unit.name + " returning attack factor = " + (unit.GetComponent<UnitDatabaseFields>().attackFactor/2));
        //        return (unit.GetComponent<UnitDatabaseFields>().attackFactor / 2);  // Need to check on this, I think the attack factor is one if out of supply
        //    }
        //}

        /// <summary>
        /// This is a routine use to unhighlight a unit.  It checks for supply status
        /// to determine if it should be white or gray
        /// </summary>
        /// <param name="unit"></param>
        public static void UnhighlightUnit(GameObject unit)
        {
            if (unit.GetComponent<UnitDatabaseFields>().inSupply)
                unit.GetComponent<SpriteRenderer>().material.color = Color.white;
            else
                unit.GetComponent<SpriteRenderer>().material.color = Color.gray;
        }

        /// <summary>
        /// Removes highlighting on all units
        /// </summary>
        public static void UnhighlightAllUnits()
        {
            foreach (GameObject unit in alliedUnitsOnBoard)
                UnhighlightUnit(unit);
            foreach (GameObject unit in germanUnitsOnBoard)
                UnhighlightUnit(unit);
        }

        /// <summary>
        /// Highlights unit based on whether it is a German or Allied unit since they need different colors
        /// </summary>
        /// <param name="unit"></param>
        public static void HighlightUnit(GameObject unit)
        {
            if (unit.GetComponent<UnitDatabaseFields>().nationality == Nationality.Allied)
                unit.GetComponent<SpriteRenderer>().material.color = Color.red;
            else
                unit.GetComponent<SpriteRenderer>().material.color = Color.cyan;
        }

        /// <summary>
        /// Used to determine the number of HQ units on the hex passed
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static int NumberHQOnHex(GameObject hex)
        {
            int numberOnHex = 0;
            for (int index = 0; index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().HQ)
                    numberOnHex++;
            return (numberOnHex);
        }

        /// <summary>
        /// Returns true if no German units are on the inland port's control hexes
        /// </summary>
        /// <param name="inlandPort"></param>
        /// <returns></returns>
        public static bool CheckIfInlandPortClear(GameObject inlandPort)
        {
            foreach (GameObject hex in inlandPort.GetComponent<HexDatabaseFields>().controlHexes)
                if ((hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                        (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German))
                    return false;
            return true;
        }

        /// <summary>
        /// Pulls up gui for the player to select new or saved game or run the command file
        /// </summary>
        public static void GetNewOrSavedGame()
        {
            UnityEngine.UI.Button okButton;
            GameObject tempText;

            float panelWidth = 6 * GUIUNITIMAGESIZE;
            float panelHeight = 5 * GUIUNITIMAGESIZE;
            Canvas getNewSaveGameCanvas = new Canvas();
            GUIRoutines.CreateGUICanvas("NewSaveGameCanvas",
                    panelWidth,
                    panelHeight,
                    ref getNewSaveGameCanvas);

            // This gui has two columns, selection toggles and desription
            tempText = GUIRoutines.CreateUIText("Select", "NewSaveGameSelectText",
                    GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    4.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white,
                    getNewSaveGameCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            tempText = GUIRoutines.CreateUIText("Game Type", "NewSaveGameDescriptionText",
                    4 * GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    4.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white,
                    getNewSaveGameCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

            // Now list the four game modes
            newGameToggle = GUIRoutines.CreateToggle("NewGameToggle",
                    GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    3.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getNewSaveGameCanvas);

            tempText = GUIRoutines.CreateUIText("New Game", "NewGameDescriptionText",
                    4 * GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    3.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white,
                    getNewSaveGameCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            newGameToggle.gameObject.AddComponent<GameTypeSelectionButtonRoutines>();
            newGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => newGameToggle.gameObject.GetComponent<GameTypeSelectionButtonRoutines>().ToggleChange());

            savedGameToggle = GUIRoutines.CreateToggle("SavedGameToggle",
                    GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    2.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getNewSaveGameCanvas);

            tempText = GUIRoutines.CreateUIText("Saved Game", "SavedGameDescriptionText",
                    4 * GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    2.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white,
                    getNewSaveGameCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            savedGameToggle.gameObject.AddComponent<GameTypeSelectionButtonRoutines>();
            savedGameToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => savedGameToggle.gameObject.GetComponent<GameTypeSelectionButtonRoutines>().ToggleChange());


            commandFileToggle = GUIRoutines.CreateToggle("commandFileToggle",
                    GUIUNITIMAGESIZE * 1 - (0.5f * panelWidth),
                    1.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getNewSaveGameCanvas);
            tempText = GUIRoutines.CreateUIText("Restart Last Game Played", "CommandFileDescriptionText",
                    4 * GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE,
                    GUIUNITIMAGESIZE * 4 - (0.5f * panelWidth),
                    1.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    Color.white,
                    getNewSaveGameCanvas);
            tempText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            commandFileToggle.gameObject.AddComponent<GameTypeSelectionButtonRoutines>();
            commandFileToggle.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => commandFileToggle.gameObject.GetComponent<GameTypeSelectionButtonRoutines>().ToggleChange());

            // Add an OK button
            okButton = GUIRoutines.CreateButton("getNewSaveGameOKButton", "OK",
                    GUIUNITIMAGESIZE * 3 - (0.5f * panelWidth),
                    0.5f * GUIUNITIMAGESIZE - (0.5f * panelHeight),
                    getNewSaveGameCanvas);
            okButton.gameObject.AddComponent<GameTypeSelectionButtonRoutines>();
            okButton.onClick.AddListener(okButton.GetComponent<GameTypeSelectionButtonRoutines>().NewSavedGameOK);
        }

        /// <summary>
        /// This routine asks the question passed.  It returns true for yes and false for no
        /// This version takes two delegates passed for execution when a button is clicked
        /// </summary>
        /// <param name="question"></param>
        /// <param name="yesButton"></param>
        /// <param name="noButton"></param>
        /// <param name="yesMethod"></param>
        /// <param name="noMethod"></param>
        public static void AskUserYesNoQuestion(string question, ref UnityEngine.UI.Button yesButton, ref UnityEngine.UI.Button noButton, UnityEngine.Events.UnityAction yesMethod, UnityEngine.Events.UnityAction noMethod, float panelWidth = 2f, float panelHeight = 3f)
        {
            Canvas questionCanvas = new Canvas();
            panelWidth = panelWidth * GUIUNITIMAGESIZE;
            panelHeight = panelHeight * GUIUNITIMAGESIZE;
            GUIRoutines.CreateGUICanvas("YesNoCanvas", panelWidth, panelHeight, ref questionCanvas);
            GUIRoutines.CreateUIText(question, "YesNoQuestionText",
                panelWidth,
                panelHeight - GUIUNITIMAGESIZE,
                0,
                0.5f * GUIUNITIMAGESIZE,
                Color.white,
                questionCanvas);

            yesButton = GUIRoutines.CreateButton("YesButton", "Yes",
                -0.5f * GUIUNITIMAGESIZE,
                0.5f * GUIUNITIMAGESIZE - 0.5f * panelHeight,
                questionCanvas);
            yesButton.gameObject.AddComponent<YesNoButtonRoutines>();
            yesButton.gameObject.GetComponent<YesNoButtonRoutines>().yesAction = yesMethod;
            yesButton.onClick.AddListener(yesButton.GetComponent<YesNoButtonRoutines>().YesButtonSelected);

            noButton = GUIRoutines.CreateButton("NoButton", "No",
                0.5f * GUIUNITIMAGESIZE,
                0.5f * GUIUNITIMAGESIZE - 0.5f * panelHeight,
                questionCanvas);
            noButton.gameObject.AddComponent<YesNoButtonRoutines>();
            noButton.gameObject.GetComponent<YesNoButtonRoutines>().noAction = noMethod;
            noButton.onClick.AddListener(noButton.GetComponent<YesNoButtonRoutines>().NoButtonSelected);
        }

        /// <summary>
        /// Pulls up a gui for the user to select which side to play - German or Ally
        /// </summary>
        public static void AskUserWhichSideToPlay()
        {
            Canvas questionCanvas = new Canvas();
            float panelWidth = 2 * GUIUNITIMAGESIZE;
            float panelHeight = 3 * GUIUNITIMAGESIZE;
            GUIRoutines.CreateGUICanvas("ChooseSideCanvas", panelWidth, panelHeight, ref questionCanvas);
            GUIRoutines.CreateUIText("Which side are you playing?", "ChooseSideText",
                2 * GUIUNITIMAGESIZE,
                2 * GUIUNITIMAGESIZE,
                GUIUNITIMAGESIZE - 0.5f * panelWidth,
                2 * GUIUNITIMAGESIZE - 0.5f * panelHeight,
                Color.white,
                questionCanvas);

            UnityEngine.UI.Button allyButton;
            allyButton = GUIRoutines.CreateButton("AllyButton", "Ally",
                0.5f * GUIUNITIMAGESIZE - 0.5f * panelWidth,
                0.5f * GUIUNITIMAGESIZE - 0.5f * panelHeight,
                questionCanvas);
            allyButton.gameObject.AddComponent<ChooseSideButtonRoutines>();
            allyButton.onClick.AddListener(allyButton.GetComponent<ChooseSideButtonRoutines>().AllyButtonSelected);

            UnityEngine.UI.Button germanButton;
            germanButton = GUIRoutines.CreateButton("GermanButon", "German",
                1.5f * GUIUNITIMAGESIZE - 0.5f * panelWidth,
                0.5f * GUIUNITIMAGESIZE - 0.5f * panelHeight,
                questionCanvas);
            germanButton.gameObject.AddComponent<ChooseSideButtonRoutines>();
            germanButton.onClick.AddListener(germanButton.GetComponent<ChooseSideButtonRoutines>().GermanButtonSelected);
        }

        /// <summary>
        /// This routine displays the units on the passed hex in the static gui display
        /// </summary>
        /// <param name="hex"></param>
        public static void GuiDisplayUnitsOnHex(GameObject hex)
        {
            // First need to wipe out any units currently displayed
            GuiClearUnitsOnHex();

            GameObject.Find("UnitDisplayPanel").GetComponent<CanvasGroup>().alpha = 1;
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
                GuiDisplayUnit(hex, "guiHexDisplayFirstUnit", "UnitDisplayImage1", 0, 90);
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 1)
                GuiDisplayUnit(hex, "guiHexDisplaySecondUnit", "UnitDisplayImage2", 1, 150);
            if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 2)
                GuiDisplayUnit(hex, "guiHexDisplayThirdUnit", "UnitDisplayImage3", 2, 210);
        }

        /// <summary>
        /// Updates the static gui display that shows how many weeks the allied have met victory conditions
        /// </summary>
        public static void GuiDisplayAlliedVictoryStatus()
        {
            GameObject.Find("AlliedVictoryText").GetComponent<TextMeshProUGUI>().text = turnsAlliedMetVictoryCondition + " Allied Victory Weeks";
        }

        /// <summary>
        /// Updates the static gui with the number of Allied units on victory hexes
        /// </summary>
        public static void GuiDisplayAlliedVictoryUnits()
        {
            GameObject.Find("AlliedUnitVictoryText").GetComponent<TextMeshProUGUI>().text = ReturnNumberAlliedVictoryUnits() + " Units on Victory Hexes";
        }

        /// <summary>
        /// Updates the static gui with the current loss ratio
        /// </summary>
        public static void GuiUpdateLossRatioText()
        {
            if ((alliedFactorsEliminated == 0) || (germanFactorsEliminated == 0))
                GameObject.Find("LossRatioText").GetComponent<TextMeshProUGUI>().text = "0 Allied/German Loss";
            else
                GameObject.Find("LossRatioText").GetComponent<TextMeshProUGUI>().text = ((float)(alliedFactorsEliminated) / ((float)germanFactorsEliminated)).ToString("0.00") + " Allied/German Loss";
        }

        /// <summary>
        /// Allied victory is achieved when 10 divisions are in supply in Germany for 4 consecutive turns or no German units on the board
        /// </summary>
        public static bool CheckForAlliedVictory()
        {
            //  Only display the victory screen for the first turn victory has been met
            if (alliedVictory)
                return false;

            // If this is a game against the computer and the computer is playing the Germans, check if it resigns
            if ((gameMode == GameModeValues.AI) && (nationalityUserIsPlaying == Nationality.Allied))
            {
                // The computer will resign if the Allies have three times more attack factors on the board than the Germans have defense factors
                int alliedAttackFactors = 0;
                int germanDefenseFactors = 0;

                foreach (GameObject unit in alliedUnitsOnBoard)
                    alliedAttackFactors += unit.GetComponent<UnitDatabaseFields>().attackFactor;
                foreach (GameObject unit in germanUnitsOnBoard)
                    germanDefenseFactors += unit.GetComponent<UnitDatabaseFields>().defenseFactor;

                if (alliedAttackFactors / germanDefenseFactors >= 3)
                {
                    GuiUpdateStatusMessage("The computer is resigning due to the the overwhelming Allied force");
                    alliedVictory = true;
                    DisplayAlliedVictoryScreen();
                    return true;
                }

            }

            // Units that count for victory have to be non-HQ and in supply
            int count = 0;
            foreach (GameObject unit in alliedUnitsOnBoard)
                if ((unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().AlliedVictoryHex) &&
                        unit.GetComponent<UnitDatabaseFields>().inSupply && !unit.GetComponent<UnitDatabaseFields>().HQ)
                    count++;

            WriteToLogFile(count + " - number of allied units meeting victory conditions");
            if (count > 9)
                turnsAlliedMetVictoryCondition++;
            else
                turnsAlliedMetVictoryCondition = 0;

            if (turnsAlliedMetVictoryCondition == 4)
            {
                // Update the number of weeks otherwise it will be showing 3 which will be confusing
                GuiDisplayAlliedVictoryStatus();
                alliedVictory = true;
                DisplayAlliedVictoryScreen();
                return true;
            }

            if (germanUnitsOnBoard.Count == 0)
            {
                alliedVictory = true;
                DisplayAlliedVictoryScreen();
                return true;
            }

            return false;
        }

        /// <summary>
        /// German victory is decided by there being no Allied units on the board anytime after the second invasion has taken place or after turn 16.  
        /// The German also wins if the Allied player hasn't achieved victory by the 50th turn.
        /// </summary>
        public static bool CheckForGermanVictory()
        {
            //  Only display the victory screen for the first turn victory has been met
            if (germanVictory)
                return false;

            // If this is a computer game and the computer is playing the Allies, check if the computer resigns
            if ((gameMode == GameModeValues.AI) && (nationalityUserIsPlaying == Nationality.German))
            {
                // The computer resigns if there have been four turns without an Allied victory and new hexes aren't being controled
                // Note that numberOfHexesInAlliedControl contains the number of hexes that were in Allied control at the start of the turn
                if ((numberOfTurnsWithoutSuccessfulAttack >= 4) && (turnNumber > 8) && (numberOfHexesInAlliedControl >= ReturnNumberOfHexesInAlliedControl()))
                {
                    germanVictory = true;
                    DisplayGermanVictoryScreen();
                    return true;
                }
            }

            if ((turnNumber > 8) && (alliedUnitsOnBoard.Count == 0))
            {
                germanVictory = true;
                DisplayGermanVictoryScreen();
                return true;
            }
            else if ((turnNumber >= 50) && !alliedVictory)
            {
                germanVictory = true;
                DisplayGermanVictoryScreen();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns the strength of victory.  Note this returns a score for whichever side has the victory.
        /// </summary>
        public static int CalculateStrengthOfVictory()
        {
            int strengthOfVictory = 1;
            float lossRatio;

            // There are three factors that impact strength of victory: difficulty, loss ratio, and victor turn

            if (easiestDifficultySettingUsed > 9)
                strengthOfVictory += 3;
            else if (easiestDifficultySettingUsed > 7)
                strengthOfVictory += 2;
            else if (easiestDifficultySettingUsed > 5)
                strengthOfVictory++;
            else if (easiestDifficultySettingUsed == 5) { } // Don't adjust - this is the norm
            else if (easiestDifficultySettingUsed > 2)
                strengthOfVictory--;
            else if (easiestDifficultySettingUsed > 0)
                strengthOfVictory -= 2;
            else if (easiestDifficultySettingUsed == 0)
                strengthOfVictory -= 3;

            WriteToLogFile("calculateStrengthOfVictory: strengthOfVictory = " + strengthOfVictory + " easiestDifficultySettingUsed = " + easiestDifficultySettingUsed);

            WriteToLogFile("calculateStrengthOfVictory: alliedFactorsEliminated = " + alliedFactorsEliminated + " germanFactorsEliminated = " + germanFactorsEliminated);
            if (germanVictory)
                lossRatio = ((float)alliedFactorsEliminated) / ((float)germanFactorsEliminated);
            else
                lossRatio = ((float)germanFactorsEliminated) / ((float)alliedFactorsEliminated);

            if (lossRatio >= 2)
                strengthOfVictory += 2;
            else if (lossRatio >= 1.5)
                strengthOfVictory++;
            else if (lossRatio >= 1)
            {
                // Don't make any adjustment for a loss ratio of 1
            }
            else if (lossRatio >= 0.75)
                strengthOfVictory--;
            else if (lossRatio > 0)
                strengthOfVictory -= 2;
            else if (lossRatio == 0)
                strengthOfVictory += 3; // In the unlikely event that no losses were suffered...

            WriteToLogFile("calculateStrengthOfVictory: strengthOfVictory = " + strengthOfVictory + " lossRatio = " + lossRatio);

            if (germanVictory)
            {
                if (turnNumber <= 16)
                    strengthOfVictory += 2;
                else if (turnNumber < 50)
                    strengthOfVictory++;
            }
            else
            {
                if (turnNumber <= 31)
                    strengthOfVictory += 2;
                else if (turnNumber <= 41)
                    strengthOfVictory++;
            }
            WriteToLogFile("calculateStrengthOfVictory: strengthOfVictory = " + strengthOfVictory + " turnNumber = " + turnNumber);

            return (strengthOfVictory);
        }

        /// <summary>
        /// Sets the message that will be displayed on the victory screen for an Allied victory
        /// </summary>
        public static void DisplayAlliedVictoryScreen()
        {
            GuiUpdateStatusMessage("Allied victory conditions have been met");

            int strengthOfVictory = CalculateStrengthOfVictory();
            string message = "";

            WriteToLogFile("displayGermanVictoryScreen: strengthOfVicory = " + strengthOfVictory + " player is " + nationalityUserIsPlaying);

            if (gameMode == GameModeValues.Hotseat)
            {
                if (strengthOfVictory < 1)
                    message = "While the Allied player has met the victory conditions the results are considered a draw    strength of victory = " + strengthOfVictory;
                else if (strengthOfVictory < 3)
                    message = "The Allied player has attained a minor victory    strength of victory = " + strengthOfVictory;
                else if (strengthOfVictory < 5)
                    message = "The Allied player has attained a victory    strength of victory = " + strengthOfVictory;
                else
                    message = "The Allied player has attained a decisive victory    strength of victory = " + strengthOfVictory;
            }

            else
            {
                if (nationalityUserIsPlaying == Nationality.Allied)
                {
                    if (strengthOfVictory < 1)
                        message = "While you have met the victory conditions the results are considered a draw    strength of victory = " + strengthOfVictory;
                    else if (strengthOfVictory < 3)
                        message = "Congratulations, you have attained a minor victory    strength of victory = " + strengthOfVictory;
                    else if (strengthOfVictory < 5)
                        message = "Congratulations you have attained a victory    strength of victory = " + strengthOfVictory;
                    else
                        message = "Congratulations you have attained a decisive victory    strength of victory = " + strengthOfVictory;
                }
                else
                {
                    if (strengthOfVictory < 1)
                        message = "While your opponent has met the victory conditions the results are considered a draw    strength of victory = " + strengthOfVictory;
                    else if (strengthOfVictory < 3)
                        message = "You have suffered a minor defeat    strength of victory = " + strengthOfVictory;
                    else if (strengthOfVictory < 5)
                        message = "You have suffered a defeat    strength of victory = " + strengthOfVictory;
                    else
                        message = "You have suffered a decisive defeat    strength of victory = " + strengthOfVictory;
                }
            }

            VictoryScreen(message);
        }

        /// <summary>
        /// Sets the message that will be displayed on the victory screen for a German victory
        /// </summary>
        public static void DisplayGermanVictoryScreen()
        {
            GuiUpdateStatusMessage("German victory conditions have been met");

            int strengthOfVictory = CalculateStrengthOfVictory();
            string message = "";

            WriteToLogFile("displayGermanVictoryScreen: strengthOfVicory = " + strengthOfVictory + " player is " + nationalityUserIsPlaying);

            if (gameMode == GameModeValues.Hotseat)
            {
                if (strengthOfVictory < 1)
                    message = "While the German player has met the victory conditions the results are considered a draw    strength of victory = " + strengthOfVictory;
                else if (strengthOfVictory < 3)
                    message = "The German player has attained a minor victory    strength of victory = " + strengthOfVictory;
                else if (strengthOfVictory < 5)
                    message = "The German player has attained a victory    strength of victory = " + strengthOfVictory;
                else
                    message = "The German player has attained a decisive victory    strength of victory = " + strengthOfVictory;
            }

            else
            {
                if (nationalityUserIsPlaying == Nationality.German)
                {
                    if (strengthOfVictory < 1)
                        message = "While you have met the victory conditions the results are considered a draw    strength of victory = " + strengthOfVictory;
                    else if (strengthOfVictory < 3)
                        message = "Congratulations, you have attained a minor victory    strength of victory = " + strengthOfVictory;
                    else if (strengthOfVictory < 5)
                        message = "Congratulations you have attained a victory    strength of victory = " + strengthOfVictory;
                    else
                        message = "Congratulations you have attained a decisive victory    strength of victory = " + strengthOfVictory;
                }
                else
                {
                    if (strengthOfVictory < 1)
                        message = "While your opponent has met the victory conditions the results are considered a draw    strength of victory = " + strengthOfVictory;
                    else if (strengthOfVictory < 3)
                        message = "You have suffered a minor defeat    strength of victory = " + strengthOfVictory;
                    else if (strengthOfVictory < 5)
                        message = "You have suffered a defeat    strength of victory = " + strengthOfVictory;
                    else
                        message = "You have suffered a decisive defeat    strength of victory = " + strengthOfVictory;
                }
            }
            VictoryScreen(message);
        }

        /// <summary>
        /// Creates the victory gui
        /// </summary>
        /// <param name="message"></param>
        public static void VictoryScreen(string message)
        {
            UnityEngine.UI.Button okButton;
            WriteToLogFile("victoryScreen: executing with message = " + message);
            GUIRoutines.RemoveAllGUIs();
            Canvas victoryCanvas = null;
            GUIRoutines.CreateGUICanvas("AlliedVictoryMessage", 1000, 200, ref victoryCanvas);
            GUIRoutines.CreateUIText("..." + message + " ...", "VictoryMessageText", 1000, 200, 0, 0, Color.white, victoryCanvas);
            okButton = GUIRoutines.CreateButton("VictoryOKButton", "OK",
                    0,
                    -30,
                    victoryCanvas);
            okButton.gameObject.AddComponent<GUIButtonRoutines>();
            okButton.onClick.AddListener(okButton.GetComponent<GUIButtonRoutines>().VictoryOK);
        }

        /// <summary>
        /// Returns number of Allied units on victory hexes
        /// </summary>
        /// <returns></returns>
        private static int ReturnNumberAlliedVictoryUnits()
        {
            int count = 0;
            foreach (GameObject unit in alliedUnitsOnBoard)
                if ((unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().AlliedVictoryHex) &&
                        !unit.GetComponent<UnitDatabaseFields>().HQ)
                    count++;
            return (count);
        }

        /// <summary>
        /// Displays a single unit in the correct position on the gui panel
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="gameObjectName"></param>
        /// <param name="findGameObjectName"></param>
        /// <param name="unitNumber"></param>
        /// <param name="panelHeight"></param>
        private static void GuiDisplayUnit(GameObject hex, string gameObjectName, string findGameObjectName, int unitNumber, int panelHeight)
        {
            GameObject tempPrefab;
            Image tempImage;

            tempPrefab = Instantiate(Resources.Load("GUI Image") as GameObject, new Vector3(0, -20, 0), Quaternion.identity);
            tempPrefab.name = gameObjectName;
            tempImage = tempPrefab.GetComponent<Image>();
            tempImage.transform.position = tempPrefab.transform.position; // Added after upgrade to 2017.f3
            tempImage.transform.SetParent(GameObject.Find(findGameObjectName).transform, false);
            tempImage.sprite = hex.GetComponent<HexDatabaseFields>().occupyingUnit[unitNumber].GetComponent<SpriteRenderer>().sprite;
            tempImage.rectTransform.sizeDelta = new Vector2(GUIUNITIMAGESIZE, GUIUNITIMAGESIZE);
            GameObject.Find("UnitDisplayPanel").GetComponent<RectTransform>().sizeDelta = new Vector2(100, panelHeight);
        }

        /// <summary>
        /// This routine clears the units displayed in the gui
        /// </summary>
        public static void GuiClearUnitsOnHex()
        {
            // First need to wipe out any units currently displayed
            DestroyImmediate(GameObject.Find("guiHexDisplayFirstUnit"));
            DestroyImmediate(GameObject.Find("guiHexDisplaySecondUnit"));
            DestroyImmediate(GameObject.Find("guiHexDisplayThirdUnit"));
            GameObject.Find("UnitDisplayPanel").GetComponent<CanvasGroup>().alpha = 0;
        }

        /// <summary>
        /// This routine shows the file dialog for loading a saved game
        /// </summary>
        /// <returns></returns>
        public static string GuiFileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = GameControl.path + "\\TGCOutputFiles"
            };
            DialogResult dResult = dialog.ShowDialog();
            if (dResult == DialogResult.OK)
            {
                return (dialog.FileName);
            }
            return null;
        }

        // The following strings will be used to store the last five messages.  I used to just keep appending the status text but it 
        // gets to be too big for the Unity editor.
        private static string oldMessage1, oldMessage2, oldMessage3, oldMessage4, oldMessage5;
        /// <summary>
        /// Updated the message displayed on the status screen
        /// </summary>
        /// <param name="message"></param>
        public static void GuiUpdateStatusMessage(string message)
        {
            oldMessage1 = oldMessage2;
            oldMessage2 = oldMessage3;
            oldMessage3 = oldMessage4;
            oldMessage4 = oldMessage5;
            oldMessage5 = message;
            // During the AI turn do not send status messages
            if (!((gameMode == GameModeValues.AI) && !localControl) && !commandFileBeingRead)
            {
                WriteToLogFile("guiUpdateStatusMessage: " + message);
                GameObject.Find("StatusMessageText").GetComponent<TextMeshProUGUI>().text = oldMessage5 + "\n\n" + oldMessage4 + "\n\n" + oldMessage3 + "\n\n" + oldMessage2 + "\n\n" + oldMessage1;
            }
        }

        /// <summary>
        /// Updates the turn number displayed on the status screen
        /// </summary>
        public static void GuiUpdateTurn()
        {
            DateTime dday = new DateTime(1944, 6, 6);
            DateTime weekStart = dday.AddDays((turnNumber - 1) * 7);
            DateTime weekEnd = weekStart.AddDays(6);
            GameObject.Find("GUITurnTextObject").GetComponent<TextMeshProUGUI>().text = " Turn Number " + turnNumber;
            GameObject.Find("GUIDateTextObject").GetComponent<TextMeshProUGUI>().text = weekStart.ToString("MM/dd/yyyy") + " - " + weekEnd.ToString("MM/dd/yyyy");
        }

        /// <summary>
        /// Updates the phase displayed on the static gui
        /// </summary>
        /// <param name="currentPhase"></param>
        public static void GuiUpdatePhase(string currentPhase)
        {
            GameObject.Find("GUIPhaseTextObject").GetComponent<TextMeshProUGUI>().text = currentPhase;
            WriteToLogFile("Changing to " + currentPhase + " Phase");
        }

        /// <summary>
        /// This routine writes out all of the values of the variables stored in Global Definitions that are needed for saving
        /// </summary>
        /// <param name="fileWriter"></param>
        public static void WriteGlobalVariables(StreamWriter fileWriter)
        {
            fileWriter.Write(numberOfCarpetBombingsUsed + " ");
            fileWriter.Write(numberInvasionsExecuted + " ");
            fileWriter.Write(firstInvasionAreaIndex + " ");
            fileWriter.Write(secondInvasionAreaIndex + " ");
            fileWriter.Write(germanReplacementsRemaining + " ");
            fileWriter.Write(alliedReplacementsRemaining + " ");
            fileWriter.Write(alliedCapturedBrest + " ");
            fileWriter.Write(alliedCapturedBoulogne + " ");
            fileWriter.Write(alliedCapturedRotterdam + " ");
            if (firstInvasionAreaIndex != -1)
                fileWriter.Write(invasionAreas[firstInvasionAreaIndex].turn + " ");
            else
                fileWriter.Write("0 ");
            if (secondInvasionAreaIndex != -1)
                fileWriter.Write(invasionAreas[secondInvasionAreaIndex].turn + " ");
            else
                fileWriter.Write("0 ");
            fileWriter.Write(numberOfTurnsWithoutSuccessfulAttack + " ");
            fileWriter.Write(hexesAttackedLastTurn.Count + " ");
            for (int index = 0; index < hexesAttackedLastTurn.Count; index++)
                fileWriter.Write(hexesAttackedLastTurn[index].name + " ");
            fileWriter.Write(combatResultsFromLastTurn.Count + " ");
            for (int index = 0; index < combatResultsFromLastTurn.Count; index++)
                fileWriter.Write(combatResultsFromLastTurn[index] + " ");
            fileWriter.Write(turnsAlliedMetVictoryCondition + " ");
            fileWriter.Write(alliedFactorsEliminated + " ");
            fileWriter.Write(germanFactorsEliminated + " ");
            fileWriter.Write(easiestDifficultySettingUsed + " ");
            fileWriter.WriteLine();
        }

        /// <summary>
        /// Writes message to the log file
        /// </summary>
        /// <param name="logEntry"></param>
        public static void WriteToLogFile(string logEntry)
        {
            using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalGameFields.logfile))
                writeFile.WriteLine(logEntry);
        }

        /// <summary>
        /// Writes a command to the command file. Also sends socket message if network game
        /// </summary>
        /// <param name="commandString"></param>
        public static void WriteToCommandFile(string commandString)
        {
            if (!commandFileBeingRead)
            {
                using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalGameFields.commandFile))
                    writeFile.WriteLine(commandString);

                using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalGameFields.fullCommandFile))
                    writeFile.WriteLine(commandString);

                //if (localControl && (gameMode == GameModeValues.Peer2PeerNetwork))
                //{
                //    TransportScript.SendMessageToRemoteComputer(commandString);
                //}
            }
        }

        /// <summary>
        /// Deletes the current command file if it exists and writes the header line to a new version
        /// </summary>
        public static void DeleteCommandFile()
        {
            if (File.Exists(GameControl.path + GlobalGameFields.commandFile))
                File.Delete(GameControl.path + GlobalGameFields.commandFile);
            using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalGameFields.commandFile))
                writeFile.WriteLine(commandFileHeader);
        }

        public static void DeleteFullCommandFile()
		{
            // Now do the same thing for the full command file
            if (File.Exists(GameControl.path + GlobalGameFields.fullCommandFile))
                File.Delete(GameControl.path + GlobalGameFields.fullCommandFile);
            using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalGameFields.fullCommandFile))
                writeFile.WriteLine(commandFileHeader);
        }

        /// <summary>
        /// This routine returns the local public ip address
        /// </summary>
        /// <returns></returns>
        public static string GetLocalPublicIPAddress()
        {
            string url = "http://checkip.dyndns.org";
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            return a4;
        }

        /// <summary>
        /// This routine sends a chat message
        /// </summary>
        public static void ExecuteChatMessage()
        {
            //string messageText = GameObject.Find("ChatInputField").GetComponent<InputField>().text;
            //GameObject.Find("ChatText").GetComponent<Text>().text = messageText + Environment.NewLine + GameObject.Find("ChatText").GetComponent<Text>().text;
            //TransportScript.SendMessageToRemoteComputer(CHATMESSAGEKEYWORD + " " + messageText);
            //GameObject.Find("ChatInputField").GetComponent<InputField>().text = "";
        }

        /// <summary>
        /// This routine executes when receiving a chat message
        /// </summary>
        public static void AddChatMessage(string messageText)
        {
            WriteToLogFile("addChatMessage: received message " + messageText);
            GameObject.Find("ChatText").GetComponent<TextMeshProUGUI>().text = messageText + Environment.NewLine + GameObject.Find("ChatText").GetComponent<TextMeshProUGUI>().text;
        }

        /// <summary>
        /// Used to switch the local control variable and adjusting access to the gui buttons
        /// </summary>
        /// <param name="localControlValue"></param>
        public static void SwitchLocalControl(bool localControlValue)
        {
            localControl = localControlValue;

            if (localControlValue)
            {
                nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                //undoButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                //MustAttackToggle.GetComponent<Toggle>().interactable = true;
                //AssignCombatButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                //DisplayAllCombatsButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                //AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
                //GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
                //AlliedSupplySourcesButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                SettingsButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                HideUnitsToggle.GetComponent<Toggle>().interactable = true;
                HistoricalProgressToggle.GetComponent<Toggle>().interactable = true;
            }
            else
                SetGUIForNonLocalControl();
        }

        public static void SetGUIForNonLocalControl()
        {
            nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            MustAttackToggle.GetComponent<Toggle>().interactable = false;
            AssignCombatButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            DisplayAllCombatsButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
            GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
            AlliedSupplySourcesButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            SettingsButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            HideUnitsToggle.GetComponent<Toggle>().interactable = false;
            HistoricalProgressToggle.GetComponent<Toggle>().interactable = false;
        }

        /// <summary>
        /// This routine returns a byte of 1 or 0 for a boolean value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte WriteBooleanToSaveFormat(bool value)
        {
            if (value == true)
                return (1);
            else
                return (0);
        }

        public static bool ReturnBoolFromSaveFormat(string value)
        {
            if (value == "1")
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Returns all units of the passed nationality that are on the board
        /// </summary>
        /// <param name="nationality"></param>
        /// <returns></returns>
        public static List<GameObject> ReturnNationUnitsOnBoard(Nationality nationality)
        {
            //writeToLogFile("returnNationUnitsOnBoard: executing ... nationality = " + nationality);
            List<GameObject> returnList = new List<GameObject>();
            foreach (Transform unit in allUnitsOnBoard.transform)
                if (unit.GetComponent<UnitDatabaseFields>().nationality == nationality)
                    returnList.Add(unit.gameObject);
            return (returnList);
        }

        /// <summary>
        /// Returns the list passed sorted by highest attack factor to smallest
        /// </summary>
        public static void SortUnitListByAttackFactor(List<GameObject> unitList)
        {
            GameObject tempUnit;
            for (int index = 0; index < (unitList.Count - 1); index++)
            {
                for (int index2 = (index + 1); index2 < (unitList.Count - 1); index2++)
                    if (unitList[index].GetComponent<UnitDatabaseFields>().attackFactor < unitList[index2].GetComponent<UnitDatabaseFields>().attackFactor)
                    {
                        tempUnit = unitList[index];
                        unitList[index] = unitList[index2];
                        unitList[index2] = tempUnit;
                    }
            }
        }

        /// <summary>
        /// Returns the number of hexes in Allied control
        /// </summary>
        /// <returns></returns>
        public static int ReturnNumberOfHexesInAlliedControl()
        {
            int returnNumber = 0;
            foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                if (hex.GetComponent<HexDatabaseFields>().alliedControl)
                {
                    //GlobalDefinitions.writeToLogFile("returnNumberOfHexesInAlliedControl:       "+ hex.name);

                    returnNumber++;
                }
            return (returnNumber);
        }

        public static void GuiDisplayAIStatus(string message)
        {
            Canvas aiStatusCanvas = null;
            GUIRoutines.CreateGUICanvas("AIStatusMessage", 1000, 200, ref aiStatusCanvas);
            GUIRoutines.CreateUIText("..." + message + " ...", "AIStatusMessageText", 1000, 200, 0, 0, Color.white, aiStatusCanvas);
        }


        /// <summary>
        /// Returns the passed unit to the OOB sheet
        /// </summary>
        /// <param name="unit"></param>
        public static void ReturnUnitToOOBShet(GameObject unit)
        {
            unit.transform.position = unit.GetComponent<UnitDatabaseFields>().OOBLocation;
            unit.GetComponent<UnitDatabaseFields>().inSupply = true;
            unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor;
            UnhighlightUnit(unit.gameObject);

            unit.transform.parent = GameObject.Find("Units Eliminated").transform;
            unit.GetComponent<UnitDatabaseFields>().occupiedHex = null;
        }


        // THE FOLLOWING TWO ROUTINES ARE LEFT HERE INSTEAD OF BEING PUT IN THE COMMON LIBRARY BECAUSE STACKING IS BASED ON NATIONALITY WHICH IS A GAME SPECIFIC FIELD

        /// <summary>
        /// Returns true if the hex is under the stacking limit, otherwise it returns false
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool HexUnderStackingLimit(GameObject hex, Nationality nationality)
        {
            if (nationality == Nationality.German)
            {
                if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count >= GlobalDefinitions.GermanStackingLimit)
                    return (false);
                else
                    return (true);
            }

            else
            {
                bool hqPresent = false;
                int tempAlliedStackingLimit;

                // For allied stacks need to see if any of the units are HQ units.  If at least one is then the stacking limit is increased by 1
                for (int index = 0; index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                    if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().HQ)
                        hqPresent = true;

                if (hqPresent)
                    tempAlliedStackingLimit = GlobalDefinitions.AlliedStackingLimit + 1;
                else
                    tempAlliedStackingLimit = GlobalDefinitions.AlliedStackingLimit;

                if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count >= tempAlliedStackingLimit)
                    return (false);
                else
                    return (true);
            }
        }

        /// <summary>
        /// This routine will check if the passed hex is overstacked
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool StackingLimitExceeded(GameObject hex, GlobalDefinitions.Nationality nationality)
        {
            bool hqPresent;
            if ((nationality == Nationality.German) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > GlobalDefinitions.GermanStackingLimit))
                return (true);

            if ((nationality == Nationality.Allied) && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > GlobalDefinitions.AlliedStackingLimit))
            {
                // For allied stacks need to see if any of the units are HQ units.  If at least one is then the stacking limit is increased by 1
                hqPresent = false;
                for (int index = 0; index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                {
                    if (hex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().HQ)
                        hqPresent = true;
                }
                if (hqPresent)
                {
                    if (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > (GlobalDefinitions.AlliedStackingLimit + 1))
                        return (true);
                }
                else
                    return (true);
            }

            // If the hex is overstacked we would have returned true by now so that means it isn't overstacked therefore return false
            return (false);
        }

        /// <summary>
        /// Eliminate units on a sea hex - used at the end of a turn
        /// </summary>
        public static void EliminateUnitsOnSeaHexes()
		{
            foreach (Transform unit in GlobalDefinitions.allUnitsOnBoard.transform)
			{
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea)
				{
                    MoveUnitToDeadPile(unit.gameObject);
				}
			}

        }


        // These are the key words used in the messaging between the two networked computers
        public const string PLAYSIDEKEYWORD = "SetPlaySide";
        public const string PASSCONTROLKEYWORK = "PassControl";
        public const string SETGAMESTATEKEYWORD = "SetState";
        public const string READTURNKEYWORD = "Turn";
        public const string READSAVEDUNITKEYWORK = "UnitRecord";
        public const string READSAVEDHEXKEYWORD = "HexRecord";
        public const string READSAVEDGAMECONTROLPARAMETERSKEYWORD = "GameControlParameters";
        public const string READSAVEDGLOBALDEFINITIONSPARAMETERSKEYWORD = "GlobalDefinitionsParameters";
        public const string MOUSESELECTIONKEYWORD = "MouseSelection";
        public const string MOUSEDOUBLECLICKIONKEYWORD = "MouseDoubleClick";
        public const string MULTIUNITSELECTIONKEYWORD = "MultiUnitSelection";
        public const string MULTIUNITSELECTIONCANCELKEYWORD = "MultiUnitSelectionCancel";
        public const string SETCAMERAPOSITIONKEYWORD = "SetCameraPosition";
        public const string DISPLAYCOMBATRESOLUTIONKEYWORD = "DisplayCombatResolution";
        public const string NEXTPHASEKEYWORD = "NextPhase";
        public const string EXECUTETACTICALAIROKKEYWORD = "ExecteTacticalAirOK";
        public const string ADDCLOSEDEFENSEKEYWORD = "AddCloseDefense";
        public const string CANCELCLOSEDEFENSEKEYWORD = "CancelCloseDefense";
        public const string LOCATECLOSEDEFENSEKEYWORD = "LocateCloseDefense";
        public const string ADDRIVERINTERDICTIONKEYWORD = "AddRiverInterdiction";
        public const string CANCELRIVERINTERDICTIONKEYWORD = "CancelRiverInterdiction";
        public const string LOCATERIVERINTERDICTIONKEYWORD = "LocateRiverInterdiction";
        public const string ADDUNITINTERDICTIONKEYWORD = "AddUnitInterdiction";
        public const string CANCELUNITINTERDICTIONKEYWORD = "CancelUnitInterdiction";
        public const string LOCATEUNITINTERDICTIONKEYWORD = "LocateUnitInterdiction";
        public const string TACAIRMULTIUNITSELECTIONKEYWORD = "TacticalAirMultiUnitSelection";
        public const string SETCOMBATTOGGLEKEYWORD = "SetCombatToggle";
        public const string RESETCOMBATTOGGLEKEYWORD = "ResetCombatToggle";
        public const string COMBATGUIOKKEYWORD = "CombatGUIOK";
        public const string COMBATGUICANCELKEYWORD = "CombatGUICancel";
        public const string ADDCOMBATAIRSUPPORTKEYWORD = "AddCombatAirSupport";
        public const string REMOVECOMBATAIRSUPPORTKEYWORD = "RemoveCombatAirSupport";
        public const string COMBATRESOLUTIONSELECTEDKEYWORD = "CombatResolutionSelected";
        public const string COMBATLOCATIONSELECTEDKEYWORD = "CombatLocationSelected";
        public const string COMBATCANCELKEYWORD = "CombatCancel";
        public const string COMBATOKKEYWORD = "CombatOK";
        public const string CARPETBOMBINGRESULTSSELECTEDKEYWORD = "CarpetBombingResultsSelected";
        public const string RETREATSELECTIONKEYWORD = "RetreatSelection";
        public const string SELECTPOSTCOMBATMOVEMENTKEYWORD = "SelectPostCombatMovement";
        public const string DESELECTPOSTCOMBATMOVEMENTKEYWORD = "DeselectPostCombatMovement";
        public const string ADDEXCHANGEKEYWORD = "AddExchange";
        public const string OKEXCHANGEKEYWORD = "OKExchange";
        public const string REMOVEEXCHANGEKEYWORD = "RemoveExchange";
        public const string POSTCOMBATOKKEYWORD = "PostCombatOK";
        public const string DISPLAYALLIEDSUPPLYKEYWORD = "DisplayAlliedSupply";
        public const string SETSUPPLYKEYWORD = "SetSupply";
        public const string RESETSUPPLYKEYWORD = "ResetSupply";
        public const string LOCATESUPPLYKEYWORD = "LocateSupply";
        public const string OKSUPPLYKEYWORD = "OkSupply";
        public const string OKSUPPLYWITHENDPHASEKEYWORD = "OkSupplyWithEndPhase";
        public const string CHANGESUPPLYSTATUSKEYWORD = "ChangeSupplyStatus";
        public const string SENDSAVEFILELINEKEYWORD = "SendSaveFileLine";
        public const string SAVEFILENAMEKEYWORD = "SaveFileName";
        public const string SAVEFILETRANSMITCOMPLETEKEYWORD = "SaveFileTransmitComplete";
        public const string GAMEDATALOADEDKEYWORD = "GameDataLoaded";
        public const string PLAYNEWGAMEKEYWORD = "PlayNewGame";
        public const string INVASIONAREASELECTIONKEYWORD = "InvasionAreaSelection";
        public const string CARPETBOMBINGSELECTIONKEYWORD = "CarpetBombingSelection";
        public const string CARPETBOMBINGLOCATIONKEYWORD = "CarpetBombingLocation";
        public const string CARPETBOMBINGOKKEYWORD = "CarpetBombingOK";
        public const string DIEROLLRESULT1KEYWORD = "DieRollResult1";
        public const string DIEROLLRESULT2KEYWORD = "DieRollResult2";
        public const string UNDOKEYWORD = "Undo";
        public const string CHATMESSAGEKEYWORD = "ChatMessage";
        public const string SENDTURNFILENAMEWORD = "SendTurnFileName";
        public const string DISPLAYALLIEDSUPPLYRANGETOGGLEWORD = "DisplayAlliedSupplyRangeToggle";
        public const string DISPLAYGERMANSUPPLYRANGETOGGLEWORD = "DisplayGermanSupplyRangeToggle";
        public const string DISPLAYMUSTATTACKTOGGLEWORD = "DisplayMustAttackToggle";
        public const string TOGGLEAIRSUPPORTCOMBATTOGGLE = "ToggleAirSupportCombatToggle";
        public const string TOGGLECARPETBOMBINGCOMBATTOGGLE = "ToggleCarpetBombingCombatToggle";
        public const string LOADCOMBATKEYWORD = "LoadCombat";
        public const string DISCONNECTFROMREMOTECOMPUTER = "DisconnectFromRemoteComputer";
        public const string YESBUTTONSELECTEDKEYWORD = "YesButtonSelected";
        public const string NOBUTTONSELECTEDKEYWORD = "NoButtonSelected";
        public const string ALLIEDREPLACEMENTKEYWORD = "AlliedReplacement";
        public const string GERMANREPLACEMENTKEYWORD = "GermanReplacement";
        public const string AGGRESSIVESETTINGKEYWORD = "AggressiveSetting";
        public const string DIFFICULTYSETTINGKEYWORD = "DifficultySetting";
    }

    public class HexLocation
    {
        public int x;
        public int y;
    }

    public class Combat : MonoBehaviour
    {
        public List<GameObject> defendingUnits = new List<GameObject>();
        public List<GameObject> attackingUnits = new List<GameObject>();
        public bool attackAirSupport;
        public bool defenseAirSupport;
        public bool carpetBombing;
        public UnityEngine.UI.Button locateButton;
        public UnityEngine.UI.Button cancelButton;
        public UnityEngine.UI.Button resolveButton;
        public Toggle airSupportToggle;
    }

    public class SupplyGUIObject : MonoBehaviour
    {
        public GameObject supplySource;
        public Toggle supplyToggle;
        public UnityEngine.UI.Button locateButton;
    }

    /// <summary>
    /// This class is used to define the parameters of each invasion area.
    /// </summary>
    public class InvasionArea
    {
        public string name;
        public bool firstInvasionArea;
        public bool secondInvasionArea;
        public int turn = 0;
        public bool invaded = false;
        public bool failed = false;
        public int firstTurnArmor;
        public int firstTurnInfantry;
        public int firstTurnAirborne;
        public int secondTurnArmor;
        public int secondTurnInfantry;
        public int secondTurnAirborne;
        public int divisionsPerTurn;
        public int totalUnitsUsedThisTurn;
        public int armorUnitsUsedThisTurn;
        public int infantryUnitsUsedThisTurn;
        public int airborneUnitsUsedThisTurn;
        public int infantryUsedAsArmorThisTurn;
        public int airborneUsedAsInfantryThisTurn;

        public List<GameObject> invasionHexes = new List<GameObject>();
    }
}
