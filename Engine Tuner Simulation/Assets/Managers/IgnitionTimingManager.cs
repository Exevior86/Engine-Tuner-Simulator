using UnityEngine;
using TMPro;

public class IgnitionTimingManager : MonoBehaviour
{
    [Header("UI Axis Header Containers")]
    public Transform xAxisHeaderContainer; // Horizontal Layout Group (IT-X-AxisContainer)
    public Transform yAxisSidebarContainer; // Vertical Layout Group (IT-Y-AxisContainer)
    public Transform gridContainer;         // Content (Grid Layout Group) (ITContent)

    [Header("UI Prefabs")]
    public GameObject cellInputFieldPrefab; // Prefab with TMP_InputField
    public GameObject axisLabelPrefab;      // Prefab with plain TMP_Text

    [Header("Table Grid Dimensions")]
    public int rpmBins = 10;
    public int loadBins = 10;

    private float minRPM = 1000f;

    /// <summary>
    /// Master function to draw and sync the interactive ignition matrix UI.
    /// </summary>
    public void InitializeIgnitionMapUI(EngineData engine)
    {
        Debug.Log("[DEBUG LOOP CHECK] InitializeIgnitionMapUI called! before method");
        if (engine == null)
        {
            Debug.LogError("[Ignition Manager] Cannot build table, EngineData is null!");
            return;
        }

        // 1. Clear out any old visual elements
        ClearActiveLayouts();

        // 2. Fetch the dynamic limits from the active engine build specs directly (no scene scanning)
        float maxRPM = CalculateActiveRedline(engine);
        float maxKpa = GetMaxMapPressure(engine);
        float minKpa = engine.baseMinKpa;

        // 3. Generate perimeter text labels (Matched to: X = kPa, Y = RPM)
        GenerateAxisLabels(maxRPM, minKpa, maxKpa);

        // 4. Populate or verify the data array matrix
        InitializeIgnitionTableData(engine);

        // 5. Spawn the interactive clickable input cells
        PopulateGridCells(engine);
    }

    private void ClearActiveLayouts()
    {
        foreach (Transform child in xAxisHeaderContainer) Destroy(child.gameObject);
        foreach (Transform child in yAxisSidebarContainer) Destroy(child.gameObject);
        foreach (Transform child in gridContainer) Destroy(child.gameObject);
    }

    private void GenerateAxisLabels(float maxRPM, float minKpa, float maxKpa)
    {
        Debug.Log("[DEBUG LOOP CHECK] GenerateAxisLabels called!");
        // 1. New X-Axis (Engine Load kPa) -> Left to Right (Low to High)
        float kpaStep = (maxKpa - minKpa) / (rpmBins - 1);
        for (int col = 0; col < rpmBins; col++)
        {
            float currentKpa = minKpa + (kpaStep * col);
            GameObject labelObj = Instantiate(axisLabelPrefab, xAxisHeaderContainer);
            int roundedKpa = Mathf.RoundToInt(currentKpa);
            labelObj.GetComponent<TMP_Text>().text = $"{roundedKpa}";
        }

        // 2. New Y-Axis (RPM) -> Bottom to Top (Low to High)
        float rpmStep = (maxRPM - minRPM) / (loadBins - 1);
        for (int row = loadBins - 1; row >= 0; row--)
        {
            float currentRPM = minRPM + (rpmStep * row);
            GameObject labelObj = Instantiate(axisLabelPrefab, yAxisSidebarContainer);
            int roundedRPM = Mathf.RoundToInt(currentRPM / 100f) * 100;
            labelObj.GetComponent<TMP_Text>().text = roundedRPM.ToString();
        }
    }

    private void InitializeIgnitionTableData(EngineData engine)
    {
        // 1. Scan the active data array to see if it's completely empty or full of zeros
        bool isTableAllZeros = true;
        if (engine.savedIgnitionTableData != null && engine.savedIgnitionTableData.Length == (loadBins * rpmBins))
        {
            for (int i = 0; i < engine.savedIgnitionTableData.Length; i++)
            {
                if (engine.savedIgnitionTableData[i] != 0f)
                {
                    isTableAllZeros = false;
                    break;
                }
            }
        }

        // 2. If it is null, wrong size, or completely filled with 0.0, force-generate our baseline
        if (engine.savedIgnitionTableData == null || engine.savedIgnitionTableData.Length != (loadBins * rpmBins) || isTableAllZeros)
        {
            engine.savedIgnitionTableData = new float[loadBins * rpmBins];

            for (int row = 0; row < loadBins; row++) // Y-Axis: RPM (0 = 1000 RPM, 9 = Redline)
            {
                float rpmFactor = (float)row / (loadBins - 1);

                for (int col = 0; col < rpmBins; col++) // X-Axis: kPa (0 = 20 kPa, 9 = Max kPa)
                {
                    float loadFactor = (float)col / (rpmBins - 1);

                    // --- DELIBERATELY BAD / UNOPTIMIZED BASELINE TIMING ---
                    // We start with a lazy, sluggish 12.0° BTDC at idle
                    float baseTiming = 12f;

                    // We aggressively advance timing up to +22.0° as RPM climbs (too fast for a stock valvetrain)
                    float rpmAdvance = rpmFactor * 22f;

                    // We fail to retard the timing enough under load (only pull -8.0° instead of a safe -14.0° to -16.0°)
                    float loadRetard = loadFactor * 8f;

                    float dynamicTiming = baseTiming + rpmAdvance - loadRetard;

                    // --- THE KNOCK TRIGGER DEFECT ---
                    // In the upper-right area (High Load + Mid-High RPM), we overshoot the advance by +8.0°.
                    // This creates a massive spike in combustion pressure before the piston reaches TDC.
                    // When the player climbs onto the dyno, this defect will immediately trigger engine knock!
                    if (loadFactor > 0.5f && rpmFactor > 0.4f)
                    {
                        dynamicTiming += 8.0f;
                    }

                    // Clamp to realistic physical tuning boundaries
                    dynamicTiming = Mathf.Clamp(dynamicTiming, -10f, 45f);

                    int index = (row * rpmBins) + col;
                    engine.savedIgnitionTableData[index] = Mathf.Round(dynamicTiming * 10f) / 10f;
                }
            }
            Debug.Log("[Ignition Manager] A deliberately unoptimized timing baseline has been generated!");
        }
    }

    private void PopulateGridCells(EngineData engine)
    {
        Debug.Log("[DEBUG LOOP CHECK] populateGridCells called!");
        for (int row = loadBins - 1; row >= 0; row--)
        {
            for (int col = 0; col < rpmBins; col++)
            {
                GameObject newCell = Instantiate(cellInputFieldPrefab, gridContainer);
                TMP_InputField inputField = newCell.GetComponent<TMP_InputField>();

                int targetIndex = (row * rpmBins) + col;
                float cellValue = engine.savedIgnitionTableData[targetIndex];

                inputField.text = cellValue.ToString("F1");

                // ➔ ADD THIS: Apply the initial heat map color to the cell background
                ApplyCellColor(inputField, cellValue);

                int runningIndex = targetIndex;
                inputField.onEndEdit.AddListener((newValue) =>
                {
                    OnCellDataUpdated(engine, runningIndex, newValue, inputField);
                });
            }
        }
    }

    private void OnCellDataUpdated(EngineData engine, int index, string value, TMP_InputField field)
    {
        Debug.Log("[DEBUG LOOP CHECK] OnCellDataUpdated called!");
        if (float.TryParse(value, out float parsedValue))
        {
            engine.savedIgnitionTableData[index] = Mathf.Clamp(parsedValue, -10f, 50f);
            Debug.Log($"[ECU Sync] Ignition array index [{index}] manually updated to: {engine.savedIgnitionTableData[index]}° BTDC");
        }

        float finalValue = engine.savedIgnitionTableData[index];
        field.text = finalValue.ToString("F1");

        // ➔ ADD THIS: Update the color in real-time when the player edits a cell!
        ApplyCellColor(field, finalValue);
    }

    /// <summary>
    /// Blends the cell background color from green (low timing) to red (high timing advance).
    /// </summary>
    private void ApplyCellColor(TMP_InputField field, float value)
    {
        // Get the target image component of the InputField's background
        UnityEngine.UI.Image bgImage = field.GetComponent<UnityEngine.UI.Image>();
        if (bgImage == null) bgImage = field.placeholder.GetComponentInParent<UnityEngine.UI.Image>();

        if (bgImage != null)
        {
            // Normalize our timing range (-10 to 45) to a 0.0 to 1.0 scale
            float minLimit = -10f;
            float maxLimit = 45f;
            float t = Mathf.InverseLerp(minLimit, maxLimit, value);

            // Define our gradient endpoints
            Color lowColor = new Color(0.12f, 0.53f, 0.12f, 1f);  // Clean, dark forest green for low advance
            Color highColor = new Color(0.78f, 0.15f, 0.15f, 1f); // Professional, rich red for high advance

            // Set the background color based on the value
            bgImage.color = Color.Lerp(lowColor, highColor, t);
        }
    }

    // --- SELF-SUFFICIENT SPEC CALCULATORS ---

    public float CalculateActiveRedline(EngineData engine)
    {
        float redline = engine.factoryMaxRPM;
        if (engine.installedHead != null) redline += 300f;
        if (engine.installedValvetrain != null) redline += 700f;
        return redline;
    }

    public float GetMaxMapPressure(EngineData engine)
    {
        if (engine.installedTurbo != null || engine.installedSupercharger != null)
        {
            return 240f;
        }
        return engine.baseMaxKpa;
    }
}