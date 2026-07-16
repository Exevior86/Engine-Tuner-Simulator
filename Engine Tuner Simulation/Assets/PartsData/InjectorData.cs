using UnityEngine;

[CreateAssetMenu(fileName = "NewInjector", menuName = "Engine Sim/Parts/Injectors")]
public class InjectorData : EnginePartData, IEngineModifier
{
    [Header("Injector Specifications")]
    [Tooltip("Maximum fuel flow rate in cc/min (Stat 1). e.g., 550, 1000, 1300, 2200.")]
    public float flowRateCcMin;          // Stat 1

    [Tooltip("Atomization and spray quality factor (Stat 2). Higher values represent finer mist and cleaner burn.")]
    [Range(0.8f, 1.0f)]
    public float atomizationFactor;      // Stat 2

    [Tooltip("The mechanical opening delay (latency) in milliseconds. Larger injectors typically have slightly higher latency.")]
    public float latencyMilliseconds;

    public void ApplyModifications(EngineSimulationState state)
    {
        // 1. Establish the absolute injection volume ceiling in the simulation state
        state.InjectorFlowCcPerMin = flowRateCcMin;

        // 2. High-quality atomization slightly increases Volumetric Efficiency (VE) 
        // because vaporized fuel takes up less physical liquid volume in the intake port
        float veBonus = (atomizationFactor - 0.8f) * 0.1f;
        state.BaseVolumetricEfficiency += Mathf.Max(0f, veBonus);

        // 3. Excellent atomization acts as a minor cooling/anti-knock buffer
        state.FuelKnockResistanceFactor += (atomizationFactor - 0.8f) * 0.2f;

        Debug.Log($"[Fuel System] Installed {partName}: Flow Capacity is {flowRateCcMin} cc/min. Atomization Rating: {atomizationFactor * 100f:F0}%");
    }
}