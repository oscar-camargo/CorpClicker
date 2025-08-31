using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MessageLogManager : MonoBehaviour
{
    public static MessageLogManager Instance { get; private set; }
    private void Awake() => Instance = this;

    [Header("UI References")]
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private GameObject messageEntryPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject spacerPrefab;
    [SerializeField] private GameObject ghostMessagePlaceholder;

    [Header("Settings")]
    [SerializeField] private float messageInterval = 5f;
    [SerializeField, Range(0f, 1f)] private float spawnProbability = 0.3f;
    [SerializeField] private int maxMessages = 50;

    [Header("Special message color")]
    [SerializeField] private Color specialColor = new Color32(128, 0, 0, 255); // maroon

    private readonly Queue<GameObject> activeMessages = new Queue<GameObject>();
    private readonly Queue<GameObject> activeSpacers = new Queue<GameObject>();

    private void Start()
    {
        StartCoroutine(RandomMessageCoroutine());
    }

    public void Post(string text) //CURRENTLY NOT USED, BUT CAN BE USED LATER
    {
        var go = Instantiate(messageEntryPrefab, contentParent);
        go.GetComponent<MessageEntryUI>()?.SetText(text);
        EnqueueAndScroll(go);
    }

    public void PostSpecial(string text)
    {
        var go = Instantiate(messageEntryPrefab, contentParent);
        go.GetComponent<MessageEntryUI>()?.SetText(text, specialColor, true);
        EnqueueAndScroll(go);
    }

    private IEnumerator RandomMessageCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(messageInterval);

            if (Random.value <= spawnProbability && MessageLoader.Messages.Count > 0)
            {
                float previousHeight = 0f;
                var entry = MessageLoader.Messages[Random.Range(0, MessageLoader.Messages.Count)];

                if (contentParent.childCount > 0)
                {
                    Transform lastChild = contentParent.GetChild(contentParent.childCount - 1);
                    LayoutElement lastLayout = lastChild.GetComponent<LayoutElement>();
                    if (lastLayout != null)
                        previousHeight = lastChild.GetComponent<RectTransform>().rect.height;

                    GameObject ghostEntry = AddMessage(entry, ghostMessagePlaceholder.transform);
                    float newHeight = ghostEntry.GetComponent<RectTransform>().rect.height;
                    Destroy(ghostEntry);

                    float spacingNeeded = Mathf.Max(16f, Mathf.Abs(previousHeight - newHeight) * 0.5f); // Adjustable
                    GameObject spacer = Instantiate(spacerPrefab, contentParent);
                    spacer.GetComponent<LayoutElement>().preferredHeight = spacingNeeded;
                    activeSpacers.Enqueue(spacer);

                    AddMessage(entry, contentParent.transform);

                }

                else
                {
                    AddMessage(entry, contentParent.transform);
                }
            }
        }
    }

    public GameObject AddMessage(MessageEntryData entryData, Transform parent)
    {
        GameObject newEntry = Instantiate(messageEntryPrefab, parent);
        var entry = newEntry.GetComponent<MessageEntryUI>();
        if (entry != null)
            entry.Setup(entryData);

        activeMessages.Enqueue(newEntry);



        if (activeMessages.Count > maxMessages)
        {
            Destroy(activeMessages.Dequeue());
            if (activeSpacers.Count > maxMessages / 2 - 1)
                Destroy(activeSpacers.Dequeue());
        }

        ScrollToBottom();

        return newEntry;
    }

    private void EnqueueAndScroll(GameObject go)
    {
        activeMessages.Enqueue(go);
        if (activeMessages.Count > maxMessages)
        {
            Destroy(activeMessages.Dequeue());
            if (activeSpacers.Count > maxMessages / 2 - 1)
                Destroy(activeSpacers.Dequeue());
        }
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        scrollRect.verticalNormalizedPosition = 0f;
    }

}
