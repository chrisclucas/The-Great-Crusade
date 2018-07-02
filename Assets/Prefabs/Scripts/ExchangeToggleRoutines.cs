using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ExchangeToggleRoutines : MonoBehaviour
{
    public GameObject unit;
    public bool attacker = true;
    public List<GameObject> attackingUnits;

    public void addOrSubtractExchangeFactors()
    {
        if (GetComponent<ExchangeToggleRoutines>().attacker)
        {
            if (GetComponent<Toggle>().isOn)
            {
                GlobalDefinitions.exchangeFactorsSelected += GlobalDefinitions.returnAttackFactor(GetComponent<ExchangeToggleRoutines>().unit);
                GlobalDefinitions.unitsToExchange.Add(GetComponent<ExchangeToggleRoutines>().unit);
                GlobalDefinitions.highlightUnit(GetComponent<ExchangeToggleRoutines>().unit);
                if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
                    TransportScript.SendSocketMessage(GlobalDefinitions.ADDEXCHANGEKEYWORD + " " + name);
            }
            else
            {
                GlobalDefinitions.exchangeFactorsSelected -= GlobalDefinitions.returnAttackFactor(GetComponent<ExchangeToggleRoutines>().unit);
                GlobalDefinitions.unitsToExchange.Remove(GetComponent<ExchangeToggleRoutines>().unit);
                GlobalDefinitions.unhighlightUnit(GetComponent<ExchangeToggleRoutines>().unit);
                if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
                    TransportScript.SendSocketMessage(GlobalDefinitions.REMOVEEXCHANGEKEYWORD + " " + name);
            }
        }
        else
        {
            if (GetComponent<Toggle>().isOn)
            {
                GlobalDefinitions.exchangeFactorsSelected += GlobalDefinitions.calculateUnitDefendingFactor(GetComponent<ExchangeToggleRoutines>().unit, attackingUnits);
                GlobalDefinitions.unitsToExchange.Add(GetComponent<ExchangeToggleRoutines>().unit);
                GlobalDefinitions.highlightUnit(GetComponent<ExchangeToggleRoutines>().unit);
                if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
                    TransportScript.SendSocketMessage(GlobalDefinitions.ADDEXCHANGEKEYWORD + " " + name);
            }
            else
            {
                GlobalDefinitions.exchangeFactorsSelected -= GlobalDefinitions.calculateUnitDefendingFactor(GetComponent<ExchangeToggleRoutines>().unit, attackingUnits);
                GlobalDefinitions.unitsToExchange.Remove(GetComponent<ExchangeToggleRoutines>().unit);
                GlobalDefinitions.unhighlightUnit(GetComponent<ExchangeToggleRoutines>().unit);
                if ((GlobalDefinitions.gameMode == GlobalDefinitions.GameModeValues.Network) && (GlobalDefinitions.localControl))
                    TransportScript.SendSocketMessage(GlobalDefinitions.REMOVEEXCHANGEKEYWORD + " " + name);
            }
        }
        GameObject.Find("ExchangeText").GetComponent<Text>().text = "Select " + GlobalDefinitions.exchangeFactorsToLose + " factors\nFactors selected so far: " + GlobalDefinitions.exchangeFactorsSelected;
    }
}
