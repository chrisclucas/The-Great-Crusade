using UnityEngine;
using UnityEngine.UI;

public class CombatResolutionButtonRoutines : MonoBehaviour
{
    public GameObject curentCombat;
    public GameObject attackFactorTextGameObject;
    public GameObject oddsTextGameObject;

    /// <summary>
    /// Called from the combat resolution gui to cancel a combat - determined by the combatResoultionArrayIndex loaded
    /// </summary>
    public void CancelAttack()
    {
        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.COMBATCANCELKEYWORD + " " + name);

        // Since we are going to reset the mustBeAttackedUnits list I need to clear out all the highlighting since 
        // there are cases where units were added to the list because of cross river attacks but haven't been assigned
        // to a combat yet.  If I don't clear highlighting here they won't be reset because they aren't mustBeAttackedUnits anymore

        foreach (GameObject unit in curentCombat.GetComponent<Combat>().defendingUnits)
        {
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
            GlobalDefinitions.UnhighlightUnit(unit);
        }

        foreach (GameObject unit in curentCombat.GetComponent<Combat>().attackingUnits)
        {
            unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack = false;
            GlobalDefinitions.UnhighlightUnit(unit);
        }

        // Check if we need to give a air mission back
        if (curentCombat.GetComponent<Combat>().attackAirSupport)
            GlobalDefinitions.tacticalAirMissionsThisTurn--;

        // Need to check if we need to give back carpet bombing
        if (curentCombat.GetComponent<Combat>().carpetBombing)
        {
            GlobalDefinitions.carpetBombingUsedThisTurn = false;
            GlobalDefinitions.numberOfCarpetBombingsUsed--;
            curentCombat.GetComponent<Combat>().carpetBombing = false;
            curentCombat.GetComponent<Combat>().defendingUnits[0].GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().carpetBombingActive = false;
        }

        GlobalDefinitions.allCombats.Remove(curentCombat);

        // Need to get rid of all the buttons and toggles in the remaining combats since they will be regenerated
        foreach (GameObject combat in GlobalDefinitions.allCombats)
        {
            DestroyImmediate(combat.GetComponent<Combat>().locateButton);
            DestroyImmediate(combat.GetComponent<Combat>().resolveButton);
            DestroyImmediate(combat.GetComponent<Combat>().cancelButton);
            DestroyImmediate(combat.GetComponent<Combat>().airSupportToggle);
        }

        GlobalDefinitions.RemoveGUI(GlobalDefinitions.combatResolutionGUIInstance);

        if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance") ||
                GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn)
            CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, true);
        else
            CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, false);

        if (GlobalDefinitions.allCombats.Count > 0)
        {
            CombatResolutionRoutines.CombatResolutionDisplay();
        }
        else
        {
            // If the last battle has been canceled then turn the button back on
            GameObject.Find("ResolveCombatButton").GetComponent<Button>().interactable = true;
        }
    }

    /// <summary>
    /// Executes when the OK button on the combat resolution screen is clicked
    /// </summary>
    public void Ok()
    {
        // If network game notify the remote system to execute OK
        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.COMBATOKKEYWORD + " " + name);

        if (GetComponentInChildren<Text>().text == "Continue")
        {
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.combatResolutionGUIInstance);

            // Turn the button back on
            GameObject.Find("ResolveCombatButton").GetComponent<Button>().interactable = true;

            // Determine what state we are in and set the next executeMethod
            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanMovementStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().ExecuteSelectUnit;
            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedCombatStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanCombatStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteSelectUnit;
            if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedInvasionStateInstance")
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedInvasionState>().ExecuteSelectUnit;
            if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedAirborneStateInstance")
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedAirborneState>().ExecuteSelectUnit;
        }
        else
        {
            // Turn the button back on
            GameObject.Find("ResolveCombatButton").GetComponent<Button>().interactable = true;

            GlobalDefinitions.RemoveGUI(GlobalDefinitions.combatResolutionGUIInstance);

            GlobalDefinitions.allCombats.Clear();

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecuteQuit();
        }
    }

    private Vector3 GetAnchor(Vector2 ndcSpace)
    {
        Vector3 worldPosition;

        Vector4 viewSpace = new Vector4(ndcSpace.x, ndcSpace.y, 1.0f, 1.0f);

        Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        // Transform to projection coordinate.
        Vector4 projectionToWorld = mainCamera.projectionMatrix.inverse * viewSpace;

        // Perspective divide.
        projectionToWorld /= projectionToWorld.w;

        // Z-component is backwards in Unity.
        projectionToWorld.z = -projectionToWorld.z;

        // Transform from camera space to world space.
        worldPosition = mainCamera.transform.position + mainCamera.transform.TransformVector(projectionToWorld);

        return worldPosition;
    }

    public void LocateAttack()
    {
        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.COMBATLOCATIONSELECTEDKEYWORD + " " + name);

        Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        // This centers the camera on the defending hex
        mainCamera.transform.position = new Vector3(
                curentCombat.GetComponent<Combat>().defendingUnits[0].transform.position.x,
                curentCombat.GetComponent<Combat>().defendingUnits[0].transform.position.y,
                mainCamera.transform.position.z);
        // This then moves the camera over to the left so that the gui doesn't cover the unit
        mainCamera.transform.position = new Vector3(
                mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
                curentCombat.GetComponent<Combat>().defendingUnits[0].transform.position.y,
                mainCamera.transform.position.z);
    }

    /// <summary>
    /// This routine is called when the user selects the Resolve button for a combat
    /// </summary>
    public void ResolutionSelected()
    {

        // Write out the name of the toggle being executed in order to send it once the die roll is known
        if (GlobalDefinitions.localControl)
            GlobalDefinitions.CombatResultToggleName = name;

        // If combat resolution hasn't started then check to make sure all required combats have been created
        if (!GlobalDefinitions.combatResolutionStarted)
        {
            if (CombatRoutines.CheckIfRequiredUnitsAreUncommitted(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality, true))
            {
                GlobalDefinitions.GuiUpdateStatusMessage("Cannot start combat resolution, highlighted units must be committed to combat first.");


            }
            else
            {

                // All required units are attacking or being attacked
                GlobalDefinitions.combatResolutionStarted = true;

                // Get rid of the "Continue" button since combat resolution has started
                GlobalDefinitions.combatResolutionOKButton.SetActive(false);

                // Once combat resolution starts, canceling an attack is no longer an option so get rid of all cancel buttons
                // Also can't assign any more air support so make those toggles non-interactive
                foreach (GameObject combat in GlobalDefinitions.allCombats)
                {
                    if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
                        combat.GetComponent<Combat>().airSupportToggle.interactable = false;
                    DestroyImmediate(combat.GetComponent<Combat>().cancelButton.gameObject);
                }

                // Only check for carpet bombing if Allies are attacking.  This is needed to keep the German attacks from being loaded
                if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality == GlobalDefinitions.Nationality.Allied)
                {
                    GlobalDefinitions.combatResultsFromLastTurn.Clear();
                    GlobalDefinitions.hexesAttackedLastTurn.Clear();
                    // Store all hexes being attacked this turn.  Used for carpet bombing availability next turn
                    foreach (GameObject combat in GlobalDefinitions.allCombats)
                        foreach (GameObject defender in combat.GetComponent<Combat>().defendingUnits)
                            if (!GlobalDefinitions.hexesAttackedLastTurn.Contains(defender.GetComponent<UnitDatabaseFields>().occupiedHex))
                                GlobalDefinitions.hexesAttackedLastTurn.Add(defender.GetComponent<UnitDatabaseFields>().occupiedHex);
                }
            }
        }

        if (GlobalDefinitions.combatResolutionStarted)
        {
            CombatResolutionRoutines.DetermineCombatResults(curentCombat, gameObject.GetComponent<RectTransform>().anchoredPosition);

            // Get rid of the locate button on the attack being resolved, can't gaurantee that the units are still there after resolution
            DestroyImmediate(curentCombat.GetComponent<Combat>().locateButton.gameObject);
            //Get rid of the resolve button since the battle has been resolved.  This is also used to determine if all combats have been resolved.
            //DestroyImmediate(GameObject.Find(GlobalDefinitions.CombatResultToggleName));
            GlobalDefinitions.RemoveGUI(gameObject);
        }

        // Check if all the attacks have been resolved by seeing if there are any more Resolve buttons left
        bool allAttacksResolved = true;
        foreach (GameObject combat in GlobalDefinitions.allCombats)
            if (combat.GetComponent<Combat>().resolveButton != null)
                allAttacksResolved = false;

        // If all attacks have been resolved turn on the quit button (which is the continue button with the text changed
        if (allAttacksResolved)
        {
            GlobalDefinitions.combatResolutionOKButton.SetActive(true);
            GlobalDefinitions.combatResolutionOKButton.GetComponent<Button>().GetComponentInChildren<Text>().text = "Quit";
        }
    }

    // Called when the user select to change the status of air support on the combat resolution gui
    public void AddAttackAirSupport()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.ADDCOMBATAIRSUPPORTKEYWORD + " " + name);

            if (GlobalDefinitions.tacticalAirMissionsThisTurn < GlobalDefinitions.maxNumberOfTacticalAirMissions)
            {
                curentCombat.GetComponent<Combat>().attackAirSupport = true;
                GlobalDefinitions.WriteToLogFile("addAttackAirSupport: incrementing GlobalDefinitions.tacticalAirMissionsThisTurn");
                GlobalDefinitions.tacticalAirMissionsThisTurn++;
                attackFactorTextGameObject.GetComponent<Text>().text =
                        GlobalDefinitions.CalculateAttackFactor(
                        curentCombat.GetComponent<Combat>().attackingUnits,
                        curentCombat.GetComponent<Combat>().attackAirSupport).ToString();
                oddsTextGameObject.GetComponent<Text>().text =
                        GlobalDefinitions.ConvertOddsToString(
                        GlobalDefinitions.ReturnCombatOdds(curentCombat.GetComponent<Combat>().defendingUnits,
                        curentCombat.GetComponent<Combat>().attackingUnits,
                        curentCombat.GetComponent<Combat>().attackAirSupport));
            }
            else
            {
                GlobalDefinitions.GuiUpdateStatusMessage("No more air support missions left to assign");
                GetComponent<Toggle>().isOn = false;
            }
        }
        else
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.REMOVECOMBATAIRSUPPORTKEYWORD + " " + name);

            curentCombat.GetComponent<Combat>().attackAirSupport = false;
            GlobalDefinitions.tacticalAirMissionsThisTurn--;
            attackFactorTextGameObject.GetComponent<Text>().text =
                    GlobalDefinitions.CalculateAttackFactor(
                    curentCombat.GetComponent<Combat>().attackingUnits,
                    curentCombat.GetComponent<Combat>().attackAirSupport).ToString();
            oddsTextGameObject.GetComponent<Text>().text =
                    GlobalDefinitions.ConvertOddsToString(
                    GlobalDefinitions.ReturnCombatOdds(
                    curentCombat.GetComponent<Combat>().defendingUnits,
                    curentCombat.GetComponent<Combat>().attackingUnits,
                    curentCombat.GetComponent<Combat>().attackAirSupport));
        }
    }
}
