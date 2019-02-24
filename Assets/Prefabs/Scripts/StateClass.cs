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
public class GameStateControl : MonoBehaviour
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
    public virtual void Initialize()
    {
        // Any state that starts need to have the next phase button available
        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
    }

    public methodDelegateWithParameters executeMethod;

    public virtual void ExecuteUndo(InputMessage inputMessage) { }

    public virtual void ExecuteQuit()
    {
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = nextGameState;
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
    }
}

public class SetUpState : GameState
{
    public override void Initialize()
    {
        base.Initialize();

        // If this is a network game the state will be handeled directly
        if (GlobalDefinitions.gameMode != GlobalDefinitions.GameModeValues.Peer2PeerNetwork)
            executeMethod = ExecuteTypeOfGame;

        GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;
    }

    public void ExecuteTypeOfGame(InputMessage inputMessage)
    {
        GlobalDefinitions.GuiUpdatePhase("Setup Mode");

        // We only need to check for the type of game if it's hotseat or AI and not Network
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Hotseat) || (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI))
            GlobalDefinitions.GetNewOrSavedGame();
    }

    /// <summary>
    /// This executes when the Saved Game option is selected
    /// </summary>
    public void ExecuteSavedGame()
    {
        string turnFileName;

        // Since at this point we know we are starting a saved game and not running the command file, remove the command file
        if (!GlobalDefinitions.commandFileBeingRead)
            if (File.Exists(GameControl.path + GlobalDefinitions.commandFile))
            {
                GlobalDefinitions.DeleteCommandFile();
                GlobalDefinitions.DeleteFullCommandFile();
            }

        // This calls up the file browser
        turnFileName = GlobalDefinitions.GuiFileDialog();

        if (turnFileName == null)
            ExecuteNewGame();
        else
        {

            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadTurnFile(turnFileName);

            // The normal command file is taken care of in the writeSavedTurnFile() routine since it is upated every time a new turn file is written
            // The full command file is taken care of here along with writing the difficulty and aggressiveness settings
            using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalDefinitions.fullCommandFile))
            {
                writeFile.WriteLine(GlobalDefinitions.AGGRESSIVESETTINGKEYWORD + " " + GlobalDefinitions.aggressiveSetting);
                writeFile.WriteLine(GlobalDefinitions.DIFFICULTYSETTINGKEYWORD + " " + GlobalDefinitions.difficultySetting);
                writeFile.WriteLine("SavedTurnFile " + turnFileName);
            }

            // If this is a network game send the file name to the remote computer so it can be reSquested through the file transfer routines.  It's silly that 
            // I have to tell it what to ask for but I bought the code and that is how it works
            if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork) && (GlobalDefinitions.localControl))
                NetworkRoutines.SendSocketMessage(GlobalDefinitions.SENDTURNFILENAMEWORD + " " + turnFileName);
        }
    }

    /// <summary>
    /// This executes when the New Game option is selected
    /// </summary>
    public void ExecuteNewGame()
    {
        int fileNumber;

        // Since at this point we know we are starting a new game and not running the command file, remove the command file
        if (!GlobalDefinitions.commandFileBeingRead)
            if (File.Exists(GameControl.path + GlobalDefinitions.commandFile))
            {
                GlobalDefinitions.DeleteCommandFile();
                GlobalDefinitions.DeleteFullCommandFile();
            }

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

        GlobalDefinitions.GuiUpdateStatusMessage("German setup file number = " + fileNumber);
        GameControl.createBoardInstance.GetComponent<CreateBoard>().ReadGermanPlacement(GameControl.path + "GermanSetup\\TGCGermanSetup" + fileNumber + ".txt");

        // The network communication is a little different for a new game so I can't use the routine to write to the command file
        // since I don't want it sending a message to the remote computer.
        if (!GlobalDefinitions.commandFileBeingRead)
        {            
            using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalDefinitions.fullCommandFile))
            {
                writeFile.WriteLine(GlobalDefinitions.AGGRESSIVESETTINGKEYWORD + " " + GlobalDefinitions.aggressiveSetting);
                writeFile.WriteLine(GlobalDefinitions.DIFFICULTYSETTINGKEYWORD + " " + GlobalDefinitions.difficultySetting);
                writeFile.WriteLine(GlobalDefinitions.PLAYNEWGAMEKEYWORD + " " + fileNumber);
            }
            using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalDefinitions.commandFile))
            {
                writeFile.WriteLine(GlobalDefinitions.AGGRESSIVESETTINGKEYWORD + " " + GlobalDefinitions.aggressiveSetting);
                writeFile.WriteLine(GlobalDefinitions.DIFFICULTYSETTINGKEYWORD + " " + GlobalDefinitions.difficultySetting);
                writeFile.WriteLine(GlobalDefinitions.PLAYNEWGAMEKEYWORD + " " + fileNumber);
            }
        }

        // If this is a game where the computer is playing the Germans then exit out of setup at this point
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI) && (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.Nationality.Allied))
        {
            ExecuteQuit();
        }
        else
        {
            executeMethod = ExecuteSelectUnit;
            GlobalDefinitions.GuiUpdatePhase(currentNationality + " Setup Mode");
            GlobalDefinitions.GuiUpdateStatusMessage("German Setup Mode: Place units in preparation for an invasion.\n        Note that static units must go on coastal hexes, ports, or inland ports\n        German reserves must start on starred hexes in Germany");
            GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = true;
        }
    }

    /// <summary>
    /// wrapper needed to execute IEnumerator
    /// </summary>
    public void ReadCommandFile()
    {
        StartCoroutine("ExecuteCommandFile");
    }

    /// <summary>
    /// Routine used to load the command file.  Needs to run as a parallel process to account for the fact that the AI runs as a parallel process
    /// </summary>
    private IEnumerator ExecuteCommandFile()
    {
        char[] delimiterChars = { ' ' };
        string line;
        string[] switchEntries;

        StreamReader theReader = new StreamReader(GameControl.path + GlobalDefinitions.commandFile);

        using (theReader)
        {
            line = theReader.ReadLine();
            if (line != GlobalDefinitions.commandFileHeader)
            {
                GlobalDefinitions.GuiUpdateStatusMessage("The game mode selected does not match the game mode of the file\nFile game mode = " + line);
                MainMenuRoutines.GetGameModeUI();
            }
            else
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Reading previous game, please wait...");
                GlobalDefinitions.commandFileBeingRead = true;
                do
                {
                    // When reading the command file, need to wait if the AI is executing
                    while ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI) && !GlobalDefinitions.localControl)
                    {
                        yield return new WaitForSeconds(1f);
                    }
                    line = theReader.ReadLine();
                    GlobalDefinitions.WriteToLogFile("readCommandFile: reading line - " + line);
                    Debug.Log("readCommandFile: reading line - " + line);
                    if (line != null)
                    {
                        switchEntries = line.Split(delimiterChars);
                        switch (switchEntries[0])
                        {
                            case "SavedTurnFile":
                                // A path name with a space in it will cause the name to be split.  Anything after the [0] entry needs to be added back together
                                int a = 2;
                                string fileName = switchEntries[1];
                                while (a < switchEntries.Length)
                                {
                                    fileName = fileName + " " + switchEntries[a];
                                    a++;
                                }
                                GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadTurnFile(fileName);
                                break;
                            case GlobalDefinitions.PLAYNEWGAMEKEYWORD:
                                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
                                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();

                                // Set the global parameter on what file to use, can't pass it to the executeNoResponse since it is passed as a method delegate elsewhere
                                GlobalDefinitions.germanSetupFileUsed = Convert.ToInt32(switchEntries[1]);

                                GameControl.setUpStateInstance.GetComponent<SetUpState>().ExecuteNewGame();
                                break;
                            default:
                                ExecuteGameCommand.ProcessCommand(line);
                                break;
                        }
                    }
                }
                while (line != null);
            }
            GlobalDefinitions.commandFileBeingRead = false;
            GlobalDefinitions.GuiUpdateStatusMessage("Read complete");
            theReader.Close();
        }  
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        GlobalDefinitions.selectedUnit =
                GameControl.setupRoutinesInstance.GetComponent<SetupRoutines>().GetUnitToSetup(currentNationality, inputMessage.unit);

        if (GlobalDefinitions.selectedUnit == null)
            executeMethod = ExecuteSelectUnit;
        else
            executeMethod = ExecuteSelectUnitDestination;
    }

    public void ExecuteSelectUnitDestination(InputMessage inputMessage)
    {
        GameControl.setupRoutinesInstance.GetComponent<SetupRoutines>().GetUnitSetupDestination(GlobalDefinitions.selectedUnit, inputMessage.hex);
        executeMethod = ExecuteSelectUnit;
    }

    public override void ExecuteQuit()
    {
        // Generally the easiest difficulty setting used is updated in the combat state sine it is where it is used.  I'm setting it here
        // since a game is started with the setting from the last game and if it isn't reset it will represent the lowest setting ever used, not just in the current game
        GlobalDefinitions.easiestDifficultySettingUsed = GlobalDefinitions.difficultySetting;

        // Just in case a Quit was issued in the middle of a move, unhighlight the selectedUnit
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<Renderer>() != null))
        {
            GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
            GlobalDefinitions.selectedUnit = null;
        }

        if (SetupRoutines.UpdateHexFields())
        {
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = nextGameState;
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
        }
    }
}

public class TurnInitializationState : GameState
{
    // There are no modes in this state, all actions get executed by the initialization including the state transition
    public override void Initialize()
    {
        // If this is a network game the control needs to be swapped here
        if (GlobalDefinitions.localControl && GlobalDefinitions.gameStarted && (GlobalDefinitions.sideControled == GlobalDefinitions.Nationality.German) && 
                (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork))
        {
            GlobalDefinitions.WriteToLogFile("TurnInitializationState: passing control to remote computer");
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.PASSCONTROLKEYWORK);
            GlobalDefinitions.SwitchLocalControl(false);
        }

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GlobalDefinitions.GuiUpdatePhase("Turn initialization");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();

        // Set flag for tracking how many turns without a successful attack.
        if (GlobalDefinitions.SuccessfulAttacksLastTurn() && (GlobalDefinitions.turnNumber > 2))
            GlobalDefinitions.numberOfTurnsWithoutSuccessfulAttack = 0;
        else
            GlobalDefinitions.numberOfTurnsWithoutSuccessfulAttack++;
        GlobalDefinitions.WriteToLogFile("Turn Initialization: number of turns without successful attack = " + GlobalDefinitions.numberOfTurnsWithoutSuccessfulAttack);

        // Write out an end of turn save file 
        if (GlobalDefinitions.turnNumber == 0)
            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().WriteSaveTurnFile("Setup");
        else
            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().WriteSaveTurnFile("EndOfGerman");

        // Increment the turn number
        GlobalDefinitions.turnNumber++;
        GlobalDefinitions.WriteToLogFile("TurnInitialization: Starting turn number " + GlobalDefinitions.turnNumber);
        GlobalDefinitions.GuiUpdateTurn();

        // Update Allied victory weeks display
        GlobalDefinitions.GuiDisplayAlliedVictoryStatus();
        GlobalDefinitions.GuiDisplayAlliedVictoryUnits();

        // One of the Free French units is the French 5th armor.  If it isn't available then check to see if the Free French units are available
        if ((GlobalDefinitions.turnNumber > 27) && (GameObject.Find("Armor-FR-5").GetComponent<UnitDatabaseFields>().turnAvailable > GlobalDefinitions.turnNumber))
            CombatResolutionRoutines.CheckForAvailableFreeFrenchUnits();

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

        GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().InitializeAreaCounters();

        // Reset the count of the number of airborne drops this turn
        GlobalDefinitions.currentAirborneDropsThisTurn = 0;

        // Reset the number of air missions used this turn
        GlobalDefinitions.tacticalAirMissionsThisTurn = 0;

        // Reset air mission hex highlights
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().riverInterdiction = false;
            hex.GetComponent<HexDatabaseFields>().closeDefenseSupport = false;
            GlobalDefinitions.UnhighlightHex(hex.gameObject);
        }

        // Reset air mission unit highlights
        foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
            unit.GetComponent<UnitDatabaseFields>().unitInterdiction = false;

        // Clear out air mission lists
        GlobalDefinitions.riverInderdictedHexes.Clear();
        GlobalDefinitions.interdictedUnits.Clear();
        GlobalDefinitions.closeDefenseHexes.Clear();

        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().InitializeUnits();

        GlobalDefinitions.numberOfHexesInAlliedControl = GlobalDefinitions.ReturnNumberOfHexesInAlliedControl();
        GlobalDefinitions.WriteToLogFile("TurnInitializationState: Number of hexes in Allied control = " + GlobalDefinitions.numberOfHexesInAlliedControl);
        ExecuteQuit();
    }
}

public class AlliedReplacementState : GameState
{
    public override void Initialize()
    {
        GlobalDefinitions.GuiUpdatePhase("Allied Replacement Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

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
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().CalculateAlliedRelacementFactors();

        if ((GlobalDefinitions.turnNumber > 8) && (GlobalDefinitions.alliedReplacementsRemaining > 3) &&
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().CheckIfAlliedReplacementsAvailable())
        {
            GlobalDefinitions.GuiUpdateStatusMessage("Allied replacement factors remaining = " + GlobalDefinitions.alliedReplacementsRemaining + " select an allied unit from the OOB sheet");
            executeMethod = ExecuteSelectUnit; // Initialize the current mode state
        }
        else
        {
            // If no replacements are available then transition to the next state
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = nextGameState;
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
        }
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().SelectAlliedReplacementUnit(inputMessage.unit);
        if (GlobalDefinitions.alliedReplacementsRemaining > 3)
            GlobalDefinitions.GuiUpdateStatusMessage("Allied replacement factors remaining = " + GlobalDefinitions.alliedReplacementsRemaining + " select an allied unit from the OOB sheet");
        else
            ExecuteQuit();
    }
}

public class SupplyState : GameState
{
    public override void Initialize()
    {
        GlobalDefinitions.GuiUpdatePhase("Allied Supply Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;

        // I'm using this to execute the available reinforcement ports since it has to execute once
        // Ports that are available for landing reinforcements this turn must be occupied during the supply phase (i.e. before movement)
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().DetermineAvailableReinforcementPorts();

        if (GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().SetAlliedSupplyStatus(false))
        {
            GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().HighlightUnsuppliedUnits();
            GlobalDefinitions.WriteToLogFile("SupplyState: executing createSupplySourceGUI");
            GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().CreateSupplySourceGUI(false);
            executeMethod = ExecuteSelectUnit;
        }
        else
            ExecuteQuit();
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        // The mode stays here until the OK button on the gui is pressed to end the process
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().ChangeUnitSupplyStatus(inputMessage.hex);
    }
}

public class AlliedInvasionState : GameState
{
    public override void Initialize()
    {
        GlobalDefinitions.GuiUpdatePhase("Allied Invasion Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

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
            GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().SelectInvasionArea();
            // Initialize mode state - note this is the state that will be executed after the gui selection is made in the call above
            executeMethod = ExecuteSelectUnit;
        }
        else if ((GlobalDefinitions.turnNumber >= 9) && (GlobalDefinitions.turnNumber <= 16) && GlobalDefinitions.secondInvasionAreaIndex == -1)
        {
            // I need to set the method here so the user can see the units while he is making his decision.
            executeMethod = ExecuteSelectUnit;
            GlobalDefinitions.AskUserYesNoQuestion("Do you want to launch a second invasion this turn?", ref GlobalDefinitions.SecondInvasionYesButton, ref GlobalDefinitions.SecondInvasionNoButton, ExecuteSecondInvasion, ExecuteNoSecondInvasion);
        }
        else
            ExecuteQuit();
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        // Check if the user has selected a German hex to see what's on it
        if ((inputMessage.unit == null) || (inputMessage.unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
        {
            // Don't do anything
        }
        else if (inputMessage.unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
            GlobalDefinitions.GuiDisplayUnitsOnHex(inputMessage.unit.GetComponent<UnitDatabaseFields>().occupiedHex);

        if (GlobalDefinitions.guiList.Count == 0)
        {
            GlobalDefinitions.selectedUnit = GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().GetInvadingUnit(inputMessage.unit);
            if (GlobalDefinitions.selectedUnit == null)
                executeMethod = ExecuteSelectUnit; // Stay with this mode if unit not selected
            else
                executeMethod = ExecuteSelectUnitDestination;
        }
    }

    public void ExecuteSelectUnitDestination(InputMessage inputMessage)
    {
        GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().GetUnitInvasionHex(GlobalDefinitions.selectedUnit, inputMessage.hex);
        executeMethod = ExecuteSelectUnit;
    }

    /// <summary>
    /// This is the routine that get called if the player answers yes to a second invasion question
    /// </summary>
    public void ExecuteSecondInvasion()
    {
        GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().SelectInvasionArea();
        GlobalDefinitions.invasionsTookPlaceThisTurn = true;
        // Initialize mode state - note this is the state that will be executed after the gui selection is made in the call above
        executeMethod = ExecuteSelectUnit;
    }

    /// <summary>
    /// Placeholder for a no answer to the second invasion
    /// </summary>
    public void ExecuteNoSecondInvasion()
    {
        // Note I don't care what's in the input message I just need to pass something here
        ExecuteQuit();
    }

    public override void ExecuteUndo(InputMessage inputMessage)
    {
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<SpriteRenderer>() != null))
        {
            // Can't undo movement of a unit if it has been committed to an attack.  The attack has to be canceled first
            if (!GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
            {

                if (GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex != null)
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex,
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.selectedUnit);

                // If there is no beginning hex location on the unit that is because it started the turn in Britain
                else if (currentNationality == GlobalDefinitions.Nationality.Allied)
                {
                    GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().DecrementInvasionUnitLimits(GlobalDefinitions.selectedUnit);
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.selectedUnit,
                            false);
                }

                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement = GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().movementFactor;
                GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
            else
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Unit selected is committed to an attack.  Attack must be canceled in order to undo movement. \nIn order to cancel combat click the Display All Combats button");
                GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
        }
        else
            GlobalDefinitions.GuiUpdateStatusMessage("Undo failed - no unit selected");

        executeMethod = ExecuteSelectUnit;
    }

    public void LoadCombat(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().PrepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.German);
    }
}

public class AlliedAirborneState : GameState
{
    private bool alliedAirborneUnitsAvailable = false;
    public override void Initialize()
    {
        GlobalDefinitions.GuiUpdatePhase("Allied Airborne Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;

        // Initilize mode state
        executeMethod = ExecuteSelectUnit;

        if (GlobalDefinitions.selectedUnit != null)
        {
            GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
            GlobalDefinitions.selectedUnit = null;
        }

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
            GlobalDefinitions.UnhighlightHex(hex.gameObject);
        }

        alliedAirborneUnitsAvailable = false;
        // The first thing to be done is to see if there are any airborne units in Britain.  There isn't any reason to do anything for 
        // airborne drops if there aren't any units available.
        foreach (Transform unitTransform in GameObject.Find("Units In Britain").transform)
            if ((unitTransform.GetComponent<UnitDatabaseFields>().airborne) && (unitTransform.GetComponent<UnitDatabaseFields>().turnAvailable <= GlobalDefinitions.turnNumber))
                alliedAirborneUnitsAvailable = true;

        if (alliedAirborneUnitsAvailable)
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().SetAirborneLimits();

        if ((GlobalDefinitions.maxNumberAirborneDropsThisTurn == 0) || !alliedAirborneUnitsAvailable)
            // If there are no airborne units available move to next state
            ExecuteQuit();
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ProcessAirborneUnitSelection(inputMessage.unit);

        // Change modes only if a valid unit has been selected
        if (GlobalDefinitions.selectedUnit != null)
            executeMethod = ExecuteSelectUnitDestination;
    }

    public void ExecuteSelectUnitDestination(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ProcessAirborneDrop(inputMessage.hex);

        // Even if an invalid hex is selected we go back to unit selection since everything is cleared out regardles if the unit dropped
        executeMethod = ExecuteSelectUnit;
    }

    public override void ExecuteUndo(InputMessage inputMessage)
    {
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<SpriteRenderer>() != null))
        {
            // Can't undo movement of a unit if it has been committed to an attack.  The attack has to be canceled first
            if (!GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
            {
                GlobalDefinitions.currentAirborneDropsThisTurn--;
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(
                        GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                        GlobalDefinitions.selectedUnit,
                        false);

                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement = GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().movementFactor;
                GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
            else
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Unit selected is committed to an attack.  Attack must be canceled in order to undo movement. \nIn order to cancel combat click the Display All Combats button");
                GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
        }
        else
            GlobalDefinitions.GuiUpdateStatusMessage("Undo failed - no unit selected");

        executeMethod = ExecuteSelectUnit;
    }

    public void LoadCombat(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().PrepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.German);
    }

    public override void ExecuteQuit()
    {
        GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().MoveUnopposedSeaUnits();

        //  Just in case a unit is selected when Q was hit
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<SpriteRenderer>() != null))
        {
            GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
        }
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().RemoveHexHighlighting();

        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = nextGameState;
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
    }
}

public class MovementState : GameState
{
    public override void Initialize()
    {
        if (currentNationality == GlobalDefinitions.Nationality.Allied)
        {
            GlobalDefinitions.GuiUpdatePhase("Allied Movement Mode");
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;
        }
        else
        {
            GlobalDefinitions.GuiUpdatePhase("German Movement Mode");
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;
        }
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;

        // Initialize mode
        executeMethod = ExecuteSelectUnit;
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ProcessUnitSelectionForMovement(inputMessage.unit, currentNationality);

        if (inputMessage.unit != null)
            if ((currentNationality == GlobalDefinitions.Nationality.Allied) && inputMessage.unit.GetComponent<UnitDatabaseFields>().inBritain)
                executeMethod = ExecuteSelectReinforcementDestination;
            else if (GlobalDefinitions.startHex != null)
            {
                executeMethod = ExecuteSelectUnitDestination;
            }

        // Need to set this so that the desitination routines know what unit is moving
        GlobalDefinitions.selectedUnit = inputMessage.unit;
    }

    public void ExecuteSelectUnitDestination(InputMessage inputMessage)
    {
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().GetUnitMoveDestination(GlobalDefinitions.selectedUnit, GlobalDefinitions.startHex,
                inputMessage.hex);

        executeMethod = ExecuteSelectUnit;
    }

    public void ExecuteSelectReinforcementDestination(InputMessage inputMessage)
    {
        if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandAlliedUnitFromOffBoard(GlobalDefinitions.selectedUnit, inputMessage.hex, true))
        {
            GlobalDefinitions.startHex = GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex;
            // We will only be waiting for a destination selection if the hex landed in isn't in enemy ZOC
            if (!GlobalDefinitions.startHex.GetComponent<HexDatabaseFields>().inGermanZOC)
                executeMethod = ExecuteSelectUnitDestination;
            else
                executeMethod = ExecuteSelectUnit;
        }
        else
            executeMethod = ExecuteSelectUnit;
    }

    public override void ExecuteUndo(InputMessage inputMessage)
    {
        if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<SpriteRenderer>() != null))
        {
            // Can't undo movement of a unit if it has been committed to an attack.  The attack has to be canceled first
            if (!GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
            {

                if (GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex != null)
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().beginningTurnHex,
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.selectedUnit);

                // If there is no beginning hex location on the unit that is because it started the turn in Britain
                else if (currentNationality == GlobalDefinitions.Nationality.Allied)
                {
                    GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().DecrementInvasionUnitLimits(GlobalDefinitions.selectedUnit);
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnitBackToBritain(
                            GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.selectedUnit,
                            false);
                }

                GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().remainingMovement = GlobalDefinitions.selectedUnit.GetComponent<UnitDatabaseFields>().movementFactor;
                GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
            else
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Unit selected is committed to an attack.  Attack must be canceled in order to undo movement. \nIn order to cancel combat click the Display All Combats button");
                GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
        }
        else
            GlobalDefinitions.GuiUpdateStatusMessage("Undo failed - no unit selected");

        executeMethod = ExecuteSelectUnit;
    }

    public void LoadCombat(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        if (currentNationality == GlobalDefinitions.Nationality.Allied)
            GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().PrepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.German);
        else
            GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().PrepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.Allied);
    }

    public override void ExecuteQuit()
    {
        // Check if there are any units overstacked.  Can't leave movement with overstacked units.
        if (MovementRoutines.CheckIfMovementDone(currentNationality))
        {

            // At the end of the movement mode remove all HQ's in enemy ZOC
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().RemoveHQInEnemyZOC(currentNationality);

            // Just in case a Q was issued in the middle of a move, unhighlight the selectedUnit
            if ((GlobalDefinitions.selectedUnit != null) && (GlobalDefinitions.selectedUnit.GetComponent<Renderer>() != null))
            {
                GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                GlobalDefinitions.selectedUnit = null;
            }
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().RemoveHexHighlighting();

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = nextGameState;
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
        }
        else
            // Units are overstacked so reset the state
            executeMethod = ExecuteSelectUnit;
    }
}

public class CombatState : GameState
{
    public override void Initialize()
    {
        if (currentNationality == GlobalDefinitions.Nationality.Allied)
        {
            GlobalDefinitions.GuiUpdatePhase("Allied Combat Mode");
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;
        }
        else
        {
            GlobalDefinitions.GuiUpdatePhase("German Combat Mode");
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;
        }

        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();

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

        CombatRoutines.CheckIfRequiredUnitsAreUncommitted(currentNationality, true);

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

        executeMethod = ExecuteSelectUnit;
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        if (currentNationality == GlobalDefinitions.Nationality.Allied)
            GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().PrepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.German);
        else
            GameControl.combatRoutinesInstance.GetComponent<CombatRoutines>().PrepForCombatDisplay(inputMessage.hex, GlobalDefinitions.Nationality.Allied);
    }

    public void ExecuteRetreatMovement(InputMessage inputMessage)
    {
        // Game flow controlled by gui that gets called up
        CombatResolutionRoutines.RetreatHexSelection(inputMessage.hex, currentNationality);
    }

    public void ExecutePostCombatMovement(InputMessage inputMessage)
    {
        // Game flow controled by gui that gets called up
        CombatResolutionRoutines.ExecutePostCombatMovement(inputMessage.hex);
    }

    public override void ExecuteQuit()
    {
        bool alliedVictory = false;
        bool germanVictory = false;

        // If combat resolutin wasn't started then check to make sure that there aren't units that need to attack or be attacked
        // Note the check for combatResolutionStarted is needed because the result of combat can create must attack units for the next turn so we can't check if there has been resolution
        // not to mention that if combat resolution was started it was already checked that required units were involved in a combat already
        if ((!GlobalDefinitions.combatResolutionStarted) && (CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, true)))
        {
            GlobalDefinitions.GuiUpdateStatusMessage("Units highlighted are required to be involved in combat this turn and not committed to combat.  Cannot exit combat mode until this is resolved by adding units to combat.");
        }
        else if (GlobalDefinitions.allCombats.Count > 0)
        {
            GlobalDefinitions.GuiUpdateStatusMessage("Must resolve committed combats before exiting combat mode.  Click the Display All Combats button and resolve all combats.");
        }
        else
        {
            // Doesn't matter if this is ending AI combat or not, just set it to false
            GlobalDefinitions.AICombat = false;

            // If there were no combat resolutions this turn clear out the hexesAttackedLastTurn
            // This is used for carpet bombing so only pertains to Allies
            if ((!GlobalDefinitions.combatResolutionStarted) && (currentNationality == GlobalDefinitions.Nationality.Allied))
            {
                GlobalDefinitions.hexesAttackedLastTurn.Clear();
                GlobalDefinitions.combatResultsFromLastTurn.Clear();
            }

            GlobalDefinitions.combatResolutionStarted = false;

            if (currentNationality == GlobalDefinitions.Nationality.Allied)
                CombatResolutionRoutines.EndAlliedCombatPhase();
            else
            {
                CombatResolutionRoutines.EndGermanCombatPhase();

                // If this is the end of German combat, check if victory conditions have been met
                alliedVictory = GlobalDefinitions.CheckForAlliedVictory();
                germanVictory = GlobalDefinitions.CheckForGermanVictory();
            }

            if (GlobalDefinitions.AIExecuting)
                GlobalDefinitions.AIExecuting = false;

            GlobalDefinitions.GuiUpdateLossRatioText();

            // Only move to the next phase if the vicotry screen isn't being displayed
            if (!alliedVictory && !germanVictory)
            {
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = nextGameState;
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
            }
        }
    }
}

public class AlliedTacticalAirState : GameState
{
    public override void Initialize()
    {
        GlobalDefinitions.GuiUpdatePhase("Allied Tactical Air Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        executeMethod = NonToggleSelection;

        // All game flow in this state is determined through the gui
        CombatResolutionRoutines.CreateTacticalAirGUI();
    }

    public void ExecuteCloseDefenseSelection(InputMessage inputMessage)
    {
        CombatResolutionRoutines.SetCloseDefenseHex(inputMessage.hex);
    }

    public void ExecuteRiverInterdictionSelection(InputMessage inputMessage)
    {
        CombatResolutionRoutines.GetRiverInterdictedHex(inputMessage.hex);
    }

    public void ExecuteUnitInterdictionSelection(InputMessage inputMessage)
    {
        CombatResolutionRoutines.GetInterdictedUnit(inputMessage.hex);
    }

    public void NonToggleSelection(InputMessage inputMessage)
    {
        GlobalDefinitions.GuiUpdateStatusMessage("Must select the type of air mission from the menu before selecting unit or hex");
    }
}

public class GermanIsolationState : GameState
{
    // There are no modes in this state, all actions get executed by the initialization including the state transition
    public override void Initialize()
    {
        // If this is a network game the control needs to be swapped here
        if (GlobalDefinitions.localControl && (GlobalDefinitions.sideControled == GlobalDefinitions.Nationality.Allied) && 
                (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork))
        {
            GlobalDefinitions.WriteToLogFile("GermanIsolationState: passing control to remote computer");
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.PASSCONTROLKEYWORK);
            GlobalDefinitions.SwitchLocalControl(false);
        }

        GlobalDefinitions.GuiUpdatePhase("German Isolation Check Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;

        //GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().writeSaveTurnFile("EndOfAllied");
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().InitializeUnits();
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().SetGermanSupplyStatus(false);
        GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().WriteSaveTurnFile("EndOfAllied");

        ExecuteQuit();
    }
}

public class GermanReplacementState : GameState
{
    public override void Initialize()
    {
        GlobalDefinitions.GuiUpdatePhase("German Replacement Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;

        if (GlobalDefinitions.turnNumber > 15)
        {
            GlobalDefinitions.germanReplacementsRemaining += 5;
            GlobalDefinitions.GuiUpdateStatusMessage("German replacement factors remaining = " + GlobalDefinitions.germanReplacementsRemaining + "\nSelect a German replacement unit from the OOB sheet or click the End Current Phase button to save the factors for next turn");
            // Initialized mode
            executeMethod = ExecuteSelectUnit;
        }
        else
            ExecuteQuit();
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().SelectGermanReplacementUnit(inputMessage.unit))
        {
            GlobalDefinitions.HighlightUnit(GlobalDefinitions.selectedUnit);
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().HighlightGermanReplacementHexes();
            GlobalDefinitions.GuiUpdateStatusMessage("Select a highlighted hex to place the replacement unit");
            executeMethod = ExecuteSelectUnitDestination;
        }
        else
            GlobalDefinitions.GuiUpdateStatusMessage("Selected unit has to be on the OOB sheet.\nGerman replacement factors remaining = " + GlobalDefinitions.germanReplacementsRemaining + "\nSelect a German replacement unit from the OOB sheet\nor click the End Current Phase button to save the factors for next turn");
    }

    public void ExecuteSelectUnitDestination(InputMessage inputMessage)
    {
        if (GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().LandGermanUnitFromOffBoard(GlobalDefinitions.selectedUnit, inputMessage.hex))
        {
            if (GlobalDefinitions.germanReplacementsRemaining > 0)
                GlobalDefinitions.GuiUpdateStatusMessage("German replacement factors remaining = " + GlobalDefinitions.germanReplacementsRemaining + " select an German unit from the OOB sheet or click the End Current Phase button to save the factors for next turn");
        }
        GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
        GlobalDefinitions.selectedUnit = null;
        if (GlobalDefinitions.germanReplacementsRemaining == 0)
            ExecuteQuit();
        else
            executeMethod = ExecuteSelectUnit;
    }
}

public class GermanAISetupState : GameState
{
    public override void Initialize()
    {
        base.Initialize();

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

        GlobalDefinitions.GuiUpdatePhase("German AI Setup Mode");
        GlobalDefinitions.GetNewOrSavedGame();

        //GlobalDefinitions.askUserYesNoQuestion("Do you want to load a saved game.  If you answer No a new game will begin", ref GlobalDefinitions.TypeOfGameYesButton, ref GlobalDefinitions.TypeOfGameNoButton, executeYesResponse, executeNoResponse);
    }

    public void ExecuteYesResponse()
    {
        string turnFileName;

        // This calls up the file browser
        turnFileName = GlobalDefinitions.GuiFileDialog();

        if (turnFileName != null)
            GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadTurnFile(turnFileName);
    }

    public void ExecuteNoResponse()
    {
        // Randomly pick a German setup file
        int fileNumber = GlobalDefinitions.dieRoll.Next(1, 10);

        GlobalDefinitions.GuiUpdateStatusMessage("German setup file number = " + fileNumber);
        GameControl.createBoardInstance.GetComponent<CreateBoard>().ReadGermanPlacement(GameControl.path + "GermanSetup\\TGCGermanSetup" + fileNumber + ".txt");

        // Executing this to set the ZOC's of the hexes
        SetupRoutines.UpdateHexFields();

        //executeMethod = executeQuit;
        //GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(GameControl.inputMessage.GetComponent<InputMessage>());
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.ExecuteQuit();
    }

    public override void ExecuteQuit()
    {
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = nextGameState;
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
    }
}

public class AlliedAIState : GameState
{
    DateTime executeTime;
    bool alliedAIExecuting;
    string messageText;

    // Use the udpate routine to show the status of the AI executing
    private void Update()
    {
        if (alliedAIExecuting)
        {
            GlobalDefinitions.RemoveAllGUIs();
            GlobalDefinitions.GuiDisplayAIStatus(messageText);
        }
    }

    // There are no modes in this state, all actions get executed by the initialization including the state transition
    public override void Initialize()
    {
        GlobalDefinitions.AIExecuting = true;
        alliedAIExecuting = true;

        GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GlobalDefinitions.SwitchLocalControl(false);
        executeTime = DateTime.Now;
        GlobalDefinitions.WriteToLogFile("Starting Allied AI at: " + DateTime.Now);
        GlobalDefinitions.GuiUpdatePhase("Allied AI Mode");
        GlobalDefinitions.GuiUpdateStatusMessage("Executing AI turn");
        messageText = "Executing AI Turn";
        base.Initialize();

        StartCoroutine("ExecuteAlliedAIMode");
    }

    private IEnumerator ExecuteAlliedAIMode()
    {
        messageText = "Initializing Units";
        yield return new WaitForSeconds(.1f);
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().InitializeUnits();

        messageText = "Determining Reinforcement Ports";
        yield return new WaitForSeconds(.1f);
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().DetermineAvailableReinforcementPorts();

        messageText = "Determining Supply Status";
        yield return new WaitForSeconds(.1f);
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().SetAlliedSupplyStatus(false);

        // If it's the first or ninth turn execute an invasion
        if ((GlobalDefinitions.turnNumber == 1) || (GlobalDefinitions.turnNumber == 9))
        {
            messageText = "Determine Invasion Site";
            yield return new WaitForSeconds(.1f);
            AIRoutines.DetermineInvasionSite();
        }

        // If it is turn 9 or later the check if the allied player gains replacement points
        if (GlobalDefinitions.turnNumber > 8)
        {
            messageText = "Calculating Replacement Factors";
            yield return new WaitForSeconds(.1f);
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().CalculateAlliedRelacementFactors();
        }

        // Check for replacements
        if ((GlobalDefinitions.turnNumber > 8) && (GlobalDefinitions.alliedReplacementsRemaining > 3) &&
                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().CheckIfAlliedReplacementsAvailable())
        {
            messageText = "Selecting Replacements";
            yield return new WaitForSeconds(.1f);
            AIRoutines.SelectAlliedAIReplacementUnits();
        }

        // Make supply movements with HQ's before combat moves, supply is a problem for the Allies
        messageText = "Making Supply Movmements";
        yield return new WaitForSeconds(.1f);
        AIRoutines.MakeSupplyMovements();

        // Set the airborne limits available this turn
        messageText = "Set Airborne Limits";
        yield return new WaitForSeconds(.1f);
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().SetAirborneLimits();

        // Make combat moves
        messageText = "Making Combat Movement";
        yield return new WaitForSeconds(.1f);
        List<GameObject> defendingHexes = new List<GameObject>();
        AIRoutines.SetAlliedAttackHexValues(defendingHexes);
        AIRoutines.CheckForAICombat(GlobalDefinitions.Nationality.Allied, defendingHexes, GlobalDefinitions.germanUnitsOnBoard);

        // Land any reinforcements that are available
        messageText = "Landing Reinforcements";
        yield return new WaitForSeconds(.1f);
        AIRoutines.LandAllAlliedReinforcementUnits();

        // We don't want the HQ units moved by any of the coming routines because they don't deal with supply
        // Mark all HQ's as already being moved
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            if (unit.GetComponent<UnitDatabaseFields>().HQ)
                unit.GetComponent<UnitDatabaseFields>().hasMoved = true;

        // Make strategic moves (units that are out of attack range)
        messageText = "Making Strategic Movement";
        yield return new WaitForSeconds(.1f);
        AIRoutines.MakeAlliedStrategicMoves(GlobalDefinitions.alliedUnitsOnBoard);

        // Determine movement actions
        messageText = "Moving Remaining Units";
        yield return new WaitForSeconds(.1f);
        AIRoutines.MoveAllUnits(GlobalDefinitions.Nationality.Allied);

        // At this point if there are hq units in an enemy ZOC they are eliminated
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().RemoveHQInEnemyZOC(GlobalDefinitions.Nationality.Allied);

        // There are scenarios where a unit can be blocked from moving so they were not able to escape from an enemy ZOC
        // but were not able to meet the target odds.  Check for units in enemy ZOC here and assign combat regardless of odds
        messageText = "Set Remaining Attacks";
        yield return new WaitForSeconds(.1f);
        AIRoutines.SetDefaultAttacks(GlobalDefinitions.Nationality.Allied);

        // Since hex control doesn't change when the AI is moving units set ownership here
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().alliedControl = true;

        // Clear out the combat results from the last turn
        //foreach (GlobalDefinitions.CombatResults result in GlobalDefinitions.combatResultsFromLastTurn)
        //    GlobalDefinitions.writeToLogFile("executeAlliedAIState:    " + result);
        //GlobalDefinitions.writeToLogFile("executeAlliedAIState: Successful attacks = " + AIRoutines.successfulAttacksLastTurn());

        GlobalDefinitions.WriteToLogFile("Ending Allied AI at: " + DateTime.Now + " AI ran for " + (DateTime.Now - executeTime));
        GlobalDefinitions.AICombat = true;
        GlobalDefinitions.SwitchLocalControl(true);

        // Get rid of the last status message
        GlobalDefinitions.RemoveAllGUIs();

        // This will stop the status message from executing in the update() routine
        alliedAIExecuting = false;

        // Pass to interactive control to resolve combats
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.alliedCombatStateInstance.GetComponent<CombatState>();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();

        if (GlobalDefinitions.allCombats.Count == 0)
        {
            // Quit the combat mode since there are no combats to resolve
            GlobalDefinitions.GuiUpdateStatusMessage("No Allied attacks being made this turn - moving to German movement mode");
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.ExecuteQuit();
        }
        else
        {
            GlobalDefinitions.GuiUpdateStatusMessage("Resolve Allied combats");

            AIRoutines.CheckForAICarpetBombingShouldBeAdded();

            // Call up the resolution gui to resolve combats
            GameControl.GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().ExecuteCombatResolution();
        }

    }
}

public class AlliedAITacticalAirState : GameState
{
    public override void Initialize()
    {
        GlobalDefinitions.SwitchLocalControl(false);
        GlobalDefinitions.GuiUpdatePhase("Allied AI Tactical Air Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.Allied;

        // When waiting for a second invasion after defeating the first invasion there isn't any need to assign air missions
        GlobalDefinitions.WriteToLogFile("AlliedAITacticalAirState: number of allied units on board = " + GlobalDefinitions.alliedUnitsOnBoard.Count);
        if (GlobalDefinitions.alliedUnitsOnBoard.Count > 0)
        {
            AIRoutines.AssignAlliedAirMissions();
        }

        GlobalDefinitions.SwitchLocalControl(true);

        ExecuteQuit();
    }
}

public class GermanAIState : GameState
{
    DateTime executeTime;
    string messageText;
    bool germanAIExecuting;


    private void Update()
    {
        if (germanAIExecuting)
        {
            GlobalDefinitions.RemoveAllGUIs();
            GlobalDefinitions.GuiDisplayAIStatus(messageText);
        }
    }

    // There are no modes in this state, all actions get executed by the initialization including the state transition
    public override void Initialize()
    {
        GlobalDefinitions.AIExecuting = true;
        germanAIExecuting = true;
        GlobalDefinitions.SwitchLocalControl(false);
        executeTime = DateTime.Now;
        GlobalDefinitions.WriteToLogFile("Starting German AI at: " + DateTime.Now);
        GlobalDefinitions.GuiUpdatePhase("German AI Mode");
        base.Initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        StartCoroutine("ExecuteGermanAIMode");
    }
    private IEnumerator ExecuteGermanAIMode()
    {

        // Note that when the AI is running there are no state transitions.  It will run through all of the actions that need to be executed.

        // Execute the isolation check
        messageText = "AI Initializing Units";
        GlobalDefinitions.WriteToLogFile("executeGermanAIMode: initializing units");
        yield return new WaitForSeconds(.1f);
        GlobalDefinitions.GuiClearUnitsOnHex();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality = GlobalDefinitions.Nationality.German;
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().InitializeUnits();
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().SetGermanSupplyStatus(false);
        GameControl.readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().WriteSaveTurnFile("EndOfAllied");

        // Execute replacement selection
        if (GlobalDefinitions.turnNumber > 15)
        {
            messageText = "AI Selecting Replacement Units";
            GlobalDefinitions.WriteToLogFile("executeGermanAIMode: selecting replacement units");
            yield return new WaitForSeconds(.1f);
            GlobalDefinitions.germanReplacementsRemaining += 5;
            AIRoutines.GermanAIReplacementUnits();
        }

        // Movement
        messageText = "AI Making Reinforcement Moves";
        GlobalDefinitions.WriteToLogFile("executeGermanAIMode: moving all reinforcement German units");
        yield return new WaitForSeconds(.1f);
        // Make strategic moves (units that are out of attack range)
        AIRoutines.MakeGermanReinforcementMoves();

        // Make combat moves
        messageText = "AI Making Combat Moves";
        GlobalDefinitions.WriteToLogFile("executeGermanAIMode: making combat moves");
        yield return new WaitForSeconds(.1f);
        List<GameObject> defendingHexes = new List<GameObject>();
        AIRoutines.SetGermanAttackHexValues(defendingHexes);
        AIRoutines.CheckForAICombat(GlobalDefinitions.Nationality.German, defendingHexes, GlobalDefinitions.alliedUnitsOnBoard);

        // Determine movement actions
        messageText = "AI Moving Units";
        GlobalDefinitions.WriteToLogFile("executeGermanAIMode: moving all units");
        yield return new WaitForSeconds(.1f);
        AIRoutines.MoveAllUnits(GlobalDefinitions.Nationality.German);

        // If there are hq units in enemy ZOC at this point they are eliminated
        GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().RemoveHQInEnemyZOC(GlobalDefinitions.Nationality.German);

        // There are scenarios where a unit can be blocked from moving so they were not able to escape from an enemy ZOC
        // but were not able to meet the target odds.  Check for units in enemy ZOC here and assign combat regardless of odds
        messageText = "AI Assigning Default Attacks";
        GlobalDefinitions.WriteToLogFile("executeGermanAIMode: making default attacks");
        yield return new WaitForSeconds(.1f);
        AIRoutines.SetDefaultAttacks(GlobalDefinitions.Nationality.German);

        // Since hex control doesn't change when the AI is moving units set ownership here
        foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
        {
            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().alliedControl = false;
            unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().successfullyInvaded = false;
        }

        GlobalDefinitions.WriteToLogFile("Ending German AI at: " + DateTime.Now + " AI ran for " + (DateTime.Now - executeTime));
        GlobalDefinitions.AICombat = true;
        GlobalDefinitions.SwitchLocalControl(true);

        // Get rid of the last status gui
        GlobalDefinitions.RemoveAllGUIs();

        // Stop the AI update messages
        germanAIExecuting = false;

        // Pass to interactive control to resolve combats
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.germanCombatStateInstance.GetComponent<CombatState>();
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();

        if (GlobalDefinitions.allCombats.Count == 0)
        {
            // Quit the combat mode since there are no combats to resolve
            GlobalDefinitions.GuiUpdateStatusMessage("No German attacks being made this turn - moving to Allied movement mode");
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.ExecuteQuit();
        }
        else
        {
            GlobalDefinitions.GuiUpdateStatusMessage("Resolve German combats");
            // Call up the resolution gui to resolve combats
            GameControl.GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().ExecuteCombatResolution();
        }
    }
}

public class VictoryState : GameState
{
    // This is the state that is entered after a victory is achieved
    public override void Initialize()
    {
        // Set the play to be in local control we're not looking to keep the two computers in sync anymore
        GlobalDefinitions.SwitchLocalControl(true);

        GlobalDefinitions.GuiUpdatePhase("Victory Mode");
        GlobalDefinitions.GuiClearUnitsOnHex();
        base.Initialize();

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = false;

        executeMethod = ExecuteSelectUnit;
    }

    public void ExecuteSelectUnit(InputMessage inputMessage)
    {
        // If the hex selected has units on it display them in the gui
        if ((inputMessage.unit != null) && (inputMessage.unit.GetComponent<UnitDatabaseFields>().occupiedHex != null))
            GlobalDefinitions.GuiDisplayUnitsOnHex(inputMessage.unit.GetComponent<UnitDatabaseFields>().occupiedHex);
    }
}
