using UnityEngine;

public class EcuController : MonoBehaviour
{
    [Header("Central System References")]
    public CarTuningSessionManager sessionManager;
    public DiagnosticsPanelManager diagnosticsPanelManager; // Direct reference to your new panel script
    public DataLogger dataLogger;

    [Header("Simulation Settings (For Testing)")]
    [Tooltip("Press 'G' in Playmode to simulate a quick 10-second dyno run log!")]
    public bool enableTestKeys = true;

    private bool isSimulatingRun = false;
    private float simTimer = 0f;
    private float simRpm = 1000f;

    // ➔ ADD THESE: Keep track of the active engine limits for the current run
    public float activeRedline = 7000f;
    public float activeMaxKpa = 115f;

    private void Start()
    {
        if (dataLogger == null) dataLogger = GetComponent<DataLogger>();
        if (sessionManager == null) sessionManager = GetComponent<CarTuningSessionManager>();

        // Auto-detect the new panel manager if not set
        if (diagnosticsPanelManager == null) diagnosticsPanelManager = FindFirstObjectByType<DiagnosticsPanelManager>();
    }

    private void Update()
    {
        if (enableTestKeys)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (!isSimulatingRun)
                    StartTestDynoRun();
                else
                    StopTestDynoRun();
            }
        }

        if (isSimulatingRun)
        {
            SimulateEngineTelemetry();
        }
    }

    public void StartTestDynoRun()
    {
        isSimulatingRun = true;
        simTimer = 0f;
        simRpm = 1000f;

        // ➔ NEW: Fetch dynamic mechanical limits from the engine before starting!
        if (sessionManager != null && sessionManager.activeEngineProfile != null)
        {
            EngineData activeEngine = sessionManager.activeEngineProfile;

            // We can call the helper methods you already wrote in your mapping managers!
            activeRedline = CalculateActiveRedline(activeEngine);
            activeMaxKpa = GetMaxMapPressure(activeEngine);

            Debug.Log($"[Dyno Sim] Starting run. Dynamic Redline: {activeRedline} RPM. Max Load: {activeMaxKpa} kPa.");
        }
        else
        {
            // Fallback defaults if no engine profile is loaded yet
            activeRedline = 7000f;
            activeMaxKpa = 115f;
        }

        if (dataLogger != null)
        {
            dataLogger.StartRecording();
        }
    }

    public void StopTestDynoRun()
    {
        isSimulatingRun = false;

        if (dataLogger != null)
        {
            dataLogger.StopRecording();
        }

        // Force the new UI panel manager to refresh and draw the selected lines!
        if (diagnosticsPanelManager != null)
        {
            diagnosticsPanelManager.RefreshGraphs();
        }

    }

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

    /// <summary>
    /// Generates physical-feeling engine numbers over a mock 10-second pull,
    /// now updated to simulate all 10 advanced logging channels!
    /// </summary>
    private void SimulateEngineTelemetry()
    {
        simTimer += Time.deltaTime;

        float maxDuration = 10f;
        float progress = Mathf.Clamp01(simTimer / maxDuration);

        // --- NOISE GENERATORS ---
        // 1. High-frequency electrical/vibrational noise (Sensor jitter)
        float jitter = Random.Range(-0.15f, 0.15f);

        // 2. Medium-frequency rolling waves (mimics wastegate oscillation or air turbulence)
        // Using Perlin Noise with time-based scaling creates organic drift
        float waveNoise = Mathf.PerlinNoise(simTimer * 4.0f, 0.0f) * 2.0f - 1.0f; // Range: -1 to 1
        float rapidWaveNoise = Mathf.PerlinNoise(simTimer * 12.0f, 1.0f) * 2.0f - 1.0f; // Rapid fluctuations

        // --- 1. RPM Sweeping ---
        // Engine RPM is heavy and damped by inertia, but still has minor mechanical vibration
        float baseRpm = Mathf.Lerp(1000f, activeRedline, progress);
        simRpm = baseRpm + (jitter * 15f);

        // --- 2. Dynamic & Noisy Boost ---
        bool isBoosted = activeMaxKpa > 115f;
        float targetBoost = isBoosted ? 18.0f : 0f;
        float boost = 0f;

        if (isBoosted)
        {
            // Boost builds, overshoots, settles, then oscillates and tapers
            float spoolProgress = Mathf.Clamp01(progress / 0.3f); // Spools hard in the first 30% of the run
            float peakBoost = 18.0f;

            // Base curve
            boost = Mathf.Lerp(-10.0f, peakBoost, spoolProgress);

            if (progress > 0.3f)
            {
                // Active boost control phase: Simulate wastegate duty cycle hunting
                // Adding a rolling oscillation + rapid turbulent noise to match your image reference
                float boostFluctuation = (waveNoise * 0.4f) + (rapidWaveNoise * 0.25f);

                // Boost taper as the turbo runs out of breath at high RPM
                float taper = (progress - 0.3f) * 3.5f;
                boost = peakBoost - taper + boostFluctuation;
            }
        }
        else
        {
            // Naturally Aspirated: Engine runs vacuum, transitioning to atmospheric pressure (0 PSI)
            float vacuumWave = waveNoise * 0.1f;
            boost = Mathf.Lerp(-10f, 0f, progress) + vacuumWave;
        }

        // Final clamp to keep physical safety
        boost = Mathf.Clamp(boost, -14.7f, 25f);

        // --- 3. Air-Fuel Ratio (AFR) ---
        // AFR oscillates rapidly due to injector spray patterns and sensor update speeds
        float targetRichness = isBoosted ? 11.2f : 12.8f;
        float baseAfr = Mathf.Lerp(14.7f, targetRichness, progress);
        // Inject high-frequency jitter + slight enrichment waves
        float afr = baseAfr + (rapidWaveNoise * 0.08f) + (jitter * 0.05f);

        // --- 4. Ignition Timing Advance ---
        // Spark timing moves around depending on load and dynamic ECU corrections
        float baseTiming = 24f - (boost * 0.5f) + (progress * 5f);
        float timing = baseTiming + (waveNoise * 0.3f);

        // --- 5. Feedback Knock ---
        float knock = 0f;
        if (progress > 0.55f && progress < 0.65f && isBoosted)
        {
            knock = -2.8f;
            timing -= 2.8f; // Pull timing when knock happens!
        }

        // --- 6. Air Mass (g/s) ---
        // MAF sensors are incredibly noisy because they read turbulent intake air pulses
        float baseAirMass = (simRpm / activeRedline) * 150f;
        float boostMultiplier = 1.0f + (Mathf.Max(0f, boost) / 14.7f);
        float airMass = (baseAirMass * boostMultiplier);
        // MAF noise scales with volume: higher air velocity = much noisier sensor readings!
        airMass += (rapidWaveNoise * (airMass * 0.03f)) + (jitter * 2.0f);

        // --- 7. Ethanol Concentration ---
        // Fuel composition sensors are stable, but still have trace electrical noise
        float ethanol = 10f + (jitter * 0.05f);

        // --- 8. Wastegate Position ---
        // Wastegate duty cycle flutters rapidly to keep boost steady under oscillation
        float wastegate = 0f;
        if (isBoosted)
        {
            float baseWg = 100f;
            if (progress > 0.3f)
            {
                baseWg = Mathf.Lerp(100f, 45f, (progress - 0.3f) / 0.7f);
            }
            // Add flutter representing the solenoid pulsing
            wastegate = Mathf.Clamp(baseWg + (rapidWaveNoise * 2.5f), 0f, 100f);
        }

        // --- 9. Intake Temp Manifold ---
        // Temps move slowly, but still exhibit slight sensor decimal bounce
        float baseTemp = 85f;
        float tempRise = (boost > 0) ? (boost * 2.2f) : 0f;
        float intakeTemp = baseTemp + tempRise + (jitter * 0.2f);

        // Feed our noisy, realistic parameters into our data logger!
        if (dataLogger != null)
        {
            dataLogger.RecordFrame(
                simRpm,
                boost,
                afr,
                timing,
                knock,
                airMass,
                ethanol,
                targetBoost,
                wastegate,
                intakeTemp
            );
        }

        if (simTimer >= maxDuration)
        {
            StopTestDynoRun();
        }
    }
}