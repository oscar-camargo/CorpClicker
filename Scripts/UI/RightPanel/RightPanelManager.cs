using UnityEngine;

public class RightPanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject dashboardPanel;
    [SerializeField] private GameObject outlookPanel;

    private void Start()
    {
        // Ensure dashboard is shown by default
        ShowDashboard();
    }

    public void ShowDashboard()
    {
        dashboardPanel.SetActive(true);
        outlookPanel.SetActive(false);
    }

    public void ShowOutlook()
    {
        dashboardPanel.SetActive(false);
        outlookPanel.SetActive(true);
    }
}
