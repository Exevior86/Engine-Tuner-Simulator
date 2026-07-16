using UnityEngine;

[CreateAssetMenu(fileName = "NewTurbo", menuName = "Engine Sim/Parts/Turbo")]
public class TurboData : EnginePartData, IEngineModifier
{
    [Header("Forced Induction")]
    public float compressorFlowLbMin; // Stat 1 (Air mass flow potential, e.g. 50 lb/min)
    public float spoolThresholdRpm;   // Stat 2 (e.g. 2800 RPM)
    public float maxBoostPsi;         // Stat 3 (e.g. 18 PSI limit)
    public float turbineA_R;          // Stat 4 (Larger A/R = more top-end power, but more lag)

    public void ApplyModifications(EngineSimulationState state)
    {
        // Set maximum absolute manifold pressure (MAP) in kPa
        state.MaxPressureAbsoluteKpa = 101.325f + (maxBoostPsi * 6.89476f);
        state.TurboSpoolThresholdRpm = spoolThresholdRpm;

        // Flow limit of compressor wheel
        state.MaxAirflowGramsPerSecond = Mathf.Max(state.MaxAirflowGramsPerSecond, compressorFlowLbMin * 7.5599f);
    }
}