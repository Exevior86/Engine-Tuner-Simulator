using UnityEngine;

public class Quit : MonoBehaviour
{
    // Call this from a UI Button OnClick() to quit the application.
    public void QuitApplication()
    {
#if UNITY_EDITOR
        // Stop play mode when running inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }
}
