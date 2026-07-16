using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiagnosticsPanelManager : MonoBehaviour
{
    [System.Serializable]
    public class ChannelConfig
    {
        public string channelId;
        public string displayName;
        public Color lineColor;
        public float minValue;
        public float maxValue;
        public bool isDefaultOn;
    }

    [Header("UI Containers")]
    public Transform toggleListContent;
    public RectTransform graphContainer;

    [Header("Prefabs")]
    public GameObject togglePrefab;
    public GameObject lineGraphPrefab;

    [Header("Channel Settings")]
    public List<ChannelConfig> channels = new List<ChannelConfig>();

    [Header("Y-Axis Dynamic Labels")]
    public RectTransform yAxisPanel;      // Drag your YAxisPanel container here
    public GameObject yAxisLabelPrefab;   // Drag a simple TextMeshPro text prefab here

    private DataLogger logger;
    private Dictionary<string, UILineGraph> activeRenderers = new Dictionary<string, UILineGraph>();
    private Dictionary<string, bool> channelStates = new Dictionary<string, bool>();

    private void Awake()
    {
        logger = FindFirstObjectByType<DataLogger>();
        InitializeChannelList();
    }

    private void InitializeChannelList()
    {
        if (channels.Count == 0)
        {
            channels.Add(new ChannelConfig { channelId = "rpm", displayName = "RPM (rpm)", lineColor = new Color(0.7f, 0.6f, 1.0f), minValue = 0f, maxValue = 8000f, isDefaultOn = true });
            channels.Add(new ChannelConfig { channelId = "boost", displayName = "Boost (psi)", lineColor = Color.red, minValue = -14.7f, maxValue = 30f, isDefaultOn = true });
            channels.Add(new ChannelConfig { channelId = "targetBoost", displayName = "Target Boost (psi)", lineColor = new Color(1f, 0.5f, 0f), minValue = -14.7f, maxValue = 30f, isDefaultOn = false });
            channels.Add(new ChannelConfig { channelId = "airMass", displayName = "Air Mass (g/s)", lineColor = Color.cyan, minValue = 0f, maxValue = 400f, isDefaultOn = false });
            channels.Add(new ChannelConfig { channelId = "ethanol", displayName = "Ethanol Conc (%)", lineColor = Color.yellow, minValue = 0f, maxValue = 100f, isDefaultOn = false });
            channels.Add(new ChannelConfig { channelId = "intakeTemp", displayName = "Intake Temp Manifold (°F)", lineColor = new Color(1f, 0.2f, 0.6f), minValue = 50f, maxValue = 200f, isDefaultOn = false });
            channels.Add(new ChannelConfig { channelId = "wastegate", displayName = "Wastegate Position (%)", lineColor = Color.gray, minValue = 0f, maxValue = 100f, isDefaultOn = false });
            channels.Add(new ChannelConfig { channelId = "knock", displayName = "Feedback Knock (°)", lineColor = Color.green, minValue = 0f, maxValue = -8f, isDefaultOn = true });
        }

        foreach (Transform child in toggleListContent) Destroy(child.gameObject);

        foreach (var config in channels)
        {
            GameObject toggleObj = Instantiate(togglePrefab, toggleListContent);
            DiagnosticChannelToggle toggleScript = toggleObj.GetComponent<DiagnosticChannelToggle>();

            if (toggleScript != null)
            {
                toggleScript.Setup(config.channelId, config.displayName, config.lineColor, config.isDefaultOn, this);
            }

            channelStates[config.channelId] = config.isDefaultOn;
        }

        UpdateAxisLabels();
    }

    public void OnToggleChangedFromButton(string channelId, bool isOn)
    {
        Debug.Log($"[UI Debug] Toggle Clicked: {channelId} is now {isOn}");
        channelStates[channelId] = isOn;
        RefreshGraphs();
    }

    public bool IsChannelActive(string channelId)
    {
        return channelStates.ContainsKey(channelId) && channelStates[channelId];
    }

    public void RefreshGraphs()
    {
        foreach (var config in channels)
        {
            bool isChannelActive = IsChannelActive(config.channelId);

            if (isChannelActive && logger != null && logger.activeLog.Count > 0)
            {
                if (!activeRenderers.TryGetValue(config.channelId, out UILineGraph graph))
                {
                    GameObject graphObj = Instantiate(lineGraphPrefab, graphContainer);
                    graphObj.name = $"GraphLine_{config.channelId}";

                    graph = graphObj.GetComponent<UILineGraph>();
                    graph.lineColor = config.lineColor;
                    graph.graphContainer = graphContainer;

                    activeRenderers[config.channelId] = graph;
                }

                List<Vector2> points = GenerateNormalizedPoints(config);
                graph.DrawGraphLine(points);
            }
            else
            {
                if (activeRenderers.TryGetValue(config.channelId, out UILineGraph graph))
                {
                    if (graph != null && graph.gameObject != null)
                    {
                        Destroy(graph.gameObject);
                    }
                    activeRenderers.Remove(config.channelId);
                }

                Transform leftoverLine = graphContainer.Find($"GraphLine_{config.channelId}");
                if (leftoverLine != null)
                {
                    Destroy(leftoverLine.gameObject);
                }
            }
        }

        UpdateAxisLabels();
    }

    private List<Vector2> GenerateNormalizedPoints(ChannelConfig config)
    {
        List<Vector2> normalizedPoints = new List<Vector2>();
        int totalFrames = logger.activeLog.Count;
        if (totalFrames < 2) return normalizedPoints;

        int maxDrawnPoints = 100;
        int step = Mathf.Max(1, totalFrames / maxDrawnPoints);

        float totalDuration = logger.activeLog[totalFrames - 1].timestamp;

        for (int i = 0; i < totalFrames; i += step)
        {
            LogFrame frame = logger.activeLog[i];
            float normX = totalDuration > 0f ? (frame.timestamp / totalDuration) : ((float)i / (totalFrames - 1));
            float rawValue = GetChannelValue(frame, config.channelId);
            float normY = Mathf.InverseLerp(config.minValue, config.maxValue, rawValue);

            normalizedPoints.Add(new Vector2(normX, normY));
        }

        if ((totalFrames - 1) % step != 0)
        {
            LogFrame finalFrame = logger.activeLog[totalFrames - 1];
            float rawValue = GetChannelValue(finalFrame, config.channelId);
            float normY = Mathf.InverseLerp(config.minValue, config.maxValue, rawValue);
            normalizedPoints.Add(new Vector2(1f, normY));
        }

        return normalizedPoints;
    }

    private void UpdateAxisLabels()
    {
        if (yAxisPanel == null || yAxisLabelPrefab == null) return;

        // Clear out old generated labels
        foreach (Transform child in yAxisPanel)
        {
            Destroy(child.gameObject);
        }

        // 1. Get current redline limit from engine simulation
        float maxRpm = 7000f; // Default baseline redline
        EcuController ecuController = FindFirstObjectByType<EcuController>();
        if (ecuController != null)
        {
            maxRpm = ecuController.activeRedline;
        }

        // 2. Generate labels in steps of 1000 RPM (e.g., 7000 RPM creates 8 labels: 7k down to 0)
        // We run the loop in reverse so the highest RPM stays at the top of the vertical layout group
        int stepSize = 1000;
        int maxSteps = Mathf.FloorToInt(maxRpm / stepSize);

        for (int i = maxSteps; i >= 0; i--)
        {
            float currentRpm = i * stepSize;

            // Instantiate label prefab inside our vertical layout panel
            GameObject labelObj = Instantiate(yAxisLabelPrefab, yAxisPanel);
            TMP_Text textComponent = labelObj.GetComponent<TMP_Text>();

            if (textComponent != null)
            {
                textComponent.text = $"{currentRpm:F0}";
            }
        }
    }

    public float GetChannelValue(LogFrame frame, string channelId)
    {
        return channelId switch
        {
            "rpm" => frame.rpm,
            "boost" => frame.boost,
            "targetBoost" => frame.targetBoost,
            "airMass" => frame.airMass,
            "ethanol" => frame.ethanolConcentration,
            "intakeTemp" => frame.manifoldIntakeTemp,
            "wastegate" => frame.wastegateDutyCycle,
            "knock" => frame.feedbackKnock,
            _ => 0f,
        };
    }
}