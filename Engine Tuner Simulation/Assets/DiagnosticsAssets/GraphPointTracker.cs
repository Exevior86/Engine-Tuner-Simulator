using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class GraphPointerTracker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerMoveHandler
{
    [Header("UI References")]
    public RectTransform graphContainer;
    public RectTransform hoverLine;
    public RectTransform tooltipBox;

    [Header("Tooltip Texts")]
    public TMP_Text rpmText;
    public TMP_Text boostText;
    public TMP_Text knockText;
    public TMP_Text afrText;

    private DataLogger logger;
    private bool isPointerInside = false;

    private void Start()
    {
        logger = FindFirstObjectByType<DataLogger>();
        hoverLine.gameObject.SetActive(false);
        tooltipBox.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        hoverLine.gameObject.SetActive(true);
        tooltipBox.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        hoverLine.gameObject.SetActive(false);
        tooltipBox.gameObject.SetActive(false);
    }

    // Handles PC mouse hover movement
    public void OnPointerMove(PointerEventData eventData)
    {
        UpdateTracker(eventData.position);
    }

    // Handles mobile finger dragging / sliding
    public void OnDrag(PointerEventData eventData)
    {
        UpdateTracker(eventData.position);
    }

    private void UpdateTracker(Vector2 screenPosition)
    {
        if (logger == null || logger.activeLog.Count == 0) return;

        // Convert screen space coordinates to Graph Panel's Local coordinates
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(graphContainer, screenPosition, null, out Vector2 localPoint))
        {
            float graphWidth = graphContainer.rect.width;

            // Normalize X position to a 0.0 to 1.0 range
            float normalizedX = Mathf.Clamp01((localPoint.x + (graphWidth * 0.5f)) / graphWidth);

            // Find the closest index in our list of recorded frames
            int frameIndex = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (logger.activeLog.Count - 1)), 0, logger.activeLog.Count - 1);
            LogFrame selectedFrame = logger.activeLog[frameIndex];

            // 1. Move the Vertical line
            float targetX = (normalizedX * graphWidth) - (graphWidth * 0.5f);
            hoverLine.anchoredPosition = new Vector2(targetX, hoverLine.anchoredPosition.y);

            // 2. Position the Tooltip Panel (keep it slightly offset so it doesn't cover the vertical line)
            float tooltipOffset = (normalizedX > 0.5f) ? -150f : 150f; // Flip side if pointer is near right edge
            tooltipBox.anchoredPosition = new Vector2(targetX + tooltipOffset, tooltipBox.anchoredPosition.y);

            // 3. Populate values
            rpmText.text = $"RPM: <color=#b5a2ff>{selectedFrame.rpm:F0}</color>";
            boostText.text = $"Boost (psi): <color=#ff4d4d>{selectedFrame.boost:F2}</color>";
            knockText.text = $"Feedback Knock: <color=#ff9e9e>{selectedFrame.feedbackKnock:F1}</color>";
            afrText.text = $"AFR: <color=#4dff4d>{selectedFrame.airFuelRatio:F2}</color>";
        }
    }
}