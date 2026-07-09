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

            float s1 = float.Parse(data[5]);
            float s2 = float.Parse(data[6]);
            float s3 = float.Parse(data[7]);
            float s4 = float.Parse(data[8]);
            float s5 = float.Parse(data[9]);

            EnginePartData assetInstance = null;
            string targetSubfolder = "Misc";

            // Translate generic CSV parameters directly to specific ScriptableObject types
            switch (partType)
            {
                case "Camshaft":
                    var cam = ScriptableObject.CreateInstance<CamshaftData>();
                    cam.valveLiftMM = s1;
                    cam.durationDegrees = s2;
                    cam.optimalPowerBandStartRPM = s3;
                    assetInstance = cam;
                    targetSubfolder = "Core";
                    break;

                case "Downpipe":
                    var dp = ScriptableObject.CreateInstance<DownpipeData>();
                    dp.pipingDiameterInches = s1;
                    dp.hasCatalyticConverter = (s2 == 1);
                    assetInstance = dp;
                    targetSubfolder = "Airflow";
                    break;

                case "EBC":
                    var ebc = ScriptableObject.CreateInstance<BoostControllerData>();
                    ebc.isElectronic = (s1 == 1);
                    ebc.maxFrequencyHz = s2;
                    assetInstance = ebc;
                    targetSubfolder = "ForcedInduction";
                    break;

                case "EngineBlock":
                    var block = ScriptableObject.CreateInstance<EngineBlockData>();
                    block.blockDisplacementLiters = s1;
                    block.cylinderCount = (int)s2;
                    block.maxTorqueCapacity = s3;
                    assetInstance = block;
                    targetSubfolder = "Core";
                    break;

                case "EngineAssembly":
                    var eng = ScriptableObject.CreateInstance<EngineData>();
                    // Assigning eng metadata directly instead of routing through the generic assetInstance
                    eng.engineCode = partName;

                    string engFileName = partName.Replace(" ", "_").Replace("/", "-");
                    string engPath = $"Assets/PartsCatalog/Assemblies/{engFileName}.asset";

                    AssetDatabase.CreateAsset(eng, engPath);
                    importCount++;
                    continue;

                case "ExhaustManifold":
                    var exMan = ScriptableObject.CreateInstance<ExhaustManifoldData>();
                    exMan.runnerDiameterInches = s1;
                    exMan.isTubularHeader = (s2 == 1);
                    assetInstance = exMan;
                    targetSubfolder = "Airflow";
                    break;

                case "Headers":
                    var headMan = ScriptableObject.CreateInstance<ExhaustManifoldData>();
                    headMan.runnerDiameterInches = s1;
                    headMan.isTubularHeader = (s2 == 1);
                    assetInstance = headMan;
                    targetSubfolder = "Airflow";
                    break;

                case "Exhaust":
                    var ex = ScriptableObject.CreateInstance<CatBackExhaustData>();
                    ex.diameterInches = s1;
                    ex.mufflerRestrictionFactor = s2;
                    assetInstance = ex;
                    targetSubfolder = "Airflow";
                    break;

                case "FlexFuelSensor":
                    // Change from FuelSystemPartData to FlexFuelSensorData
                    var flex = ScriptableObject.CreateInstance<FlexFuelSensorData>();
                    flex.permitsEthanolBlending = (s1 == 1);
                    assetInstance = flex;
                    targetSubfolder = "Fuel";
                    break;

                case "Fuel":
                    // Change from FuelSystemPartData to FuelData
                    var fuel = ScriptableObject.CreateInstance<FuelData>();
                    fuel.octaneRating = s1;
                    fuel.stoichiometricAFR = s2;
                    fuel.coolingFactor = s3;
                    assetInstance = fuel;
                    targetSubfolder = "Fuel";
                    break;

                case "FuelPump":
                    // Change from FuelSystemPartData to FuelPumpData
                    var pump = ScriptableObject.CreateInstance<FuelPumpData>();
                    pump.flowRateLitersPerHour = s1;
                    pump.maxPressurePSI = s2;
                    assetInstance = pump;
                    targetSubfolder = "Fuel";
                    break;

                case "Head":
                    var head = ScriptableObject.CreateInstance<CylinderHeadData>();
                    head.combustionChamberVolumeCC = s1;
                    head.valvesPerCylinder = (int)s2;
                    assetInstance = head;
                    targetSubfolder = "Core";
                    break;

                case "HeadPortValvetrain":
                    var vTrain = ScriptableObject.CreateInstance<ValvetrainData>();
                    vTrain.maxRPMCeiling = s1;
                    vTrain.airflowCFMModifier = s2;
                    assetInstance = vTrain;
                    targetSubfolder = "Core";
                    break;

                case "Injectors":
                    var inj = ScriptableObject.CreateInstance<InjectorData>();
                    inj.flowRateCCMin = s1;
                    inj.maxSafeDutyCycle = s2;
                    assetInstance = inj;
                    targetSubfolder = "Fuel";
                    break;

                case "IntakeManifold":
                    var intMan = ScriptableObject.CreateInstance<IntakeManifoldData>();
                    intMan.plenumVolumeLiters = s1;
                    intMan.runnerLengthMM = (int)s2;
                    assetInstance = intMan;
                    targetSubfolder = "Airflow";
                    break;

                case "Intercooler":
                    var ic = ScriptableObject.CreateInstance<IntercoolerData>();
                    ic.coreVolumeCubicInches = s1;
                    ic.coolingEfficiencyFactor = s2;
                    assetInstance = ic;
                    targetSubfolder = "ForcedInduction";
                    break;

                case "Pistons":
                    var pst = ScriptableObject.CreateInstance<PistonsRodsData>();
                    pst.pistonDomeVolumeCC = s1;
                    pst.isForged = (s2 == 1);
                    assetInstance = pst;
                    targetSubfolder = "Core";
                    break;

                case "Supercharger":
                    var sc = ScriptableObject.CreateInstance<SuperchargerData>();
                    sc.displacementPerRev = s1;
                    sc.maxDriveRatio = s2;
                    assetInstance = sc;
                    targetSubfolder = "ForcedInduction";
                    break;

                case "ThrottleBody":
                    var tb = ScriptableObject.CreateInstance<ThrottleBodyData>();
                    tb.butterflyDiameterMM = s1;
                    assetInstance = tb;
                    targetSubfolder = "Airflow";
                    break;

                case "Turbo":
                    var trb = ScriptableObject.CreateInstance<TurboData>();
                    trb.maxFlowLbsMin = s1;
                    trb.spoolRPM = s2;
                    trb.maxBoostLimit = s3;
                    trb.turbineHousingA_R = s4;
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

                // Format a string name safe for file systems
                string structuralFileName = partName.Replace(" ", "_").Replace("/", "-");
                string assetDestinationPath = $"Assets/PartsCatalog/{targetSubfolder}/{structuralFileName}.asset";

                AssetDatabase.CreateAsset(assetInstance, assetDestinationPath);
                importCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Master Upgrade Matrix Synced! Generated {importCount} high-fidelity data assets across category directories.");
    }

    private static void VerifyFolderStructure()
    {
        string baseDir = "Assets/PartsCatalog";
        string[] categories = { "Core", "Airflow", "ForcedInduction", "Fuel", "Assemblies", "Misc" };

        if (!AssetDatabase.IsValidFolder(baseDir))
            AssetDatabase.CreateFolder("Assets", "PartsCatalog");

        foreach (string subFolder in categories)
        {
            string cleanCombinedPath = $"{baseDir}/{subFolder}";
            if (!AssetDatabase.IsValidFolder(cleanCombinedPath))
                AssetDatabase.CreateFolder(baseDir, subFolder);
        }
    }
}