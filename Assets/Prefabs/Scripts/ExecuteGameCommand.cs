using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ExecuteGameCommand : MonoBehaviour {


    /// <summary>
    /// This routine is what processes the message received from the opponent computer
    /// </summary>
    /// <param name="message"></param>
    public static void ProcessCommand(string message)
    {
        char[] delimiterChars = { ' ' };
        string[] switchEntries = message.Split(delimiterChars);

        string[] lineEntries = message.Split(delimiterChars);
        // I am going to use the same routine to read records that is used when reading from a file.
        // In order to do this I need to drop the first word on the line since the files don't have key words
        for (int index = 0; index < (lineEntries.Length - 1); index++)
            lineEntries[index] = lineEntries[index + 1];

        switch (switchEntries[0])
        {
            case "TURNFILELINE":
                GlobalDefinitions.WriteToLogFile(message);
                break;



            case GlobalDefinitions.PLAYSIDEKEYWORD:
                if (switchEntries[1] == "German")
                    GlobalDefinitions.sideControled = GlobalDefinitions.Nationality.German;
                else
                    GlobalDefinitions.sideControled = GlobalDefinitions.Nationality.Allied;
                break;
            case GlobalDefinitions.PASSCONTROLKEYWORK:
                GlobalDefinitions.SwitchLocalControl(true);
                GlobalDefinitions.WriteToLogFile("processNetworkMessage: Message received to set local control");
                break;
            case GlobalDefinitions.SETCAMERAPOSITIONKEYWORD:
                Camera.main.transform.position = new Vector3(float.Parse(switchEntries[1]), float.Parse(switchEntries[2]), float.Parse(switchEntries[3]));
                Camera.main.GetComponent<Camera>().orthographicSize = float.Parse(switchEntries[4]);
                break;
            case GlobalDefinitions.MOUSESELECTIONKEYWORD:
                if (switchEntries[1] != "null")
                    GameControl.inputMessage.GetComponent<InputMessage>().hex = GameObject.Find(switchEntries[1]);
                else
                    GameControl.inputMessage.GetComponent<InputMessage>().hex = null;

                if (switchEntries[2] != "null")
                    GameControl.inputMessage.GetComponent<InputMessage>().unit = GameObject.Find(switchEntries[2]);
                else
                    GameControl.inputMessage.GetComponent<InputMessage>().unit = null;
                
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.executeMethod(GameControl.inputMessage.GetComponent<InputMessage>());
                break;
            case GlobalDefinitions.MOUSEDOUBLECLICKIONKEYWORD:
                GlobalDefinitions.Nationality passedNationality;

                if (switchEntries[2] == "German")
                    passedNationality = GlobalDefinitions.Nationality.German;
                else
                    passedNationality = GlobalDefinitions.Nationality.Allied;


                if (GlobalDefinitions.selectedUnit != null)
                    GlobalDefinitions.UnhighlightUnit(GlobalDefinitions.selectedUnit);
                foreach (Transform hex in GameObject.Find("Board").transform)
                    GlobalDefinitions.UnhighlightHex(hex.gameObject);
                GlobalDefinitions.selectedUnit = null;


                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().CallMultiUnitDisplay(GameObject.Find(switchEntries[1]), passedNationality);
                break;
            case GlobalDefinitions.DISPLAYCOMBATRESOLUTIONKEYWORD:
                CombatResolutionRoutines.CombatResolutionDisplay();
                break;
            case GlobalDefinitions.NEXTPHASEKEYWORD:
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.ExecuteQuit();
                break;

            case GlobalDefinitions.EXECUTETACTICALAIROKKEYWORD:
                TacticalAirToggleRoutines.TacticalAirOK();
                break;
            case GlobalDefinitions.ADDCLOSEDEFENSEKEYWORD:
                GameObject.Find("CloseDefense").GetComponent<TacticalAirToggleRoutines>().AddCloseDefenseHex();
                break;
            case GlobalDefinitions.CANCELCLOSEDEFENSEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().CancelCloseDefense();
                break;
            case GlobalDefinitions.LOCATECLOSEDEFENSEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().LocateCloseDefense();
                break;
            case GlobalDefinitions.ADDRIVERINTERDICTIONKEYWORD:
                GameObject.Find("RiverInterdiction").GetComponent<TacticalAirToggleRoutines>().AddRiverInterdiction();
                break;
            case GlobalDefinitions.CANCELRIVERINTERDICTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().CancelRiverInterdiction();
                break;
            case GlobalDefinitions.LOCATERIVERINTERDICTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().LocateRiverInterdiction();
                break;
            case GlobalDefinitions.ADDUNITINTERDICTIONKEYWORD:
                GameObject.Find("UnitInterdiction").GetComponent<TacticalAirToggleRoutines>().AddInterdictedUnit();
                break;
            case GlobalDefinitions.CANCELUNITINTERDICTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().CancelInterdictedUnit();
                break;
            case GlobalDefinitions.LOCATEUNITINTERDICTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().LocateInterdictedUnit();
                break;
            case GlobalDefinitions.TACAIRMULTIUNITSELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<TacticalAirToggleRoutines>().MultiUnitSelection();
                break;

            case GlobalDefinitions.MULTIUNITSELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.MULTIUNITSELECTIONCANCELKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<MultiUnitMovementToggleRoutines>().CancelGui();
                break;
            case GlobalDefinitions.LOADCOMBATKEYWORD:
                GameObject GUIButtonInstance = new GameObject("GUIButtonInstance");
                GUIButtonInstance.AddComponent<GUIButtonRoutines>();
                GUIButtonInstance.GetComponent<GUIButtonRoutines>().LoadCombat();
                break;

            case GlobalDefinitions.SETCOMBATTOGGLEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.RESETCOMBATTOGGLEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.COMBATGUIOKKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatGUIOK>().OkCombatGUISelection();
                break;
            case GlobalDefinitions.COMBATGUICANCELKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatGUIOK>().CancelCombatGUISelection();
                break;

            case GlobalDefinitions.ADDCOMBATAIRSUPPORTKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.REMOVECOMBATAIRSUPPORTKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.COMBATRESOLUTIONSELECTEDKEYWORD:
                // Load the combat results; the die roll is on the Global variable
                //GlobalDefinitions.writeToLogFile("Die Roll 1 = " + GlobalDefinitions.dieRollResult1);
                //GlobalDefinitions.writeToLogFile("Die Roll 2 = " + GlobalDefinitions.dieRollResult2);
                GameObject.Find(switchEntries[1]).GetComponent<CombatResolutionButtonRoutines>().ResolutionSelected();
                break;
            case GlobalDefinitions.COMBATLOCATIONSELECTEDKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatResolutionButtonRoutines>().LocateAttack();
                break;
            case GlobalDefinitions.COMBATCANCELKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatResolutionButtonRoutines>().CancelAttack();
                break;
            case GlobalDefinitions.COMBATOKKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CombatResolutionButtonRoutines>().Ok();
                break;
            case GlobalDefinitions.CARPETBOMBINGRESULTSSELECTEDKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.RETREATSELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.SELECTPOSTCOMBATMOVEMENTKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.DESELECTPOSTCOMBATMOVEMENTKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.ADDEXCHANGEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.REMOVEEXCHANGEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.OKEXCHANGEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<ExchangeOKRoutines>().ExchangeOKSelected();
                break;
            case GlobalDefinitions.POSTCOMBATOKKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<PostCombatMovementOkRoutines>().ExecutePostCombatMovement();
                break;
            case GlobalDefinitions.DISPLAYALLIEDSUPPLYKEYWORD:
                if (switchEntries[1] == "True")
                    GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().CreateSupplySourceGUI(true);
                else
                    GameControl.supplyRoutinesInstance.GetComponent<SupplyRoutines>().CreateSupplySourceGUI(false);
                break;
            case GlobalDefinitions.SETSUPPLYKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.RESETSUPPLYKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = false;
                break;
            case GlobalDefinitions.LOCATESUPPLYKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<SupplyButtonRoutines>().LocateSupplySource();
                break;
            case GlobalDefinitions.OKSUPPLYKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<SupplyButtonRoutines>().OkSupply();
                break;
            case GlobalDefinitions.OKSUPPLYWITHENDPHASEKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<SupplyButtonRoutines>().OkSupplyWithEndPhase();
                break;
            case GlobalDefinitions.CHANGESUPPLYSTATUSKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.YESBUTTONSELECTEDKEYWORD:
                GameObject.Find("YesButton").GetComponent<YesNoButtonRoutines>().YesButtonSelected();
                break;
            case GlobalDefinitions.NOBUTTONSELECTEDKEYWORD:
                GameObject.Find("NoButton").GetComponent<YesNoButtonRoutines>().NoButtonSelected();
                break;
            case GlobalDefinitions.SAVEFILENAMEKEYWORD:
                if (File.Exists(GameControl.path + "TGCOutputFiles\\TGCRemoteSaveFile.txt"))
                    File.Delete(GameControl.path + "TGCOutputFiles\\TGCRemoteSaveFile.txt");
                break;
            case GlobalDefinitions.SENDSAVEFILELINEKEYWORD:
                using (StreamWriter saveFile = File.AppendText(GameControl.path + "TGCOutputFiles\\TGCRemoteSaveFile.txt"))
                {
                    for (int index = 1; index < (switchEntries.Length); index++)
                        saveFile.Write(switchEntries[index] + " ");
                    saveFile.WriteLine();
                }
                break;
            case GlobalDefinitions.PLAYNEWGAMEKEYWORD:
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState = GameControl.setUpStateInstance.GetComponent<SetUpState>();
                GameControl.gameStateControlInstance.GetComponent<GameStateControl>().currentState.Initialize();

                // Set the global parameter on what file to use, can't pass it to the executeNoResponse since it is passed as a method delegate elsewhere
                GlobalDefinitions.germanSetupFileUsed = Convert.ToInt32(switchEntries[1]);

                GameControl.setUpStateInstance.GetComponent<SetUpState>().ExecuteNewGame();
                break;

            case GlobalDefinitions.INVASIONAREASELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.CARPETBOMBINGSELECTIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<Toggle>().isOn = true;
                break;
            case GlobalDefinitions.CARPETBOMBINGLOCATIONKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CarpetBombingToggleRoutines>().LocateCarpetBombingHex();
                break;
            case GlobalDefinitions.CARPETBOMBINGOKKEYWORD:
                GameObject.Find(switchEntries[1]).GetComponent<CarpetBombingOKRoutines>().CarpetBombingOK();
                break;

            case GlobalDefinitions.DIEROLLRESULT1KEYWORD:
                GlobalDefinitions.dieRollResult1 = Convert.ToInt32(switchEntries[1]);
                break;
            case GlobalDefinitions.DIEROLLRESULT2KEYWORD:
                GlobalDefinitions.dieRollResult2 = Convert.ToInt32(switchEntries[1]);
                break;
            case GlobalDefinitions.UNDOKEYWORD:
                GameControl.GUIButtonRoutinesInstance.GetComponent<GUIButtonRoutines>().ExecuteUndo();
                break;
            case GlobalDefinitions.CHATMESSAGEKEYWORD:
                string chatMessage = "";
                for (int index = 0; index < (switchEntries.Length - 1); index++)
                    chatMessage += switchEntries[index + 1] + " ";
                GlobalDefinitions.WriteToLogFile("Chat message received: " + chatMessage);
                GlobalDefinitions.AddChatMessage(chatMessage);
                break;
            case GlobalDefinitions.SENDTURNFILENAMEWORD:
                // This command tells the remote computer what the name of the file is that will provide the saved turn file

                // The file name could have ' ' in it so need to reconstruct the full name
                string receivedFileName;
                receivedFileName = switchEntries[1];
                for (int i = 2; i < switchEntries.Length; i++)
                    receivedFileName = receivedFileName + " " + switchEntries[i];

                GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().initiateFileTransferServer();
                GlobalDefinitions.WriteToLogFile("Received name of save file, calling FileTransferServer: fileName = " + receivedFileName + "  path to save = " + GameControl.path);
                GameControl.fileTransferServerInstance.GetComponent<FileTransferServer>().RequestFile(GlobalDefinitions.opponentIPAddress, receivedFileName, GameControl.path, true);
                break;

            case GlobalDefinitions.DISPLAYALLIEDSUPPLYRANGETOGGLEWORD:
                if (GameObject.Find("AlliedSupplyToggle").GetComponent<Toggle>().isOn)
                    GameObject.Find("AlliedSupplyToggle").GetComponent<Toggle>().isOn = false;
                else
                    GameObject.Find("AlliedSupplyToggle").GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.DISPLAYGERMANSUPPLYRANGETOGGLEWORD:
                if (GameObject.Find("GermanSupplyToggle").GetComponent<Toggle>().isOn)
                    GameObject.Find("GermanSupplyToggle").GetComponent<Toggle>().isOn = false;
                else
                    GameObject.Find("GermanSupplyToggle").GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.DISPLAYMUSTATTACKTOGGLEWORD:
                if (GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn)
                    GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn = false;
                else
                    GlobalDefinitions.MustAttackToggle.GetComponent<Toggle>().isOn = true;
                break;

            case GlobalDefinitions.TOGGLEAIRSUPPORTCOMBATTOGGLE:
                {
                    if (GlobalDefinitions.combatAirSupportToggle != null)
                    {
                        if (GlobalDefinitions.combatAirSupportToggle.GetComponent<Toggle>().isOn)
                            GlobalDefinitions.combatAirSupportToggle.GetComponent<Toggle>().isOn = false;
                        else
                            GlobalDefinitions.combatAirSupportToggle.GetComponent<Toggle>().isOn = true;
                    }
                    break;
                }

            case GlobalDefinitions.TOGGLECARPETBOMBINGCOMBATTOGGLE:
                {
                    if (GlobalDefinitions.combatCarpetBombingToggle != null)
                    {
                        if (GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn)
                            GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn = false;
                        else
                            GlobalDefinitions.combatCarpetBombingToggle.GetComponent<Toggle>().isOn = true;
                    }
                    break;
                }
            case GlobalDefinitions.DISCONNECTFROMREMOTECOMPUTER:
                {
                    // Quit the game and go back to the main menu
                    GameObject guiButtonInstance = new GameObject("GUIButtonInstance");
                    guiButtonInstance.AddComponent<GUIButtonRoutines>();
                    guiButtonInstance.GetComponent<GUIButtonRoutines>().YesMain();
                    break;
                }
            case GlobalDefinitions.ALLIEDREPLACEMENTKEYWORD:
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().SelectAlliedReplacementUnit(GameObject.Find(switchEntries[1]));
                break;
            case GlobalDefinitions.GERMANREPLACEMENTKEYWORD:
                GameControl.movementRoutinesInstance.GetComponent<MovementRoutines>().SelectGermanReplacementUnit(GameObject.Find(switchEntries[1]));
                break;
            case GlobalDefinitions.AGGRESSIVESETTINGKEYWORD:
                GlobalDefinitions.aggressiveSetting = Convert.ToInt32(switchEntries[1]);
                break;
            case GlobalDefinitions.DIFFICULTYSETTINGKEYWORD:
                GlobalDefinitions.difficultySetting = Convert.ToInt32(switchEntries[1]);
                break;

            default:
                GlobalDefinitions.WriteToLogFile("processCommand: Unknown network command received: " + message);
                break;
        }
    }
}
