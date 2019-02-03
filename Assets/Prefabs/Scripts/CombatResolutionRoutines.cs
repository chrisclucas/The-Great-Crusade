//#define OUTPUTDEBUG

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CombatResolutionRoutines : MonoBehaviour
{
    /// <summary>
    /// Takes the string passed to it and returns an integer representing the odds
    /// </summary>
    /// <param name="combatOdds"></param>
    /// <returns></returns>
    private static int translateCombatOddsToArrayIndex(string combatOdds)
    {
        switch (combatOdds)
        {
            case "1:7":
                return (0);
            case "1:6":
                return (1);
            case "1:5":
                return (2);
            case "1:4":
                return (3);
            case "1:3":
                return (4);
            case "1:2":
                return (5);
            case "1:1":
                return (6);
            case "2:1":
                return (7);
            case "3:1":
                return (8);
            case "4:1":
                return (9);
            case "5:1":
                return (10);
            case "6:1":
                return (11);
            case "7:1":
                return (12);
            default:
                GlobalDefinitions.guiUpdateStatusMessage("Internal Error - Unknown Odds Found - " + combatOdds);
                return (11);
        }
    }

    /// <summary>
    /// Translates the enumerated combat result to a string
    /// </summary>
    /// <param name="combatResults"></param>
    /// <returns></returns>
    private static string convertResultsToString(GlobalDefinitions.CombatResults combatResults)
    {
        switch (combatResults)
        {
            case GlobalDefinitions.CombatResults.Aback2:
                return ("Aback2");
            case GlobalDefinitions.CombatResults.Aelim:
                return ("Aelim");
            case GlobalDefinitions.CombatResults.Dback2:
                return ("Dback2");
            case GlobalDefinitions.CombatResults.Delim:
                return ("Delim");
            case GlobalDefinitions.CombatResults.Exchange:
                return ("Exchange");
            default:
                GlobalDefinitions.guiUpdateStatusMessage("Internal Error - Can't translate combat results - " + combatResults);
                return ("");
        }
    }

    /// <summary>
    /// Displays the large GUI that the user uses to select the order of combat resolution
    /// </summary>
    public static void combatResolutionDisplay()
    {
#if OUTPUTDEBUG
        GlobalDefinitions.writeToLogFile("combatResolutionDisplay: executing - number of combats = " + GlobalDefinitions.allCombats.Count);
        foreach (GameObject singleCombat in GlobalDefinitions.allCombats)
        {
            GlobalDefinitions.writeToLogFile("combatResolutionDisplay:  Defenders");
            foreach (GameObject defender in singleCombat.GetComponent<Combat>().defendingUnits)
                GlobalDefinitions.writeToLogFile("combatResolutionDisplay:      " + defender.name);
            GlobalDefinitions.writeToLogFile("combatResolutionDisplay:  Attackers");
            foreach (GameObject attacker in singleCombat.GetComponent<Combat>().attackingUnits)
                GlobalDefinitions.writeToLogFile("combatResolutionDisplay:      " + attacker.name);
            GlobalDefinitions.writeToLogFile("combatResolutionDisplay:");
        }
#endif

        float yPosition = 0;
        Button okButton;
        bool playerControl = true;

        GlobalDefinitions.writeToLogFile("combatResolutionDisplay: executing");

        GameObject combatResolutionGuiInstance;

        // I'm going to set a flag to indicate when the display is being presented to resolve the AI's combats.
        // The air support toggle and cancel button will be disabled since the user should not be able to do anything other than 
        // resolve combats
        if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
        {
            if ((GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.Nationality.German) && (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance"))
                playerControl = false;
            if ((GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.Nationality.Allied) && (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance"))
                playerControl = false;
        }

        float panelWidth = (15 * GlobalDefinitions.GUIUNITIMAGESIZE);
        float panelHeight;
        if (GlobalDefinitions.allCombats.Count == 0)
            panelHeight = ((GlobalDefinitions.allCombats.Count * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE) + 4 * GlobalDefinitions.GUIUNITIMAGESIZE);
        else
            panelHeight = ((GlobalDefinitions.allCombats.Count * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE) + 2 * GlobalDefinitions.GUIUNITIMAGESIZE);

        Canvas combatCanvas = null;

        // In case a scrolling window is needed for the combats need to create a content panel
        GameObject combatContentPanel = new GameObject("CombatContentPanel");
        GlobalDefinitions.combatContentPanel = combatContentPanel;
        Image panelImage = combatContentPanel.AddComponent<Image>();

        panelImage.color = new Color32(0, 44, 255, 220);
        panelImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panelImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panelImage.rectTransform.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelImage.rectTransform.anchoredPosition = new Vector2(0, 0);

        if (panelHeight > (UnityEngine.Screen.height - 50))
            combatResolutionGuiInstance = GlobalDefinitions.createScrollingGUICanvas("CombatResolutionGUIInstance",
                    panelWidth,
                    panelHeight,
                    ref combatContentPanel,
                    ref combatCanvas);
        else
        {
            combatResolutionGuiInstance = GlobalDefinitions.createGUICanvas("CombatResolutionGUIInstance",
                panelWidth,
                panelHeight,
                ref combatCanvas);
            combatContentPanel.transform.SetParent(combatResolutionGuiInstance.transform, false);
        }
        GlobalDefinitions.combatResolutionGUIInstance = combatResolutionGuiInstance;

        // Put a series of text boxes along the top row to serve as the header

        // The first three columns contain images of the defending units
        GlobalDefinitions.createText("Units on Defense", "UnitsHeaderText",
                3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 1 * 1.25f - 0.5f * panelWidth,
                (GlobalDefinitions.allCombats.Count + 1.25f) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas).transform.SetParent(combatContentPanel.transform, false);

        // In column four the defense factor will be listed
        GlobalDefinitions.createText("Defense", "DefenseHeaderText",
                1.1f * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 4 * 1.25f - 0.5f * panelWidth,
                (GlobalDefinitions.allCombats.Count + 1.25f) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas).transform.SetParent(combatContentPanel.transform, false);

        // In column five the attack factor will be listed
        GlobalDefinitions.createText("Attack", "AttackHeaderText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 5 * 1.25f - 0.5f * panelWidth,
                (GlobalDefinitions.allCombats.Count + 1.25f) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas).transform.SetParent(combatContentPanel.transform, false);

        // In column six the odds will be listed
        GlobalDefinitions.createText("Odds", "OddsHeaderText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 6 * 1.25f - 0.5f * panelWidth,
                (GlobalDefinitions.allCombats.Count + 1.25f) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas).transform.SetParent(combatContentPanel.transform, false);

        // In column seven the carpet bombing indicator will be placed if Allied mode
        // if it is the German mode will put the close defense indicator
        if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
            GlobalDefinitions.createText("Carpet Bomb", "CarpetBombHeaderText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 7 * 1.25f - 0.5f * panelWidth,
                    (GlobalDefinitions.allCombats.Count + 1.25f) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas).transform.SetParent(combatContentPanel.transform, false);
        else
            GlobalDefinitions.createText("Air Def", "CloseDefenseHeaderText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 7 * 1.25f - 0.5f * panelWidth,
                    (GlobalDefinitions.allCombats.Count + 1.25f) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas).transform.SetParent(combatContentPanel.transform, false);

        // In column eight the air support toggle will be placed only if it is Allied combat
        if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
            GlobalDefinitions.createText("Air Support", "AirSupportHeaderText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 8 * 1.25f - 0.5f * panelWidth,
                    (GlobalDefinitions.allCombats.Count + 1.25f) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas).transform.SetParent(combatContentPanel.transform, false);

        //  In column nine the combat results will be listed
        GlobalDefinitions.createText("Combat Results", "CombatResultsHeaderText",
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE * 9 * 1.25f - 0.5f * panelWidth,
                (GlobalDefinitions.allCombats.Count + 1.25f) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas).transform.SetParent(combatContentPanel.transform, false);

        foreach (GameObject combat in GlobalDefinitions.allCombats)
        {
            GameObject attackFactorTextGameObject;
            GameObject oddsTextGameObject;

            // The OK button will be at the 1 row position so the combats need to start at the 2nd position
            yPosition = 2 * GlobalDefinitions.GUIUNITIMAGESIZE + GlobalDefinitions.allCombats.IndexOf(combat) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight;

            // This creates images of the defenders.  The first three columns are reserved for defending unit images
            //foreach (GameObject defendingUnit in combat.GetComponent<Combat>().defendingUnits)
            for (int a = 0; ((a < combat.GetComponent<Combat>().defendingUnits.Count) && (a < 3)); a++)
            {
#if OUTPUTDEBUG

                GlobalDefinitions.writeToLogFile("combatResolutionDisplay: unit " + defendingUnit.name + "  x = " + (GlobalDefinitions.GUIUNITIMAGESIZE * combat.GetComponent<Combat>().defendingUnits.IndexOf(defendingUnit) * 1.25f - 0.5f * panelWidth + GlobalDefinitions.GUIUNITIMAGESIZE));
                GlobalDefinitions.writeToLogFile("combatResolutionDisplay: unit index = " + combat.GetComponent<Combat>().defendingUnits.IndexOf(defendingUnit));
#endif
                GlobalDefinitions.createUnitImage(combat.GetComponent<Combat>().defendingUnits[a],
                            "UnitImage",
                            //GlobalDefinitions.GUIUNITIMAGESIZE * combat.GetComponent<Combat>().defendingUnits.IndexOf(defendingUnit) * 1.25f - 0.5f * panelWidth + GlobalDefinitions.GUIUNITIMAGESIZE,
                            GlobalDefinitions.GUIUNITIMAGESIZE * a * 1.25f - 0.5f * panelWidth + GlobalDefinitions.GUIUNITIMAGESIZE,
                            yPosition,
                            combatCanvas).transform.SetParent(combatContentPanel.transform, false);
            }
#if OUTPUTDEBUG
            GlobalDefinitions.writeToLogFile("combatResolutionDisplay: combat number = " + GlobalDefinitions.allCombats.IndexOf(combat) + " defender count = " + combat.GetComponent<Combat>().defendingUnits.Count + " attacker count = " + combat.GetComponent<Combat>().attackingUnits.Count);
#endif
            // In column four the defense factor will be listed
            GlobalDefinitions.createText(GlobalDefinitions.calculateDefenseFactor(combat.GetComponent<Combat>().defendingUnits, combat.GetComponent<Combat>().attackingUnits).ToString(),
                    "DefenseFactorText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 4 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    combatCanvas).transform.SetParent(combatContentPanel.transform, false);

            // In column five the attack factor will be listed
            attackFactorTextGameObject = GlobalDefinitions.createText(GlobalDefinitions.calculateAttackFactor(combat.GetComponent<Combat>().attackingUnits, combat.GetComponent<Combat>().attackAirSupport).ToString(),
                    "AttackFactorText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 5 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    combatCanvas);
            attackFactorTextGameObject.transform.SetParent(combatContentPanel.transform, false);

            // In column six the odds will be listed
            oddsTextGameObject = GlobalDefinitions.createText(GlobalDefinitions.convertOddsToString(GlobalDefinitions.returnCombatOdds(combat.GetComponent<Combat>().defendingUnits, combat.GetComponent<Combat>().attackingUnits, combat.GetComponent<Combat>().attackAirSupport)),
                    "OddsText",
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE * 6 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    combatCanvas);
            oddsTextGameObject.transform.SetParent(combatContentPanel.transform, false);

            // If allied turn, put "Yes" in column seven if carpet bombing is active
            // if it is German turn, put a "Yes" in the column is close defense is active
            if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
            {
                if (checkIfCarpetBombingInEffect(combat.GetComponent<Combat>().defendingUnits))
                    GlobalDefinitions.createText("Yes",
                            "CarpetBombingActiveText",
                            GlobalDefinitions.GUIUNITIMAGESIZE,
                            GlobalDefinitions.GUIUNITIMAGESIZE,
                            GlobalDefinitions.GUIUNITIMAGESIZE * 7 * 1.25f - 0.5f * panelWidth,
                            yPosition,
                            combatCanvas).transform.SetParent(combatContentPanel.transform, false);
            }
            else
            {
                if (checkIfCloseDefenseActive(combat.GetComponent<Combat>().defendingUnits))
                    GlobalDefinitions.createText("Yes",
                            "CloseDefenseActiveText",
                            GlobalDefinitions.GUIUNITIMAGESIZE,
                            GlobalDefinitions.GUIUNITIMAGESIZE,
                            GlobalDefinitions.GUIUNITIMAGESIZE * 7 * 1.25f - 0.5f * panelWidth,
                            yPosition,
                            combatCanvas).transform.SetParent(combatContentPanel.transform, false);
            }

            // In column eight a toggle will be listed to add air support if this is the Allied combat mode
            if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
            {
                combat.GetComponent<Combat>().airSupportToggle = GlobalDefinitions.createToggle("CombatResolutionAirSupportToggle" + GlobalDefinitions.allCombats.IndexOf(combat),
                    GlobalDefinitions.GUIUNITIMAGESIZE * 8 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    combatCanvas).GetComponent<Toggle>();
                combat.GetComponent<Combat>().airSupportToggle.transform.SetParent(combatContentPanel.transform, false);
                combat.GetComponent<Combat>().airSupportToggle.gameObject.AddComponent<CombatResolutionButtonRoutines>();
                combat.GetComponent<Combat>().airSupportToggle.GetComponent<CombatResolutionButtonRoutines>().curentCombat = combat;
                combat.GetComponent<Combat>().airSupportToggle.GetComponent<CombatResolutionButtonRoutines>().attackFactorTextGameObject = attackFactorTextGameObject;
                combat.GetComponent<Combat>().airSupportToggle.GetComponent<CombatResolutionButtonRoutines>().oddsTextGameObject = oddsTextGameObject;


                // The following is needed to set air support when it is already assigned
                // Note I need to do this before I turn on the listener on the toggle otherwise it will count the air mission again and it was already counted when assigned
                if (combat.GetComponent<Combat>().attackAirSupport)
                    combat.GetComponent<Combat>().airSupportToggle.GetComponent<Toggle>().isOn = true;
                else
                    combat.GetComponent<Combat>().airSupportToggle.GetComponent<Toggle>().isOn = false;

                // A separate Toggle object is needed otherwise the Listener won't work without it
                Toggle tempToggle;
                tempToggle = combat.GetComponent<Combat>().airSupportToggle.GetComponent<Toggle>();
                tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<CombatResolutionButtonRoutines>().addAttackAirSupport());

                if (!playerControl)
                    tempToggle.interactable = false;
            }

            // In column nine add a button to resolve the combat
            combat.GetComponent<Combat>().resolveButton = GlobalDefinitions.createButton("CombatResolutionResolveButton" + GlobalDefinitions.allCombats.IndexOf(combat), "Resolve",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 9 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    combatCanvas);
            combat.GetComponent<Combat>().resolveButton.transform.SetParent(combatContentPanel.transform, false);

            if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanCombatStateInstance"))
            {
                // Combat mode is the only phase that the resolve button should be active
                combat.GetComponent<Combat>().resolveButton.gameObject.AddComponent<CombatResolutionButtonRoutines>();
                combat.GetComponent<Combat>().resolveButton.GetComponent<CombatResolutionButtonRoutines>().curentCombat = combat;
                combat.GetComponent<Combat>().resolveButton.onClick.AddListener(combat.GetComponent<Combat>().resolveButton.GetComponent<CombatResolutionButtonRoutines>().resolutionSelected);
            }
            else
                combat.GetComponent<Combat>().resolveButton.interactable = false;

            // In column ten add a button to locate the combat
            combat.GetComponent<Combat>().locateButton = GlobalDefinitions.createButton("CombatResolutionLocateButton" + GlobalDefinitions.allCombats.IndexOf(combat), "Locate",
                   GlobalDefinitions.GUIUNITIMAGESIZE * 10 * 1.25f - 0.5f * panelWidth,
                   yPosition,
                   combatCanvas);
            combat.GetComponent<Combat>().locateButton.transform.SetParent(combatContentPanel.transform, false);
            combat.GetComponent<Combat>().locateButton.gameObject.AddComponent<CombatResolutionButtonRoutines>();
            combat.GetComponent<Combat>().locateButton.GetComponent<CombatResolutionButtonRoutines>().curentCombat = combat;
            combat.GetComponent<Combat>().locateButton.onClick.AddListener(combat.GetComponent<Combat>().locateButton.GetComponent<CombatResolutionButtonRoutines>().locateAttack);

            // In column eleven add a button to cancel the combat
            combat.GetComponent<Combat>().cancelButton = GlobalDefinitions.createButton("CombatResolutionCamcelButton" + GlobalDefinitions.allCombats.IndexOf(combat), "Cancel",
                    GlobalDefinitions.GUIUNITIMAGESIZE * 11 * 1.25f - 0.5f * panelWidth,
                    yPosition,
                    combatCanvas);
            combat.GetComponent<Combat>().cancelButton.transform.SetParent(combatContentPanel.transform, false);
            combat.GetComponent<Combat>().cancelButton.gameObject.AddComponent<CombatResolutionButtonRoutines>();
            combat.GetComponent<Combat>().cancelButton.GetComponent<CombatResolutionButtonRoutines>().curentCombat = combat;
            combat.GetComponent<Combat>().cancelButton.onClick.AddListener(combat.GetComponent<Combat>().cancelButton.GetComponent<CombatResolutionButtonRoutines>().cancelAttack);

            if (!playerControl)
                combat.GetComponent<Combat>().cancelButton.interactable = false;
        }

        // Need an OK button to get out of the GUI
        okButton = GlobalDefinitions.createButton("CombatResolutionOKButton", "Continue",
                7 * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas);
        okButton.transform.SetParent(combatContentPanel.transform, false);

        okButton.gameObject.AddComponent<CombatResolutionButtonRoutines>();
        okButton.onClick.AddListener(okButton.GetComponent<CombatResolutionButtonRoutines>().ok);
        GlobalDefinitions.combatResolutionOKButton = okButton.gameObject;
        GlobalDefinitions.combatResolutionOKButton.SetActive(true);

        // If this is the resolution of the AI combats, remove the Continue button because the player cannot add combats for the AI
        if (!GlobalDefinitions.AICombat)
            GlobalDefinitions.combatResolutionOKButton.SetActive(true);
        else
            GlobalDefinitions.combatResolutionOKButton.SetActive(false);
    }

    /// <summary>
    /// This is the routine that determines the outcome of a combat
    /// </summary>
    /// <param name="defendingUnits"></param>
    /// <param name="attackingUnits"></param>
    /// <param name="arrayIndex"></param>
    public static void determineCombatResults(GameObject currentCombat, Vector2 buttonLocation)
    {
        // I originally kept the die roll results local here.  With network play though I need to drive this off global variables so that I can normalize the 
        // code for both network and local and the network computer works off the local computer's results
        string combatOdds;

        Canvas carpetBombingCanvasInstance = new Canvas();
        Toggle tempToggle1;
        Toggle tempToggle2;

        combatOdds = GlobalDefinitions.convertOddsToString(GlobalDefinitions.returnCombatOdds(currentCombat.GetComponent<Combat>().defendingUnits,
                currentCombat.GetComponent<Combat>().attackingUnits, currentCombat.GetComponent<Combat>().attackAirSupport));
#if OUTPUTDEBUG
        GlobalDefinitions.writeToLogFile("Combat Results: Odds " + combatOdds);
#endif
        if ((GlobalDefinitions.gameMode != GlobalDefinitions.GameModeValues.Network) && !GlobalDefinitions.commandFileBeingRead)
        {
            GlobalDefinitions.dieRollResult1 = checkForDieRollInfluence(GlobalDefinitions.dieRoll.Next(0, 5));
            GlobalDefinitions.dieRollResult1 = 3;  // REMOVE - FOR TESTING ONLY
            // 1:1 odds results 0-Delim 1-Exchange 2-Dback2 3-Aback2 4-Aelim 5-Aelim 

            GlobalDefinitions.dieRollResult2 = checkForDieRollInfluence(GlobalDefinitions.dieRoll.Next(0, 5));
        }

        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.DIEROLLRESULT1KEYWORD + " " + GlobalDefinitions.dieRollResult1);
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.DIEROLLRESULT2KEYWORD + " " + GlobalDefinitions.dieRollResult2);

#if OUTPUTDEBUG
        GlobalDefinitions.guiUpdateStatusMessage("Combat Results: die roll result 1 = " + GlobalDefinitions.dieRollResult1);
        GlobalDefinitions.guiUpdateStatusMessage("Combat Results: die roll result 2 = " + GlobalDefinitions.dieRollResult2);
#endif

        GlobalDefinitions.guiUpdateStatusMessage("Combat Results: Odds " + combatOdds + "  Die Roll " + (GlobalDefinitions.dieRollResult1 + 1) + "   which translates to " + GlobalDefinitions.combatResultsTable[translateCombatOddsToArrayIndex(combatOdds), GlobalDefinitions.dieRollResult1]);

        if (!checkIfCarpetBombingInEffect(currentCombat.GetComponent<Combat>().defendingUnits))
        {
            // This is the path for combat results without carpet bombing ... 99.9999% of the time
            executeCombatResults(currentCombat.GetComponent<Combat>().defendingUnits,
                    currentCombat.GetComponent<Combat>().attackingUnits,
                    combatOdds,
                    GlobalDefinitions.dieRollResult1,
                    GlobalDefinitions.combatResultsTable[translateCombatOddsToArrayIndex(combatOdds), GlobalDefinitions.dieRollResult1],
                    buttonLocation);
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.COMBATRESOLUTIONSELECTEDKEYWORD + " " + GlobalDefinitions.CombatResultToggleName);

        }
        else
        {
            // Carpet bombing is active for the hex being attacked so present the user with the two options for selection

            // Get rid off the combat result gui
            GlobalDefinitions.combatResolutionGUIInstance.SetActive(false);

            GlobalDefinitions.guiUpdateStatusMessage("Combat Results2: Odds " + combatOdds + "  Die Roll " + (GlobalDefinitions.dieRollResult2 + 1) + "   which translates to " + GlobalDefinitions.combatResultsTable[translateCombatOddsToArrayIndex(combatOdds), GlobalDefinitions.dieRollResult2]);

            // Create a GUI and present the user with the two die roll results for selection.  The toggle routine will call executeCombatResults once the user selects the result so need to load up all the
            // variables it will need to pass

            float panelWidth = 5 * GlobalDefinitions.GUIUNITIMAGESIZE;
            float panelHeight = 3 * GlobalDefinitions.GUIUNITIMAGESIZE;
            GlobalDefinitions.createGUICanvas("CarpetBombingResultSelectionGUIInstance",
                    panelWidth,
                    panelHeight,
                    ref carpetBombingCanvasInstance);

            GlobalDefinitions.createText("Select a Result", "CarpetBombingGUIHeaderText",
                    3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    carpetBombingCanvasInstance);

            tempToggle1 = GlobalDefinitions.createToggle("CarpetBombingToggle1",
                    1 * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    carpetBombingCanvasInstance).GetComponent<Toggle>();
            GlobalDefinitions.createText(convertResultsToString(GlobalDefinitions.combatResultsTable[translateCombatOddsToArrayIndex(combatOdds), GlobalDefinitions.dieRollResult1]), "CarpetBombingResultText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    1 * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    carpetBombingCanvasInstance);

            tempToggle2 = GlobalDefinitions.createToggle("CarpetBombingToggle2",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    carpetBombingCanvasInstance).GetComponent<Toggle>();
            GlobalDefinitions.createText(convertResultsToString(GlobalDefinitions.combatResultsTable[translateCombatOddsToArrayIndex(combatOdds), GlobalDefinitions.dieRollResult2]), "CarpetBombingResultText",
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    carpetBombingCanvasInstance);

            tempToggle1.gameObject.AddComponent<CarpetBombingSelectionToggleRoutines>();
            tempToggle2.gameObject.AddComponent<CarpetBombingSelectionToggleRoutines>();

            tempToggle1.GetComponent<CarpetBombingSelectionToggleRoutines>().defendingUnits = currentCombat.GetComponent<Combat>().defendingUnits;
            tempToggle2.GetComponent<CarpetBombingSelectionToggleRoutines>().defendingUnits = currentCombat.GetComponent<Combat>().defendingUnits;

            tempToggle1.GetComponent<CarpetBombingSelectionToggleRoutines>().attackingUnits = currentCombat.GetComponent<Combat>().attackingUnits;
            tempToggle2.GetComponent<CarpetBombingSelectionToggleRoutines>().attackingUnits = currentCombat.GetComponent<Combat>().attackingUnits;

            tempToggle1.GetComponent<CarpetBombingSelectionToggleRoutines>().combatOdds = combatOdds;
            tempToggle2.GetComponent<CarpetBombingSelectionToggleRoutines>().combatOdds = combatOdds;

            tempToggle1.GetComponent<CarpetBombingSelectionToggleRoutines>().dieRollResult = GlobalDefinitions.dieRollResult1;
            tempToggle2.GetComponent<CarpetBombingSelectionToggleRoutines>().dieRollResult = GlobalDefinitions.dieRollResult2;

            tempToggle1.GetComponent<CarpetBombingSelectionToggleRoutines>().combatResults = GlobalDefinitions.combatResultsTable[translateCombatOddsToArrayIndex(combatOdds), GlobalDefinitions.dieRollResult1];
            tempToggle2.GetComponent<CarpetBombingSelectionToggleRoutines>().combatResults = GlobalDefinitions.combatResultsTable[translateCombatOddsToArrayIndex(combatOdds), GlobalDefinitions.dieRollResult2];

            tempToggle1.GetComponent<CarpetBombingSelectionToggleRoutines>().buttonLocation = buttonLocation;
            tempToggle2.GetComponent<CarpetBombingSelectionToggleRoutines>().buttonLocation = buttonLocation;

            tempToggle1.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => tempToggle1.GetComponent<CarpetBombingSelectionToggleRoutines>().carpetBombingResultsSelected());
            tempToggle2.GetComponent<Toggle>().onValueChanged.AddListener((bool value) => tempToggle2.GetComponent<CarpetBombingSelectionToggleRoutines>().carpetBombingResultsSelected());
        }
    }

    /// <summary>
    /// Processes the result of the combat passed
    /// </summary>
    /// <param name="defendingUnits"></param>
    /// <param name="attackingUnits"></param>
    /// <param name="combatOdds"></param>
    /// <param name="dieRollResult"></param>
    /// <param name="combatResults"></param>
    /// <param name="buttonLocation"></param>
    public static void executeCombatResults(List<GameObject> defendingUnits, List<GameObject> attackingUnits, string combatOdds, int dieRollResult,
            GlobalDefinitions.CombatResults combatResults, Vector2 buttonLocation)
    {
        // The combat results from last turn are only saved for the Allied player because it is used by the AI to determine carpet bombing and airborne attacks
        if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
            GlobalDefinitions.combatResultsFromLastTurn.Add(combatResults);

        // The added text below is to put the combat results in the Combat Results GUI in place of the deleted Resolve button
        GlobalDefinitions.createText(convertResultsToString(GlobalDefinitions.combatResultsTable[translateCombatOddsToArrayIndex(combatOdds), dieRollResult]), "CombatResolutionText",
                1.4f * GlobalDefinitions.GUIUNITIMAGESIZE,
                GlobalDefinitions.GUIUNITIMAGESIZE,
                buttonLocation.x,
                buttonLocation.y,
                GlobalDefinitions.combatResolutionGUIInstance.GetComponent<Canvas>()).transform.SetParent(GlobalDefinitions.combatContentPanel.transform, false);

        switch (combatResults)
        {
            case GlobalDefinitions.CombatResults.Aback2:

                // Eliminate the attackers that can't move and store the ones that can
                GlobalDefinitions.hexesAvailableForPostCombatMovement.Clear();
                GlobalDefinitions.retreatingUnits.Clear();
                GlobalDefinitions.retreatingUnitsBeginningHexes.Clear();
                foreach (GameObject unit in attackingUnits)
                {
                    GlobalDefinitions.retreatingUnitsBeginningHexes.Add(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                    if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea || !unitCanRetreat(unit))
                    {
                        // This means that the unit can't retreat so it will be removed before all the checks are run.
                        // Otherwise (the way it was first written, the unit would be deleted after the user selects it.
                        // This seems silly when it is clear that the unit can't retreat.
                        GlobalDefinitions.guiUpdateStatusMessage("No retreat available - eliminating unit " + unit.GetComponent<UnitDatabaseFields>().unitDesignation);
                        GlobalDefinitions.moveUnitToDeadPile(unit);
                    }
                    else
                    {
                        GlobalDefinitions.retreatingUnits.Add(unit);
                    }
                }

                // Don't need to worry about executing anything if all the attackers didn't have retreat options
                if (GlobalDefinitions.retreatingUnits.Count > 0)
                {

                    if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
                            && (GlobalDefinitions.nationalityUserIsPlaying == attackingUnits[0].GetComponent<UnitDatabaseFields>().nationality))
                        AIRoutines.executeAIAback2(GlobalDefinitions.retreatingUnits);

                    else
                    {
                        //foreach (GameObject unit in GlobalDefinitions.retreatingUnits)
                        //    GlobalDefinitions.highlightUnit(unit);

                        // We are going to need to get user input to move the units back so disable the combat resolution gui
                        GlobalDefinitions.combatResolutionGUIInstance.SetActive(false);
                        selectUnitsForRetreat();

                    }
                }
                break;

            case GlobalDefinitions.CombatResults.Aelim:
                foreach (GameObject unit in attackingUnits)
                    GlobalDefinitions.moveUnitToDeadPile(unit);
                // If we'er coming from the carpet bombing option, need to set the gui active again
                GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
                break;

            case GlobalDefinitions.CombatResults.Dback2:
                // Store the oringal defender factors.  Since the defenders will be retreating we cannot find out if the attackers are
                // able to occupy the hex since all the routines will calculate the odds from the hex where the defenders have retreated

                // Store off the attackers and defenders so that we can check if the attackers can occupy the hex after the defenders have moved
                GlobalDefinitions.dback2Attackers.Clear();
                GlobalDefinitions.dback2Defenders.Clear();
                GlobalDefinitions.retreatingUnits.Clear();
                GlobalDefinitions.retreatingUnitsBeginningHexes.Clear();
                foreach (GameObject unit in defendingUnits)
                    GlobalDefinitions.dback2Defenders.Add(unit);
                foreach (GameObject unit in attackingUnits)
                    GlobalDefinitions.dback2Attackers.Add(unit);

                loadHexesAvailableForPostCombatMovement(attackingUnits, defendingUnits);

                // Store and highlight the defenders that need to be moved
                foreach (GameObject unit in defendingUnits)
                {
                    GlobalDefinitions.retreatingUnitsBeginningHexes.Add(unit.GetComponent<UnitDatabaseFields>().occupiedHex);
                    if (!unitCanRetreat(unit))
                    {
                        // This means that the unit can't retreat so it will be removed before all the checks are run.
                        // Otherwise (the way it was first written, the unit would be deleted after the user selects it.
                        // This seems silly when it is clear that the unit can't retreat.
                        GlobalDefinitions.guiUpdateStatusMessage("No retreat available - eliminating unit " + unit.GetComponent<UnitDatabaseFields>().unitDesignation);
                        GlobalDefinitions.moveUnitToDeadPile(unit);
                    }
                    else
                    {
                        GlobalDefinitions.retreatingUnits.Add(unit);
                    }
                }

                if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
                    && (GlobalDefinitions.nationalityUserIsPlaying == defendingUnits[0].GetComponent<UnitDatabaseFields>().nationality))
                {
                    AIRoutines.executeAIDback2(GlobalDefinitions.retreatingUnits);
                }
                else
                {
                    selectUnitsForRetreat();
                }
                break;

            case GlobalDefinitions.CombatResults.Delim:
                // Need to determine if the attackers can occupy the defenders vacated hex
                loadHexesAvailableForPostCombatMovement(attackingUnits, defendingUnits);

                foreach (GameObject unit in defendingUnits)
                    GlobalDefinitions.moveUnitToDeadPile(unit);

                if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
                        && (GlobalDefinitions.nationalityUserIsPlaying == defendingUnits[0].GetComponent<UnitDatabaseFields>().nationality))
                {
                    if (GlobalDefinitions.hexesAvailableForPostCombatMovement.Count > 0)
                        AIRoutines.executeAIPostCombatMovement(attackingUnits);
                }
                else
                {
                    if (GlobalDefinitions.hexesAvailableForPostCombatMovement.Count > 0)
                        if (attackingUnits.Count > 0)
                            selectUnitsForPostCombatMovement(attackingUnits);
                        else
                            // If we're coming from the carpet bombing option, need to set the gui active again
                            GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
                }
                break;

            // Resolve exchange combat results
            case GlobalDefinitions.CombatResults.Exchange:
                if (GlobalDefinitions.calculateDefenseFactorWithoutAirSupport(defendingUnits, attackingUnits) == GlobalDefinitions.calculateAttackFactorWithoutAirSupport(attackingUnits))
                {
                    // No need to bother the user since the attack and defense factors match
                    foreach (GameObject unit in attackingUnits)
                        GlobalDefinitions.moveUnitToDeadPile(unit);
                    foreach (GameObject unit in defendingUnits)
                        GlobalDefinitions.moveUnitToDeadPile(unit);
                    // In case we're coming from the carpet bombing option, need to set the gui active again
                    GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
                }
                // This executes if the attacker had the greater number of factors
                else if (GlobalDefinitions.calculateDefenseFactorWithoutAirSupport(defendingUnits, attackingUnits) < GlobalDefinitions.calculateAttackFactorWithoutAirSupport(attackingUnits))
                {
                    loadHexesAvailableForPostCombatMovement(attackingUnits, defendingUnits);
                    // If there is only one attacker, don't need to bother the user
                    if (attackingUnits.Count == 1)
                    {
                        GlobalDefinitions.moveUnitToDeadPile(attackingUnits[0]);
                        foreach (GameObject unit in defendingUnits)
                            GlobalDefinitions.moveUnitToDeadPile(unit);
                        // In case we're coming from the carpet bombing option, need to set the gui active again
                        GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
                    }
                    else
                    {
                        GlobalDefinitions.exchangeFactorsToLose = GlobalDefinitions.calculateDefenseFactorWithoutAirSupport(defendingUnits, attackingUnits);
                        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
                                && (GlobalDefinitions.nationalityUserIsPlaying == defendingUnits[0].GetComponent<UnitDatabaseFields>().nationality))
                        {
                            GlobalDefinitions.writeToLogFile("executeCombatResults: executing executeAIExchangeForAttackingUnits");
                            AIRoutines.executeAIExchangeForAttackingUnits(attackingUnits, defendingUnits);
                        }
                        else
                        {
                            // Pull up a GUI to have the attacker determine what units will be exchanged
                            GlobalDefinitions.writeToLogFile("executeCombatResults: executing selectUnitsToExchange attacker has greater factors");
                            selectUnitsToExchange(attackingUnits, true, attackingUnits, defendingUnits);
                            GlobalDefinitions.combatResolutionGUIInstance.SetActive(false);
                        }
                    }
                }

                // This executes if the defender had the greater number of factors
                else
                {
                    // If there is only one defender, don't need to bother the user
                    if (defendingUnits.Count == 1)
                    {
                        GlobalDefinitions.moveUnitToDeadPile(defendingUnits[0]);
                        foreach (GameObject unit in attackingUnits)
                            GlobalDefinitions.moveUnitToDeadPile(unit);
                        // If we're coming from the carpet bombing option, need to set the gui active again
                        GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
                    }
                    else
                    {
                        GlobalDefinitions.exchangeFactorsToLose = (int)GlobalDefinitions.calculateAttackFactorWithoutAirSupport(attackingUnits);
                        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
                                && (GlobalDefinitions.nationalityUserIsPlaying == attackingUnits[0].GetComponent<UnitDatabaseFields>().nationality))
                        {
                            GlobalDefinitions.writeToLogFile("executeCombatResults: executing executeAIExchangeForDefendingUnits");
                            AIRoutines.executeAIExchangeForDefendingUnits(attackingUnits, defendingUnits);
                        }
                        else
                        {
                            // Pull up a GUI to have the defender determine what units will be exchanged
                            GlobalDefinitions.writeToLogFile("executeCombatResults: executing selectUnitsToExchange defense has greater factors");
                            selectUnitsToExchange(defendingUnits, false, attackingUnits, defendingUnits);
                            GlobalDefinitions.combatResolutionGUIInstance.SetActive(false);

                        }
                    }
                }
                break;

            default:
                GlobalDefinitions.guiUpdateStatusMessage("Internal Error - Unknown combat result - " + convertResultsToString(combatResults));
                break;
        }
    }

    /// <summary>
    /// Calls up the gui for the user to select the units to meet a combat exchange result
    /// </summary>
    /// <param name="unitList"></param>
    /// <param name="attackerHadMostFactors"></param>
    /// <param name="attackingUnits"></param>
    /// <param name="defendingUnits"></param>
    private static void selectUnitsToExchange(List<GameObject> unitList, bool attackerHadMostFactors, List<GameObject> attackingUnits, List<GameObject> defendingUnits)
    {
        Button okButton;
        Canvas combatCanvas = new Canvas();
        GameObject exchangeGuiInstance;
        int maxWidth;

        GlobalDefinitions.exchangeFactorsSelected = 0;

        // The panel needs to be a minimum size in order to make sure it is wider than the text
        if ((unitList.Count) > 3)
            maxWidth = unitList.Count;
        else
            maxWidth = 3;

        float panelWidth = (maxWidth + 1) * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 5 * GlobalDefinitions.GUIUNITIMAGESIZE;

        exchangeGuiInstance = GlobalDefinitions.createGUICanvas("ExchangeGUIInstance",
                panelWidth,
                panelHeight,
                ref combatCanvas);

        GlobalDefinitions.ExchangeGUIInstance = exchangeGuiInstance;

        GlobalDefinitions.createText("Select " + GlobalDefinitions.exchangeFactorsToLose + " factors\nFactors selected so far: " + GlobalDefinitions.exchangeFactorsSelected, "ExchangeText",
                4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                0.5f * (maxWidth + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                4.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas);

        float xSeperation = (maxWidth + 1) * GlobalDefinitions.GUIUNITIMAGESIZE / unitList.Count;
        float xOffset = xSeperation / 2;
        for (int index = 0; index < unitList.Count; index++)
        {
            Toggle tempToggle;
            tempToggle = GlobalDefinitions.createUnitTogglePair("ExchangeUnitToggle" + index,
                    index * xSeperation + xOffset - 0.5f * panelWidth,
                    2 * GlobalDefinitions.GUIUNITIMAGESIZE + 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas,
                    unitList[index]);
            tempToggle.gameObject.AddComponent<ExchangeToggleRoutines>();
            tempToggle.gameObject.GetComponent<ExchangeToggleRoutines>().unit = unitList[index];
            tempToggle.gameObject.GetComponent<ExchangeToggleRoutines>().attacker = attackerHadMostFactors; // Used in the toggle routines to determine if attackers or defenders are being selected
            tempToggle.gameObject.GetComponent<ExchangeToggleRoutines>().attackingUnits = attackingUnits;
            tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<ExchangeToggleRoutines>().addOrSubtractExchangeFactors());
        }

        okButton = GlobalDefinitions.createButton("ExchangeOKButton", "OK",
                0.5f * (maxWidth + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas);
        okButton.gameObject.AddComponent<ExchangeOKRoutines>();
        okButton.gameObject.GetComponent<ExchangeOKRoutines>().defendingUnits = defendingUnits;
        okButton.gameObject.GetComponent<ExchangeOKRoutines>().attackingUnits = attackingUnits;
        okButton.gameObject.GetComponent<ExchangeOKRoutines>().attackerHadMostFactors = attackerHadMostFactors;
        okButton.onClick.AddListener(okButton.GetComponent<ExchangeOKRoutines>().exchangeOKSelected);
    }

    /// <summary>
    /// Presents a gui for post combat movement
    /// </summary>
    /// <param name="unitList"></param>
    public static void selectUnitsForPostCombatMovement(List<GameObject> unitList)
    {
        Button okButton;
        int widthSeed;

        GlobalDefinitions.combatResolutionGUIInstance.SetActive(false);

        Canvas combatCanvas = new Canvas();

        // The panel needs to be a minimum size in order to make sure it is wider than the text
        if ((unitList.Count + 2) > 6)
            widthSeed = (unitList.Count + 2);
        else
            widthSeed = 6;

        float panelWidth = widthSeed * GlobalDefinitions.GUIUNITIMAGESIZE;
        float panelHeight = 4 * GlobalDefinitions.GUIUNITIMAGESIZE;
        GameObject postCombatMovementGuiInstance = GlobalDefinitions.createGUICanvas("PostCombatMovementGUIInstance",
                panelWidth,
                panelHeight,
                ref combatCanvas);
        GlobalDefinitions.postCombatMovementGuiInstance = postCombatMovementGuiInstance;

        GlobalDefinitions.createText("Select units to occupy the vacated hex", "PostCombatMovementText",
                widthSeed * GlobalDefinitions.GUIUNITIMAGESIZE,
                2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                widthSeed * GlobalDefinitions.GUIUNITIMAGESIZE / 2 - 0.5f * panelWidth,
                3.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas);

        float xSeperation = widthSeed * GlobalDefinitions.GUIUNITIMAGESIZE / unitList.Count;
        float xOffset = xSeperation / 2;
        for (int index = 0; index < unitList.Count; index++)
        {
            Toggle tempToggle;

            tempToggle = GlobalDefinitions.createUnitTogglePair("PostCombatMovementUnitToggle" + index,
                    index * xSeperation + xOffset - 0.5f * panelWidth,
                    2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas,
                    unitList[index]);

            tempToggle.gameObject.AddComponent<PostCombatMovementToggleRoutines>();
            tempToggle.GetComponent<PostCombatMovementToggleRoutines>().unit = unitList[index];
            tempToggle.GetComponent<PostCombatMovementToggleRoutines>().beginningHex = unitList[index].GetComponent<UnitDatabaseFields>().occupiedHex;
            if (unitList[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                tempToggle.GetComponent<PostCombatMovementToggleRoutines>().stackingLimit = GlobalDefinitions.AlliedStackingLimit;
            else
                tempToggle.GetComponent<PostCombatMovementToggleRoutines>().stackingLimit = GlobalDefinitions.GermanStackingLimit;
            tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<PostCombatMovementToggleRoutines>().moveSelectedUnit());
        }

        okButton = GlobalDefinitions.createButton("PostCombatMovementButton", "OK",
                widthSeed * GlobalDefinitions.GUIUNITIMAGESIZE / 2 - 0.5f * panelWidth,
                0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                combatCanvas);
        okButton.gameObject.AddComponent<PostCombatMovementOkRoutines>();
        okButton.onClick.AddListener(okButton.GetComponent<PostCombatMovementOkRoutines>().executePostCombatMovement);
    }

    /// <summary>
    /// This routine will go through the units that need to retreat and get user input to determine where they will retreat to
    /// This executes until there are no more retreating units left since after the user selects a hex destination it calls this routine again
    /// </summary>
    public static void selectUnitsForRetreat()
    {
        // This is for the case where the retreating units have been eliminated and the attackers can occupy the vacant hexes
        if (GlobalDefinitions.retreatingUnits.Count == 0)
        {
            if (GlobalDefinitions.hexesAvailableForPostCombatMovement.Count > 0)
            {
                if (GlobalDefinitions.dback2Attackers.Count > 0)
                    selectUnitsForPostCombatMovement(GlobalDefinitions.dback2Attackers);
            }
            else
            {
                GlobalDefinitions.dback2Attackers.Clear();
                GlobalDefinitions.dback2Defenders.Clear();
                GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
            }
        }

        else if (GlobalDefinitions.retreatingUnits.Count == 1)
        {
            // If there is only one unit don't need to have a gui, just need to have the user choose the retreat hex
            // if highlightRetreatMovement comes back with false it means that there was no retreat available and the unit was deleted
            List<GameObject> retreatHexes = returnRetreatHexes(GlobalDefinitions.retreatingUnits[0]);
            if (retreatHexes.Count > 0)
            {
                GlobalDefinitions.combatResolutionGUIInstance.SetActive(false);
                GlobalDefinitions.highlightUnit(GlobalDefinitions.retreatingUnits[0]);
                foreach (GameObject hex in retreatHexes)
                    GlobalDefinitions.highlightHexForMovement(hex);
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<CombatState>().executeRetreatMovement;
            }
            else
            {
                // This executes when there are not enough retreat hexes for all the units that need to retreat
                GlobalDefinitions.guiUpdateStatusMessage("No retreat available - eliminating " + GlobalDefinitions.retreatingUnits[0].name);
                GlobalDefinitions.moveUnitToDeadPile(GlobalDefinitions.retreatingUnits[0]);

                if ((GlobalDefinitions.hexesAvailableForPostCombatMovement.Count > 0) && (GlobalDefinitions.dback2Attackers.Count > 0))
                    selectUnitsForPostCombatMovement(GlobalDefinitions.dback2Attackers);
                else
                    GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
            }
        }
        else
        {
            GlobalDefinitions.combatResolutionGUIInstance.SetActive(false);

            Canvas combatCanvas = new Canvas();

            float panelWidth = (GlobalDefinitions.retreatingUnits.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE;
            float panelHeight = 3 * GlobalDefinitions.GUIUNITIMAGESIZE;
            GlobalDefinitions.createGUICanvas("RetreatGUIInstance",
                    panelWidth,
                    panelHeight,
                    ref combatCanvas);

            GlobalDefinitions.createText("Select unit to retreat", "RetreatText",
                    3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    1 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    0.5f * (GlobalDefinitions.retreatingUnits.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                    combatCanvas);

            float xSeperation = (GlobalDefinitions.retreatingUnits.Count + 1) * GlobalDefinitions.GUIUNITIMAGESIZE / GlobalDefinitions.retreatingUnits.Count;
            float xOffset = xSeperation / 2;
            for (int index = 0; index < GlobalDefinitions.retreatingUnits.Count; index++)
            {
                Toggle tempToggle;

                tempToggle = GlobalDefinitions.createUnitTogglePair("RetreatUnitToggle" + index,
                        index * xSeperation + xOffset - 0.5f * panelWidth,
                        1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                        combatCanvas,
                        GlobalDefinitions.retreatingUnits[index]);

                tempToggle.gameObject.AddComponent<RetreatToggleRoutines>();
                tempToggle.GetComponent<RetreatToggleRoutines>().unit = GlobalDefinitions.retreatingUnits[index];
                tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<RetreatToggleRoutines>().selectUnitsToMove());
            }
        }
    }

    /// <summary>
    /// Returns the avaialable retreat hexes for the unit passed
    /// </summary>
    /// <param name="retreatingUnit"></param>
    /// <returns></returns>
    public static List<GameObject> returnRetreatHexes(GameObject retreatingUnit)
    {
        // The process that will be used to check retreat is to execute two movement sections of one hex at a time.  This is needed to account
        // for the fact that the movement doesn't need to be efficient.  A retreat of two hexes can leave a unit only one hex away from where
        // it started.
        List<GameObject> firstMoveHexes = new List<GameObject>();
        List<GameObject> retreatHexes = new List<GameObject>();
        retreatingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().remainingMovement = 1;
        foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            if (retreatingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                if (checkForRetreatMovementAvailableNoStackingRestrictions(retreatingUnit.GetComponent<UnitDatabaseFields>().occupiedHex,
                        retreatingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide], retreatingUnit))
                    firstMoveHexes.Add(retreatingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);

        // The above loop does the check for the first move.  Now take all the stored first move hexes and check for another one hex movement
        foreach (GameObject hex in firstMoveHexes)
        {
            hex.GetComponent<HexDatabaseFields>().remainingMovement = 1;
            foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                if (hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                    if (checkForRetreatMovementAvailable(hex, hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide], retreatingUnit))
                        retreatHexes.Add(hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
        }

        return (retreatHexes);
    }

    /// <summary>
    /// Returns the avaialable retreat hexes for the unit passed
    /// </summary>
    /// <param name="initialHexToCheck"></param>
    /// <param name="retreatingUnit"></param>
    /// <returns></returns>
    public static bool highlightRetreatMovement(GameObject retreatingUnit, bool hexesShouldBeHighlighted)
    {
        List<GameObject> storedHexes = new List<GameObject>();
        // The process that will be used to check retreat is to execute two movement sections of one hex at a time.  This is needed to account
        // for the fact that the movement doesn't need to be efficient.  A retreat of two hexes can leave a unit only one hex away from where
        // it started.
        List<GameObject> hexesToCheck = new List<GameObject>();
        List<GameObject> firstMoveHexes = new List<GameObject>();
        GameObject initialHexToCheck = retreatingUnit.GetComponent<UnitDatabaseFields>().occupiedHex;
        initialHexToCheck.GetComponent<HexDatabaseFields>().remainingMovement = 1;
        hexesToCheck.Add(initialHexToCheck);
        bool storeHex;
        while (hexesToCheck.Count > 0)
        {
            foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            {
                if (hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                {
                    storeHex = false;

                    if (checkForRetreatMovementAvailableNoStackingRestrictions(hexesToCheck[0], hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide], retreatingUnit))
                        storeHex = true;

                    // See if the current neighbor needs to be popped to the stack for checking
                    if (storeHex && !storedHexes.Contains(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]))
                    {
                        hexesToCheck.Add(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                        firstMoveHexes.Add(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                    }
                }
            }
            hexesToCheck.RemoveAt(0);
        }

        // The above loop does the check for the first move.  Now take all the stored first move hexes and check for another one hex movement
        foreach (GameObject hex in firstMoveHexes)
        {
            hex.GetComponent<HexDatabaseFields>().remainingMovement = 1;
            hexesToCheck.Add(hex);
            while (hexesToCheck.Count > 0)
            {
                foreach (GlobalDefinitions.HexSides hexSide in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                {
                    if (hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide] != null)
                    {
                        storeHex = false;

                        if (checkForRetreatMovementAvailable(hexesToCheck[0], hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide], retreatingUnit))
                            storeHex = true;

                        // See if the current neighbor needs to be popped to the stack for checking
                        if (storeHex && !storedHexes.Contains(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]))
                        {
                            hexesToCheck.Add(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                            storedHexes.Add(hexesToCheck[0].GetComponent<HexDatabaseFields>().Neighbors[(int)hexSide]);
                        }
                    }
                }
                hexesToCheck.RemoveAt(0);
            }
        }

        // A retreating unit must be able to retreat two hexes.  If the stored hex has a remaining movement of 1 on it then that means it was the first move.
        // If they all have greater than 0 then that means the unit was only able to retreat one hex and should be removed
        bool retreatPossible = false;
        foreach (GameObject hex in storedHexes)
            if (hex.GetComponent<HexDatabaseFields>().remainingMovement == 0)
                retreatPossible = true;
        if (!retreatPossible)
            return (false);
        else
            return (true);
    }

    /// <summary>
    /// This routine is used in the beginning to determine if a unit is avaialble to retreat.  It cleans up the 
    /// hexes that are set by highlightRetreatMovement before returning.  It is used at the start of the process
    /// to eleiminate units that cannot retreat.  Note this does not solve the issue where there are two units
    /// to retreat but only room for one.
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static bool unitCanRetreat(GameObject unit)
    {
        if (returnRetreatHexes(unit).Count > 0)
            return (true);
        else
            return (false);
    }

    /// <summary>
    /// This routine determines if the unit passed can retreat from the begining hex to the destination hex
    /// </summary>
    /// <param name="beginningHex"></param>
    /// <param name="destinationHex"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    private static bool checkForRetreatMovementAvailable(GameObject beginningHex, GameObject destinationHex, GameObject unit)
    {
        if (!checkForRetreatMovementAvailableNoStackingRestrictions(beginningHex, destinationHex, unit))
            // Even without stacking limits the move is not possible
            return (false);
        else
        {
            //Check if there is a stacking limitation for occupying the hex and also check that it isn't a bridge hex
            if (GlobalDefinitions.hexUnderStackingLimit(destinationHex, unit.GetComponent<UnitDatabaseFields>().nationality) &&
                    !destinationHex.GetComponent<HexDatabaseFields>().bridge)
            {
                destinationHex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                return (true);
            }
        }
        return (false);
    }

    /// <summary>
    /// This routine checks if a unit can retreat between the hexes passed without enforcing any stacking limits
    /// </summary>
    /// <param name="beginningHex"></param>
    /// <param name="destinationHex"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    private static bool checkForRetreatMovementAvailableNoStackingRestrictions(GameObject beginningHex, GameObject destinationHex, GameObject unit)
    {
        // First check if there is any remaining movement cost available from the start hex
        if (beginningHex.GetComponent<HexDatabaseFields>().remainingMovement == 0)
            return (false);

        // Check if the hex was a hex used to execute the attack
        if (GlobalDefinitions.retreatingUnitsBeginningHexes.Contains(destinationHex))
            return (false);

        // Check if the unit is occupied by an enemy unit
        if ((destinationHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                (unit.GetComponent<UnitDatabaseFields>().nationality != destinationHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality))
            return (false);

        // This check needs to be done before the true check below since it is for impassible, sea, or netral countries - which can't be entered
        if ((destinationHex.GetComponent<HexDatabaseFields>().impassible) || (destinationHex.GetComponent<HexDatabaseFields>().neutralCountry) ||
                (destinationHex.GetComponent<HexDatabaseFields>().sea))
            return (false);

        // Cannot enter an enemy ZOC
        if ((unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German) && destinationHex.GetComponent<HexDatabaseFields>().inAlliedZOC)
            return (false);
        else if ((unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied) && destinationHex.GetComponent<HexDatabaseFields>().inGermanZOC)
            return (false);

        // Check if the destination hex was an original defending hex, if so it can't be used to retreat
        if (GlobalDefinitions.hexesAvailableForPostCombatMovement.Contains(destinationHex))
            return (false);

        if (beginningHex.GetComponent<HexDatabaseFields>().remainingMovement > 0)
        {
            destinationHex.GetComponent<HexDatabaseFields>().remainingMovement = beginningHex.GetComponent<HexDatabaseFields>().remainingMovement - 1;

            if (destinationHex.GetComponent<HexDatabaseFields>().remainingMovement == 0)
                return (true);
            else
                return (true);
        }
        else
            return (false);
    }

    /// <summary>
    /// Ths routine takes the unit at the 0 position in the GlobalDefinitions.retreatingUnits list to the hex selected by the user
    /// After moving it cleans up the hexes
    /// </summary>
    /// <param name="attackingNationality"></param>
    public static void retreatHexSelection(GameObject hex, GlobalDefinitions.Nationality attackingNationality)
    {
        if ((hex != null) && hex.GetComponent<HexDatabaseFields>().availableForMovement)
        {
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnit(hex, GlobalDefinitions.retreatingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex, GlobalDefinitions.retreatingUnits[0]);
            GlobalDefinitions.unhighlightUnit(GlobalDefinitions.retreatingUnits[0]);
            GlobalDefinitions.retreatingUnits.RemoveAt(0);

            // Clean up the hexes that were highlighted and reset remainingMovement
            foreach (GameObject cleanHex in GlobalDefinitions.allHexesOnBoard)
            {
                cleanHex.GetComponent<HexDatabaseFields>().remainingMovement = 0;
                cleanHex.GetComponent<HexDatabaseFields>().availableForMovement = false;
                GlobalDefinitions.unhighlightHex(cleanHex.gameObject);
            }
            //storedHexes.Clear();

            // Go back and check if additional units need to be retreated
            selectUnitsForRetreat();
        }
        else
        {
            GlobalDefinitions.guiUpdateStatusMessage("Invalid hex.  Please select again");
        }
    }

    /// <summary>
    /// Load all hexes that are available for post combat movement and moves invading units ashore
    /// </summary>
    /// <param name="attackingUnits"></param>
    /// <param name="defendingUnits"></param>
    public static void loadHexesAvailableForPostCombatMovement(List<GameObject> attackingUnits, List<GameObject> defendingUnits)
    {
        // Post combat movement is available if the defenders were doubled or tripled (i.e. city, mountain, fortress, or cross river)
        GlobalDefinitions.hexesAvailableForPostCombatMovement.Clear();
        foreach (GameObject unit in defendingUnits)
            if (GlobalDefinitions.calculateUnitDefendingFactor(unit, attackingUnits) > unit.GetComponent<UnitDatabaseFields>().defenseFactor)
                if (!GlobalDefinitions.hexesAvailableForPostCombatMovement.Contains(unit.GetComponent<UnitDatabaseFields>().occupiedHex))
                    GlobalDefinitions.hexesAvailableForPostCombatMovement.Add(unit.GetComponent<UnitDatabaseFields>().occupiedHex);

        // Post combat movement is also available if the attacking units were conducting an invasion
        foreach (GameObject attackingUnit in attackingUnits)
            if (attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea)
                foreach (GameObject defendingUnit in defendingUnits)
                    if (!GlobalDefinitions.hexesAvailableForPostCombatMovement.Contains(defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex))
                        GlobalDefinitions.hexesAvailableForPostCombatMovement.Add(defendingUnit.GetComponent<UnitDatabaseFields>().occupiedHex);
    }

    /// <summary>
    /// Take the unit loaded into the unitSelectedForPostCombatMovement and move it to the hex selected
    /// </summary>
    public static void executePostCombatMovement(GameObject hex)
    {
        GlobalDefinitions.writeToLogFile("executePostCombatMovement: Moving unit " + GlobalDefinitions.unitSelectedForPostCombatMovement.name + " to hex " + hex.name);
        if (GlobalDefinitions.hexesAvailableForPostCombatMovement.Contains(hex) && hex.GetComponent<HexDatabaseFields>().availableForMovement)
        {
            // Move the unit

            // If the unit is coming from a sea hex then this is a successful invasion
            if (GlobalDefinitions.unitSelectedForPostCombatMovement.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea)
            {
                GlobalDefinitions.writeToLogFile("executePostCombatMovement: setting hex " + hex.name + " to a successful invaded hex");
                hex.GetComponent<HexDatabaseFields>().successfullyInvaded = true;
                hex.GetComponent<HexDatabaseFields>().alliedControl = true;
            }

            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().moveUnit(hex, GlobalDefinitions.unitSelectedForPostCombatMovement.GetComponent<UnitDatabaseFields>().occupiedHex,
                            GlobalDefinitions.unitSelectedForPostCombatMovement);

            // Take highlighting off the hexes
            foreach (GameObject highlightHex in GlobalDefinitions.hexesAvailableForPostCombatMovement)
            {
                highlightHex.GetComponent<HexDatabaseFields>().availableForMovement = false;
                GlobalDefinitions.unhighlightHex(highlightHex);
            }

            // Now turn the gui back on for selection of any other units
            GlobalDefinitions.postCombatMovementGuiInstance.SetActive(true);
        }
        else
            GlobalDefinitions.guiUpdateStatusMessage("Hex selected is not available for post-combat movement");
    }

    /// <summary>
    /// Will return true if all defenders are on the same hex that has carpet bombing actice
    /// </summary>
    /// <param name="defendingUnits"></param>
    /// <returns></returns>
    private static bool checkIfCarpetBombingInEffect(List<GameObject> defendingUnits)
    {
        if (defendingUnits.Count > 1)
        {
            if (defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().carpetBombingActive)
            {
                for (int index = 1; index < defendingUnits.Count; index++)
                {
                    if (defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex != defendingUnits[index].GetComponent<UnitDatabaseFields>().occupiedHex)
                        return (false);
                }
                return (true);
            }
            else
                return (false);
        }
        else if (defendingUnits.Count == 1)
        {
            if (defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().carpetBombingActive)
                return (true);
            else
                return (false);
        }
        else
        {
            // This should never happen
            GlobalDefinitions.writeToLogFile("Error - null set of defending units passed to checkIfCarpetBombingInEffect()");
            return (false);
        }
    }

    /// <summary>
    /// Called at the end of the combat phase to reset all carpet bombing flags
    /// </summary>
    public static void resetCarpetBombingHexes()
    {
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            hex.GetComponent<HexDatabaseFields>().carpetBombingActive = false;
    }

    /// <summary>
    /// Returns true is any of the defenders has close defense support allocated to it
    /// </summary>
    /// <param name="defendingUnits"></param>
    /// <returns></returns>
    private static bool checkIfCloseDefenseActive(List<GameObject> defendingUnits)
    {
        foreach (GameObject unit in defendingUnits)
            if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().closeDefenseSupport)
                return (true);
        return (false);
    }

    /// <summary>
    /// Need a routine to end combat because is gets executed from two diffent points.  A user hitting "Next" when there are no 
    /// attacks or exiting out of the combat resolution gui when all combats have been resolved.
    /// </summary>
    public static void endAlliedCombatPhase()
    {
        resetCarpetBombingHexes();
        GlobalDefinitions.guiDisplayAlliedVictoryUnits();
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().setAlliedSupplyStatus(true);
    }

    /// <summary>
    /// Need a routine to end combat because is gets executed from two diffent points.  A user hitting "Next" when there are no 
    /// attacks or exiting out of the combat resolution gui when all combats have been resolved.
    /// </summary>
    public static void endGermanCombatPhase()
    {
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().setGermanSupplyStatus(true);

        // This is the second check of the German turn.  The rule is that a unit that starts and ends two consecutive turns out of supply are eliminated.
        // A unit that has a 1 for turns out of supply at this stage didn't start the turn out of supply so reset it to 0
        foreach (GameObject unit in GlobalDefinitions.germanUnitsOnBoard)
            if (unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply == 1)
                unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;
    }

    /// <summary>
    /// This is the routine that pulls up the gui for assiging allied tactical air support
    /// </summary>
    public static void createTacticalAirGUI()
    {
        Canvas tacticalAirCanvasInstance = new Canvas();
        float yPosition = 0;
        Button okButton;
        float numberOfRows;
        int toggleIndex = 0; // Used to index the toggle names to make them unique for access by network play
        Toggle closeDefenseToggle;
        Toggle unitInterdictionToggle;
        Toggle riverInterdictionToggle;

        if (GlobalDefinitions.tacticalAirMissionsThisTurn <= GlobalDefinitions.maxNumberOfTacticalAirMissions)
        {
            if ((GlobalDefinitions.maxNumberOfTacticalAirMissions - GlobalDefinitions.tacticalAirMissionsThisTurn) > 0)
                numberOfRows = GlobalDefinitions.closeDefenseHexes.Count + GlobalDefinitions.interdictedUnits.Count + GlobalDefinitions.riverInderdictedHexes.Count + 3.5f;
            else
                numberOfRows = GlobalDefinitions.closeDefenseHexes.Count + GlobalDefinitions.interdictedUnits.Count + GlobalDefinitions.riverInderdictedHexes.Count + 2.5f;

            float panelWidth = 9 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
            float panelHeight = (numberOfRows) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;

            GameObject tacticalAirGUIInstance = GlobalDefinitions.createGUICanvas("TacticalAirGUIInstance",
                    panelWidth,
                    panelHeight,
                    ref tacticalAirCanvasInstance);

            GlobalDefinitions.tacticalAirGUIInstance = tacticalAirGUIInstance;

            yPosition = 0.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight;
            okButton = GlobalDefinitions.createButton("tacticalAirOKButton", "OK",
                        4.5f * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);
            okButton.onClick.AddListener(TacticalAirToggleRoutines.tacticalAirOK);
            yPosition += 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;

            // The gui is being built from the bottom up.  At the bottom list the river interdicted units, then the interdicted units, and then the close defense hexes

            // The toggleIndex is used to create unique names for each of the toggles so they can be found during remote play
            toggleIndex = 0;
            foreach (GameObject hex in GlobalDefinitions.riverInderdictedHexes)
            {
                Button riverInterdictionLocateButton;
                Button riverInterdictionCancelButton;

                GlobalDefinitions.createText("River Interdiction", "RiverInerdictionInstanceText",
                        2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        1 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);

                GlobalDefinitions.createText(hex.name, "RiverInerdictionHexText",
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        3 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);

                riverInterdictionLocateButton = GlobalDefinitions.createButton("riverInterdictionLocateButton" + toggleIndex, "Locate",
                        5 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);
                riverInterdictionLocateButton.gameObject.AddComponent<TacticalAirToggleRoutines>();
                riverInterdictionLocateButton.gameObject.GetComponent<TacticalAirToggleRoutines>().hex = hex;
                riverInterdictionLocateButton.onClick.AddListener(riverInterdictionLocateButton.GetComponent<TacticalAirToggleRoutines>().locateRiverInterdiction);

                riverInterdictionCancelButton = GlobalDefinitions.createButton("riverInterdictionCancelButton" + toggleIndex, "Cancel",
                        7 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);
                riverInterdictionCancelButton.gameObject.AddComponent<TacticalAirToggleRoutines>();
                riverInterdictionCancelButton.gameObject.GetComponent<TacticalAirToggleRoutines>().hex = hex;
                riverInterdictionCancelButton.onClick.AddListener(riverInterdictionCancelButton.GetComponent<TacticalAirToggleRoutines>().cancelRiverInterdiction);

                yPosition += 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
                toggleIndex++;
            }

            toggleIndex = 0;
            foreach (GameObject unit in GlobalDefinitions.interdictedUnits)
            {
                Button unitInterdictionLocateButton;
                Button unitInterdictionCancelButton;

                GlobalDefinitions.createText("Unit Interdiction", "UnitInerdictionInstanceText",
                        2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        1 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);

                GlobalDefinitions.createUnitImage(unit, "tacticalAirUnitInterdictionImage",
                        3 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);

                unitInterdictionLocateButton = GlobalDefinitions.createButton("unitInterdictionLocateButton" + toggleIndex, "Locate",
                        5 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);
                unitInterdictionLocateButton.gameObject.AddComponent<TacticalAirToggleRoutines>();
                unitInterdictionLocateButton.gameObject.GetComponent<TacticalAirToggleRoutines>().unit = unit;
                unitInterdictionLocateButton.onClick.AddListener(unitInterdictionLocateButton.GetComponent<TacticalAirToggleRoutines>().locateInterdictedUnit);

                unitInterdictionCancelButton = GlobalDefinitions.createButton("unitInterdictionCancelButton" + toggleIndex, "Cancel",
                        7 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);
                unitInterdictionCancelButton.gameObject.AddComponent<TacticalAirToggleRoutines>();
                unitInterdictionCancelButton.gameObject.GetComponent<TacticalAirToggleRoutines>().unit = unit;
                unitInterdictionCancelButton.onClick.AddListener(unitInterdictionCancelButton.GetComponent<TacticalAirToggleRoutines>().cancelInterdictedUnit);

                yPosition += 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
                toggleIndex++;
            }

            toggleIndex = 0;
            foreach (GameObject hex in GlobalDefinitions.closeDefenseHexes)
            {
                Button closeDefenseLocateButton;
                Button closeDefenseCancelButton;
                GlobalDefinitions.createText("Close Defense", "CloseDefenseInstanceText",
                        2 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        1 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);
                //for (int index = 0; ((index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count) && (index < 2)); index++)
                for (int index = 0; (index < hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count); index++)
                {
                    GlobalDefinitions.createUnitImage(hex.GetComponent<HexDatabaseFields>().occupyingUnit[index], "tacticalAirDefenseImage",
                        (index + 2) * 1f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth + 40 + index * 5,
                        yPosition,
                        tacticalAirCanvasInstance);
                }

                closeDefenseLocateButton = GlobalDefinitions.createButton("closeDefenseLocateButton" + toggleIndex, "Locate",
                        5 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);
                closeDefenseLocateButton.gameObject.AddComponent<TacticalAirToggleRoutines>();
                closeDefenseLocateButton.gameObject.GetComponent<TacticalAirToggleRoutines>().hex = hex;
                closeDefenseLocateButton.onClick.AddListener(closeDefenseLocateButton.GetComponent<TacticalAirToggleRoutines>().locateCloseDefense);


                closeDefenseCancelButton = GlobalDefinitions.createButton("closeDefenseCancelButton" + toggleIndex, "Cancel",
                        7 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);
                closeDefenseCancelButton.gameObject.AddComponent<TacticalAirToggleRoutines>();
                closeDefenseCancelButton.gameObject.GetComponent<TacticalAirToggleRoutines>().hex = hex;
                closeDefenseCancelButton.onClick.AddListener(closeDefenseCancelButton.GetComponent<TacticalAirToggleRoutines>().cancelCloseDefense);

                yPosition += 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
                toggleIndex++;
            }

            // If there are air factors left then display the toggels to select more missions
            if ((GlobalDefinitions.maxNumberOfTacticalAirMissions - GlobalDefinitions.tacticalAirMissionsThisTurn) > 0)
            {

                closeDefenseToggle = GlobalDefinitions.createToggle("CloseDefense",
                        2 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance).GetComponent<Toggle>();
                closeDefenseToggle.gameObject.AddComponent<TacticalAirToggleRoutines>();
                closeDefenseToggle.onValueChanged.AddListener((bool value) => closeDefenseToggle.GetComponent<TacticalAirToggleRoutines>().addCloseDefenseHex());


                riverInterdictionToggle = GlobalDefinitions.createToggle("RiverInterdiction",
                        4.5f * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance).GetComponent<Toggle>();
                riverInterdictionToggle.gameObject.AddComponent<TacticalAirToggleRoutines>();
                riverInterdictionToggle.onValueChanged.AddListener((bool value) => riverInterdictionToggle.GetComponent<TacticalAirToggleRoutines>().addRiverInterdiction());

                unitInterdictionToggle = GlobalDefinitions.createToggle("UnitInterdiction",
                        7 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance).GetComponent<Toggle>();
                unitInterdictionToggle.gameObject.AddComponent<TacticalAirToggleRoutines>();
                unitInterdictionToggle.onValueChanged.AddListener((bool value) => unitInterdictionToggle.GetComponent<TacticalAirToggleRoutines>().addInterdictedUnit());

                yPosition += 0.75f * GlobalDefinitions.GUIUNITIMAGESIZE;

                GlobalDefinitions.createText("Assign Close Defense Support", "CloseDefenseText",
                        3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        2 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);

                GlobalDefinitions.createText("Assign River Interdiction", "RiverInterdictionText",
                        3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        4.5f * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);

                GlobalDefinitions.createText("Assign Unit Interdiction", "UnitInterdictionText",
                        3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                        GlobalDefinitions.GUIUNITIMAGESIZE,
                        7 * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                        yPosition,
                        tacticalAirCanvasInstance);

                yPosition += 0.75f * GlobalDefinitions.GUIUNITIMAGESIZE;
            }

            GlobalDefinitions.numberTacticalAirFactorsRemainingText = GlobalDefinitions.createText((GlobalDefinitions.maxNumberOfTacticalAirMissions - GlobalDefinitions.tacticalAirMissionsThisTurn) + " number of air factors remaining", "RemainingFactorsText",
                    7 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    4.5f * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    yPosition,
                    tacticalAirCanvasInstance);

            yPosition += 0.75f * GlobalDefinitions.GUIUNITIMAGESIZE;

            GlobalDefinitions.createText("Allied Tactical Air", "TacticalAirHeaderText",
                    4 * GlobalDefinitions.GUIUNITIMAGESIZE,
                    GlobalDefinitions.GUIUNITIMAGESIZE,
                    4.5f * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                    yPosition,
                    tacticalAirCanvasInstance);
        }
    }

    /// <summary>
    /// This routine checks that the hex passed is available for close air defense and sets the flag if it is
    /// </summary>
    public static void setCloseDefenseHex(GameObject closeDefenseHex)
    {
        if ((closeDefenseHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                (closeDefenseHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied))
        {
            closeDefenseHex.GetComponent<HexDatabaseFields>().closeDefenseSupport = true;
            closeDefenseHex.transform.localScale = new Vector2(0.9f, 0.9f);
            closeDefenseHex.GetComponent<SpriteRenderer>().material.color = GlobalDefinitions.TacticalAirCloseDefenseHighlightColor;
            closeDefenseHex.GetComponent<SpriteRenderer>().sortingOrder = 2;
            GlobalDefinitions.tacticalAirMissionsThisTurn++;
            GlobalDefinitions.closeDefenseHexes.Add(closeDefenseHex);
        }
        else
        {
            GlobalDefinitions.guiUpdateStatusMessage("Invalid hex selected for Close Defense Support.  Hex must be occupied by Allied units");
        }

        // Only pull up a gui if this isn't the AI calling the routine
        if (!GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI))
            return;

        // If a gui isn't already up then call up a tactical air gui
        if (GlobalDefinitions.guiList.Count == 0)
            createTacticalAirGUI();

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().nonToggleSelection;

    }

    /// <summary>
    /// This routine takes the user input and sets a river interdiction hex
    /// </summary>
    public static void getRiverInterdictedHex(GameObject riverInterdictionHex)
    {
        bool riverHex = false;

        if (riverInterdictionHex == null)
            GlobalDefinitions.guiUpdateStatusMessage("No hex selected; must select a hex bordered by a river");

        else if (riverInterdictionHex.GetComponent<HexDatabaseFields>().bridge)
            GlobalDefinitions.guiUpdateStatusMessage("Dyke cannot be selected for river interdiction");
        else
        {
            for (int index = 0; index < 6; index++)
            {
                if (!riverHex && (riverInterdictionHex.GetComponent<BoolArrayData>().riverSides[index] == true))
                {
                    riverHex = true;
                    riverInterdictionHex.transform.localScale = new Vector2(0.75f, 0.75f);
                    riverInterdictionHex.GetComponent<SpriteRenderer>().material.color = GlobalDefinitions.TacticalAirRiverInterdictionHighlightColor;
                    riverInterdictionHex.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    riverInterdictionHex.GetComponent<HexDatabaseFields>().riverInterdiction = true;
                    GlobalDefinitions.tacticalAirMissionsThisTurn++;
                    GlobalDefinitions.riverInderdictedHexes.Add(riverInterdictionHex);
                }
            }
            if (!riverHex)
                GlobalDefinitions.guiUpdateStatusMessage("Hex selected doesn't border a river; not valid for river interdiction");
        }

        // Only pull up a gui if this isn't the AI calling the routine
        if (GlobalDefinitions.localControl)
        {
            // If a gui isn't already up then call up a tactical air gui
            if (GlobalDefinitions.guiList.Count == 0)
                createTacticalAirGUI();

            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().nonToggleSelection;
        }
    }

    /// <summary>
    /// Stores the unit to interdict based on the user input
    /// </summary>
    public static void getInterdictedUnit(GameObject interdictedUnitHex)
    {
        int numberOfUnits = 0;
        int currentUnitCount = 0;
        Canvas tacticalAirMultiUnitCanvasInstance = new Canvas();
        if (interdictedUnitHex != null)
        {
            if (interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0)
            {
                if (interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German)
                {
                    // Check for units that already interdicted and are available for strategic movement
                    for (int index = 0; index < interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                        if (!interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().unitInterdiction &&
                                interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().availableForStrategicMovement)
                            numberOfUnits++;

                    if (numberOfUnits == 1)
                    {
                        // There is only one unit available on the hex so don't need to have user select
                        for (int index = 0; index < interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                            if (!interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().unitInterdiction &&
                                    interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().availableForStrategicMovement)
                                addInterdictedUnitToList(interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index]);
                    }
                    else if (numberOfUnits == 0)
                    {
                        GlobalDefinitions.guiUpdateStatusMessage("All units on the hex have already been interdicted or aren't available for strategic movement anyhow");

                        // If a gui isn't already up then call up a tactical air gui
                        if (GlobalDefinitions.guiList.Count == 0)
                            createTacticalAirGUI();

                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().nonToggleSelection;
                    }
                    else
                    {
                        float panelWidth = (numberOfUnits + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE;
                        float panelHeight = 3 * GlobalDefinitions.GUIUNITIMAGESIZE;
                        // Will need to present a gui to the user to select which unit he wants to select
                        GlobalDefinitions.tacticalAirGUIInstance = GlobalDefinitions.createGUICanvas("InterdictedAirGUIInstance",
                                panelWidth,
                                panelHeight,
                                ref tacticalAirMultiUnitCanvasInstance);
                        GlobalDefinitions.createText("Select unit for interdiction", "TacticalAirMultiUnitText",
                                (numberOfUnits + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE,
                                3 * GlobalDefinitions.GUIUNITIMAGESIZE,
                                0.5f * (numberOfUnits + 1) * 1.25f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelWidth,
                                2.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                                tacticalAirMultiUnitCanvasInstance);
                        for (int index = 0; index < interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit.Count; index++)
                            if (!interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().unitInterdiction &&
                                    interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index].GetComponent<UnitDatabaseFields>().availableForStrategicMovement)
                            {
                                Toggle tempToggle;
                                tempToggle = GlobalDefinitions.createUnitTogglePair("tacticalAirMultiUnitTogglePair" + index,
                                            (currentUnitCount + 1) * GlobalDefinitions.GUIUNITIMAGESIZE * 1.25f - 0.5f * panelWidth,
                                            1.5f * GlobalDefinitions.GUIUNITIMAGESIZE - 0.5f * panelHeight,
                                            tacticalAirMultiUnitCanvasInstance,
                                            interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index]);
                                tempToggle.gameObject.AddComponent<TacticalAirToggleRoutines>();
                                tempToggle.GetComponent<TacticalAirToggleRoutines>().unit = interdictedUnitHex.GetComponent<HexDatabaseFields>().occupyingUnit[index];
                                tempToggle.onValueChanged.AddListener((bool value) => tempToggle.GetComponent<TacticalAirToggleRoutines>().multiUnitSelection());
                                currentUnitCount++;
                            }
                    }
                }
                else
                {
                    GlobalDefinitions.guiUpdateStatusMessage("Unit selected must be German");
                    // Only pull up a gui if this isn't the AI calling the routine
                    if (GlobalDefinitions.localControl)
                    {

                        // If a gui isn't already up then call up a tactical air gui
                        if (GlobalDefinitions.guiList.Count == 0)
                            createTacticalAirGUI();

                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().nonToggleSelection;
                    }
                }
            }
            else
            {
                GlobalDefinitions.guiUpdateStatusMessage("No units to interdict on the hex selected");

                // Only pull up a gui if this isn't the AI calling the routine
                if (GlobalDefinitions.localControl)
                {
                    // If a gui isn't already up then call up a tactical air gui
                    if (GlobalDefinitions.guiList.Count == 0)
                        createTacticalAirGUI();

                    GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().nonToggleSelection;
                }
            }
        }
        else
        {
            GlobalDefinitions.guiUpdateStatusMessage("No hex selected; must select a hex that has German units on it");

            // Only pull up a gui if this isn't the AI calling the routine
            if (GlobalDefinitions.localControl)
            {

                // If a gui isn't already up then call up a tactical air gui
                if (GlobalDefinitions.guiList.Count == 0)
                    createTacticalAirGUI();

                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().nonToggleSelection;
            }
        }
    }

    public static void addInterdictedUnitToList(GameObject unit)
    {
        GlobalDefinitions.tacticalAirMissionsThisTurn++;
        GlobalDefinitions.interdictedUnits.Add(unit);
        unit.GetComponent<UnitDatabaseFields>().unitInterdiction = true;

        // If a gui isn't already up then call up a tactical air gui
        if (GlobalDefinitions.guiList.Count == 0)
            createTacticalAirGUI();

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().nonToggleSelection;
    }

    /// <summary>
    /// This set the turn available on the Free French units to the curent turn if German units are North of the line indicated on the board
    /// </summary>
    public static void checkForAvailableFreeFrenchUnits()
    {
        bool germanUnitsCleared = true;
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            if (hex.GetComponent<HexDatabaseFields>().FreeFrenchAvailableHex && (hex.GetComponent<HexDatabaseFields>().occupyingUnit.Count > 0) &&
                    (hex.GetComponent<HexDatabaseFields>().occupyingUnit[0].GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.German))
                germanUnitsCleared = false;

        // The German units are all North of the line so the Free French units need to be marked as being available
        if (germanUnitsCleared)
        {
            GlobalDefinitions.guiUpdateStatusMessage("The five Free French units at the bottom of the board are now available");
            GameObject.Find("Armor-FR-5").GetComponent<UnitDatabaseFields>().turnAvailable = GlobalDefinitions.turnNumber;
            GameObject.Find("Infantry-FR-14").GetComponent<UnitDatabaseFields>().turnAvailable = GlobalDefinitions.turnNumber;
            GameObject.Find("Infantry-FR-2").GetComponent<UnitDatabaseFields>().turnAvailable = GlobalDefinitions.turnNumber;
            GameObject.Find("Infantry-FR-3").GetComponent<UnitDatabaseFields>().turnAvailable = GlobalDefinitions.turnNumber;
            GameObject.Find("Infantry-FR-4").GetComponent<UnitDatabaseFields>().turnAvailable = GlobalDefinitions.turnNumber;
        }
    }

    /// <summary>
    /// Checks if the die roll passed should be influenced.  Passes adjusted value back
    /// </summary>
    /// <param name="dieRoll"></param>
    /// <returns></returns>
    private static int checkForDieRollInfluence(int dieRoll)
    {
        int adjustedDieRoll = dieRoll;
        int influenceRoll = GlobalDefinitions.dieRoll.Next(1, 5);

        // first check for whether this is an AI game.  If not there is no influence
        if (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.AI)
        {

            // The next check is whether the AI is attacking or defending.  If the localControl is true the AI is defending.
            if (!GlobalDefinitions.AIExecuting)
            {
#if OUTPUTDEBUG
                GlobalDefinitions.writeToLogFile("CheckForDieRollInfluence: AI defending");
#endif
                // The AI is defending
                switch (GlobalDefinitions.difficultySetting)
                {
                    case 0:
                        if (dieRoll > 0)
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll);
#endif
                        return (adjustedDieRoll);
                    case 1:
                        if ((dieRoll > 0) && (influenceRoll < 5))
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 2:
                        if ((dieRoll > 0) && (influenceRoll < 4))
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 3:
                        if ((dieRoll > 0) && (influenceRoll < 3))
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 4:
                        if ((dieRoll > 0) && (influenceRoll < 2))
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 5:
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 6:
                        if ((dieRoll > 5) && (influenceRoll < 2))
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 7:
                        if ((dieRoll < 5) && (influenceRoll < 3))
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 8:
                        if ((dieRoll < 5) && (influenceRoll < 4))
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 9:
                        if ((dieRoll < 5) && (influenceRoll < 5))
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 10:
                        if (dieRoll < 5)
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll);
#endif
                        return (adjustedDieRoll);

                }
            }
            else
            {
#if OUTPUTDEBUG
                GlobalDefinitions.writeToLogFile("CheckForDieRollInfluence: AI attacking");
#endif
                // The AI is attacking
                switch (GlobalDefinitions.difficultySetting)
                {
                    case 0:
                        if (dieRoll < 5)
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll);
#endif
                        return (adjustedDieRoll);
                    case 1:
                        if ((dieRoll < 5) && (influenceRoll < 5))
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 2:
                        if ((dieRoll < 5) && (influenceRoll < 4))
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 3:
                        if ((dieRoll < 5) && (influenceRoll < 3))
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 4:
                        if ((dieRoll < 5) && (influenceRoll < 2))
                            adjustedDieRoll++;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 5:
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 6:
                        if ((dieRoll > 0) && (influenceRoll < 2))
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 7:
                        if ((dieRoll > 0) && (influenceRoll < 3))
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 8:
                        if ((dieRoll > 0) && (influenceRoll < 4))
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 9:
                        if ((dieRoll > 0) && (influenceRoll < 5))
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll + " influence roll = " + influenceRoll);
#endif
                        return (adjustedDieRoll);
                    case 10:
                        if (dieRoll > 0)
                            adjustedDieRoll--;
#if OUTPUTDEBUG
                        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = ");
#endif
                        return (adjustedDieRoll);

                }
            }
        }
#if OUTPUTDEBUG
        GlobalDefinitions.writeToLogFile("checkForDieRollInfluence: die roll = " + dieRoll + " difficulty setting = " + GlobalDefinitions.difficultySetting + " adjusted die roll = " + adjustedDieRoll);
#endif
        return (adjustedDieRoll);
    }

    public static void adjustAggressiveness()
    {
        switch (GlobalDefinitions.aggressiveSetting)
        {
            case 1:
#if OUTPUTDEBUG
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting maximumAIOdds = 5");
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting minimumAIOdds = 3");
#endif
                GlobalDefinitions.maximumAIOdds = 5;
                GlobalDefinitions.minimumAIOdds = 3;
                break;
            case 2:
#if OUTPUTDEBUG
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting maximumAIOdds = 4");
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting minimumAIOdds = 2"); 
#endif
                GlobalDefinitions.maximumAIOdds = 4;
                GlobalDefinitions.minimumAIOdds = 2;
                break;
            case 3:
#if OUTPUTDEBUG
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting maximumAIOdds = 3");
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting minimumAIOdds = 1");
#endif
                GlobalDefinitions.maximumAIOdds = 3;
                GlobalDefinitions.minimumAIOdds = 1;
                break;
            case 4:
#if OUTPUTDEBUG
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting maximumAIOdds = 2");
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting minimumAIOdds = 1");
#endif
                GlobalDefinitions.maximumAIOdds = 2;
                GlobalDefinitions.minimumAIOdds = 1;
                break;
            case 5:
#if OUTPUTDEBUG
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting maximumAIOdds = 1");
                GlobalDefinitions.writeToLogFile("adjustAggressiveness: setting minimumAIOdds = -2");
#endif
                GlobalDefinitions.maximumAIOdds = 3;
                GlobalDefinitions.minimumAIOdds = -2;
                break;
        }
    }
}