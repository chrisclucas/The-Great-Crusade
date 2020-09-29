using UnityEngine;

namespace TheGreatCrusade
{
    public class YesNoButtonRoutines : MonoBehaviour
    {
        public UnityEngine.Events.UnityAction yesAction;
        public UnityEngine.Events.UnityAction noAction;
        /// <summary>
        /// Sets variables for a yes response
        /// </summary>
        public void YesButtonSelected()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.YESBUTTONSELECTEDKEYWORD);
            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            yesAction();
        }

        /// <summary>
        /// Sets variables for a no response
        /// </summary>
        public void NoButtonSelected()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.NOBUTTONSELECTEDKEYWORD);
            GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
            noAction();
        }

    }
}