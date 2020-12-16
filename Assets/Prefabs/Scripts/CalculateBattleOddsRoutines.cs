using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonRoutines
{

    public class CalculateBattleOddsRoutines
    {

        // Use these constants to determine what the scaling of defense factors is for different attacks
        public static int defenseFactorScalingForFortress = 3;
        public static int defenseFactorScalingForCity = 2;
        public static int defenseFactorScalingForFortifiedZone = 2;
        public static int defenseFactorScalingForRiver = 2;
        public static int defenseFactorScalingForMountain = 2;

        /// <summary>
        /// This routine is used to return the attacker factor of the unit passed
        /// It checks if the unit is out of supply
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float ReturnAttackFactor(GameObject unit)
        {
            if (unit.GetComponent<UnitDatabaseFields>().inSupply)
            {
                //writeToLogFile("returnAttackFactor: unit " + unit.name + " returning attack factor = " + unit.GetComponent<UnitDatabaseFields>().attackFactor);
                return (unit.GetComponent<UnitDatabaseFields>().attackFactor);
            }
            else
            {
                //writeToLogFile("returnAttackFactor: unit " + unit.name + " returning attack factor = " + (unit.GetComponent<UnitDatabaseFields>().attackFactor/2));
                return (unit.GetComponent<UnitDatabaseFields>().attackFactor / 2);  // Need to check on this, I think the attack factor is one if out of supply
            }
        }

        /// <summary>
        /// This routine returns the number of attack factors for the passed list of attackers.
        /// The arrayIndex passed is used to determine if there is attack air support
        /// </summary>
        /// <param name="attackingUnits"></param>
        /// <param name="arrayIndex"></param>
        /// <returns></returns>
        public static float CalculateAttackFactor(List<GameObject> attackingUnits, bool addCombatAirSupport)
        {
            float totalAttackFactors = 0f;

            foreach (GameObject unit in attackingUnits)
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    totalAttackFactors += ReturnAttackFactor(unit);

            // Check if this attack has air support for the attacker
            if (addCombatAirSupport)
                totalAttackFactors++;

            // Check if the total factors is 0 (this is due to a static unit out of supply that must attack).  If it is return 1 since 
            // accomodating a 0 attack factor isn't worth it
            if (totalAttackFactors == 0)
                totalAttackFactors = 1f;

            return (totalAttackFactors);
        }

        /// <summary>
        /// Returns the number of attacking factors from the list passed
        /// </summary>
        /// <param name="attackingUnits"></param>
        /// <returns></returns>
        public static float CalculateAttackFactorWithoutAirSupport(List<GameObject> attackingUnits)
        {
            float totalAttackFactors = 0f;

            foreach (GameObject unit in attackingUnits)
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    totalAttackFactors += ReturnAttackFactor(unit);

            // Check if the total factors is 0 (this is due to a static unit out of supply that must attack).  If it is return 1 since 
            // accomodating a 0 attack factor isn't worth it
            if (totalAttackFactors == 0)
                totalAttackFactors = 1f;

            return (totalAttackFactors);
        }


        /// <summary>
        /// Returns the defense factor of the unit passed based on the list of attackers that are passed
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="attackingUnits"></param>
        /// <returns></returns>
        public static int CalculateUnitDefendingFactor(GameObject unit, List<GameObject> attackingUnits)
        {
            bool onlyCrossRiverAttack = true;
            int commttedAttackerNumber = 0;

            // The following is needed to resolve an issue where the defenders are selected first.  Without setting committedAttackerNumber
            // it will check for attacks across river and since there are no attacker committed yet and the default is to assume a cross
            // river attack it will end up doubling the defender factors until attackers are chosen
            foreach (GameObject attackingUnit in attackingUnits)
                if (attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    commttedAttackerNumber++;

            // First determine if the unit's odds are doubled because it is on a city, mountain, or fortified zone or tripled if fortress
            // If it is we don't need to check for rivers because it won't make a difference
            if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().city)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * defenseFactorScalingForCity);
            else if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().mountain)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * defenseFactorScalingForMountain);
            else if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortifiedZone)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * defenseFactorScalingForFortifiedZone);
            else if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().fortress)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * defenseFactorScalingForFortress);

            if (attackingUnits.Count == 0)
                return (unit.GetComponent<UnitDatabaseFields>().defenseFactor);

            else if (commttedAttackerNumber > 0)
            {
                // Need to check if the attack is taking place only across a river
                foreach (GameObject attackingUnit in attackingUnits)
                    foreach (HexDefinitions.HexSides hexSides in Enum.GetValues(typeof(HexDefinitions.HexSides)))
                        if (attackingUnit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack
                                    && (attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().Neighbors[(int)hexSides] == unit.GetComponent<UnitDatabaseFields>().occupiedHex)
                                    && (!attackingUnit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<BooleanArrayData>().riverSides[(int)hexSides]))
                        {
                            // All we need is one unit to not be attacking across a river to negate the doubled defense
                            onlyCrossRiverAttack = false;
                            return (unit.GetComponent<UnitDatabaseFields>().defenseFactor);
                        }

                if (onlyCrossRiverAttack)
                    return (unit.GetComponent<UnitDatabaseFields>().defenseFactor * defenseFactorScalingForRiver);
            }
            return (unit.GetComponent<UnitDatabaseFields>().defenseFactor);
        }


        /// <summary>
        /// Returns the defense factors of the defending units passed when being attacked by the attacking units passed.
        /// Note that I need to know who is attacking since attacking across a river increases defense
        /// </summary>
        /// <param name="defendingUnits"></param>
        /// <param name="attackingUnits"></param>
        /// <returns></returns>
        public static int CalculateDefenseFactorWithoutAirSupport(List<GameObject> defendingUnits, List<GameObject> attackingUnits)
        {
            int totalDefendFactors = 0;

            foreach (GameObject unit in defendingUnits)
            {
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    totalDefendFactors += CalculateUnitDefendingFactor(unit, attackingUnits);
            }
            return (totalDefendFactors);
        }

        /// <summary>
        /// Returns the defense factors of the defending units passed.  It scales the defense factors based on the attackers and also adds air defense if present
        /// </summary>
        /// <param name="defendingUnits"></param>
        /// <param name="attackingUnits"></param>
        /// <returns></returns>
        public static int CalculateDefenseFactor(List<GameObject> defendingUnits, List<GameObject> attackingUnits)
        {
            int totalDefendFactors = 0;

            foreach (GameObject unit in defendingUnits)
            {
                //GlobalDefinitions.writeToLogFile("calculateDefenseFactor: defending unit " + unit.name + " isCommittedToAnAttack = " + unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack);
                //GlobalDefinitions.writeToLogFile("calculateDefenseFactor:   calculateUnitDefendingFactor returns = " + calculateUnitDefendingFactor(unit, attackingUnits));
                if (unit.GetComponent<UnitDatabaseFields>().isCommittedToAnAttack)
                    totalDefendFactors += CalculateUnitDefendingFactor(unit, attackingUnits);
            }
            foreach (GameObject unit in defendingUnits)
                if (unit.GetComponent<UnitDatabaseFields>().occupiedHex.GetComponent<HexDatabaseFields>().closeDefenseSupport)
                    return (totalDefendFactors + 1);
            return (totalDefendFactors);
        }

        /// <summary>
        /// This returns the combat odds of the attackers and defenders passed
        /// </summary>
        /// <param name="defendingUnits"></param>
        /// <param name="attackingUnits"></param>
        /// <param name="addCombatAirSupport"></param>
        /// <returns></returns>
        public static int ReturnCombatOdds(List<GameObject> defendingUnits, List<GameObject> attackingUnits, bool attackAirSupport)
        {
            // This routine returns a positive integer when the attackers are stronger than the defender and a negative number when the defenders are stronger than the attackers
            int odds;
            // Odds are always rounded to the defenders advantage.  For example an attack 4 to defender 7 is 1:2
            // while an attack 7 to a defender 4 is 1:1
            if ((CalculateAttackFactor(attackingUnits, attackAirSupport) == 0) || (CalculateDefenseFactor(defendingUnits, attackingUnits) == 0))
            {
                IORoutines.WriteToLogFile(
                        "returnCombatOdds: returning 0 - attackFactor = " +
                        CalculateAttackFactor(attackingUnits, attackAirSupport) +
                        " - defense factor = " + CalculateDefenseFactor(defendingUnits, attackingUnits));
                return (0);
            }
            if (CalculateDefenseFactor(defendingUnits, attackingUnits) >
                    CalculateAttackFactor(attackingUnits, attackAirSupport))
            {
                if ((CalculateDefenseFactor(defendingUnits, attackingUnits) % CalculateAttackFactor(attackingUnits, attackAirSupport)) > 0)
                    odds = (CalculateDefenseFactor(defendingUnits, attackingUnits) / (int)CalculateAttackFactor(attackingUnits, attackAirSupport)) + 1;
                else
                    odds = (CalculateDefenseFactor(defendingUnits, attackingUnits) / (int)CalculateAttackFactor(attackingUnits, attackAirSupport));
                // 1:6 is the worst odds avaialble.  All odds greater than this will be returned as 7:1.
                if (odds > 6)
                    odds = 7;
                odds = -odds;
                return (odds);
            }
            else
            {
                odds = (int)CalculateAttackFactor(attackingUnits, attackAirSupport) / CalculateDefenseFactor(defendingUnits, attackingUnits);
                if (odds > 6)
                    odds = 7;
                return (odds);
            }
        }

        /// <summary>
        /// This is a special case.  Need the combat odds displayed when the user is selecting units for the battle but attack air support hasn't been assigned yet
        /// </summary>
        /// <param name="defendingUnits"></param>
        /// <param name="attackingUnits"></param>
        /// <returns></returns>
        public static int ReturnCombatGUICombatOdds(List<GameObject> defendingUnits, List<GameObject> attackingUnits)
        {
            // This routine returns a positive integer when the attackers are stronger than the defender and a negative number when the defenders are stronger than the attackers
            int odds;
            // Odds are always rounded to the defenders advantage.  For example an attack 4 to defender 7 is 1:2
            // while an attack 7 to a defender 4 is 1:1
            if ((CalculateAttackFactorWithoutAirSupport(attackingUnits) == 0) || (CalculateDefenseFactor(defendingUnits, attackingUnits) == 0))
                return (0);
            if (CalculateDefenseFactor(defendingUnits, attackingUnits) > CalculateAttackFactorWithoutAirSupport(attackingUnits))
            {
                if ((CalculateDefenseFactor(defendingUnits, attackingUnits) % CalculateAttackFactorWithoutAirSupport(attackingUnits)) > 0)
                    odds = (CalculateDefenseFactor(defendingUnits, attackingUnits) / (int)CalculateAttackFactorWithoutAirSupport(attackingUnits)) + 1;
                else
                    odds = (CalculateDefenseFactor(defendingUnits, attackingUnits) / (int)CalculateAttackFactorWithoutAirSupport(attackingUnits));
                // 1:6 is the worst odds avaialble.  All odds greater than this will be returned as 1:7.
                if (odds > 6)
                    odds = 7;
                odds = -odds;
                return (odds);
            }
            else
            {
                odds = (int)CalculateAttackFactorWithoutAirSupport(attackingUnits) / CalculateDefenseFactor(defendingUnits, attackingUnits);
                if (odds > 6)
                    odds = 7;
                return (odds);
            }
        }
    }

}
