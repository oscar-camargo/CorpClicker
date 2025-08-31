using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MessageEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image iconImage;

    [SerializeField] private Sprite importantIcon; // assign in prefab

    public void Setup(MessageEntryData data)
    {
        timestampText.text = DateTime.Now.ToString("HH:mm:ss");
        messageText.text = data.message;

        if (data.isImportant && importantIcon != null)
        {
            iconImage.sprite = importantIcon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    //Helper
    public void SetText(string text) => SetText(text, null, false);

    public void SetText(string text, Color? overrideColor, bool bold)
    {
        if (timestampText) timestampText.text = DateTime.Now.ToString("HH:mm:ss");
        if (!messageText) return;

        messageText.richText = true;
        messageText.text = bold ? $"<b>{text}</b>" : text;
        if (overrideColor.HasValue) messageText.color = overrideColor.Value;
    }
}
