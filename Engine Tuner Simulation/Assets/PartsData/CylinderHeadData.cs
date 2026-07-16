using UnityEngine;

[CreateAssetMenu(fileName = "NewCylinderHead", menuName = "Engine Sim/Parts/Cylinder Head")]
public class CylinderHeadData : EnginePartData, IEngineModifier
{
    [Header("Port & Valve Geometry")]
    [Tooltip("Intake valve diameter in mm (Stat 1). Larger valves allow massive high-RPM flow.")]
    public float valveDiameterMm;        // Stat 1 (e.g., 45mm to 58mm)

    [Tooltip("Number of valves per cylinder (Stat 2). 4-valve designs rev higher and flow better than 2-valve.")]
    public int valvesPerCylinder;        // Stat 2 (e.g., 2 or 4)

    [Header("Combustion Chamber & Thermal Performance")]
    [Tooltip("Optimized chamber shape increases knock resistance by eliminating hot spots.")]
    [Range(0f, 0.2f)]
    public float combustionCoolingFactor; // Material/shape heat dissipation benefit

    [Tooltip("Static compression ratio modification. Positive = tighter chamber (more power, higher knock risk).")]
    public float compressionRatioOffset;  // e.g., +0.5 to compression ratio (10.0:1 -> 10.5:1)

    public void ApplyModifications(EngineSimulationState state)
    {
        // 1. Calculate volumetric efficiency scaling based on total valve area
        // A 4-valve head has significantly more curtain area than a 2-valve head
        float valveAreaScale = (valveDiameterMm * 0.01f) * (valvesPerCylinder == 4 ? 1.4f : 1.0f);
        state.BaseVolumetricEfficiency += valveAreaScale;

        // 2. Multi-valve heads feature lighter components, directly raising safe rev potential
        if (valvesPerCylinder == 4)
        {
            state.MaxRedline = Mathf.Max(state.MaxRedline, 7500f);
        }

        // 3. Optimized combustion chambers and alloy materials directly fight engine knock
        state.FuelKnockResistanceFactor += combustionCoolingFactor;

        // 4. Note: If your ECU tracks compression ratio, apply the offset here:
        // state.CompressionRatio += compressionRatioOffset;
    }
}