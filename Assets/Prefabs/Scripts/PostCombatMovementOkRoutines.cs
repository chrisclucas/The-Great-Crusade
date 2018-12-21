using UnityEngine;

public class PostCombatMovementOkRoutines : MonoBehaviour
{
    public void executePostCombatMovement()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.POSTCOMBATOKKEYWORD + " " + name);
        GlobalDefinitions.removeGUI(transform.parent.gameObject);
        GlobalDefinitions.hexesAvailableForPostCombatMovement.Clear();
        GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
    }
}
