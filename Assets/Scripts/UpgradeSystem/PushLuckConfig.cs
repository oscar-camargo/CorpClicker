using UnityEngine;

[CreateAssetMenu(fileName="PushLuckConfig", menuName="Upgrades/Push Luck Config")]
public class PushLuckConfig : ScriptableObject
{
    [Header("Click thresholds")]
    public int tier1Min = 100;   // 100–149
    public int tier2Min = 150;   // 150–199/200
    public int tier3Min = 200;   // 200–250
    public int maxClicks = 250;

    [Header("Rewards per tier")]
    public double tier1FlatPerClick = 8.0;
    public double tier2FlatPerClick = 12.0;
    public double tier3FlatPerClick = 16.0;

    public double tier1AutoMult = 1.05;
    public double tier2AutoMult = 1.10;
    public double tier3AutoMult = 1.20;

    [Header("Duration")]
    public float[] baseDurationByLevel = new float[3] { 10f, 15f, 20f }; // L1/L2/L3
    public Vector2 moraleDurationMultRange = new Vector2(0.90f, 1.10f);   // min..max

    [Header("Fail probabilities (curves over tier range)")]
    // X=0..1 dentro del rango del tier, Y=prob 0..1
    public AnimationCurve tier2FailCurve = new AnimationCurve(
        new Keyframe(0f, 0.10f), new Keyframe(1f, 0.30f));
    public AnimationCurve tier3FailCurve = new AnimationCurve(
        new Keyframe(0f, 0.20f), new Keyframe(1f, 0.60f));

    [Header("Penalties")]
    public float blockClicksSeconds = 15f;     // fallo tier2
    public float blockAllSeconds = 15f;        // fallo tier3
    public float[] lockMinutesByLevel = new float[3] { 3f, 2.5f, 2f }; // L1/L2/L3
}
