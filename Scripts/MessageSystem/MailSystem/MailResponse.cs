using System;
using UnityEngine;

[Serializable]
public class MailResponse
{
    [TextArea]
    public string responseText;
    public int moraleModifier;
    public int reputationModifier;
}
