using UnityEngine;

[CreateAssetMenu(fileName = "NewCamshaft", menuName = "Engine Sim/Parts/Camshaft")]
public class CamshaftData : EnginePartData, IEngineModifier
{
    [Header("Valvetrain Timing")]
    public float valveLiftMm;         // Stat 1 (e.g., 9.2mm)
    public float advertisedDuration;  // Stat 2 (e.g., 256 degrees)
    public float peakVEShiftRpm;      // Stat 3 (RPM shift offset)

    public void ApplyModifications(EngineSimulationState state)
    {
        // Increased lift and duration directly improve volumetric breathing efficiency
        state.BaseVolumetricEfficiency += (valveLiftMm * 0.015f) + (advertisedDuration * 0.0005f);
    }
}