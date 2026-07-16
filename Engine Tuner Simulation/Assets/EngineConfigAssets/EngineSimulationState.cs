using UnityEngine;

public class EngineSimulationState
{
    // Core Displacement & RPM Limits
    public float DisplacementLiters = 2.0f;
    public float MaxRedline = 6000f;

    // Airflow & Breathing Capacity
    public float BaseVolumetricEfficiency = 0.80f; // NA baseline efficiency
    public float MaxAirflowGramsPerSecond = 150f;  // Max flow the intake/engine can physically swallow

    // Exhaust Restriction (Backpressure)
    public float ExhaustBackpressureFactor = 1.0f; // 1.0 = stock restriction. Lower is better.

    // Forced Induction Performance
    public float MaxPressureAbsoluteKpa = 101.325f; // Baseline MAP (Naturally Aspirated)
    public float TurboSpoolThresholdRpm = 3000f;    // RPM where turbo starts building positive pressure
    public float TurboEfficiencyFactor = 0.70f;     // Thermal efficiency of compressed air

    // Cooling Performance
    public float IntakeChargeCoolingFactor = 0.20f; // Effectiveness of cooling. Higher = denser air.

    // Fueling & Injection Limits
    public float FuelOctaneRating = 91f;
    public float FuelStoichiometricAFR = 14.7f;
    public float FuelKnockResistanceFactor = 0.0f;  // Extra protection buffer
    public float InjectorFlowCcPerMin = 550f;       // Max fuel volume capacity
    public float FuelPumpFlowLitersPerHour = 150f;  // Fuel delivery limit
    public bool HasFlexFuelSensor = false;          // Enables logging/scaling

    // --- BACKWARDS COMPATIBILITY ALIASES ---
    // These properties map the old simple names directly to the new physics-focused fields!
    public float Displacement
    {
        get => DisplacementLiters;
        set => DisplacementLiters = value;
    }

    public float MaxKpa
    {
        get => MaxPressureAbsoluteKpa;
        set => MaxPressureAbsoluteKpa = value;
    }
}

public interface IEngineModifier
{
    void ApplyModifications(EngineSimulationState state);
}