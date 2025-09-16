using UnityEngine;
using Clicker.Core;



public enum UpgradeEffectType
{
    FlatPerClick,
    AutoGenerate,
    PushYourLuck
}

namespace Clicker.Upgrades
{
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Clicker/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Visuals")]
        public Sprite[] icons; // index by level (0 = level 1)

        [Header("Identity")]
        public string title;
        [TextArea] public string Description;
        public int maxLevel = 3;

        [Header("Type")]
        public UpgradeEffectType effectType;

        [Header("Costs")]
        public double[] costPerLevel;         // used for FlatPerClick & PushYourLuck
        public double baseCost;               // used only for AutoGenerate
        public double costGrowthFactor = 1.15;// used only for AutoGenerate (per unit)

        [Header("Effects (generic)")]
        // FlatPerClick: effectPerLevel = +KPI per click
        // AutoGenerate: effectPerLevel[0] = KPI per unit per tick (or per second) (read at level 0 as you do)
        // PushYourLuck: not required but kept for future extensibility
        public double[] effectPerLevel;

        [Header("Tooltip")]
        [TextArea] public string effectDescription;
        [TextArea] public string[] flavorTexts; // one per level (0 = level 1)

        // ---- Push-Your-Luck specific ----
        [Header("Push-Your-Luck (Hotkey + Config)")]
        public KeyCode activationKey = KeyCode.R;
        public bool requiresShift = false;
        public bool requiresCtrl  = false;
        public PushLuckConfig pushLuckConfig; // assign the SO

        // --- Upgrade ID for save system --- //
        [Header("ID")]
        [SerializeField] public string upgradeID;

        // ---------- API ----------
        public double GetCostAtLevel(int level, int purchases = 0)
        {
            if (effectType == UpgradeEffectType.AutoGenerate)
                return System.Math.Round(baseCost * System.Math.Pow(costGrowthFactor, purchases));

            return level < costPerLevel.Length ? costPerLevel[level] : costPerLevel[^1];
        }

        public double GetEffectAtLevel(int level) =>
            level < effectPerLevel.Length ? effectPerLevel[level] : effectPerLevel[^1];
    }
}
