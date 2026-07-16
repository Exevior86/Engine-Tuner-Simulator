using System.Collections.Generic;
using UnityEngine;

public class EngineConfigManager : MonoBehaviour
{
    [Header("Simulation Database")]
    public List<EnginePartData> availableParts = new List<EnginePartData>();
    public List<EnginePartData> equippedParts = new List<EnginePartData>();

    private EcuController ecu;
    private DiagnosticsPanelManager diagnosticsManager;

    private void Awake()
    {
        ecu = FindFirstObjectByType<EcuController>();
        diagnosticsManager = FindFirstObjectByType<DiagnosticsPanelManager>();

        LoadAllCatalogParts();
    }

    private void Start()
    {
        // Equip baseline stock parts if empty (Matching N/A startup state!)
        if (equippedParts.Count == 0)
        {
            EquipPartByName("ForgeSteel Cast-Iron 2.0L");
            EquipPartByName("ForgeSteel Cast Head");
            EquipPartByName("Kinetic Street Springs");
            EquipPartByName("ForgeSteel Cast Replacement");
            EquipPartByName("AeroFlow Mild-Street");
            EquipPartByName("AeroFlow OEM Upgrade");
            EquipPartByName("AeroFlow 65mm Upgrade");
            EquipPartByName("AeroFlow Cast Replacement");
            EquipPartByName("AeroFlow High-Flow Catted");
            EquipPartByName("AeroFlow Sport Muffler");
            EquipPartByName("Commercial 91 Octane");
            EquipPartByName("Hydra Flow-150 Pump");
            EquipPartByName("Hydra Flow-550");

            // Turbo, Intercooler, and EBC slots remain empty (No parts equipped)
        }

        ApplyConfiguration();
    }

    private void LoadAllCatalogParts()
    {
        availableParts.Clear();
        EnginePartData[] loadedParts = Resources.LoadAll<EnginePartData>("PartsCatalog");

        foreach (EnginePartData part in loadedParts)
        {
            if (part == null) continue;
            availableParts.Add(part);
        }
    }

    /// <summary>
    /// Evaluates equipped modifications polymorphically and updates the physical simulation limits
    /// </summary>
    public void ApplyConfiguration()
    {
        if (ecu == null) ecu = FindFirstObjectByType<EcuController>();

        // Create clean baseline state parameters
        EngineSimulationState engineState = new EngineSimulationState();

        // Loop through all parts. If they implement our interface, apply them!
        foreach (EnginePartData part in equippedParts)
        {
            if (part is IEngineModifier modifier)
            {
                modifier.ApplyModifications(engineState);
            }
        }

        // Push final computed parameters to the ECU physics engine safely
        if (ecu != null)
        {
            ecu.activeRedline = engineState.MaxRedline;
            ecu.activeMaxKpa = engineState.MaxKpa;
            // Map others if tracking:
            // ecu.activeVE = engineState.VolumetricEfficiency;
        }

        Debug.Log($"[Engine Config] Applied Setup: {engineState.Displacement}L | Redline: {engineState.MaxRedline} RPM | Max MAP: {engineState.MaxKpa:F1} kPa");

        if (diagnosticsManager != null)
        {
            diagnosticsManager.RefreshGraphs();
        }
    }

    public void EquipPartByName(string partName)
    {
        EnginePartData newPart = availableParts.Find(p => string.Equals(p.partName, partName, System.StringComparison.OrdinalIgnoreCase));
        if (newPart == null) return;

        // Safely remove any parts that share the exact same type to avoid duplicates
        equippedParts.RemoveAll(p => p.GetType() == newPart.GetType());

        equippedParts.Add(newPart);
        ApplyConfiguration();
    }

    public bool IsPartEquipped(string partName)
    {
        return equippedParts.Exists(p => string.Equals(p.partName, partName, System.StringComparison.OrdinalIgnoreCase));
    }
}