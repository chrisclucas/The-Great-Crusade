using UnityEngine;
using UnityEngine.UI;

public class SupplyButtonRoutines : MonoBehaviour
{
    public GameObject supplySource;
    public GameObject unit;

    /// <summary>
    /// This routine is called when a toggle value is changed (note not clicked - don't want to have the overhead of coding event handlers to use the onPointerClick
    /// </summary>
    public void checkToggle()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.SETSUPPLYKEYWORD + " " + name);

            // The toggle was turned on.  Turn any other toggles that are on off.
            // Don't need to reset highlighting since the highlighting routine called
            // sets all units.
            //GlobalDefinitions.writeToLogFile("checkToggle: number of supply GUI = " + GlobalDefinitions.supplyGUI.Count);
            //GlobalDefinitions.writeToLogFile("checkToggle: this toggle name = " + this.name);
            foreach (GameObject supplyGui in GlobalDefinitions.supplyGUI)
                if (supplyGui.GetComponent<SupplyGUIObject>().supplyToggle.name != this.name)
                {
                    //GlobalDefinitions.writeToLogFile("checkToggle:      turning off Toggle = " + supplyGui.GetComponent<SupplyGUIObject>().supplyToggle.name);
                    supplyGui.GetComponent<SupplyGUIObject>().supplyToggle.GetComponent<Toggle>().isOn = false;
                }

            //GlobalDefinitions.writeToLogFile("checkToggle: setting currentSupplySource to " + supplySource.name);
            GlobalDefinitions.currentSupplySource = supplySource;
            GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().highlightUnitsAvailableForSupply(supplySource);
        }
        else
        {
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.RESETSUPPLYKEYWORD + " " + name);

            GlobalDefinitions.currentSupplySource = null;
            // The toggle was turned off so reset all highlighting
            foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
                GlobalDefinitions.unhighlightUnit(unit);
        }
    }

    /// <summary>
    /// Moves the camera to the supply source related to the locate button pressed
    /// </summary>
    public void locateSupplySource()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.LOCATESUPPLYKEYWORD + " " + name);

        Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        // This centers the camera on the unit
        mainCamera.transform.position = new Vector3(supplySource.transform.position.x, supplySource.transform.position.y, mainCamera.transform.position.z);        
        // This then moves the camera over to the left so that the gui doesn't cover the unit
        mainCamera.transform.position = new Vector3(
            mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
            supplySource.transform.position.y,
            mainCamera.transform.position.z);

    }

    /// <summary>
    /// Executed when the ok button is selected on the supply sources gui for supply assignment.  At the end of a phase.
    /// </summary>
    public void okSupplyWithEndPhase()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.OKSUPPLYKEYWORD + " " + name);

        // Reset all highlighting
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
        {
            GlobalDefinitions.unhighlightUnit(unit);
            if (unit.GetComponent<UnitDatabaseFields>().inSupply)
                unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;
        }

        // I need to know whether this is the beginning or end of a turn since unsupplied units are only eliminated at the end of a turn
        // With the implementation of the AI there is no Allied supply state but I think that the check at the end of combat doesn't come through this path... which might not be right
        //if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState == GameControl.alliedSupplyStateInstance.GetComponent<SupplyState>())
            GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().checkIfAlliedUnsuppliedUnitsShouldBeEliminated(false);
        //else
        //GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().checkIfAlliedUnsuppliedUnitsShouldBeEliminated(true);

        GlobalDefinitions.removeGUI(GlobalDefinitions.supplySourceGUIInstance);

        // Turn the button back on
        GameObject.Find("SupplySourcesButton").GetComponent<Button>().interactable = true;

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit();
    }

    /// <summary>
    /// Called to exit the supply gui when used for display
    /// </summary>
    public void okSupply()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.OKSUPPLYKEYWORD + " " + name);

        // Reset all highlighting
        foreach (GameObject unit in GlobalDefinitions.alliedUnitsOnBoard)
        {
            GlobalDefinitions.unhighlightUnit(unit);
            if (unit.GetComponent<UnitDatabaseFields>().inSupply)
                unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = 0;
        }

        // I need to know whether this is the beginning or end of a turn since unsupplied units are only eliminated at the end of a turn
        // With the implementation of the AI there is no Allied supply state but I think that the check at the end of combat doesn't come through this path... which might not be right
        //if (GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState == GameControl.alliedSupplyStateInstance.GetComponent<SupplyState>())
        GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().checkIfAlliedUnsuppliedUnitsShouldBeEliminated(false);
        //else
        //GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().checkIfAlliedUnsuppliedUnitsShouldBeEliminated(true);

        // Turn the button back on
        GameObject.Find("SupplySourcesButton").GetComponent<Button>().interactable = true;

        GlobalDefinitions.removeGUI(GlobalDefinitions.supplySourceGUIInstance);
    }

    /// <summary>
    /// This routine switches the supply status of the unit selected from the multi-unit gui
    /// </summary>
    public void selectFromMultiUnits()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.writeToCommandFile(GlobalDefinitions.CHANGESUPPLYSTATUSKEYWORD + " " + name);

            GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().swapSupplyStatus(unit);
        }
        GlobalDefinitions.removeGUI(transform.parent.gameObject);
        GlobalDefinitions.supplySourceGUIInstance.SetActive(true);
    }
}
