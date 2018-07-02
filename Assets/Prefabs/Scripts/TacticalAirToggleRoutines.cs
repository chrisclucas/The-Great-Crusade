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
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.ADDCLOSEDEFENSEKEYWORD);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().executeCloseDefenseSelection;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
    }

    public void cancelCloseDefense()
    {
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.CANCELCLOSEDEFENSEKEYWORD + " " + name);

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
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.LOCATECLOSEDEFENSEKEYWORD + " " + name);

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
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.ADDUNITINTERDICTIONKEYWORD);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().executeUnitInterdictionSelection;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
    }

    public void cancelInterdictedUnit()
    {
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.CANCELUNITINTERDICTIONKEYWORD + " " + name);

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
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.LOCATEUNITINTERDICTIONKEYWORD + " " + name);

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
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.ADDRIVERINTERDICTIONKEYWORD);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeMethod =
                GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().executeRiverInterdictionSelection;
        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
    }

    public void cancelRiverInterdiction()
    {
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.CANCELRIVERINTERDICTIONKEYWORD + " " + name);

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
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.LOCATERIVERINTERDICTIONKEYWORD + " " + name);

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
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.TACAIRMULTIUNITSELECTIONKEYWORD + " " + name);

        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);
        CombatResolutionRoutines.addInterdictedUnitToList(unit);
    }

    public static void tacticalAirOK()
    {
        if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
            TransportScript.SendSocketMessage(GlobalDefinitions.EXECUTETACTICALAIROKKEYWORD);

        GlobalDefinitions.removeGUI(GlobalDefinitions.tacticalAirGUIInstance);

        GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.executeQuit(GameControl.inputMessage.GetComponent<InputMessage>());
    }
}
