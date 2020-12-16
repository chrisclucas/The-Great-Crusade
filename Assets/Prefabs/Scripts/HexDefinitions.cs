using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonRoutines
{
    public class HexDefinitions : MonoBehaviour
    {
        public enum HexSides { North, NorthEast, SouthEast, South, SouthWest, NorthWest };

        // The following is a list of all hexes on the board.  It is loaded once after the map is read in.  Keeps from having to do GameObject.Find all the time to get the hexes
        public static List<GameObject> allHexesOnBoard = new List<GameObject>();
    }    
}