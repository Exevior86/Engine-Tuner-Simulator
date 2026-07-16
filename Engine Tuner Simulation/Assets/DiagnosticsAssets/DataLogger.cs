using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LogFrame
{
    public float timestamp;

    // Core Telemetry
    public float rpm;
    public float boost;
    public float airFuelRatio;
    public float ignitionTiming;
    public float feedbackKnock;

    // ➔ NEW: Advanced Telemetry
    public float airMass;             // Grams per second (g/s)
    public float ethanolConcentration; // 0% to 100%
    public float targetBoost;         // PSI
    public float wastegateDutyCycle;  // 0% to 100%
    public float manifoldIntakeTemp;  // °F
}

public class DataLogger : MonoBehaviour
{
    public List<LogFrame> activeLog = new List<LogFrame>();
    private float recordingStartTime = 0f; // ➔ ADD THIS: Tracks when the button was clicked
    private bool isRecording = false;

    public void StartRecording()
    {
        activeLog.Clear();
        recordingStartTime = Time.time; // ➔ Record the start wall-clock time
        isRecording = true;
        Debug.Log("[Data Logger] Recording started.");
    }

    public void StopRecording()
    {
        isRecording = false;
        Debug.Log($"[Data Logger] Recording stopped. Captured {activeLog.Count} frames.");
    }

    public void ClearLog()
    {
        activeLog.Clear();
        Debug.Log("[Data Logger] Log cleared.");
    }

    public void RecordFrame(
        float currentRpm,
        float currentBoost,
        float currentAfr,
        float currentTiming,
        float currentKnock,
        float currentAirMass,
        float currentEthanol,
        float currentTargetBoost,
        float currentWastegate,
        float currentIntakeTemp)
    {
        if (!isRecording) return;

        // ➔ FIXED: Calculate timestamp relative to the session start!
        float relativeTimestamp = Time.time - recordingStartTime;

        LogFrame frame = new LogFrame
        {
            timestamp = relativeTimestamp, // Always starts exactly at 0.0s!
            rpm = currentRpm,
            boost = currentBoost,
            airFuelRatio = currentAfr,
            ignitionTiming = currentTiming,
            feedbackKnock = currentKnock,
            airMass = currentAirMass,
            ethanolConcentration = currentEthanol,
            targetBoost = currentTargetBoost,
            wastegateDutyCycle = currentWastegate,
            manifoldIntakeTemp = currentIntakeTemp
        };
        activeLog.Add(frame);
    }
}