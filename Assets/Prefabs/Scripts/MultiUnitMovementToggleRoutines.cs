using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiUnitMovementToggleRoutines : MonoBehaviour
{
    public GameObject unit;

    public void selectUnitToMove()
    {
        // Send the selection to the network computer
        if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
            TransportScript.SendSocketMessage(GlobalDefinitions.MULTIUNITSELECTIONKEYWORD + " " + name);

        List<GameObject> movementHexes = new List<GameObject>();
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.highlightUnit(unit);
            GlobalDefinitions.selectedUnit = unit;
            GlobalDefinitions.startHex = unit.GetComponent<UnitDatabaseFields>().occupiedHex;

            // We don't want to highlight the movement hexes if we're in setup mode
            if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name != "setUpStateInstance")
            {
                movementHexes = GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().returnAvailableMovementHexes(GlobalDefinitions.startHex, GlobalDefinitions.selectedUnit);

                foreach (GameObject hex in movementHexes)
                    GlobalDefinitions.highlightHexForMovement(hex);
            }

            GlobalDefinitions.removeGUI(transform.parent.gameObject);

            if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                    (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanMovementStateInstance"))
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().executeSelectUnitDestination;
            else if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "setUpStateInstance")
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<SetUpState>().executeSelectUnitDestination;

            //if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
            //    GameControl.sendMouseClickToNetwork(GlobalDefinitions.selectedUnit, GlobalDefinitions.startHex);
        }

        else
        {
            // This actually should never happen since when the toggle is selected the gui is removed so there is never a chance to unselect the toggle
            GlobalDefinitions.removeGUI(transform.parent.gameObject);
        }
    }

    /// <summary>
    /// Executes when the cancel button is pressed
    /// </summary>
    public void cancelGui()
    {
        // Send the selection to the network computer
        if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
            TransportScript.SendSocketMessage(GlobalDefinitions.MULTIUNITSELECTIONCANCELKEYWORD + " " + name);

        if ((GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "alliedMovementStateInstance") ||
                (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "germanMovementStateInstance"))
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<MovementState>().executeSelectUnit;
        else if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.name == "setUpStateInstance")
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<SetUpState>().executeSelectUnit;

        GlobalDefinitions.removeGUI(transform.parent.gameObject);
    }
}
