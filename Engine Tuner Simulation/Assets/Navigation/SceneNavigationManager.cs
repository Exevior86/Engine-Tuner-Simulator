using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        // Listen for whenever a scene changes so we can automatically wire up buttons
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Clean up our listener if the object is ever permanently destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This runs automatically EVERY time a new scene finishes loading
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Look for any UI buttons in the newly loaded scene that need to be hooked up
        AttachButtonsDynamically();
    }

    /// <summary>
    /// Finds buttons in the active scene by tag or name and forces them to link to this immortal manager.
    /// </summary>
    private void AttachButtonsDynamically()
    {
        // 1. Look for a button named "PlayButton" or "TuningButton" in your Main Menu scene
        GameObject playBtnObj = GameObject.Find("PlayButton"); // ➔ Make sure your Main Menu play button is named exactly this!
        if (playBtnObj != null)
        {
            Button btn = playBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners(); // Clear dead references
                btn.onClick.AddListener(LoadTuningWorkshop); // Snap the link to the active immortal manager
                Debug.Log("Successfully re-wired Main Menu Play Button!");
            }
        }

        // 2. Look for a button named "HomeButton" in your Tuning or Dyno scenes
        GameObject homeBtnObj = GameObject.Find("HomeButton"); // ➔ Make sure your Home/Return buttons are named exactly this!
        if (homeBtnObj != null)
        {
            Button btn = homeBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(LoadHomeMenu);
                Debug.Log("Successfully re-wired Return Home Button!");
            }
        }
    }

    /// <summary>
    /// Navigates the player instantly to the primary Home Menu.
    /// </summary>
    public void LoadHomeMenu()
    {
        Debug.Log("Exiting to Home Menu...");
        SceneManager.LoadScene(mainHomeSceneName);
    }

    /// <summary>
    /// Enters the calibration workshop bay.
    /// </summary>
    public void LoadTuningWorkshop()
    {
        Debug.Log("Entering Tuning Workshop...");
        SceneManager.LoadScene(tuningWorkshopSceneName);
    }

    /// <summary>
    /// Launches the dyno cell test environment.
    /// </summary>
    public void LoadDynoTestCell()
    {
        // PRO TIP: This is where you will add code later to save the active 
        // engine map data arrays right before the new scene boots up!
        Debug.Log("Saving ECU maps and launching Dyno Cell...");
        SceneManager.LoadScene(dynoTestCellSceneName);
    }
}