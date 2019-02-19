using UnityEngine;
using UnityEngine.UI;

public class CarpetBombingOKRoutines : MonoBehaviour
{
    public void CarpetBombingOK()
    {
        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.CARPETBOMBINGOKKEYWORD + " " + name);
        foreach (Transform childTransform in transform.parent.transform)
            if (childTransform.gameObject.GetComponent<Toggle>() != null)
                if (childTransform.gameObject.GetComponent<Toggle>().isOn)
                {
                    childTransform.gameObject.GetComponent<CarpetBombingToggleRoutines>().hex.GetComponent<HexDatabaseFields>().carpetBombingActive = true;
                    GlobalDefinitions.numberOfCarpetBombingsUsed++;
                    GlobalDefinitions.carpetBombingUsedThisTurn = true;
                }
        GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
        GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod = GameControl.alliedCombatStateInstance.GetComponent<CombatState>().ExecuteSelectUnit;
    }
}
