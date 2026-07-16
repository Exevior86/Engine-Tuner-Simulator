using UnityEngine;

[CreateAssetMenu(fileName = "NewPistonRods", menuName = "Engine Sim/Parts/Pistons and Rods")]
public class PistonRodsData : EnginePartData, IEngineModifier
{
    [Header("Piston Geometry")]
    [Tooltip("Compression ratio offset (Stat 1). e.g., -5 (dished low-comp), 0 (stock), +5 (domed high-comp).")]
    public float compressionOffset;      // Stat 1

    [Header("Structural Integrity & Mass")]
    [Tooltip("Piston & Rod material strength rating (Stat 2). 1 = Forged, 0 = Cast Stock.")]
    public float materialStrengthRating;  // Stat 2

    [Tooltip("Weight reduction of the rotating assembly in grams. e.g., 150g lighter components.")]
    public float weightReductionGrams;

    public void ApplyModifications(EngineSimulationState state)
    {
        // 1. Structural torque limit calculations
        // Upgrading to robust forged components directly allows the block to handle higher torque/cylinder pressures
        if (materialStrengthRating > 0)
        {
            // Boosts the maximum supportable cylinder pressure capability of our simulation
            state.FuelKnockResistanceFactor += 0.05f; // Forged pistons transfer heat better, helping prevent hotspots
        }

        // 2. Adjusting rotating inertia
        // Lighter internals allow the engine to rev higher safely, complementing upgraded valvetrains
        if (weightReductionGrams > 0)
        {
            state.MaxRedline += (weightReductionGrams * 0.5f); // E.g., shaving weight lets you spin faster safely
        }

        // 3. Compression ratio effects on knock and thermal efficiency
        // (If your physics engine calculates a dynamic compression ratio, you apply the offset here)
        Debug.Log($"[Engine Internals] Installed {partName}: Comp Offset: {compressionOffset:F1} | Strength Rating: {materialStrengthRating}");
    }
}