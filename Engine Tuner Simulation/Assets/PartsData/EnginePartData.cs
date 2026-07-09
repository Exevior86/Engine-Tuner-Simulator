using UnityEngine;

public abstract class EnginePartData : ScriptableObject
{
    [Header("General Logistics")]
    public string partName;
    [TextArea(2, 5)]
    public string partDescription;
    public float purchaseCost;

    public Sprite partIcon;

    [Header("Progression & Mechanics")]
    public int unlockTier;          // Career mode tier level (Tier 1, Tier 2, etc.)
    public float componentWeight;   // Weight in lbs or kg (affects total vehicle weight)

    [Header("Degradation (Optional)")]
    public float maximumDurability = 100f; // For wear-and-tear mechanics later
}