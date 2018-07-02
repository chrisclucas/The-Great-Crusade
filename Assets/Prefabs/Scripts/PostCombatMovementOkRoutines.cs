using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostCombatMovementOkRoutines : MonoBehaviour
{
    public void executePostCombatMovement()
    {
        if (GlobalDefinitions.localControl && (GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network))
            TransportScript.SendSocketMessage(GlobalDefinitions.POSTCOMBATOKKEYWORD + " " + name);
        GlobalDefinitions.removeGUI(transform.parent.gameObject);
        GlobalDefinitions.hexesAvailableForPostCombatMovement.Clear();
        GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
    }
}
