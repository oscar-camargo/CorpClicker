using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MailManager : MonoBehaviour
{
    [Header("Spawner")]
    public float checkInterval = 10f;
    [Range(0f, 1f)] public float spawnChance = 0.4f;
    public int maxMails = 4;
    public List<MailData> mailPool;

    [Header("UI")]
    public List<MailSlot> mailSlots;
    public GameObject mailPrefab;
    public Transform mailDisplayArea;
    public Text noMailText; // optional: hook if you use it

    [Header("Expiry")]
    [SerializeField] private float mailExpireSeconds = 60f;

    private readonly List<MailData> activeMails = new();
    private readonly Dictionary<MailSlot, Coroutine> expiryBySlot = new();
    private readonly Dictionary<MailSlot, MailData> mailBySlot   = new();

    private MailSlot currentOpenSlot;

    void Start()
    {
        foreach (var slot in mailSlots) slot.gameObject.SetActive(false);
        InvokeRepeating(nameof(TryAddRandomMail), checkInterval, checkInterval);
        UpdateNoMailText();
    }

    public void TryAddRandomMail()
    {
        if (activeMails.Count >= maxMails) return;
        if (Random.value > spawnChance) return;

        var newMail = mailPool[Random.Range(0, mailPool.Count)];
        activeMails.Add(newMail);

        // find first hidden slot
        foreach (var slot in mailSlots)
        {
            if (!slot.gameObject.activeSelf)
            {
                slot.gameObject.SetActive(true);
                slot.Setup(newMail, this);
                mailBySlot[slot] = newMail;

                // start expiry timer for this slot
                if (expiryBySlot.TryGetValue(slot, out var running))
                    StopCoroutine(running);
                expiryBySlot[slot] = StartCoroutine(ExpireAfter(slot, mailExpireSeconds));

                UpdateNoMailText();
                break;
            }
        }
    }

    public void DisplayMail(MailData mailData, MailSlot sourceSlot)
    {
        currentOpenSlot = sourceSlot;

        // clear previous
        foreach (Transform child in mailDisplayArea) Destroy(child.gameObject);

        // spawn and populate
        var mailGO = Instantiate(mailPrefab, mailDisplayArea);
        mailGO.GetComponent<MailEntryUI>().Setup(mailData); // your existing UI setup
        // tooltip/buttons already wired in MailEntryUI
    }

    public void MarkMailAsHandled(MailData mail)
    {
        if (currentOpenSlot != null)
        {
            // stop expiry timer for the opened slot
            if (expiryBySlot.TryGetValue(currentOpenSlot, out var co))
            {
                StopCoroutine(co);
                expiryBySlot.Remove(currentOpenSlot);
            }

            // hide slot + remove bookkeeping
            currentOpenSlot.gameObject.SetActive(false);
            if (mailBySlot.TryGetValue(currentOpenSlot, out var data))
            {
                activeMails.Remove(data);
                mailBySlot.Remove(currentOpenSlot);
            }

            // close mail view
            foreach (Transform child in mailDisplayArea) Destroy(child.gameObject);
            currentOpenSlot = null;
            UpdateNoMailText();
        }
    }

    private IEnumerator ExpireAfter(MailSlot slot, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        // if still active, expire it
        if (slot != null && slot.gameObject.activeSelf)
        {
            // if this expired one is currently open, close the panel
            if (currentOpenSlot == slot)
            {
                foreach (Transform child in mailDisplayArea) Destroy(child.gameObject);
                currentOpenSlot = null;
            }

            slot.gameObject.SetActive(false);

            if (mailBySlot.TryGetValue(slot, out var data))
            {
                activeMails.Remove(data);
                mailBySlot.Remove(slot);
            }

            expiryBySlot.Remove(slot);
            UpdateNoMailText();
        }
    }

    private void UpdateNoMailText()
    {
        if (!noMailText) return;
        // show when there is NO active slot visible
        bool anyActive = false;
        foreach (var s in mailSlots) { if (s.gameObject.activeSelf) { anyActive = true; break; } }
        noMailText.gameObject.SetActive(!anyActive);
        if (!anyActive) noMailText.text = "You have no mails";
    }

    public bool MailOpen() => currentOpenSlot != null;
}
