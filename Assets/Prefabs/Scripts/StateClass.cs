using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Collections;

public delegate object delMethod(object delMessage);
public delegate void methodDelegateWithParameters(InputMessage inputMessage);
public delegate void Action();

public class InputMessage : MonoBehaviour
{
    public List<GameObject> args = new List<GameObject>();
    public GameObject hex;
    public GameObject unit;
}

/// <summary>
/// This class is used as a singleton created in GameControl to track current state information and create the individual state instances and state transitions
/// </summary>
public class gameStateControl : MonoBehaviour
{
    //public enGameTypes currentGameType;
    public GameState currentState;
    public bool localControl;

    private void Awake()
    {

    }
}

/// <summary>
/// This class is used to process inputs to the game from the user, networked players, the AI, or email files
/// This is the main control for the game since all actions are based off of inputs
/// </summary>
public class InputControl : MonoBehaviour
{
    public InputMessage inputMessage;
    void Start()
    {
    }
}

public class GameState : MonoBehaviour
{
    public GameState nextGameState;
    public bool userControl;
    public GlobalDefinitions.Nationality currentNationality;

    //public virtual void initialize(InputMessage inputMessage)
    public virtual void initialize()
    {
        // Any state that starts need to have the next phase button available
        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
    }

    public methodDelegateWithParameters executeMethod;

    public virtual void executeUndo(InputMessage inputMessage) { }

    public virtual void executeQuit()
    {
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = nextGameState;
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
    }
}

public class SetUpState : GameState
{
    public override void initialize()
    {
        base.initialize();

        // If this is a network game the state will be handeled directly
        if (GlobalDefinitions.gameMode != GlobalDefinitions.GameModeValues.Network)
            executeMethod = executeTypeOfGame;

        GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;
    }

    public void executeTypeOfGame(InputMessage inputMessage)
    {
        GlobalDefinitions.guiUpdatePhase("Setup Mode");

        // We only need to check for the type of game if it's hotseat or AI and not Network
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Hotseat) || (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI))
            GlobalDefinitions.getNewOrSavedGame();
    }

    public void executeSavedGame()
    {
        string turnFileName;

        // Since at this point we know we are starting a new game and not running the command file, remove the command file
        if (!GlobalDefinitions.commandFileBeingRead)
            if (File.Exists(GameControl.path + GlobalDefinitions.commandFile))
                GlobalDefinitions.deleteCommandFile();

        // This calls up the file browser
        turnFileName = GlobalDefinitions.guiFileDialog();

        if (turnFileName == null)
            executeNewGame();
        else
        {

            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().readTurnFile(turnFileName);

            // If this is a network game send the file name to the remote computer so it can be requested through the file transfer routines.  It's silly that 
            // I have to tell it what to ask for but I bought the code and that is how it works
            GlobalDefinitions.writeToLogFile("ExecuteYesResponse: GameMode = " + GlobalDefinitions.gameMode + " localControl" + GlobalDefinitions.localControl);
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.SENDTURNFILENAMEWORD + " " + turnFileName);
        }
    }

    public void executeNewGame()
    {
        int fileNumber;

        // If the fileNumber is less than 100 the number to be used is being passed as part of a network game
        if (GlobalDefinitions.germanSetupFileUsed == 100)
        {
            // Randomly pick a German setup file
            fileNumber = GlobalDefinitions.dieRoll.Next(1, 10);
            GlobalDefinitions.germanSetupFileUsed = fileNumber;
        }
        else
            // The file to use has been set - this is a network game and the other computer is determining the file to use
            fileNumber = GlobalDefinitions.germanSetupFileUsed;

        GlobalDefinitions.guiUpdateStatusMessage("German setup file number = " + fileNumber);
        GameControl.createBoardInstance.GetComponent<CreateBoard>().readGermanPlacement(GameControl.path + "GermanSetup\\TGCGermanSetup" + fileNumber + ".txt");

        // The network communication is a little different for a new game so I can't use the routine to write to the command file
        // since I don't want it sending a message to the remote computer.
        if (!GlobalDefinitions.commandFileBeingRead)
            using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalDefinitions.commandFile))
                writeFile.WriteLine(GlobalDefinitions.PLAYNEWGAMEKEYWORD + " " + fileNumber);

        // If this is a game where the computer is playing the Germans then exit out of setup at this point
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI) && (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.Nationality.Allied))
        {
            executeQuit();
        }
        else
        {
            executeMethod = executeSelectUnit;
            GlobalDefinitions.guiUpdatePhase(currentNationality + " Setup Mode");
            GlobalDefinitions.guiUpdateStatusMessage("German Setup Mode: Place units in preparation for an invasion.\n        Note that static units must go on coastal hexes, ports, or inland ports\n        German reserves must start on starred hexes in Germany");
            GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = true;
        }
    }

    /// <summary>
    /// Routine used to load the command file
    /// </summary>
    public void readCommandFile()
    {
        char[] delimiterChars = { ' ' };
        string line;
        string[] switchEntries;

        GlobalDefinitions.commandFileBeingRead = true;

        StreamReader theReader = new StreamReader(GameControl.path + GlobalDefinitions.commandFile);
        using (theReader)
        {
            // The first thing we need to do is to read the header and determine if the command file was written playing
            // the same game type that is currently in play.  If not then the command file will not be read.
            line = theReader.ReadLine();
            GlobalDefinitions.writeToLogFile("readCommandFile: reading line - " + line);
            
            // At this point the command file header should be constructed for this game.  I need to compare that to the header line in the command file.
            if (line != GlobalDefinitions.commandFileHeader)
            {
                // The game types are not the same.  Inform the user.
                GlobalDefinitions.guiUpdateStatusMessage("The command file game mode doesn't match the current game mode - cannot execute");
                GlobalDefinitions.commandFileBeingRead = false;
                theReader.Close();
                MainMenuRoutines.getGameModeUI();
                return;
            }


            do
            {
                line = theReader.ReadLine();
                GlobalDefinitions.writeToLogFile("readCommandFile: reading line - " + line);
                if (line != null)
                {
                    switchEntries = line.Split(delimiterChars);
                    switch (switchEntries[0])
                    {
                        case "SavedTurnFile":
                            // A path name with a space in it will cause the name to be split.  Anything after the [0] entry needs to be added back together
                            int a = 3;
                            string fileName = switchEntries[2];
                            GlobalDefinitions.writeToLogFile("readCommandFile: switchEntries[1] = " + switchEntries[1] + "  loop length = " + switchEntries.Length);
                            while (a < switchEntries.Length)
                            {
                                GlobalDefinitions.writeToLogFile("readCommandFile: a = " + a + " switchEntries[a] = " + switchEntries[a]);
                                fileName = fileName + " " + switchEntries[a];
                                a++;
                            }
                            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().readTurnFile(fileName);
                            break;
                        case GlobalDefinitions.PLAYNEWGAMEKEYWORD:
                            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
                            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();

                            // Set the global parameter on what file to use, can't pass it to the executeNoResponse since it is passed as a method delegate elsewhere
                            GlobalDefinitions.germanSetupFileUsed = Convert.ToInt32(switchEntries[1]);

                            GameControl.setUpStateInstance.GetComponent<SetUpState>().executeNewGame();
                            break;
                        default:
                            ExecuteGameCommand.processCommand(line);
                            break;
                    }
                }
            }
            while (line != null);
            theReader.Close();
        }
        GlobalDefinitions.commandFileBeingRead = false;
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        GlobalDefinitions.selectedUnit =
                GameControl.setupRoutinesInstance.GetComponent<SetupRoutines>().getUnitToSetup(currentNationality, inputMessage.unit);

        if (GlobalDefinitions.selectedUnit == null)
            executeMethod = executeSelectUnit;
        else
            executeMethod = executeSelectUnitDestination;
    }

    public void executeSelectUnitDestination(InputMessage inputMessage)
    {
        GameControl.setupRoutinesInstance.GetComponent<SetupRoutines>().getUnitSetupDestination(GlobalDefinitions.selectedUnit, inputMessage.hex);
        executeMethod = executeSelectUnit;
    }

    public override void executeQuit()
    {
        // Generally the easiest difficulty setting used is updated in the combat state sine it is where it is used.  I'm setting it here
        // since a game is started with the setting from the last game and if it isn't reset it will represent the lowest setting ever used, not just in the current game
        GlobalDefinitions.easiestDifficultySettingUsed = GlobalDefinitions.difficultySetting;

        // Just in case a Quit was issued in the middle of a move, unhighlight the selectedUnit
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<Renderer>() != null))
        {
            GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
            GlobalDefinitions.selectedUnit = null;
        }

        if (SetupRoutines.updateHexFields())
        {
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = nextGameState;
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
        }
    }
}

public class TurnInitializationState : GameState
{
    // There are no modes in this state, all actions get executed by the initialization including the state transition
    public override void initialize()
    {
        // If this is a network game the control needs to be swapped here
        if (GlobalDefinitions.localControl && GlobalDefinitions.gameStarted && (GlobalDefinitions.sideControled == GlobalDefinitions.Nationality.German) && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
        {
            GlobalDefinitions.writeToLogFile("TurnInitializationState: passing control to remote computer");
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.PASSCONTROLKEYWORK);
            GlobalDefinitions.localControl = false;
        }

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GlobalDefinitions.guiUpdatePhase("Turn initialization");
        GlobalDefinitions.writeToLogFile("TurnInitializationState: Initialization");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();

        // Write out an end of turn save file 
        if (GlobalDefinitions.turnNumber == 0)
            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().writeSaveTurnFile("Setup");
        else
            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().writeSaveTurnFile("EndOfGerman");

        // Increment the turn number
        GlobalDefinitions.turnNumber++;
        GlobalDefinitions.guiUpdateTurn();

        // Update Allied victory weeks display
        GlobalDefinitions.guiDisplayAlliedVictoryStatus();
        GlobalDefinitions.guiDisplayAlliedVictoryUnits();

        // One of the Free French units is the French 5th armor.  If it isn't available then check to see if the Free French units are available
        if ((GlobalDefinitions.turnNumber > 27) && (GameObject.Find("Armor-FR-5").GetComponent<UnitDatabaseFields>().turnAvailable > GlobalDefinitions.turnNumber))
            CombatResolutionRoutines.checkForAvailableFreeFrenchUnits();

        // Increment the invasion turn parameters
        if ((GlobalDefinitions.firstInvasionAreaIndex > -1) &&
                (GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].invaded &&
                !GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].failed))
        {
            GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].turn++;
        }

        if ((GlobalDefinitions.secondInvasionAreaIndex > -1) &&
                (GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].invaded &&
                (GlobalDefinitions.firstInvasionAreaIndex != GlobalDefinitions.secondInvasionAreaIndex) &&
                !GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].failed))
        {
            GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].turn++;
        }

        GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().initializeAreaCounters();

        // Reset the count of the number of airborne drops this turn
        GlobalDefinitions.currentAirborneDropsThisTurn = 0;

        // Reset the number of air missions used this turn
        GlobalDefinitions.tacticalAirMissionsThisTurn = 0;

        // Reset air mission hex highlights
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().riverInterdiction = false;
            hex.GetComponent<HexDatabaseFields>().closeDefenseSupport = false;
            GlobalDefinitions.unhighlightHex(hex.gameObject);
        }

        // Reset air mission unit highlights
        foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
            unit.GetComponent<UnitDatabaseFields>().unitInterdiction = false;

        // Clear out air mission lists
        GlobalDefinitions.riverInderdictedHexes.Clear();
        GlobalDefinitions.interdictedUnits.Clear();
        GlobalDefinitions.closeDefenseHexes.Clear();

        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().initializeUnits();

        GlobalDefinitions.writeToLogFile("TurnInitializationState: Number of hexes in Allied control = " + GlobalDefinitions.returnNumberOfAlliedHexes());

        GlobalDefinitions.writeToLogFile("TurnInitializationState: executeQuit");
        executeQuit();
    }
}

public class AlliedReplacementState : GameState
{
    public override void initialize()
    {
        GlobalDefinitions.guiUpdatePhase("Allied Replacement Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        // If it is turn 9 or later the check if the allied player gains replacement points
        if (GlobalDefinitions.turnNumber > 8)
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().calculateAlliedRelacementFactors();

        if ((GlobalDefinitions.turnNumber > 8) && (GlobalDefinitions.alliedReplacementsRemaining > 3) &&
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().checkIfAlliedReplacementsAvailable())
        {
            GlobalDefinitions.guiUpdateStatusMessage("Allied replacement factors remaining = " + GlobalDefinitions.alliedReplacementsRemaining + " select an allied unit from the OOB sheet");
            executeMethod = executeSelectUnit; // Initialize the current mode state
        }
        else
        {
            // If no replacements are available then transition to the next state
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = nextGameState;
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
        }
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().selectAlliedReplacementUnit(inputMessage.unit);
        if (GlobalDefinitions.alliedReplacementsRemaining > 3)
            GlobalDefinitions.guiUpdateStatusMessage("Allied replacement factors remaining = " + GlobalDefinitions.alliedReplacementsRemaining + " select an allied unit from the OOB sheet");
        else
            executeQuit();
    }
}

public class SupplyState : GameState
{
    public override void initialize()
    {
        GlobalDefinitions.guiUpdatePhase("Allied Supply Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;

        // I'm using this to execute the available reinforcement ports since it has to execute once
        // Ports that are available for landing reinforcements this turn must be occupied during the supply phase (i.e. before movement)
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().determineAvailableReinforcementPorts();

        if (GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().setAlliedSupplyStatus(false))
        {
            GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().highlightUnsuppliedUnits();
            GlobalDefinitions.writeToLogFile("SupplyState: executing createSupplySourceGUI");
            GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().createSupplySourceGUI(false);
            executeMethod = executeSelectUnit;
        }
        else
            executeQuit();
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        // The mode stays here until the OK button on the gui is pressed to end the process
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().changeUnitSupplyStatus(inputMessage.hex);
    }
}

public class AlliedInvasionState : GameState
{
    public override void initialize()
    {
        GlobalDefinitions.guiUpdatePhase("Allied Invasion Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        if (GlobalDefinitions.turnNumber == 1)
        {
            GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().selectInvasionArea();
            // Initialize mode state - note this is the state that will be executed after the gui selection is made in the call above
            executeMethod = executeSelectUnit;
        }
        else if ((GlobalDefinitions.turnNumber >= 9) && (GlobalDefinitions.turnNumber <= 16) && GlobalDefinitions.secondInvasionAreaIndex == -1)
        {
            // I need to set the method here so the user can see the units while he is making his decision.
            executeMethod = executeSelectUnit;
            GlobalDefinitions.askUserYesNoQuestion("Do you want to launch a second invasion this turn?", ref GlobalDefinitions.SecondInvasionYesButton, ref GlobalDefinitions.SecondInvasionNoButton, executeSecondInvasion, executeNoSecondInvasion);
        }
        else
            executeQuit();
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        // Check if the user has selected a German hex to see what's on it
        if ((inputMessage.unit == null) || (inputMessage.unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
        {
            // Don't do anything
        }
        else if (inputMessage.unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
            GlobalDefinitions.guiDisplayUnitsOnHex(inputMessage.unit.GetComponent<UnitDatabaseFields>().occupiedHex);

        if (GlobalDefinitions.guiList.Count == 0)
        {
            GlobalDefinitions.selectedUnit = GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().getInvadingUnit(inputMessage.unit);
            if (GlobalDefinitions.selectedUnit == null)
                executeMethod = executeSelectUnit; // Stay with this mode if unit not selected
            else
                executeMethod = executeSelectUnitDestination;
        }
    }

    public void executeSelectUnitDestination(InputMessage inputMessage)
    {
        GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().getUnitInvasionHex(GlobalDefinitions.selectedUnit, inputMessage.hex);
        executeMethod = executeSelectUnit;
    }

    /// <summary>
    /// This is the routine that get called if the player answers yes to a second invasion question
    /// </summary>
    public void executeSecondInvasion()
    {
        GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().selectInvasionArea();
        // Initialize mode state - note this is the state that will be executed after the gui selection is made in the call above
        executeMethod = executeSelectUnit;
    }

    /// <summary>
    /// Placeholder for a no answer to the second invasion
    /// </summary>
    public void executeNoSecondInvasion()
    {
        // Note I don't care what's in the input message I just need to pass something here
        executeQuit();
    }

    public override void executeUndo(InputMessage inputMessage)
    {
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<SpriteRenderer>() != null))
        {
            // Can't undo movement of a unit if it has been committed to an attack.  The attack has to be canceled first
            if (!GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
            {

                if (GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex != null)
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnit(
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex,
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.selectedUnit);

                // If there is no beginning hex location on the unit that is because it started the turn in Britain
                else if (currentNationality == GlobalDefinitions.Nationality.Allied)
                {
                    GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().decrementInvasionUnitLimits(GlobalDefinitions.selectedUnit);
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnitBackToBritain(
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.selectedUnit,
                            false);
                }

                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement = GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().movementFactor;
                GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
            else
            {
                GlobalDefinitions.guiUpdateStatusMessage("Unit selected is committed to an attack.  Attack must be canceled in order to undo movement. \nIn order to cancel combat click the Display All Combats button");
                GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Undo failed - no unit selected");

        executeMethod = executeSelectUnit;
    }

    public void loadCombat(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().prepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.German);
    }
}

public class AlliedAirborneState : GameState
{
    private bool alliedAirborneUnitsAvailable = false;
    public override void initialize()
    {
        GlobalDefinitions.guiUpdatePhase("Allied Airborne Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;

        // Initilize mode state
        executeMethod = executeSelectUnit;

        if (GlobalDefinitions.selectedUnit != null)
        {
            GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
            GlobalDefinitions.selectedUnit = null;
        }

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            GlobalDefinitions.unhighlightHex(hex.gameObject);
        }

        alliedAirborneUnitsAvailable = false;
        // The first thing to be done is to see if there are any airborne units in Britain.  There isn't any reason to do anything for 
        // airborne drops if there aren't any units available.
        foreach (Transform unitTransform in GameObject.Find("Units In Britain").transform)
            if ((unitTransform.GetComponent<UnitDatabaseFields>().airborne) && (unitTransform.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber))
                alliedAirborneUnitsAvailable = true;

        if (alliedAirborneUnitsAvailable)
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().setAirborneLimits();

        if ((GlobalDefinitions.maxNumberAirborneDropsThisTurn == 0) || !alliedAirborneUnitsAvailable)
            // If there are no airborne units available move to next state
            executeQuit();
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().processAirborneUnitSelection(inputMessage.unit);

        // Change modes only if a valid unit has been selected
        if (GlobalDefinitions.selectedUnit != null)
            executeMethod = executeSelectUnitDestination;
    }

    public void executeSelectUnitDestination(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().processAirborneDrop(inputMessage.hex);

        // Even if an invalid hex is selected we go back to unit selection since everything is cleared out regardles if the unit dropped
        executeMethod = executeSelectUnit;
    }

    public override void executeUndo(InputMessage inputMessage)
    {
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<SpriteRenderer>() != null))
        {
            // Can't undo movement of a unit if it has been committed to an attack.  The attack has to be canceled first
            if (!GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
            {
                GlobalDefinitions.currentAirborneDropsThisTurn--;
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnitBackToBritain(
                        GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                        GlobalDefinitions.selectedUnit,
                        false);

                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement = GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().movementFactor;
                GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
            else
            {
                GlobalDefinitions.guiUpdateStatusMessage("Unit selected is committed to an attack.  Attack must be canceled in order to undo movement. \nIn order to cancel combat click the Display All Combats button");
                GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Undo failed - no unit selected");

        executeMethod = executeSelectUnit;
    }

    public void loadCombat(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().prepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.German);
    }

    public override void executeQuit()
    {
        GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().moveUnopposedSeaUnits();

        //  Just in case a unit is selected when Q was hit
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<SpriteRenderer>() != null))
        {
            GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
        }
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().removeHexHighlighting();

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = nextGameState;
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
    }
}

public class MovementState : GameState
{
    public override void initialize()
    {
        if (currentNationality == GlobalDefinitions.Nationality.Allied)
        {
            GlobalDefinitions.guiUpdatePhase("Allied Movement Mode");
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;
        }
        else
        {
            GlobalDefinitions.guiUpdatePhase("German Movement Mode");
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;
        }
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;

        // Initialize mode
        executeMethod = executeSelectUnit;
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        GlobalDefinitions.writeToLogFile("executeSelectUnit: executing");
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().processUnitSelectionForMovement(inputMessage.unit, currentNationality);

        if (inputMessage.unit != null)
            if ((currentNationality == GlobalDefinitions.Nationality.Allied) && inputMessage.unit.GetComponent<UnitDatabaseFields>().inBritain)
                executeMethod = executeSelectReinforcementDestination;
            else if (GlobalDefinitions.startHex != null)
            {
                executeMethod = executeSelectUnitDestination;
            }

        // Need to set this so that the desitination routines know what unit is moving
        GlobalDefinitions.selectedUnit = inputMessage.unit;
    }

    public void executeSelectUnitDestination(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().getUnitMoveDestination(GlobalDefinitions.selectedUnit, GlobalDefinitions.startHex,
                inputMessage.hex);
        // Need to make sure the unit didn't move back to Britain
        if (GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
        {
            if (currentNationality == GlobalDefinitions.Nationality.Allied)
                // If an allied unit stops on a hex mark the hex as being in Allied control
                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().alliedControl = true;
            else
            {
                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().alliedControl = false;
                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().successfullyInvaded = false;
            }
        }
        executeMethod = executeSelectUnit;
    }

    public void executeSelectReinforcementDestination(InputMessage inputMessage)
    {
        if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().landAlliedUnitFromOffBoard(GlobalDefinitions.selectedUnit, inputMessage.hex, true))
        {
            GlobalDefinitions.startHex = GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex;
            // We will only be waiting for a destination selection if the hex landed in isn't in enemy ZOC
            if (!GlobalDefinitions.startHex.GetComponent<HexDatabaseFields>().inGermanZOC)
                executeMethod = executeSelectUnitDestination;
            else
                executeMethod = executeSelectUnit;
        }
        else
            executeMethod = executeSelectUnit;
    }

    public override void executeUndo(InputMessage inputMessage)
    {
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<SpriteRenderer>() != null))
        {
            // Can't undo movement of a unit if it has been committed to an attack.  The attack has to be canceled first
            if (!GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
            {

                if (GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex != null)
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnit(
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex,
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.selectedUnit);

                // If there is no beginning hex location on the unit that is because it started the turn in Britain
                else if (currentNationality == GlobalDefinitions.Nationality.Allied)
                {
                    GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().decrementInvasionUnitLimits(GlobalDefinitions.selectedUnit);
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnitBackToBritain(
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.selectedUnit,
                            false);
                }

                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement = GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().movementFactor;
                GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
            else
            {
                GlobalDefinitions.guiUpdateStatusMessage("Unit selected is committed to an attack.  Attack must be canceled in order to undo movement. \nIn order to cancel combat click the Display All Combats button");
                GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Undo failed - no unit selected");

        executeMethod = executeSelectUnit;
    }

    public void loadCombat(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        if (currentNationality == GlobalDefinitions.Nationality.Allied)
            GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().prepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.German);
        else
            GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().prepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.Allied);
    }

    public override void executeQuit()
    {
        // Check if there are any units overstacked.  Can't leave movement with overstacked units.
        if (MovementRoutines.checkIfMovementDone(currentNationality))
        {

            // At the end of the movement mode remove all HQ's in enemy ZOC
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().removeHQInEnemyZOC(currentNationality);

            // Just in case a Q was issued in the middle of a move, unhighlight the selectedUnit
            if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<Renderer>() != null))
            {
                GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().removeHexHighlighting();

            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = nextGameState;
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
        }
        else
            // Units are overstacked so reset the state
            executeMethod = executeSelectUnit;
    }
}

public class CombatState : GameState
{
    public override void initialize()
    {
        if (currentNationality == GlobalDefinitions.Nationality.Allied)
        {
            GlobalDefinitions.guiUpdatePhase("Allied Combat Mode");
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;
        }
        else
        {
            GlobalDefinitions.guiUpdatePhase("German Combat Mode");
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;
        }

        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = true;

        if (GlobalDefinitions.AICombat)
        {
            GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
            GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
            GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
            GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
            GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
            GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = true;
            GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = true;
            GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
            GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
            GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;
        }

        CombatRoutines.checkIfRequiredUnitsAreUncommitted(currentNationality, true);

        // I'm deviating from the rules.  This was the original implementation of carpet bombing.  Per the rules it gets assigned at the beginning of the combat phase.
        // I'm allowing the user to assign carpet bombing just like air support.  It can happen during combat assignment or on the combat resolution screen, before resolution
        // starts.

        // Need to check here on whether the AI is playing the Allied player because we don't want the gui pulled up if it is.  The AI takes care of it separately
        //if ((currentNationality == GlobalDefinitions.Nationality.Allied) && CombatRoutines.checkForCarpetBombing() &&
        //        ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Hotseat) || (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) ||
        //        ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI) && (GlobalDefinitions.currentSidePlaying == GlobalDefinitions.Nationality.Allied))))
        //    CombatRoutines.displayCarpetBombingHexesAvailable();
        //else
        //    executeMethod = executeSelectUnit;

        // Check if the current difficulty needs to be stored as the easiest used
        if (GlobalDefinitions.easiestDifficultySettingUsed > GlobalDefinitions.difficultySetting)
            GlobalDefinitions.easiestDifficultySettingUsed = GlobalDefinitions.difficultySetting;

        // If this is the first combat turn the easiest difficulty need to written.  When loading a saved game from setup the difficulty shouldn't be used
        if (GlobalDefinitions.turnNumber == 1)
            GlobalDefinitions.easiestDifficultySettingUsed = GlobalDefinitions.difficultySetting;

        executeMethod = executeSelectUnit;
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        if (currentNationality == GlobalDefinitions.Nationality.Allied)
            GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().prepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.German);
        else
            GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().prepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.Allied);
    }

    public void executeRetreatMovement(InputMessage inputMessage)
    {
        // Game flow controlled by gui that gets called up
        CombatResolutionRoutines.retreatHexSelection(inputMessage.hex, currentNationality);
    }

    public void executePostCombatMovement(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        CombatResolutionRoutines.executePostCombatMovement(inputMessage.hex);
    }

    public override void executeQuit()
    {
        bool alliedVictory = false;
        bool germanVictory = false;

        // If combat resolutin wasn't started then check to make sure that there aren't units that need to attack or be attacked
        // Note the check for combatResolutionStarted is needed because the result of combat can create must attack units for the next turn so we can't check if there has been resolution
        // not to mention that if combat resolution was started it was already checked that required units were involved in a combat already
        if ((!GlobalDefinitions.combatResolutionStarted) && (CombatRoutines.checkIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality, true)))
        {
            GlobalDefinitions.guiUpdateStatusMessage("Units highlighted are required to be involved in combat this turn and not committed to combat.  Cannot exit combat mode until this is resolved by adding units to combat.");
        }
        else if (GlobalDefinitions.allCombats.Count > 0)
        {
            GlobalDefinitions.guiUpdateStatusMessage("Must resolve committed combats before exiting combat mode.  Click the Display All Combats button and resolve all combats.");
        }
        else
        {
            // Doesn't matter if this is ending AI combat or not, just set it to false
            GlobalDefinitions.AICombat = false;

            // If there were no combat resolutions this turn clear out the hexesAttackedLastTurn
            // This is used for carpet bombing so only pertains to Allies
            if ((!GlobalDefinitions.combatResolutionStarted) && (currentNationality == GlobalDefinitions.Nationality.Allied))
                GlobalDefinitions.hexesAttackedLastTurn.Clear();

            GlobalDefinitions.combatResolutionStarted = false;

            if (currentNationality == GlobalDefinitions.Nationality.Allied)
                CombatResolutionRoutines.endAlliedCombatPhase();
            else
            {
                CombatResolutionRoutines.endGermanCombatPhase();

                // If this is the end of German combat, check if victory conditions have been met
                alliedVictory = GlobalDefinitions.checkForAlliedVictory();
                germanVictory = GlobalDefinitions.checkForGermanVictory();
            }

            if (GlobalDefinitions.AIExecuting)
                GlobalDefinitions.AIExecuting = false;

            GlobalDefinitions.guiUpdateLossRatioText();

            // Only move to the next phase if the vicotry screen isn't being displayed
            if (!alliedVictory && !germanVictory)
            {
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = nextGameState;
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
            }
        }
    }
}

public class AlliedTacticalAirState : GameState
{
    public override void initialize()
    {
        GlobalDefinitions.guiUpdatePhase("Allied Tactical Air Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        executeMethod = nonToggleSelection;

        // All game flow in this state is determined through the gui
        CombatResolutionRoutines.createTacticalAirGUI();
    }

    public void executeCloseDefenseSelection(InputMessage inputMessage)
    {
        CombatResolutionRoutines.setCloseDefenseHex(inputMessage.hex);
    }

    public void executeRiverInterdictionSelection(InputMessage inputMessage)
    {
        CombatResolutionRoutines.getRiverInterdictedHex(inputMessage.hex);
    }

    public void executeUnitInterdictionSelection(InputMessage inputMessage)
    {
        CombatResolutionRoutines.getInterdictedUnit(inputMessage.hex);
    }

    public void nonToggleSelection(InputMessage inputMessage)
    {
        GlobalDefinitions.guiUpdateStatusMessage("Must select the type of air mission from the menu before selecting unit or hex");
    }
}

public class GermanIsolationState : GameState
{
    // There are no modes in this state, all actions get executed by the initialization including the state transition
    public override void initialize()
    {
        // If this is a network game the control needs to be swapped here
        if (GlobalDefinitions.localControl && (GlobalDefinitions.sideControled == GlobalDefinitions.Nationality.Allied) && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
        {
            GlobalDefinitions.writeToLogFile("GermanIsolationState: passing control to remote computer");
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.PASSCONTROLKEYWORK);
            GlobalDefinitions.localControl = false;
        }

        GlobalDefinitions.guiUpdatePhase("German Isolation Check Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;

        //GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().writeSaveTurnFile("EndOfAllied");
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().initializeUnits();
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().setGermanSupplyStatus(false);
        GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().writeSaveTurnFile("EndOfAllied");

        executeQuit();
    }
}

public class GermanReplacementState : GameState
{
    public override void initialize()
    {
        GlobalDefinitions.guiUpdatePhase("German Replacement Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;

        if (GlobalDefinitions.turnNumber > 15)
        {
            GlobalDefinitions.germanReplacementsRemaining += 5;
            GlobalDefinitions.guiUpdateStatusMessage("German replacement factors remaining = " + GlobalDefinitions.germanReplacementsRemaining + " select a German unit from the OOB sheet");
            GlobalDefinitions.guiUpdateStatusMessage("Select a German replacement unit from the OOB sheet or click the End Current Phase button to save the factors for next turn");
            // Initialized mode
            executeMethod = executeSelectUnit;
        }
        else
            executeQuit();
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().selectGermanReplacementUnit(inputMessage.unit))
        {
            GlobalDefinitions.highlightUnit(GlobalDefinitions.selectedUnit);
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().highlightGermanReplacementHexes();
            GlobalDefinitions.guiUpdateStatusMessage("Select a highlighted hex to place the replacement unit");
            executeMethod = executeSelectUnitDestination;
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Selected unit has to be on the OOB sheet.  Select a German replacement unit from the OOB sheet or click the End Current Phase button to save the factors for next turn");
    }

    public void executeSelectUnitDestination(InputMessage inputMessage)
    {
        if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().landGermanUnitFromOffBoard(GlobalDefinitions.selectedUnit, inputMessage.hex))
        {
            if (GlobalDefinitions.germanReplacementsRemaining > 0)
                GlobalDefinitions.guiUpdateStatusMessage("German replacement factors remaining = " + GlobalDefinitions.germanReplacementsRemaining + " select an German unit from the OOB sheet or click the End Current Phase button to save the factors for next turn");
        }
        GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
        GlobalDefinitions.selectedUnit = null;
        if (GlobalDefinitions.germanReplacementsRemaining == 0)
            executeQuit();
        else
            executeMethod = executeSelectUnit;
    }
}

public class GermanAISetupState : GameState
{
    public override void initialize()
    {
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        // Generally the easiest difficulty setting used is updated in the combat state since it is where it is used.  I'm setting it here
        // since a game is started with the setting from the last game and if it isn't reset it will represent the lowest setting ever used, not just in the current game
        GlobalDefinitions.easiestDifficultySettingUsed = GlobalDefinitions.difficultySetting;

        GlobalDefinitions.guiUpdatePhase("German AI Setup Mode");
        GlobalDefinitions.getNewOrSavedGame();

        //GlobalDefinitions.askUserYesNoQuestion("Do you want to load a saved game.  If you answer No a new game will begin", ref GlobalDefinitions.TypeOfGameYesButton, ref GlobalDefinitions.TypeOfGameNoButton, executeYesResponse, executeNoResponse);
    }

    public void executeYesResponse()
    {
        string turnFileName;

        // This calls up the file browser
        turnFileName = GlobalDefinitions.guiFileDialog();

        if (turnFileName != null)
            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().readTurnFile(turnFileName);
    }

    public void executeNoResponse()
    {
        // Randomly pick a German setup file
        int fileNumber = GlobalDefinitions.dieRoll.Next(1, 10);

        GlobalDefinitions.guiUpdateStatusMessage("German setup file number = " + fileNumber);
        GameControl.createBoardInstance.GetComponent<CreateBoard>().readGermanPlacement(GameControl.path + "GermanSetup\\TGCGermanSetup" + fileNumber + ".txt");

        // Executing this to set the ZOC's of the hexes
        SetupRoutines.updateHexFields();

        //executeMethod = executeQuit;
        //GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(GameControl.inputMessage.GetComponent<InputMessage>());
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit();
    }

    public override void executeQuit()
    {
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = nextGameState;
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();
    }
}

public class AlliedAIState : GameState
{
    DateTime executeTime;
    bool alliedAIExecuting;
    InputMessage inputMessageParameter;
    string messageText;

    // Use the udpate routine to show the status of the AI executing
    private void Update()
    {
        if (alliedAIExecuting)
        {
            GlobalDefinitions.removeAllGUIs();
            GlobalDefinitions.guiDisplayAIStatus(messageText);
        }
    }

    //InputMessage inputMessage;
    // There are no modes in this state, all actions get executed by the initialization including the state transition
    public override void initialize()
    {
        GlobalDefinitions.AIExecuting = true;
        alliedAIExecuting = true;
        //inputMessageParameter = inputMessage; // Used to pass the input message to the Coroutine

        GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GlobalDefinitions.localControl = false;
        executeTime = DateTime.Now;
        GlobalDefinitions.writeToLogFile("Starting Allied AI at: " + DateTime.Now);
        GlobalDefinitions.guiUpdatePhase("Allied AI Mode");
        GlobalDefinitions.guiUpdateStatusMessage("Executing AI turn");
        messageText = "Executing AI Turn";
        base.initialize();

        StartCoroutine("executeAlliedAIMode");
    }

    private IEnumerator executeAlliedAIMode()
    {
        messageText = "Initializing Units";
        yield return new WaitForSeconds(.1f);
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().initializeUnits();

        messageText = "Determining Reinforcement Ports";
        yield return new WaitForSeconds(.1f);
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().determineAvailableReinforcementPorts();

        messageText = "Determining Supply Status";
        yield return new WaitForSeconds(.1f);
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().setAlliedSupplyStatus(false);

        // If it's the first or ninth turn execute an invasion
        if ((GlobalDefinitions.turnNumber == 1) || (GlobalDefinitions.turnNumber == 9))
        {
            messageText = "Determine Invasion Site";
            yield return new WaitForSeconds(.1f);
            AIRoutines.determineInvasionSite();
        }

        // If it is turn 9 or later the check if the allied player gains replacement points
        if (GlobalDefinitions.turnNumber > 8)
        {
            messageText = "Calculating Replacement Factors";
            yield return new WaitForSeconds(.1f);
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().calculateAlliedRelacementFactors();
        }

        // Check for replacements
        if ((GlobalDefinitions.turnNumber > 8) && (GlobalDefinitions.alliedReplacementsRemaining > 3) &&
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().checkIfAlliedReplacementsAvailable())
        {
            messageText = "Selecting Replacements";
            yield return new WaitForSeconds(.1f);
            AIRoutines.selectAlliedAIReplacementUnits();
        }

        // Make supply movements with HQ's before combat moves, supply is a problem for the Allies
        messageText = "Making Supply Movmements";
        yield return new WaitForSeconds(.1f);
        AIRoutines.makeSupplyMovements();

        // Set the airborne limits available this turn
        messageText = "Set Airborne Limits";
        yield return new WaitForSeconds(.1f);
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().setAirborneLimits();

        // Make combat moves
        messageText = "Making Combat Movement";
        yield return new WaitForSeconds(.1f);
        List<GameObject> defendingHexes = new List<GameObject>();
        AIRoutines.setAlliedAttackHexValues(defendingHexes);
        AIRoutines.checkForAICombat(GlobalDefinitions.Nationality.Allied, defendingHexes, GlobalDefinitions.germanUnitsOnBoard);

        // Land any reinforcements that are available
        messageText = "Landing Reinforcements";
        yield return new WaitForSeconds(.1f);
        AIRoutines.landAllAlliedReinforcementUnits();

        // We don't want the HQ units moved by any of the coming routines because they don't deal with supply
        // Mark all HQ's as already being moved
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            if (unit.GetComponent<UnitDatabaseFields>().HQ)
                unit.GetComponent<UnitDatabaseFields>().hasMoved = true;

        // Make strategic moves (units that are out of attack range)
        messageText = "Making Strategic Movement";
        yield return new WaitForSeconds(.1f);
        AIRoutines.makeAlliedStrategicMoves(GlobalDefinitions.alliedUnitsOnBoard);

        // Determine movement actions
        messageText = "Moving Remaining Units";
        yield return new WaitForSeconds(.1f);
        AIRoutines.moveAllUnits(GlobalDefinitions.Nationality.Allied);

        // Set flag for tracking how many turns without an attack.
        if (GlobalDefinitions.allCombats.Count == 0)
            GlobalDefinitions.numberOfTurnsWithoutAttack++;
        else
            GlobalDefinitions.numberOfTurnsWithoutAttack = 0;

        // At this point if there are hq units in an enemy ZOC they are eliminated
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().removeHQInEnemyZOC(GlobalDefinitions.Nationality.Allied);

        // There are scenarios where a unit can be blocked from moving so they were not able to escape from an enemy ZOC
        // but were not able to meet the target odds.  Check for units in enemy ZOC here and assign combat regardless of odds
        messageText = "Set Remaining Attacks";
        yield return new WaitForSeconds(.1f);
        AIRoutines.setDefaultAttacks(GlobalDefinitions.Nationality.Allied);

        // Since hex control doesn't change when the AI is moving units set ownership here
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().alliedControl = true;

        // Clear out the combat results from the last turn
        //foreach (GlobalDefinitions.CombatResults result in GlobalDefinitions.combatResultsFromLastTurn)
        //    GlobalDefinitions.writeToLogFile("executeAlliedAIState:    " + result);
        //GlobalDefinitions.writeToLogFile("executeAlliedAIState: Successful attacks = " + AIRoutines.successfulAttacksLastTurn());

        GlobalDefinitions.writeToLogFile("Ending Allied AI at: " + DateTime.Now + " AI ran for " + (DateTime.Now - executeTime));
        GlobalDefinitions.AICombat = true;
        GlobalDefinitions.localControl = true;

        // Get rid of the last status message
        GlobalDefinitions.removeAllGUIs();

        // This will stop the status message from executing in the update() routine
        alliedAIExecuting = false;

        // Pass to interactive control to resolve combats
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = GameControl.alliedCombatStateInstance.GetComponent<CombatState>();
        //GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize(inputMessageParameter);
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();

        if (GlobalDefinitions.allCombats.Count == 0)
        {
            // Quit the combat mode since there are no combats to resolve
            GlobalDefinitions.guiUpdateStatusMessage("No Allied attacks being made this turn - moving to German movement mode");
            //GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit(inputMessageParameter);
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit();
        }
        else
        {
            GlobalDefinitions.guiUpdateStatusMessage("Resolve Allied combats");
            // Call up the resolution gui to resolve combats
            GameControl.GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().executeCombatResolution();
        }

    }
}

public class AlliedAITacticalAirState : GameState
{
    public override void initialize()
    {
        GlobalDefinitions.localControl = false;
        GlobalDefinitions.guiUpdatePhase("Allied AI Tactical Air Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        // When waiting for a second invasion after defeating the first invasion there isn't any need to assign air missions
        GlobalDefinitions.writeToLogFile("AlliedAITacticalAirState: number of allied units on board = " + GlobalDefinitions.alliedUnitsOnBoard.Count);
        if (GlobalDefinitions.alliedUnitsOnBoard.Count > 0)
        {
            AIRoutines.assignAlliedAirMissions();
        }

        GlobalDefinitions.localControl = true;

        executeQuit();
    }
}

public class GermanAIState : GameState
{
    DateTime executeTime;
    string messageText;
    bool germanAIExecuting;
    InputMessage inputMessageParameter;


    private void Update()
    {
        if (germanAIExecuting)
        {
            GlobalDefinitions.removeAllGUIs();
            GlobalDefinitions.guiDisplayAIStatus(messageText);
        }
    }

    // There are no modes in this state, all actions get executed by the initialization including the state transition
    public override void initialize()
    {
        GlobalDefinitions.AIExecuting = true;
        germanAIExecuting = true;
        GlobalDefinitions.localControl = false;
        executeTime = DateTime.Now;
        GlobalDefinitions.writeToLogFile("Starting German AI at: " + DateTime.Now);
        GlobalDefinitions.guiUpdatePhase("German AI Mode");
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        //inputMessageParameter = inputMessage;

        StartCoroutine("executeGermanAIMode");
    }
    private IEnumerator executeGermanAIMode()
    {

        // Note that when the AI is running there are no state transitions.  It will run through all of the actions that need to be executed.

        // Execute the isolation check
        messageText = "AI Initializing Units";
        GlobalDefinitions.writeToLogFile("executeGermanAIMode: initializing units");
        yield return new WaitForSeconds(.1f);
        GlobalDefinitions.guiClearUnitsOnHex();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().initializeUnits();
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().setGermanSupplyStatus(false);
        GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().writeSaveTurnFile("EndOfAllied");

        // Execute replacement selection
        if (GlobalDefinitions.turnNumber > 15)
        {
            messageText = "AI Selecting Replacement Units";
            GlobalDefinitions.writeToLogFile("executeGermanAIMode: selecting replacement units");
            yield return new WaitForSeconds(.1f);
            GlobalDefinitions.germanReplacementsRemaining += 5;
            AIRoutines.germanAIReplacementUnits();
        }

        // Movement
        messageText = "AI Making Reinforcement Moves";
        GlobalDefinitions.writeToLogFile("executeGermanAIMode: moving all reinforcement German units");
        yield return new WaitForSeconds(.1f);
        // Make strategic moves (units that are out of attack range)
        AIRoutines.makeGermanReinforcementMoves();

        // Make combat moves
        messageText = "AI Making Combat Moves";
        GlobalDefinitions.writeToLogFile("executeGermanAIMode: making combat moves");
        yield return new WaitForSeconds(.1f);
        List<GameObject> defendingHexes = new List<GameObject>();
        AIRoutines.setGermanAttackHexValues(defendingHexes);
        AIRoutines.checkForAICombat(GlobalDefinitions.Nationality.German, defendingHexes, GlobalDefinitions.alliedUnitsOnBoard);

        // Determine movement actions
        messageText = "AI Moving Units";
        GlobalDefinitions.writeToLogFile("executeGermanAIMode: moving all units");
        yield return new WaitForSeconds(.1f);
        AIRoutines.moveAllUnits(GlobalDefinitions.Nationality.German);

        // If there are hq units in enemy ZOC at this point they are eliminated
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().removeHQInEnemyZOC(GlobalDefinitions.Nationality.German);

        // There are scenarios where a unit can be blocked from moving so they were not able to escape from an enemy ZOC
        // but were not able to meet the target odds.  Check for units in enemy ZOC here and assign combat regardless of odds
        messageText = "AI Assigning Default Attacks";
        GlobalDefinitions.writeToLogFile("executeGermanAIMode: making default attacks");
        yield return new WaitForSeconds(.1f);
        AIRoutines.setDefaultAttacks(GlobalDefinitions.Nationality.German);

        // Since hex control doesn't change when the AI is moving units set ownership here
        foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
        {
            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().alliedControl = false;
            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().successfullyInvaded = false;
        }

        GlobalDefinitions.writeToLogFile("Ending German AI at: " + DateTime.Now + " AI ran for " + (DateTime.Now - executeTime));
        GlobalDefinitions.AICombat = true;
        GlobalDefinitions.localControl = true;

        // Get rid of the last status gui
        GlobalDefinitions.removeAllGUIs();

        // Stop the AI update messages
        germanAIExecuting = false;

        // Pass to interactive control to resolve combats
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = GameControl.germanCombatStateInstance.GetComponent<CombatState>();
        //GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize(inputMessageParameter);
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize();

        if (GlobalDefinitions.allCombats.Count == 0)
        {
            // Quit the combat mode since there are no combats to resolve
            GlobalDefinitions.guiUpdateStatusMessage("No German attacks being made this turn - moving to Allied movement mode");
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit();
        }
        else
        {
            GlobalDefinitions.guiUpdateStatusMessage("Resolve German combats");
            // Call up the resolution gui to resolve combats
            GameControl.GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().executeCombatResolution();
        }
    }
}

public class VictoryState : GameState
{
    // This is the state that is entered after a victory is achieved
    public override void initialize()
    {
        // Set the play to be in local control we're not looking to keep the two computers in sync anymore
        GlobalDefinitions.localControl = true;

        GlobalDefinitions.guiUpdatePhase("Victory Mode");
        GlobalDefinitions.guiClearUnitsOnHex();
        base.initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        executeMethod = executeSelectUnit;
    }

    public void executeSelectUnit(InputMessage inputMessage)
    {
        // If the hex selected has enemy units on it display them in the gui
        if (inputMessage.unit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
            GlobalDefinitions.guiDisplayUnitsOnHex(inputMessage.unit.GetComponent<UnitDatabaseFields>().occupiedHex);
    }
}
