using UnityEngine;

[CreateAssetMenu(fileName = "NewIntakeManifold", menuName = "Engine Sim/Parts/Intake Manifold")]
public class IntakeManifoldData : EnginePartData, IEngineModifier
{
    [Header("Air Induction")]
    public float plenumVolumeLiters;  // Stat 1
    public float maxAirflowCapacity;  // Stat 2 (g/s flow limit)

    public void ApplyModifications(EngineSimulationState state)
    {
        state.BaseVolumetricEfficiency += 0.04f;
        state.MaxAirflowGramsPerSecond += maxAirflowCapacity;
    }
}