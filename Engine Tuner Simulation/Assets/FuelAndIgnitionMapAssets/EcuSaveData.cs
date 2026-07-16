using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    [Header("Player Economy")]
    public float playerCash;

    [Header("Garage Inventory System")]
    // Rather than saving the ScriptableObject itself, save the unique item ID strings
    public string equippedEngineCode;
    public string equippedTurboId;
    public string equippedValvetrainId;
    public string equippedCylinderHeadId;

    // List of unique item ID strings the player has bought and owns in their storage
    public List<string> ownedPartsInventory = new List<string>();

    [Header("ECU Tables")]
    public float[] fuelTable;
    public float[] ignitionTable;

    [Header("Saved Dyno History")]
    public List<DynoRunLog> dynoHistory = new List<DynoRunLog>();
}

[System.Serializable]
public class DynoRunLog
{
    public string timestamp;
    public float maxHorsepower;
    public float maxTorque;
    public string statusSummary; // e.g., "Clean Pull" or "Engine Knock Blocked!"
}