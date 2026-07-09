using UnityEngine;

[CreateAssetMenu(fileName = "New Induction Intake", menuName = "Apex Sim/Parts/Airflow/Intake")]
public class IntakeData : EnginePartData
{
    public float tubeDiameterInches;
    public float filtrationEfficiency; // Affects raw intake restriction
}

[CreateAssetMenu(fileName = "New Throttle Body", menuName = "Apex Sim/Parts/Airflow/Throttle Body")]
public class ThrottleBodyData : EnginePartData
{
    public float butterflyDiameterMM;
}

[CreateAssetMenu(fileName = "New Intake Manifold", menuName = "Apex Sim/Parts/Airflow/Intake Manifold")]
public class IntakeManifoldData : EnginePartData
{
    public float plenumVolumeLiters;
    public int runnerLengthMM;
}

[CreateAssetMenu(fileName = "New Exhaust Manifold", menuName = "Apex Sim/Parts/Airflow/Exhaust Manifold")]
public class ExhaustManifoldData : EnginePartData
{
    public float runnerDiameterInches;
    public bool isTubularHeader; // True for tubular headers, false for restrictive cast iron
}

[CreateAssetMenu(fileName = "New Downpipe", menuName = "Apex Sim/Parts/Airflow/Downpipe")]
public class DownpipeData : EnginePartData
{
    public float pipingDiameterInches;
    public bool hasCatalyticConverter; // High-flow vs straight-pipe backpressure modifier
}

[CreateAssetMenu(fileName = "New Cat-Back Exhaust", menuName = "Apex Sim/Parts/Airflow/Cat-Back Exhaust")]
public class CatBackExhaustData : EnginePartData
{
    public float diameterInches;
    public float mufflerRestrictionFactor;
}