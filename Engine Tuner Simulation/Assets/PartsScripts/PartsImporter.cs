#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class PartsImporter : EditorWindow
{
    [MenuItem("Apex Tools/Run Master Database Import")]
    public static void ImportAllPartsFromCSV()
    {
        string filePath = Application.dataPath + "/Resources/PartsDatabase.csv";

        if (!File.Exists(filePath))
        {
            Debug.LogError($"Database missing. Please verify file placement at: {filePath}");
            return;
        }

        // Establish core target tracking folders
        VerifyFolderStructure();

        string[] lines = File.ReadAllLines(filePath);
        int importCount = 0;

        // Skip line 0 (CSV Matrix headers)
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] data = lines[i].Split(',');

            string partType = data[0].Trim();
            string partName = data[1].Trim();
            float cost = float.Parse(data[2]);
            float weight = float.Parse(data[3]);
            int tier = int.Parse(data[4]);

            // 1. Pull image sheet mapping from index 5
            string csvImageName = data[5].Trim();

            // 2. Parse generic performance parameters
            float s1 = float.Parse(data[6]);
            float s2 = float.Parse(data[7]);
            float s3 = float.Parse(data[8]);
            float s4 = float.Parse(data[9]);
            float s5 = float.Parse(data[10]);

            EnginePartData assetInstance = null;
            string targetSubfolder = "Misc";

            // Translate generic parameters directly to specialized fields!
            switch (partType)
            {
                case "Camshaft":
                    var cam = ScriptableObject.CreateInstance<CamshaftData>();
                    cam.valveLiftMm = s1;
                    cam.advertisedDuration = s2;
                    cam.peakVEShiftRpm = s3;
                    assetInstance = cam;
                    targetSubfolder = "Core";
                    break;

                case "Downpipe":
                    var dp = ScriptableObject.CreateInstance<DownpipeData>();
                    dp.pipeDiameterInches = s1;
                    dp.isCatted = (s2 == 1);
                    dp.restrictionReduction = s3 > 0 ? s3 : (s1 > 3.0f ? 0.45f : 0.25f); // Use CSV value if provided, else fallback
                    assetInstance = dp;
                    targetSubfolder = "Airflow";
                    break;

                case "EBC":
                    var ebc = ScriptableObject.CreateInstance<BoostControllerData>();
                    ebc.maxRegulatedBoostPsi = s2 == 0 ? 12f : s2; // Handle manual bleed vs electronic targets
                    ebc.spoolEfficiencyGain = s2 > 0 ? (s2 / 200f) : 0.05f; 
                    ebc.boostStabilityFactor = s2 > 0 ? 0.8f : 0.2f;
                    assetInstance = ebc;
                    targetSubfolder = "ForcedInduction";
                    break;

                case "EngineBlock":
                    var block = ScriptableObject.CreateInstance<EngineBlockData>();
                    block.displacementLiters = s1;
                    block.cylinderCount = (int)s2;
                    block.maximumTorqueLimit = s3;
                    assetInstance = block;
                    targetSubfolder = "Core";
                    break;

                case "Headers":
                    var headers = ScriptableObject.CreateInstance<ExhaustHeadersData>();
                    headers.primaryRunnerDiameterInches = s1;
                    headers.isLongTubeHeader = (s2 == 1);
                    headers.restrictionReduction = s3 > 0 ? s3 : 0.20f;
                    assetInstance = headers;
                    targetSubfolder = "Airflow";
                    break;

                case "Exhaust":
                    var ex = ScriptableObject.CreateInstance<CatBackExhaustData>();
                    ex.pipeDiameterInches = s1;
                    ex.restrictionReduction = s2;
                    assetInstance = ex;
                    targetSubfolder = "Airflow";
                    break;

                case "FlexFuelSensor":
                    var flex = ScriptableObject.CreateInstance<FlexFuelSensorData>();
                    flex.isEnabled = (s1 == 1);
                    assetInstance = flex;
                    targetSubfolder = "Fuel";
                    break;

                case "Fuel":
                    var fuel = ScriptableObject.CreateInstance<FuelData>();
                    fuel.octaneRating = s1;
                    fuel.stoichiometricAFR = s2;
                    fuel.latentHeatEvaporationVal = s3;
                    assetInstance = fuel;
                    targetSubfolder = "Fuel";
                    break;

                case "FuelPump":
                    var pump = ScriptableObject.CreateInstance<FuelPumpData>();
                    pump.flowRateLph = s1;
                    pump.maxOperatingPressurePsi = s2;
                    pump.pumpEfficiency = 0.92f;
                    assetInstance = pump;
                    targetSubfolder = "Fuel";
                    break;

                case "Head":
                    var head = ScriptableObject.CreateInstance<CylinderHeadData>();
                    head.valveDiameterMm = s1;
                    head.valvesPerCylinder = (int)s2;
                    head.combustionCoolingFactor = s2 == 4 ? 0.15f : 0.05f;
                    head.compressionRatioOffset = s2 == 4 ? 0.5f : 0f;
                    assetInstance = head;
                    targetSubfolder = "Core";
                    break;

                case "HeadPortValvetrain":
                    var vTrain = ScriptableObject.CreateInstance<ValvetrainData>();
                    vTrain.safeRedlineLimit = s1;
                    vTrain.valveFlowIncrease = s2;
                    assetInstance = vTrain;
                    targetSubfolder = "Core";
                    break;

                case "Injectors":
                    var inj = ScriptableObject.CreateInstance<InjectorData>();
                    inj.flowRateCcMin = s1;
                    inj.atomizationFactor = s2;
                    inj.latencyMilliseconds = s1 > 1000f ? 1.3f : 0.9f;
                    assetInstance = inj;
                    targetSubfolder = "Fuel";
                    break;

                case "IntakeManifold":
                    var intMan = ScriptableObject.CreateInstance<IntakeManifoldData>();
                    intMan.plenumVolumeLiters = s1;
                    intMan.maxAirflowCapacity = s2;
                    assetInstance = intMan;
                    targetSubfolder = "Airflow";
                    break;

                case "Intercooler":
                    var ic = ScriptableObject.CreateInstance<IntercoolerData>();
                    ic.heatDissipationCapacityWatts = s1;
                    ic.flowEfficiencyFactor = s2;
                    assetInstance = ic;
                    targetSubfolder = "ForcedInduction";
                    break;

                case "Pistons":
                    var pst = ScriptableObject.CreateInstance<PistonRodsData>();
                    pst.compressionOffset = s1;
                    pst.materialStrengthRating = s2; // Direct float assignment!
                    pst.weightReductionGrams = s2 > 0 ? 180f : 0f;
                    assetInstance = pst;
                    targetSubfolder = "Core";
                    break;

                case "Supercharger":
                    var sc = ScriptableObject.CreateInstance<SuperchargerData>();
                    sc.instantaneousBoostPsi = s1;
                    sc.pressureMultiplier = s2;
                    assetInstance = sc;
                    targetSubfolder = "ForcedInduction";
                    break;

                case "ThrottleBody":
                    var tb = ScriptableObject.CreateInstance<ThrottleBodyData>();
                    tb.throttleDiameterMm = s1;
                    tb.responseScaleMultiplier = s1 > 80f ? 1.3f : 1.1f;
                    assetInstance = tb;
                    targetSubfolder = "Airflow";
                    break;

                case "Turbo":
                    var trb = ScriptableObject.CreateInstance<TurboData>();
                    trb.compressorFlowLbMin = s1;
                    trb.spoolThresholdRpm = s2;
                    trb.maxBoostPsi = s3;
                    trb.turbineA_R = s4;
                    assetInstance = trb;
                    targetSubfolder = "ForcedInduction";
                    break;
            }

            if (assetInstance != null)
            {
                // Inject structural metadata values common to all objects
                assetInstance.partName = partName;
                assetInstance.purchaseCost = cost;
                assetInstance.componentWeight = weight;
                assetInstance.unlockTier = tier;
                assetInstance.imageFileName = csvImageName;

                // Format a string name safe for file systems
                string structuralFileName = partName.Replace(" ", "_").Replace("/", "-");
                string assetDestinationPath = $"Assets/Resources/PartsCatalog/{targetSubfolder}/{structuralFileName}.asset";

                // --- THE CRITICAL FIX: FORCE DELETE THE OLD FILE TYPE FIRST ---
                if (File.Exists(Path.Combine(Application.dataPath, assetDestinationPath.Substring(7))))
                {
                    AssetDatabase.DeleteAsset(assetDestinationPath);
                }

                AssetDatabase.CreateAsset(assetInstance, assetDestinationPath);
                importCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"SOLID Upgrade Matrix Synced! Generated {importCount} high-fidelity data assets across category directories.");
    }

    private static void VerifyFolderStructure()
    {
        string baseDir = "Assets/Resources/PartsCatalog";
        string[] categories = { "Core", "Airflow", "ForcedInduction", "Fuel", "Assemblies", "Misc" };

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        if (!AssetDatabase.IsValidFolder(baseDir))
            AssetDatabase.CreateFolder("Assets/Resources", "PartsCatalog");

        foreach (string subFolder in categories)
        {
            string cleanCombinedPath = $"{baseDir}/{subFolder}";
            if (!AssetDatabase.IsValidFolder(cleanCombinedPath))
                AssetDatabase.CreateFolder(baseDir, subFolder);
        }
    }
}
#endif