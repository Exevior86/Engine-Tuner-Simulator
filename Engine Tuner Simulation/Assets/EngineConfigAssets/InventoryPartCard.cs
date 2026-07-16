using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPartCard : MonoBehaviour
{
    [Header("Visual Fields")]
    public TMP_Text partNameText;
    public TMP_Text partStatsText;
    public Image partIcon; // Assigned to your Card's Image component!

    [Header("Action Controls")]
    public Button equipButton;
    public TMP_Text buttonLabelText; // Reference to the TEXT child of your Equip Button!
    public GameObject equippedLabel;
    public GameObject equippedCheckmark;

    /// <summary>
    /// Populates owned parts in the Engine Config Panel with clear installation state triggers.
    /// </summary>
    public void SetupCard(EnginePartData part, string formattedStats, bool isEquipped, System.Action onEquipPressed)
    {
        if (partNameText != null) partNameText.text = part.partName.ToUpper();
        if (partStatsText != null) partStatsText.text = formattedStats;

        // --- GET SPRITE USING THE SHOP'S BULLETPROOF LOGIC ---
        if (partIcon != null)
        {
            Sprite loadedSprite = GetPartIcon(part);

            if (loadedSprite != null)
            {
                partIcon.sprite = loadedSprite;
                partIcon.color = Color.white; // Ensure it's fully opaque/visible
            }
            else
            {
                // Fallback style if no image exists yet
                partIcon.sprite = null;
                partIcon.color = new Color(1, 1, 1, 0.1f); // Semi-transparent blank
            }
        }

        // Toggle visual state based on whether this item is currently installed on the engine
        if (isEquipped)
        {
            if (equipButton != null) equipButton.gameObject.SetActive(false);
            if (equippedLabel != null) equippedLabel.SetActive(true);
            if (equippedCheckmark != null) equippedCheckmark.SetActive(true);
        }
        else
        {
            if (equipButton != null) equipButton.gameObject.SetActive(true);
            if (equippedLabel != null) equippedLabel.SetActive(false);
            if (equippedCheckmark != null) equippedCheckmark.SetActive(false);

            if (buttonLabelText != null)
            {
                buttonLabelText.text = "EQUIP";
            }

            if (equipButton != null)
            {
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(() => onEquipPressed?.Invoke());
            }
        }
    }

    /// <summary>
    /// Multi-asset atlas lookups to cleanly parse sprite sheet slices or fallback graphics.
    /// </summary>
    private Sprite GetPartIcon(EnginePartData part)
    {
        if (string.IsNullOrEmpty(part.imageFileName)) return null;

        // Check if this part points to a combined sprite sheet layout mapping split token
        if (part.imageFileName.Contains(":"))
        {
            string[] sheetTokens = part.imageFileName.Split(':');
            string masterSheetName = sheetTokens[0];     // e.g. "camshaftsSheet"
            string targetSubSpriteName = sheetTokens[1]; // e.g. "camshaftsSheet_0"

            string fullResourcePath = $"PartsCatalog/PartIcons/{masterSheetName}";

            // Load all sliced sub-sprites inside that master file boundary
            Sprite[] totalSheetSprites = Resources.LoadAll<Sprite>(fullResourcePath);

            if (totalSheetSprites == null || totalSheetSprites.Length == 0)
            {
                Debug.LogError($"[🎨 Art Error] '{part.partName}' failed. Unity cannot find the sheet asset at: 'Assets/Resources/{fullResourcePath}'");
                return null;
            }

            // Clean, single-pass case-insensitive verification loop
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
            // Fallback strategy for individual standalone image asset files
            return Resources.Load<Sprite>($"PartsCatalog/PartIcons/{part.imageFileName}");
        }
    }
}