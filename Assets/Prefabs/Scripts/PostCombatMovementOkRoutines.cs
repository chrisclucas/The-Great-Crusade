using UnityEngine;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class PostCombatMovementOkRoutines : MonoBehaviour
    {
        public void ExecutePostCombatMovement()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.POSTCOMBATOKKEYWORD + " " + name);
            GUIRoutines.RemoveGUI(transform.parent.gameObject);
            GlobalDefinitions.hexesAvailableForPostCombatMovement.Clear();
            GlobalDefinitions.combatResolutionGUIInstance.SetActive(true);
        }
    }
}