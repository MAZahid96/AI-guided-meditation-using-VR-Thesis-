using UnityEngine;

public class BreathingBackground : MonoBehaviour
{
    public Camera cam;
    public Color minColor = new Color(0.1f, 0.2f, 0.6f);
    public Color maxColor = new Color(0.3f, 0.5f, 1f);
    public float secondsPerBreath = 6f;

    void Reset() { cam = GetComponent<Camera>(); }
    void Update()
    {
        if (!cam) return;
        float t = (Mathf.Sin(Time.time * 2f * Mathf.PI / secondsPerBreath) + 1f) * 0.5f;
        cam.backgroundColor = Color.Lerp(minColor, maxColor, t);
    }
}