using UnityEngine;

[CreateAssetMenu(fileName = "NewDownpipe", menuName = "Engine Sim/Parts/Downpipe")]
public class DownpipeData : EnginePartData, IEngineModifier
{
    [Header("Downpipe Dimensions")]
    [Tooltip("Internal pipe diameter in inches (Stat 1). e.g., 2.5, 3.0, or 3.5 inches.")]
    public float pipeDiameterInches;     // Stat 1

    [Header("Catalytic Converter Restriction")]
    [Tooltip("Does this unit feature a catalytic converter? (Stat 2: 1 = Catted, 0 = Catless/Straight Pipe)")]
    public bool isCatted;                // Stat 2

    [Tooltip("Overall reduction in flow restriction. e.g., 0.25 = 25% drop in backpressure.")]
    [Range(0f, 0.5f)]
    public float restrictionReduction;

    public void ApplyModifications(EngineSimulationState state)
    {
        // 1. Directly drop exhaust backpressure
        state.ExhaustBackpressureFactor -= restrictionReduction;

        // 2. Increase base Volumetric Efficiency (VE) as diameter expands
        float veGain = (pipeDiameterInches - 2.0f) * 0.04f; // Every half-inch over 2.0" adds substantial flow
        state.BaseVolumetricEfficiency += Mathf.Max(0f, veGain);

        // 3. Drop Turbo Spool Threshold RPM (massive impact on lag!)
        // Catless designs spool even faster because there is zero substrate slowing down exhaust pulses
        float spoolRpmReduction = isCatted ? 200f : 400f;
        state.TurboSpoolThresholdRpm -= spoolRpmReduction;

        // 4. Dropping EGTs reduces heat-soak knock risks
        if (!isCatted)
        {
            state.FuelKnockResistanceFactor += 0.05f; // Catless yields maximum thermal relief
        }
    }
}