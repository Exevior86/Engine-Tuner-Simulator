using UnityEngine;

[CreateAssetMenu(fileName = "NewFlexFuelSensor", menuName = "Engine Sim/Parts/Flex Fuel Sensor")]
public class FlexFuelSensorData : EnginePartData, IEngineModifier
{
    [Header("Diagnostics")]
    public bool isEnabled; // Stat 1

    public void ApplyModifications(EngineSimulationState state)
    {
        state.HasFlexFuelSensor = isEnabled;
    }
}