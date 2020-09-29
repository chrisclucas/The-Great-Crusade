using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Windows.Forms;
using CommonRoutines;

//[ExecuteInEditMode]

namespace TheGreatCrusade
{
    public class GameControl : MonoBehaviour
    {
        // These are the singletons that will be used to access routines in the different classes
        public static GameObject transportScriptInstance;
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
        public GameObject mapGraphicsInstance;

        // Use this for initialization
        void Start()
        {
            GlobalDefinitions.InitializeFileNames();
            // Set up the log file
            path = System.IO.Directory.GetCurrentDirectory() + "\\";

            // Put the log and command file in a try block since an exception will be thrown if the game was installed in an un-writeable folder
            try
            {

                if (File.Exists(path + GlobalGameFields.logfile))
                    File.Delete(path + GlobalGameFields.logfile);

                using (StreamWriter logFile = File.AppendText(GameControl.path + GlobalGameFields.logfile))
                {
                    logFile.WriteLine("Starting game at: " + DateTime.Now);
                    logFile.WriteLine("GameControl start(): path = " + System.IO.Directory.GetCurrentDirectory() + "\\");
                }

            }
            catch
            {
                MessageBox.Show("ERROR: Cannot access log file - cannot continue");
                GlobalDefinitions.GuiUpdateStatusMessage("Internal Error - Cannot access log file - cannot continue");
            }

            GlobalDefinitions.WriteToLogFile("Game Version " + GlobalDefinitions.releaseVersion);

            // There are three files that should have been installed with the game.  Note, I could get rid of all three of these and just have the
            // board and the units built into the game rather than reading them.  But I haven't done this based on a somewhat vauge idea that this will
            // make future games easier to build.
            // The three files are:
            //      TGCBoardSetup.txt - this has been split into four files, each checked at time of execution 7/25/20
            //      TGCBritainUnitLocation.txt
            //      TGCGermanSetup.txt
            // Check here that the files exist.  If they don't then exit out now


            if (!File.Exists(path + GlobalGameFields.britainUnitLocationFile))
            {
                MessageBox.Show("ERROR: " + GlobalGameFields.britainUnitLocationFile + "  file not found - cannot continue");
                UnityEngine.Application.Quit();
            }
            else
                GlobalGameFields.britainUnitLocationFile = path + GlobalGameFields.britainUnitLocationFile;

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
            GameObject.Find("ChatInputField").GetComponent<InputField>().onEndEdit.AddListener(delegate { GlobalDefinitions.ExecuteChatMessage(); });
            GlobalDefinitions.chatPanel = GameObject.Find("ChatPanel");
            GlobalDefinitions.chatPanel.SetActive(false);

            // Add a canvas to add UI elements (i.e. text) to the board
            GlobalDefinitions.mapText = new GameObject();
            GlobalDefinitions.mapText.name = "Map Text";
            GlobalDefinitions.mapText.transform.SetParent(GameObject.Find("Map Graphics").transform);
            GlobalDefinitions.mapGraphicCanvas = GlobalDefinitions.mapText.AddComponent<Canvas>();
            GlobalDefinitions.mapText.AddComponent<CanvasScaler>();
            GlobalDefinitions.mapGraphicCanvas.renderMode = RenderMode.WorldSpace;
            GlobalDefinitions.mapGraphicCanvas.sortingLayerName = "Text";

            // The first thing that needs to be done is store the locations of the units.  They 
            // are sitting on the order of battle sheet and this will be their "dead" location
            GlobalDefinitions.WriteToLogFile("Setting unit OOB locations");
            foreach (Transform unit in GameObject.Find("Units Eliminated").transform)
                unit.GetComponent<UnitDatabaseFields>().OOBLocation = unit.position;

            GlobalDefinitions.WriteToLogFile("GameControl start(): Creating Singletons");
            // Create singletons of each of the routine classes
            CreateSingletons();

            GlobalDefinitions.WriteToLogFile("GameControl start(): Setting up the map");
            // Set up the map from the read location
            createBoardInstance.GetComponent<CreateBoard>().ReadMapSetup();

            // Load the global for storing all hexes on the board
            //foreach (Transform hex in GameObject.Find("Board").transform)
            //    GlobalDefinitions.allHexesOnBoard.Add(hex.gameObject);

            // Deal with the configuration settings
            GlobalGameFields.settingsFile = path + GlobalGameFields.settingsFile;
            // Check if the setting file is present, if it isn't write out a default
            if (!File.Exists(GlobalGameFields.settingsFile))
            {
                GlobalDefinitions.difficultySetting = 5;
                GlobalDefinitions.aggressiveSetting = 3;
                readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().WriteSettingsFile(5, 3);
            }
            else
            {
                // If the file exists read the configuration settings
                readWriteRoutinesInstance.GetComponent<ReadWriteRoutines>().ReadSettingsFile();
            }
            // Reset the min/max odds since the aggressiveness has just been read
            CombatResolutionRoutines.AdjustAggressiveness();

            AIRoutines.SetIntrinsicHexValues();

            // AI TESTING
            hexValueGuiInstance = new GameObject();
            Canvas hexValueCanvas = hexValueGuiInstance.AddComponent<Canvas>();
            hexValueGuiInstance.AddComponent<CanvasScaler>();
            hexValueCanvas.renderMode = RenderMode.WorldSpace;
            hexValueCanvas.sortingLayerName = "Hex";
            hexValueGuiInstance.name = "hexValueGuiInstance";

            // AI TESTING
            //foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            //    GlobalDefinitions.createHexText(Convert.ToString(hex.GetComponent<HexDatabaseFields>().hexValue), hex.name + "HexValueText", 20, 20, hex.position.x, hex.position.y, 14, hexValueCanvas);

            GlobalDefinitions.WriteToLogFile("GameControl start(): Putting Allied units in Britain - reading from file: " + GlobalGameFields.britainUnitLocationFile);
            // When restarting a game the units won't have their Britain location loaded so this needs to be done before a restart file is read
            createBoardInstance.GetComponent<CreateBoard>().ReadBritainPlacement(GlobalGameFields.britainUnitLocationFile);

            GlobalDefinitions.WriteToLogFile("GameControl start(): Setting up invasion areas");
            createBoardInstance.GetComponent<CreateBoard>().SetupInvasionAreas();

            // Make sure the game doesn't start with selected unit or hex
            GlobalDefinitions.selectedUnit = null;
            GlobalDefinitions.startHex = null;

            // Reset the list of active GUI's
            GlobalDefinitions.guiList.Clear();

            gameStateControlInstance = new GameObject("gameStateControl");
            gameStateControlInstance.AddComponent<GameStateControl>();
            inputMessage = new GameObject("inputMessage");
            inputMessage.AddComponent<InputMessage>();

            GlobalDefinitions.allUnitsOnBoard = GameObject.Find("Units On Board");

            // Turn off the background of the unit display panel
            GameObject.Find("UnitDisplayPanel").GetComponent<CanvasGroup>().alpha = 0;

            // Setup the state for when victory is achieved
            victoryState = new GameObject("victoryState");
            victoryState.AddComponent<VictoryState>();

            // At this point everything has been setup.  Call up GUI to have the user select the type of game being played
            GlobalDefinitions.WriteToLogFile("GameControl start(): calling getGameModeUI()");
            MainMenuRoutines.GetGameModeUI();
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
                                    ((gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                                    (gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanMovementStateInstance") ||
                                    (gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "setUpStateInstance")))
                            {
                                // When we have a double click that means that there was already a single click that would have selected a unit
                                // Unhighlight it and then remove it
                                if (GlobalDefinitions.selectedUnit != null)
                                    GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                                foreach (Transform hex in GameObject.Find("Board").transform)
                                    GlobalDefinitions.UnhighlightHex(hex.gameObject);
                                GlobalDefinitions.selectedUnit = null;

                                GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.SETCAMERAPOSITIONKEYWORD + " " + Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z + " " + Camera.main.GetComponent<Camera>().orthographicSize);

                                // I had a bug where double clicking on an off-board unit causes an exception in the following line because it is assuming a hex is being clicked
                                if (GeneralHexRoutines.GetHexFromUserInput(Input.mousePosition) != null)
                                    GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.MOUSEDOUBLECLICKIONKEYWORD + " " + GeneralHexRoutines.GetHexFromUserInput(Input.mousePosition).name + " " + gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality);

                                movementRoutinesInstance.GetComponent<MovementRoutines>().CallMultiUnitDisplay(GeneralHexRoutines.GetHexFromUserInput(Input.mousePosition),
                                    gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality);
                            }
                            // If not double click then process a normal click
                            else
                            {
                                inputMessage.GetComponent<InputMessage>().hex = GeneralHexRoutines.GetHexFromUserInput(Input.mousePosition);
                                inputMessage.GetComponent<InputMessage>().unit = GeneralHexRoutines.GetUnitWithoutHex(Input.mousePosition);

                                RecordMouseClick(inputMessage.GetComponent<InputMessage>().unit, inputMessage.GetComponent<InputMessage>().hex);

                                gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod(inputMessage.GetComponent<InputMessage>());
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
                    //if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork)
                    //{
                    //    string message;

                    //    NetworkEventType receivedNetworkEvent = TransportScript.checkForNetworkEvent(out message);

                    //    if (receivedNetworkEvent == NetworkEventType.DataEvent)
                    //    {
                    //        // The only message that is valid when in control is a chat message

                    //        char[] delimiterChars = { ' ' };
                    //        string[] switchEntries = message.Split(delimiterChars);

                    //        switch (switchEntries[0])
                    //        {
                    //            case GlobalDefinitions.CHATMESSAGEKEYWORD:
                    //                string chatMessage = "";
                    //                for (int index = 0; index < (switchEntries.Length - 1); index++)
                    //                    chatMessage += switchEntries[index + 1] + " ";
                    //                GlobalDefinitions.WriteToLogFile("Chat message received: " + chatMessage);
                    //                GlobalDefinitions.AddChatMessage(chatMessage);
                    //                break;
                    //            default:
                    //                GlobalDefinitions.WriteToLogFile("ERROR: unexpected data message received when in control (only chat message valid - message = " + message);
                    //                break;
                    //        }
                    //    }
                    //}

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

                //else if (!GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork))
                //{
                //    string message;
                //    NetworkEventType receivedNetworkEvent = TransportScript.checkForNetworkEvent(out message);

                //    if (receivedNetworkEvent == NetworkEventType.DataEvent)
                //        ExecuteGameCommand.ProcessCommand(message);
                //}

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
        public static void RecordMouseClick(GameObject unit, GameObject hex)
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.SETCAMERAPOSITIONKEYWORD + " " + Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z + " " + Camera.main.GetComponent<Camera>().orthographicSize);

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

            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.MOUSESELECTIONKEYWORD + " " + hexName + " " + unitName);
        }

        /// <summary>
        /// This routine sets the game state to the side who is in control
        /// </summary>
        /// <param name="currentSide"></param> this is the side that is passed in the saved game file that should be in control
        public static void SetGameState(string currentSide)
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
                            GlobalDefinitions.WriteToLogFile("setGameState: setting game state to turnInitializationStateInstance");
                            gameStateControlInstance.GetComponent<GameStateControl>().currentState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
                        }
                        else
                        {
                            GlobalDefinitions.WriteToLogFile("setGameState: setting game state to germanAIStateInstance");
                            gameStateControlInstance.GetComponent<GameStateControl>().currentState = germanAIStateInstance.GetComponent<GermanAIState>();
                        }
                    }
                    else
                    {
                        GlobalDefinitions.WriteToLogFile("setGameState: setting game state to germanIsolationStateInstance  turn number = " + GlobalDefinitions.turnNumber);
                        // Check if this is a setup file in order to allow the player to update the setup if he wants to
                        if (GlobalDefinitions.turnNumber == 0)
                        {
                            GlobalDefinitions.WriteToLogFile("setGameState: setting game state to setUpStateInstance");
                            gameStateControlInstance.GetComponent<GameStateControl>().currentState = setUpStateInstance.GetComponent<SetUpState>();
                        }

                        else
                        {
                            GlobalDefinitions.WriteToLogFile("setGameState: setting game state to germanIsolationStateInstance");
                            gameStateControlInstance.GetComponent<GameStateControl>().currentState = germanIsolationStateInstance.GetComponent<GermanIsolationState>();
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
                        GlobalDefinitions.WriteToLogFile("setGameState: setting game state to setUpStateInstance");
                        gameStateControlInstance.GetComponent<GameStateControl>().currentState = setUpStateInstance.GetComponent<SetUpState>();
                    }
                    else
                    {
                        GlobalDefinitions.WriteToLogFile("setGameState: setting game state to germanIsolationStateInstance");
                        gameStateControlInstance.GetComponent<GameStateControl>().currentState = germanIsolationStateInstance.GetComponent<GermanIsolationState>();
                    }
                }
            }
            else
            {
                // The game state is for the Allied player to be in control
                // Note we don't need to check for a setup file here since that would indicate that the German side is in control
                GlobalDefinitions.WriteToLogFile("setGameState: Allied in control, setting game state to turnInitializationStateInstance");
                gameStateControlInstance.GetComponent<GameStateControl>().currentState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
                if (GlobalDefinitions.gameMode != GlobalDefinitions.GameModeValues.AI)
                    // Do not set the currentSidePlaying variable if it is an AI game since it will already have been set during the game selection
                    // This is being set for network play since it has no meaning in hotseat
                    GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.Allied;
            }
        }

        /// <summary>
        /// Creates the singletons for each of the routine classes
        /// </summary>
        private void CreateSingletons()
        {
            //fileTransferServerInstance = new GameObject("fileTransferInstance");
            //fileTransferServerInstance.AddComponent<FileTransferServer>();

            //transportScriptInstance = new GameObject("transportScriptInstance");
            //transportScriptInstance.AddComponent<TransportScript>();

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
        }

        /// <summary>
        /// Creates the state instances and sets up the state transitions for network or hotseat games
        /// </summary>
        public static void CreateStatesForHotSeatOrNetwork()
        {
            GlobalDefinitions.WriteToLogFile("createStatesForHotSeatOrNetwork: executing");

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
        public static void CreateStatesForAI(GlobalDefinitions.Nationality nationalityBeingPlayed)
        {
            GlobalDefinitions.WriteToLogFile("createStatesForAI: executing");
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

}