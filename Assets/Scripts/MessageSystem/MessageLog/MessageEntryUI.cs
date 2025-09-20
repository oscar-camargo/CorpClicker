using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MessageEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private float verticalPadding = 8f; // top+bottom padding you want

    LayoutElement le;
    RectTransform rt;

    void Awake()
    {
        le = GetComponent<LayoutElement>();
        rt = GetComponent<RectTransform>();
    }

    public void Setup(MessageEntryData data)
    {
        timeText.text = DateTime.Now.ToString("HH:mm:ss");
        bodyText.text = data.message;

        //Currently, the "important message" icon is not used. Might use it in the future, so I'm keeping this block
        //if (data.isImportant && importantIcon != null)
        //{
        //    iconImage.sprite = importantIcon;
        //   iconImage.enabled = true;
        //}
        //else
        //{
        //    iconImage.enabled = false;
        //}
    }

    public void SetText(string text, Color? overrideColor, bool bold)
    {
        if (timeText) timeText.text = System.DateTime.Now.ToString("HH:mm:ss");

        if (bodyText)
        {
            bodyText.richText = true;
            bodyText.enableWordWrapping = true;
            bodyText.enableAutoSizing = false;
            bodyText.overflowMode = TextOverflowModes.Overflow;
            bodyText.text = bold ? $"<b>{text}</b>" : text;
            if (overrideColor.HasValue) bodyText.color = overrideColor.Value;

            // ---- robust width/height calc ----
            var rootRT = (RectTransform)transform;
            var parentRT = rootRT.parent as RectTransform;
            var hlg = GetComponent<HorizontalLayoutGroup>();

            float containerW = rootRT.rect.width;
            if (containerW <= 0f && parentRT) containerW = parentRT.rect.width;

            float pads = 0f, spacing = 0f;
            if (hlg != null)
            {
                pads = hlg.padding.left + hlg.padding.right;
                spacing = hlg.spacing;
            }

            float timeW = 0f;
            if (timeText)
            {
                var tLE = timeText.GetComponent<LayoutElement>();
                timeW = (tLE && tLE.preferredWidth > 0f)
                    ? tLE.preferredWidth
                    : LayoutUtility.GetPreferredWidth(timeText.rectTransform);
            }

            float bodyW = Mathf.Max(0f, containerW - pads - spacing - timeW);

            bodyText.ForceMeshUpdate();
            float bodyH = bodyText.GetPreferredValues(bodyText.text, bodyW, 0f).y;

            float timeH = 0f;
            if (timeText)
            {
                timeText.ForceMeshUpdate();
                timeH = timeText.GetPreferredValues(timeText.text, timeW, 0f).y;
            }

            if (!le) le = GetComponent<LayoutElement>();
            if (le) le.preferredHeight = Mathf.Max(bodyH, timeH) + verticalPadding;
        }

        if (!rt) rt = GetComponent<RectTransform>();
        if (rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

}
