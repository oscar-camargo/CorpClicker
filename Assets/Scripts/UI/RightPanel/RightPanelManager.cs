using UnityEngine;
using UnityEngine.SceneManagement;

public class RightPanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject dashboardPanel;
    [SerializeField] private GameObject outlookPanel;
    [SerializeField] private GameObject promoteButton;

    [Header("Scene Management")]
    [SerializeField] private string stickySceneName = "StickyCodex";

    private void Start()
    {
        // Ensure dashboard is shown by default
        ShowDashboard();

        promoteButton.SetActive(false);
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

    public void openSticky()
    {
        SceneManager.LoadScene(stickySceneName);
    }
}
