using UnityEngine;

[CreateAssetMenu(fileName = "New Turbocharger", menuName = "Apex Sim/Parts/Forced Induction/Turbocharger")]
public class TurboData : EnginePartData
{
    [Header("Compressor Parameters")]
    [Tooltip("Peak air mass flow rate in lbs/min. Dictates the absolute ceiling of horsepower potential.")]
    public float maxFlowLbsMin;

    [Header("Turbine & Housing Dynamics")]
    [Tooltip("Engine speed (RPM) where exhaust energy is sufficient to begin building positive manifold pressure.")]
    public float spoolRPM;

    [Tooltip("Maximum mechanical or structural pressure ceiling in PSI before wastegate bypass or backpressure choking occurs.")]
    public float maxBoostLimit;

    [Tooltip("Area/Radius ratio of the turbine housing. Lower = faster low-end spool; Higher = massive top-end flow with increased lag.")]
    public float turbineHousingA_R;

    [Header("Future Expansion")]
    [Tooltip("Reserved value slot for potential simulation extensions (e.g., compressor efficiency coefficients).")]
    public float extraSimulationScalar;
}

[CreateAssetMenu(fileName = "New Supercharger", menuName = "Apex Sim/Parts/Forced Induction/Supercharger")]
public class SuperchargerData : EnginePartData
{
    [Header("Mechanical Displacement")]
    [Tooltip("Cubic inches or cubic centimeters of air volume displaced per physical input shaft rotation.")]
    public float displacementPerRev;

    [Tooltip("The maximum step-up drive ratio limit for the driving pulley assembly before belt slip or mechanical shear.")]
    public float maxDriveRatio;
}

[CreateAssetMenu(fileName = "New Intercooler", menuName = "Apex Sim/Parts/Forced Induction/Intercooler")]
public class IntercoolerData : EnginePartData
{
    [Header("Thermal Dynamics")]
    [Tooltip("Internal core volume capacity measured in cubic inches.")]
    public float coreVolumeCubicInches;

    [Tooltip("Thermal efficiency coefficient rating from 0.0 to 1.0. Directly scales down calculated Intake Air Temperature (IAT) values.")]
    public float coolingEfficiencyFactor;
}

[CreateAssetMenu(fileName = "New Boost Controller", menuName = "Apex Sim/Parts/Forced Induction/Boost Controller")]
public class BoostControllerData : EnginePartData
{
    [Header("Solenoid Mapping")]
    [Tooltip("True = Electronic 3/4-way pulse-width modulation mapping via software. False = Static mechanical spring bleed valve.")]
    public bool isElectronic;

    [Tooltip("The operational frequency limit of the electronic solenoid mechanism measured in Hertz.")]
    public float maxFrequencyHz;
}