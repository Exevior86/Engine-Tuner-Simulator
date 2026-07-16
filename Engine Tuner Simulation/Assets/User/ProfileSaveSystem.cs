using UnityEngine;
using System.IO;

public class ProfileSaveSystem : MonoBehaviour
{
    public static ProfileSaveSystem Instance { get; private set; }
    public UserProfile ActiveProfile { get; private set; }

    // Explicitly default to true so it can never accidentally skip the name panel
    public bool IsNewUser { get; private set; } = true;

    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "user_profile.json");

        // Let's sweep the drive immediately on boot
        LoadProfile();
    }

    public void LoadProfile()
    {
        // 1. If the file literally does not exist on your computer, they are 100% a new user
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("[Save System] Physical save file not found on disk. Setting IsNewUser = TRUE.");
            IsNewUser = true;
            ActiveProfile = null;
            return;
        }

        try
        {
            string jsonString = File.ReadAllText(saveFilePath);

            // Safety Check: If the file exists but it's completely empty text
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                IsNewUser = true;
                return;
            }

            ActiveProfile = JsonUtility.FromJson<UserProfile>(jsonString);

            // 2. Structural Validation: Does it have a default or blank name payload?
            if (ActiveProfile == null || string.IsNullOrWhiteSpace(ActiveProfile.playerName) || ActiveProfile.playerName == "Driver")
            {
                Debug.Log("[Save System] File data is blank or uninitialized placeholder. Setting IsNewUser = TRUE.");
                IsNewUser = true;
            }
            else
            {
                // ONLY set false if a valid custom profile name was read out of the JSON string
                IsNewUser = false;
                Debug.Log($"[Save System] Profile verified for: {ActiveProfile.playerName}. IsNewUser = FALSE.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Save System] Error reading save path. Defaulting to new user: {e.Message}");
            IsNewUser = true;
        }
    }

    public void InitializeNewProfile(string chosenName)
    {
        ActiveProfile = new UserProfile();
        ActiveProfile.playerName = chosenName.Trim();
        ActiveProfile.cashBalance = 5000f;
        ActiveProfile.purchasedPartIDs.Add("Commercial_91_Octane");

        IsNewUser = false; // Now they are no longer a new user!
        SaveProfile();
    }

    public void SaveProfile()
    {
        if (ActiveProfile == null) return;
        string jsonString = JsonUtility.ToJson(ActiveProfile, true);
        File.WriteAllText(saveFilePath, jsonString);
    }
}