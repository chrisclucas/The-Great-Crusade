using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CommonRoutines;

namespace TheGreatCrusade
{
    public class ExchangeToggleRoutines : MonoBehaviour
    {
        public GameObject unit;
        public bool attacker = true;
        public List<GameObject> attackingUnits;

        public void AddOrSubtractExchangeFactors()
        {
            if (GetComponent<ExchangeToggleRoutines>().attacker)
            {
                if (GetComponent<Toggle>().isOn)
                {
                    GlobalDefinitions.exchangeFactorsSelected += CalculateBattleOddsRoutines.ReturnAttackFactor(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.unitsToExchange.Add(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.HighlightUnit(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.ADDEXCHANGEKEYWORD + " " + name);
                }
                else
                {
                    GlobalDefinitions.exchangeFactorsSelected -= CalculateBattleOddsRoutines.ReturnAttackFactor(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.unitsToExchange.Remove(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.UnhighlightUnit(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.REMOVEEXCHANGEKEYWORD + " " + name);
                }
            }
            else
            {
                if (GetComponent<Toggle>().isOn)
                {
                    GlobalDefinitions.exchangeFactorsSelected += CalculateBattleOddsRoutines.CalculateUnitDefendingFactor(GetComponent<ExchangeToggleRoutines>().unit, attackingUnits);
                    GlobalDefinitions.unitsToExchange.Add(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.HighlightUnit(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.ADDEXCHANGEKEYWORD + " " + name);
                }
                else
                {
                    GlobalDefinitions.exchangeFactorsSelected -= CalculateBattleOddsRoutines.CalculateUnitDefendingFactor(GetComponent<ExchangeToggleRoutines>().unit, attackingUnits);
                    GlobalDefinitions.unitsToExchange.Remove(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.UnhighlightUnit(GetComponent<ExchangeToggleRoutines>().unit);
                    GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.REMOVEEXCHANGEKEYWORD + " " + name);
                }
            }
            GameObject.Find("ExchangeText").GetComponent<TextMeshProUGUI>().text = "Select " + GlobalDefinitions.exchangeFactorsToLose + " factors\nFactors selected so far: " + GlobalDefinitions.exchangeFactorsSelected;
        }
    }
}