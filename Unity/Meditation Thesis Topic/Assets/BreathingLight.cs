using UnityEngine;

[ExecuteAlways]
public class BreathingLight : MonoBehaviour
{
    public Light targetLight;

    [Header("Intensity (cd)")]
    public float minIntensity = 0f;
    public float maxIntensity = 5000f;

    [Header("Optional color breathing")]
    public bool animateColor = false;
    public Color minColor = new Color(0.20f, 0.35f, 1f); // soft blue
    public Color maxColor = new Color(0.55f, 0.75f, 1f); // brighter blue

    [Header("Breath timing (seconds)")]
    public float inhale = 4f;
    public float holdTop = 0f;
    public float exhale = 6f;
    public float holdBottom = 0f;

    [Header("Smoothing")]
    [Range(0f, 1f)] public float ease = 0.5f; // 0=linear, 1=sine

    float tCycle;
    float cycleDur;

    void Reset() { targetLight = GetComponent<Light>(); }

    void Awake()
    {
        if (!targetLight) targetLight = GetComponent<Light>();
        RecalcCycle();
        // Color temperature can override color in URP/HDRP – safer off for color breathing
#if UNITY_EDITOR
        if (targetLight) targetLight.useColorTemperature = false;
#endif
    }

    void OnValidate() { RecalcCycle(); }

    void RecalcCycle()
    {
        inhale = Mathf.Max(0f, inhale);
        holdTop = Mathf.Max(0f, holdTop);
        exhale = Mathf.Max(0f, exhale);
        holdBottom = Mathf.Max(0f, holdBottom);
        cycleDur = Mathf.Max(0.01f, inhale + holdTop + exhale + holdBottom);
    }

    void Update()
    {
        if (!targetLight) return;

        // progress over whole cycle
        tCycle = (tCycle + Time.deltaTime) % cycleDur;
        float x = tCycle;

        // value 0..1 over inhale/hold/exhale/hold
        float value;
        if (x < inhale) value = Ease01(x / inhale);                         // inhale up
        else if (x < inhale + holdTop) value = 1f;                                         // top hold
        else if (x < inhale + holdTop + exhale) value = 1f - Ease01((x - inhale - holdTop) / exhale); // exhale down
        else value = 0f;                                         // bottom hold

        // intensity
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, value);

        // optional color breathing
        if (animateColor)
            targetLight.color = Color.Lerp(minColor, maxColor, value);
    }

    // 0..1 easing blend (linear -> sine)
    float Ease01(float u)
    {
        u = Mathf.Clamp01(u);
        float s = 0.5f - 0.5f * Mathf.Cos(u * Mathf.PI); // sine ease
        return Mathf.Lerp(u, s, Mathf.Clamp01(ease));
    }
}