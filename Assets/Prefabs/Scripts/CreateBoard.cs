using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using Convert = System.Convert;
using Exception = System.Exception;

using System.IO;
using UnityEngine;

//[ExecuteInEditMode]

/// <summary>
/// This is the main routine that creates the hexes under the picture of the board
/// </summary>
public class CreateBoard : MonoBehaviour
{

    public Vector3 v3Center = Vector3.zero;
    public float edgeLength = 3.95f;  // Length of the hex edge
    //public float edgeLength = 2.133f;  // Length of the hex edge scaled with 0.54
    public GameObject hexagonPrefab;
    private GameObject hexInstance;
    private Vector3 v3Pos;

    /// <summary>
    /// This is the routine that reads the file that sets up the hexes
    /// </summary>
    /// <param name="fileName"></param>
    public void ReadMapSetup(string fileName)
    {
        char[] delimiterChars = { ' ', ',', ')', '(' };
        List<string> storedCoordinates = new List<string>();
        string line;
        string riverName;
        string cityName;

        int hexPositionX;
        int hexPositionY;
        int lineNumber = 0;

        GameObject unit;

        // The following variables are used to determine what words are needed when reading the file
        int hexXPositionWord = 1;
        int hexYPositionWord = 2;
        int hexTypeWord = 4;
        int hexCityNameWord = 5;
        int hexSupplyCapacity = 5;
        int hexInvasionIndex = 6;
        int hexPortNameWord = 7;
        int seaHexInvasionIndex = 5;
        int hexInvasionTargetX = 7;
        int hexInvasionTargetY = 8;
        int inlandPortX = 2;
        int inlandPortY = 3;
        int historicalWeekCaptured = 4;

        int riverX1Word = 1;
        int riverY1Word = 2;
        int riverX2Word = 5;
        int riverY2Word = 6;

        int unitNameWord = 0;
        int unitXWord = 2;
        int unitYWord = 3;

        if (!File.Exists(fileName))
        {
            // There is no recovering from this error but let the user know
            GlobalDefinitions.guiUpdateStatusMessage("Unable to open the map file " + fileName);
            return;
        }

        StreamReader theReader = new StreamReader(fileName);

        using (theReader)
        {
            do
            {
                line = theReader.ReadLine();
                //GlobalDefinitions.writeToLogFile("readMapSetup: reading line - " + line);
                lineNumber++;
                if (line != null)
                {
                    string[] switchEntries = line.Split(delimiterChars);
                    //GlobalDefinitions.writeToLogFile("readMapSetup: Keyword found - " + switchEntries[0]);
                    switch (switchEntries[0])
                    {
                        case "Turn":
                            GlobalDefinitions.turnNumber = Int32.Parse(switchEntries[1]);
                            break;
                        case "Hexes":
                            line = theReader.ReadLine();
                            lineNumber++;
                            while (line != "}")
                            {
                                // Note the entires will have two "extra" splits because of the ()
                                // The ( begins the line so it returns a null entrie
                                // The ) is followed by another delimiter (" ") so it also returns null
                                string[] entries = line.Split(delimiterChars);
                                if ((entries.Length >= hexCityNameWord) && (entries[hexXPositionWord] != null) &&
                                        (entries[hexYPositionWord] != null) && (entries[hexTypeWord] != null))
                                {
                                    hexPositionX = Convert.ToInt32(entries[hexXPositionWord]);
                                    hexPositionY = Convert.ToInt32(entries[hexYPositionWord]);
                                    if (storedCoordinates.Contains(hexPositionX + "_" + hexPositionY))
                                        GlobalDefinitions.writeToLogFile("Duplicae Hexes at X " + hexPositionX + " Y " + hexPositionY);
                                    else
                                        storedCoordinates.Add(hexPositionX + "_" + hexPositionY);
                                    v3Pos.x = ((3f * edgeLength) / 2f) * hexPositionX;
                                    if ((hexPositionX % 2) == 0)
                                        v3Pos.y = -Mathf.Sqrt(3) * edgeLength;
                                    else
                                        v3Pos.y = -Mathf.Sqrt(3) / 2 * edgeLength;
                                    v3Pos.y += Mathf.Sqrt(3) * edgeLength * hexPositionY;
                                    v3Pos.z = 0;

                                    // When I first started writing this I had a differnt resource for each type of hex.
                                    // Not that I am overlaying a picture of the actual board there really is no reason 
                                    // to load special hexes so I'm just loading a plan hex for highlighting purposes.
                                    //hexagonPrefab = (GameObject)Resources.Load(entries[hexTypeWord]);
                                    hexagonPrefab = (GameObject)Resources.Load("Land");

                                    if (hexagonPrefab == null)
                                    {

                                        GlobalDefinitions.writeToLogFile("Returned null for " + hexPositionX + " " + hexPositionY + " " + entries[hexTypeWord]);
                                        GlobalDefinitions.writeToLogFile("Hex instance did not instantiate at x" + hexPositionX + " y" + hexPositionY);
                                        GlobalDefinitions.writeToLogFile("  Transform position = x" + v3Pos.x + " y " + v3Pos.y);
                                        GlobalDefinitions.writeToLogFile("  Hex type = " + entries[hexTypeWord]);
                                    }
                                    else
                                    {
                                        hexInstance = Instantiate(hexagonPrefab);
                                        if (hexInstance == null)
                                            GlobalDefinitions.writeToLogFile("Hex did not instantiate");

                                        hexInstance.name = entries[hexTypeWord] + "_x" + hexPositionX + "_y" + hexPositionY;
                                        hexInstance.transform.position = v3Pos;
                                        hexInstance.transform.SetParent(GameObject.Find("Board").transform);
                                        hexInstance.GetComponent<HexDatabaseFields>().xMapCoor = hexPositionX;
                                        hexInstance.GetComponent<HexDatabaseFields>().yMapCoor = hexPositionY;

                                        // Add the hex to the global list so I don't have to use GameObject.Find all the time
                                        GlobalDefinitions.allHexesOnBoard.Add(hexInstance);
                                        if (entries[hexTypeWord] == "City")
                                        {
                                            // Need to account for city names with blanks in them
                                            if (entries.Length > hexCityNameWord)
                                            {
                                                cityName = entries[hexCityNameWord];
                                                for (int i = (hexCityNameWord + 1); i < entries.Length; i++)
                                                    cityName += "_" + entries[i];
                                                hexInstance.GetComponent<HexDatabaseFields>().hexName = cityName;
                                            }
                                            hexInstance.GetComponent<HexDatabaseFields>().city = true;
                                        }
                                        if (entries[hexTypeWord] == "Fortress")
                                        {
                                            // Need to account for city names with blanks in them
                                            if (entries.Length > hexCityNameWord)
                                            {
                                                cityName = entries[hexCityNameWord];
                                                for (int i = (hexCityNameWord + 1); i < entries.Length; i++)
                                                    cityName += "_" + entries[i];
                                                hexInstance.GetComponent<HexDatabaseFields>().hexName = cityName;
                                            }
                                            hexInstance.GetComponent<HexDatabaseFields>().fortress = true;
                                        }
                                        if (entries[hexTypeWord] == "Port")
                                        {
                                            // In addition to the name ports also have supply capacity
                                            hexInstance.GetComponent<HexDatabaseFields>().supplyCapacity = Int32.Parse(entries[hexSupplyCapacity]);
                                            hexInstance.GetComponent<HexDatabaseFields>().invasionAreaIndex = Int32.Parse(entries[hexInvasionIndex]);

                                            // Need to account for city names with blanks in them
                                            if (entries.Length > hexPortNameWord)
                                            {
                                                cityName = entries[hexPortNameWord];
                                                for (int i = (hexPortNameWord + 1); i < entries.Length; i++)
                                                    cityName += "_" + entries[i];
                                                hexInstance.GetComponent<HexDatabaseFields>().hexName = cityName;
                                            }
                                            hexInstance.GetComponent<HexDatabaseFields>().city = true;
                                            hexInstance.GetComponent<HexDatabaseFields>().coastalPort = true;
                                        }
                                        if (entries[hexTypeWord] == "InlandPort")
                                        {
                                            // In addition to the name ports also have supply capacity
                                            hexInstance.GetComponent<HexDatabaseFields>().supplyCapacity = Int32.Parse(entries[hexSupplyCapacity]);
                                            hexInstance.GetComponent<HexDatabaseFields>().invasionAreaIndex = Int32.Parse(entries[hexInvasionIndex]);

                                            // Need to account for city names with blanks in them
                                            if (entries.Length > hexPortNameWord)
                                            {
                                                cityName = entries[hexPortNameWord];
                                                for (int i = (hexPortNameWord + 1); i < entries.Length; i++)
                                                    cityName += "_" + entries[i];
                                                hexInstance.GetComponent<HexDatabaseFields>().hexName = cityName;
                                            }
                                            hexInstance.GetComponent<HexDatabaseFields>().city = true;
                                            hexInstance.GetComponent<HexDatabaseFields>().inlandPort = true;
                                        }
                                        if (entries[hexTypeWord] == "FortifiedPort")
                                        {
                                            // In addition to the name ports also have supply capacity
                                            hexInstance.GetComponent<HexDatabaseFields>().supplyCapacity = Int32.Parse(entries[hexSupplyCapacity]);
                                            hexInstance.GetComponent<HexDatabaseFields>().invasionAreaIndex = Int32.Parse(entries[hexInvasionIndex]);

                                            // Need to account for city names with blanks in them
                                            if (entries.Length > hexPortNameWord)
                                            {
                                                cityName = entries[hexPortNameWord];
                                                for (int i = (hexPortNameWord + 1); i < entries.Length; i++)
                                                    cityName += "_" + entries[i];
                                                hexInstance.GetComponent<HexDatabaseFields>().hexName = cityName;
                                            }
                                            hexInstance.GetComponent<HexDatabaseFields>().fortress = true;
                                            hexInstance.GetComponent<HexDatabaseFields>().coastalPort = true;
                                        }
                                        if (entries[hexTypeWord] == "InlandFortifiedPort")
                                        {
                                            // In addition to the name ports also have supply capacity
                                            hexInstance.GetComponent<HexDatabaseFields>().supplyCapacity = Int32.Parse(entries[hexSupplyCapacity]);
                                            hexInstance.GetComponent<HexDatabaseFields>().invasionAreaIndex = Int32.Parse(entries[hexInvasionIndex]);

                                            // Need to account for city names with blanks in them
                                            if (entries.Length > hexPortNameWord)
                                            {
                                                cityName = entries[hexPortNameWord];
                                                for (int i = (hexPortNameWord + 1); i < entries.Length; i++)
                                                    cityName += "_" + entries[i];
                                                hexInstance.GetComponent<HexDatabaseFields>().hexName = cityName;
                                            }
                                            hexInstance.GetComponent<HexDatabaseFields>().fortress = true;
                                            hexInstance.GetComponent<HexDatabaseFields>().inlandPort = true;
                                        }
                                        if (entries[hexTypeWord] == "Coast")
                                        {
                                            // Store the supply capacity
                                            hexInstance.GetComponent<HexDatabaseFields>().supplyCapacity = Int32.Parse(entries[hexSupplyCapacity]);
                                            hexInstance.GetComponent<HexDatabaseFields>().invasionAreaIndex = Int32.Parse(entries[hexInvasionIndex]);
                                            hexInstance.GetComponent<HexDatabaseFields>().coast = true;
                                            // The only time the name will be used for coast hex is if it is a successful beach invasion hex
                                            hexInstance.GetComponent<HexDatabaseFields>().hexName = "Invasion Beach";
                                        }
                                        if (entries[hexTypeWord] == "Mountain")
                                        {
                                            hexInstance.GetComponent<HexDatabaseFields>().mountain = true;
                                        }
                                        if (entries[hexTypeWord] == "Impassible")
                                        {
                                            hexInstance.GetComponent<HexDatabaseFields>().impassible = true;
                                        }
                                        if (entries[hexTypeWord] == "FortifiedReplacement")
                                        {
                                            hexInstance.GetComponent<HexDatabaseFields>().fortifiedZone = true;
                                            hexInstance.GetComponent<HexDatabaseFields>().germanRepalcement = true;
                                        }
                                        if (entries[hexTypeWord] == "Replacement")
                                        {
                                            hexInstance.GetComponent<HexDatabaseFields>().germanRepalcement = true;
                                        }
                                        if (entries[hexTypeWord] == "Fortified")
                                        {
                                            hexInstance.GetComponent<HexDatabaseFields>().fortifiedZone = true;
                                        }
                                        if (entries[hexTypeWord] == "Bridge")
                                        {
                                            hexInstance.GetComponent<HexDatabaseFields>().bridge = true;
                                        }
                                        if (entries[hexTypeWord] == "Sea")
                                        {
                                            hexInstance.GetComponent<HexDatabaseFields>().sea = true;
                                            hexInstance.GetComponent<HexDatabaseFields>().invasionAreaIndex = Int32.Parse(entries[seaHexInvasionIndex]);
                                            hexInstance.GetComponent<HexDatabaseFields>().invasionTargetX = Convert.ToInt32(entries[hexInvasionTargetX]);
                                            hexInstance.GetComponent<HexDatabaseFields>().invasionTargetY = Convert.ToInt32(entries[hexInvasionTargetY]);
                                        }
                                    }
                                }
                                else
                                {
                                    // There were not 5 entries on the Hex line which means something is wrong, or one of the fields was null.
                                    GlobalDefinitions.writeToLogFile("Error in file on Hex line " + lineNumber + " - entries should be 5 or greater there are " + entries.Length);
                                }

                                line = theReader.ReadLine();
                                lineNumber++;
                            }
                            SetupHexNeighbors();
                            break;
                        case "River":
                            // The name of the river will be on the "River" line.  Need to account for river names with blanks in them
                            riverName = "";
                            for (int i = 1; i < (switchEntries.Length - 1); i++)  // The last character on the line is { which is why the check is for Length-1
                                riverName += switchEntries[i];

                            line = theReader.ReadLine();
                            lineNumber++;
                            while (line != "}")
                            {
                                //  The number of entries is more than you would think because of the ()'s.
                                // Even though they are delimiters they always appear at the start of an entry which results in a null entry
                                string[] entries = line.Split(delimiterChars);
                                if (entries.Length == (riverY2Word + 2))
                                {
                                    DrawRiverBetweenHexes(GlobalDefinitions.getHexAtXY(
                                            Convert.ToInt32(entries[riverX1Word]),
                                            Convert.ToInt32(entries[riverY1Word])),
                                            GlobalDefinitions.getHexAtXY(
                                            Convert.ToInt32(entries[riverX2Word]),
                                            Convert.ToInt32(entries[riverY2Word])));
                                }
                                else
                                {
                                    // There were not 5 entries on the Hex line which means something is wrong
                                    GlobalDefinitions.writeToLogFile("Error in file on River line " + lineNumber + " - entries should be " + (riverY2Word + 2) + " but there are " + entries.Length);
                                    GlobalDefinitions.writeToLogFile("    line text - " + line);
                                }
                                line = theReader.ReadLine();
                                lineNumber++;
                            }
                            break;
                        case "Units":
                            line = theReader.ReadLine();
                            lineNumber++;
                            while (line != "}")
                            {
                                string[] entries = line.Split(delimiterChars);
                                if (entries.Length == (unitYWord + 2))
                                {
                                    unit = GameObject.Find(entries[unitNameWord]);
                                    if (unit == null)
                                        GlobalDefinitions.writeToLogFile("Unable to find unit - " + entries[0]);
                                    else
                                    {
                                        unit.GetComponent<UnitDatabaseFields>().occupiedHex = GlobalDefinitions.getHexAtXY(
                                                Convert.ToInt32(entries[unitXWord]), Convert.ToInt32(entries[unitYWord]));
                                    }
                                    GameControl.setupRoutinesInstance.GetComponent<SetupRoutines>().getUnitSetupDestination(unit, Convert.ToInt32(entries[unitXWord]), Convert.ToInt32(entries[unitYWord]));
                                    // Assign the unit to be on the board
                                    unit.transform.parent = GlobalDefinitions.allUnitsOnBoard.transform;
                                    unit.GetComponent<UnitDatabaseFields>().inBritain = false;
                                }
                                else
                                {
                                    // There need to be 15 entries on the line (the () count as one each
                                    GlobalDefinitions.writeToLogFile("Error for unit " + entries[unitNameWord] + " in file on line " + lineNumber + " should be 5 entries but there are " + entries.Length);
                                }
                                line = theReader.ReadLine();
                                lineNumber++;
                            }
                            break;
                        case "InlandPort":
                            hexInstance = GlobalDefinitions.getHexAtXY(Convert.ToInt32(switchEntries[inlandPortX]), Convert.ToInt32(switchEntries[inlandPortY]));
                            line = theReader.ReadLine();
                            lineNumber++;
                            while (line != "}")
                            {
                                string[] entries = line.Split(delimiterChars);
                                hexInstance.GetComponent<HexDatabaseFields>().controlHexes.Add(GlobalDefinitions.getHexAtXY(Convert.ToInt32(entries[hexXPositionWord]), Convert.ToInt32(entries[hexYPositionWord])));
                                line = theReader.ReadLine();
                                lineNumber++;
                            }
                            break;
                        case "FreeFrench":
                            line = theReader.ReadLine();
                            lineNumber++;
                            while (line != "}")
                            {
                                string[] entries = line.Split(delimiterChars);
                                GlobalDefinitions.getHexAtXY(Convert.ToInt32(entries[hexXPositionWord]), Convert.ToInt32(entries[hexYPositionWord])).GetComponent<HexDatabaseFields>().FreeFrenchAvailableHex = true;
                                line = theReader.ReadLine();
                                lineNumber++;
                            }
                            break;
                        case "AlliedVictory":
                            line = theReader.ReadLine();
                            lineNumber++;
                            while (line != "}")
                            {
                                string[] entries = line.Split(delimiterChars);
                                GlobalDefinitions.getHexAtXY(Convert.ToInt32(entries[hexXPositionWord]), Convert.ToInt32(entries[hexYPositionWord])).GetComponent<HexDatabaseFields>().AlliedVictoryHex = true;
                                line = theReader.ReadLine();
                                lineNumber++;
                            }
                            break;
                        case "HistoricalProgress":
                            line = theReader.ReadLine();
                            lineNumber++;
                            while (line != "}")
                            {
                                string[] entries = line.Split(delimiterChars);
                                GlobalDefinitions.getHexAtXY(Convert.ToInt32(entries[hexXPositionWord]), Convert.ToInt32(entries[hexYPositionWord])).GetComponent<HexDatabaseFields>().historyWeekCaptured = Convert.ToInt32(entries[historicalWeekCaptured]);
                                line = theReader.ReadLine();
                                lineNumber++;
                            }
                            break;
                        default:
                            GlobalDefinitions.writeToLogFile("ReadMapSetup: unknown header found in the file");
                            break;
                    }
                }
            }
            while (line != null);
            theReader.Close();
        }
    }

    /// <summary>
    /// This routine takes the two hexes passed to it and draws a river between the two hexes.
    /// </summary>
    /// <param name="hex1"></param>
    /// <param name="hex2"></param>
    private void DrawRiverBetweenHexes(GameObject hex1, GameObject hex2)
    {
        // Note this was originally written when the plan was to draw the board rather than to use a picture.  This routine is still needed
        // even though a picture is being used because it sets up the ZOC for the rivers.
        GameObject hexToUse;
        GameObject hexNotUsing;
        Vector3 point1 = new Vector3();
        Vector3 point2 = new Vector3();

        // The first thing is to determine how the two hexes are neighbors.  When drawing the line there may be biases
        // based on the location so in order to make sure all the biases go the same direction I will use whichever
        // hex is the lowest.  The transform position is in the center of the hex so there 
        // are cases where one has the minimum y and the other has a minimum x.  The minimum y will take precedence.
        // Note with the flat-top hex layout we are using the x's can be equal but the y's can't.
        if (hex1.transform.position.y < hex2.transform.position.y)
        {
            hexToUse = hex1;
            hexNotUsing = hex2;
        }
        else
        {
            hexToUse = hex2;
            hexNotUsing = hex1;
        }

        if (hexToUse.GetComponent<HexDatabaseFields>().Neighbors[(int)GlobalDefinitions.HexSides.North] == hexNotUsing)
        {
            // The river lies on the N edge of hexToUse
            point1.x = (float)(hexToUse.transform.position.x - (0.5 * edgeLength));
            point1.y = (float)(hexToUse.transform.position.y + (Mathf.Sqrt(3f) * edgeLength / 2f));
            point1.z = 0f;

            point2.x = (float)(hexToUse.transform.position.x + (0.5 * edgeLength));
            point2.y = (float)(hexToUse.transform.position.y + (Mathf.Sqrt(3f) * edgeLength / 2f));
            point2.z = 0f;

            // Update the ZOC's of the hexes; they do not cross rivers
            hexToUse.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.North] = false;
            hexNotUsing.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.South] = false;
            hexToUse.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.North] = true;
            hexNotUsing.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.South] = true;
        }
        else if (hexToUse.GetComponent<HexDatabaseFields>().Neighbors[(int)GlobalDefinitions.HexSides.NorthEast] == hexNotUsing)
        {
            // The river lies on the NE edge of hexToUse
            point1.x = (float)(hexToUse.transform.position.x + (0.5 * edgeLength));
            point1.y = (float)(hexToUse.transform.position.y + (Mathf.Sqrt(3f) * edgeLength / 2f));
            point1.z = 0f;

            point2.x = (float)(hexToUse.transform.position.x + edgeLength);
            point2.y = (float)(hexToUse.transform.position.y);
            point2.z = 0f;

            // Update the ZOC's of the hexes; they do not cross rivers
            hexToUse.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.NorthEast] = false;
            hexNotUsing.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.SouthWest] = false;
            hexToUse.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.NorthEast] = true;
            hexNotUsing.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.SouthWest] = true;
        }
        else if (hexToUse.GetComponent<HexDatabaseFields>().Neighbors[(int)GlobalDefinitions.HexSides.SouthEast] == hexNotUsing)
        {
            // The river lies on the SE edge of hexToUse
            point1.x = (float)(hexToUse.transform.position.x + edgeLength);
            point1.y = (float)(hexToUse.transform.position.y);
            point1.z = 0f;

            point2.x = (float)(hexToUse.transform.position.x + (0.5 * edgeLength));
            point2.y = (float)(hexToUse.transform.position.y - (Mathf.Sqrt(3f) * edgeLength / 2f));
            point2.z = 0f;

            // Update the ZOC's of the hexes; they do not cross rivers
            hexToUse.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.SouthEast] = false;
            hexNotUsing.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.NorthWest] = false;
            hexToUse.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.SouthEast] = true;
            hexNotUsing.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.NorthWest] = true;
        }
        else if (hexToUse.GetComponent<HexDatabaseFields>().Neighbors[(int)GlobalDefinitions.HexSides.South] == hexNotUsing)
        {
            // The river lies on the S edge of hexToUse
            point1.x = (float)(hexToUse.transform.position.x - (0.5 * edgeLength));
            point1.y = (float)(hexToUse.transform.position.y - (Mathf.Sqrt(3f) * edgeLength / 2f));
            point1.z = 0f;

            point2.x = (float)(hexToUse.transform.position.x + (0.5 * edgeLength));
            point2.y = (float)(hexToUse.transform.position.y - (Mathf.Sqrt(3f) * edgeLength / 2f));
            point2.z = 0f;

            // Update the ZOC's of the hexes; they do not cross rivers
            hexToUse.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.South] = false;
            hexNotUsing.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.North] = false;
            hexToUse.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.South] = true;
            hexNotUsing.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.North] = true;
        }
        else if (hexToUse.GetComponent<HexDatabaseFields>().Neighbors[(int)GlobalDefinitions.HexSides.SouthWest] == hexNotUsing)
        {
            // The river lies on the SW edge of hexToUse
            point1.x = (float)(hexToUse.transform.position.x + (0.5 * edgeLength));
            point1.y = (float)(hexToUse.transform.position.y - (Mathf.Sqrt(3f) * edgeLength / 2f));
            point1.z = 0f;

            point2.x = (float)(hexToUse.transform.position.x - edgeLength);
            point2.y = (float)(hexToUse.transform.position.y);
            point2.z = 0f;

            // Update the ZOC's of the hexes; they do not cross rivers
            hexToUse.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.SouthWest] = false;
            hexNotUsing.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.NorthEast] = false;
            hexToUse.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.SouthWest] = true;
            hexNotUsing.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.NorthEast] = true;
        }
        else if (hexToUse.GetComponent<HexDatabaseFields>().Neighbors[(int)GlobalDefinitions.HexSides.NorthWest] == hexNotUsing)
        {
            // The river lies on the Nw edge of hexToUse
            point1.x = (float)(hexToUse.transform.position.x - edgeLength);
            point1.y = (float)(hexToUse.transform.position.y);
            point1.z = 1f;

            point2.x = (float)(hexToUse.transform.position.x - (0.5 * edgeLength));
            point2.y = (float)(hexToUse.transform.position.y + (Mathf.Sqrt(3f) * edgeLength / 2f));
            point2.z = 1f;

            // Update the ZOC's of the hexes; they do not cross rivers
            hexToUse.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.NorthWest] = false;
            hexNotUsing.GetComponent<BoolArrayData>().exertsZOC[(int)GlobalDefinitions.HexSides.SouthEast] = false;
            hexToUse.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.NorthWest] = true;
            hexNotUsing.GetComponent<BoolArrayData>().riverSides[(int)GlobalDefinitions.HexSides.SouthEast] = true;
        }
        else
        {
            // This is a problem, the two hexes aren't neighbors
            Debug.Log("Two hexes provided for river that don't abutt.  Hex 1 (" +
                    hex1.GetComponent<HexDatabaseFields>().xMapCoor + "," + hex1.GetComponent<HexDatabaseFields>().yMapCoor + ")  + Hex 2(" +
                    hex2.GetComponent<HexDatabaseFields>().xMapCoor + ", " + hex2.GetComponent<HexDatabaseFields>().yMapCoor + ") ");
        }

        //GlobalDefinitions.DrawBlueLineBetweenTwoPoints(point1, point2);
    }

    /// <summary>
    /// This routine will setup the object references for adjacent hexes
    /// Note for the edge hexes, where there is no neighbor, the reference will not be set and it will stay null
    /// It also sets the current hex to not project ZOC if it is a fortress, sea, impassible, or neutral
    /// </summary>
    private void SetupHexNeighbors()
    {
        HexLocation currentHexCoodinates = new HexLocation();
        HexLocation neightborHexCoordinates = new HexLocation();
        GameObject neighborHex;

        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            currentHexCoodinates.x = hex.GetComponent<HexDatabaseFields>().xMapCoor;
            currentHexCoodinates.y = hex.GetComponent<HexDatabaseFields>().yMapCoor;
            foreach (GlobalDefinitions.HexSides hexSides in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
            {
                neightborHexCoordinates = calculateNeighborCoordinates(currentHexCoodinates, hexSides);
                neighborHex = GlobalDefinitions.getHexAtXY(neightborHexCoordinates.x, neightborHexCoordinates.y);
                if (neighborHex != null)
                {
                    hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] = neighborHex;
                    if (ZOCExtendToHex(hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides]))
                        hex.GetComponent<BoolArrayData>().exertsZOC[(int)hexSides] = true;
                    else
                        hex.GetComponent<BoolArrayData>().exertsZOC[(int)hexSides] = false;
                }
            }
        }
        // The following code sets the current hex to not project ZOC if it is a fortress, sea, impassible, or neutral
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            foreach (GlobalDefinitions.HexSides hexSides in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                if (hex.GetComponent<HexDatabaseFields>().fortress || hex.GetComponent<HexDatabaseFields>().sea ||
                        hex.GetComponent<HexDatabaseFields>().impassible || hex.GetComponent<HexDatabaseFields>().neutralCountry)
                    hex.GetComponent<BoolArrayData>().exertsZOC[(int)hexSides] = false;
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
            hex.GetComponent<HexDatabaseFields>().invasionTarget = GlobalDefinitions.getHexAtXY(hex.GetComponent<HexDatabaseFields>().invasionTargetX, hex.GetComponent<HexDatabaseFields>().invasionTargetY);
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            if (hex.GetComponent<HexDatabaseFields>().sea)
            {
                foreach (GlobalDefinitions.HexSides hexSides in Enum.GetValues(typeof(GlobalDefinitions.HexSides)))
                {
                    // Find the neighbor that is the invasion target.  Note, in the case of inland ports the ZOC will never get setup and that is correct
                    if (hex.GetComponent<HexDatabaseFields>().invasionTarget == hex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides])
                    {
                        // If the target is a fortress than the ZOC does not apply. If I set a ZOC for fortresses here (which I did at first) 
                        // it causes problems with clearing ZOC in the event of a battle
                        if (!hex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<HexDatabaseFields>().fortress)
                        {
                            hex.GetComponent<BoolArrayData>().exertsZOC[(int)hexSides] = true;
                            hex.GetComponent<HexDatabaseFields>().invasionTarget.GetComponent<BoolArrayData>().exertsZOC[GlobalDefinitions.returnHexSideOpposide((int)hexSides)] = true;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// This routine determines if ZOC extends into the hex passed.  Note this does not depend on the hex that is looking to exert ZOC to the passed hex
    /// River ZOC's are setup when the rivers are drawn
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    private bool ZOCExtendToHex(GameObject hex)
    {
        if (hex.GetComponent<HexDatabaseFields>().fortress)
            return (false);
        else if (hex.GetComponent<HexDatabaseFields>().sea)
            return (false);
        else if (hex.GetComponent<HexDatabaseFields>().impassible)
            return (false);
        else if (hex.GetComponent<HexDatabaseFields>().neutralCountry)
            return (false);
        return (true);
    }

    /// <summary>
    /// This routine returns the coordinates of the neighbor of the hex passed that is on the side passed
    /// </summary>
    /// <param name="hexCoordinates"></param>
    /// <param name="sideToCheck"></param>
    /// <returns></returns>
    private HexLocation calculateNeighborCoordinates(HexLocation hexCoordinates, GlobalDefinitions.HexSides sideToCheck)
    {
        HexLocation returnValue = new HexLocation();

        returnValue.x = hexCoordinates.x;
        returnValue.y = hexCoordinates.y;
        if (sideToCheck == GlobalDefinitions.HexSides.SouthWest)
        {
            returnValue.x = hexCoordinates.x - 1;
            if ((hexCoordinates.x % 2) == 0)
                returnValue.y = hexCoordinates.y - 1;
        }

        if (sideToCheck == GlobalDefinitions.HexSides.NorthWest)
        {
            returnValue.x = hexCoordinates.x - 1;
            if ((hexCoordinates.x % 2) != 0)
                returnValue.y = hexCoordinates.y + 1;
        }

        if (sideToCheck == GlobalDefinitions.HexSides.North)
        {
            returnValue.y = hexCoordinates.y + 1;
        }

        if (sideToCheck == GlobalDefinitions.HexSides.NorthEast)
        {
            returnValue.x = hexCoordinates.x + 1;
            if ((hexCoordinates.x % 2) != 0)
                returnValue.y = hexCoordinates.y + 1;
        }

        if (sideToCheck == GlobalDefinitions.HexSides.SouthEast)
        {
            returnValue.x = hexCoordinates.x + 1;
            if ((hexCoordinates.x % 2) == 0)
                returnValue.y = hexCoordinates.y - 1;
        }

        if (sideToCheck == GlobalDefinitions.HexSides.South)
        {
            returnValue.y = hexCoordinates.y - 1;
        }

        return (returnValue);
    }

    /// <summary>
    /// This is a utility funtion to be used when adding a lot of hex prefabs
    /// </summary>
    private void setupColliderOnHexes()
    {
        foreach (GameObject hex in GlobalDefinitions.allHexesOnBoard)
        {
            // Add a sphere collider
            hex.gameObject.AddComponent<SphereCollider>();
            // Set the radius of the collider
            hex.gameObject.GetComponent<SphereCollider>().radius = 3.3f;
            // While I'm here, set the sorting layer on the Sprite renderer
            hex.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Hex";
            // Now add the script with the Hex database fields on it
            hex.gameObject.AddComponent<HexDatabaseFields>();
        }
    }

    public void readBritainPlacement(string fileName)
    {
        char[] delimiterChars = { ' ', ',', ')', '(' };
        string line;
        GameObject placedUnit = new GameObject();

        int lineNumber = 0;

        // The following variables are used to determine what words are needed when reading the file
        int unitName = 0;
        int xPosition = 1;
        int yPosition = 2;
        int turnAvailable = 3;

        StreamReader theReader = new StreamReader(fileName);
        if (theReader == null)
        {
            GlobalDefinitions.guiUpdateStatusMessage("Unable to read Britain placement file " + fileName);
        }
        using (theReader)
        {
            do
            {
                line = theReader.ReadLine();
                lineNumber++;
                if (line != null)
                {
                    string[] lineEntries = line.Split(delimiterChars);

                    placedUnit = GameObject.Find(lineEntries[unitName]);
                    placedUnit.transform.position =
                            new Vector3((float)(Convert.ToDouble(lineEntries[xPosition]) + GlobalDefinitions.boardOffsetX),
                            (float)(Convert.ToDouble(lineEntries[yPosition]) + GlobalDefinitions.boardOffsetY), 0f);
                    placedUnit.transform.parent = GameObject.Find("Units In Britain").transform;
                    placedUnit.GetComponent<UnitDatabaseFields>().inBritain = true;
                    placedUnit.GetComponent<UnitDatabaseFields>().turnAvailable = Convert.ToInt32(lineEntries[turnAvailable]);
                    placedUnit.GetComponent<UnitDatabaseFields>().locationInBritain = new Vector2((float)(Convert.ToDouble(lineEntries[xPosition]) + GlobalDefinitions.boardOffsetX), (float)(Convert.ToDouble(lineEntries[yPosition]) + GlobalDefinitions.boardOffsetY));
                }
            }
            while (line != null);
            theReader.Close();
        }
    }

    /// <summary>
    /// Reads the German setup file
    /// </summary>
    /// <param name="fileName"></param>
    public void readGermanPlacement(string fileName)
    {
        char[] delimiterChars = { ' ', ',', ')', '(' };
        string line;
        GameObject placedUnit = new GameObject();

        int lineNumber = 0;

        // The following variables are used to determine what words are needed when reading the file
        int unitName = 0;
        int xHexCoor = 1;
        int yHexCoor = 2;

        StreamReader theReader = new StreamReader(fileName);

        if (theReader == null)
        {
            // Can't recover from this error but notify the user
            GlobalDefinitions.guiUpdateStatusMessage("Cannot access German setup file " + fileName);
        }
        else
        {
            using (theReader)
            {
                do
                {
                    line = theReader.ReadLine();
                    lineNumber++;
                    if (line != null)
                    {
                        GameObject hex;
                        string[] lineEntries = line.Split(delimiterChars);
                        placedUnit = GameObject.Find(lineEntries[unitName]);
                        hex = GlobalDefinitions.getHexAtXY(Convert.ToInt32(lineEntries[xHexCoor]), Convert.ToInt32(lineEntries[yHexCoor]));
                        GlobalDefinitions.putUnitOnHex(placedUnit, hex);
                        placedUnit.transform.parent = GlobalDefinitions.allUnitsOnBoard.transform;
                    }
                }
                while (line != null);
                theReader.Close();
            }
        }
    }

    /// <summary>
    /// This routine sets up all the parameters for the invasion areas
    /// </summary>
    public void setupInvasionAreas()
    {
        GlobalDefinitions.invasionAreas[0] = new InvasionArea();
        GlobalDefinitions.invasionAreas[1] = new InvasionArea();
        GlobalDefinitions.invasionAreas[2] = new InvasionArea();
        GlobalDefinitions.invasionAreas[3] = new InvasionArea();
        GlobalDefinitions.invasionAreas[4] = new InvasionArea();
        GlobalDefinitions.invasionAreas[5] = new InvasionArea();
        GlobalDefinitions.invasionAreas[6] = new InvasionArea();


        // The rest of the start routine is setting up the invasion areas
        GlobalDefinitions.invasionAreas[0].name = "South France";
        GlobalDefinitions.invasionAreas[0].firstTurnArmor = 1;
        GlobalDefinitions.invasionAreas[0].firstTurnInfantry = 6;
        GlobalDefinitions.invasionAreas[0].firstTurnAirborne = 1;
        GlobalDefinitions.invasionAreas[0].secondTurnArmor = 2;
        GlobalDefinitions.invasionAreas[0].secondTurnInfantry = 5;
        GlobalDefinitions.invasionAreas[0].secondTurnAirborne = 2;
        GlobalDefinitions.invasionAreas[0].divisionsPerTurn = 8;
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(44, 31));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(45, 30));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(45, 29));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(46, 29));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(47, 28));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(47, 27));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(47, 25));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(47, 24));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(46, 24));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(46, 23));
        GlobalDefinitions.invasionAreas[0].invasionHexes.Add(GlobalDefinitions.getHexAtXY(46, 20));

        GlobalDefinitions.invasionAreas[1].name = "Bay of Biscay";
        GlobalDefinitions.invasionAreas[1].firstTurnArmor = 0;
        GlobalDefinitions.invasionAreas[1].firstTurnInfantry = 3;
        GlobalDefinitions.invasionAreas[1].firstTurnAirborne = 1;
        GlobalDefinitions.invasionAreas[1].secondTurnArmor = 1;
        GlobalDefinitions.invasionAreas[1].secondTurnInfantry = 2;
        GlobalDefinitions.invasionAreas[1].secondTurnAirborne = 1;
        GlobalDefinitions.invasionAreas[1].divisionsPerTurn = 4;
        GlobalDefinitions.invasionAreas[1].invasionHexes.Add(GlobalDefinitions.getHexAtXY(37, 8));
        GlobalDefinitions.invasionAreas[1].invasionHexes.Add(GlobalDefinitions.getHexAtXY(35, 7));
        GlobalDefinitions.invasionAreas[1].invasionHexes.Add(GlobalDefinitions.getHexAtXY(34, 7));
        GlobalDefinitions.invasionAreas[1].invasionHexes.Add(GlobalDefinitions.getHexAtXY(32, 7));
        GlobalDefinitions.invasionAreas[1].invasionHexes.Add(GlobalDefinitions.getHexAtXY(32, 6));
        GlobalDefinitions.invasionAreas[1].invasionHexes.Add(GlobalDefinitions.getHexAtXY(31, 5));
        GlobalDefinitions.invasionAreas[1].invasionHexes.Add(GlobalDefinitions.getHexAtXY(29, 5));
        GlobalDefinitions.invasionAreas[1].invasionHexes.Add(GlobalDefinitions.getHexAtXY(27, 4));

        GlobalDefinitions.invasionAreas[2].name = "Brittany";
        GlobalDefinitions.invasionAreas[2].firstTurnArmor = 0;
        GlobalDefinitions.invasionAreas[2].firstTurnInfantry = 4;
        GlobalDefinitions.invasionAreas[2].firstTurnAirborne = 2;
        GlobalDefinitions.invasionAreas[2].secondTurnArmor = 2;
        GlobalDefinitions.invasionAreas[2].secondTurnInfantry = 2;
        GlobalDefinitions.invasionAreas[2].secondTurnAirborne = 1;
        GlobalDefinitions.invasionAreas[2].divisionsPerTurn = 6;
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(25, 2));
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(22, 0));
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(20, 2));
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(20, 3));
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(20, 4));
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(21, 4));
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(21, 5));
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(21, 6));
        GlobalDefinitions.invasionAreas[2].invasionHexes.Add(GlobalDefinitions.getHexAtXY(21, 7));

        GlobalDefinitions.invasionAreas[3].name = "Normandy";
        GlobalDefinitions.invasionAreas[3].firstTurnArmor = 0;
        GlobalDefinitions.invasionAreas[3].firstTurnInfantry = 6;
        GlobalDefinitions.invasionAreas[3].firstTurnAirborne = 3;
        GlobalDefinitions.invasionAreas[3].secondTurnArmor = 2;
        GlobalDefinitions.invasionAreas[3].secondTurnInfantry = 4;
        GlobalDefinitions.invasionAreas[3].secondTurnAirborne = 0;
        GlobalDefinitions.invasionAreas[3].divisionsPerTurn = 9;
        GlobalDefinitions.invasionAreas[3].invasionHexes.Add(GlobalDefinitions.getHexAtXY(19, 6));
        GlobalDefinitions.invasionAreas[3].invasionHexes.Add(GlobalDefinitions.getHexAtXY(16, 8));
        GlobalDefinitions.invasionAreas[3].invasionHexes.Add(GlobalDefinitions.getHexAtXY(16, 9));
        GlobalDefinitions.invasionAreas[3].invasionHexes.Add(GlobalDefinitions.getHexAtXY(18, 9));
        GlobalDefinitions.invasionAreas[3].invasionHexes.Add(GlobalDefinitions.getHexAtXY(18, 10));
        GlobalDefinitions.invasionAreas[3].invasionHexes.Add(GlobalDefinitions.getHexAtXY(18, 11));

        GlobalDefinitions.invasionAreas[4].name = "Le Havre";
        GlobalDefinitions.invasionAreas[4].firstTurnArmor = 0;
        GlobalDefinitions.invasionAreas[4].firstTurnInfantry = 6;
        GlobalDefinitions.invasionAreas[4].firstTurnAirborne = 3;
        GlobalDefinitions.invasionAreas[4].secondTurnArmor = 2;
        GlobalDefinitions.invasionAreas[4].secondTurnInfantry = 5;
        GlobalDefinitions.invasionAreas[4].secondTurnAirborne = 0;
        GlobalDefinitions.invasionAreas[4].divisionsPerTurn = 10;
        GlobalDefinitions.invasionAreas[4].invasionHexes.Add(GlobalDefinitions.getHexAtXY(17, 11));
        GlobalDefinitions.invasionAreas[4].invasionHexes.Add(GlobalDefinitions.getHexAtXY(17, 12));
        GlobalDefinitions.invasionAreas[4].invasionHexes.Add(GlobalDefinitions.getHexAtXY(16, 13));

        GlobalDefinitions.invasionAreas[5].name = "Pas De Calais";
        GlobalDefinitions.invasionAreas[5].firstTurnArmor = 2;
        GlobalDefinitions.invasionAreas[5].firstTurnInfantry = 7;
        GlobalDefinitions.invasionAreas[5].firstTurnAirborne = 3;
        GlobalDefinitions.invasionAreas[5].secondTurnArmor = 4;
        GlobalDefinitions.invasionAreas[5].secondTurnInfantry = 5;
        GlobalDefinitions.invasionAreas[5].secondTurnAirborne = 0;
        GlobalDefinitions.invasionAreas[5].divisionsPerTurn = 12;
        GlobalDefinitions.invasionAreas[5].invasionHexes.Add(GlobalDefinitions.getHexAtXY(16, 14));
        GlobalDefinitions.invasionAreas[5].invasionHexes.Add(GlobalDefinitions.getHexAtXY(15, 14));
        GlobalDefinitions.invasionAreas[5].invasionHexes.Add(GlobalDefinitions.getHexAtXY(14, 15));
        GlobalDefinitions.invasionAreas[5].invasionHexes.Add(GlobalDefinitions.getHexAtXY(13, 15));
        GlobalDefinitions.invasionAreas[5].invasionHexes.Add(GlobalDefinitions.getHexAtXY(12, 16));
        GlobalDefinitions.invasionAreas[5].invasionHexes.Add(GlobalDefinitions.getHexAtXY(12, 17));
        GlobalDefinitions.invasionAreas[5].invasionHexes.Add(GlobalDefinitions.getHexAtXY(11, 17));
        GlobalDefinitions.invasionAreas[5].invasionHexes.Add(GlobalDefinitions.getHexAtXY(11, 18));

        GlobalDefinitions.invasionAreas[6].name = "North Sea";
        GlobalDefinitions.invasionAreas[6].firstTurnArmor = 0;
        GlobalDefinitions.invasionAreas[6].firstTurnInfantry = 6;
        GlobalDefinitions.invasionAreas[6].firstTurnAirborne = 3;
        GlobalDefinitions.invasionAreas[6].secondTurnArmor = 2;
        GlobalDefinitions.invasionAreas[6].secondTurnInfantry = 4;
        GlobalDefinitions.invasionAreas[6].secondTurnAirborne = 1;
        GlobalDefinitions.invasionAreas[6].divisionsPerTurn = 9;
        GlobalDefinitions.invasionAreas[6].invasionHexes.Add(GlobalDefinitions.getHexAtXY(10, 19));
        GlobalDefinitions.invasionAreas[6].invasionHexes.Add(GlobalDefinitions.getHexAtXY(9, 20));
        GlobalDefinitions.invasionAreas[6].invasionHexes.Add(GlobalDefinitions.getHexAtXY(8, 21));
        GlobalDefinitions.invasionAreas[6].invasionHexes.Add(GlobalDefinitions.getHexAtXY(7, 21));
        GlobalDefinitions.invasionAreas[6].invasionHexes.Add(GlobalDefinitions.getHexAtXY(6, 22));
        GlobalDefinitions.invasionAreas[6].invasionHexes.Add(GlobalDefinitions.getHexAtXY(5, 22));
    }
}
