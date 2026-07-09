using UnityEngine;

public class NavigationButton : MonoBehaviour
{
    // Call this function for your "Go to Workshop" buttons
    public void PressGoToWorkshop()
    {
        if (SceneNavigationManager.Instance != null)
        {
            SceneNavigationManager.Instance.LoadTuningWorkshop();
        }
    }

    // Call this function for your "Go to Dyno" buttons
    public void PressGoToDyno()
    {
        if (SceneNavigationManager.Instance != null)
        {
            SceneNavigationManager.Instance.LoadDynoTestCell();
        }
    }

    // Call this function for your "Go to Home / Menu" buttons
    public void PressGoToHome()
    {
        if (SceneNavigationManager.Instance != null)
        {
            SceneNavigationManager.Instance.LoadHomeMenu();
        }
    }
}