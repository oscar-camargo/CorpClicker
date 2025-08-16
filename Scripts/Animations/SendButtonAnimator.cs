using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SendButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private RectTransform rect;
    private Vector3 originalScale;

    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float speed = 0.1f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale * hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ClickBounce());
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        while (Vector3.Distance(rect.localScale, target) > 0.01f)
        {
            rect.localScale = Vector3.Lerp(rect.localScale, target, 0.2f);
            yield return new WaitForSeconds(speed);
        }
        rect.localScale = target;
    }

    IEnumerator ClickBounce()
    {
        yield return ScaleTo(originalScale * clickScale);
        yield return ScaleTo(originalScale * hoverScale); // stay in hover state
    }
}
