using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;
using TMPro;

public class GraphHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
    [Header("UI Element References")]
    public RectTransform hoverLine;
    public RectTransform tooltipPopup;
    public TMP_Text timeText;             // Left at top for time/RPM header
    public TMP_Text primaryValueText;     // Will hold the dynamically stacked active values

    private RectTransform graphRect;
    private DataLogger logger;
    private DiagnosticsPanelManager panelManager;
    private bool isMouseOver = false;

    private void Awake()
    {
        graphRect = GetComponent<RectTransform>();
        logger = FindFirstObjectByType<DataLogger>();
        panelManager = FindFirstObjectByType<DiagnosticsPanelManager>();

        if (hoverLine != null) hoverLine.gameObject.SetActive(false);
        if (tooltipPopup != null) tooltipPopup.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (logger == null || logger.activeLog.Count == 0) return;
        isMouseOver = true;

        if (hoverLine != null) hoverLine.gameObject.SetActive(true);
        if (tooltipPopup != null) tooltipPopup.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;

        if (hoverLine != null) hoverLine.gameObject.SetActive(false);
        if (tooltipPopup != null) tooltipPopup.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateHoverState(eventData);
    }

    private void Update()
    {
        if (isMouseOver)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            UpdateHoverState(pointerData);
        }
    }

    private void UpdateHoverState(PointerEventData eventData)
    {
        if (logger == null || logger.activeLog.Count == 0) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(graphRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float width = graphRect.rect.width;
            float height = graphRect.rect.height;

            float leftAlignedX = localPoint.x + (width * graphRect.pivot.x);
            float leftAlignedY = localPoint.y + (height * graphRect.pivot.y);

            float clampedX = Mathf.Clamp(leftAlignedX, 0f, width);
            float clampedY = Mathf.Clamp(leftAlignedY, 0f, height);
            float normalizedX = clampedX / width;

            if (hoverLine != null)
            {
                hoverLine.anchorMin = new Vector2(0f, 0f);
                hoverLine.anchorMax = new Vector2(0f, 1f);
                hoverLine.pivot = new Vector2(0f, 0.5f);
                hoverLine.anchoredPosition = new Vector2(clampedX, 0f);
            }

            if (tooltipPopup != null)
            {
                tooltipPopup.anchorMin = Vector2.zero;
                tooltipPopup.anchorMax = Vector2.zero;
                tooltipPopup.pivot = new Vector2(0f, 0f);

                // --- X-AXIS BOUNDARY (FLIP LEFT/RIGHT) ---
                float tooltipX = clampedX + 15f;
                if (tooltipX > width - tooltipPopup.rect.width)
                {
                    tooltipX = clampedX - tooltipPopup.rect.width - 15f;
                }

                // --- Y-AXIS BOUNDARY (FLIP UP/DOWN) ---
                float tooltipY = clampedY + 15f;
                // ➔ NEW: If the tooltip would go off the top edge, flip it to the bottom of the cursor
                if (tooltipY > height - tooltipPopup.rect.height)
                {
                    tooltipY = clampedY - tooltipPopup.rect.height - 15f;
                }

                // Bulletproof clamp to make sure it never goes below the bottom edge of the graph container
                tooltipY = Mathf.Max(5f, tooltipY);

                tooltipPopup.anchoredPosition = new Vector2(tooltipX, tooltipY);
            }

            LogFrame closestFrame = GetClosestLogFrame(normalizedX);
            UpdateTooltipText(closestFrame);
        }
    }

    private LogFrame GetClosestLogFrame(float normalizedX)
    {
        int totalFrames = logger.activeLog.Count;
        float totalDuration = logger.activeLog[totalFrames - 1].timestamp;
        float targetTime = normalizedX * totalDuration;

        int low = 0;
        int high = totalFrames - 1;

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (logger.activeLog[mid].timestamp < targetTime)
                low = mid + 1;
            else
                high = mid;
        }

        return logger.activeLog[low];
    }

    private void UpdateTooltipText(LogFrame frame)
    {
        if (panelManager == null) return;

        // Header Text: Always show the timestamp
        if (timeText != null)
        {
            timeText.text = $"Time: {frame.timestamp:F2}s";
        }

        if (primaryValueText != null)
        {
            StringBuilder sb = new StringBuilder();

            // ➔ DYNAMIC READOUT: Check each channel and only append to string if active!
            foreach (var channel in panelManager.channels)
            {
                if (panelManager.IsChannelActive(channel.channelId))
                {
                    float rawValue = panelManager.GetChannelValue(frame, channel.channelId);
                    string hexColor = ColorUtility.ToHtmlStringRGB(channel.lineColor);

                    // Add a nice color-coded readout for this channel
                    sb.AppendLine($"<color=#{hexColor}>■</color> {channel.displayName}: {FormatValue(rawValue, channel.channelId)}");
                }
            }

            primaryValueText.text = sb.ToString().TrimEnd();
        }
    }

    // Small helper to keep decimals neat depending on what type of unit we are showing
    private string FormatValue(float value, string id)
    {
        return id switch
        {
            "rpm" => $"{value:F0}",
            "boost" => $"{value:F1} PSI",
            "targetBoost" => $"{value:F1} PSI",
            "airMass" => $"{value:F1} g/s",
            "ethanol" => $"{value:F0}%",
            "intakeTemp" => $"{value:F0}°F",
            "wastegate" => $"{value:F0}%",
            "knock" => $"{value:F1}°",
            _ => $"{value:F1}"
        };
    }
}