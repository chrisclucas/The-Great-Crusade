using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheGreatCrusade
{
    public class TacticalAirToggleRoutines : MonoBehaviour
    {
        public float yPosition;
        public GameObject hex;
        public GameObject unit;

        public void AddCloseDefenseHex()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.ADDCLOSEDEFENSEKEYWORD);

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().ExecuteCloseDefenseSelection;
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.tacticalAirGUIInstance);
        }

        public void CancelCloseDefense()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.CANCELCLOSEDEFENSEKEYWORD + " " + name);

            for (int index = 0; index < GlobalDefinitions.closeDefenseHexes.Count; index++)
                if (GlobalDefinitions.closeDefenseHexes[index] == hex)
                {
                    GlobalDefinitions.closeDefenseHexes[index].GetComponent<HexDatabaseFields>().closeDefenseSupport = false;
                    GlobalDefinitions.UnhighlightHex(GlobalDefinitions.closeDefenseHexes[index]);
                    GlobalDefinitions.closeDefenseHexes.Remove(hex);
                }
            GlobalDefinitions.tacticalAirMissionsThisTurn--;
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.tacticalAirGUIInstance);
            CombatResolutionRoutines.CreateTacticalAirGUI();
        }

        public void LocateCloseDefense()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.LOCATECLOSEDEFENSEKEYWORD + " " + name);

            Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            // This centers the camera on the hex
            mainCamera.transform.position = new Vector3(hex.transform.position.x, hex.transform.position.y, mainCamera.transform.position.z);
            // This then moves the camera over to the left so that the gui doesn't cover the unit
            mainCamera.transform.position = new Vector3(
                    mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
                    hex.transform.position.y,
                    mainCamera.transform.position.z);
        }

        public void AddInterdictedUnit()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.ADDUNITINTERDICTIONKEYWORD);

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().ExecuteUnitInterdictionSelection;
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.tacticalAirGUIInstance);
        }

        public void CancelInterdictedUnit()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.CANCELUNITINTERDICTIONKEYWORD + " " + name);

            for (int index = 0; index < GlobalDefinitions.interdictedUnits.Count; index++)
                if (GlobalDefinitions.interdictedUnits[index] == unit)
                {
                    GlobalDefinitions.interdictedUnits[index].GetComponent<UnitDatabaseFields>().unitInterdiction = false;
                    GlobalDefinitions.interdictedUnits.Remove(unit);
                }
            GlobalDefinitions.tacticalAirMissionsThisTurn--;
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.tacticalAirGUIInstance);
            CombatResolutionRoutines.CreateTacticalAirGUI();
        }

        public void LocateInterdictedUnit()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.LOCATEUNITINTERDICTIONKEYWORD + " " + name);

            Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            // This centers the camera on the unit
            mainCamera.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y, mainCamera.transform.position.z);
            // This then moves the camera over to the left so that the gui doesn't cover the unit
            mainCamera.transform.position = new Vector3(
                    mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
                    unit.transform.position.y,
                    mainCamera.transform.position.z);
        }

        public void AddRiverInterdiction()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.ADDRIVERINTERDICTIONKEYWORD);

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<AlliedTacticalAirState>().ExecuteRiverInterdictionSelection;
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.tacticalAirGUIInstance);
        }

        public void CancelRiverInterdiction()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.CANCELRIVERINTERDICTIONKEYWORD + " " + name);

            for (int index = 0; index < GlobalDefinitions.riverInderdictedHexes.Count; index++)
                if (GlobalDefinitions.riverInderdictedHexes[index] == hex)
                {
                    GlobalDefinitions.riverInderdictedHexes[index].GetComponent<HexDatabaseFields>().riverInterdiction = false;
                    GlobalDefinitions.UnhighlightHex(GlobalDefinitions.riverInderdictedHexes[index]);
                    GlobalDefinitions.riverInderdictedHexes.Remove(hex);
                }
            GlobalDefinitions.tacticalAirMissionsThisTurn--;
            GlobalDefinitions.RemoveGUI(GlobalDefinitions.tacticalAirGUIInstance);
            CombatResolutionRoutines.CreateTacticalAirGUI();
        }

        public void LocateRiverInterdiction()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.LOCATERIVERINTERDICTIONKEYWORD + " " + name);

            Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            // This centers the camera on the hex
            mainCamera.transform.position = new Vector3(hex.transform.position.x, hex.transform.position.y, mainCamera.transform.position.z);
            // This then moves the camera over to the left so that the gui doesn't cover the unit
            mainCamera.transform.position = new Vector3(
                    mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
                    hex.transform.position.y,
                    mainCamera.transform.position.z);
        }

        public void MultiUnitSelection()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.TACAIRMULTIUNITSELECTIONKEYWORD + " " + name);

            GlobalDefinitions.RemoveGUI(GlobalDefinitions.tacticalAirGUIInstance);
            CombatResolutionRoutines.AddInterdictedUnitToList(unit);
        }

        public static void TacticalAirOK()
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.EXECUTETACTICALAIROKKEYWORD);

            GlobalDefinitions.RemoveGUI(GlobalDefinitions.tacticalAirGUIInstance);

            GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.ExecuteQuit();
        }
    }
}