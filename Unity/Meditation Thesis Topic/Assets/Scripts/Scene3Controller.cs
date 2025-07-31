using UnityEngine;

public class Scene3Controller : MonoBehaviour
{
    void Start()
    {
        if (TTSManager.instance != null)
        {
            TTSManager.instance.Speak("Thank you for meditating with me. Have a peaceful day.");
        }
        else
        {
            Debug.LogWarning("TTSManager not found in Scene3.");
        }
    }
}
