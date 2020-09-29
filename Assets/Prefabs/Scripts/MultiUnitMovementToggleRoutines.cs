using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheGreatCrusade
{
    public class MultiUnitMovementToggleRoutines : MonoBehaviour
    {
        public GameObject unit;

        public void SelectUnitToMove()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.MULTIUNITSELECTIONKEYWORD + " " + name);

            List<GameObject> movementHexes = new List<GameObject>();
            if (GetComponent<Toggle>().isOn)
            {
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                {
                    GlobalDefinitions.GuiUpdateStatusMessage("Unit selected is committed to an attack\nCancel attack if you want to move this unit");
                    GetComponent<Toggle>().isOn = false;
                }
                else
                {
                    GlobalDefinitions.HighlightUnit(unit);
                    GlobalDefinitions.selectedUnit = unit;
                    GlobalDefinitions.startHex = unit.GetComponent<UnitDatabaseFields>().occupiedHex;

                    // We don't want to highlight the movement hexes if we're in setup mode
                    if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name != "setUpStateInstance")
                    {
                        movementHexes = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().ReturnAvailableMovementHexes(GlobalDefinitions.startHex, GlobalDefinitions.selectedUnit);

                        foreach (GameObject hex in movementHexes)
                            GlobalDefinitions.HighlightHexForMovement(hex);
                    }

                    GlobalDefinitions.RemoveGUI(transform.parent.gameObject);

                    if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                            (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanMovementStateInstance"))
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().ExecuteSelectUnitDestination;
                    else if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "setUpStateInstance")
                        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<SetUpState>().ExecuteSelectUnitDestination;

                    //if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
                    //    GameControl.sendMouseClickToNetwork(GlobalDefinitions.selectedUnit, GlobalDefinitions.startHex);
                }
            }

            else
            {
                // This actually should never happen since when the toggle is selected the gui is removed so there is never a chance to unselect the toggle
                GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            }
        }

        /// <summary>
        /// Executes when the cancel button is pressed
        /// </summary>
        public void CancelGui()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.MULTIUNITSELECTIONCANCELKEYWORD + " " + name);

            if ((GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "germanMovementStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<MovementState>().ExecuteSelectUnit;
            else if (GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.name == "setUpStateInstance")
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<SetUpState>().ExecuteSelectUnit;

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        }
    }
}