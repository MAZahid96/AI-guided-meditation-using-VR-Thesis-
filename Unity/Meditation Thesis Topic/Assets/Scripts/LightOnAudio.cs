using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LightOnAudio : MonoBehaviour
{
    [Header("References")]
    public AudioSource voiceSource;   // AI/TTS AudioSource
    public Light targetLight;

    [Header("Intensity")]
    public float offIntensity = 0f;
    public float onIntensity = 5000f;   // use high values if you need through-eyelid brightness
    public float fadeSpeed = 6f;        // higher = snappier

    [Header("Optional: respond to volume (RMS)")]
    public bool pulseWithVolume = false;
    public float volumeGain = 1.5f;     // scales the pulse
    public int sampleCount = 256;

    float[] _samples;

    void Reset()
    {
        voiceSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (!voiceSource) voiceSource = GetComponent<AudioSource>();
        if (!targetLight) Debug.LogWarning("LightOnAudio: assign targetLight.");
        _samples = new float[sampleCount];
        if (targetLight) targetLight.intensity = offIntensity;
    }

    void Update()
    {
        if (!targetLight || !voiceSource) return;

        float target = voiceSource.isPlaying ? onIntensity : offIntensity;

        if (pulseWithVolume && voiceSource.isPlaying)
        {
            // Simple RMS-based boost
            voiceSource.GetOutputData(_samples, 0);
            float sum = 0f;
            for (int i = 0; i < _samples.Length; i++) sum += _samples[i] * _samples[i];
            float rms = Mathf.Sqrt(sum / _samples.Length);
            target *= (1f + rms * volumeGain); // subtle pulse on speech loudness
        }

        targetLight.intensity = Mathf.Lerp(targetLight.intensity, target, Time.deltaTime * fadeSpeed);
    }
}
