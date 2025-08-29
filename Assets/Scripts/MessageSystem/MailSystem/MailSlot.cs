using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MailSlot : MonoBehaviour
{
    private MailData assignedMail;
    private MailManager mailManager;

    public void Setup(MailData mail, MailManager manager)
    {
        assignedMail = mail;
        mailManager = manager;

        var label = GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = mail.subject;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(DisplayThisMail);
    }

    private void DisplayThisMail()
    {
        // IMPORTANT: pass this slot so the manager can cancel/handle expiry
        mailManager.DisplayMail(assignedMail, this);
    }

    public MailData GetAssignedMail() => assignedMail;

    public void Clear()
    {
        assignedMail = null;
        // optional: remove listeners to be tidy
        var btn = GetComponent<Button>();
        if (btn) btn.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
    }
}
