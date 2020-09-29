using System.IO;
using UnityEngine;
using System;
using UnityEngine.UI;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class ReadWriteRoutines : MonoBehaviour
    {
        /// <summary>
        /// This is the routine called to write out the save turn file
        /// </summary>
        public void WriteSaveTurnFile(string saveFileType)
        {
            // There are three types of saved files: setup, end of Allied turn, and end of German turn.  The name of the file reflects this using the string passed
            string turnString;
            if (GlobalDefinitions.turnNumber < 10)
                turnString = "0" + GlobalDefinitions.turnNumber.ToString();
            else
                turnString = GlobalDefinitions.turnNumber.ToString();
            StreamWriter fileWriter = new StreamWriter(GameControl.path + "TGCOutputFiles\\TGCSaveFile_Turn" + turnString + "_" + saveFileType + ".txt");
            if (fileWriter == null)
                GlobalDefinitions.GuiUpdateStatusMessage("Unable to open save file " + GameControl.path + "TGCOutputFiles\\TGCSaveFile_Turn" + turnString + "_" + saveFileType + ".txt");
            else
            {
                fileWriter.WriteLine("Turn " + GlobalDefinitions.turnNumber);

                fileWriter.Write("Global_Definitions ");
                GlobalDefinitions.WriteGlobalVariables(fileWriter);

                fileWriter.WriteLine("Hexes");
                foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
                {
                    hex.GetComponent<HexDatabaseFields>().WriteHexFields(fileWriter);
                }
                fileWriter.WriteLine("End");
                fileWriter.WriteLine("Units");
                foreach (Transform unit in GameObject.Find("Units Eliminated").transform)
                {
                    unit.GetComponent<UnitDatabaseFields>().WriteUnitFields(fileWriter);
                }
                foreach (Transform unit in GlobalDefinitions.allUnitsOnBoard.transform)
                {
                    unit.GetComponent<UnitDatabaseFields>().WriteUnitFields(fileWriter);
                }
                foreach (Transform unit in GameObject.Find("Units In Britain").transform)
                {
                    unit.GetComponent<UnitDatabaseFields>().WriteUnitFields(fileWriter);
                }
                fileWriter.WriteLine("End");

                // In network mode the Game Control loads the currentState and it works best if everything is set before it gets called.
                fileWriter.Write("Game_Control ");

                if (saveFileType == "Setup")
                    fileWriter.Write("German");
                else
                    fileWriter.Write(GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.currentNationality);

                fileWriter.WriteLine();
                fileWriter.Close();

                // A new saved file has been written so delete the current command file and load the new turn file
                if (!GlobalDefinitions.commandFileBeingRead)
                {
                    if (File.Exists(GameControl.path + GlobalGameFields.commandFile))
                        GlobalDefinitions.DeleteCommandFile();
                    using (StreamWriter writeFile = File.AppendText(GameControl.path + GlobalGameFields.commandFile))
                    {
                        writeFile.WriteLine(GlobalDefinitions.AGGRESSIVESETTINGKEYWORD + " " + GlobalDefinitions.aggressiveSetting);
                        writeFile.WriteLine(GlobalDefinitions.DIFFICULTYSETTINGKEYWORD + " " + GlobalDefinitions.difficultySetting);
                        writeFile.WriteLine("SavedTurnFile " + GameControl.path + "TGCOutputFiles\\TGCSaveFile_Turn" + turnString + "_" + saveFileType + ".txt");
                    }
                }
            }
        }

        /// <summary>
        /// This routine reads the contents of a save file
        /// </summary>
        /// <param name="fileName"></param>
        public void ReadTurnFile(string fileName)
        {
            char[] delimiterChars = { ' ' };
            string line;
            string[] lineEntries;
            string[] switchEntries;

            GlobalDefinitions.WriteToLogFile("readTurnFile: executing - passed file = " + fileName);

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
                                GlobalDefinitions.turnNumber = Convert.ToInt32(switchEntries[1]);
                                GlobalDefinitions.GuiUpdateTurn();
                                break;
                            case "Game_Control":
                                GameControl.SetGameState(switchEntries[1]);
                                break;
                            case "Global_Definitions":
                                ReadGlobalVariables(switchEntries);
                                break;
                            case "Hexes":
                                line = theReader.ReadLine();
                                lineEntries = line.Split(delimiterChars);
                                while (lineEntries[0] != "End")
                                {
                                    string[] entries = line.Split(delimiterChars);
                                    ProcessHexRecord(entries);

                                    line = theReader.ReadLine();
                                    lineEntries = line.Split(delimiterChars);
                                }
                                break;
                            case "Units":
                                line = theReader.ReadLine();
                                lineEntries = line.Split(delimiterChars);
                                while (lineEntries[0] != "End")
                                {
                                    string[] entries = line.Split(delimiterChars);
                                    ProcessUnitRecord(entries);

                                    line = theReader.ReadLine();
                                    lineEntries = line.Split(delimiterChars);
                                }
                                break;
                        }
                    }
                }
                while (line != null);
                theReader.Close();

                GlobalDefinitions.WriteToLogFile("readTurnFile: File read complete.  Initialize Game State");

                // If we just read a setup file, put the player into the setup state so that he can update the setup if he wants.
                if ((GlobalDefinitions.turnNumber == 0) && (GlobalDefinitions.nationalityUserIsPlaying == GlobalDefinitions.Nationality.German))
                {
                    GlobalDefinitions.nextPhaseButton.GetComponent<Button>().interactable = true;
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod =
                                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.GetComponent<SetUpState>().ExecuteSelectUnit;
                }
                // Otherwise, execute the init for the next state
                else
                    GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();
            }
        }

        /// <summary>
        /// This routine reads a single record for a hex
        /// </summary>
        /// <param name="entries"></param>
        public void ProcessHexRecord(string[] entries)
        {
            GameObject hex;

            hex = GameObject.Find(entries[0]);

            hex.GetComponent<HexDatabaseFields>().inGermanZOC = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[1]);
            hex.GetComponent<HexDatabaseFields>().inAlliedZOC = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[2]);
            hex.GetComponent<HexDatabaseFields>().alliedControl = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[3]);
            hex.GetComponent<HexDatabaseFields>().successfullyInvaded = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[4]);
            hex.GetComponent<HexDatabaseFields>().closeDefenseSupport = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[5]);
            hex.GetComponent<HexDatabaseFields>().riverInterdiction = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[6]);

            // By calling the unhighlight rouitne (no hexes should be highlighted at this point) it will 
            // turn close defense and interdicted river hexes the correct color
            GlobalDefinitions.UnhighlightHex(GameObject.Find(entries[0]));
        }

        /// <summary>
        /// This routine reads a single record for a unit
        /// </summary>
        /// <param name="entries"></param>
        public void ProcessUnitRecord(string[] entries)
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
            unit.GetComponent<UnitDatabaseFields>().inBritain = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[3]);
            unit.GetComponent<UnitDatabaseFields>().unitInterdiction = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[4]);
            unit.GetComponent<UnitDatabaseFields>().invasionAreaIndex = Convert.ToInt32(entries[5]);
            unit.GetComponent<UnitDatabaseFields>().availableForStrategicMovement = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[6]);
            unit.GetComponent<UnitDatabaseFields>().inSupply = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[7]);
            // Need to adjust the highlighting of the unit if it is out of supply
            GlobalDefinitions.UnhighlightUnit(unit);
            if (entries[8] == "null")
                unit.GetComponent<UnitDatabaseFields>().supplySource = null;
            else
                unit.GetComponent<UnitDatabaseFields>().supplySource = GameObject.Find(entries[8]);
            unit.GetComponent<UnitDatabaseFields>().supplyIncrementsOutOfSupply = Convert.ToInt32(entries[9]);
            unit.GetComponent<UnitDatabaseFields>().unitEliminated = GlobalDefinitions.ReturnBoolFromSaveFormat(entries[10]);


            if (unit.GetComponent<UnitDatabaseFields>().occupiedHex != null)
            {
                GeneralHexRoutines.PutUnitOnHex(unit, unit.GetComponent<UnitDatabaseFields>().occupiedHex);
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
                GlobalDefinitions.WriteToLogFile("processUnitRecord: Unit read error - " + entries[1] + ": found no location to place this unit");
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
        public void ReadGlobalVariables(string[] entries)
        {
            GlobalDefinitions.numberOfCarpetBombingsUsed = Convert.ToInt32(entries[1]);
            GlobalDefinitions.numberInvasionsExecuted = Convert.ToInt32(entries[2]);
            GlobalDefinitions.firstInvasionAreaIndex = Convert.ToInt32(entries[3]);
            if (GlobalDefinitions.firstInvasionAreaIndex != -1)
            {
                GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].invaded = true;
            }
            GlobalDefinitions.secondInvasionAreaIndex = Convert.ToInt32(entries[4]);
            if (GlobalDefinitions.secondInvasionAreaIndex != -1)
            {
                GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].invaded = true;
            }
            GlobalDefinitions.germanReplacementsRemaining = Convert.ToInt32(entries[5]);
            GlobalDefinitions.alliedReplacementsRemaining = Convert.ToInt32(entries[6]);
            GlobalDefinitions.alliedCapturedBrest = Convert.ToBoolean(entries[7]);
            GlobalDefinitions.alliedCapturedBoulogne = Convert.ToBoolean(entries[8]);
            GlobalDefinitions.alliedCapturedRotterdam = Convert.ToBoolean(entries[9]);
            if (GlobalDefinitions.firstInvasionAreaIndex != -1)
            {
                GlobalDefinitions.invasionAreas[GlobalDefinitions.firstInvasionAreaIndex].turn = Convert.ToInt32(entries[10]);
            }
            if (GlobalDefinitions.secondInvasionAreaIndex != -1)
            {
                GlobalDefinitions.invasionAreas[GlobalDefinitions.secondInvasionAreaIndex].turn = Convert.ToInt32(entries[11]);
            }
            GlobalDefinitions.numberOfTurnsWithoutSuccessfulAttack = Convert.ToInt32(entries[12]);

            GlobalDefinitions.hexesAttackedLastTurn.Clear();
            int loopLimit = Convert.ToInt32(entries[13]);
            int entryIndex = 13;
            for (int index = 0; index < loopLimit; index++)
            {
                entryIndex++;
                GlobalDefinitions.hexesAttackedLastTurn.Add(GameObject.Find(entries[entryIndex]));
            }

            entryIndex++;
            GlobalDefinitions.combatResultsFromLastTurn.Clear();
            loopLimit = Convert.ToInt32(entries[entryIndex]);
            for (int index = 0; index < loopLimit; index++)
            {
                entryIndex++;

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
            entryIndex++;
            GlobalDefinitions.alliedFactorsEliminated = Convert.ToInt32(entries[entryIndex]);
            entryIndex++;
            GlobalDefinitions.germanFactorsEliminated = Convert.ToInt32(entries[entryIndex]);

            entryIndex++;
            GlobalDefinitions.easiestDifficultySettingUsed = Convert.ToInt32(entries[entryIndex]);

            GlobalDefinitions.GuiUpdateLossRatioText();
            GlobalDefinitions.GuiDisplayAlliedVictoryStatus();
        }

        /// <summary>
        /// Reads the configuration settings from the configuration file
        /// </summary>
        public void ReadSettingsFile()
        {
            char[] delimiterChars = { ' ' };
            string line;
            string[] switchEntries;

            StreamReader theReader = new StreamReader(GlobalGameFields.settingsFile);
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
                                GlobalDefinitions.WriteToLogFile("readSettingsFile: " + line);
                                GlobalDefinitions.difficultySetting = Convert.ToInt32(switchEntries[1]);
                                break;
                            case "Aggressive":
                                GlobalDefinitions.WriteToLogFile("readSettingsFile: " + line);
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
        public void WriteSettingsFile(int difficultySetting, int aggressiveSetting)
        {
            if (File.Exists(GlobalGameFields.settingsFile))
                File.Delete(GlobalGameFields.settingsFile);
            StreamWriter fileWriter = new StreamWriter(GlobalGameFields.settingsFile);
            GlobalDefinitions.WriteToLogFile("writeSettingsFile: creating file = " + GlobalGameFields.settingsFile);
            if (fileWriter == null)
                GlobalDefinitions.GuiUpdateStatusMessage("Unable to create the settings file " + GameControl.path + "TGCSettingsFile.txt");
            else
            {
                GlobalDefinitions.difficultySetting = Convert.ToInt32(difficultySetting);
                GlobalDefinitions.aggressiveSetting = Convert.ToInt32(aggressiveSetting);

                fileWriter.WriteLine("Difficulty " + difficultySetting);
                GlobalDefinitions.WriteToLogFile("writeSettingsFile: Difficulty = " + difficultySetting);
                fileWriter.WriteLine("Aggressive " + aggressiveSetting);
                GlobalDefinitions.WriteToLogFile("writeSettingsFile: Aggressive = " + aggressiveSetting);
                fileWriter.Close();
            }
        }
    }
}