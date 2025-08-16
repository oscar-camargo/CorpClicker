using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MailManager : MonoBehaviour
{
    [Header("Spawner")]
    public float checkInterval = 10f;
    [Range(0f, 1f)] public float spawnChance = 0.4f;
    public int maxMails = 4;
    public List<MailData> mailPool;

    [Header("UI")]
    public List<MailSlot> mailSlots;       // Preloaded 4 buttons in MailQueue
    public GameObject mailPrefab;          // IncomingMail prefab (full mail view)
    public Transform mailDisplayArea;      // Where the mail prefab gets shown

    [Header("No Mail Display")]
    public GameObject noMailTextGroup; // Parent object of the 3 elements
    public TextMeshProUGUI noMailSubject;
    public TextMeshProUGUI noMailContent;


    private List<MailData> activeMails = new();

    void Start()
    {
        // Hide all mail buttons on start
        foreach (var slot in mailSlots)
            slot.gameObject.SetActive(false);

        InvokeRepeating(nameof(TryAddRandomMail), checkInterval, checkInterval);
    }

    public void TryAddRandomMail()
    {
        if (activeMails.Count >= maxMails) return;
        if (mailPool == null || mailPool.Count == 0) return;
        if (Random.value > spawnChance) return;

        MailData newMail = mailPool[Random.Range(0, mailPool.Count)];
        activeMails.Add(newMail);

        foreach (var slot in mailSlots)
        {
            if (!slot.gameObject.activeSelf)
            {
                slot.gameObject.SetActive(true);
                slot.Setup(newMail, this);
                break;
            }
        }

        UpdateNoMailText();
    }


    public void DisplayMail(MailData mailData)
    {
        // Remove any previously shown mail
        foreach (Transform child in mailDisplayArea)
            Destroy(child.gameObject);

        noMailTextGroup.SetActive(false);

        // Instantiate and populate the selected mail
        GameObject mailGO = Instantiate(mailPrefab, mailDisplayArea);
        mailGO.GetComponent<MailEntryUI>().Setup(mailData, this);
    }

    public void UpdateNoMailText()
    {
        if (activeMails.Count == 0)
        {
            noMailTextGroup.SetActive(true);
            noMailSubject.text = "Incoming mail:";
            noMailContent.text = "You have no mails... for now. Enjoy it while it lasts.";
        }
        else
        {
            noMailTextGroup.SetActive(true);
            noMailSubject.text = "Incoming mail:";
            noMailContent.text = "Check your inbox!";
        }
    }

    public void MarkMailAsHandled(MailData mail)
    {
        if (activeMails.Contains(mail))
            activeMails.Remove(mail);

        // Find and disable the button associated with this mail
        foreach (var slot in mailSlots)
        {
            if (slot.gameObject.activeSelf && slot.GetAssignedMail() == mail)
            {
                slot.Clear();
                break;
            }
        }

        UpdateNoMailText();
    }



}
