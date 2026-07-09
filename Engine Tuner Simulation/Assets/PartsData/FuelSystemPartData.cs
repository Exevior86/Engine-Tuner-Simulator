using UnityEngine;

[CreateAssetMenu(fileName = "New Fuel Profile", menuName = "Apex Sim/Parts/Fuel/Fuel Profile")]
public class FuelData : EnginePartData
{
    [Header("Combustion Parameters")]
    [Tooltip("Resistance to auto-ignition / knock ceiling (e.g., 91, 93, 105 octane).")]
    public float octaneRating;

    [Tooltip("The chemically perfect air-to-fuel balance balance point for a clean burn (e.g., 14.7 for pump gas, 9.7 for pure E85).")]
    public float stoichiometricAFR;

    [Tooltip("The latent heat evaporation cooling modifier. Higher values drop calculated Intake Air Temperature (IAT) during combustion.")]
    public float coolingFactor;
}

[CreateAssetMenu(fileName = "New Fuel Pump", menuName = "Apex Sim/Parts/Fuel/Fuel Pump")]
public class FuelPumpData : EnginePartData
{
    [Header("Pump Mechanics")]
    public float flowRateLitersPerHour;
    public float maxPressurePSI;
}

[CreateAssetMenu(fileName = "New Injectors", menuName = "Apex Sim/Parts/Fuel/Injectors")]
public class InjectorData : EnginePartData
{
    [Header("Injector Flow")]
    public float flowRateCCMin;
    public float maxSafeDutyCycle = 0.85f;
}

[CreateAssetMenu(fileName = "New Flex Fuel Sensor", menuName = "Apex Sim/Parts/Fuel/Flex Fuel Sensor")]
public class FlexFuelSensorData : EnginePartData
{
    [Header("Sensor Logic")]
    public bool permitsEthanolBlending;
}