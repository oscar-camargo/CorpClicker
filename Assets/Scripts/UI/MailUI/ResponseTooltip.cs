using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResponseTooltipDisplay : MonoBehaviour
{
    public static ResponseTooltipDisplay Instance { get; private set; }

    [Header("Root & Position")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private Vector2 offset = new Vector2(50f, 0f);

    [Header("Morale UI")]
    [SerializeField] private GameObject moraleUpArrow;
    [SerializeField] private GameObject moraleDownArrow;
    [SerializeField] private TextMeshProUGUI moraleText;

    [Header("Reputation UI")]
    [SerializeField] private GameObject repUpArrow;
    [SerializeField] private GameObject repDownArrow;
    [SerializeField] private TextMeshProUGUI repText;

    [Header("Colors")]
    [SerializeField] private Color positiveColor = new Color(0.13f, 0.75f, 0.23f); // verde
    [SerializeField] private Color negativeColor = new Color(0.85f, 0.15f, 0.15f); // rojo
    [SerializeField] private Color zeroColor = new Color(0.70f, 0.70f, 0.70f);     // gris

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(MailResponse response, RectTransform anchor)
    {
        if (response == null) return;

        // Morale
        ApplyStat(moraleUpArrow, moraleDownArrow, moraleText, response.moraleModifier);

        // Reputation
        ApplyStat(repUpArrow, repDownArrow, repText, response.reputationModifier);

        tooltipRoot.SetActive(true);
        PositionTooltip(anchor);
    }

    public void Hide()
    {
        tooltipRoot.SetActive(false);
    }

    private void ApplyStat(GameObject up, GameObject down, TextMeshProUGUI txt, int value)
    {
        // Solo muestra una flecha; para 0 oculta ambas
        bool pos = value > 0;
        bool neg = value < 0;

        if (up)   up.SetActive(pos);
        if (down) down.SetActive(neg);

        if (txt)
        {
            txt.text  = Mathf.Abs(value).ToString();        // “10”
            txt.color = pos ? positiveColor : (neg ? negativeColor : zeroColor);
            if (value == 0) txt.text = "0";                 // o “—” si prefieres
        }
    }

    private void PositionTooltip(RectTransform anchor)
    {
        Vector3[] corners = new Vector3[4];
        anchor.GetWorldCorners(corners);
        Vector3 anchorRightCenter = (corners[2] + corners[3]) * 0.5f;

        RectTransform canvasRect = tooltipRoot.transform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, anchorRightCenter),
            null,
            out Vector2 anchoredPos))
        {
            tooltipRoot.GetComponent<RectTransform>().anchoredPosition = anchoredPos + offset;
        }
    }
}
