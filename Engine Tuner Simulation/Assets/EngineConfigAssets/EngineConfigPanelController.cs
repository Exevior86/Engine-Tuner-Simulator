using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EngineConfigPanelController : MonoBehaviour
{
    [Header("Dependencies")]
    public EngineConfigManager configManager;
    public EconomyManager economyManager;
    public Transform cardContainer; // ScrollRect Content
    public GameObject partCardPrefab;

    [Header("Header Elements")]
    public Image slotIcon;
    public TMP_Text slotTitleText;

    [Header("Currently Equipped Display")]
    public Image equippedPartImage;
    public TMP_Text equippedPartNameText;
    public TMP_Text equippedStatValueText; // Multiline text for stats

    [Header("Navigation Tabs")]
    public Button ownedPartsTabButton;
    public Button allPartsTabButton;
    private bool showOnlyOwned = true;

    [Header("Dropdown Filter Setup")]
    public TMP_Dropdown partTypeDropdown; // ➔ DRAG YOUR DROPDOWN GAMEOBJECT HERE!

    private string currentCategory = "ENGINE BLOCK";
    private List<string> currentDropdownTypes = new List<string>();

    private void Start()
    {
        // Setup Tab listeners
        if (ownedPartsTabButton != null) ownedPartsTabButton.onClick.AddListener(() => SetTabMode(true));
        if (allPartsTabButton != null) allPartsTabButton.onClick.AddListener(() => SetTabMode(false));

        // Safely load the default category using our simple wrapper
        OpenCategorySimple("ENGINE BLOCK");
    }

    public void OpenCategory(string dataTypeName, string displayName, Sprite categoryIcon)
    {
        currentCategory = dataTypeName;

        if (slotTitleText != null) slotTitleText.text = displayName;

        if (slotIcon != null)
        {
            if (categoryIcon != null)
            {
                slotIcon.sprite = categoryIcon;
                slotIcon.color = Color.white;
                slotIcon.gameObject.SetActive(true);
            }
            else
            {
                slotIcon.color = new Color(1, 1, 1, 0.1f);
            }
        }

        // Dynamically rebuild the dropdown options for this brand-new panel category context!
        SetupDropdownFilters();

        RefreshPanel();
    }

    private void SetTabMode(bool ownedOnly)
    {
        showOnlyOwned = ownedOnly;
        RefreshPanel();
    }

    public void RefreshPanel()
    {
        UpdateEquippedPreview();
        PopulateList();
    }

    /// <summary>
    /// Populates dropdown options with specific structural C# classes linked to the visual category.
    /// </summary>
    private void SetupDropdownFilters()
    {
        if (partTypeDropdown == null) return;

        partTypeDropdown.onValueChanged.RemoveAllListeners();

        // 1. Gather class types mapped to the current panel (e.g. EngineBlockData, CamshaftData, etc.)
        List<string> classTypes = GetClassTypesForCategory(currentCategory);

        currentDropdownTypes.Clear();
        currentDropdownTypes.Add("ALL COMPONENTS"); // Dynamic fallback index 0

        foreach (string rawTypeName in classTypes)
        {
            // Turn ugly class names like "CylinderHeadData" into elegant labels like "CYLINDER HEAD"
            string cleanName = rawTypeName.Replace("Data", "").ToUpper();
            currentDropdownTypes.Add(cleanName);
        }

        // 2. Build the visual TMP dropdown options
        partTypeDropdown.ClearOptions();
        partTypeDropdown.AddOptions(currentDropdownTypes);

        // 3. Set to default "ALL COMPONENTS" and bind event listener
        partTypeDropdown.value = 0;
        partTypeDropdown.onValueChanged.AddListener(OnDropdownFilterChanged);
    }

    private void OnDropdownFilterChanged(int selectedIndex)
    {
        // Refresh when selecting a sub-component class
        RefreshPanel();
    }

    /// <summary>
    /// Updates the top preview displaying what is currently installed.
    /// </summary>
    private void UpdateEquippedPreview()
    {
        if (configManager == null) return;

        List<string> targetClassTypes = GetClassTypesForCategory(currentCategory);
        EnginePartData equippedPart = configManager.equippedParts.Find(p =>
            targetClassTypes.Contains(p.GetType().Name)
        );

        if (equippedPart != null)
        {
            if (equippedPartNameText != null) equippedPartNameText.text = equippedPart.partName.ToUpper();
            if (equippedStatValueText != null) equippedStatValueText.text = FormatPartStats(equippedPart);

            if (equippedPartImage != null)
            {
                equippedPartImage.sprite = GetEquippedPartIcon(equippedPart);
                equippedPartImage.color = Color.white;
                equippedPartImage.gameObject.SetActive(true);
            }
        }
        else
        {
            if (equippedPartNameText != null) equippedPartNameText.text = "NO PART EQUIPPED";
            if (equippedStatValueText != null) equippedStatValueText.text = "";
            if (equippedPartImage != null) equippedPartImage.gameObject.SetActive(false);
        }
    }

    private Sprite GetEquippedPartIcon(EnginePartData part)
    {
        if (string.IsNullOrEmpty(part.imageFileName)) return null;

        if (part.imageFileName.Contains(":"))
        {
            string[] sheetTokens = part.imageFileName.Split(':');
            string masterSheetName = sheetTokens[0];
            string targetSubSpriteName = sheetTokens[1];

            string fullResourcePath = $"PartsCatalog/PartIcons/{masterSheetName}";
            Sprite[] totalSheetSprites = Resources.LoadAll<Sprite>(fullResourcePath);

            if (totalSheetSprites != null)
            {
                foreach (Sprite subSprite in totalSheetSprites)
                {
                    if (string.Equals(subSprite.name, targetSubSpriteName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return subSprite;
                    }
                }
            }
            return null;
        }
        else
        {
            return Resources.Load<Sprite>($"PartsCatalog/PartIcons/{part.imageFileName}");
        }
    }

    /// <summary>
    /// Spawns and configures the list of items inside the scroll area.
    /// </summary>
    private void PopulateList()
    {
        if (cardContainer == null) return;

        // 1. CLEANLY CLEAR ALL PREVIOUSLY SPAWNED CARDS
        for (int i = cardContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = cardContainer.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        if (configManager == null) return;

        // 2. Determine target filtering class types based on current category AND dropdown selection
        List<string> targetClassTypes = new List<string>();

        if (partTypeDropdown == null || partTypeDropdown.value == 0)
        {
            // Show all subclass types within this category
            targetClassTypes = GetClassTypesForCategory(currentCategory);
        }
        else
        {
            // Keep ONLY the specific subclass chosen in the dropdown (convert clean label back to class string name)
            string chosenCleanLabel = currentDropdownTypes[partTypeDropdown.value];

            // Map clean uppercase labels back to physical database class names
            List<string> rawClassNames = GetClassTypesForCategory(currentCategory);
            string matchingRawClass = rawClassNames.Find(raw => raw.Replace("Data", "").ToUpper() == chosenCleanLabel);

            if (!string.IsNullOrEmpty(matchingRawClass))
            {
                targetClassTypes.Add(matchingRawClass);
            }
            else
            {
                // Fallback
                targetClassTypes = GetClassTypesForCategory(currentCategory);
            }
        }

        // 3. Gather all available parts that match any of our target class types
        List<EnginePartData> displayedParts = configManager.availableParts.FindAll(part =>
            targetClassTypes.Contains(part.GetType().Name)
        );

        // 4. Filter by player-owned inventory
        if (showOnlyOwned && economyManager != null)
        {
            displayedParts = displayedParts.FindAll(part => economyManager.IsPartOwned(part.partName));
        }

        // 5. Render each part as a high-tech row card
        foreach (EnginePartData part in displayedParts)
        {
            GameObject cardObj = Instantiate(partCardPrefab, cardContainer);
            InventoryPartCard cardUI = cardObj.GetComponent<InventoryPartCard>();

            if (cardUI != null)
            {
                bool isCurrentlyEquipped = configManager.IsPartEquipped(part.partName);
                bool isCurrentlyOwned = economyManager != null && economyManager.IsPartOwned(part.partName);

                // Pass all 4 parameters sequentially (exactly matching the new script):
                // 1. part (EnginePartData)
                // 2. FormatPartStats(part) (string)
                // 3. isCurrentlyEquipped (bool)
                // 4. Action (System.Action)
                cardUI.SetupCard(
                    part,
                    FormatPartStats(part),
                    isCurrentlyEquipped,
                    () =>
                    {
                        if (isCurrentlyOwned)
                        {
                            configManager.EquipPartByName(part.partName);
                            RefreshPanel();
                        }
                    }
                );
            }
        }
    }

    private List<string> GetClassTypesForCategory(string category)
    {
        List<string> targetClassTypes = new List<string>();

        // Force uppercase and trim to prevent any and all typing mismatches!
        string normalized = category.Trim().ToUpper();

        switch (normalized)
        {
            case "INTAKE":
                targetClassTypes.Add("IntakeManifoldData");
                targetClassTypes.Add("ThrottleBodyData");
                break;

            case "INTERCOOLER":
                targetClassTypes.Add("IntercoolerData");
                break;

            case "COOLING":
                targetClassTypes.Add("FuelData");
                break;

            case "EXHAUST":
                targetClassTypes.Add("CatBackExhaustData");
                targetClassTypes.Add("DownpipeData");
                targetClassTypes.Add("ExhaustHeadersData");
                break;

            case "TURBO":
                targetClassTypes.Add("TurboData");
                break;

            case "ENGINE BLOCK":
                targetClassTypes.Add("EngineBlockData");
                targetClassTypes.Add("CylinderHeadData");
                targetClassTypes.Add("PistonRodsData");
                targetClassTypes.Add("CamshaftData");
                targetClassTypes.Add("ValvetrainData");
                break;

            case "FUEL SYSTEM":
                targetClassTypes.Add("FuelPumpData");
                targetClassTypes.Add("InjectorData");
                break;

            case "ECU":
                targetClassTypes.Add("BoostControllerData");
                targetClassTypes.Add("FlexFuelSensorData");
                break;

            default:
                targetClassTypes.Add(category);
                break;
        }

        return targetClassTypes;
    }

    /// <summary>
    /// Translates specialized part data parameters into clean engineering specs.
    /// </summary>
    private string FormatPartStats(EnginePartData part)
    {
        if (part is TurboData turbo)
        {
            return $"Compressor Flow: {turbo.compressorFlowLbMin:F0} lb/min\n" +
                   $"Full Spool: {turbo.spoolThresholdRpm:F0} RPM\n" +
                   $"Boost Limit: +{turbo.maxBoostPsi:F0} PSI";
        }
        else if (part is EngineBlockData block)
        {
            return $"Displacement: {block.displacementLiters:F1}L\n" +
                   $"Cylinders: {block.cylinderCount}\n" +
                   $"Max Torque: {block.maximumTorqueLimit:F0} Nm";
        }
        else if (part is CamshaftData cam)
        {
            return $"Valve Lift: {cam.valveLiftMm:F1} mm\n" +
                   $"Duration: {cam.advertisedDuration:F0}°\n" +
                   $"VE Shift: {cam.peakVEShiftRpm:F0} RPM";
        }
        else if (part is CylinderHeadData head)
        {
            return $"Valve Dia: {head.valveDiameterMm:F1} mm\n" +
                   $"Valves/Cyl: {head.valvesPerCylinder}\n" +
                   $"Cooling: +{head.combustionCoolingFactor * 100f:F0}%";
        }
        else if (part is ValvetrainData valvetrain)
        {
            return $"Safe Redline: {valvetrain.safeRedlineLimit:F0} RPM\n" +
                   $"VE Flow Gain: +{valvetrain.valveFlowIncrease * 100f:F0}%";
        }
        else if (part is DownpipeData dp)
        {
            return $"Diameter: {dp.pipeDiameterInches:F1}\"\n" +
                   $"Catted: {(dp.isCatted ? "Yes" : "No")}\n" +
                   $"Flow Increase: +{dp.restrictionReduction * 100f:F0}%";
        }
        else if (part is ExhaustHeadersData headers)
        {
            return $"Runner Dia: {headers.primaryRunnerDiameterInches:F2}\"\n" +
                   $"Type: {(headers.isLongTubeHeader ? "Long-Tube" : "Shorty")}\n" +
                   $"Restriction Drop: -{headers.restrictionReduction * 100f:F0}%";
        }
        else if (part is CatBackExhaustData ex)
        {
            return $"Diameter: {ex.pipeDiameterInches:F1}\"\n" +
                   $"Flow Increase: +{ex.restrictionReduction * 100f:F0}%";
        }
        else if (part is FuelPumpData pump)
        {
            return $"Flow Rate: {pump.flowRateLph:F0} LPH\n" +
                   $"Max Pressure: {pump.maxOperatingPressurePsi:F0} PSI";
        }
        else if (part is InjectorData injector)
        {
            return $"Flow Rate: {injector.flowRateCcMin:F0} cc/min\n" +
                   $"Atomization: {injector.atomizationFactor * 100f:F0}%";
        }
        else if (part is FuelData fuel)
        {
            return $"Octane: {fuel.octaneRating:F0} Oct\n" +
                   $"Stoich AFR: {fuel.stoichiometricAFR:F1}:1\n" +
                   $"Cooling: +{fuel.latentHeatEvaporationVal * 100f:F0}%";
        }
        else if (part is IntakeManifoldData intake)
        {
            return $"Plenum Vol: {intake.plenumVolumeLiters:F1}L\n" +
                   $"Flow Capacity: {intake.maxAirflowCapacity:F0} g/s";
        }
        else if (part is IntercoolerData ic)
        {
            return $"Heat Dissip: {ic.heatDissipationCapacityWatts:F0} W\n" +
                   $"Flow Efficiency: {ic.flowEfficiencyFactor * 100f:F0}%";
        }
        else if (part is PistonRodsData pistons)
        {
            return $"Comp Offset: {pistons.compressionOffset:F1}\n" +
                   $"Strength Rating: {pistons.materialStrengthRating}";
        }
        else if (part is ThrottleBodyData tb)
        {
            return $"Bore Size: {tb.throttleDiameterMm:F0} mm\n" +
                   $"Response Multiplier: {tb.responseScaleMultiplier:F1}x";
        }

        return $"Cost: ${part.purchaseCost:F0}\n" +
               $"Weight: {part.componentWeight:F1} lbs";
    }

    public void OpenCategorySimple(string visualCategoryName)
    {
        string normalizedCategory = visualCategoryName.Trim().ToUpper();
        string displayName = normalizedCategory;
        string iconFileName = "";

        switch (normalizedCategory)
        {
            case "INTAKE":
                displayName = "INTAKE SYSTEMS";
                iconFileName = "intake_icon";
                break;
            case "INTERCOOLER":
                displayName = "INTERCOOLER CORE";
                iconFileName = "intercooler_icon";
                break;
            case "COOLING":
                displayName = "THERMAL COOLING & FUEL";
                iconFileName = "cooling_icon";
                break;
            case "EXHAUST":
                displayName = "EXHAUST SYSTEM";
                iconFileName = "exhaust_icon";
                break;
            case "TURBO":
                displayName = "TURBOCHARGER";
                iconFileName = "turbo_icon";
                break;
            case "ENGINE BLOCK":
                displayName = "CORE ROTATING ASSEMBLY";
                iconFileName = "engine_block_icon";
                break;
            case "FUEL SYSTEM":
                displayName = "FUEL DELIVERY";
                iconFileName = "fuel_system_icon";
                break;
            case "ECU":
                displayName = "ENGINE MANAGEMENT (ECU)";
                iconFileName = "ecu_icon";
                break;
        }

        Sprite categorySprite = null;
        if (!string.IsNullOrEmpty(iconFileName))
        {
            categorySprite = Resources.Load<Sprite>($"PartsCatalog/PartIcons/{iconFileName}");
        }

        if (categorySprite == null)
        {
            List<string> targetClassTypes = GetClassTypesForCategory(normalizedCategory);
            EnginePartData equippedPart = configManager.equippedParts.Find(p =>
                targetClassTypes.Contains(p.GetType().Name)
            );
            if (equippedPart != null)
            {
                categorySprite = GetEquippedPartIcon(equippedPart);
            }
        }

        OpenCategory(normalizedCategory, displayName, categorySprite);
    }
}