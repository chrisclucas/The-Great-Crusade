using UnityEngine;
using UnityEngine.UI;

public class CarpetBombingOKRoutines : MonoBehaviour
{
    public void carpetBombingOK()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.CARPETBOMBINGOKKEYWORD + " " + name);
        foreach (Transform childTransform in transform.parent.transform)
            if (childTransform.gameObject.GetComponent<Toggle>() != null)
                if (childTransform.gameObject.GetComponent<Toggle>().isOn)
                {
                    childTransform.gameObject.GetComponent<CarpetBombingToggleRoutines>().hex.GetComponent<HexDatabaseFields>().carpetBombingActive = true;
                    GlobalDefinitions.numberOfCarpetBombingsUsed++;
                    GlobalDefinitions.carpetBombingUsedThisTurn = true;
                }
        GlobalDefinitions.removeGUI(transform.parent.gameObject);
        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod = GameControl.alliedCombatStateInstance.GetComponent<CombatState>().executeSelectUnit;
    }
}
