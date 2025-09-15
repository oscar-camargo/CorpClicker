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
        var stats = Clicker.PlayerStats.PlayerStatsManager.Instance;

        if (response.isChaotic)
        {
            // Normalize & roll
            float sum = response.probCaseA + response.probCaseB + response.probCaseC;
            if (sum <= 0f) sum = 1f;
            float r = Random.value * sum;

            if (r < response.probCaseA)
            {
                stats?.AddMorale(response.caseA_MoraleDelta);
                stats?.AddReputation(response.caseA_ReputationDelta);
                MessageLogManager.Instance?.PostSpecial("Chaotic reply: -10 morale, +15 reputation.");
            }
            else if (r < response.probCaseA + response.probCaseB)
            {
                stats?.AddMorale(response.caseB_MoraleDelta);
                stats?.AddReputation(response.caseB_ReputationDelta);
                MessageLogManager.Instance?.PostSpecial("Chaotic reply: +15 morale, -10 reputation.");
            }
            else
            {
                Clicker.Upgrades.UpgradeManager.Instance?.AddNextPushDurationBonus(response.caseC_PushDurationBonus);
                MessageLogManager.Instance?.PostSpecial($"+{response.caseC_PushDurationBonus:0} s bonus to next Push-Your-Luck boost!");
            }
        }
        else
        {
            stats?.AddMorale(response.moraleModifier);
            stats?.AddReputation(response.reputationModifier);
        }

        stats?.NotifyMailAnswered();          // resets rep-decay idle timer
        mailManager.MarkMailAsHandled(mail);  // closes/removes the mail
        Destroy(gameObject);
    }

}
