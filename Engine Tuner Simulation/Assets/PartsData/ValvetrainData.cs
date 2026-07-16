using UnityEngine;

[CreateAssetMenu(fileName = "NewValvetrain", menuName = "Engine Sim/Parts/Valvetrain")]
public class ValvetrainData : EnginePartData, IEngineModifier
{
    [Header("Port Flow & Valvetrain")]
    public float safeRedlineLimit;     // Stat 1 (e.g. 8500 RPM)
    public float valveFlowIncrease;    // Stat 2 (e.g. +0.12 VE flow)

    public void ApplyModifications(EngineSimulationState state)
    {
        state.MaxRedline = Mathf.Max(state.MaxRedline, safeRedlineLimit);
        state.BaseVolumetricEfficiency += valveFlowIncrease;
    }
}