using UnityEngine;

[System.Serializable]
public class EngineProfile
{
    public string engineName;
    public int cylinderCount; // 4 or 6

    [Header("Base Factory Specs")]
    public float factoryMaxRPM = 6500f;
    public float baseMinKpa = 20f;
    public float baseMaxKpa = 115f;

    [Header("Installed Valvetrain modifications")]
    public bool hasUpgradedCylinderHeads = false;
    public bool hasUpgradedValveSprings = false; // ➔ NEW: Controls RPM ceiling expansions
    public bool hasStage2Camshafts = false;       // ➔ NEW: Controls volumetric efficiency scales

    [Header("Forced Induction")]
    public bool hasTurbochargerKit = false;

    /// <summary>
    /// Dynamically calculates the true maximum RPM threshold for the tuning grid.
    /// Valve springs and head work compounding allowances are tracked here.
    /// </summary>
    public float GetCalculatedMaxRPM()
    {
        float currentMax = factoryMaxRPM;

        // Factory head casting port cleanup
        if (hasUpgradedCylinderHeads)
        {
            currentMax += 300f;
        }

        // Heavy-duty dual valve springs completely prevent high-RPM valve float
        if (hasUpgradedValveSprings)
        {
            currentMax += 700f; // ➔ Unlocks a massive rev jump!
        }

        return currentMax;
    }

    /// <summary>
    /// Dynamically calculates the pressure ceiling based on forced induction parts status.
    /// </summary>
    public float GetCalculatedMaxKpa()
    {
        if (hasTurbochargerKit)
        {
            return 240f;
        }
        return baseMaxKpa;
    }
}