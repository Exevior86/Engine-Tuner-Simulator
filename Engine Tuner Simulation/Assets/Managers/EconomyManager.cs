using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [Header("Economy Configuration")]
    [SerializeField] private float playerMoney = 5000f;
    [SerializeField] private List<string> ownedPartNames = new List<string>();

    public float CurrentMoney => playerMoney;

    // Decoupled Event: UI or other systems can listen to this without caring HOW the money changed
    public event Action<float> OnMoneyChanged;
    public event Action<string> OnPartUnlocked;

    private void Awake()
    {
        // Establish baseline factory-owned parts (Strictly Naturally Aspirated!)
        if (ownedPartNames.Count == 0)
        {
            // Core Block & Internals
            UnlockPartFree("ForgeSteel Cast-Iron 2.0L");
            UnlockPartFree("ForgeSteel Cast Head");
            UnlockPartFree("Kinetic Street Springs");
            UnlockPartFree("ForgeSteel Cast Replacement");
            UnlockPartFree("AeroFlow Mild-Street");

            // Airflow (No Intercooler!)
            UnlockPartFree("AeroFlow OEM Upgrade");
            UnlockPartFree("AeroFlow 65mm Upgrade");

            // Exhaust System
            UnlockPartFree("AeroFlow Cast Replacement");
            UnlockPartFree("AeroFlow High-Flow Catted");
            UnlockPartFree("AeroFlow Sport Muffler");

            // Fuel & Ignition
            UnlockPartFree("Commercial 91 Octane");
            UnlockPartFree("Hydra Flow-150 Pump");
            UnlockPartFree("Hydra Flow-550");

            // NOTE: Turbo ("Stock-Spool T20"), Intercooler ("AeroFlow Side-Mount Core"), 
            // and EBC ("Chronos Manual Bleed Bleeder") are intentionally omitted.
        }
    }

    public bool IsPartOwned(string partName)
    {
        if (string.IsNullOrEmpty(partName)) return false;
        return ownedPartNames.Contains(partName.Trim());
    }

    /// <summary>
    /// Validates cash reserves and processes the purchase transaction.
    /// </summary>
    public bool TryPurchasePart(string partName, float cost)
    {
        if (IsPartOwned(partName)) return true;

        if (playerMoney >= cost)
        {
            playerMoney -= cost;
            ownedPartNames.Add(partName);

            // Fire events to announce state updates cleanly
            OnMoneyChanged?.Invoke(playerMoney);
            OnPartUnlocked?.Invoke(partName);

            Debug.Log($"[Economy] Successfully purchased: {partName} for ${cost}. Balance: ${playerMoney}");
            return true;
        }

        Debug.LogWarning($"[Economy] Transaction declined for: {partName}. Missing: ${cost - playerMoney}");
        return false;
    }

    public void UnlockPartFree(string partName)
    {
        if (IsPartOwned(partName)) return;
        ownedPartNames.Add(partName);
        OnPartUnlocked?.Invoke(partName);
    }

    /// <summary>
    /// Returns the active player inventory names list to the serializer.
    /// </summary>
    public List<string> GetOwnedPartNames()
    {
        return new List<string>(ownedPartNames);
    }

    /// <summary>
    /// Overwrites the runtime wallet and unlocks loaded inventories directly from the save payload.
    /// </summary>
    public void LoadSavedEconomyState(float cash, List<string> loadedParts)
    {
        playerMoney = cash;
        ownedPartNames = new List<string>(loadedParts);

        // Notify UI immediately to draw current bank assets
        OnMoneyChanged?.Invoke(playerMoney);
        Debug.Log($"[Economy] Successfully restored saved wallet balance: ${playerMoney:N0}");
    }
}