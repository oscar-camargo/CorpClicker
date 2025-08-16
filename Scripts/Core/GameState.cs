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
    }
}
