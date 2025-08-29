using UnityEngine;
using System;
using Clicker.PlayerStats;

namespace Clicker.Core
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        private double currentKPI = 0;

        public double TemporaryKpiMultiplier { get; set; } = 1.0;

        public event Action<double> OnKPIChanged;

        public double TempFlatKpiPerClick { get; set; } = 0.0;
        public double TempAutomationMultiplier { get; set; } = 1.0;
        private float clicksBlockedUntil = 0f;
        private float allBlockedUntil    = 0f;

        public void BlockClicks(float seconds) => clicksBlockedUntil = Mathf.Max(clicksBlockedUntil, Time.time + seconds);
        public void BlockAllKPI(float seconds) => allBlockedUntil    = Mathf.Max(allBlockedUntil, Time.time + seconds);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void AddKPI(double baseAmount)
        {
            float moraleMult = 1f;
            var stats = PlayerStatsManager.Instance;
            if (stats != null)
                moraleMult = stats.GetMoraleKpiMultiplier();

            double final = baseAmount * TemporaryKpiMultiplier * moraleMult;
            currentKPI += final;
            OnKPIChanged?.Invoke(currentKPI);
        }

        public double GetKPI() => currentKPI;

        public bool HasEnoughKPI(double amount) => currentKPI >= amount;

        public void SpendKPI(double amount)
        {
            currentKPI = Math.Max(0, currentKPI - amount);
            OnKPIChanged?.Invoke(currentKPI);
        }

        public void AddKPIFromClick(double baseAmount)
        {
            if (Time.time < allBlockedUntil || Time.time < clicksBlockedUntil) return;

            float moraleMult = 1f;
            var stats = PlayerStatsManager.Instance;
            if (stats != null) moraleMult = stats.GetMoraleKpiMultiplier();

            double final = (baseAmount + TempFlatKpiPerClick) * moraleMult * TemporaryKpiMultiplier;
            currentKPI += final;
            OnKPIChanged?.Invoke(currentKPI);
        }


        public void AddKPIFromAuto(double baseAmount)
        {
            if (Time.time < allBlockedUntil) return;

            float moraleMult = 1f;
            var stats = PlayerStatsManager.Instance;
            if (stats != null) moraleMult = stats.GetMoraleKpiMultiplier();

            double final = baseAmount * moraleMult * TempAutomationMultiplier;
            currentKPI += final;
            OnKPIChanged?.Invoke(currentKPI);
        }

        public int SecondsUntilKpiUnblocked()
        {
            float remaining = Mathf.Max(allBlockedUntil, clicksBlockedUntil) - Time.time;
            return remaining > 0f ? Mathf.CeilToInt(remaining) : 0;
        }
    }
}
