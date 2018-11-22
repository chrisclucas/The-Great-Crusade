using System.IO;
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

        // Put the log file in a try block since an exception will be thrown if the game was installed in an un-writeable folder
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
            GlobalDefinitions.guiUpdateStatusMessage("ERROR: Cannot access log file - cannot continue");
        }

        GlobalDefinitions.writeToLogFile ("Game Version " + GlobalDefinitions.releaseVersion);

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
            MessageBox.Show("ERROR: TGCBoardSetup.txt file not found - cannot continue");
            UnityEngine.Application.Quit();
        }
        else
            GlobalDefinitions.boardsetupfile = path + GlobalDefinitions.boardsetupfile;

        if (!File.Exists(path + GlobalDefinitions.britainunitlocationfile))
        {
            MessageBox.Show("ERROR: TGCBritainUnitLocation.txt file not found - cannot continue");
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

                        sendMouseDoubleClickToNetwork(GlobalDefinitions.getHexFromUserInput(Input.mousePosition),
                            gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality);

                        movementRoutinesInstance.GetComponent<MovementRoutines>().callMultiUnitDisplay(GlobalDefinitions.getHexFromUserInput(Input.mousePosition),
                            gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality);
                    }
                    // If not double click then process a normal click
                    else
                    {
                        inputMessage.GetComponent<InputMessage>().hex = GlobalDefinitions.getHexFromUserInput(Input.mousePosition);
                        inputMessage.GetComponent<InputMessage>().unit = GlobalDefinitions.getUnitWithoutHex(Input.mousePosition);

                        // If a network game send the mouse click to the opponent's computer
                        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
                            sendMouseClickToNetwork(inputMessage.GetComponent<InputMessage>().unit, inputMessage.GetComponent<InputMessage>().hex);

                        gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(inputMessage.GetComponent<InputMessage>());

                    }

                    initialTouch = Time.time;
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
                if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network)
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

            else if (!GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
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
                        processNetworkMessage(message);
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
                // The user side is controled by the hotseat section above.  The AI doesn't need anything during update since it states don't have input or transitions.
            }

            else if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.EMail)
            {

            }
        }
    }

    /// <summary>
    /// This routine is what processes the message received from the opponent computer
    /// </summary>
    /// <param name="message"></param>
    public void processNetworkMessage(string message)
    {
        char[] delimiterChars = { ' ' };
        string[] switchEntries = message.Split(delimiterChars);

        string[] lineEntries = message.Split(delimiterChars);
        // I am going to use the same routine to read records that is used when reading from a file.
        // In order to do this I need to drop the first word on the line since the files don't have key words
        for (int index = 0; index < (lineEntries.Length - 1); index++)
            lineEntries[index] = lineEntries[index + 1];

        switch (switchEntries[0])
        {
            // Message sent by initiating computer to indicate the side the local player will be in playing
            // SetPlaySide <Nationality: German or Allied
            // Note that the default is Allied
            case GlobalDefinitions.PLAYSIDEKEYWORD:
                if (switchEntries[1] == "German")
                    GlobalDefinitions.sideControled = GlobalDefinitions.Nationality.German;
                else
                    GlobalDefinitions.sideControled = GlobalDefinitions.Nationality.Allied;
                break;
            case GlobalDefinitions.PASSCONTROLKEYWORK:
                GlobalDefinitions.localControl = true;
                GlobalDefinitions.writeToLogFile("processNetworkMessage: Message received to set local control");
                break;
            case GlobalDefinitions.SETCAMERAPOSITIONKEYWORD:
                Camera.main.transform.position = new Vector3(float.Parse(switchEntries[1]), float.Parse(switchEntries[2]), float.Parse(switchEntries[3]));
                Camera.main.GetComponent<Camera>().orthographicSize = float.Parse(switchEntries[4]);
                break;
            case GlobalDefinitions.MOUSESELECTIONKEYWORD:
                if (switchEntries[1] != "null")
                    inputMessage.GetComponent<InputMessage>().hex = GameObject.Find(switchEntries[1]);
                else
                    inputMessage.GetComponent<InputMessage>().hex = null;

                if (switchEntries[2] != "null")
                    inputMessage.GetComponent<InputMessage>().unit = GameObject.Find(switchEntries[2]);
                else
                    inputMessage.GetComponent<InputMessage>().unit = null;

                gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod(inputMessage.GetComponent<InputMessage>());
                break;
            case GlobalDefinitions.MOUSEDOUBLECLICKIONKEYWORD:
                GlobalDefinitions.Nationality passedNationality;

                if (switchEntries[2] == "German")
                    passedNationality = GlobalDefinitions.Nationality.German;
                else
                    passedNationality = GlobalDefinitions.Nationality.Allied;


                if (GlobalDefinitions.selectedUnit != null)
                    GlobalDefinitions.unhighlightUnit(GlobalDefinitions.selectedUnit);
                foreach (Transform hex in GameObject.Find("Board").transform)
                    GlobalDefinitions.unhighlightHex(hex.gameObject);
                GlobalDefinitions.selectedUnit = null;


                movementRoutinesInstance.GetComponent<MovementRoutines>().callMultiUnitDisplay(GameObject.Find(switchEntries[1]), passedNationality);
                break;
            case GlobalDefinitions.DISPLAYCOMBATRESOLUTIONKEYWORD:
                CombatResolutionRoutines.combatResolutionDisplay();
                break;
            case GlobalDefinitions.QUITKEYWORD:
                GlobalDefinitions.writeToLogFile("processNetworkMessage: Executing Quit");
                gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit(inputMessage.GetComponent<InputMessage>());
                break;

            case GlobalDefinitions.EXECUTETACTICALAIROKKEYWORD:
                TacticalAirToggleRoutines.tacticalAirOK();
                break;
            case GlobalDefinitions.ADDCLOSEDEFENSEKEYWORD:
                GameObject.Find("CloseDefense").GetComponent<TacticalAirToggleRoutines>().addCloseDefenseHex();
                break;
            case GlobalDefinitions.CANCELCLOSEDEFENSEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().cancelCloseDefense();
                break;
            case GlobalDefinitions.LOCATECLOSEDEFENSEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().locateCloseDefense();
                break;
            case GlobalDefinitions.ADDRIVERINTERDICTIONKEYWORD:
                GameObject.Find("RiverInterdiction").GetComponent<TacticalAirToggleRoutines>().addRiverInterdiction();
                break;
            case GlobalDefinitions.CANCELRIVERINTERDICTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().cancelRiverInterdiction();
                break;
            case GlobalDefinitions.LOCATERIVERINTERDICTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().locateRiverInterdiction();
                break;
            case GlobalDefinitions.ADDUNITINTERDICTIONKEYWORD:
                GameObject.Find("UnitInterdiction").GetComponent<TacticalAirToggleRoutines>().addInterdictedUnit();
                break;
            case GlobalDefinitions.CANCELUNITINTERDICTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().cancelInterdictedUnit();
                break;
            case GlobalDefinitions.LOCATEUNITINTERDICTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().locateInterdictedUnit();
                break;
            case GlobalDefinitions.TACAIRMULTIUNITSELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().multiUnitSelection();
                break;

            case GlobalDefinitions.MULTIUNITSELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.MULTIUNITSELECTIONCANCELKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<MultiUnitMovementToggleRoutines>().cancelGui();
                break;
            case GlobalDefinitions.LOADCOMBATKEYWORD:
                GameObject GUIButtonInstance = new GameObject("GUIButtonInstance");
                GUIButtonInstance.AddComponent<GUIButtonRoutines>();
                GUIButtonInstance.GetComponent<GUIButtonRoutines>().loadCombat();
                break;

            case GlobalDefinitions.SETCOMBATTOGGLEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.RESETCOMBATTOGGLEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.COMBATGUIOKKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatGUIOK>().okCombatGUISelection();
                break;
            case GlobalDefinitions.COMBATGUICANCELKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatGUIOK>().cancelCombatGUISelection();
                break;

            case GlobalDefinitions.ADDCOMBATAIRSUPPORTKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.REMOVECOMBATAIRSUPPORTKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.COMBATRESOLUTIONSELECTEDKEYWORD:
                // Load the combat results; the die roll is on the Global variable
                GameObject.Find(switchEntries[1]).GetComponent<CombatResolutionButtonRoutines>().resolutionSelected();
                break;
            case GlobalDefinitions.COMBATLOCATIONSELECTEDKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatResolutionButtonRoutines>().locateAttack();
                break;
            case GlobalDefinitions.COMBATCANCELKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatResolutionButtonRoutines>().cancelAttack();
                break;
            case GlobalDefinitions.COMBATOKKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatResolutionButtonRoutines>().ok();
                break;
            case GlobalDefinitions.CARPETBOMBINGRESULTSSELECTEDKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.RETREATSELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.POSTCOMBATMOVEMENTKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.ADDEXCHANGEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.REMOVEEXCHANGEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.OKEXCHANGEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<ExchangeOKRoutines>().exchangeOKSelected();
                break;
            case GlobalDefinitions.POSTCOMBATOKKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<PostCombatMovementOkRoutines>().executePostCombatMovement();
                break;
            case GlobalDefinitions.SETSUPPLYKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.RESETSUPPLYKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.LOCATESUPPLYKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<SupplyButtonRoutines>().locateSupplySource();
                break;
            case GlobalDefinitions.OKSUPPLYKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<SupplyButtonRoutines>().okSupplyWithEndPhase();
                break;
            case GlobalDefinitions.CHANGESUPPLYSTATUSKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.SAVEFILENAMEKEYWORD:
                if (File.Exists(GameControl.path + "TGCOutputFiles\\TGCRemoteSaveFile.txt"))
                    File.Delete(GameControl.path + "TGCOutputFiles\\TGCRemoteSaveFile.txt");
                break;
            case GlobalDefinitions.SENDSAVEFILELINEKEYWORD:
                using (StreamWriter saveFile = File.AppendText(GameControl.path + "TGCOutputFiles\\TGCRemoteSaveFile.txt"))
                {
                    for (int index = 1; index < (switchEntries.Length); index++)
                        saveFile.Write(switchEntries[index] + " ");
                    saveFile.WriteLine();
                }
                break;
            case GlobalDefinitions.PLAYNEWGAMEKEYWORD:
                gameStateControlInstance.GetComponent<gameStateControl>().currentState = setUpStateInstance.GetComponent<SetUpState>();
                gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize(inputMessage.GetComponent<InputMessage>());

                // Set the global parameter on what file to use, can't pass it to the executeNoResponse since it is passed as a method delegate elsewhere
                GlobalDefinitions.germanSetupFileUsed = Convert.ToInt32(switchEntries[1]);

                setUpStateInstance.GetComponent<SetUpState>().executeNoResponse();
                break;

            case GlobalDefinitions.INVASIONAREASELECTIONKEYWORD:
                GlobalDefinitions.writeToLogFile("processNetworkMessage: Received INVASIONAREASELECTIONKEYWORD - turning toggle " + switchEntries[1] + " to true");
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.CARPETBOMBINGSELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.CARPETBOMBINGLOCATIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CarpetBombingToggleRoutines>().locateCarpetBombingHex();
                break;
            case GlobalDefinitions.CARPETBOMBINGOKKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CarpetBombingOKRoutines>().carpetBombingOK();
                break;

            case GlobalDefinitions.DIEROLLRESULT1KEYWORD:
                GlobalDefinitions.dieRollResult1 = Convert.ToInt32(switchEntries[1]);
                break;
            case GlobalDefinitions.DIEROLLRESULT2KEYWORD:
                GlobalDefinitions.dieRollResult2 = Convert.ToInt32(switchEntries[1]);
                break;
            case GlobalDefinitions.UNDOKEYWORD:
                GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().executeUndo();
                break;
            case GlobalDefinitions.CHATMESSAGEKEYWORD:
                string chatMessage = "";
                for (int index = 0; index < (switchEntries.Length - 1); index++)
                    chatMessage += switchEntries[index + 1] + " ";
                GlobalDefinitions.writeToLogFile("Chat message received: " + chatMessage);
                GlobalDefinitions.addChatMessage(chatMessage);
                break;
            case GlobalDefinitions.SENDTURNFILENAMEWORD:
                // This command tells the remote computer what the name of the file is that will provide the saved turn file

                // The file name could have ' ' in it so need to reconstruct the full name
                string receivedFileName;
                receivedFileName = switchEntries[1];
                for (int i = 2; i < switchEntries.Length; i++)
                    receivedFileName = receivedFileName + " " + switchEntries[i];

                GlobalDefinitions.writeToLogFile("Received name of save file, calling FileTransferServer: fileName = " + receivedFileName + "  path to save = " + path);
                fileTransferServerInstance.GetComponent<FileTransferServer>().RequestFile(GlobalDefinitions.opponentIPAddress, receivedFileName, GameControl.path, true);
                break;

            case GlobalDefinitions.DISPLAYALLIEDSUPPLYRANGETOGGLEWORD:
                if (GameObject.Find("AlliedSupplyToggle").GetComponent<Toggle>().isOn)
                    GameObject.Find("AlliedSupplyToggle").GetComponent<Toggle>().isOn = false;
                else
                    GameObject.Find("AlliedSupplyToggle").GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.DISPLAYGERMANSUPPLYRANGETOGGLEWORD:
                if (GameObject.Find("GermanSupplyToggle").GetComponent<Toggle>().isOn)
                    GameObject.Find("GermanSupplyToggle").GetComponent<Toggle>().isOn = false;
                else
                    GameObject.Find("GermanSupplyToggle").GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.DISPLAYMUSTATTACKTOGGLEWORD:
                if (GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn)
                    GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn = false;
                else
                    GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.TOGGLEAIRSUPPORTCOMBATTOGGLE:
                {
                    if (GlobalDefinitions.combatAirSupportToggle != null)
                    {
                        if (GlobalDefinitions.combatAirSupportToggle.GetComponent<Toggle>().isOn)
                            GlobalDefinitions.combatAirSupportToggle.GetComponent<Toggle>().isOn = false;
                        else
                            GlobalDefinitions.combatAirSupportToggle.GetComponent<Toggle>().isOn = true;
                    }
                    break;
                }

            case GlobalDefinitions.TOGGLECARPETBOMBINGCOMBATTOGGLE:
                {
                    if (GlobalDefinitions.combatCarpetBombingToggle != null)
                    {
                        if (GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn)
                            GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn = false;
                        else
                            GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn = true;
                    }
                    break;
                }
            case GlobalDefinitions.DISCONNECTFROMREMOTECOMPUTER:
                {
                    // Quit the game and go back to the main menu
                    GameObject guiButtonInstance = new GameObject("GUIButtonInstance");
                    guiButtonInstance.AddComponent<GUIButtonRoutines>();
                    guiButtonInstance.GetComponent<GUIButtonRoutines>().yesMain();
                    break;
                }

            default:
                GlobalDefinitions.writeToLogFile("processNetworkMessage: Unknown network command received: " + message);
                break;
        }
    }

    /// <summary>
    /// This routine sends a command for a mouse click to the opponent's computer
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="hex"></param>
    public static void sendMouseClickToNetwork(GameObject unit, GameObject hex)
    {
        TransportScript.SendSocketMessage(GlobalDefinitions.SETCAMERAPOSITIONKEYWORD + " " + Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z + " " + Camera.main.GetComponent<Camera>().orthographicSize);

        string hexName;
        string unitName;
        if (hex != null)
        {
            hexName = hex.name;
            //GlobalDefinitions.writeToLogFile("sendMoustClickToNetwork: processing with hex = " + hex.name);
        }
        else
            hexName = "null";

        if (unit != null)
        {
            unitName = unit.name;
            //GlobalDefinitions.writeToLogFile("sendMoustClickToNetwork: processing with unit = " + unit.name);
        }
        else
            unitName = "null";

        TransportScript.SendSocketMessage(GlobalDefinitions.MOUSESELECTIONKEYWORD + " " + hexName + " " + unitName);
    }

    /// <summary>
    /// Send the informaiton needed for a double click to the network computer
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="currentNationality"></param>
    public static void sendMouseDoubleClickToNetwork(GameObject hex, GlobalDefinitions.Nationality currentNationality)
    {
        TransportScript.SendSocketMessage(GlobalDefinitions.SETCAMERAPOSITIONKEYWORD + " " + Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z + " " + Camera.main.GetComponent<Camera>().orthographicSize);

        TransportScript.SendSocketMessage(GlobalDefinitions.MOUSEDOUBLECLICKIONKEYWORD + " " + hex.name + " " + currentNationality);
    }

    /// <summary>
    /// Writes out the current state, used for writing to save files
    /// </summary>
    /// <param name="fileWriter"></param>
    public static void writeGameControlStatusVariables(StreamWriter fileWriter)
    {
        fileWriter.Write(gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality);
        fileWriter.WriteLine();
    }

    /// <summary>
    /// This routine initializes to the side who is in control
    /// </summary>
    /// <param name="entries"></param>
    public static void setGameState(string currentSide)
    {
        if (Convert.ToString(currentSide) == "German")
        {
            if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
            {
                if (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.Nationality.Allied)
                {
                    GlobalDefinitions.writeToLogFile("setGameState: setting game state to germanAIStateInstance");
                    gameStateControlInstance.GetComponent<gameStateControl>().currentState = germanAIStateInstance.GetComponent<GermanAIState>();
                }
                else
                {
                    GlobalDefinitions.writeToLogFile("setGameState: setting game state to germanIsolationStateInstance");
                    gameStateControlInstance.GetComponent<gameStateControl>().currentState = germanIsolationStateInstance.GetComponent<GermanIsolationState>();
                }
            }
            else
            {
                // Do not set the currentSidePlaying variable if it is an AI game since it will already have been set
                GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.German;
                GlobalDefinitions.writeToLogFile("setGameState: setting game state to germanIsolationStateInstance");
                gameStateControlInstance.GetComponent<gameStateControl>().currentState = germanIsolationStateInstance.GetComponent<GermanIsolationState>();
            }
        }
        else
        {
            if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
            {
                if (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.Nationality.German)
                {
                    GlobalDefinitions.writeToLogFile("setGameState: setting game state to alliedAIStateInstance");
                    gameStateControlInstance.GetComponent<gameStateControl>().currentState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
                    //gameStateControlInstance.GetComponent<gameStateControl>().currentState = alliedAIStateInstance.GetComponent<AlliedAIState>();
                }
                else
                {
                    GlobalDefinitions.writeToLogFile("setGameState: setting game state to turnInitializationStateInstance");
                    gameStateControlInstance.GetComponent<gameStateControl>().currentState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
                }
            }
            else
            {
                // Do not set the currentSidePlaying variable if it is an AI game since it will already have been set
                GlobalDefinitions.nationalityUserIsPlaying = GlobalDefinitions.Nationality.Allied;
                GlobalDefinitions.writeToLogFile("setGameState: setting game state to turnInitializationStateInstance");
                gameStateControlInstance.GetComponent<gameStateControl>().currentState = turnInitializationStateInstance.GetComponent<TurnInitializationState>();
            }
        }

        GlobalDefinitions.writeToLogFile("setGameState: localControl = " + GlobalDefinitions.localControl);
        if (!GlobalDefinitions.localControl)
        {
            inputMessage.GetComponent<InputMessage>().hex = null;
            inputMessage.GetComponent<InputMessage>().unit = null;
            GlobalDefinitions.writeToLogFile("setGameState: call intialization");
            //gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize(inputMessage.GetComponent<InputMessage>());
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

