using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CarTuningSessionManager : MonoBehaviour
{
    [Header("Save System Settings")]
    [Tooltip("The filename used to write this specific engine profile to disk.")]
    public string saveFileName = "active_ecu_tune.json";

    [Header("Central Manager References")]
    public FuelMappingManager fuelMappingManager;
    public IgnitionTimingManager ignitionTimingManager;
    public EconomyManager economyManager;       
    public EngineConfigManager configManager;   

    [Header("Active Session Vehicle Data")]
    [Tooltip("If blank, the script will generate a default stock 4-cylinder profile asset to prevent crash states.")]
    public EngineData activeEngineProfile;

    private void Start()
    {
        // 1. Try to load the persistent engine configuration from the global game state
        if (GameManager.Instance != null && GameManager.Instance.equippedEngine != null)
        {
            activeEngineProfile = GameManager.Instance.equippedEngine;
            Debug.Log($"[Tuning Session] Successfully linked to active vehicle data: {activeEngineProfile.engineName}");
        }
        else
        {
            // 2. Fallback: Generate a clean temporary stock profile
            Debug.LogWarning("[Tuning Session] No persistent vehicle data located in GameManager. Initializing a factory baseline profile for testing.");
            CreateFallbackStockProfile();
        }

        // ➔ AUTO-LOAD: Check if there is a saved file on the hard drive and overwrite our active tables
        LoadSavedEcuTune();

        // 3. Command the ECU dashboards to populate their grid layouts
        SyncAndRefreshEcuDashboards();
    }

    private void OnDisable()
    {
        // Auto-saves if the active panel is turned off, or if we transition to the Dyno scene!
        AutoSaveEcuTune();
    }

    private void OnApplicationQuit()
    {
        // Auto-saves the exact millisecond the player Alt+F4s or closes the application!
        AutoSaveEcuTune();
    }

    /// <summary>
    /// Silent background save that doesn't spam normal UI buttons.
    /// </summary>
    private void AutoSaveEcuTune()
    {
        if (activeEngineProfile == null) return;

        // 1. Initialize default/fallback arrays
        List<string> owned = new List<string>();
        List<string> equipped = new List<string>();
        float cash = 5000f;

        // 2. Safely extract values from our decoupled managers if they exist in the scene
        if (economyManager != null)
        {
            cash = economyManager.CurrentMoney;

            // Re-verify what parts we own from our serialized list
            // (Make sure ownedPartNames is marked public or has a public getter in EconomyManager)
            owned = economyManager.GetOwnedPartNames();
        }

        if (configManager != null)
        {
            foreach (var part in configManager.equippedParts)
            {
                if (part != null) equipped.Add(part.partName);
            }
        }

        // 3. Package everything into the master serialization payload
        ECUSaveData saveData = new ECUSaveData
        {
            engineCode = activeEngineProfile.engineCode,
            fuelTable = activeEngineProfile.savedFuelTableData,
            ignitionTable = activeEngineProfile.savedIgnitionTableData,

            playerMoney = cash,
            ownedPartNames = owned,
            equippedPartNames = equipped
        };

        string jsonText = JsonUtility.ToJson(saveData, true);
        string fullPath = Path.Combine(Application.persistentDataPath, saveFileName);

        File.WriteAllText(fullPath, jsonText);
        Debug.Log($"[Auto-Save] Full profile backup (ECU + Inventory) saved to: {fullPath}");
    }

    /// <summary>
    /// Forces the interactive screen UI grids to completely clear, recalculate their 
    /// scaling boundaries, and draw the matching cell layouts.
    /// </summary>
    public void SyncAndRefreshEcuDashboards()
    {
        if (activeEngineProfile == null)
        {
            Debug.LogError("[Tuning Session] Aborting layout sync. Active engine data object is missing!");
            return;
        }

        // Initialize the Fuel Map UI space we just finalized
        if (fuelMappingManager != null)
        {
            fuelMappingManager.InitializeFuelMapUI(activeEngineProfile);
        }
        else
        {
            Debug.LogWarning("[Tuning Session] FuelMappingManager link is unassigned in the inspector slot.");
        }

        // ➔ FIX: Uncommented this block so the Ignition Table is actively commanded to draw!
        if (ignitionTimingManager != null)
        {
            ignitionTimingManager.InitializeIgnitionMapUI(activeEngineProfile);
        }
        else
        {
            Debug.LogWarning("[Tuning Session] IgnitionTimingManager link is unassigned in the inspector slot.");
        }
    }

    /// <summary>
    /// Generates a bulletproof, 100% factory stock 4-cylinder calibration file.
    /// </summary>
    private void CreateFallbackStockProfile()
    {
        activeEngineProfile = ScriptableObject.CreateInstance<EngineData>();
        activeEngineProfile.engineName = "Factory Stock 4-Cylinder";
        activeEngineProfile.engineCode = "4G-NATURAL";
        activeEngineProfile.cylinderCount = 4;
        activeEngineProfile.factoryMaxRPM = 6500f;
        activeEngineProfile.baseMinKpa = 20f;
        activeEngineProfile.baseMaxKpa = 115f;

        // Ensure the persistent GameManager takes ownership of this data asset so the dyno can see it
        if (GameManager.Instance != null)
        {
            GameManager.Instance.equippedEngine = activeEngineProfile;
        }
    }

    /// <summary>
    /// Serializes the active engine's fuel and ignition arrays into a persistent JSON file.
    /// Attach this function to your UI "Save" button!
    /// </summary>
    public void SaveActiveEcuTune()
    {
        if (activeEngineProfile == null) return;

        // 1. Initialize default/fallback arrays
        List<string> owned = new List<string>();
        List<string> equipped = new List<string>();
        float cash = 5000f;

        // 2. Safely extract values from our decoupled managers if they exist in the scene
        if (economyManager != null)
        {
            cash = economyManager.CurrentMoney;

            // Re-verify what parts we own from our serialized list
            // (Make sure ownedPartNames is marked public or has a public getter in EconomyManager)
            owned = economyManager.GetOwnedPartNames();
        }

        if (configManager != null)
        {
            foreach (var part in configManager.equippedParts)
            {
                if (part != null) equipped.Add(part.partName);
            }
        }

        // 3. Package everything into the master serialization payload
        ECUSaveData saveData = new ECUSaveData
        {
            engineCode = activeEngineProfile.engineCode,
            fuelTable = activeEngineProfile.savedFuelTableData,
            ignitionTable = activeEngineProfile.savedIgnitionTableData,

            playerMoney = cash,
            ownedPartNames = owned,
            equippedPartNames = equipped
        };

        string jsonText = JsonUtility.ToJson(saveData, true);
        string fullPath = Path.Combine(Application.persistentDataPath, saveFileName);

        File.WriteAllText(fullPath, jsonText);
        Debug.Log($"[Auto-Save] Full profile backup (ECU + Inventory) saved to: {fullPath}");
    }

    /// <summary>
    /// Looks for a saved JSON file and overwrites the active engine's maps with those values.
    /// </summary>
    public void LoadSavedEcuTune()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[Save System] No save file located at {fullPath}. Using baseline defaults.");
            return;
        }

        string jsonText = File.ReadAllText(fullPath);
        ECUSaveData loadedData = JsonUtility.FromJson<ECUSaveData>(jsonText);

        // 1. Restore the engine data arrays directly into active calibration
        if (activeEngineProfile != null)
        {
            activeEngineProfile.savedFuelTableData = loadedData.fuelTable;
            activeEngineProfile.savedIgnitionTableData = loadedData.ignitionTable;

            Debug.Log("[Save System] ECU Calibration successfully loaded!");
            SyncAndRefreshEcuDashboards();
        }

        // 2. Restore Economy Progress (Wallet and Garage Slots)
        if (economyManager != null && loadedData.ownedPartNames != null && loadedData.ownedPartNames.Count > 0)
        {
            economyManager.LoadSavedEconomyState(loadedData.playerMoney, loadedData.ownedPartNames);
        }

        // 3. Restore Equipped Configuration State
        if (configManager != null && loadedData.equippedPartNames != null && loadedData.equippedPartNames.Count > 0)
        {
            configManager.equippedParts.Clear();
            foreach (string partName in loadedData.equippedPartNames)
            {
                configManager.EquipPartByName(partName);
            }
            configManager.ApplyConfiguration();
        }
    }

    [System.Serializable]
    public class ECUSaveData
    {
        public string engineCode;
        public float[] fuelTable;
        public float[] ignitionTable;

        // ➔ ADDED: Global player profile progress fields
        public float playerMoney;
        public List<string> ownedPartNames;
        public List<string> equippedPartNames;
    }
}