using UnityEngine;
using System;
using System.Collections;
using Clicker.PlayerStats;

namespace Clicker.Core
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        private double currentKPI = 0;

        public double TemporaryKpiMultiplier { get; set; } = 1.0;

        private double allTimeKPI = 0; //Keeps track of KPI produced so far since game started

        public double GlobalKpiMultiplier { get; private set; } = 1.0;
        Coroutine rushCo;

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
            DontDestroyOnLoad(gameObject);
        }

        public void AddKPI(double baseAmount)
        {
            float moraleMult = 1f;
            var stats = PlayerStatsManager.Instance;
            if (stats != null)
                moraleMult = stats.GetMoraleKpiMultiplier();

            double final = baseAmount * TemporaryKpiMultiplier * moraleMult;
            currentKPI += final;
            allTimeKPI += final;
            OnKPIChanged?.Invoke(currentKPI);
        }

        public double GetKPI() => currentKPI;

        public double GetAllTimeKPI() => allTimeKPI;

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

            double final = (baseAmount + TempFlatKpiPerClick) * moraleMult * TemporaryKpiMultiplier * GlobalKpiMultiplier;
            currentKPI += final;
            allTimeKPI += final;
            OnKPIChanged?.Invoke(currentKPI);
        }


        public void AddKPIFromAuto(double baseAmount)
        {
            if (Time.time < allBlockedUntil) return;

            float moraleMult = 1f;
            var stats = PlayerStatsManager.Instance;
            if (stats != null) moraleMult = stats.GetMoraleKpiMultiplier();

            double final = baseAmount * moraleMult * TempAutomationMultiplier * GlobalKpiMultiplier;
            currentKPI += final;
            allTimeKPI += final;
            OnKPIChanged?.Invoke(currentKPI);
        }

        public int SecondsUntilKpiUnblocked()
        {
            float remaining = Mathf.Max(allBlockedUntil, clicksBlockedUntil) - Time.time;
            return remaining > 0f ? Mathf.CeilToInt(remaining) : 0;
        }

        public void SetKPI(double current, double allTime)
        {
            currentKPI = current;
            allTimeKPI = allTime;
            OnKPIChanged?.Invoke(currentKPI);
        }

        public void StartGlobalKpiRush(float mult, float duration)
        {
            if (rushCo != null) StopCoroutine(rushCo);
            rushCo = StartCoroutine(RunRush(mult, duration));
        }

        private IEnumerator RunRush(float mult, float dur)
        {
            GlobalKpiMultiplier = mult;
            yield return new WaitForSeconds(dur);
            GlobalKpiMultiplier = 1.0;
            rushCo = null;
        }

    }
}
