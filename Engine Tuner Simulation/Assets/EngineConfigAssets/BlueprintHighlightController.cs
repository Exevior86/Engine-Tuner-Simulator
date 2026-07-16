using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintHighlightController : MonoBehaviour
{
    [System.Serializable]
    public class HotspotMapping
    {
        public string categoryKey;          // e.g., "TURBO", "ENGINE BLOCK"
        public Button button;               // The UI Button component
        public GameObject highlightOverlay; // The child "HighlightGlow" GameObject to toggle active
    }

    [Header("Hotspots Config")]
    public List<HotspotMapping> hotspots = new List<HotspotMapping>();

    private void Start()
    {
        // Start with ENGINE BLOCK highlighted by default to match our start state!
        SetActiveHighlight("ENGINE BLOCK");
    }

    /// <summary>
    /// Highlights the selected component and dims/hides all others.
    /// </summary>
    public void SetActiveHighlight(string categoryKey)
    {
        string targetKey = categoryKey.Trim().ToUpper();

        foreach (var hotspot in hotspots)
        {
            if (string.IsNullOrEmpty(hotspot.categoryKey)) continue;

            bool isSelected = (hotspot.categoryKey.Trim().ToUpper() == targetKey);

            // Toggle the red glow frame GameObject active/inactive
            if (hotspot.highlightOverlay != null)
            {
                hotspot.highlightOverlay.SetActive(isSelected);
            }
        }
    }
}