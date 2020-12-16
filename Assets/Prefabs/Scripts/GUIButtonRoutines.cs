using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class GUIButtonRoutines : MonoBehaviour
    {

        public static Button yesButton;
        public static Button noButton;

        /// <summary>
        /// Button the ends the current phase on the static gui
        /// </summary>
        public void GoToNextPhase()
        {
            // Check if there is a gui up before we move to the next phase since it could result in unknown state
            if (GUIRoutines.guiList.Count == 0)
            {
                // Need to do this first since during changes in control the next phase routine passes control so this would never be sent
                GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.NEXTPHASEKEYWORD);

                // Quit the current game state
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.ExecuteQuit();
            }
            else
                GlobalDefinitions.GuiUpdateStatusMessage("Resolve displayed menu before trying advancing to the next phase");
        }

        /// <summary>
        /// User selects Main Menu from the static gui
        /// </summary>
        public void GoToMainMenu()
        {

            // If this is a network game and the player isn't in control do not allow to reset.  Player has to quit to exit in this case.
            if (!GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork))
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Cannot reset game when not in control");
                return;
            }

            // Turn off the button so that the same gui can't be pulled up
            GlobalDefinitions.mainMenuButton.GetComponent<Button>().interactable = false;

            // Turn off any guis that are on
            if (GUIRoutines.guiList.Count > 0)
                foreach (GameObject gui in GUIRoutines.guiList)
                    gui.SetActive(false);

            GlobalDefinitions.AskUserYesNoQuestion("Are you sure you want to quit?", ref yesButton, ref noButton, YesMain, NoMain);
        }

        /// <summary>
        /// User selects Quit from the static gui
        /// </summary>
        public void QuitApplication()
        {
            // Turn off the button
            GlobalDefinitions.quitButton.GetComponent<Button>().interactable = false;

            // Turn off any guis that are on
            if (GUIRoutines.guiList.Count > 0)
                foreach (GameObject gui in GUIRoutines.guiList)
                    gui.SetActive(false);

            GlobalDefinitions.AskUserYesNoQuestion("Are you sure you want to quit?", ref yesButton, ref noButton, YesQuit, NoQuit);
        }

        /// <summary>
        /// Executes when the user indicates he wants to go to main menu
        /// </summary>
        public void YesMain()
        {
            List<GameObject> removeUnitList = new List<GameObject>();

            // If this is a network game I've already checked that the player is in control
            //if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork)
            //{
            //    GlobalDefinitions.WriteToLogFile("YesMain: Calling ResetConnection()");
            //    //byte error;
            //    //NetworkTransport.Disconnect(TransportScript.receivedHostId, TransportScript.gameConnectionId, out error);
            //    //Network.Disconnect();
            //    TransportScript.ResetConnection(TransportScript.computerId);
            //}

            // Copy list so the guis can be removed
            List<GameObject> removeList = new List<GameObject>();
            foreach (GameObject gui in GUIRoutines.guiList)
                removeList.Add(gui);


            // Get rid of all active guis
            foreach (GameObject gui in removeList)
                GUIRoutines.RemoveGUI(gui);

            // Put all the units back on the OOB sheet
            foreach (Transform unit in GlobalDefinitions.allUnitsOnBoard.transform)
            {
                unit.GetComponent<UnitDatabaseFields>().unitInterdiction = false;
                unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
                unit.GetComponent<UnitDatabaseFields>().hasMoved = false;
                unit.GetComponent<UnitDatabaseFields>().unitEliminated = false;
                unit.GetComponent<UnitDatabaseFields>().occupiedHex = null;
                unit.GetComponent<UnitDatabaseFields>().beginningTurnHex = null;
                unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex = -1;
                unit.GetComponent<UnitDatabaseFields>().inSupply = true;
                unit.GetComponent<UnitDatabaseFields>().supplySource = null;
                unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;
                unit.GetComponent<UnitDatabaseFields>().remainingMovement = unit.GetComponent<UnitDatabaseFields>().movementFactor;
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
                {
                    GlobalDefinitions.UnhighlightUnit(unit.gameObject);
                    GeneralHexRoutines.RemoveUnitFromHex(unit.gameObject, unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                    unit.GetComponent<UnitDatabaseFields>().occupiedHex = null;
                }

                removeUnitList.Add(unit.gameObject);
            }

            foreach (GameObject unit in removeUnitList)
                GlobalDefinitions.ReturnUnitToOOBShet(unit);

            // Clear out the lists keeping track of both side's units on board
            GlobalDefinitions.alliedUnitsOnBoard.Clear();
            GlobalDefinitions.germanUnitsOnBoard.Clear();

            // Go through the hexes and reset all highlighting
            foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
            {
                hex.GetComponent<HexDatabaseFields>().riverInterdiction = false;
                hex.GetComponent<HexDatabaseFields>().closeDefenseSupport = false;
                hex.GetComponent<HexDatabaseFields>().successfullyInvaded = false;
                hex.GetComponent<HexDatabaseFields>().alliedControl = false;
                hex.GetComponent<HexDatabaseFields>().inAlliedZOC = false;
                hex.GetComponent<HexDatabaseFields>().inGermanZOC = false;
                hex.GetComponent<HexDatabaseFields>().occupyingUnit.Clear();
                hex.GetComponent<HexDatabaseFields>().unitsExertingZOC.Clear();
                hex.GetComponent<HexDatabaseFields>().availableForMovement = false;
                hex.GetComponent<HexDatabaseFields>().strategicRemainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                hex.GetComponent<HexDatabaseFields>().supplySources.Clear();
                hex.GetComponent<HexDatabaseFields>().unitsThatCanBeSupplied.Clear();
                hex.GetComponent<HexDatabaseFields>().closeDefenseSupport = false;
                hex.GetComponent<HexDatabaseFields>().riverInterdiction = false;
                hex.GetComponent<HexDatabaseFields>().carpetBombingActive = false;

                GlobalDefinitions.UnhighlightHex(hex.gameObject);
            }

            GlobalDefinitions.WriteToLogFile("Putting Allied units in Britain");
            // When restarting a game the units won't have their Britain location loaded so this needs to be done before a restart file is read
            GameControl.createBoardInstance.GetComponent<CreateBoard>().ReadBritainPlacement(GlobalGameFields.britainUnitLocationFile);

            GlobalDefinitions.ResetAllGlobalDefinitions();

            // Turn the button back on
            GlobalDefinitions.mainMenuButton.GetComponent<Button>().interactable = true;

            MainMenuRoutines.GetGameModeUI();
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        private void YesQuit()
        {
            // Turn the button back on - only applies to the editor since otherwise the applciation quits
            GlobalDefinitions.quitButton.GetComponent<Button>().interactable = true;

            Application.Quit();
        }

        /// <summary>
        /// Change of mind, do not quit
        /// </summary>
        private void NoQuit()
        {
            // Turn the button back on
            GlobalDefinitions.quitButton.GetComponent<Button>().interactable = true;

            // Turn back on any guis that are active
            foreach (GameObject gui in GUIRoutines.guiList)
                gui.SetActive(true);
        }

        /// <summary>
        /// Change of mind, do not go to the main menu
        /// </summary>
        private void NoMain()
        {
            // Turn the button back on
            GlobalDefinitions.mainMenuButton.GetComponent<Button>().interactable = true;

            // Turn back on any guis that are active
            foreach (GameObject gui in GUIRoutines.guiList)
                gui.SetActive(true);
        }

        /// <summary>
        /// Executes when the user wants to display the current combats
        /// </summary>
        public void ExecuteCombatResolution()
        {
            if (GUIRoutines.guiList.Count == 0)
            {
                if (GlobalDefinitions.allCombats.Count > 0)
                {
                    // Turn off the button
                    GameObject.Find("ResolveCombatButton").GetComponent<Button>().interactable = false;

                    CombatResolutionRoutines.CombatResolutionDisplay();

                    // When this is called by the AI then the line below end up calling two guis when the command file is being read
                    if (!GlobalDefinitions.AICombat)
                        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.DISPLAYCOMBATRESOLUTIONKEYWORD);
                }
                else
                    GlobalDefinitions.GuiUpdateStatusMessage("No combats have been assigned therefore there is nothing to resolve");
            }
            else
                GlobalDefinitions.GuiUpdateStatusMessage("Resolve the currently displayed menu before trying to bring up combat display");
        }

        /// <summary>
        /// Undo from the static gui
        /// </summary>
        public void ExecuteUndo()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.UNDOKEYWORD);

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.ExecuteUndo(GameControl.inputMessage.GetComponent<InputMessage>());
        }

        /// <summary>
        /// This routine processes the chagne in the toggle displaying the allied supply range
        /// </summary>
        public void DisplayAlliedSupplyRange()
        {
            // Notify the remote computer
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.DISPLAYALLIEDSUPPLYRANGETOGGLEWORD);


            if (gameObject.GetComponent<Toggle>().isOn)
            {
                // Check if the German supply range is being displayed, it must be turned off first otherwise we will get confused
                if (GlobalDefinitions.displayGermanSupplyStatus)
                {
                    GameObject.Find("GermanSupplyToggle").GetComponent<Toggle>().isOn = false;
                }

                // Set the global variable so the other unhighlighing can be processed properly
                GlobalDefinitions.displayAlliedSupplyStatus = true;
                // Highlight all hexes that have supply available
                foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                    if (hex.GetComponent<HexDatabaseFields>().alliedInSupply)
                        GlobalDefinitions.HighlightHexInSupply(hex);
            }
            else
            {
                // Set the global variable so the other unhighlighing can be processed properly
                GlobalDefinitions.displayAlliedSupplyStatus = false;
                // Turn off supply highlighting
                foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                    GlobalDefinitions.UnhighlightHexSupplyRange(hex);
            }
        }

        /// <summary>
        /// Toggle that displays all units that must be attacked
        /// </summary>
        public void DisplayMustAttackUnits()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.DISPLAYMUSTATTACKTOGGLEWORD);

            if (gameObject.GetComponent<Toggle>().isOn)
                CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, true);
            else
            {
                foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                    GlobalDefinitions.UnhighlightUnit(unit);
                foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
                    GlobalDefinitions.UnhighlightUnit(unit);
            }

            //else
            //{
            //    foreach (GameObject unit in GlobalDefinitions.mustAttackUnits)
            //        GlobalDefinitions.unhighlightUnit(unit);
            //    foreach (GameObject unit in GlobalDefinitions.mustBeAttackedUnits)
            //        GlobalDefinitions.unhighlightUnit(unit);
            //    GlobalDefinitions.mustAttackUnits.Clear();
            //    GlobalDefinitions.mustBeAttackedUnits.Clear();
            //}
        }

        /// <summary>
        /// Executes when the user selects Assign Combat from the static gui
        /// </summary>
        public void LoadCombat()
        {
            if (GUIRoutines.guiList.Count == 0)
            {
                GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.LOADCOMBATKEYWORD);

                GlobalDefinitions.GuiUpdateStatusMessage("Select a hex to attack; hex must contain enemy units that are adjacent to friendly units");
                if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedInvasionStateInstance")
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedInvasionState>().LoadCombat;
                if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedAirborneStateInstance")
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedAirborneState>().LoadCombat;
                if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                        (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanMovementStateInstance"))
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().LoadCombat;
                if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                        (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance"))
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteSelectUnit;
            }
            else
                GlobalDefinitions.GuiUpdateStatusMessage("Resolve the currently displayed menu before assigning combat");
        }

        /// <summary>
        /// This routine processes the chagne in the toggle displaying the German supply range
        /// </summary>
        public void DisplayGermanSupplyRange()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.DISPLAYGERMANSUPPLYRANGETOGGLEWORD);

            if (gameObject.GetComponent<Toggle>().isOn)
            {
                // Check if the German supply range is being displayed, it must be turned off first otherwise we will get confused
                if (GlobalDefinitions.displayAlliedSupplyStatus)
                {
                    GameObject.Find("AlliedSupplyToggle").GetComponent<Toggle>().isOn = false;
                }

                // Set the global variable so the other unhighlighing can be processed properly
                GlobalDefinitions.displayGermanSupplyStatus = true;
                // Highlight all hexes that have supply available
                foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                    if (hex.GetComponent<HexDatabaseFields>().germanInSupply)
                        GlobalDefinitions.HighlightHexInSupply(hex);
            }
            else
            {
                // Set the global variable so the other unhighlighing can be processed properly
                GlobalDefinitions.displayGermanSupplyStatus = false;
                // Turn off supply highlighting
                foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                    GlobalDefinitions.UnhighlightHexSupplyRange(hex);
            }
        }

        /// <summary>
        /// Executes when the toggle is changed, highlights the hexes that were under Allied control in the current turn
        /// </summary>
        public void DisplayHisoricalProgress()
        {
            if (GameObject.Find("ShowHistoryToggle").GetComponent<Toggle>().isOn)
                // The toggle is on so highlight the hexes
                foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                {
                    if ((hex.GetComponent<HexDatabaseFields>().historyWeekCaptured <= GlobalDefinitions.turnNumber) &&
                            !hex.GetComponent<HexDatabaseFields>().sea && !hex.GetComponent<HexDatabaseFields>().bridge)
                    {
                        Renderer targetRenderer = hex.GetComponent(typeof(SpriteRenderer)) as Renderer;
                        //hex.transform.localScale = new Vector2(0.75f, 0.75f);
                        targetRenderer.sortingLayerName = "Highlight";
                        targetRenderer.material.color = Color.blue;
                        targetRenderer.sortingOrder = 2;
                    }
                }
            else
                // The toggle is off so unhightlight the hexes
                foreach (GameObject hex in HexDefinitions.allHexesOnBoard)
                    GlobalDefinitions.UnhighlightHex(hex);
        }

        /// <summary>
        /// Executes when the toogle is changed and either makes the units on the board visible or hides them
        /// </summary>
        public void HideUnits()
        {
            if (GameObject.Find("HideUnitsToggle").GetComponent<Toggle>().isOn)
            {
                // The toggle is on so hide the units
                //foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
                //{
                //    Renderer targetRenderer = unit.GetComponent(typeof(SpriteRenderer)) as Renderer;
                //    targetRenderer.material.color = new Vector4(0f, 0f, 0f, 0f);
                //}
                //foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                //{
                //    Renderer targetRenderer = unit.GetComponent(typeof(SpriteRenderer)) as Renderer;
                //    targetRenderer.material.color = new Vector4(0f, 0f, 0f, 0f);
                //}
                foreach (Transform unit in GlobalDefinitions.allUnitsOnBoard.transform)
                {
                    Renderer targetRenderer = unit.GetComponent(typeof(SpriteRenderer)) as Renderer;
                    targetRenderer.material.color = new Vector4(0f, 0f, 0f, 0f);
                }
            }
            else
            {
                // Bring the units back
                //foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
                //    GlobalDefinitions.unhighlightUnit(unit);
                //foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                //    GlobalDefinitions.unhighlightUnit(unit);
                foreach (Transform unit in GlobalDefinitions.allUnitsOnBoard.transform)
                    GlobalDefinitions.UnhighlightUnit(unit.gameObject);
            }
        }

        public void UpdateSettings()
        {
            // Turn the button off so that it can't pull up more of the same window
            GameObject.Find("SettingsButton").GetComponent<Button>().interactable = false;

            Slider agressivenessSlider;
            Slider diffiultySlider;
            Button cancelButton;
            Button okButton;
            Canvas settingCanvas = new Canvas();
            //GameObject settingCanvasGameObject = new GameObject("settingCanvasGameObject");

            float panelWidth = 10 * GlobalDefinitions.GUIUNITIMAGESIZE;
            float panelHeight = 7 * GlobalDefinitions.GUIUNITIMAGESIZE;

            // Turn off any guis that are on
            if (GUIRoutines.guiList.Count > 0)
                foreach (GameObject gui in GUIRoutines.guiList)
                    gui.SetActive(false);

            //settingCanvasGameObject = GUIRoutines.CreateGUICanvas("SettingsGUIInstance",
            GUIRoutines.CreateGUICanvas("SettingsGUIInstance",
                    panelWidth,
                    panelHeight,
                    ref settingCanvas);

            GUIRoutines.CreateUIText("Settings", "SettingsText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    0,
                    3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    Color.white, settingCanvas);

            agressivenessSlider = GUIRoutines.CreateSlider("AgressivenessSlider", "GUI Slider15",
                    0,
                    2f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    settingCanvas);
            agressivenessSlider.value = GlobalDefinitions.aggressiveSetting;
            agressivenessSlider.onValueChanged.AddListener(delegate { UpdateAggressivenessSettingText(agressivenessSlider.value); });

            GUIRoutines.CreateUIText("Aggressive", "AggressiveText",
                    3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    2f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    Color.white, settingCanvas);

            GUIRoutines.CreateUIText("Defensive", "DefensiveText",
                    3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    -3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    2f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    Color.white, settingCanvas);

            GlobalDefinitions.aggressivenessSettingText = GUIRoutines.CreateUIText(Convert.ToString(agressivenessSlider.value), "AggressivenessSettingText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    0,
                    1.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    Color.white, settingCanvas);

            GUIRoutines.CreateUIText("Computer Aggressiveness", "ComputerAggressivenessText",
                    3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    0,
                    1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    Color.white, settingCanvas);

            diffiultySlider = GUIRoutines.CreateSlider("DifficultySlider", "GUI Slider010",
                    0,
                    0,
                    settingCanvas);
            diffiultySlider.value = GlobalDefinitions.difficultySetting;
            diffiultySlider.onValueChanged.AddListener(delegate { UpdateDifficultySettingText(diffiultySlider.value); });

            GUIRoutines.CreateUIText("Harder", "HarderText",
                    3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    0,
                    Color.white, settingCanvas);

            GUIRoutines.CreateUIText("Easier", "EasierText",
                    3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    -3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    0,
                    Color.white, settingCanvas);

            GlobalDefinitions.difficultySettingText = GUIRoutines.CreateUIText(Convert.ToString(diffiultySlider.value), "DifficultySettingText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    0,
                    -0.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    Color.white, settingCanvas);

            GUIRoutines.CreateUIText("Game Difficulty", "GameDifficultyText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    0,
                    -1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    Color.white, settingCanvas);

            okButton = GUIRoutines.CreateButton("settingOKButton", "OK",
                    -1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    -3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    settingCanvas);
            okButton.gameObject.AddComponent<SettingGUIButtons>();
            okButton.onClick.AddListener(okButton.GetComponent<SettingGUIButtons>().OkSelected);

            cancelButton = GUIRoutines.CreateButton("settingCancelButton", "Cancel",
                    1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    -3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                    settingCanvas);
            cancelButton.gameObject.AddComponent<SettingGUIButtons>();
            cancelButton.onClick.AddListener(cancelButton.GetComponent<SettingGUIButtons>().CancelSelected);
        }

        public void UpdateDifficultySettingText(float value)
        {
            GlobalDefinitions.difficultySettingText.GetComponent<TextMeshProUGUI>().text = Convert.ToString(value);
        }

        public void UpdateAggressivenessSettingText(float value)
        {
            GlobalDefinitions.aggressivenessSettingText.GetComponent<TextMeshProUGUI>().text = Convert.ToString(value);
        }

        /// <summary>
        /// Executes when the user hits OK on the victory screen
        /// </summary>
        public void VictoryOK()
        {
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.victoryState.GetComponent<VictoryState>();
            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
            GUIRoutines.RemoveAllGUIs();
        }
    }
}