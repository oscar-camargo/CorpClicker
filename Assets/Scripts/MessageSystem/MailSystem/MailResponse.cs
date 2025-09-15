using System;
using UnityEngine;

[Serializable]
public class MailResponse
{
    [TextArea]
    public string responseText;
    public int moraleModifier;
    public int reputationModifier;

    [Header("Chaotic (optional)")]
    public bool isChaotic = false;

    [Range(0f,1f)] public float probCaseA = 0.45f; // morale -10, rep +15
    [Range(0f,1f)] public float probCaseB = 0.45f; // morale +15, rep -10
    [Range(0f,1f)] public float probCaseC = 0.10f; // +duration to next push boost

    [SerializeField] public int   caseA_MoraleDelta = -10;
    [SerializeField] public int   caseA_ReputationDelta = +15;

    [SerializeField] public int   caseB_MoraleDelta = +15;
    [SerializeField] public int   caseB_ReputationDelta = -10;

    [SerializeField] public float caseC_PushDurationBonus = 10f;    // seconds
}
