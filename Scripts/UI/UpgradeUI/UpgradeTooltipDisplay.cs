using UnityEngine;
using TMPro;
using Clicker.Upgrades;

public class UpgradeTooltipDisplay : MonoBehaviour
{
    public static UpgradeTooltipDisplay Instance { get; private set; }

    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI flavorText;
    [SerializeField] private Vector2 offset = new Vector2(50f, 0f);

    private bool isVisible = false;

    private void Awake()
    {
        Instance = this;
        Hide(); // Make sure it starts hidden
        Debug.Log("Tooltip system initialized.");
    }

    public void Show(UpgradeData data, RectTransform anchor)
    {
        tooltipRoot.SetActive(true);
        isVisible = true;

        titleText.text = data.title;
        effectText.text = data.effectDescription;
        int level = UpgradeManager.Instance.GetUpgradeLevel(data); // current level
        int index = Mathf.Clamp(level, 0, data.flavorTexts.Length - 1); // safe index

        flavorText.text = data.flavorTexts.Length > 0 ? data.flavorTexts[index] : "";

        PositionTooltip(anchor);
        Debug.Log("Now showing tooltip");
    }


    public void Hide()
    {
        tooltipRoot.SetActive(false);
        isVisible = false;
    }

    private void PositionTooltip(RectTransform anchor)
    {
        // Get world position of the anchor
        Vector3[] corners = new Vector3[4];
        anchor.GetWorldCorners(corners);
        Vector3 anchorRightCenter = (corners[2] + corners[3]) * 0.5f;

        // Convert world position to anchored position
        Vector2 anchoredPos;
        RectTransform canvasRect = tooltipRoot.transform.parent as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, anchorRightCenter),
            null,
            out anchoredPos))
        {
            tooltipRoot.GetComponent<RectTransform>().anchoredPosition = anchoredPos + offset;
        }
    }

}
