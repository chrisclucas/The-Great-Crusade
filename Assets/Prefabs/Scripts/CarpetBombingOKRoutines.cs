using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarpetBombingOKRoutines : MonoBehaviour
{
    public void carpetBombingOK()
    {
        if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
            TransportScript.SendSocketMessage(GlobalDefinitions.CARPETBOMBINGOKKEYWORD + " " + name);
        foreach (Transform childTransform in transform.parent.transform)
            if (childTransform.gameObject.GetComponent<Toggle>() != null)
                if (childTransform.gameObject.GetComponent<Toggle>().isOn)
                {
                    childTransform.gameObject.GetComponent<CarpetBombingToggleRoutines>().hex.GetComponent<HexDatabaseFields>().carpetBombingActive = true;
                    GlobalDefinitions.numberOfCarpetBombingsUsed++;
                }
        GlobalDefinitions.removeGUI(transform.parent.gameObject);
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod = GameControl.alliedCombatStateInstance.GetComponent<CombatState>().executeSelectUnit;
    }
}
