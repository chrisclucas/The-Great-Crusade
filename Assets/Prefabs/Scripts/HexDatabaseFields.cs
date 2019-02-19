using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HexDatabaseFields : MonoBehaviour
{
    //=====================================================================================
    // These fields are general attributes of the hex
    //=====================================================================================
    /// <summary>
    /// A string that can be used when referring to the hex to the user
    /// </summary>
    public string hexName;
    /// <summary>
    /// indicates whether a hex is a city
    /// </summary>
    public bool city = false;
    /// <summary>
    /// indicates whether a hex is a fortress
    /// </summary>
    public bool fortress = false;
    /// <summary>
    /// indicates whether a hex is a mountain area
    /// </summary>
    public bool mountain = false;
    /// <summary>
    /// indicates whether the hex is an impassible mountain top
    /// </summary>
    public bool impassible = false;
    /// <summary>
    /// indicates whether the hex is a coastal square that can be invaded
    /// </summary>
    public bool coast = false;
    /// <summary>
    /// indicates whether a hex is where German replacements are placed
    /// </summary>
    public bool germanRepalcement = false;
    /// <summary>
    /// indicates whether a hex is a fortified zone
    /// </summary>
    public bool fortifiedZone = false;
    /// <summary>
    /// indicates whether a hex is part of a neutral country
    /// </summary>
    public bool neutralCountry = false;
    /// <summary>
    /// indicates that the hex is a port on a coastal hex
    /// </summary>
    public bool coastalPort = false;
    /// <summary>
    /// indicates that the hex is an inland port - able to supply but not invade
    /// </summary>
    public bool inlandPort = false;
    /// <summary>
    /// indcates whether a hex is a bridge over a water hex. In D-Day this is a dike
    /// </summary>
    public bool bridge = false;
    /// <summary>
    /// Indicates that the hex is a normal sea hex
    /// </summary>
    public bool sea = false;
    /// <summary>
    /// Indicates whether the occupation of the hex counts to Allied victory
    /// </summary>
    public bool AlliedVictoryHex = false;
    /// <summary>
    /// Used to show the actual historical progress made by the Allies
    /// </summary>
    public int historyWeekCaptured = 0;
    /// <summary>
    /// Inidcates if the hex is used to determine the availability of Free French units after the 28th week
    /// </summary>
    public bool FreeFrenchAvailableHex = false;
    /// <summary>
    /// Array that contains references to neighboring hexes - order is N NW SW S SE NE
    /// </summary>
    public GameObject[] Neighbors = new GameObject[6];

    //=====================================================================================
    // These fields are general dynamic attributes of the hex - set during game-play
    //=====================================================================================
    /// <summary>
    /// inidcates that the hex lies within a German unit's ZOC
    /// </summary>
    public bool inGermanZOC = false;
    /// <summary>
    /// indicates that the hex lies within an Allied unit's ZOC 
    /// </summary>
    public bool inAlliedZOC = false;
    /// <summary>
    /// The AI needs to know who is exerting ZOC
    /// </summary>
    public List<GameObject> unitsExertingZOC = new List<GameObject>();
    /// <summary>
    /// indicates that the hex is in allied control for supply purposes
    /// </summary>
    public bool alliedControl = false;
    /// <summary>
    /// object list to the units occupying the hex
    /// </summary>
    public List<GameObject> occupyingUnit = new List<GameObject>();

    //=====================================================================================
    // These fields are invasion attributes of the hex
    //=====================================================================================
    /// <summary>
    /// for invasion hexes this has a link to the hex that can be invaded
    /// </summary>
    public GameObject invasionTarget = null;
    public int invasionTargetX;
    public int invasionTargetY;
    /// <summary>
    /// This is the index for the invasion area the hex is a part of.  
    /// It only applies to hexes that can land reinforcements.
    /// </summary>
    public int invasionAreaIndex = -1;

    //=====================================================================================
    // These fields are supply attributes of the hex
    //=====================================================================================
    /// <summary>
    /// the amount of supply that the allies can bring in through the hex
    /// </summary>
    public int supplyCapacity = 0;
    /// <summary>
    /// Amount of supply capacity that is available
    /// </summary>
    public int unassignedSupply = 0;
    /// <summary>
    /// Indicates the supply range of a supply source
    /// </summary>
    public int supplyRange = 0;
    /// <summary>
    /// Successfully invaded hexes have special capabilities
    /// </summary>
    public bool successfullyInvaded = false;
    /// <summary>
    /// Lists the supply sources available for the hex
    /// </summary>
    public List<GameObject> supplySources = new List<GameObject>();
    /// <summary>
    /// This list are the hexes that must be free from Germans to use the hex for supply or landing
    /// </summary>
    public List<GameObject> controlHexes = new List<GameObject>();
    /// <summary>
    /// List of units that could supplied by this hex (of course if it is a supply source)
    /// </summary>
    public List<GameObject> unitsThatCanBeSupplied = new List<GameObject>();
    /// <summary>
    /// The following booleans are needed for the display of supply range.  I can't rerun the
    /// supply assignment routines without changing the supply situation (which is only 
    /// supposed to be run at the beginning and end of a turn) so I will use these booleans to
    /// store the supply range.
    /// </summary>
    public bool alliedInSupply = false;
    public bool germanInSupply = false;

    //=====================================================================================
    // These fields are the hex x and y coordinates
    // Probably should make these a single type since I don't use them separately
    //=====================================================================================
    /// <summary>
    /// the x map coordinate of the hex in the map 
    /// </summary>
    public int xMapCoor;
    /// <summary>
    /// the x map coordinate of the hex in the map 
    /// </summary>
    public int yMapCoor;

    //=====================================================================================
    // These fields are movement attributes of the hex
    //=====================================================================================
    /// <summary>
    /// during the movement phase this indicates how much movement is left of the unit being moved
    /// </summary>
    public int remainingMovement = 0;
    /// <summary>
    /// this field takes care of strategic movement
    /// </summary>
    public int strategicRemainingMovement = 0;
    /// <summary>
    /// during movement phase this indicates whether the current unit can move to the hex
    /// </summary>
    public bool availableForMovement = false;

    //=====================================================================================
    // These fields are allied air attributes of the hex
    //=====================================================================================
    /// <summary>
    /// Determines if carpet bombing is in effect for the hex
    /// </summary>
    public bool carpetBombingActive = false;
    /// <summary>
    /// Determines if defensive support is assigned to the hex
    /// </summary>
    public bool closeDefenseSupport = false;
    /// <summary>
    /// Determines if the hex is being river interdicted
    /// </summary>
    public bool riverInterdiction = false;

    /// <summary>
    /// This routine will write out the fields of the hex
    /// </summary>
    public void WriteHexFields(StreamWriter fileWriter)
    {
        fileWriter.Write(name + " ");
        fileWriter.Write(GlobalDefinitions.WriteBooleanToSaveFormat(inGermanZOC) + " ");
        fileWriter.Write(GlobalDefinitions.WriteBooleanToSaveFormat(inAlliedZOC) + " ");
        fileWriter.Write(GlobalDefinitions.WriteBooleanToSaveFormat(alliedControl) + " ");
        fileWriter.Write(GlobalDefinitions.WriteBooleanToSaveFormat(successfullyInvaded) + " ");
        fileWriter.Write(GlobalDefinitions.WriteBooleanToSaveFormat(closeDefenseSupport) + " ");
        fileWriter.Write(GlobalDefinitions.WriteBooleanToSaveFormat(riverInterdiction) + " ");
        fileWriter.WriteLine();
    }

    //=====================================================================================
    // These fields are used by the AI
    //=====================================================================================
    /// <summary>
    /// This field determines what the intrisic value of the hex is; based on the hex type
    /// </summary>
    public int intrinsicHexValue;
    /// <summary>
    /// This is the value of the hex; both intrinsic and contextual
    /// </summary>
    public int hexValue;

    // Debugging fields
    public int supplyModifier = 0;
    public int adjacentUnitModifier = 0;
    public int sharedZOCModifier = 0;
    public int abuttingZOCModifier = 0;
    public int stackedUnitModfier = 0;
    public int riverModifier = 0;
    public int enemyDistanceModifier = 0;
}
