using UnityEngine;

[CreateAssetMenu(fileName = "NewThrottleBody", menuName = "Engine Sim/Parts/Throttle Body")]
public class ThrottleBodyData : EnginePartData, IEngineModifier
{
    [Header("Throttle Body Dimensions")]
    [Tooltip("Internal bore diameter in millimeters (Stat 1). e.g., 65mm, 70mm, 80mm, 90mm, 105mm.")]
    public float throttleDiameterMm;     // Stat 1

    [Header("Response & Flow Characteristics")]
    [Tooltip("Airflow response rating. Larger throttle bodies allow instant plenum pressure fill.")]
    [Range(1f, 1.5f)]
    public float responseScaleMultiplier = 1.0f;

    public void ApplyModifications(EngineSimulationState state)
    {
        // 1. Calculate breathing efficiency gains based on cross-sectional area scaling
        // We use 65mm as our baseline (0% gain)
        float areaScale = (throttleDiameterMm * throttleDiameterMm) / (65f * 65f);
        float veGain = (areaScale - 1f) * 0.05f; // Every step up yields a solid boost to high-RPM breathing

        state.BaseVolumetricEfficiency += Mathf.Max(0f, veGain);

        // 2. Increase maximum physical airflow cap (g/s)
        state.MaxAirflowGramsPerSecond += (throttleDiameterMm - 65f) * 5f;

        // 3. Improve turbo spool speed slightly because the intake tract can fill instantly
        state.TurboSpoolThresholdRpm -= (throttleDiameterMm - 65f) * 8f;

        Debug.Log($"[Induction] Installed {partName}: Bore Size: {throttleDiameterMm}mm | Flow Area Factor: {areaScale:F2}x");
    }
}