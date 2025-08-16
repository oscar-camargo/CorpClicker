using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clicker.Upgrades;
using Clicker.UI.UpgradeUI;
using Clicker.Core;

namespace Clicker.Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [SerializeField] private List<UpgradeData> allUpgrades;
        [SerializeField] private UpgradeSlotDisplay upgradeUIPrefab;
        [SerializeField] private Transform upgradeUIParent;
        [SerializeField] private float autoGenerationInterval = 1f;

        private Dictionary<UpgradeData, int> upgradeLevels = new();
        private Dictionary<UpgradeData, Coroutine> activeBoosts = new();
        private Dictionary<UpgradeData, UpgradeSlotDisplay> upgradeDisplayMap = new();
        private Dictionary<UpgradeData, int> purchaseCounts = new();

        private float idleTimer = 0f;
        private bool idleBoostReady = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            foreach (var upgrade in allUpgrades)
            {
                upgradeLevels[upgrade] = 0;

                var ui = Instantiate(upgradeUIPrefab, upgradeUIParent).GetComponent<UpgradeSlotDisplay>();
                ui.Setup(upgrade, 0);

                upgradeDisplayMap[upgrade] = ui;
            }

            StartCoroutine(AutoGenerateKPICoroutine());
        }


        private void Update()
        {
            idleTimer += Time.deltaTime;

            foreach (var upgrade in allUpgrades)
            {
                if (upgrade.title.ToLower().Contains("idle") && upgradeLevels[upgrade] > 0 && idleTimer >= 3f)
                    idleBoostReady = true;

                if (upgrade.title.ToLower().Contains("temporary") && upgradeLevels[upgrade] > 0 &&
                    Input.GetKeyDown(upgrade.activationKey) &&
                    (!upgrade.requiresShift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                    (!upgrade.requiresCtrl || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                {
                    TriggerTemporaryBoost(upgrade);
                }
            }
        }

        public void OnPlayerClick()
        {
            if (idleBoostReady)
            {
                foreach (var upgrade in allUpgrades)
                {
                    if (upgrade.title.ToLower().Contains("idle") && upgradeLevels[upgrade] > 0)
                    {
                        TriggerTemporaryBoost(upgrade);
                        idleBoostReady = false;
                        idleTimer = 0f;
                        break;
                    }
                }
            }
            else idleTimer = 0f;
        }

        public void PurchaseUpgrade(UpgradeData upgrade)
        {
            if (!upgradeLevels.ContainsKey(upgrade))
                upgradeLevels[upgrade] = 0;

            if (!purchaseCounts.ContainsKey(upgrade))
                purchaseCounts[upgrade] = 0;

            int purchases = purchaseCounts[upgrade];

            double cost = upgrade.isAutomation
                ? upgrade.GetCostAtLevel(0, purchases) // level not used
                : upgrade.GetCostAtLevel(upgradeLevels[upgrade]);

            if (!GameState.Instance.HasEnoughKPI(cost)) return;

            GameState.Instance.SpendKPI(cost);
            purchaseCounts[upgrade]++;

            if (!upgrade.isAutomation && upgradeLevels[upgrade] < upgrade.maxLevel)
                upgradeLevels[upgrade]++;

            upgradeDisplayMap[upgrade].Refresh(upgradeLevels[upgrade]);
        }


        private void TriggerTemporaryBoost(UpgradeData upgrade)
        {
            if (activeBoosts.ContainsKey(upgrade)) return;
            Coroutine c = StartCoroutine(HandleTemporaryBoost(upgrade));
            activeBoosts[upgrade] = c;
        }

        private IEnumerator HandleTemporaryBoost(UpgradeData upgrade)
        {
            float duration = upgrade.boostDuration;
            double originalMultiplier = GameState.Instance.TemporaryKpiMultiplier;
            GameState.Instance.TemporaryKpiMultiplier *= upgrade.GetEffectAtLevel(upgradeLevels[upgrade]);

            yield return new WaitForSeconds(duration);

            GameState.Instance.TemporaryKpiMultiplier = originalMultiplier;
            activeBoosts.Remove(upgrade);
        }

        private IEnumerator AutoGenerateKPICoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoGenerationInterval);
                foreach (var upgrade in allUpgrades)
                {
                    if (upgrade.effectType == UpgradeEffectType.AutoGenerate && purchaseCounts.ContainsKey(upgrade))
                    {
                        int count = purchaseCounts[upgrade];
                        double kpiPerUnit = upgrade.GetEffectAtLevel(0); // always use index 0
                        GameState.Instance.AddKPI(kpiPerUnit * count);
                    }
                }
            }
        }

        public int GetUpgradeLevel(UpgradeData upgrade)
        {
            return upgradeLevels.ContainsKey(upgrade) ? upgradeLevels[upgrade] : 0;
        }

        public double GetClickMultiplier()
        {
            double multiplier = 1.0;
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade.effectType == UpgradeEffectType.ClickMultiplier)
                    multiplier += upgrade.GetEffectAtLevel(GetUpgradeLevel(upgrade)) - 1.0;
            }
            return multiplier;
        }

        public double GetFlatBonusPerClick()
        {
            double bonus = 0.0;
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade.effectType == UpgradeEffectType.FlatPerClick)
                        bonus += upgrade.GetEffectAtLevel(GetUpgradeLevel(upgrade));
            }
            return bonus;
        }

        private void OnEnable()
        {
            GameState.Instance.OnKPIChanged += RefreshAllButtons;
        }

        private void OnDisable()
        {
            if (GameState.Instance != null)
                GameState.Instance.OnKPIChanged -= RefreshAllButtons;
        }

        private void RefreshAllButtons(double currentKPI)
        {
            foreach (var upgrade in allUpgrades)
            {
                if (!upgradeLevels.ContainsKey(upgrade)) continue;

                int level = upgradeLevels[upgrade];
                upgradeDisplayMap[upgrade].Refresh(level);
            }
        }

        public int GetPurchaseCount(UpgradeData upgrade)
        {
            return purchaseCounts.ContainsKey(upgrade) ? purchaseCounts[upgrade] : 0;
        }
    }
}
