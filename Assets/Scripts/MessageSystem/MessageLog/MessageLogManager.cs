using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageLogManager : MonoBehaviour
{
    public static MessageLogManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private GameObject messageEntryPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject spacerPrefab;                 // must have LayoutElement
    [SerializeField] private RectTransform ghostMessagePlaceholder;   // same width constraints as content

    [Header("Spacing")]
    [SerializeField] private float baseSpacing = 10f;
    [SerializeField] private float diffFactor = 0.25f;
    [SerializeField] private bool dynamicSpacing = true;
    [SerializeField] private float maxExtraSpacing = 14f;
    [SerializeField, Range(0f, 1f)] private float heightSmoothing = 0.35f;

    [Header("Special color")]
    [SerializeField] private Color specialColor = new Color32(128, 0, 0, 255);

    [Header("Capacity")]
    [SerializeField] private int maxMessages = 50;

    [Header("Random messages")]
    [SerializeField] private float messageInterval = 5f;
    [SerializeField, Range(0f, 1f)] private float spawnProbability = 0.3f;

    private readonly Queue<GameObject> activeMessages = new();
    private readonly Queue<GameObject> activeSpacers = new();

    // smoothed recent item height (for homogeneous spacing)
    private bool _hasAvgHeight = false;
    private float _avgHeight = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(RandomMessageCoroutine());
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------- Public API ----------
    public void Post(string text) => AddText(text, null, false);
    public void PostSpecial(string text) => AddText(text, specialColor, true);
    public void AddData(MessageEntryData data) => AddWithSpacer(ui => ui.Setup(data));

    // Safe helpers (optional for callers)
    public static void PostSafe(string t) { if (Instance) Instance.Post(t); }
    public static void PostSpecialSafe(string t) { if (Instance) Instance.PostSpecial(t); }

    // ---------- Random message loop ----------
    private IEnumerator RandomMessageCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(messageInterval);

            if (Random.value <= spawnProbability &&
                MessageLoader.Messages != null &&
                MessageLoader.Messages.Count > 0)
            {
                var entry = MessageLoader.Messages[Random.Range(0, MessageLoader.Messages.Count)];
                AddData(entry);
            }
        }
    }

    // ---------- Unified add path ----------
    private void AddText(string text, Color? color, bool bold)
        => AddWithSpacer(ui => ui.SetText(text, color, bold));

    private void AddWithSpacer(System.Action<MessageEntryUI> binder)
    {
        // measure ghost at SAME width as real column
        var ghostParent = ghostMessagePlaceholder ? ghostMessagePlaceholder : contentParent;
        var ghostGO = Instantiate(messageEntryPrefab, ghostParent);
        var ghostUI = ghostGO.GetComponent<MessageEntryUI>();
        binder(ghostUI);
        Canvas.ForceUpdateCanvases();
        var ghostRT = ghostGO.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(ghostRT);
        float newH = LayoutUtility.GetPreferredHeight(ghostRT);
        Destroy(ghostGO);

        // compute spacing using smoothed height to avoid tall/short oscillations
        float spacing;
        if (!dynamicSpacing || !_hasAvgHeight) spacing = baseSpacing;
        else
        {
            float extra = Mathf.Abs(newH - _avgHeight) * diffFactor;
            spacing = baseSpacing + Mathf.Min(extra, maxExtraSpacing);
        }

        if (HasAnyRealEntry())
            AddSpacer(spacing);

        // add the real entry
        var go = Instantiate(messageEntryPrefab, contentParent);
        var ui = go.GetComponent<MessageEntryUI>();
        binder(ui);

        // update EMA for next spacing calc
        if (!_hasAvgHeight) { _avgHeight = newH; _hasAvgHeight = true; }
        else _avgHeight = Mathf.Lerp(_avgHeight, newH, heightSmoothing);

        EnqueueAndScroll(go);
    }

    private bool HasAnyRealEntry()
    {
        for (int i = 0; i < contentParent.childCount; i++)
            if (contentParent.GetChild(i).GetComponent<MessageEntryUI>() != null)
                return true;
        return false;
    }

    private void AddSpacer(float height)
    {
        var sp = Instantiate(spacerPrefab, contentParent);
        var le = sp.GetComponent<LayoutElement>();
        if (le) le.preferredHeight = height;
        activeSpacers.Enqueue(sp);
    }

    private void EnqueueAndScroll(GameObject go)
    {
        activeMessages.Enqueue(go);

        // trim old
        if (activeMessages.Count > maxMessages)
        {
            if (activeMessages.TryDequeue(out var old)) Destroy(old);
            if (activeSpacers.Count > maxMessages / 2 - 1 && activeSpacers.TryDequeue(out var oldSp))
                Destroy(oldSp);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        if (scrollRect) scrollRect.verticalNormalizedPosition = 0f;
    }
}
