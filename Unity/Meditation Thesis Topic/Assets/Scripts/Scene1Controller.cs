using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene1Controller : MonoBehaviour
{
    VoiceCommandListener voice;
    // OpenAIManager gpt;
    TTSManager tts;
    bool waitYes = true, waitStart = false;

    void Start()
    {
        voice = FindObjectOfType<VoiceCommandListener>();
        // gpt = FindObjectOfType<OpenAIManager>();
        tts = FindObjectOfType<TTSManager>();

        voice.onCommandRecognized += OnVoice;

        Debug.Log("Scene1 Start: Asking for 'yes'");
        // gpt.onResponseComplete = () => Debug.Log("Initial prompt done.");
        // gpt.SendCustomPrompt("We are going to begin the meditation session. If you are ready, please say yes.");
        tts.Speak("We are going to begin the meditation session. If you are ready, please say yes.");
    }

    void OnVoice(string cmd)
    {
        Debug.Log("Voice recognized: " + cmd);

        if (waitYes && cmd == "yes")
        {
            Debug.Log("In Scene1 - User said 'yes'");
            waitYes = false;
            waitStart = true;

            // gpt.onResponseComplete = () => Debug.Log("Prompt after YES finished.");
            // gpt.SendCustomPrompt("Great. Sit comfortably. When you're ready, say start.");
            tts.Speak("Great. Sit comfortably. When you're ready, say start.");

        }
        else if (waitStart && cmd == "start")
        {
            waitStart = false;
            Debug.Log("In Scene1 - User said 'start'");

            // gpt.onResponseComplete = () =>
            tts.Speak("Close your eyes when you are ready and follow my guidance.", () =>

            {
                Debug.Log("Prompt after START finished. Loading Scene2 in 5s");
                Invoke("LoadMeditationScene", 5f);
            }); ;

            // gpt.SendCustomPrompt("Close your eyes when you are ready and follow my guidance.");
            tts.Speak("Close your eyes when you are ready and follow my guidance.");
        }
    }

    void LoadMeditationScene()
    {
        Debug.Log("Scene1 -> Scene2 loading...");
        SceneManager.LoadScene("meditationScene");
    }

    void OnDestroy()
    {
        voice.onCommandRecognized -= OnVoice;
    }
}
