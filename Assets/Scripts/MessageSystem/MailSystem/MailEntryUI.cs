using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Clicker.PlayerStats;

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
            var hover = responseButtons[i].GetComponent<MailResponseHover>();
            if (hover == null) hover = responseButtons[i].gameObject.AddComponent<MailResponseHover>();
            hover.SetResponse(response);
            responseButtons[i].onClick.AddListener(() =>
            {
                if (ResponseTooltipDisplay.Instance != null)
                    ResponseTooltipDisplay.Instance.Hide();
            });
        }
    }

    private void HandleResponse(MailResponse response)
    {
        var stats = PlayerStatsManager.Instance;
        if (stats != null)
        {
            stats.AddMorale(response.moraleModifier);
            stats.AddReputation(response.reputationModifier);
            stats.NotifyMailAnswered();
        }

        mailManager.MarkMailAsHandled(mail); // hides the queue button + updates no-mail text
        Destroy(gameObject);                 // closes the opened mail UI
    }
}
