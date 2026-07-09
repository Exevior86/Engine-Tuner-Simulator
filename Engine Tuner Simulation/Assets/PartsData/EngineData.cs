using UnityEngine;

[CreateAssetMenu(fileName = "New Base Engine Assembly", menuName = "Apex Sim/Assemblies/Engine Configuration")]
public class EngineData : ScriptableObject
{
    public string engineCode; // e.g., "K24A2", "SR20DET", "Indy-V6"

    [Header("Equipped Hardware Slots")]
    public EngineBlockData equippedBlock;
    public CylinderHeadData equippedHead;
    public ValvetrainData equippedValvetrain;
    public CamshaftData equippedCams;
    public PistonsRodsData equippedPistons;

    [Header("Equipped Breathing Slots")]
    public IntakeData equippedIntake;
    public ThrottleBodyData equippedThrottleBody;
    public IntakeManifoldData equippedIntakeManifold;
    public ExhaustManifoldData equippedExhaustManifold;
    public DownpipeData equippedDownpipe;
    public CatBackExhaustData equippedExhaustSystem;

    [Header("Equipped Forced Induction")]
    public TurboData equippedTurbo;
    public SuperchargerData equippedSupercharger;
    public IntercoolerData equippedIntercooler;
    public BoostControllerData equippedEBC;

    [Header("Equipped Fuel System")]
    public FuelPumpData equippedFuelPump;
    public InjectorData equippedInjectors;
    public FlexFuelSensorData equippedFlexSensor;
}