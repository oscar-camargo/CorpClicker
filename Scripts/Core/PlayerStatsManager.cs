using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // you installed DOTween
using System;
using Clicker.Core;

namespace Clicker.PlayerStats
{

    public class PlayerStatsManager : MonoBehaviour
    {
        public static PlayerStatsManager Instance { get; private set; }

        [Header("UI (Image Type = Filled)")]
        [SerializeField] private Image moraleFill;      // set Image.type=Filled, FillMethod=Horizontal
        [SerializeField] private Image reputationFill;  // set Image.type=Filled, FillMethod=Horizontal
        [SerializeField] private float barTween = 0.25f; // 0 = instant

        [Header("Stats 0..100")]
        [Range(0,100)] public int morale = 50;
        [Range(0,100)] public int reputation = 50;

        [Header("Morale → KPI Multiplier")]
        public float moraleMultAt0 = 0.8f;
        public float moraleMultAt100 = 1.2f;
        public AnimationCurve moraleCurve = AnimationCurve.Linear(0,0, 1,1);

        [Header("Reputation thresholds for level-based upgrades")]
        // Passing each threshold unlocks the NEXT level (base = 1).
        // Example [30,60,90]: rep 45 ⇒ passed 30 only ⇒ max allowed level = 2
        public int[] levelThresholds = new int[] { 30, 60, 90 };

        [Header("Automation cap by reputation (linear)")]
        public int autoCapAt0 = 1;
        public int autoCapAt100 = 20;

        // Events if other systems want to react
        public event Action<int> OnMoraleChanged;
        public event Action<int> OnReputationChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start() => RefreshUI(instant:true);

        // --- Public API (used by mail responses, etc.) ---
        public void AddMorale(int delta)      => SetMorale(morale + delta);
        public void AddReputation(int delta)  => SetReputation(reputation + delta);

        public void SetMorale(int value)
        {
            int c = Mathf.Clamp(value, 0, 100);
            if (c == morale) return;
            morale = c;
            OnMoraleChanged?.Invoke(morale);
            RefreshUI(instant:false);
        }

        public void SetReputation(int value)
        {
            int c = Mathf.Clamp(value, 0, 100);
            if (c == reputation) return;
            reputation = c;
            OnReputationChanged?.Invoke(reputation);
            RefreshUI(instant:false);
        }

        // Morale buff for any KPI source (click/auto)
        public float GetMoraleKpiMultiplier()
        {
            float t = morale / 100f;
            float shaped = moraleCurve != null ? Mathf.Clamp01(moraleCurve.Evaluate(t)) : t;
            return Mathf.Lerp(moraleMultAt0, moraleMultAt100, shaped);
        }

        // Level-gate for “level-based” upgrades (click upgrades, etc.)
        public int GetMaxAllowedUpgradeLevel()
        {
            int passed = 0;
            for (int i = 0; i < levelThresholds.Length; i++)
                if (reputation >= levelThresholds[i]) passed++;
            return 1 + passed; // base level 1 + thresholds passed
        }

        public bool CanIncreaseToNextLevel(int currentLevel)
        {
            int next = currentLevel + 1;
            return next <= GetMaxAllowedUpgradeLevel();
        }

        // Automation units allowed given current reputation (linear; tweak as needed)
        public int GetAutomationCap()
        {
            float t = reputation / 100f;
            return Mathf.FloorToInt(Mathf.Lerp(autoCapAt0, autoCapAt100, t));
        }

        // Normalized accessors if you need them elsewhere
        public float Morale01 => morale / 100f;
        public float Reputation01 => reputation / 100f;

        // --- UI update ---
        private void RefreshUI(bool instant)
        {
            float m = morale / 100f;
            float r = reputation / 100f;

            if (moraleFill)
            {
                if (instant || barTween <= 0f) moraleFill.fillAmount = m;
                else moraleFill.DOFillAmount(m, barTween).SetUpdate(true);
            }

            if (reputationFill)
            {
                if (instant || barTween <= 0f) reputationFill.fillAmount = r;
                else reputationFill.DOFillAmount(r, barTween).SetUpdate(true);
            }
        }
    }
}