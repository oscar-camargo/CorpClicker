using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clicker.Upgrades;
using Clicker.UI.UpgradeUI;
using Clicker.Core;
using Clicker.PlayerStats;

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
        private Dictionary<UpgradeData, UpgradeSlotDisplay> upgradeDisplayMap = new();
        private Dictionary<UpgradeData, int> purchaseCounts = new();

        // Push-your-luck state
        private int pushClicks = 0;          // 0..config.maxClicks
        private bool pushActive = false;
        private float pushLockUntil = 0f;

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
                bool isPush = upgrade.effectType == UpgradeEffectType.PushYourLuck;
                ui.ShowChargeBar(isPush);

                upgradeDisplayMap[upgrade] = ui;
            }

            StartCoroutine(AutoGenerateKPICoroutine());
        }

        private void Update()
        {
            // Handle Push-Your-Luck hotkey
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade.effectType != UpgradeEffectType.PushYourLuck) continue;
                if (upgradeLevels[upgrade] <= 0) continue;
                if (Time.time < pushLockUntil || pushActive) continue;

                bool pressed = Input.GetKeyDown(upgrade.activationKey)
                               && (!upgrade.requiresShift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                               && (!upgrade.requiresCtrl  || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

                if (pressed)
                    TryActivatePushLuck(upgrade);
            }

            var pu = GetPushUpgrade();
            if (pu != null && pu.pushLuckConfig != null &&
                upgradeDisplayMap.TryGetValue(pu, out var ui))
            {
                float fill = Mathf.Clamp01((float)pushClicks / pu.pushLuckConfig.maxClicks);
                ui.UpdateChargeBar(fill);
            }
        }

        public void OnPlayerClick()
        {
            // Charge Push-Your-Luck on any player click
            var pu = GetPushUpgrade();
            if (pu != null && upgradeLevels[pu] > 0 && !pushActive && Time.time >= pushLockUntil && pu.pushLuckConfig != null)
            {
                pushClicks = Mathf.Min(pu.pushLuckConfig.maxClicks, pushClicks + 1);
            }
        }

        public void PurchaseUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null) return;

            if (!upgradeLevels.ContainsKey(upgrade))  upgradeLevels[upgrade] = 0;
            if (!purchaseCounts.ContainsKey(upgrade)) purchaseCounts[upgrade] = 0;

            int currentLevel = upgradeLevels[upgrade];
            int purchases    = purchaseCounts[upgrade];

            var stats = PlayerStatsManager.Instance;
            if (stats != null)
            {
                bool isAuto = upgrade.effectType == UpgradeEffectType.AutoGenerate;

                if (isAuto)
                {
                    int cap = stats.GetAutomationCap();
                    if (purchases >= cap)
                    {
                        Debug.Log($"Blocked: automation cap {cap} at reputation {stats.reputation}.");
                        return;
                    }
                }
                else // FlatPerClick or PushYourLuck are level-based
                {
                    if (currentLevel < upgrade.maxLevel &&
                        !stats.CanIncreaseToNextLevel(currentLevel))
                    {
                        int needed = stats.RequiredReputationForLevel(currentLevel + 1);
                        Debug.Log($"Blocked: need reputation >= {needed} for level {currentLevel + 1}.");
                        return;
                    }
                }
            }

            double cost = (upgrade.effectType == UpgradeEffectType.AutoGenerate)
                ? upgrade.GetCostAtLevel(0, purchases)
                : upgrade.GetCostAtLevel(currentLevel);

            if (!GameState.Instance.HasEnoughKPI(cost)) return;

            GameState.Instance.SpendKPI(cost);
            purchaseCounts[upgrade]++;

            if (upgrade.effectType != UpgradeEffectType.AutoGenerate && currentLevel < upgrade.maxLevel)
                upgradeLevels[upgrade] = currentLevel + 1;

            if (upgradeDisplayMap.TryGetValue(upgrade, out var ui))
                ui.Refresh(upgradeLevels[upgrade]);
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
                        double kpiPerUnit = upgrade.GetEffectAtLevel(0);
                        GameState.Instance.AddKPIFromAuto(kpiPerUnit * count); // use auto path
                    }
                }
            }
        }

        // ---------- Push-Your-Luck ----------

        private UpgradeData GetPushUpgrade()
        {
            foreach (var u in allUpgrades)
                if (u.effectType == UpgradeEffectType.PushYourLuck)
                    return u;
            return null;
        }

        private void TryActivatePushLuck(UpgradeData pu)
        {
            var cfg = pu.pushLuckConfig;
            if (cfg == null) { Debug.LogWarning("PushLuckConfig not assigned on upgrade."); return; }
            if (pushClicks < cfg.tier1Min) return;

            int lvl = Mathf.Clamp(GetUpgradeLevel(pu), 1, Mathf.Min(3, pu.maxLevel));

            float baseDur = cfg.baseDurationByLevel[Mathf.Clamp(lvl - 1, 0, cfg.baseDurationByLevel.Length - 1)];
            float moraleT = PlayerStatsManager.Instance ? PlayerStatsManager.Instance.Morale01 : 0.5f;
            float dur = baseDur * Mathf.Lerp(cfg.moraleDurationMultRange.x, cfg.moraleDurationMultRange.y, moraleT);

            int c = pushClicks;
            double flat = 0.0;
            double auto = 1.0f;
            float failChance = 0f;
            FailType fail = FailType.None;

            if (c >= cfg.tier1Min && c < cfg.tier2Min)
            {
                flat = cfg.tier1FlatPerClick;
                auto = cfg.tier1AutoMult;
            }
            else if (c >= cfg.tier2Min && c < cfg.tier3Min)
            {
                flat = cfg.tier2FlatPerClick;
                auto = cfg.tier2AutoMult;
                float t = Mathf.InverseLerp(cfg.tier2Min, cfg.tier3Min, c);
                failChance = Mathf.Clamp01(cfg.tier2FailCurve.Evaluate(t));
                fail = FailType.BlockClicks;
            }
            else // tier3..max
            {
                flat = cfg.tier3FlatPerClick;
                auto = cfg.tier3AutoMult;
                float t = Mathf.InverseLerp(cfg.tier3Min, cfg.maxClicks, c);
                failChance = Mathf.Clamp01(cfg.tier3FailCurve.Evaluate(t));
                fail = FailType.BlockAll;
            }

            bool failed = (failChance > 0f) && (Random.value < failChance);

            if (failed)
            {
                if (fail == FailType.BlockClicks) GameState.Instance.BlockClicks(cfg.blockClicksSeconds);
                else                              GameState.Instance.BlockAllKPI(cfg.blockAllSeconds);

                float lockSecs = cfg.lockMinutesByLevel[Mathf.Clamp(lvl - 1, 0, cfg.lockMinutesByLevel.Length - 1)] * 60f;
                pushLockUntil = Time.time + lockSecs;
                StartCoroutine(CooldownUIRoutine(pu, lockSecs));
                ResetPushState();
                return;
            }

            if (upgradeDisplayMap.TryGetValue(pu, out var uiClear))
            uiClear.UpdateChargeBar(0f);
            pushClicks = 0;

            StartCoroutine(PushLuckSuccessRoutine(pu, flat, auto, dur));
        }

        private IEnumerator PushLuckSuccessRoutine(UpgradeData pu, double flatPerClick, double autoMult, float duration)
        {
            pushActive = true;

            double prevFlat = GameState.Instance.TempFlatKpiPerClick;
            double prevAuto = GameState.Instance.TempAutomationMultiplier;

            GameState.Instance.TempFlatKpiPerClick = prevFlat + flatPerClick;
            GameState.Instance.TempAutomationMultiplier = prevAuto * autoMult;

            if (upgradeDisplayMap.TryGetValue(pu, out var ui))
                ui.BeginCooldownVisuals();

            float end = Time.time + duration;
            while (Time.time < end)
            {
                int cd = Mathf.CeilToInt(end - Time.time);
                int penalty = GameState.Instance ? GameState.Instance.SecondsUntilKpiUnblocked() : 0;
                if (upgradeDisplayMap.TryGetValue(pu, out var ui2))
                    ui2.SetCooldownOrPenalty(cd, penalty);
                yield return null;
            }

            GameState.Instance.TempFlatKpiPerClick = prevFlat;
            GameState.Instance.TempAutomationMultiplier = prevAuto;

            if (upgradeDisplayMap.TryGetValue(pu, out var ui3))
                ui3.EndCooldownVisuals();

            pushActive = false;
            ResetPushState();
        }


        private IEnumerator CooldownUIRoutine(UpgradeData pu, float lockDuration)
        {
            if (!upgradeDisplayMap.TryGetValue(pu, out var ui)) yield break;

            ui.BeginCooldownVisuals();

            float lockEnd = Time.time + lockDuration;

            while (true)
            {
                int cd = Mathf.CeilToInt(lockEnd - Time.time);
                int penalty = GameState.Instance ? GameState.Instance.SecondsUntilKpiUnblocked() : 0;

                ui.SetCooldownOrPenalty(cd, penalty);

                if (cd <= 0 && penalty <= 0) break;
                yield return null;
            }

            ui.EndCooldownVisuals();
        }



        private enum FailType { None, BlockClicks, BlockAll }

        // ---------- Helpers ----------

        public int GetUpgradeLevel(UpgradeData upgrade)
        {
            return upgradeLevels.ContainsKey(upgrade) ? upgradeLevels[upgrade] : 0;
        }

        public double GetClickMultiplier()
        {
            // No multiplicative click upgrades in this stage; keep 1.0 for compatibility.
            return 1.0;
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
                upgradeDisplayMap[upgrade].Refresh(upgradeLevels[upgrade]);
            }
        }

        public int GetPurchaseCount(UpgradeData upgrade)
        {
            return purchaseCounts.ContainsKey(upgrade) ? purchaseCounts[upgrade] : 0;
        }

        public float GetPushCharge01()
        {
            var pu = GetPushUpgrade();
            if (pu == null || pushClicks <= 0 || pu.pushLuckConfig == null) return 0f;
            return Mathf.Clamp01((float)pushClicks / pu.pushLuckConfig.maxClicks);
        }

        public bool IsPushCooldownActive()
        {
            var pu = GetPushUpgrade();
            return pu != null && Time.time < pushLockUntil;
        }

        private void ResetPushState()
        {
            pushClicks = 0;
            var pu = GetPushUpgrade();
            if (pu != null && upgradeDisplayMap.TryGetValue(pu, out var ui))
                ui.UpdateChargeBar(0f);
        }
    }
}