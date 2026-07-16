using UnityEngine;
using TMPro;

public class FuelMappingManager : MonoBehaviour
{
    [Header("UI Axis Header Containers")]
    public Transform xAxisHeaderContainer; // Horizontal Layout Group
    public Transform yAxisSidebarContainer; // Vertical Layout Group
    public Transform gridContainer;         // Content (Grid Layout Group)

    [Header("UI Prefabs")]
    public GameObject cellInputFieldPrefab; // Prefab with TMP_InputField
    public GameObject axisLabelPrefab;      // Prefab with plain TMP_Text

    [Header("Table Grid Dimensions")]
    public int rpmBins = 10;
    public int loadBins = 10;

    private float minRPM = 1000f;

    /// <summary>
    /// Master function to draw and sync the interactive fuel matrix UI.
    /// </summary>
    public void InitializeFuelMapUI(EngineData engine)
    {
        if (engine == null)
        {
            Debug.LogError("[Fuel Manager] Cannot build table, EngineData is null!");
            return;
        }

        // 1. Clear out any old visual elements
        ClearActiveLayouts();

        // 2. Fetch the dynamic limits from the active engine build specs
        float maxRPM = CalculateActiveRedline(engine);
        float maxKpa = GetMaxMapPressure(engine);
        float minKpa = engine.baseMinKpa;

        // 3. Generate perimeter text labels
        GenerateAxisLabels(maxRPM, minKpa, maxKpa);

        // 4. Populate or verify the data array matrix
        InitializeFuelTableData(engine);

        // 5. Spawn the interactive clickable input cells
        PopulateGridCells(engine, maxKpa);
    }

    private void ClearActiveLayouts()
    {
        foreach (Transform child in xAxisHeaderContainer) Destroy(child.gameObject);
        foreach (Transform child in yAxisSidebarContainer) Destroy(child.gameObject);
        foreach (Transform child in gridContainer) Destroy(child.gameObject);
    }

    private void GenerateAxisLabels(float maxRPM, float minKpa, float maxKpa)
    {
        // 1. New X-Axis (Engine Load kPa) -> Left to Right (Low to High)
        float kpaStep = (maxKpa - minKpa) / (rpmBins - 1); // Note: keeping 10x10 grid
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

    private void InitializeFuelTableData(EngineData engine)
    {
        engine.savedFuelTableData = new float[loadBins * rpmBins];

        for (int row = 0; row < loadBins; row++) // Y-Axis: RPM
        {
            float rpmFactor = (float)row / (loadBins - 1);

            for (int col = 0; col < rpmBins; col++) // X-Axis: kPa
            {
                float loadFactor = (float)col / (rpmBins - 1);

                // Safe base pulse-width curves
                float baseFuel = 2.2f;
                float progressiveScaling = (rpmFactor * 4.3f) + (loadFactor * 5.0f);
                float baseCalibration = baseFuel + progressiveScaling;

                // Slightly lean choke point at top-right (load > 60%, RPM > 40%)
                if (loadFactor > 0.6f && rpmFactor > 0.4f)
                {
                    baseCalibration *= 0.88f;
                }

                int index = (row * rpmBins) + col;
                engine.savedFuelTableData[index] = Mathf.Round(baseCalibration * 10f) / 10f;
            }
        }
    }

    // ➔ FIXED: Added the missing maxKpa parameter to the signature
    private void PopulateGridCells(EngineData engine, float maxKpa)
    {
        for (int row = loadBins - 1; row >= 0; row--)
        {
            for (int col = 0; col < rpmBins; col++)
            {
                GameObject newCell = Instantiate(cellInputFieldPrefab, gridContainer);
                TMP_InputField inputField = newCell.GetComponent<TMP_InputField>();

                int targetIndex = (row * rpmBins) + col;
                float cellValue = engine.savedFuelTableData[targetIndex];

                inputField.text = cellValue.ToString("F1");

                // ➔ FIXED: Apply color dynamically based on engine limit rules
                ApplyCellColor(inputField, cellValue, maxKpa);

                int runningIndex = targetIndex;
                inputField.onEndEdit.AddListener((newValue) =>
                {
                    OnCellDataUpdated(engine, runningIndex, newValue, inputField, maxKpa);
                });
            }
        }
    }

    // ➔ FIXED: Added maxKpa parameter pass-through here
    private void OnCellDataUpdated(EngineData engine, int index, string value, TMP_InputField field, float maxKpa)
    {
        if (float.TryParse(value, out float parsedValue))
        {
            // Clamp values to realistic pulse width limits (1.0ms to 25.0ms)
            engine.savedFuelTableData[index] = Mathf.Clamp(parsedValue, 1.0f, 25.0f);
        }

        float finalValue = engine.savedFuelTableData[index];
        field.text = finalValue.ToString("F1");

        // ➔ FIXED: Apply color dynamically on updates
        ApplyCellColor(field, finalValue, maxKpa);
    }

    /// <summary>
    /// Blends the cell background color from green (low fuel pulse) to red (high fuel pulse).
    /// </summary>
    private void ApplyCellColor(TMP_InputField field, float value, float maxKpa)
    {
        UnityEngine.UI.Image bgImage = field.GetComponent<UnityEngine.UI.Image>();
        if (bgImage == null) bgImage = field.placeholder.GetComponentInParent<UnityEngine.UI.Image>();

        if (bgImage != null)
        {
            // Dynamic Fuel Scaling:
            // Naturally aspirated engines require less peak fuel (~12ms max scale)
            // Forced induction engines flow significantly more air/fuel under boost (~18ms max scale)
            float minFuel = 2.0f;
            float maxFuel = (maxKpa > 115f) ? 18.0f : 12.0f;

            float t = Mathf.InverseLerp(minFuel, maxFuel, value);

            // Professional dark-forest-green to rich-red gradient
            Color lowColor = new Color(0.12f, 0.53f, 0.12f, 1f);
            Color highColor = new Color(0.78f, 0.15f, 0.15f, 1f);

            bgImage.color = Color.Lerp(lowColor, highColor, t);
        }
    }

    // --- MECHANICAL SPEC BOUNDARY CALCULATORS ---

    /// <summary>
    /// Checks if a performance head or valvetrain asset is equipped in the slot.
    /// If so, it expands the grid's maximum RPM ceiling.
    /// </summary>
    public float CalculateActiveRedline(EngineData engine)
    {
        float redline = engine.factoryMaxRPM;

        // Simply check if any aftermarket head assembly is bolted on
        if (engine.installedHead != null)
            redline += 300f;

        // Simply check if any upgraded valvetrain package is equipped
        if (engine.installedValvetrain != null)
            redline += 700f;

        return redline;
    }

    /// <summary>
    /// Checks if a turbocharger or supercharger asset is present to scale to boost loads.
    /// </summary>
    public float GetMaxMapPressure(EngineData engine)
    {
        // If either forced induction slot is populated, expand the grid resolution to 240 kPa
        if (engine.installedTurbo != null || engine.installedSupercharger != null)
        {
            return 240f;
        }
        return engine.baseMaxKpa; // 115 kPa factory stock NA ceiling
    }
}