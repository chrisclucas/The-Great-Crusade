using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheGreatCrusade
{
    public class CarpetBombingSelectionToggleRoutines : MonoBehaviour
    {
        public List<GameObject> defendingUnits;
        public List<GameObject> attackingUnits;
        public string combatOdds;
        public int dieRollResult;
        public GlobalDefinitions.CombatResults combatResults;
        public Vector2 buttonLocation;

        public void CarpetBombingResultsSelected()
        {
            if (gameObject.GetComponent<Toggle>().isOn)
            {
                if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Peer2PeerNetwork) && (!GlobalDefinitions.localControl))
                {
                    GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
                    CombatResolutionRoutines.ExecuteCombatResults(defendingUnits, attackingUnits, combatOdds, dieRollResult, combatResults, buttonLocation);
                }
                else
                {
                    GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.COMBATRESOLUTIONSELECTEDKEYWORD + " " + GlobalDefinitions.CombatResultToggleName);
                    GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.CARPETBOMBINGRESULTSSELECTEDKEYWORD + " " + name + " " + dieRollResult);
                    GlobalDefinitions.RemoveGUI(transform.parent.gameObject);
                    CombatResolutionRoutines.ExecuteCombatResults(defendingUnits, attackingUnits, combatOdds, dieRollResult, combatResults, buttonLocation);

                }
            }
        }
    }
}
