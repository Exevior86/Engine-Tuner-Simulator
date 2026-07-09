using UnityEngine;

public class TuningBayPanelManager : MonoBehaviour
{
    [Header("UI Panel GameObjects")]
    public GameObject objectivesPanel;
    public GameObject partsPanel;
    public GameObject engineConfigPanel;
    public GameObject fuelMapPanel;
    public GameObject ignitionTimingPanel;
    public GameObject diagnosticsPanel;

    private void Start()
    {
        // Start fresh with all panels closed when entering the bay
        HideAllPanels();
    }

    /// <summary>
    /// Instantly deactivates every panel in the canvas matrix.
    /// </summary>
    public void HideAllPanels()
    {
        if (objectivesPanel != null) objectivesPanel.SetActive(false);
        if (partsPanel != null) partsPanel.SetActive(false);
        if (engineConfigPanel != null) engineConfigPanel.SetActive(false);
        if (fuelMapPanel != null) fuelMapPanel.SetActive(false);
        if (ignitionTimingPanel != null) ignitionTimingPanel.SetActive(false);
        if (diagnosticsPanel != null) diagnosticsPanel.SetActive(false);
    }

    /// <summary>
    /// Helper method that handles the toggle or swap calculation for any target panel.
    /// </summary>
    private void TogglePanel(GameObject targetPanel)
    {
        if (targetPanel == null) return;

        // Check if the panel is already active in the scene
        if (targetPanel.activeSelf)
        {
            // Clicked a second time ➔ Close it
            targetPanel.SetActive(false);
            Debug.Log($"{targetPanel.name} closed via secondary click.");
        }
        else
        {
            // First click ➔ Clear the screen and open it
            HideAllPanels();
            targetPanel.SetActive(true);
            Debug.Log($"{targetPanel.name} opened.");
        }
    }

    // --- Sidebar Button Link Targets ---

    public void ShowObjectives()
    {
        TogglePanel(objectivesPanel);
    }

    public void ShowParts()
    {
        TogglePanel(partsPanel);
    }

    public void ShowEngineConfig()
    {
        TogglePanel(engineConfigPanel);
    }

    public void ShowFuelMap()
    {
        TogglePanel(fuelMapPanel);
    }

    public void ShowIgnitionTiming()
    {
        TogglePanel(ignitionTimingPanel);
    }

    public void ShowDiagnostics()
    {
        TogglePanel(diagnosticsPanel);
    }
}