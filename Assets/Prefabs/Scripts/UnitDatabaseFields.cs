using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//[ExecuteInEditMode]

public class UnitDatabaseFields : MonoBehaviour
{
    //=====================================================================================
    // These fields are general attributes of the unit
    //=====================================================================================
    /// <summary>
    /// base attack factor of the unit
    /// </summary>
    public int attackFactor = 4;
    /// <summary>
    /// base defense factor of the unit
    /// </summary>
    public int defenseFactor = 4;
    /// <summary>
    /// base movement factor of the unit
    /// </summary>
    public int movementFactor = 4;
    /// <summary>
    /// indicates whether the unit is ariborne
    /// </summary>
    public bool airborne = false;
    /// <summary>
    /// indicates whether the unit is an armor unit
    /// </summary>
    public bool armor = false;
    /// <summary>
    /// indicates whether the unit is an infantry unit
    /// </summary>
    public bool infantry = false;
    /// <summary>
    /// inidcates whether the unit is German static unit
    /// </summary>
    public bool germanStatic = false;
    /// <summary>
    /// Indicates if the unit is a headquarters unit
    /// </summary>
    public bool HQ = false;
    /// <summary>
    /// enumerated type that indicates the nationality of the unit
    /// </summary>
    public GlobalDefinitions.Nationality nationality;
    /// <summary>
    /// string containing the unit description
    /// </summary>
    public string unitDesignation = "";
    /// <summary>
    /// Variable indicating which week the unit is available
    /// </summary>
    public int turnAvailable;
    /// <summary>
    /// This is the location that the unit has on the Order of Battle Sheet (i.e. the dead pool)
    /// </summary>
    public Vector2 OOBLocation;
    /// <summary>
    /// Location in Britain that an Allied unit occupies
    /// </summary>
    public Vector2 locationInBritain;

    //=====================================================================================
    // These fields are general dynamic attributes of the unit that are set during game play
    //=====================================================================================
    /// <summary>
    /// a link to the hex that the unit is occupying
    /// </summary>
    public GameObject occupiedHex = null;
    /// <summary>
    /// This is a pointer to the hex that the unit started the turn on
    /// </summary>
    public GameObject beginningTurnHex = null;
    /// <summary>
    /// Flag used to determine if an Allied unit is in Britain
    /// </summary>
    public bool inBritain = true;
    /// <summary>
    /// Used to determine when a unit is attacking
    /// </summary>
    public bool isCommittedToAnAttack = false;
    /// <summary>
    /// Indicates that the German unit is interdicted
    /// </summary>
    public bool unitInterdiction = false;
    /// <summary>
    /// Used to determine what area's limits to decrease when a unit move from Britain is undone by the user
    /// </summary>
    public int invasionAreaIndex = -1;
    /// <summary>
    /// Used to determine if the unit is eliminated.  Need this for when restarting a game.
    /// </summary>
    public bool unitEliminated = false;
    /// <summary>
    /// Indicates the group that the unit is grouped into for AI games
    /// </summary>
    public int groupNumber = 0;

    //=====================================================================================
    // These fields are movement attributes of the unit
    //=====================================================================================
    /// <summary>
    /// during movement used to determine how much movement a unit has left
    /// </summary>
    public int remainingMovement = 4;
    /// <summary>
    /// Only allied and German armor and airborn can use strategic movement
    /// </summary>
    public bool availableForStrategicMovement = true;
    /// <summary>
    /// indicates whether a unit has already moved in the movement phase
    /// </summary>
    public bool hasMoved = false;

    //=====================================================================================
    // These fields are supply attributes of the unit
    //=====================================================================================
    /// <summary>
    /// Indicates whether a unit is in supply or not
    /// </summary>
    public bool inSupply = true;
    /// <summary>
    /// Contains the index to the occupied hex's supplySources list of the source of supply for the unit
    /// </summary>
    public GameObject supplySource;
    /// <summary>
    /// Tracks the number of increments that the unit has been out of supply
    /// </summary>
    public int supplyIncrementsOutOfSupply = 0;

    //=====================================================================================
    // These fields are used by the AI
    //=====================================================================================
    public List<GameObject> unitsThatCanBeAttackedThisTurn = new List<GameObject>();
    public List<GameObject> availableMovementHexes = new List<GameObject>();

    /// <summary>
    /// This routine will write out the fields of the unit
    /// </summary>
    public void writeUnitFields(StreamWriter theWriter)
    {
        theWriter.Write(name + " ");
        if (occupiedHex != null)
            theWriter.Write(occupiedHex.name + " ");
        else
            theWriter.Write("null ");
        if (beginningTurnHex != null)
            theWriter.Write(beginningTurnHex.name + " ");
        else
            theWriter.Write("null ");
        theWriter.Write(GlobalDefinitions.writeBooleanToSaveFormat(inBritain) + " ");
        theWriter.Write(GlobalDefinitions.writeBooleanToSaveFormat(unitInterdiction) + " ");
        theWriter.Write(invasionAreaIndex + " ");
        theWriter.Write(GlobalDefinitions.writeBooleanToSaveFormat(availableForStrategicMovement) + " ");
        theWriter.Write(GlobalDefinitions.writeBooleanToSaveFormat(inSupply) + " ");
        if (supplySource != null)
            theWriter.Write(supplySource.name + " ");
        else
            theWriter.Write("null ");
        theWriter.Write(supplyIncrementsOutOfSupply + " ");
        theWriter.Write(GlobalDefinitions.writeBooleanToSaveFormat(unitEliminated) + " ");

        theWriter.WriteLine();
    }
}
