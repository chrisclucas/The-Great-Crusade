using UnityEngine;

namespace TheGreatCrusade
{
    public class PostCombatMovementOkRoutines : MonoBehaviour
    {
        public void ExecutePostCombatMovement()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.POSTCOMBATOKKEYWORD + " " + name);
            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            GlobalDefinitions.hexesAvailableForPostCombatMovement.Clear();
            GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
        }
    }
}