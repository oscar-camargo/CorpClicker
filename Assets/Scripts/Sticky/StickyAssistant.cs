using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

public class StickyNudge : MonoBehaviour
{
    [SerializeField] private CanvasGroup bubbleRoot;
    [SerializeField] private TextMeshProUGUI bubbleText;
    [SerializeField] private string hint = "Click me for more info!";
    [SerializeField] private Vector2 intervalMinutes = new Vector2(2f, 5f);
    [SerializeField] private float showSeconds = 3f;
    [SerializeField] private float fadeIn = 0.25f, fadeOut = 0.15f;

    private Tween fadeT;
    private Coroutine loopCo;

    void OnEnable() { loopCo = StartCoroutine(Loop()); }
    void OnDisable()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        fadeT?.Kill();
        if (bubbleRoot) { bubbleRoot.alpha = 0; bubbleRoot.gameObject.SetActive(false); }
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            float wait = Random.Range(intervalMinutes.x, intervalMinutes.y) * 60f;
            yield return new WaitForSeconds(wait);
            if (Time.timeScale == 0f) continue; // don't nag while paused

            if (bubbleText) bubbleText.text = hint;
            ShowBubble();
            yield return new WaitForSeconds(showSeconds);
            HideBubble();
        }
    }

    private void ShowBubble()
    {
        if (!bubbleRoot) return;
        fadeT?.Kill();
        bubbleRoot.gameObject.SetActive(true);
        bubbleRoot.alpha = 0f;
        fadeT = bubbleRoot.DOFade(1f, fadeIn).SetUpdate(true);
    }

    private void HideBubble()
    {
        if (!bubbleRoot) return;
        fadeT?.Kill();
        fadeT = bubbleRoot.DOFade(0f, fadeOut)
                          .SetUpdate(true)
                          .OnComplete(() => { if (bubbleRoot) bubbleRoot.gameObject.SetActive(false); });
    }
}