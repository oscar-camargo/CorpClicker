using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MailEntryUI : MonoBehaviour
{
    public TextMeshProUGUI subjectText;
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI responseTitleText;
    public Button[] responseButtons;

    private MailData mail;
    private MailManager mailManager;

    public void Setup(MailData data, MailManager manager)
    {
        mail = data;
        mailManager = manager;

        subjectText.text = data.subject;
        contentText.text = data.content;
        responseTitleText.gameObject.SetActive(true);

        for (int i = 0; i < responseButtons.Length; i++)
        {
            int index = i;
            var response = data.responses[i];
            responseButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = response.responseText;
            responseButtons[i].onClick.RemoveAllListeners();
            responseButtons[i].onClick.AddListener(() => HandleResponse(response));
            responseButtons[i].gameObject.SetActive(true);
        }
    }

    private void HandleResponse(MailResponse response)
    {
        mailManager.MarkMailAsHandled(mail);
        Destroy(gameObject);
    }
}
