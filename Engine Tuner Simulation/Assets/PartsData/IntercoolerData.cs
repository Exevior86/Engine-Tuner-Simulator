using UnityEngine;

[CreateAssetMenu(fileName = "NewIntercooler", menuName = "Engine Sim/Parts/Intercooler")]
public class IntercoolerData : EnginePartData, IEngineModifier
{
    [Header("Charge Air Cooling")]
    public float heatDissipationCapacityWatts; // Stat 1
    public float flowEfficiencyFactor;         // Stat 2

    public void ApplyModifications(EngineSimulationState state)
    {
        state.IntakeChargeCoolingFactor = flowEfficiencyFactor;
        state.BaseVolumetricEfficiency += 0.02f; // Cooler, denser air increases VE
    }
}