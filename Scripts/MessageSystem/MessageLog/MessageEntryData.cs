[System.Serializable]
public class MessageEntryData
{
    public string category;
    public string message;
    public bool isImportant;

    public MessageEntryData(string category, string message, bool isImportant)
    {
        this.category = category;
        this.message = message;
        this.isImportant = isImportant;
    }
}
