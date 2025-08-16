using UnityEngine;

public class MailSpawner : MonoBehaviour
{
    public MailManager mailManager;
    public float checkInterval = 10f;
    [Range(0f, 1f)] public float spawnChance = 0.4f;

    private void Start()
    {
        InvokeRepeating(nameof(TrySpawnMail), checkInterval, checkInterval);
    }

    void TrySpawnMail()
    {
        // Let MailManager handle the actual mail count and logic
        mailManager.TryAddRandomMail();
    }
}
