using UnityEngine;

public class Scene3Controller : MonoBehaviour
{
    void Start()
    {
        if (TTSManager.instance != null)
        {
            TTSManager.instance.Speak("Please Open Your Eyes and Thank you for meditating with me. Have a peaceful day.");
        }
        else
        {
            Debug.LogWarning("TTSManager not found in Scene3.");
        }
    }
}
