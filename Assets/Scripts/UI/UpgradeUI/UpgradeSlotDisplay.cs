using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Clicker.Upgrades;
using UnityEngine.EventSystems;

namespace Clicker.UI.UpgradeUI
{
    public class UpgradeSlotDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image levelBar;
        [SerializeField] private Image chargeBarFill;

        [Header("Count / Cooldown")]
        [SerializeField] private TextMeshProUGUI purchaseCountText;   
        [SerializeField] private GameObject purchaseCountContainer;   
        [SerializeField] private CanvasGroup slotCanvasGroup;      

        private UpgradeData upgradeData;
        private bool cooldownOverlayActive = false; 

        public void Setup(UpgradeData data, int level)
        {
            upgradeData = data;

            titleText.text = data.title;
            if (upgradeData.icons != null && upgradeData.icons.Length > 0)
                icon.sprite = upgradeData.icons[0];

            costText.text = $"{data.GetCostAtLevel(level)} KPI";
            UpdateLevelBar(level);
            Refresh(level);
        }

        public void UpdateLevelBar(int level)
        {
            if (levelBar == null || upgradeData == null || upgradeData.maxLevel <= 0) return;

            bool isAuto = upgradeData.effectType == UpgradeEffectType.AutoGenerate;
            levelBar.fillAmount = isAuto ? 0f : (float)level / upgradeData.maxLevel;
        }

        public void Refresh(int newLevel)
        {
            if (upgradeData == null) return;

            bool isAuto = upgradeData.effectType == UpgradeEffectType.AutoGenerate;
            int purchases = UpgradeManager.Instance.GetPurchaseCount(upgradeData);

            if (purchaseCountContainer)
                purchaseCountContainer.SetActive(cooldownOverlayActive || (isAuto && purchases > 0));

            if (purchaseCountText && !cooldownOverlayActive)
                purchaseCountText.text = isAuto ? purchases.ToString() : "";

            double cost = isAuto
                ? upgradeData.GetCostAtLevel(0, purchases)
                : upgradeData.GetCostAtLevel(newLevel);

            costText.text = (!isAuto && newLevel >= upgradeData.maxLevel) ? "MAX" : $"{cost} KPI";

            int iconIdxLimit = (upgradeData.icons?.Length ?? 1) - 1;
            int iconIndex = isAuto ? purchases : Mathf.Clamp(newLevel, 0, iconIdxLimit);
            if (upgradeData.icons != null && upgradeData.icons.Length > 0)
                icon.sprite = upgradeData.icons[Mathf.Clamp(iconIndex, 0, iconIdxLimit)];

            if (levelBar)
                levelBar.fillAmount = isAuto ? 0f : (upgradeData.maxLevel > 0 ? (float)newLevel / upgradeData.maxLevel : 0f);
        }

        public void UpdateChargeBar(float normalizedValue)
        {
            if (!chargeBarFill) return;
            chargeBarFill.fillAmount = Mathf.Clamp01(normalizedValue);
            chargeBarFill.color = EvaluateColor(normalizedValue);
        }

        private Color EvaluateColor(float value)
        {
            if (value < 0.5f)
                return Color.Lerp(Color.green, Color.yellow, value * 2f);
            else
                return Color.Lerp(Color.yellow, Color.red, (value - 0.5f) * 2f);
        }

        public void ShowChargeBar(bool show)
        {
            if (chargeBarFill && chargeBarFill.transform.parent)
                chargeBarFill.transform.parent.gameObject.SetActive(show);
        }

        // --- Cooldown overlay ---

        public void BeginCooldownVisuals()
        {
            cooldownOverlayActive = true;
            if (slotCanvasGroup) slotCanvasGroup.alpha = 0.5f;
            if (purchaseCountContainer) purchaseCountContainer.SetActive(true);
        }

        public void SetCooldownSeconds(int seconds)
        {
            if (purchaseCountText) purchaseCountText.text = seconds.ToString();
        }

        public void EndCooldownVisuals()
        {
            cooldownOverlayActive = false;
            if (slotCanvasGroup) slotCanvasGroup.alpha = 1f;

            bool isAuto = upgradeData != null && upgradeData.effectType == UpgradeEffectType.AutoGenerate;
            int purchases = isAuto ? UpgradeManager.Instance.GetPurchaseCount(upgradeData) : 0;

            if (purchaseCountContainer)
                purchaseCountContainer.SetActive(isAuto && purchases > 0);

            if (purchaseCountText)
                purchaseCountText.text = isAuto ? purchases.ToString() : "";
        }

        public void SetCooldownOrPenalty(int cooldownSec, int penaltySec)
        {
            if (!purchaseCountText) return;
            purchaseCountText.richText = true;

            if (penaltySec > 0)                // prioridad: penalty
            {
                purchaseCountText.text = $"<color=#FF4D4D>{penaltySec}</color>";
            }
            else if (cooldownSec > 0)          // si no hay penalty, muestra cooldown
            {
                purchaseCountText.text = cooldownSec.ToString();
            }
            else
            {
                purchaseCountText.text = "";
            }
        }

        // --- Pointer events ---

        public void OnPointerEnter(PointerEventData eventData)
            => UpgradeTooltipDisplay.Instance.Show(upgradeData, GetComponent<RectTransform>());

        public void OnPointerExit(PointerEventData eventData)
            => UpgradeTooltipDisplay.Instance.Hide();

        public void OnPointerClick(PointerEventData eventData)
            => UpgradeManager.Instance.PurchaseUpgrade(upgradeData);
    }
}
