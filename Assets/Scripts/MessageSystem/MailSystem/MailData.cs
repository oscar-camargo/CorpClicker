using UnityEngine;

[CreateAssetMenu(fileName = "NewMail", menuName = "Mail System/Mail Data")]
public class MailData : ScriptableObject
{
    public string subject;
    [TextArea(5, 10)]
    public string content;
    public MailResponse[] responses = new MailResponse[4];
}
