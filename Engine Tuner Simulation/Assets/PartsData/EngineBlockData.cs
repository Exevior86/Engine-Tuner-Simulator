using UnityEngine;

[CreateAssetMenu(fileName = "NewEngineBlock", menuName = "Engine Sim/Parts/Engine Block")]
public class EngineBlockData : EnginePartData, IEngineModifier
{
    [Header("Block Architecture")]
    public float displacementLiters; // Stat 1 (e.g., 2.0L or 2.4L)
    public int cylinderCount;        // Stat 2 (e.g., 4 or 6)
    public float maximumTorqueLimit; // Stat 3 (Structural limits before bending rods)

    public void ApplyModifications(EngineSimulationState state)
    {
        state.DisplacementLiters = displacementLiters;
    }
}