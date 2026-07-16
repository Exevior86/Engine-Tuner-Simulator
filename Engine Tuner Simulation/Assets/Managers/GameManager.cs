using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Active Vehicle State")]
    [Tooltip("This is the engine configuration that the Dyno Scene will read from.")]
    public EngineData equippedEngine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // ➔ Prevents this object from dying when loading scenes!
    }
}