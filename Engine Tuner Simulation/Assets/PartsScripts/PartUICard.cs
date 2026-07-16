using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartUICard : MonoBehaviour
{
    [Header("UI Element Hooks")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Button purchaseButton;
    public TextMeshProUGUI buttonLabelText; // ➔ Optional: Drag your button's text child here!

    /// <summary>
    /// Populates the UI fields dynamically and binds the shop's purchase transaction logic.
    /// </summary>
    public void SetupCard(EnginePartData data, Sprite dynamicIcon, bool isOwned, System.Action onBuyPressed)
    {
        if (nameText != null) nameText.text = data.partName.ToUpper();
        if (costText != null) costText.text = $"${data.purchaseCost:N0}";

        // If a matching picture was found in your sprite sheet/folder, use it!
        if (iconImage != null)
        {
            if (dynamicIcon != null)
            {
                iconImage.sprite = dynamicIcon;
                iconImage.color = Color.white;
            }
            else
            {
                // Safety Fallback
                iconImage.sprite = null;
                iconImage.color = new Color(1, 1, 1, 0.1f);
                Debug.LogWarning($"Missing shop artwork for: {data.partName}. Check your sprite sheet mapping column!");
            }
        }

        // Configure the purchase button state based on ownership
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();

            if (isOwned)
            {
                purchaseButton.interactable = false; // Can't buy what you already own!

                if (buttonLabelText != null)
                    buttonLabelText.text = "OWNED";
                else if (purchaseButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                    purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = "OWNED";
            }
            else
            {
                purchaseButton.interactable = true;

                if (buttonLabelText != null)
                    buttonLabelText.text = "BUY";
                else if (purchaseButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                    purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = "BUY";

                purchaseButton.onClick.AddListener(() => onBuyPressed?.Invoke());
            }
        }
    }
}