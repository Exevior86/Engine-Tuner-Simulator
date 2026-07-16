using UnityEngine;

[CreateAssetMenu(fileName = "NewCatBackExhaust", menuName = "Engine Sim/Parts/Cat-Back Exhaust")]
public class CatBackExhaustData : EnginePartData, IEngineModifier
{
    [Header("Exhaust System")]
    public float pipeDiameterInches;    // Stat 1
    public float restrictionReduction;  // Stat 2

    public void ApplyModifications(EngineSimulationState state)
    {
        state.ExhaustBackpressureFactor -= restrictionReduction;
        state.BaseVolumetricEfficiency += 0.03f;

        // Shave off lag due to improved exhaust gas expansion speed
        state.TurboSpoolThresholdRpm -= 150f;
    }
}