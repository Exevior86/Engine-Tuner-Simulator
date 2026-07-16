using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiagnosticChannelToggle : MonoBehaviour
{
    [Header("UI Component Bindings")]
    public Toggle toggle;
    public TMP_Text ToggleName;   // ➔ UPDATED to match your prefab name
    public Image ToggleColor;     // ➔ UPDATED to match your prefab name

    private string channelId;
    private DiagnosticsPanelManager panelManager;

    /// <summary>
    /// Called dynamically by DiagnosticsPanelManager to configure this toggle
    /// </summary>
    public void Setup(string id, string displayName, Color lineColor, bool isDefaultOn, DiagnosticsPanelManager manager)
    {
        channelId = id;
        panelManager = manager;

        // Set the visual text using your newly named component
        if (ToggleName != null)
        {
            ToggleName.text = displayName;
        }
        else
        {
            Debug.LogWarning($"[UI] ToggleName is not assigned on prefab: {gameObject.name}");
        }

        // Color-code your newly named color block
        if (ToggleColor != null)
        {
            ToggleColor.color = lineColor;
        }
        else
        {
            Debug.LogWarning($"[UI] ToggleColor is not assigned on prefab: {gameObject.name}");
        }

        // Set state and bind click event
        if (toggle != null)
        {
            toggle.isOn = isDefaultOn;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(OnToggleClicked);
        }
    }

    private void OnToggleClicked(bool isOn)
    {
        if (panelManager != null)
        {
            panelManager.OnToggleChangedFromButton(channelId, isOn);
        }
    }
}