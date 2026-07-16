using UnityEngine;

[CreateAssetMenu(fileName = "NewExhaustHeaders", menuName = "Engine Sim/Parts/Exhaust Headers")]
public class ExhaustHeadersData : EnginePartData, IEngineModifier
{
    [Header("Exhaust Header Specs")]
    public float primaryRunnerDiameterInches; // Stat 1 (e.g. 1.75" or 2.0" primary pipes)
    public bool isLongTubeHeader;              // Stat 2 (Long-tubes scavenge better, shorties spool faster)
    public float restrictionReduction;         // Stat 3 (Efficiency multiplier, e.g. 0.15 for 15% improvement)

    public void ApplyModifications(EngineSimulationState state)
    {
        // 1. Reducing exhaust restriction directly lowers backpressure
        state.ExhaustBackpressureFactor -= restrictionReduction;

        // 2. Lower backpressure allows the cylinder to clear exhaust gas more cleanly, increasing Volumetric Efficiency
        state.BaseVolumetricEfficiency += (primaryRunnerDiameterInches * 0.02f);

        // 3. Spool characteristics change:
        if (isLongTubeHeader)
        {
            // Long tubes scavenge exhaust gas incredibly well at mid-to-high RPM, boosting VE
            state.BaseVolumetricEfficiency += 0.03f;
            state.TurboSpoolThresholdRpm -= 100f; // Minor spool reduction
        }
        else
        {
            // Shorty headers keep exhaust velocity high close to the turbine inlet, speeding up turbo spool
            state.TurboSpoolThresholdRpm -= 250f; // Significant spool reduction
        }
    }
}