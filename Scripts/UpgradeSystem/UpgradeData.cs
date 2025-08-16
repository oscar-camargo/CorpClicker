using UnityEngine;
using Clicker.Core;



public enum UpgradeEffectType
    {
        FlatPerClick,
        ClickMultiplier,
        AutoGenerate,
        TemporaryBoost,
        IdleTrigger
    }
namespace Clicker.Upgrades
{
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Clicker/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        public Sprite[] icons; // Indexed by level (0 = level 1)
        public string title;

        [TextArea]
        public string Description;

        public int maxLevel = 3;

        [Header("Upgrade Type")]
        public UpgradeEffectType effectType;

        [Header("Upgrade Costs")]
        public double[] costPerLevel;

        [Header("Effects")]
        public double[] effectPerLevel;

        [Header("Tooltip Info")]
        [TextArea]
        public string effectDescription;

        [TextArea]
        public string[] flavorTexts; // One per level (0 = level 1, etc.)

        [Header("Temporary Boost Settings")]
        public KeyCode activationKey;
        public bool requiresShift;
        public bool requiresCtrl;
        public float boostDuration = 15f;

        [Header("Automation Upgrade Settings")]
        public bool isAutomation;
        public double baseCost; // Only used for automation
        public double costGrowthFactor = 1.15;


        public double GetCostAtLevel(int level, int purchases = 0)
        {
            if (isAutomation)
            {
                return System.Math.Round(baseCost * System.Math.Pow(costGrowthFactor, purchases));
            }

            return level < costPerLevel.Length ? costPerLevel[level] : costPerLevel[^1];
        }


        public double GetEffectAtLevel(int level) =>
            level < effectPerLevel.Length ? effectPerLevel[level] : effectPerLevel[^1];
    }
}


