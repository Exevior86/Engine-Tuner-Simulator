using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;

public class PartsShopController : MonoBehaviour
{
    [Header("Infrastructure Config")]
    public Transform gridContainer;
    public GameObject partCardPrefab;

    [Header("UI Filter Config")]
    public TMP_Dropdown categoryDropdown;

    [Header("Economy Integration")]
    public EconomyManager economyManager; // ➔ Drag your EconomyManager GameObject here!
    public TMP_Text shopWalletText;       // ➔ Drag your "Cash: $5,000" text element here!

    // Master cache of all valid parts discovered on your hard drive
    private List<EnginePartData> masterCatalogItems = new List<EnginePartData>();

    // Tracks the current subfolder directory names available to filter by
    private List<string> availableCategories = new List<string>();

    private void Start()
    {
        // Subscribe to economy changes for dynamic UI refreshes
        if (economyManager != null)
        {
            economyManager.OnMoneyChanged += UpdateShopWalletUI;
            UpdateShopWalletUI(economyManager.CurrentMoney);
        }

        LoadAllCatalogParts();
        SetupCategoryDropdown();
        RefreshShopDisplay();
    }

    private void OnDestroy()
    {
        if (economyManager != null)
        {
            economyManager.OnMoneyChanged -= UpdateShopWalletUI;
        }
    }

    private void UpdateShopWalletUI(float currentCash)
    {
        if (shopWalletText != null)
        {
            shopWalletText.text = $"CASH: ${currentCash:N0}";
        }
    }

    /// <summary>
    /// Automatically scans the Resources directory to find and sort every imported part asset.
    /// </summary>
    private void LoadAllCatalogParts()
    {
        masterCatalogItems.Clear();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
#endif

        EnginePartData[] loadedParts = Resources.LoadAll<EnginePartData>("PartsCatalog");

        foreach (EnginePartData part in loadedParts)
        {
            if (part == null) continue;
            if (part.purchaseCost == 0 && string.IsNullOrEmpty(part.partName)) continue;

            masterCatalogItems.Add(part);
        }

        // ➔ THE SORT FIX: Group by folder Category first, then sort alphabetically by Name
        masterCatalogItems = masterCatalogItems
            .OrderBy(part => part.GetType().Name) // Groups FuelData together, InjectorData together, etc.
            .ThenBy(part => part.partName)         // Sorts alphabetically within those tight type groups
            .ToList();

        Debug.Log($"Shop Controller automatically discovered and sorted {masterCatalogItems.Count} valid parts from Resources!");
    }

    /// <summary>
    /// Parses the physical folder layout categories and links them to the UI dropdown selector options.
    /// </summary>
    private void SetupCategoryDropdown()
    {
        if (categoryDropdown == null)
        {
            Debug.LogError("[Shop Error] Please assign your category TMP_Dropdown component inside the Inspector panel!");
            return;
        }

        HashSet<string> uniqueCategories = new HashSet<string> { "All Categories" };

        foreach (EnginePartData part in masterCatalogItems)
        {
#if UNITY_EDITOR
            // Extract the subfolder name straight out of the physical directory path
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(part);
            if (!string.IsNullOrEmpty(assetPath))
            {
                string directoryName = Path.GetFileName(Path.GetDirectoryName(assetPath));
                if (directoryName != "PartsCatalog" && !string.IsNullOrEmpty(directoryName))
                {
                    uniqueCategories.Add(directoryName);
                }
            }
#endif
        }

        // Sort choices so "All Categories" is always index 0, followed by alphabetical categories
        availableCategories = uniqueCategories.OrderBy(c => c == "All Categories" ? 0 : 1).ThenBy(c => c).ToList();

        // Refresh dropdown options visually
        categoryDropdown.ClearOptions();
        categoryDropdown.AddOptions(availableCategories);

        // Bind code loop updates to dropdown click modifications
        categoryDropdown.onValueChanged.RemoveAllListeners();
        categoryDropdown.onValueChanged.AddListener(OnFilterDropdownChanged);
    }

    private void OnFilterDropdownChanged(int selectedIndex)
    {
        RefreshShopDisplay();
    }

    /// <summary>
    /// Cleans the viewport display grid and instantiates layout items matching selection criteria.
    /// </summary>
    public void RefreshShopDisplay()
    {
        // 1. Wipe current visual display container elements completely
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Identify active category target context string rules
        string selectedCategory = availableCategories[categoryDropdown.value];

        // 3. Filter our sorted master data cache using LINQ queries
        List<EnginePartData> filteredItems = masterCatalogItems;

        if (selectedCategory != "All Categories")
        {
            filteredItems = masterCatalogItems.Where(part =>
            {
#if UNITY_EDITOR
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(part);
                string folderName = Path.GetFileName(Path.GetDirectoryName(assetPath));
                return string.Equals(folderName, selectedCategory, System.StringComparison.OrdinalIgnoreCase);
#else
                return true;
#endif
            }).ToList();
        }

        // 4. Populate your grid container viewport matching only the clean subset
        foreach (EnginePartData part in filteredItems)
        {
            GameObject newCard = Instantiate(partCardPrefab, gridContainer);

            PartUICard cardScript = newCard.GetComponent<PartUICard>();

            if (cardScript != null && economyManager != null)
            {
                bool isOwned = economyManager.IsPartOwned(part.partName);
                Sprite dynamicIcon = GetPartIcon(part);

                // ➔ RESTORED: Calls SetupCard on PartUICard using the separate Shop-specific signature
                cardScript.SetupCard(
                    part,
                    dynamicIcon,
                    isOwned,
                    onBuyPressed: () =>
                    {
                        if (!isOwned)
                        {
                            if (economyManager.TryPurchasePart(part.partName, part.purchaseCost))
                            {
                                RefreshShopDisplay(); // Refresh the visual state to change "BUY" -> "OWNED"
                            }
                        }
                    }
                );
            }
        }
    }

    private string FormatShopStats(EnginePartData part)
    {
        return $"Cost: ${part.purchaseCost:F0}\nWeight: {part.componentWeight:F1} lbs";
    }

    /// <summary>
    /// Multi-asset atlas lookups moved to a standalone method block to preserve scannability.
    /// </summary>
    private Sprite GetPartIcon(EnginePartData part)
    {
        if (string.IsNullOrEmpty(part.imageFileName)) return null;

        if (part.imageFileName.Contains(":"))
        {
            string[] sheetTokens = part.imageFileName.Split(':');
            string masterSheetName = sheetTokens[0];
            string targetSubSpriteName = sheetTokens[1];

            string fullResourcePath = $"PartsCatalog/PartIcons/{masterSheetName}";
            Sprite[] totalSheetSprites = Resources.LoadAll<Sprite>(fullResourcePath);

            if (totalSheetSprites == null || totalSheetSprites.Length == 0)
            {
                Debug.LogError($"[🎨 Art Error] '{part.partName}' failed. Unity cannot find the sheet asset at: 'Assets/Resources/{fullResourcePath}'");
                return null;
            }

            foreach (Sprite subSprite in totalSheetSprites)
            {
                if (string.Equals(subSprite.name, targetSubSpriteName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return subSprite;
                }
            }

            Debug.LogError($"[🎨 Art Error] Found sheet '{masterSheetName}' ({totalSheetSprites.Length} slices), but none matched '{targetSubSpriteName}'.");
            return null;
        }
        else
        {
            return Resources.Load<Sprite>($"PartsCatalog/PartIcons/{part.imageFileName}");
        }
    }
}