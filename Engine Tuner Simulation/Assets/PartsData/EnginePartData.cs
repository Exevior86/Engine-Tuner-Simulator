using UnityEngine;

public abstract class EnginePartData : ScriptableObject
{
    [Header("General Logistics")]
    public string partName;
    [TextArea(2, 5)]
    public string partDescription;
    public float purchaseCost;
    public string imageFileName;

    [Header("Progression & Mechanics")]
    public int unlockTier;
    public float componentWeight;
}