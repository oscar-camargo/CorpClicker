// SaveManager.cs
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    [SerializeField] private string fileName = "save1.json";
    [SerializeField] private float autoSaveDebounce = 1.0f;

    private string Path => System.IO.Path.Combine(Application.persistentDataPath, fileName);
    private Coroutine debounceCo;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(AutoSaveLoop());
        IEnumerator AutoSaveLoop(){ while(true){ yield return new WaitForSecondsRealtime(45f); SaveManager.Instance.SaveNow(); } }

    }

    public void SaveNow()
    {
        var data = BuildSnapshot();
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(Path, json);
    }

    public void RequestAutoSave()
    {
        if (debounceCo != null) StopCoroutine(debounceCo);
        debounceCo = StartCoroutine(DebouncedSave());
    }

    IEnumerator DebouncedSave()
    {
        yield return new WaitForSecondsRealtime(autoSaveDebounce);
        SaveNow();
        debounceCo = null;
    }

    public void LoadIfExists()
    {
        if (!File.Exists(Path)) return;
        var json = File.ReadAllText(Path);
        var data = JsonConvert.DeserializeObject<SaveData>(json);
        ApplySnapshot(data);
    }

    // --- Snapshot build/apply ---

    private SaveData BuildSnapshot()
    {
        var gs = Clicker.Core.GameState.Instance;
        var ps = Clicker.PlayerStats.PlayerStatsManager.Instance;
        var um = Clicker.Upgrades.UpgradeManager.Instance;

        var s = new SaveData
        {
            savedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            currentKPI = gs?.GetKPI() ?? 0,
            allTimeKPI = gs?.GetAllTimeKPI() ?? 0,
            morale = ps ? ps.morale : 50,
            reputation = ps ? ps.reputation : 50,
        };

        if (um != null)
        {
            foreach (var u in um.GetAllUpgrades())
            {
                s.upgrades.Add(new UpgradeState {
                    id = u.Id,
                    level = um.GetUpgradeLevel(u),
                    purchases = um.GetPurchaseCount(u)
                });
            }
        }

        return s;
    }

    private void ApplySnapshot(SaveData s)
    {
        var gs = Clicker.Core.GameState.Instance;
        var ps = Clicker.PlayerStats.PlayerStatsManager.Instance;
        var um = Clicker.Upgrades.UpgradeManager.Instance;

        // Order matters: restore stats first (rep gating), then upgrades, then KPI.

        if (ps != null)
        {
            ps.SetMorale(s.morale);
            ps.SetReputation(s.reputation);
        }

        if (um != null && s.upgrades != null)
        {
            // Map by Id
            var byId = new System.Collections.Generic.Dictionary<string, Clicker.Upgrades.UpgradeData>();
            foreach (var u in um.GetAllUpgrades()) byId[u.Id] = u;

            // Clear current state
            um.ClearAllUpgradeState(); // add this helper in UpgradeManager (sets all levels/purchases to 0 & refreshes UI)

            // Apply saved
            foreach (var st in s.upgrades)
            {
                if (!byId.TryGetValue(st.id, out var data)) continue;

                um.ForceSetUpgradeLevel(data, st.level);       // direct setter (no cost, no gating)
                um.ForceSetPurchaseCount(data, st.purchases);  // direct setter
            }

            // Enforce gates in case old saves exceed new rules
            um.EnforceReputationCaps(ps != null ? ps.reputation : 0);
            um.ForceRefreshAll();
        }

        if (gs != null)
        {
            gs.SetKPI(s.currentKPI, s.allTimeKPI); // add setter to GameState to avoid OnKPIChanged spam
        }
    }
}
