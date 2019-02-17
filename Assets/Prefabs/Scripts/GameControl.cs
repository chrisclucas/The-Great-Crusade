﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Windows.Forms;

//[ExecuteInEditMode]
public class GameControl : MonoBehaviour
{
    // These are the singletons that will be used to access routines in the different classes
    public static GameObject transpostScriptInstance;
    public static GameObject createBoardInstance;
    public static GameObject setupRoutinesInstance;
    public static GameObject supplyRoutinesInstance;
    public static GameObject movementRoutinesInstance;
    public static GameObject combatRoutinesInstance;
    public static GameObject invasionRoutinesInstance;
    public static GameObject readWriteRoutinesInstance;
    public static GameObject GUIButtonRoutinesInstance;
    public static GameObject AIRoutinesInstance;
    public static GameObject clientServerRoutinesInstance;
    public static GameObject serverRoutinesInstance;

    // These are the objects that contain the different game states
    public static GameObject gameStateControlInstance;
    public static GameObject setUpStateInstance;
    public static GameObject turnInitializationStateInstance;
    public static GameObject alliedReplacementStateInstance;
    public static GameObject alliedSupplyStateInstance;
    public static GameObject alliedInvasionStateInstance;
    public static GameObject alliedAirborneStateInstance;
    public static GameObject alliedMovementStateInstance;
    public static GameObject alliedCombatStateInstance;
    public static GameObject alliedTacticalAirStateInstance;
    public static GameObject alliedAITacticalAirStateInstance;
    public static GameObject germanIsolationStateInstance;
    public static GameObject germanReplacementStateInstance;
    public static GameObject germanMovementStateInstance;
    public static GameObject germanCombatStateInstance;
    public static GameObject germanAISetupStateInstance;
    public static GameObject germanAIStateInstance;
    public static GameObject alliedAIStateInstance;
    public static GameObject victoryState;

    public static GameObject inputMessage;

    public static string path;

    public static GameObject fileTransferServerInstance;

    public GameObject hexValueGuiInstance;

    // Use this for initialization
    void Start()
    {
        // Set up the log file
        path = System.IO.Directory.GetCurrentDirectory() + "\\";

        // Put the log and command file in a try block since an exception will be thrown if the game was installed in an un-writeable folder
        try
        {

            if (File.Exists(path + GlobalDefinitions.logfile))
                File.Delete(path + GlobalDefinitions.logfile);

            using (StreamWriter logFile = File.AppendText(GameControl.path + GlobalDefinitions.logfile))
            {
                logFile.WriteLine("Starting game at: " + DateTime.Now);
                logFile.WriteLine("GameControl start(): path = " + System.IO.Directory.GetCurrentDirectory() + "\\");
            }

        }
        catch
        {
            MessageBox.Show("ERROR: Cannot access log file - cannot continue");
            GlobalDefinitions.guiUpdateStatusMessage("Internal Error - Cannot access log file - cannot continue");
        }

        GlobalDefinitions.writeToLogFile("Game Version " + GlobalDefinitions.releaseVersion);

        // There are three files that should have been installed with the game.  Note, I could get rid of all three of these and just have the
        // board and the units built into the game rather than reading them.  But I haven't done this based on a somewhat vauge idea that this will
        // make future games easier to build.
        // The three files are:
        //      TGCBoardSetup.txt
        //      TGCBritainUnitLocation.txt
        //      TGCGermanSetup.txt
        // Check here that the files exist.  If they don't then exit out now

        if (!File.Exists(path + GlobalDefinitions.boardsetupfile))
        {
            MessageBox.Show("ERROR: " + GlobalDefinitions.boardsetupfile + " file not found - cannot continue");
            UnityEngine.Application.Quit();
        }
        else
            GlobalDefinitions.boardsetupfile = path + GlobalDefinitions.boardsetupfile;

        if (!File.Exists(path + GlobalDefinitions.britainunitlocationfile))
        {
            MessageBox.Show("ERROR: " + GlobalDefinitions.britainunitlocationfile + "  file not found - cannot continue");
            UnityEngine.Application.Quit();
        }
        else
            GlobalDefinitions.britainunitlocationfile = path + GlobalDefinitions.britainunitlocationfile;

        //if (!File.Exists(path + "TGCGermanSetup.txt"))
        if (!File.Exists(path + "GermanSetup//TGCGermanSetup1.txt"))
        {
            MessageBox.Show("ERROR: TGCGermanSetup1.txt file not found - cannot continue");
            UnityEngine.Application.Quit();
        }

        GlobalDefinitions.nextPhaseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AssignCombatButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.DisplayAllCombatsButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = false;
        GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<UnityEngine.UI.Button>().interactable = false;

        // Hide the chat screen.  We will turn it back on if the user selects a network game
        GameObject.Find("ChatInputField").GetComponent<InputField>().onEndEdit.AddListener(delegate { GlobalDefinitions.executeChatMessage(); });
        GlobalDefinitions.chatPanel = GameObject.Find("ChatPanel");
        GlobalDefinitions.chatPanel.SetActive(false);

        // The first thing that needs to be done is store the locations of the units.  They 
        // are sitting on the order of battle sheet and this will be their "dead" location
        GlobalDefinitions.writeToLogFile("Setting unit OOB locations");
        foreach (Transform unit in GameObject.Find("Units Eliminated").transform)
            unit.GetComponent<UnitDatabaseFields>().OOBLocation = unit.position;

        GlobalDefinitions.writeToLogFile("GameControl start(): Creating Singletons");
        // Create singletons of each of the routine classes
        createSingletons();

        GlobalDefinitions.writeToLogFile("GameControl start(): Setting up the map - " + path + "TGCBoardSetup.txt");
        // Set up the map from the read location
        createBoardInstance.GetComponent<CreateBoard>().ReadMapSetup(path + "TGCBoardSetup.txt");

        // Load the global for storing all hexes on the board
        //foreach (Transform hex in GameObject.Find("Board").transform)
        //    GlobalDefinitions.allHexesOnBoard.Add(hex.gameObject);

        // Deal with the configuration settings
        GlobalDefinitions.settingsFile = path + GlobalDefinitions.settingsFile;
        // Check if the setting file is present, if it isn't write out a default
        if (!File.Exists(GlobalDefinitions.settingsFile))
        {
            GlobalDefinitions.difficultySetting = 5;
            GlobalDefinitions.aggressiveSetting = 3;
            readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().writeSettingsFile(5, 3);
        }
        else
        {
            // If the file exists read the configuration settings
            readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().readSettingsFile();
        }
        // Reset the min/max odds since the aggressiveness has just been read
        CombatResolutionRoutines.adjustAggressiveness();

        AIRoutines.setIntrinsicHexValues();

        // AI TESTING
        //hexValueGuiInstance = new GameObject();
        //Canvas hexValueCanvas = hexValueGuiInstance.AddComponent<Canvas>();
        //hexValueGuiInstance.AddComponent<CanvasScaler>();
        //hexValueCanvas.renderMode = RenderMode.WorldSpace;
        //hexValueGuiInstance.name = "hexValueGuiInstance";
        //hexValueCanvas.sortingLayerName = "Highlight";

        // AI TESTING
        //foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        //    GlobalDefinitions.createHexValueText(Convert.ToString(hex.GetComponent<HexDatabaseFields>().hexValue), hex.name + "HexValueText", 20, 20, hex.position.x, hex.position.y, hexValueCanvas);

        GlobalDefinitions.writeToLogFile("GameControl start(): Putting Allied units in Britain - reading from file: " + path + "TGCBritainUnitLocation.txt");
        // When restarting a game the units won't have their Britain location loaded so this needs to be done before a restart file is read
        createBoardInstance.GetComponent<CreateBoard>().readBritainPlacement(path + "TGCBritainUnitLocation.txt");

        GlobalDefinitions.writeToLogFile("GameControl start(): Setting up invasion areas");
        createBoardInstance.GetComponent<CreateBoard>().setupInvasionAreas();

        // Make sure the game doesn't start with selected unit or hex
        GlobalDefinitions.selectedUnit = null;
        GlobalDefinitions.startHex = null;

        // Reset the list of active GUI's
        GlobalDefinitions.guiList.Clear();

        gameStateControlInstance = new GameObject("gameStateControl");
        gameStateControlInstance.AddComponent<gameStateControl>();
        inputMessage = new GameObject("inputMessage");
        inputMessage.AddComponent<InputMessage>();

        GlobalDefinitions.allUnitsOnBoard = GameObject.Find("Units On Board");

        // Turn off the background of the unit display panel
        GameObject.Find("UnitDisplayPanel").GetComponent<CanvasGroup>().alpha = 0;

        // Setup the state for when victory is achieved
        victoryState = new GameObject("victoryState");
        victoryState.AddComponent<VictoryState>();

        // At this point everything has been setup.  Call up GUI to have the user select the type of game being played
        GlobalDefinitions.writeToLogFile("GameControl start(): calling getGameModeUI()");
        MainMenuRoutines.getGameModeUI();
    }

    private float initialTouch; // Used to check if the mouse click is a double click
    void Update()
    {
        if (GlobalDefinitions.gameStarted)
        {
            if (GlobalDefinitions.localControl || (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Hotseat))
            {
                if (!GlobalDefinitions.commandFileBeingRead)
                {
                    // Left mouse button click
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // Check if the user double clicked
                        if ((Time.time < initialTouch + 0.5f) &&
                                ((gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                                (gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanMovementStateInstance") ||
                                (gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "setUpStateInstance")))
                        {
                            // When we have a double click that means that there was already a single click that would have selected a unit
                            // Unhighlight it and then remove it
                            if (GlobalDefinitions.selectedUnit != null)
                                GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                            foreach (Transform hex in GameObject.Find("Board").transform)
                                GlobalDefinitions.unhighlightHex(hex.gameObject);
                            GlobalDefinitions.selectedUnit = null;

                            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.SETCAMERAPOSITIONKEYWORD + " " + Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z + " " + Camera.main.GetComponent<Camera>().orthographicSize);

                            // I had a bug where double clicking on an off-board unit causes an exception in the following line because it is assuming a hex is being clicked
                            if (GlobalDefinitions.getHexFromUserInput(Input.mousePosition) != null)
                                GlobalDefinitions.writeToCommandFile(GlobalDefinitions.MOUSEDOUBLECLICKIONKEYWORD + " " + GlobalDefinitions.getHexFromUserInput(Input.mousePosition).name + " " + gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality);

                            movementRoutinesInstance.GetComponent<MovementRoutines>().callMultiUnitDisplay(GlobalDefinitions.getHexFromUserInput(Input.mousePosition),
                                gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality);
                        }
                        // If not double click then process a normal click
                        else
                        {
                            inputMessage.GetComponent<InputMessage>().hex = GlobalDefinitions.getHexFromUserInput(Input.mousePosition);
                            inputMessage.GetComponent<InputMessage>().unit = GlobalDefinitions.getUnitWithoutHex(Input.mousePosition);

                            recordMouseClick(inputMessage.GetComponent<InputMessage>().unit, inputMessage.GetComponent<InputMessage>().hex);

                            gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(inputMessage.GetComponent<InputMessage>());

                        }

                        initialTouch = Time.time;
                    }
                }

                // Note that the EventSystem check is to ensure the mouse isn't clicking a ui button
                //else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                //{
                //    // This is a left mouse button click to select a unit or hex
                //    inputMessage.GetComponent<InputMessage>().hex = GlobalDefinitions.getHexFromUserInput(Input.mousePosition);
                //    inputMessage.GetComponent<InputMessage>().unit = GlobalDefinitions.getUnitWithoutHex(Input.mousePosition);

                //    gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(inputMessage.GetComponent<InputMessage>());
                //    if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
                //        sendMouseClickToNetwork(inputMessage.GetComponent<InputMessage>().unit, inputMessage.GetComponent<InputMessage>().hex);
                //}

                // Even though this is for when the player is in control, still need to check for chat messages
                if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork)
                {
                    NetworkEventType recNetworkEvent = NetworkTransport.Receive(out TransportScript.recHostId, out TransportScript.recConnectionId, out TransportScript.recChannelId, TransportScript.recBuffer, TransportScript.BUFFERSIZE, out TransportScript.dataSize, out TransportScript.recError);

                    switch (recNetworkEvent)
                    {
                        case NetworkEventType.DataEvent:
                            Stream stream = new MemoryStream(TransportScript.recBuffer);
                            BinaryFormatter formatter = new BinaryFormatter();
                            string message = formatter.Deserialize(stream) as string;
                            TransportScript.OnData(TransportScript.recHostId, TransportScript.recConnectionId, TransportScript.recChannelId, message, TransportScript.dataSize, (NetworkError)TransportScript.recError);

                            // The only message that is valid when in control is a chat message

                            char[] delimiterChars = { ' ' };
                            string[] switchEntries = message.Split(delimiterChars);

                            switch (switchEntries[0])
                            {
                                case GlobalDefinitions.CHATMESSAGEKEYWORD:
                                    string chatMessage = "";
                                    for (int index = 0; index < (switchEntries.Length - 1); index++)
                                        chatMessage += switchEntries[index + 1] + " ";
                                    GlobalDefinitions.writeToLogFile("Chat message received: " + chatMessage);
                                    GlobalDefinitions.addChatMessage(chatMessage);
                                    break;
                            }
                            break;
                    }
                }

                // Since I have enabled chat I have to do something to get hotkeys since chat will execute the hotkeys
                //else if (Input.GetKeyDown(KeyCode.R))
                //{
                //    GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().executeCombatResolution();
                //}

                //else if (Input.GetKeyDown(KeyCode.Q))
                //{
                //    GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().goToNextPhase();
                //}

                //else if (Input.GetKeyDown(KeyCode.U))
                //{
                //    GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().executeUndo();
                //}
            }

            else if (!GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork))
            {
                NetworkEventType recNetworkEvent = NetworkTransport.Receive(out TransportScript.recHostId, out TransportScript.recConnectionId, out TransportScript.recChannelId, TransportScript.recBuffer, TransportScript.BUFFERSIZE, out TransportScript.dataSize, out TransportScript.recError);

                switch (recNetworkEvent)
                {
                    case NetworkEventType.DisconnectEvent:
                        GlobalDefinitions.writeToLogFile("GameControl udpate() OnDisconnect: (hostId = " + TransportScript.recHostId + ", connectionId = "
                                + TransportScript.recConnectionId + ", error = " + TransportScript.recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                        GlobalDefinitions.guiUpdateStatusMessage("Disconnect event received from remote computer - resetting connection");
                        TransportScript.resetConnection(TransportScript.recHostId);

                        // Since the connetion has been broken, quit the game and go back to the main menu
                        GameObject guiButtonInstance = new GameObject("GUIButtonInstance");
                        guiButtonInstance.AddComponent<GUIButtonRoutines>();
                        guiButtonInstance.GetComponent<GUIButtonRoutines>().yesMain();
                        break;
                    case NetworkEventType.DataEvent:
                        Stream stream = new MemoryStream(TransportScript.recBuffer);
                        BinaryFormatter formatter = new BinaryFormatter();
                        string message = formatter.Deserialize(stream) as string;
                        TransportScript.OnData(TransportScript.recHostId, TransportScript.recConnectionId, TransportScript.recChannelId, message, TransportScript.dataSize, (NetworkError)TransportScript.recError);
                        ExecuteGameCommand.processCommand(message);
                        break;
                    case NetworkEventType.Nothing:
                        break;
                    case NetworkEventType.ConnectEvent:
                        {
                            GlobalDefinitions.writeToLogFile("TransportScript.OnConnect: (hostId = " + TransportScript.recHostId + ", connectionId = " + TransportScript.recConnectionId + ", error = " + TransportScript.recError.ToString() + ")" + "  " + DateTime.Now.ToString("h:mm:ss tt"));
                            break;
                        }
                    default:
                        GlobalDefinitions.writeToLogFile("GameControl Update(): Unknown network message type received: " + recNetworkEvent);
                        break;
                }
            }

            else if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
            {
                // The user side is controled by the hotseat section above.  The AI doesn't need anything during update since its states don't have input or transitions.
            }

            else if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.ClientServerNetwork)
            {

            }
        }
    }

    /// <summary>
    /// This routine sends a command for a mouse click to the opponent's computer
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="hex"></param>
    public static void recordMouseClick(GameObject unit, GameObject hex)
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.SETCAMERAPOSITIONKEYWORD + " " + Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z + " " + Camera.main.GetComponent<Camera>().orthographicSize);

        string hexName;
        string unitName;
        if (hex != null)
        {
            hexName = hex.name;
        }
        else
            hexName = "null";

        if (unit != null)
        {
            unitName = unit.name;
        }
        else
            unitName = "null";

        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.MOUSESELECTIONKEYWORD + " " + hexName + " " + unitName);
    }

    /// <summary>
    /// This routine sets the game state to the side who is in control
    /// </summary>
    /// <param name="currentSide"></param> this is the side that is passed in the saved game file that should be in control
    public static void setGameState(string currentSide)
    {
        if (Convert.ToString(currentSide) == "German")
        {
            // This executes when the German side is in control after the saved file is read in
            if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
            {
                if (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.Nationality.Allied)
                {
                    // Check if this is a setup file that was read in.  The control will be for the German to play for when the player is
                    // playing the German side in order to give him a chance to update the setup.  But if the AI is playing the German then
                    // just go to the Allied invasion state
                    if (GlobalDefinitions.turnNumber == 0)
                    {
                        GlobalDefinitions.writeToLogFile("setGameState: setting game state to turnInitializationStateInstance");
                        gameStateControlInstance.GetComponent<gameStateControl>().currentState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
                    }
                    else
                    {
                        GlobalDefinitions.writeToLogFile("setGameState: setting game state to germanAIStateInstance");
                        gameStateControlInstance.GetComponent<gameStateControl>().currentState = germanAIStateInstance.GetComponent<GermanAIState>();
                    }
                }
                else
                {
                    GlobalDefinitions.writeToLogFile("setGameState: setting game state to germanIsolationStateInstance  turn number = " + GlobalDefinitions.turnNumber);
                    // Check if this is a setup file in order to allow the player to update the setup if he wants to
                    if (GlobalDefinitions.turnNumber == 0)
                    {
                        GlobalDefinitions.writeToLogFile("setGameState: setting game state to setUpStateInstance");
                        gameStateControlInstance.GetComponent<gameStateControl>().currentState = setUpStateInstance.GetComponent<SetUpState>();
                    }

                    else
                    {
                        GlobalDefinitions.writeToLogFile("setGameState: setting game state to germanIsolationStateInstance");
                        gameStateControlInstance.GetComponent<gameStateControl>().currentState = germanIsolationStateInstance.GetComponent<GermanIsolationState>();
                    }
                }
            }
            else
            {
                // Do not set the currentSidePlaying variable if it is an AI game since it will already have been set during the game selection
                // This is being set for network play since it has no meaning in hotseat
                GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.German;
                // Check if this is a setup file in order to allow the player to update the setup if he wants to
                if (GlobalDefinitions.turnNumber == 0)
                {
                    GlobalDefinitions.writeToLogFile("setGameState: setting game state to setUpStateInstance");
                    gameStateControlInstance.GetComponent<gameStateControl>().currentState = setUpStateInstance.GetComponent<SetUpState>();
                }
                else
                {
                    GlobalDefinitions.writeToLogFile("setGameState: setting game state to germanIsolationStateInstance");
                    gameStateControlInstance.GetComponent<gameStateControl>().currentState = germanIsolationStateInstance.GetComponent<GermanIsolationState>();
                }
            }
        }
        else
        {
            // The game state is for the Allied player to be in control
            // Note we don't need to check for a setup file here since that would indicate that the German side is in control
            GlobalDefinitions.writeToLogFile("setGameState: Allied in control, setting game state to turnInitializationStateInstance");
            gameStateControlInstance.GetComponent<gameStateControl>().currentState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
            if (GlobalDefinitions.gameMode != GlobalDefinitions.GameModeValues.AI)
                // Do not set the currentSidePlaying variable if it is an AI game since it will already have been set during the game selection
                // This is being set for network play since it has no meaning in hotseat
                GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.Allied;
        }
    }

    /// <summary>
    /// Creates the singletons for each of the routine classes
    /// </summary>
    private void createSingletons()
    {
        fileTransferServerInstance = new GameObject("fileTransferServerInstance");
        fileTransferServerInstance.AddComponent<FileTransferServer>();

        transpostScriptInstance = new GameObject("transportScriptInstance");
        transpostScriptInstance.AddComponent<TransportScript>();

        setupRoutinesInstance = new GameObject("setupRoutinesInstance");
        setupRoutinesInstance.AddComponent<SetupRoutines>();

        movementRoutinesInstance = new GameObject("movementRoutinesInstance");
        movementRoutinesInstance.AddComponent<MovementRoutines>();

        supplyRoutinesInstance = new GameObject("supplyRoutinesInstance");
        supplyRoutinesInstance.AddComponent<SupplyRoutines>();

        combatRoutinesInstance = new GameObject("combatRoutinesInstance");
        combatRoutinesInstance.AddComponent<CombatRoutines>();

        invasionRoutinesInstance = new GameObject("invasionRoutinesInstance");
        invasionRoutinesInstance.AddComponent<InvasionRoutines>();

        createBoardInstance = new GameObject("createBoardInstance");
        createBoardInstance.AddComponent<CreateBoard>();

        readWriteRoutinesInstance = new GameObject("readWriteRoutinesInstance");
        readWriteRoutinesInstance.AddComponent<ReadWriteRoutines>();

        GUIButtonRoutinesInstance = new GameObject("GUIButtonRoutinesInstance");
        GUIButtonRoutinesInstance.AddComponent<GUIButtonRoutines>();

        AIRoutinesInstance = new GameObject("AIRoutinesInstance");
        AIRoutinesInstance.AddComponent<AIRoutines>();

        clientServerRoutinesInstance = new GameObject("ClientServerRoutinesInstance");
        clientServerRoutinesInstance.AddComponent<ClientServerRoutines>();

        serverRoutinesInstance = new GameObject("ServerRoutinesInstance");
        serverRoutinesInstance.AddComponent<ServerRoutines>();
    }

    /// <summary>
    /// Creates the state instances and sets up the state transitions for network or hotseat games
    /// </summary>
    public static void createStatesForHotSeatOrNetwork()
    {
        GlobalDefinitions.writeToLogFile("createStatesForHotSeatOrNetwork: executing");

        setUpStateInstance = new GameObject("setUpStateInstance");
        turnInitializationStateInstance = new GameObject("turnInitializationStateInstance");
        alliedReplacementStateInstance = new GameObject("alliedReplacementStateInstance");
        alliedSupplyStateInstance = new GameObject("alliedSupplyStateInstance");
        alliedInvasionStateInstance = new GameObject("alliedInvasionStateInstance");
        alliedAirborneStateInstance = new GameObject("alliedAirborneStateInstance");
        alliedMovementStateInstance = new GameObject("alliedMovementStateInstance");
        alliedCombatStateInstance = new GameObject("alliedCombatStateInstance");
        alliedTacticalAirStateInstance = new GameObject("alliedTacticalAirStateInstance");
        germanIsolationStateInstance = new GameObject("germanIsolationStateInstance");
        germanReplacementStateInstance = new GameObject("germanReplacementStateInstance");
        germanMovementStateInstance = new GameObject("germanMovementStateInstance");
        germanCombatStateInstance = new GameObject("germanCombatStateInstance");

        setUpStateInstance.AddComponent<SetUpState>();
        turnInitializationStateInstance.AddComponent<TurnInitializationState>();
        alliedReplacementStateInstance.AddComponent<AlliedReplacementState>();
        alliedSupplyStateInstance.AddComponent<SupplyState>();
        alliedInvasionStateInstance.AddComponent<AlliedInvasionState>();
        alliedAirborneStateInstance.AddComponent<AlliedAirborneState>();
        alliedMovementStateInstance.AddComponent<MovementState>();
        alliedCombatStateInstance.AddComponent<CombatState>();
        alliedTacticalAirStateInstance.AddComponent<AlliedTacticalAirState>();
        germanIsolationStateInstance.AddComponent<GermanIsolationState>();
        germanReplacementStateInstance.AddComponent<GermanReplacementState>();
        germanMovementStateInstance.AddComponent<MovementState>();
        germanCombatStateInstance.AddComponent<CombatState>();

        // Set up the state transitions
        setUpStateInstance.GetComponent<SetUpState>().nextGameState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
        turnInitializationStateInstance.GetComponent<TurnInitializationState>().nextGameState = alliedReplacementStateInstance.GetComponent<AlliedReplacementState>();
        alliedReplacementStateInstance.GetComponent<AlliedReplacementState>().nextGameState = alliedSupplyStateInstance.GetComponent<SupplyState>();
        alliedSupplyStateInstance.GetComponent<SupplyState>().nextGameState = alliedInvasionStateInstance.GetComponent<AlliedInvasionState>();
        alliedInvasionStateInstance.GetComponent<AlliedInvasionState>().nextGameState = alliedAirborneStateInstance.GetComponent<AlliedAirborneState>();
        alliedAirborneStateInstance.GetComponent<AlliedAirborneState>().nextGameState = alliedMovementStateInstance.GetComponent<MovementState>();
        alliedMovementStateInstance.GetComponent<MovementState>().nextGameState = alliedCombatStateInstance.GetComponent<CombatState>();
        alliedCombatStateInstance.GetComponent<CombatState>().nextGameState = alliedTacticalAirStateInstance.GetComponent<AlliedTacticalAirState>();
        alliedTacticalAirStateInstance.GetComponent<AlliedTacticalAirState>().nextGameState = germanIsolationStateInstance.GetComponent<GermanIsolationState>();
        germanIsolationStateInstance.GetComponent<GermanIsolationState>().nextGameState = germanReplacementStateInstance.GetComponent<GermanReplacementState>();
        germanReplacementStateInstance.GetComponent<GermanReplacementState>().nextGameState = germanMovementStateInstance.GetComponent<MovementState>();
        germanMovementStateInstance.GetComponent<MovementState>().nextGameState = germanCombatStateInstance.GetComponent<CombatState>();
        germanCombatStateInstance.GetComponent<CombatState>().nextGameState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();

        // Set up the correct nationality for the game states that share
        turnInitializationStateInstance.GetComponent<TurnInitializationState>().currentNationality = GlobalDefinitions.Nationality.Allied;
        germanIsolationStateInstance.GetComponent<GermanIsolationState>().currentNationality = GlobalDefinitions.Nationality.German;
        alliedMovementStateInstance.GetComponent<MovementState>().currentNationality = GlobalDefinitions.Nationality.Allied;
        alliedCombatStateInstance.GetComponent<CombatState>().currentNationality = GlobalDefinitions.Nationality.Allied;
        germanMovementStateInstance.GetComponent<MovementState>().currentNationality = GlobalDefinitions.Nationality.German;
        germanCombatStateInstance.GetComponent<CombatState>().currentNationality = GlobalDefinitions.Nationality.German;
    }

    /// <summary>
    /// Creates the state instances and sets up the state transitions for AI games
    /// </summary>
    /// <param name="nationalityBeingPlayed"></param>
    public static void createStatesForAI(GlobalDefinitions.Nationality nationalityBeingPlayed)
    {
        GlobalDefinitions.writeToLogFile("createStatesForAI: executing");
        // The AI is playing the German side
        if (nationalityBeingPlayed == GlobalDefinitions.Nationality.Allied)
        {
            setUpStateInstance = new GameObject("setUpStateInstance");
            germanAIStateInstance = new GameObject("GermanAIStateInstance");
            turnInitializationStateInstance = new GameObject("turnInitializationStateInstance");
            alliedReplacementStateInstance = new GameObject("alliedReplacementStateInstance");
            alliedSupplyStateInstance = new GameObject("alliedSupplyStateInstance");
            alliedInvasionStateInstance = new GameObject("alliedInvasionStateInstance");
            alliedAirborneStateInstance = new GameObject("alliedAirborneStateInstance");
            alliedMovementStateInstance = new GameObject("alliedMovementStateInstance");
            alliedCombatStateInstance = new GameObject("alliedCombatStateInstance");
            alliedTacticalAirStateInstance = new GameObject("alliedTacticalAirStateInstance");
            germanAISetupStateInstance = new GameObject("germanAIStateSetupInstance");

            setUpStateInstance.AddComponent<SetUpState>();
            germanAISetupStateInstance.AddComponent<GermanAISetupState>();
            turnInitializationStateInstance.AddComponent<TurnInitializationState>();
            alliedReplacementStateInstance.AddComponent<AlliedReplacementState>();
            alliedSupplyStateInstance.AddComponent<SupplyState>();
            alliedInvasionStateInstance.AddComponent<AlliedInvasionState>();
            alliedAirborneStateInstance.AddComponent<AlliedAirborneState>();
            alliedMovementStateInstance.AddComponent<MovementState>();
            alliedCombatStateInstance.AddComponent<CombatState>();
            alliedTacticalAirStateInstance.AddComponent<AlliedTacticalAirState>();
            germanAIStateInstance.AddComponent<GermanAIState>();

            // AI TESTING
            germanCombatStateInstance = new GameObject("germanCombatStateInstance");
            germanCombatStateInstance.AddComponent<CombatState>();
            germanCombatStateInstance.GetComponent<CombatState>().nextGameState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();

            // Set up the state transitions
            setUpStateInstance.GetComponent<SetUpState>().nextGameState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
            germanAISetupStateInstance.GetComponent<GermanAISetupState>().nextGameState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
            turnInitializationStateInstance.GetComponent<TurnInitializationState>().nextGameState = alliedReplacementStateInstance.GetComponent<AlliedReplacementState>();
            alliedReplacementStateInstance.GetComponent<AlliedReplacementState>().nextGameState = alliedSupplyStateInstance.GetComponent<SupplyState>();
            alliedSupplyStateInstance.GetComponent<SupplyState>().nextGameState = alliedInvasionStateInstance.GetComponent<AlliedInvasionState>();
            alliedInvasionStateInstance.GetComponent<AlliedInvasionState>().nextGameState = alliedAirborneStateInstance.GetComponent<AlliedAirborneState>();
            alliedAirborneStateInstance.GetComponent<AlliedAirborneState>().nextGameState = alliedMovementStateInstance.GetComponent<MovementState>();
            alliedMovementStateInstance.GetComponent<MovementState>().nextGameState = alliedCombatStateInstance.GetComponent<CombatState>();
            alliedCombatStateInstance.GetComponent<CombatState>().nextGameState = alliedTacticalAirStateInstance.GetComponent<AlliedTacticalAirState>();
            alliedTacticalAirStateInstance.GetComponent<AlliedTacticalAirState>().nextGameState = germanAIStateInstance.GetComponent<GermanAIState>();
            germanAIStateInstance.GetComponent<GermanAIState>().nextGameState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();

            // Set up the correct nationality for the game states that share
            turnInitializationStateInstance.GetComponent<TurnInitializationState>().currentNationality = GlobalDefinitions.Nationality.Allied;
            alliedMovementStateInstance.GetComponent<MovementState>().currentNationality = GlobalDefinitions.Nationality.Allied;
            alliedCombatStateInstance.GetComponent<CombatState>().currentNationality = GlobalDefinitions.Nationality.Allied;
        }

        // The AI is playing the Allied side
        else
        {
            setUpStateInstance = new GameObject("setUpStateInstance");
            turnInitializationStateInstance = new GameObject("turnInitializationStateInstance");
            alliedAIStateInstance = new GameObject("alliedAIStateInstance");
            alliedCombatStateInstance = new GameObject("alliedCombatStateInstance");
            alliedAITacticalAirStateInstance = new GameObject("alliedAITacticalAirStateInstance");
            germanIsolationStateInstance = new GameObject("germanIsolationStateInstance");
            germanReplacementStateInstance = new GameObject("germanReplacementStateInstance");
            germanMovementStateInstance = new GameObject("germanMovementStateInstance");
            germanCombatStateInstance = new GameObject("germanCombatStateInstance");

            setUpStateInstance.AddComponent<SetUpState>();
            turnInitializationStateInstance.AddComponent<TurnInitializationState>();
            alliedAIStateInstance.AddComponent<AlliedAIState>();
            alliedCombatStateInstance.AddComponent<CombatState>();
            alliedAITacticalAirStateInstance.AddComponent<AlliedAITacticalAirState>();
            germanIsolationStateInstance.AddComponent<GermanIsolationState>();
            germanReplacementStateInstance.AddComponent<GermanReplacementState>();
            germanMovementStateInstance.AddComponent<MovementState>();
            germanCombatStateInstance.AddComponent<CombatState>();

            setUpStateInstance.GetComponent<SetUpState>().nextGameState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
            turnInitializationStateInstance.GetComponent<TurnInitializationState>().nextGameState = alliedAIStateInstance.GetComponent<AlliedAIState>();
            alliedAIStateInstance.GetComponent<AlliedAIState>().nextGameState = alliedCombatStateInstance.GetComponent<CombatState>();
            alliedCombatStateInstance.GetComponent<CombatState>().nextGameState = alliedAITacticalAirStateInstance.GetComponent<AlliedAITacticalAirState>();
            alliedAITacticalAirStateInstance.GetComponent<AlliedAITacticalAirState>().nextGameState = germanIsolationStateInstance.GetComponent<GermanIsolationState>();
            germanIsolationStateInstance.GetComponent<GermanIsolationState>().nextGameState = germanReplacementStateInstance.GetComponent<GermanReplacementState>();
            germanReplacementStateInstance.GetComponent<GermanReplacementState>().nextGameState = germanMovementStateInstance.GetComponent<MovementState>();
            germanMovementStateInstance.GetComponent<MovementState>().nextGameState = germanCombatStateInstance.GetComponent<CombatState>();
            germanCombatStateInstance.GetComponent<CombatState>().nextGameState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();

            // Set up the correct nationality for the game states that share
            turnInitializationStateInstance.GetComponent<TurnInitializationState>().currentNationality = GlobalDefinitions.Nationality.Allied;
            germanIsolationStateInstance.GetComponent<GermanIsolationState>().currentNationality = GlobalDefinitions.Nationality.German;
            //alliedMovementStateInstance.GetComponent<MovementState>().currentNationality = GlobalDefinitions.Nationality.Allied;
            alliedCombatStateInstance.GetComponent<CombatState>().currentNationality = GlobalDefinitions.Nationality.Allied;
            germanMovementStateInstance.GetComponent<MovementState>().currentNationality = GlobalDefinitions.Nationality.German;
            germanCombatStateInstance.GetComponent<CombatState>().currentNationality = GlobalDefinitions.Nationality.German;
        }
    }
}

