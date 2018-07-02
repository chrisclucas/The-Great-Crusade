using System.IO;
using UnityEngine;
using System;

public class ReadWriteRoutines : MonoBehaviour
{
    /// <summary>
    /// This is the routine called to write out the save turn file
    /// </summary>
    public void writeSaveTurnFile(string saveFileType)
    {
        // There are three types of saved files: setup, end of Allied turn, and end of German turn.  The name of the file reflects this using the string passed
        string turnString;
        if (GlobalDefinitions.turnNumber < 10)
            turnString = "0" + GlobalDefinitions.turnNumber.ToString();
        else
            turnString = GlobalDefinitions.turnNumber.ToString();
        StreamWriter fileWriter = new StreamWriter(GameControl.path + "TGCOutputFiles\\TGCSaveFile_Turn" + turnString + "_" + saveFileType + ".txt");
        if (fileWriter == null)
            GlobalDefinitions.guiUpdateStatusMessage("Unable to open save file " + GameControl.path + "TGCOutputFiles\\TGCSaveFile_Turn" + turnString + "_" + saveFileType + ".txt");
        else
        {
            fileWriter.WriteLine("Turn " + GlobalDefinitions.turnNumber);

            fileWriter.Write("Global_Definitions ");
            GlobalDefinitions.writeGlobalVariables(fileWriter);

            fileWriter.WriteLine("Hexes");
            foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            {
                hex.GetComponent<HexDatabaseFields>().writeHexFields(fileWriter);
            }
            fileWriter.WriteLine("End");
            fileWriter.WriteLine("Units");
            foreach (Transform unit in GameObject.Find("Units Eliminated").transform)
            {
                unit.GetComponent<UnitDatabaseFields>().writeUnitFields(fileWriter);
            }
            foreach (Transform unit in GlobalDefinitions.allUnitsOnBoard.transform)
            {
                unit.GetComponent<UnitDatabaseFields>().writeUnitFields(fileWriter);
            }
            foreach (Transform unit in GameObject.Find("Units In Britain").transform)
            {
                unit.GetComponent<UnitDatabaseFields>().writeUnitFields(fileWriter);
            }
            fileWriter.WriteLine("End");

            // In network mode the Game Control loads the currentState and it works best if everything is set before it gets called.
            fileWriter.Write("Game_Control ");
            GameControl.writeGameControlStatusVariables(fileWriter);
            fileWriter.Close();
        }
    }

    /// <summary>
    /// This routine reads the contents of a save file
    /// </summary>
    /// <param name="fileName"></param>
    public void readTurnFile(string fileName)
    {
        char[] delimiterChars = { ' ' };
        string line;
        string[] lineEntries;
        string[] switchEntries;

        StreamReader theReader = new StreamReader(fileName);
        using (theReader)
        {
            do
            {
                line = theReader.ReadLine();
                if (line != null)
                {
                    switchEntries = line.Split(delimiterChars);
                    switch (switchEntries[0])
                    {
                        case "Turn":
                            GlobalDefinitions.writeToLogFile("readTurnFile: Processing Turn: " + line);
                            GlobalDefinitions.turnNumber = Convert.ToInt32(switchEntries[1]);
                            GlobalDefinitions.writeToLogFile("readTurnFile: setting global turn number = " + Convert.ToInt32(switchEntries[1]));
                            GlobalDefinitions.guiUpdateTurn();
                            //if (GlobalDefinitions.localControl && (GlobalDefinitions.GameMode == GlobalDefinitions.GameModeValues.Network))
                            //(TransportScript.sendInitialGameData(GlobalDefinitions.READTURNKEYWORD + " " + line));
                            break;
                        case "Game_Control":
                            GlobalDefinitions.writeToLogFile("readTurnFile: Processing Game Control: " + line);
                            GameControl.setGameState(switchEntries[1]);
                            //if (GlobalDefinitions.localControl && (GlobalDefinitions.GameMode == GlobalDefinitions.GameModeValues.Network))
                            //StartCoroutine(TransportScript.sendInitialGameData(GlobalDefinitions.SETGAMESTATEKEYWORD + " " + switchEntries[1]));
                            break;
                        case "Global_Definitions":
                            GlobalDefinitions.writeToLogFile("readTurnFile: Processing Global Definitions: " + line);
                            readGlobalVariables(switchEntries);
                            //if (GlobalDefinitions.localControl && (GlobalDefinitions.GameMode == GlobalDefinitions.GameModeValues.Network))
                            //StartCoroutine(TransportScript.sendInitialGameData(GlobalDefinitions.READSAVEDGLOBALDEFINITIONSPARAMETERSKEYWORD + " " + line));
                            break;
                        case "Hexes":
                            GlobalDefinitions.writeToLogFile("readTurnFile: Reading Hexes");
                            line = theReader.ReadLine();
                            lineEntries = line.Split(delimiterChars);
                            while (lineEntries[0] != "End")
                            {
                                //GlobalDefinitions.writeToLogFile("Processing Hex Record: " + line);
                                string[] entries = line.Split(delimiterChars);
                                processHexRecord(entries);

                                //if (GlobalDefinitions.localControl && (GlobalDefinitions.GameMode == GlobalDefinitions.GameModeValues.Network))
                                //StartCoroutine(TransportScript.sendInitialGameData(GlobalDefinitions.READSAVEDHEXKEYWORD + " " + line));

                                line = theReader.ReadLine();
                                lineEntries = line.Split(delimiterChars);
                            }
                            break;
                        case "Units":
                            GlobalDefinitions.writeToLogFile("readTurnFile: Reading Units");
                            line = theReader.ReadLine();
                            lineEntries = line.Split(delimiterChars);
                            while (lineEntries[0] != "End")
                            {
                                //GlobalDefinitions.writeToLogFile("Processing Unit Record: " + line);
                                string[] entries = line.Split(delimiterChars);
                                processUnitRecord(entries);

                                //if (GlobalDefinitions.localControl && (GlobalDefinitions.GameMode == GlobalDefinitions.GameModeValues.Network))
                                //StartCoroutine(TransportScript.sendInitialGameData(GlobalDefinitions.READSAVEDUNITKEYWORK + " " + line));

                                line = theReader.ReadLine();
                                lineEntries = line.Split(delimiterChars);
                            }
                            break;
                    }
                }
            }
            while (line != null);
            theReader.Close();
            //if (GlobalDefinitions.localControl && (GlobalDefinitions.GameMode == GlobalDefinitions.GameModeValues.Network))
            //StartCoroutine(TransportScript.sendInitialGameData(GlobalDefinitions.SAVEFILETRANSMITCOMPLETEKEYWORD));

            // If the current state is set-up mode then set the executeMode to executeSelectUnit so that the user can update the
            // setup file read in.  Otherwise execute the 
            GlobalDefinitions.writeToLogFile("readTurnFile: File read complete.  Initialize Game State");
            GameControl.gameStateControlInstance.GetComponent<gameStateControl>().currentState.initialize(GameControl.inputMessage.GetComponent<InputMessage>());
        }
    }

    /// <summary>
    /// This routine reads a single record for a hex
    /// </summary>
    /// <param name="entries"></param>
    public void processHexRecord(string[] entries)
    {
        GameObject hex;

        hex = GameObject.Find(entries[0]);

        hex.GetComponent<HexDatabaseFields>().inGermanZOC = GlobalDefinitions.returnBoolFromSaveFormat(entries[1]);
        hex.GetComponent<HexDatabaseFields>().inAlliedZOC = GlobalDefinitions.returnBoolFromSaveFormat(entries[2]);
        hex.GetComponent<HexDatabaseFields>().alliedControl = GlobalDefinitions.returnBoolFromSaveFormat(entries[3]);
        hex.GetComponent<HexDatabaseFields>().successfullyInvaded = GlobalDefinitions.returnBoolFromSaveFormat(entries[4]);
        hex.GetComponent<HexDatabaseFields>().closeDefenseSupport = GlobalDefinitions.returnBoolFromSaveFormat(entries[5]);
        hex.GetComponent<HexDatabaseFields>().riverInterdiction = GlobalDefinitions.returnBoolFromSaveFormat(entries[6]);

        // By calling the unhighlight rouitne (no hexes should be highlighted at this point) it will 
        // turn close defense and interdicted river hexes the correct color
        GlobalDefinitions.unhighlightHex(GameObject.Find(entries[0]));
    }

    /// <summary>
    /// This routine reads a single record for a unit
    /// </summary>
    /// <param name="entries"></param>
    public void processUnitRecord(string[] entries)
    {
        GameObject unit;

        unit = GameObject.Find(entries[0]);
        if (entries[1] == "null")
            unit.GetComponent<UnitDatabaseFields>().occupiedHex = null;
        else
            unit.GetComponent<UnitDatabaseFields>().occupiedHex = GameObject.Find(entries[1]);
        if (entries[2] == "null")
            unit.GetComponent<UnitDatabaseFields>().beginningTurnHex = null;
        else
            unit.GetComponent<UnitDatabaseFields>().beginningTurnHex = GameObject.Find(entries[2]);
        unit.GetComponent<UnitDatabaseFields>().inBritain = GlobalDefinitions.returnBoolFromSaveFormat(entries[3]);
        unit.GetComponent<UnitDatabaseFields>().unitInterdiction = GlobalDefinitions.returnBoolFromSaveFormat(entries[4]);
        unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex = Convert.ToInt32(entries[5]);
        unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = GlobalDefinitions.returnBoolFromSaveFormat(entries[6]);
        unit.GetComponent<UnitDatabaseFields>().inSupply = GlobalDefinitions.returnBoolFromSaveFormat(entries[7]);
        // Need to adjust the highlighting of the unit if it is out of supply
        GlobalDefinitions.unhighlightUnit(unit);
        if (entries[8] == "null")
            unit.GetComponent<UnitDatabaseFields>().supplySource = null;
        else
            unit.GetComponent<UnitDatabaseFields>().supplySource = GameObject.Find(entries[8]);
        unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = Convert.ToInt32(entries[9]);
        unit.GetComponent<UnitDatabaseFields>().unitEliminated = GlobalDefinitions.returnBoolFromSaveFormat(entries[10]);


        if (unit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
        {
            GlobalDefinitions.putUnitOnHex(unit, unit.GetComponent<UnitDatabaseFields>().occupiedHex);
            unit.transform.parent = GlobalDefinitions.allUnitsOnBoard.transform;
        }
        else if (unit.GetComponent<UnitDatabaseFields>().unitEliminated)
        {
            unit.transform.parent = GameObject.Find("Units Eliminated").transform;
            unit.transform.position = unit.GetComponent<UnitDatabaseFields>().OOBLocation;
        }
        else if (unit.GetComponent<UnitDatabaseFields>().inBritain)
        {
            unit.transform.parent = GameObject.Find("Units In Britain").transform;
            unit.transform.position = unit.GetComponent<UnitDatabaseFields>().locationInBritain;
        }
        else
        {
            Debug.Log("Unit read error - " + entries[1] + ": found no location to place this unit");
        }

        if (!unit.GetComponent<UnitDatabaseFields>().unitEliminated && !unit.GetComponent<UnitDatabaseFields>().inBritain)
        {
            if (unit.GetComponent<UnitDatabaseFields>().nationality == GlobalDefinitions.Nationality.Allied)
                GlobalDefinitions.alliedUnitsOnBoard.Add(unit);
            else
                GlobalDefinitions.germanUnitsOnBoard.Add(unit);
        }


    }

    /// <summary>
    /// This routine reads the record on a saved file that contains the Global Definition values
    /// </summary>
    /// <param name="entries"></param>
    public void readGlobalVariables(string[] entries)
    {
        GlobalDefinitions.numberOfCarpetBombingsUsed = Convert.ToInt32(entries[1]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set numberofCarpetBombingsUsed set to - " + GlobalDefinitions.numberOfCarpetBombingsUsed);
        GlobalDefinitions.numberInvasionsExecuted = Convert.ToInt32(entries[2]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set numberInvasionsExecuted set to - " + GlobalDefinitions.numberInvasionsExecuted);
        GlobalDefinitions.firstInvasionAreaIndex = Convert.ToInt32(entries[3]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set firstInvasionAreaIndex set to - " + GlobalDefinitions.firstInvasionAreaIndex);
        if (GlobalDefinitions.firstInvasionAreaIndex != -1)
        {
            GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].invaded = true;
            GlobalDefinitions.writeToLogFile("readGlobalVariables: Set GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].invaded set to - " + GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].invaded);
        }
        GlobalDefinitions.secondInvasionAreaIndex = Convert.ToInt32(entries[4]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set secondInvasionAreaIndex set to - " + GlobalDefinitions.secondInvasionAreaIndex);
        if (GlobalDefinitions.secondInvasionAreaIndex != -1)
        {
            GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].invaded = true;
            GlobalDefinitions.writeToLogFile("readGlobalVariables: Set GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].invaded set to - " + GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].invaded);
        }
        GlobalDefinitions.germanReplacementsRemaining = Convert.ToInt32(entries[5]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set germanReplacementsRemaining set to - " + GlobalDefinitions.germanReplacementsRemaining);
        GlobalDefinitions.alliedReplacementsRemaining = Convert.ToInt32(entries[6]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set alliedReplacementsRemaining set to - " + GlobalDefinitions.alliedReplacementsRemaining);
        GlobalDefinitions.alliedCapturedBrest = Convert.ToBoolean(entries[7]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set alliedCapturedBrest set to - " + GlobalDefinitions.alliedCapturedBrest);
        GlobalDefinitions.alliedCapturedBoulogne = Convert.ToBoolean(entries[8]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set alliedCapturedBoulogne set to - " + GlobalDefinitions.alliedCapturedBoulogne);
        GlobalDefinitions.alliedCapturedRotterdam = Convert.ToBoolean(entries[9]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set alliedCapturedRotterdam set to - " + GlobalDefinitions.alliedCapturedRotterdam);
        if (GlobalDefinitions.firstInvasionAreaIndex != -1)
        {
            GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].turn = Convert.ToInt32(entries[10]);
            GlobalDefinitions.writeToLogFile("readGlobalVariables: Set GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].turn set to - " + GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].turn);
        }
        if (GlobalDefinitions.secondInvasionAreaIndex != -1)
        {
            GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].turn = Convert.ToInt32(entries[11]);
            GlobalDefinitions.writeToLogFile("readGlobalVariables: Set GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].turn set to - " + GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].turn);
        }
        GlobalDefinitions.numberOfTurnsWithoutAttack = Convert.ToInt32(entries[12]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set numberOfTurnsWithoutAttack set to - " + GlobalDefinitions.germanReplacementsRemaining);

        GlobalDefinitions.hexesAttackedLastTurn.Clear();
        int loopLimit = Convert.ToInt32(entries[13]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: loading hexes attacked last turn Count = " + loopLimit);
        int entryIndex = 13;
        for (int index = 0; index < loopLimit; index++)
        {
            entryIndex++;
            GlobalDefinitions.hexesAttackedLastTurn.Add(GameObject.Find(entries[entryIndex]));
            GlobalDefinitions.writeToLogFile("readGlobalVariables:      " + entries[entryIndex]);
        }

        entryIndex++;
        GlobalDefinitions.combatResultsFromLastTurn.Clear();
        loopLimit = Convert.ToInt32(entries[entryIndex]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: loading combat results from last turn Count = " + loopLimit);
        for (int index = 0; index < loopLimit; index++)
        {
            entryIndex++;

            GlobalDefinitions.writeToLogFile("readGlobalVariables:      " + entries[entryIndex]);
            switch (entries[entryIndex])
            {
                case "Delim":
                    GlobalDefinitions.combatResultsFromLastTurn.Add(GlobalDefinitions.CombatResults.Delim);
                    break;
                case "Dback2":
                    GlobalDefinitions.combatResultsFromLastTurn.Add(GlobalDefinitions.CombatResults.Dback2);
                    break;
                case "Aelim":
                    GlobalDefinitions.combatResultsFromLastTurn.Add(GlobalDefinitions.CombatResults.Aelim);
                    break;
                case "Aback2":
                    GlobalDefinitions.combatResultsFromLastTurn.Add(GlobalDefinitions.CombatResults.Aback2);
                    break;
                case "Exchange":
                    GlobalDefinitions.combatResultsFromLastTurn.Add(GlobalDefinitions.CombatResults.Exchange);
                    break;
            }
        }
        entryIndex++;
        GlobalDefinitions.turnsAlliedMetVictoryCondition = Convert.ToInt32(entries[entryIndex]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set turnsAlliedMetVictoryCondition set to - " + GlobalDefinitions.turnsAlliedMetVictoryCondition);
        entryIndex++;
        GlobalDefinitions.alliedFactorsEliminated = Convert.ToInt32(entries[entryIndex]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set alliedFactorsEliminated set to - " + GlobalDefinitions.alliedFactorsEliminated);
        entryIndex++;
        GlobalDefinitions.germanFactorsEliminated = Convert.ToInt32(entries[entryIndex]);
        GlobalDefinitions.writeToLogFile("readGlobalVariables: Set germanFactorsEliminated set to - " + GlobalDefinitions.germanFactorsEliminated);
        GlobalDefinitions.guiDisplayAlliedVictoryStatus();
    }

    /// <summary>
    /// Reads the configuration settings from the configuration file
    /// </summary>
    public void readSettingsFile()
    {
        char[] delimiterChars = { ' ' };
        string line;
        string[] switchEntries;

        StreamReader theReader = new StreamReader(GlobalDefinitions.settingsFile);
        using (theReader)
        {
            do
            {
                line = theReader.ReadLine();
                if (line != null)
                {
                    switchEntries = line.Split(delimiterChars);
                    switch (switchEntries[0])
                    {
                        case "Difficulty":
                            GlobalDefinitions.writeToLogFile("readSettingsFile: " + line);
                            GlobalDefinitions.difficultySetting = Convert.ToInt32(switchEntries[1]);
                            break;
                        case "Aggressive":
                            GlobalDefinitions.writeToLogFile("readSettingsFile: " + line);
                            GlobalDefinitions.aggressiveSetting = Convert.ToInt32(switchEntries[1]);
                            break;

                    }
                }
            }
            while (line != null);
            theReader.Close();
        }
    }

    /// <summary>
    /// Write out the current configuration settings to the configuration file
    /// </summary>
    /// <param name="difficultySetting"></param>
    /// <param name="aggresivenessSetting"></param>
    public void writeSettingsFile(int difficultySetting, int aggressiveSetting)
    {
        if (File.Exists(GlobalDefinitions.settingsFile))
            File.Delete(GlobalDefinitions.settingsFile);
        StreamWriter fileWriter = new StreamWriter(GlobalDefinitions.settingsFile);
        GlobalDefinitions.writeToLogFile("writeSettingsFile: creating file = " + GlobalDefinitions.settingsFile);
        if (fileWriter == null)
            GlobalDefinitions.guiUpdateStatusMessage("Unable to create the settings file " + GameControl.path + "TGCSettingsFile.txt");
        else
        {
            GlobalDefinitions.difficultySetting = Convert.ToInt32(difficultySetting);
            GlobalDefinitions.aggressiveSetting = Convert.ToInt32(aggressiveSetting);

            fileWriter.WriteLine("Difficulty " + difficultySetting);
            GlobalDefinitions.writeToLogFile("writeSettingsFile: Difficulty = " + difficultySetting);
            fileWriter.WriteLine("Aggressive " + aggressiveSetting);
            GlobalDefinitions.writeToLogFile("writeSettingsFile: Aggressive = " + aggressiveSetting);
            fileWriter.Close();
        }
    }
}
