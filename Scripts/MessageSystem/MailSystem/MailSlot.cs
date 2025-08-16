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

        var label = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label != null)
            label.text = mail.subject;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(DisplayThisMail);
    }

    private void DisplayThisMail()
    {
        mailManager.DisplayMail(assignedMail);
    }

    public MailData GetAssignedMail() => assignedMail;

    public void Clear()
    {
        assignedMail = null;
        gameObject.SetActive(false);
    }
}
