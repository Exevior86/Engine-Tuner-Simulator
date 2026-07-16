using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PartConfig
{
    public string partType; // e.g., "Camshaft", "Turbo", "EngineBlock"
    public string partName;
    public int cost;
    public float weight;
    public int tier;
    public string imageName;

    // The 5 generic telemetry/physics stats from your spreadsheet
    public float stat1;
    public float stat2;
    public float stat3;
    public float stat4;
    public float stat5;
}