using UnityEngine;

public class YesNoButtonRoutines : MonoBehaviour
{
    public UnityEngine.Events.UnityAction yesAction;
    public UnityEngine.Events.UnityAction noAction;
    /// <summary>
    /// Sets variables for a yes response
    /// </summary>
    public void yesButtonSelected()
    {
        GlobalDefinitions.removeGUI(transform.parent.gameObject);
        yesAction();
    }

    /// <summary>
    /// Sets variables for a no response
    /// </summary>
    public void noButtonSelected()
    {
        GlobalDefinitions.removeGUI(transform.parent.gameObject);
        noAction();
    }

}
