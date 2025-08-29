using UnityEngine;
using TMPro;
using Clicker.Core;

public class KPIUIUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI kpiText;

    private void Start()
    {
        GameState.Instance.OnKPIChanged += UpdateText;
        UpdateText(GameState.Instance.GetKPI());
    }

    private void OnDisable()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnKPIChanged -= UpdateText;
    }

    void UpdateText(double value)
    {
        kpiText.text = $"{value:N0}";
    }
}
