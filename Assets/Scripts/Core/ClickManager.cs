using UnityEngine;
using UnityEngine.EventSystems;
using Clicker.Core;
using Clicker.Upgrades;

namespace Clicker.Core
{
    public class ClickManager : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private double baseKPI = 1.0;

    public void OnPointerDown(PointerEventData eventData)
    {
        UpgradeManager.Instance.OnPlayerClick();

        double baseFlat = baseKPI + UpgradeManager.Instance.GetFlatBonusPerClick();
        double mult     = UpgradeManager.Instance.GetClickMultiplier();

        GameState.Instance.AddKPIFromClick(baseFlat * mult);
    }

    }
}