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
            double KPIupgrades = baseKPI;
            KPIupgrades *= UpgradeManager.Instance.GetClickMultiplier();
            KPIupgrades += UpgradeManager.Instance.GetFlatBonusPerClick();
            UpgradeManager.Instance.OnPlayerClick(); // for idle boost checks
            GameState.Instance.AddKPI(KPIupgrades);

        }
    }
}