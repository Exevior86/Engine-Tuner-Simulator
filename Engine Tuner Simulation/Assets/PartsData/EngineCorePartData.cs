using UnityEngine;

[CreateAssetMenu(fileName = "New Engine Block", menuName = "Apex Sim/Parts/Core/Engine Block")]
public class EngineBlockData : EnginePartData
{
    public float blockDisplacementLiters;
    public int cylinderCount;
    public float maxTorqueCapacity; // Physical block structural destruction ceiling
}

[CreateAssetMenu(fileName = "New Cylinder Head", menuName = "Apex Sim/Parts/Core/Cylinder Head")]
public class CylinderHeadData : EnginePartData
{
    public float combustionChamberVolumeCC; // Combined with piston stroke to calculate static compression ratio
    public int valvesPerCylinder;
}

[CreateAssetMenu(fileName = "New Head Port & Valvetrain", menuName = "Apex Sim/Parts/Core/Valvetrain")]
public class ValvetrainData : EnginePartData
{
    public float maxRPMCeiling;          // Prevents valve float up to high RPMs (e.g., 16k RPM for Indy)
    public float airflowCFMModifier;     // Port polish flow improvement coefficient
}

[CreateAssetMenu(fileName = "New Camshaft Profile", menuName = "Apex Sim/Parts/Core/Camshaft")]
public class CamshaftData : EnginePartData
{
    public float valveLiftMM;
    public float durationDegrees;
    public float optimalPowerBandStartRPM; // Shifts where the airflow efficiency peaks
}

[CreateAssetMenu(fileName = "New Pistons & Rods", menuName = "Apex Sim/Parts/Core/Pistons and Rods")]
public class PistonsRodsData : EnginePartData
{
    public float pistonDomeVolumeCC; // Positive/negative volume shapes your final Compression Ratio
    public bool isForged;            // Radically alters structural max cylinder pressure limit
}