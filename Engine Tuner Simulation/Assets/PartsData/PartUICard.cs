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

    private EnginePartData targetData;

    /// <summary>
    /// Populates the UI fields dynamically using a raw parsed ScriptableObject asset asset wrapper.
    /// </summary>
    public void SetupCard(EnginePartData data)
    {
        targetData = data;
        nameText.text = data.partName;
        costText.text = $"${data.purchaseCost:N0}";
        iconImage.sprite = data.partIcon;

        // Clear out old buttons listeners and assign the purchase click action safely
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        Debug.Log($"Player selected or purchased: {targetData.partName}!");
        // This is where you will trigger your inventory check or bank account logic later!
    }
}