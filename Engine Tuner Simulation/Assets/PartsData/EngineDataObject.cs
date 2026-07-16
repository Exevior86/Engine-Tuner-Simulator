using UnityEngine;

[CreateAssetMenu(fileName = "NewEngineData", menuName = "TuningSim/Engine Data")]
public class EngineData : ScriptableObject
{
    [Header("Identity Metadata")]
    public string engineName = "Factory Base Engine";
    public string engineCode = "STOCK-4";
    public int cylinderCount = 4;

    [HideInInspector]
    public float[] savedFuelTableData = new float[100];

    [HideInInspector]
    public float[] savedIgnitionTableData = new float[100];

    [Header("Base Factory Constraints")]
    [Tooltip("The absolute maximum physical RPM the stock valvetrain can handle before damage.")]
    public float factoryMaxRPM = 6500f;
    [Tooltip("The lowest pressure row on the map (typically deep engine vacuum at idle).")]
    public float baseMinKpa = 20f;
    [Tooltip("The maximum pressure row for a stock naturally aspirated setup (atmospheric limit).")]
    public float baseMaxKpa = 115f;

    [Header("Assigned Component Assembly Build Slots")]
    [Tooltip("Leave as None/Null to signify the factory stock component is equipped.")]
    public EngineBlockData installedBlock = null;
    public CylinderHeadData installedHead = null;
    public ValvetrainData installedValvetrain = null;
    public PistonRodsData installedPistons = null;
    public CamshaftData installedCamshaft = null;

    [Header("Airflow & Exhaust Mechanicals")]
    public ThrottleBodyData installedThrottleBody = null;
    public IntakeManifoldData installedIntakeManifold = null;
    public ExhaustHeadersData exhaustHeaders = null;
    public DownpipeData installedDownpipe = null;
    public CatBackExhaustData installedExhaust = null;

    [Header("Forced Induction Network")]
    public TurboData installedTurbo = null;
    public SuperchargerData installedSupercharger = null;
    public IntercoolerData installedIntercooler = null;
    public BoostControllerData installedBoostController = null;

    [Header("Fuel Delivery System")]
    public FuelPumpData installedFuelPump = null;
    public InjectorData installedInjectors = null;
    public FlexFuelSensorData installedFlexSensor = null;
    public FuelData currentTankFuel = null;
}
