using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Clicker.Upgrades;
using Clicker.Core;
using UnityEngine.EventSystems;

namespace Clicker.UI.UpgradeUI
{
    public class UpgradeSlotDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image levelBar;
        [SerializeField] private TextMeshProUGUI purchaseCountText;
        [SerializeField] private GameObject purchaseCountContainer;

        private UpgradeData upgradeData;

        public void Setup(UpgradeData data, int level)
        {
            upgradeData = data;
            icon.sprite = upgradeData.icons[0];
            titleText.text = data.title;
            costText.text = $"{data.GetCostAtLevel(level)}";
            UpdateLevelBar(level);
            Refresh(level);

        }

        public void UpdateLevelBar(int level)
        {
            if (levelBar != null && upgradeData != null && upgradeData.maxLevel > 0)
            {
                levelBar.fillAmount = (float)level / upgradeData.maxLevel;
            }
        }

        public void Refresh(int newLevel)
        {
            if (upgradeData == null) return;

            int purchases = UpgradeManager.Instance.GetPurchaseCount(upgradeData);
            bool isAutomation = upgradeData.isAutomation;

            // Update purchase count UI (only for automation with purchases)
            if (purchaseCountContainer != null)
                purchaseCountContainer.SetActive(isAutomation && purchases > 0);

            if (purchaseCountText != null)
                purchaseCountText.text = $"{purchases}";

            // Update cost display
            double cost = isAutomation
                ? upgradeData.GetCostAtLevel(0, purchases) // ignore level for automation
                : upgradeData.GetCostAtLevel(newLevel);

            costText.text = (newLevel >= upgradeData.maxLevel && !isAutomation)
                ? "MAX"
                : $"{cost} KPI";

            // Update icon
            int iconIndex = isAutomation ? purchases : Mathf.Clamp(newLevel, 0, upgradeData.icons.Length - 1);
            if (upgradeData.icons.Length > 0 && iconIndex < upgradeData.icons.Length)
                icon.sprite = upgradeData.icons[iconIndex];

            // Update level bar only for non-automation upgrades
            if (!isAutomation && levelBar != null && upgradeData.maxLevel > 0)
            {
                levelBar.fillAmount = (float)newLevel / upgradeData.maxLevel;
            }
            else if (isAutomation && levelBar != null)
            {
                levelBar.fillAmount = 0f; // optional: hide or disable bar for automation
            }
        }



        public void OnPointerEnter(PointerEventData eventData)
        {
            UpgradeTooltipDisplay.Instance.Show(upgradeData, GetComponent<RectTransform>());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UpgradeTooltipDisplay.Instance.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            UpgradeManager.Instance.PurchaseUpgrade(upgradeData);
        }
    }

}
