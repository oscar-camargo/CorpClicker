using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class StickyAssistant : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup speechGroup;               // Controls visibility (fade in/out)
    public TextMeshProUGUI speechText;

    [Header("Timing")]
    public float minIdleTime = 30f;               // Min seconds between random messages
    public float maxIdleTime = 90f;               // Max seconds between random messages
    public float messageDuration = 4f;

    [Header("Dialogue Pool")]
    [TextArea(2, 5)]
    public List<string> idleLines;

    private Coroutine idleRoutine;

    void Start()
    {
        StartIdleDialogueLoop();
    }

    public void StartIdleDialogueLoop()
    {
        if (idleRoutine != null)
            StopCoroutine(idleRoutine);

        idleRoutine = StartCoroutine(RandomDialogueRoutine());
    }

    IEnumerator RandomDialogueRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(waitTime);

            if (idleLines.Count > 0)
            {
                string line = idleLines[Random.Range(0, idleLines.Count)];
                ShowDialogue(line);
            }
        }
    }

    public void ShowDialogue(string message)
    {
        speechText.text = message;
        speechGroup.alpha = 0;
        speechGroup.gameObject.SetActive(true);

        // Fade in, wait, then fade out
        Sequence sequence = DOTween.Sequence();
        sequence.Append(speechGroup.DOFade(1, 0.4f));
        sequence.AppendInterval(messageDuration);
        sequence.Append(speechGroup.DOFade(0, 0.4f));
        sequence.OnComplete(() => speechGroup.gameObject.SetActive(false));
    }
}
