using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalAirToggleRoutines : MonoBehaviour
{
    public float yPosition;
    public GameObject hex;
    public GameObject unit;

    public void addCloseDefenseHex()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.ADDCLOSEDEFENSEKEYWORD);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().executeCloseDefenseSelection;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
    }

    public void cancelCloseDefense()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.CANCELCLOSEDEFENSEKEYWORD + " " + name);

        for (int index = 0; index < GlobalDefinitions.closeDefenseHexes.Count; index++)
            if (GlobalDefinitions.closeDefenseHexes[index] == hex)
            {
                GlobalDefinitions.closeDefenseHexes[index].GetComponent<HexDatabaseFields>().closeDefenseSupport = false;
                GlobalDefinitions.unhighlightHex(GlobalDefinitions.closeDefenseHexes[index]);
                GlobalDefinitions.closeDefenseHexes.Remove(hex);
            }
        GlobalDefinitions.tacticalAirMissionsThisTurn--;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
        CombatResolutionRoutines.createTacticalAirGUI();
    }

    public void locateCloseDefense()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.LOCATECLOSEDEFENSEKEYWORD + " " + name);

        Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        // This centers the camera on the hex
        mainCamera.transform.position = new Vector3(hex.transform.position.x, hex.transform.position.y, mainCamera.transform.position.z);
        // This then moves the camera over to the left so that the gui doesn't cover the unit
        mainCamera.transform.position = new Vector3(
                mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
                hex.transform.position.y,
                mainCamera.transform.position.z);
    }

    public void addInterdictedUnit()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.ADDUNITINTERDICTIONKEYWORD);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().executeUnitInterdictionSelection;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
    }

    public void cancelInterdictedUnit()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.CANCELUNITINTERDICTIONKEYWORD + " " + name);

        for (int index = 0; index < GlobalDefinitions.interdictedUnits.Count; index++)
            if (GlobalDefinitions.interdictedUnits[index] == unit)
            {
                GlobalDefinitions.interdictedUnits[index].GetComponent<UnitDatabaseFields>().unitInterdiction = false;
                GlobalDefinitions.interdictedUnits.Remove(unit);
            }
        GlobalDefinitions.tacticalAirMissionsThisTurn--;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
        CombatResolutionRoutines.createTacticalAirGUI();
    }

    public void locateInterdictedUnit()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.LOCATEUNITINTERDICTIONKEYWORD + " " + name);

        Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        // This centers the camera on the unit
        mainCamera.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y, mainCamera.transform.position.z);
        // This then moves the camera over to the left so that the gui doesn't cover the unit
        mainCamera.transform.position = new Vector3(
                mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
                unit.transform.position.y,
                mainCamera.transform.position.z);
    }

    public void addRiverInterdiction()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.ADDRIVERINTERDICTIONKEYWORD);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().executeRiverInterdictionSelection;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
    }

    public void cancelRiverInterdiction()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.CANCELRIVERINTERDICTIONKEYWORD + " " + name);

        for (int index = 0; index < GlobalDefinitions.riverInderdictedHexes.Count; index++)
            if (GlobalDefinitions.riverInderdictedHexes[index] == hex)
            {
                GlobalDefinitions.riverInderdictedHexes[index].GetComponent<HexDatabaseFields>().riverInterdiction = false;
                GlobalDefinitions.unhighlightHex(GlobalDefinitions.riverInderdictedHexes[index]);
                GlobalDefinitions.riverInderdictedHexes.Remove(hex);
            }
        GlobalDefinitions.tacticalAirMissionsThisTurn--;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
        CombatResolutionRoutines.createTacticalAirGUI();
    }

    public void locateRiverInterdiction()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.LOCATERIVERINTERDICTIONKEYWORD + " " + name);

        Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        // This centers the camera on the hex
        mainCamera.transform.position = new Vector3(hex.transform.position.x, hex.transform.position.y, mainCamera.transform.position.z);
        // This then moves the camera over to the left so that the gui doesn't cover the unit
        mainCamera.transform.position = new Vector3(
                mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
                hex.transform.position.y,
                mainCamera.transform.position.z);
    }

    public void multiUnitSelection()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.TACAIRMULTIUNITSELECTIONKEYWORD + " " + name);

        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
        CombatResolutionRoutines.addInterdictedUnitToList(unit);
    }

    public static void tacticalAirOK()
    {
        GlobalDefinitions.writeToCommandFile(GlobalDefinitions.EXECUTETACTICALAIROKKEYWORD);

        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit(GameControl.inputMessage.GetComponent<InputMessage>());
    }
}
