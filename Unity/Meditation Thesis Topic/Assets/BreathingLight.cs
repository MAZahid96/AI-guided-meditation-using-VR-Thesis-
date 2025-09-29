using System.Collections;
using System.Collections.Generic;
// BreathingLight.cs
using UnityEngine;

public class BreathingLight : MonoBehaviour
{
    public Light targetLight;
    [Header("Intensity (cd)")]
    public float minIntensity = 0f;
    public float maxIntensity = 5000f;

    [Header("Breath timing (seconds)")]
    public float inhale = 4f;
    public float holdTop = 0f;   // set to 2f for box breathing
    public float exhale = 6f;
    public float holdBottom = 0f;

    [Header("Smoothing")]
    public float ease = 0.5f;    // 0 = linear, 0.5 = smoother

    float tCycle, cycleDur;

    void Reset() { targetLight = GetComponent<Light>(); }
    void Awake()
    {
        if (!targetLight) targetLight = GetComponent<Light>();
        cycleDur = Mathf.Max(0.01f, inhale + holdTop + exhale + holdBottom);
    }

    void Update()
    {
        if (!targetLight) return;
        tCycle = (tCycle + Time.deltaTime) % cycleDur;

        float x = tCycle;
        float value;
        if (x < inhale)
        {
            // ramp up (inhale)
            value = Ease01(x / inhale);
        }
        else if (x < inhale + holdTop)
        {
            value = 1f;
        }
        else if (x < inhale + holdTop + exhale)
        {
            // ramp down (exhale)
            float u = (x - inhale - holdTop) / exhale;
            value = 1f - Ease01(u);
        }
        else
        {
            value = 0f; // hold bottom
        }

        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, value);
    }

    float Ease01(float u)
    {
        // Smooth sine+smoothstep blend
        u = Mathf.Clamp01(u);
        float s = 0.5f - 0.5f * Mathf.Cos(u * Mathf.PI);              // sine ease
        return Mathf.Lerp(u, s, Mathf.Clamp01(ease));                  // mix with linear
    }
}
