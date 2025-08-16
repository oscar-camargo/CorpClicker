using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MessageLoader : MonoBehaviour
{
    public static List<MessageEntryData> Messages = new List<MessageEntryData>();

    [SerializeField] private string csvFileName = "message_phrases"; // drop .csv

    private void Awake()
    {
        LoadMessagesFromCSV();
    }

    private void LoadMessagesFromCSV()
    {
        TextAsset file = Resources.Load<TextAsset>(csvFileName);
        if (file == null)
        {
            Debug.LogError($"CSV file '{csvFileName}' not found in Resources.");
            return;
        }

        using (StringReader reader = new StringReader(file.text))
        {
            bool isFirstLine = true;
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                if (isFirstLine) { isFirstLine = false; continue; } // Skip header

                string[] parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    string category = parts[0].Trim();
                    string message = parts[1].Trim('\"');
                    bool isImportant = parts[2].ToLower().Contains("true");

                    Messages.Add(new MessageEntryData(category, message, isImportant));
                }
            }
        }

        Debug.Log($"Loaded {Messages.Count} messages from CSV.");
    }
}
