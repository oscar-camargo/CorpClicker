using UnityEngine;
using UnityEngine.EventSystems;

public class MailResponseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private MailResponse response;
    private RectTransform rect;

    private void Awake() => rect = transform as RectTransform;

    public void SetResponse(MailResponse r) => response = r;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ResponseTooltipDisplay.Instance != null && response != null)
            ResponseTooltipDisplay.Instance.Show(response, rect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ResponseTooltipDisplay.Instance != null)
            ResponseTooltipDisplay.Instance.Hide();
    }
}
