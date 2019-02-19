using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostCombatMovementToggleRoutines : MonoBehaviour
{
    public int stackingLimit = 0;
    public GameObject unit;
    public GameObject beginningHex;

    public void MoveSelectedUnit()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.SELECTPOSTCOMBATMOVEMENTKEYWORD + " " + name);

            // The user has selected a unit.  If there is only one hex available move it there.
            // Otherwise turn off the gui, highlight the hexes available and wait for user selection
            if (GlobalDefinitions.hexesAvailableForPostCombatMovement.Count == 0)
            {
                // This should never happen
                GlobalDefinitions.GuiUpdateStatusMessage("Internal Error - Post-combat unit selected for move but no hexes available");
            }
            else if (GlobalDefinitions.hexesAvailableForPostCombatMovement.Count == 1)
            {
                if (GlobalDefinitions.HexUnderStackingLimit(GlobalDefinitions.hexesAvailableForPostCombatMovement[0], 
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality))
                {
                    // Don't need to wait for user input since there is only one hex available

                    // If the unit is on a sea hex than this is a successful invasion
                    if (gameObject.GetComponent<PostCombatMovementToggleRoutines>().unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().sea)
                    {
                        GlobalDefinitions.hexesAvailableForPostCombatMovement[0].GetComponent<HexDatabaseFields>().successfullyInvaded = true;
                        GlobalDefinitions.hexesAvailableForPostCombatMovement[0].GetComponent<HexDatabaseFields>().alliedControl = true;
                    }

                    GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(GlobalDefinitions.hexesAvailableForPostCombatMovement[0],
                            gameObject.GetComponent<PostCombatMovementToggleRoutines>().unit.GetComponent<UnitDatabaseFields>().occupiedHex,
                            gameObject.GetComponent<PostCombatMovementToggleRoutines>().unit);
                }
                else
                {
                    GlobalDefinitions.GuiUpdateStatusMessage("Hex is at stacking limit; cannot move unit");
                    GetComponent<Toggle>().isOn = false;
                }
            }
            else
            {
                // The user has options so turn off the gui, highlight the available hexes, load the unit selected, and set the flag to wait for user input
                GlobalDefinitions.postCombatMovementGuiInstance.SetActive(false);
                foreach (GameObject hex in GlobalDefinitions.hexesAvailableForPostCombatMovement)
                    if (GlobalDefinitions.HexUnderStackingLimit(hex, GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality))
                    {
                        hex.GetComponent<HexDatabaseFields>().availableForMovement = true;
                        GlobalDefinitions.HighlightHexForMovement(hex);
                    }

                GlobalDefinitions.unitSelectedForPostCombatMovement = gameObject.GetComponent<PostCombatMovementToggleRoutines>().unit;
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<CombatState>().ExecutePostCombatMovement;
            }
        }

        // This executes when a unit is deselected
        else
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.DESELECTPOSTCOMBATMOVEMENTKEYWORD + " " + name);
            // Take the unit and move it back to its beginning hex
            GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().MoveUnit(beginningHex,
                    gameObject.GetComponent<PostCombatMovementToggleRoutines>().unit.GetComponent<UnitDatabaseFields>().occupiedHex,
                    gameObject.GetComponent<PostCombatMovementToggleRoutines>().unit);
        }
    }
}
