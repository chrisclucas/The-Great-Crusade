using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class GUIButtonRoutines : MonoBehaviour
{

    public static Button yesButton;
    public static Button noButton;

    /// <summary>
    /// Button the ends the current phase on the static gui
    /// </summary>
    public void goToNextPhase()
    {
        // Check if there is a gui up before we move to the next phase since it could result in unknown state
        if (GlobalDefinitions.guiList.Count == 0)
        {
            // Need to do this first since during changes in control the next phase routine passes control so this would never be sent
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.NEXTPHASEKEYWORD);

            // Button to quit the current game state
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit(GameControl.inputMessage.GetComponent<InputMessage>());
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Resolve current gui before advancing to the next phase");
    }

    /// <summary>
    /// User selects Main Menu from the static gui
    /// </summary>
    public void goToMainMenu()
    {

        // If this is a network game and the player isn't in control do not allow to reset.  Player has to quit to exit in this case.
        if (!GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
        {
            GlobalDefinitions.guiUpdateStatusMessage("Cannot reset game when not in control");
            return;
        }

        // Turn off the button so that the same gui can't be pulled up
        GameObject.Find("MainMenuButton").GetComponent<Button>().interactable = false;

        // Turn off any guis that are on
        if (GlobalDefinitions.guiList.Count > 0)
            foreach (GameObject gui in GlobalDefinitions.guiList)
                gui.SetActive(false);

        GlobalDefinitions.askUserYesNoQuestion("Are you sure you want to quit?", ref yesButton, ref noButton, yesMain, noMain);
    }

    /// <summary>
    /// User selects Quit from the static gui
    /// </summary>
    public void quitApplication()
    {
        // Turn off the button
        GameObject.Find("QuitButton").GetComponent<Button>().interactable = false;

        // Turn off any guis that are on
        if (GlobalDefinitions.guiList.Count > 0)
            foreach (GameObject gui in GlobalDefinitions.guiList)
                gui.SetActive(false);

        GlobalDefinitions.askUserYesNoQuestion("Are you sure you want to quit?", ref yesButton, ref noButton, yesQuit, noQuit);
    }

    /// <summary>
    /// Executes when the user indicates he wants to go to main menu
    /// </summary>
    public void yesMain()
    {
        List<GameObject> removeUnitList = new List<GameObject>();

        // If this is a network game I've already checked that the player is in control
        if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network)
        {
            TransportScript.resetConnection(TransportScript.recHostId);
        }

        // Copy list so the guis can be removed
        List<GameObject> removeList = new List<GameObject>();
        foreach (GameObject gui in GlobalDefinitions.guiList)
            removeList.Add(gui);


        // Get rid of all active guis
        foreach (GameObject gui in removeList)
            GlobalDefinitions.removeGUI(gui);

        // Put all the units back on the OOB sheet
        foreach (Transform unit in GlobalDefinitions.allUnitsOnBoard.transform)
        {
            unit.GetComponent<UnitDatabaseFields>().unitInterdiction = false;
            if (unit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
            {
                GlobalDefinitions.unhighlightUnit(unit.gameObject);
                GlobalDefinitions.removeUnitFromHex(unit.gameObject, unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                unit.GetComponent<UnitDatabaseFields>().occupiedHex = null;
            }

            removeUnitList.Add(unit.gameObject);
        }

        foreach (GameObject unit in removeUnitList)
            GlobalDefinitions.returnUnitToOOBShet(unit);

        // Clear out the lists keeping track of both side's units on board
        GlobalDefinitions.alliedUnitsOnBoard.Clear();
        GlobalDefinitions.germanUnitsOnBoard.Clear();

        // Go through the hexes and reset all highlighting
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
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

            GlobalDefinitions.unhighlightHex(hex.gameObject);
        }

        GlobalDefinitions.writeToLogFile("Putting Allied units in Britain");
        // When restarting a game the units won't have their Britain location loaded so this needs to be done before a restart file is read
        GameControl.createBoardInstance.GetComponent<CreateBoard>().readBritainPlacement(GameControl.path + "TGCBritainUnitLocation.txt");

        GlobalDefinitions.resetAllGlobalDefinitions();

        // Turn the button back on
        GameObject.Find("MainMenuButton").GetComponent<Button>().interactable = true;

        MainMenuRoutines.getGameModeUI();
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    private void yesQuit()
    {
        // Turn the button back on - only applies to the editor since otherwise the applciation quits
        GameObject.Find("QuitButton").GetComponent<Button>().interactable = true;

        Application.Quit();
    }

    /// <summary>
    /// Change of mind, do not quit
    /// </summary>
    private void noQuit()
    {
        // Turn the button back on
        GameObject.Find("QuitButton").GetComponent<Button>().interactable = true;

        // Turn back on any guis that are active
        foreach (GameObject gui in GlobalDefinitions.guiList)
            gui.SetActive(true);
    }

    /// <summary>
    /// Change of mind, do not go to the main menu
    /// </summary>
    private void noMain()
    {
        // Turn the button back on
        GameObject.Find("MainMenuButton").GetComponent<Button>().interactable = true;

        // Turn back on any guis that are active
        foreach (GameObject gui in GlobalDefinitions.guiList)
            gui.SetActive(true);
    }

    /// <summary>
    /// Executes when the user wants to display the current combats
    /// </summary>
    public void executeCombatResolution()
    {
        if (GlobalDefinitions.guiList.Count == 0)
        {
            GlobalDefinitions.writeToLogFile("executeCombatResolution: Executing  combat count = " + GlobalDefinitions.allCombats.Count);
            if (GlobalDefinitions.allCombats.Count > 0)
            {
                // Turn off the button
                GameObject.Find("ResolveCombatButton").GetComponent<Button>().interactable = false;

                CombatResolutionRoutines.combatResolutionDisplay();

                GlobalDefinitions.writeToCommandFile(GlobalDefinitions.DISPLAYCOMBATRESOLUTIONKEYWORD);
            }
            else
                GlobalDefinitions.guiUpdateStatusMessage("No combats have been selected therefore nothing to resolve");
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Resolve current gui before trying to bring up combat display");
    }

    /// <summary>
    /// Undo from the static gui
    /// </summary>
    public void executeUndo()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.UNDOKEYWORD);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeUndo(GameControl.inputMessage.GetComponent<InputMessage>());
    }

    /// <summary>
    /// This routine processes the chagne in the toggle displaying the allied supply range
    /// </summary>
    public void displayAlliedSupplyRange()
    {
        // Notify the remote computer
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.DISPLAYALLIEDSUPPLYRANGETOGGLEWORD);


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
            foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
                if (hex.GetComponent<HexDatabaseFields>().alliedInSupply)
                    GlobalDefinitions.highlightHexInSupply(hex);
        }
        else
        {
            // Set the global variable so the other unhighlighing can be processed properly
            GlobalDefinitions.displayAlliedSupplyStatus = false;
            // Turn off supply highlighting
            foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
                GlobalDefinitions.unhighlightHexSupplyRange(hex);
        }
    }

    /// <summary>
    /// Toggle that displays all units that must be attacked
    /// </summary>
    public void displayMustAttackUnits()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.DISPLAYMUSTATTACKTOGGLEWORD);

        if (gameObject.GetComponent<Toggle>().isOn)
            CombatRoutines.checkIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality, true);
        else
        {
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                GlobalDefinitions.unhighlightUnit(unit);
            foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
                GlobalDefinitions.unhighlightUnit(unit);
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
    public void loadCombat()
    {
        if (GlobalDefinitions.guiList.Count == 0)
        {
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.LOADCOMBATKEYWORD);

            GlobalDefinitions.guiUpdateStatusMessage("Select a hex to attack");
            if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedInvasionStateInstance")
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedInvasionState>().loadCombat;
            if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedAirborneStateInstance")
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedAirborneState>().loadCombat;
            if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanMovementStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().loadCombat;
            if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<CombatState>().executeSelectUnit;
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Resolve current gui before assigning combat");
    }

    /// <summary>
    /// This routine processes the chagne in the toggle displaying the German supply range
    /// </summary>
    public void displayGermanSupplyRange()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.DISPLAYGERMANSUPPLYRANGETOGGLEWORD);

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
            foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
                if (hex.GetComponent<HexDatabaseFields>().germanInSupply)
                    GlobalDefinitions.highlightHexInSupply(hex);
        }
        else
        {
            // Set the global variable so the other unhighlighing can be processed properly
            GlobalDefinitions.displayGermanSupplyStatus = false;
            // Turn off supply highlighting
            foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
                GlobalDefinitions.unhighlightHexSupplyRange(hex);
        }
    }

    /// <summary>
    /// Executes when the toggle is changed, highlights the hexes that were under Allied control in the current turn
    /// </summary>
    public void displayHisoricalProgress()
    {
        if (GameObject.Find("ShowHistoryToggle").GetComponent<Toggle>().isOn)
            // The toggle is on so highlight the hexes
            foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            {
                if ((hex.GetComponent<HexDatabaseFields>().historyWeekCaptured <= GlobalDefinitions.turnNumber) &&
                        !hex.GetComponent<HexDatabaseFields>().sea && !hex.GetComponent<HexDatabaseFields>().bridge)
                {
                    Renderer targetRenderer = hex.GetComponent(typeof(SpriteRenderer)) as Renderer;
                    hex.transform.localScale = new Vector2(0.75f, 0.75f);
                    targetRenderer.sortingLayerName = "Highlight";
                    targetRenderer.material.color = Color.blue;
                    targetRenderer.sortingOrder = 2;
                }
            }
        else
            // The toggle is off so unhightlight the hexes
            foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
                GlobalDefinitions.unhighlightHex(hex);
    }

    /// <summary>
    /// Executes when the toogle is changed and either makes the units on the board visible or hides them
    /// </summary>
    public void hideUnits()
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
                GlobalDefinitions.unhighlightUnit(unit.gameObject);
        }
    }

    public void updateSettings()
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
        if (GlobalDefinitions.guiList.Count > 0)
            foreach (GameObject gui in GlobalDefinitions.guiList)
                gui.SetActive(false);

        //settingCanvasGameObject = GlobalDefinitions.createGUICanvas("SettingsGUIInstance",
        GlobalDefinitions.createGUICanvas("SettingsGUIInstance",
                panelWidth,
                panelHeight,
                ref settingCanvas);

        GlobalDefinitions.createText("Settings", "SettingsText",
                2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                0,
                3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);

        agressivenessSlider = GlobalDefinitions.createSlider("AgressivenessSlider", "GUI Slider15",
                0,
                2f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);
        agressivenessSlider.value = GlobalDefinitions.aggressiveSetting;
        agressivenessSlider.onValueChanged.AddListener(delegate { updateAggressivenessSettingText(agressivenessSlider.value); });

        GlobalDefinitions.createText("Aggressive", "AggressiveText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                2f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);

        GlobalDefinitions.createText("Defensive", "DefensiveText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                -3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                2f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);

        GlobalDefinitions.aggressivenessSettingText = GlobalDefinitions.createText(Convert.ToString(agressivenessSlider.value), "AggressivenessSettingText",
                2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                0,
                1.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);

        GlobalDefinitions.createText("Computer Aggressiveness", "ComputerAggressivenessText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                0,
                1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);

        diffiultySlider = GlobalDefinitions.createSlider("DifficultySlider", "GUI Slider010",
                0,
                0,
                settingCanvas);
        diffiultySlider.value = GlobalDefinitions.difficultySetting;
        diffiultySlider.onValueChanged.AddListener(delegate { updateDifficultySettingText(diffiultySlider.value); });

        GlobalDefinitions.createText("Harder", "HarderText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                0,
                settingCanvas);

        GlobalDefinitions.createText("Easier", "EasierText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                -3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                0,
                settingCanvas);

        GlobalDefinitions.difficultySettingText = GlobalDefinitions.createText(Convert.ToString(diffiultySlider.value), "DifficultySettingText",
                2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                0,
                -0.5f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);

        GlobalDefinitions.createText("Game Difficulty", "GameDifficultyText",
                2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                0,
                -1f * GlobalDefinitions.GUIUNITIMAGESIZE,
        settingCanvas);

        okButton = GlobalDefinitions.createButton("settingOKButton", "OK",
                -1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                -3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);
        okButton.gameObject.AddComponent<SettingGUIButtons>();
        okButton.onClick.AddListener(okButton.GetComponent<SettingGUIButtons>().okSelected);

        cancelButton = GlobalDefinitions.createButton("settingCancelButton", "Cancel",
                1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                -3f * GlobalDefinitions.GUIUNITIMAGESIZE,
                settingCanvas);
        cancelButton.gameObject.AddComponent<SettingGUIButtons>();
        cancelButton.onClick.AddListener(okButton.GetComponent<SettingGUIButtons>().cancelSelected);
    }

    public void updateDifficultySettingText(float value)
    {
        GlobalDefinitions.difficultySettingText.GetComponent<Text>().text = Convert.ToString(value);
    }

    public void updateAggressivenessSettingText(float value)
    {
        GlobalDefinitions.aggressivenessSettingText.GetComponent<Text>().text = Convert.ToString(value);
    }

    /// <summary>
    /// Executes when the user hits OK on the victory screen
    /// </summary>
    public void victoryOK()
    {
        InputMessage inputMessage = null;
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState = GameControl.victoryState.GetComponent<VictoryState>();
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize(inputMessage);
        GlobalDefinitions.removeAllGUIs();
    }
}
