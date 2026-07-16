using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationManager : MonoBehaviour
{
    [Header("Scene Configuration Names")]
    [Tooltip("The exact string name of your primary main menu / garage landing page scene.")]
    public string mainHomeSceneName = "MainMenuScene";

    [Tooltip("The exact string name of your 2D tuning canvas workshop bay scene.")]
    public string tuningWorkshopSceneName = "TuningBayScene";

    [Tooltip("The exact string name of your loud, high-energy 3D dyno test cell scene.")]
    public string dynoTestCellSceneName = "DynoRunScene";

    // Static instance allows any UI button anywhere to call navigation seamlessly
    public static SceneNavigationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Navigates the player instantly to the primary Home Menu.
    /// Manual Inspector Assignment: SceneNavigationManager -> LoadHomeMenu
    /// </summary>
    public void LoadHomeMenu()
    {
        Debug.Log("[Navigation] Exiting to Home Menu...");
        SceneManager.LoadScene(mainHomeSceneName);
    }

    /// <summary>
    /// Enters the calibration workshop bay.
    /// Manual Inspector Assignment: SceneNavigationManager -> LoadTuningWorkshop
    /// </summary>
    public void LoadTuningWorkshop()
    {
        Debug.Log("[Navigation] Entering Tuning Workshop...");
        SceneManager.LoadScene(tuningWorkshopSceneName);
    }

    /// <summary>
    /// Launches the dyno cell test environment.
    /// Manual Inspector Assignment: SceneNavigationManager -> LoadDynoTestCell
    /// </summary>
    public void LoadDynoTestCell()
    {
        // PRO TIP: This is where you can add code later to save the active 
        // engine map data arrays right before the new scene boots up!
        Debug.Log("[Navigation] Saving ECU maps and launching Dyno Cell...");
        SceneManager.LoadScene(dynoTestCellSceneName);
    }
}