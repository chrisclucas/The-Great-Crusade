﻿using UnityEngine;
using UnityEngine.UI;

public class InvasionSelectionToggleRoutines : MonoBehaviour
{
    public int index;

    public void InvadedAreaSelected()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToLogFile("invadedAreaSelected: exeucuting");
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.INVASIONAREASELECTIONKEYWORD + " " + name);
            GameControl.invasionRoutinesInstance.GetComponent<InvasionRoutines>().SetInvasionArea(index);

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedInvasionState>().ExecuteSelectUnit;
            GlobalDefinitions.WriteToLogFile("invadedAreaSelected: execute method set to executeSelectUnit()");

            GlobalDefinitions.WriteToLogFile("invadedAreaSelected: removing gui with transform parent = " + transform.parent.name + "   game object name = " + transform.parent.gameObject.name);

            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = true;
            GlobalDefinitions.undoButton.GetComponent<Button>().interactable = true;
            GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().interactable = true;
            GlobalDefinitions.AssignCombatButton.GetComponent<Button>().interactable = true;
            GlobalDefinitions.DisplayAllCombatsButton.GetComponent<Button>().interactable = true;
            GlobalDefinitions.AlliedSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
            GlobalDefinitions.GermanSupplyRangeToggle.GetComponent<Toggle>().interactable = true;
            GlobalDefinitions.AlliedSupplySourcesButton.GetComponent<Button>().interactable = true;
        }
    }
}
