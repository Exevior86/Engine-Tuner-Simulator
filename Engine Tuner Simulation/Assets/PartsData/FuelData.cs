using UnityEngine;

[CreateAssetMenu(fileName = "NewFuel", menuName = "Engine Sim/Parts/Fuel")]
public class FuelData : EnginePartData, IEngineModifier
{
    [Header("Chemical Properties")]
    public float octaneRating;             // Stat 1 (e.g., 91, 93, 105, 120)
    public float stoichiometricAFR;        // Stat 2 (e.g., 14.7 gasoline, 9.7 ethanol)
    public float latentHeatEvaporationVal; // Stat 3 (Cooling/anti-knock rating)

    public void ApplyModifications(EngineSimulationState state)
    {
        state.FuelOctaneRating = octaneRating;
        state.FuelStoichiometricAFR = stoichiometricAFR;
        state.FuelKnockResistanceFactor += latentHeatEvaporationVal;
    }
}