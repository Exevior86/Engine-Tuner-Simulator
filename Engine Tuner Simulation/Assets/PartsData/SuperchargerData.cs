using UnityEngine;

[CreateAssetMenu(fileName = "NewSupercharger", menuName = "Engine Sim/Parts/Supercharger")]
public class SuperchargerData : EnginePartData, IEngineModifier
{
    [Header("Mechanical Supercharging")]
    public float instantaneousBoostPsi; // Stat 1 (Constant mechanical boost delivery)
    public float pressureMultiplier;    // Stat 2

    public void ApplyModifications(EngineSimulationState state)
    {
        state.MaxPressureAbsoluteKpa = 101.325f + (instantaneousBoostPsi * 6.89476f);
        state.TurboSpoolThresholdRpm = 0f; // Instant mechanical response
    }
}