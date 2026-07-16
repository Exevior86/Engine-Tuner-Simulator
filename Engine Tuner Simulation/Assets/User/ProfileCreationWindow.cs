using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProfileCreationWindow : MonoBehaviour
{
    [Header("UI Component Bindings")]
    public GameObject mainMenuPanel;        // Optional: Your main title screen UI panel
    public GameObject creationPanel;        // Your name input UI panel (keeps hidden at start)
    public TMP_InputField nameInputField;
    public Button confirmButton;

    [Header("Scene Progression Config")]
    public string nextSceneToLoad = "TuningBayScene";

    private void Start()
    {
        // Force the creation screen to be hidden when the game first loads
        if (creationPanel != null)
        {
            creationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ➔ LINK THIS TO YOUR MAIN MENU PLAY BUTTON!
    /// Evaluates the save state ONLY when the player decides to start the game.
    /// </summary>
    public void OnMainMenuPlayPressed()
    {
        if (ProfileSaveSystem.Instance == null)
        {
            Debug.LogError("[Flow Error] Save System is missing from the scene!");
            return;
        }

        // Check if this computer has a save file
        if (ProfileSaveSystem.Instance.IsNewUser)
        {
            Debug.Log("[Flow] New user detected. Displaying profile creation panel...");
            creationPanel.SetActive(true);
        }
        else
        {
            Debug.Log("[Flow] Existing user found. Skipping name entry and loading game...");
            ProceedToGame();
        }
    }

    public void OnConfirmNameButtonClicked()
    {
        string rawInputText = nameInputField.text;
        Debug.Log($"Player entered name: '{rawInputText}'");
        if (string.IsNullOrWhiteSpace(rawInputText))
        {
            Debug.LogWarning("Player name cannot be empty spaces!");
            return;
        }

        // Initialize the profile and write the user_profile.json file to disk
        ProfileSaveSystem.Instance.InitializeNewProfile(rawInputText);
        creationPanel.SetActive(false);

        ProceedToGame();
    }

    private void ProceedToGame()
    {
        Debug.Log($"Transitioning scene to: '{nextSceneToLoad}'");
        SceneManager.LoadScene(nextSceneToLoad);
    }
}