using UnityEngine;

[CreateAssetMenu(fileName = "NewBoostController", menuName = "Engine Sim/Parts/Boost Controller")]
public class BoostControllerData : EnginePartData, IEngineModifier
{
    [Header("Boost Control Characteristics")]
    [Tooltip("Maximum safe boost pressure above wastegate spring pressure this controller can regulate.")]
    public float maxRegulatedBoostPsi;      // Stat 1 (e.g. 25 PSI, 33 PSI, up to 100 PSI)

    [Tooltip("Percent reduction in spool time (keeps wastegate closed longer). e.g., 0.15 = 15% faster spool.")]
    [Range(0f, 0.5f)]
    public float spoolEfficiencyGain;       // Stat 2

    [Tooltip("Reduces boost spikes/instability. Higher = lower risk of unexpected knock.")]
    [Range(0f, 1f)]
    public float boostStabilityFactor;      // Stat 3

    public void ApplyModifications(EngineSimulationState state)
    {
        // 1. Calculate and update the maximum absolute pressure the engine can target based on this controller's limits
        float absoluteKpaLimit = 101.325f + (maxRegulatedBoostPsi * 6.89476f);

        // The ECU's target boost capacity is physically capped by whichever is lower: 
        // The turbo's physical flow capacity OR the boost controller's regulation limit!
        state.MaxPressureAbsoluteKpa = Mathf.Min(state.MaxPressureAbsoluteKpa, absoluteKpaLimit);

        // 2. Upgraded electronic solenoids keep the wastegate tightly closed, lowering the spool threshold
        state.TurboSpoolThresholdRpm -= (state.TurboSpoolThresholdRpm * spoolEfficiencyGain);

        // 3. High-quality controllers stabilize manifold pressure, directly combating feedback knock
        state.FuelKnockResistanceFactor += boostStabilityFactor * 0.10f;
    }
}