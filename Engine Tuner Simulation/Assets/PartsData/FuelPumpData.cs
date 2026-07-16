using UnityEngine;

[CreateAssetMenu(fileName = "NewFuelPump", menuName = "Engine Sim/Parts/Fuel Pump")]
public class FuelPumpData : EnginePartData, IEngineModifier
{
    [Header("Fuel Pump Specifications")]
    [Tooltip("Fuel flow rate in Liters Per Hour (LPH) (Stat 1). e.g., 150 LPH (stock), 255 LPH, 340 LPH, 450 LPH.")]
    public float flowRateLph;             // Stat 1

    [Tooltip("Maximum safe operating pressure in PSI (Stat 2). High pressure capability prevents flow drop-off under boost.")]
    public float maxOperatingPressurePsi;  // Stat 2

    [Tooltip("Efficiency rating of the pump motor. Higher efficiency reduces fuel heating/voltage drop-off.")]
    [Range(0.8f, 1.0f)]
    public float pumpEfficiency = 0.9f;

    public void ApplyModifications(EngineSimulationState state)
    {
        // 1. Set the absolute fuel delivery limit in the simulation state
        // This acts as a hard cap on how much fuel the ECU can physically inject,
        // regardless of how large the fuel injectors are!
        state.FuelPumpFlowLitersPerHour = flowRateLph * pumpEfficiency;

        // 2. If the fuel pump is a bottleneck, we can calculate a dynamic fueling horsepower safety ceiling:
        // (Roughly: 1 LPH of gasoline supports about 2 to 2.5 HP safely depending on AFR)
        float maxSupportableHp = state.FuelPumpFlowLitersPerHour * 2.2f;

        // We can use this to establish a warning/diagnostic limit or push it to the ECU
        Debug.Log($"[Fuel System] Equipped {partName}: Safe fuel pump limit is {state.FuelPumpFlowLitersPerHour:F0} LPH (~{maxSupportableHp:F0} Max HP capacity)");
    }
}