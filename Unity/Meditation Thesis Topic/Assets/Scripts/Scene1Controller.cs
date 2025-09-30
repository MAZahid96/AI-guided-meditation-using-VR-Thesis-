/*using UnityEngine;
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
           // tts.Speak("Close your eyes when you are ready and follow my guidance.");
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
*/
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene1Controller : MonoBehaviour
{
    enum Step { AskStress, WaitStress, AskYes, WaitYes, AskStart, WaitStart, Done }
    Step step = Step.AskStress;

    VoiceCommandListener voice;
    TTSManager tts;
    OpenAIManager gpt;

    void Start()
    {
        voice = FindObjectOfType<VoiceCommandListener>();
        tts = FindObjectOfType<TTSManager>();
        gpt = FindObjectOfType<OpenAIManager>();

        if (voice != null) voice.onCommandRecognized += OnVoice;

        // ✅ Ask about stress first
        AskStress();
    }

    void OnDestroy()
    {
        if (voice != null) voice.onCommandRecognized -= OnVoice;
    }

    void AskStress()
    {
        tts.Speak("Before we start, how is your stress level: high, medium, or low?");
        step = Step.WaitStress;
    }

    void OnVoice(string cmd)
    {
        string answer = cmd.ToLowerInvariant().Trim();
        Debug.Log("[Scene1] Voice recognized: " + answer);

        switch (step)
        {
            case Step.WaitStress:
                // Accept stress words like "high", "medium", "low"
                gpt?.SetStressLevelFromString(answer);
                Debug.Log("[Scene1] Stress set to " + gpt.CurrentStress);

                tts.Speak("Thank you. If you are ready, please say yes.");
                step = Step.WaitYes;
                break;

            case Step.WaitYes:
                if (answer == "yes")
                {
                    Debug.Log("[Scene1] User said YES");
                    tts.Speak("Great. Sit comfortably. When you're ready, say start.");
                    step = Step.WaitStart;
                }
                break;

            case Step.WaitStart:
                if (answer == "start")
                {
                    Debug.Log("[Scene1] User said START");
                    tts.Speak("Close your eyes and follow my instructions.", () => 
                    { 
                        string line = TailoredMeditationLine();
                        tts.Speak(line, () =>
                        {
                            Debug.Log("Prompt after START finished. Loading Scene2 in 5s");
                            Invoke(nameof(LoadMeditationScene), 5f);
                        });
                    });
                    step = Step.Done;
                }
                break;
        }
    }

    string TailoredMeditationLine()
    {
        switch (gpt.CurrentStress)
        {
            case OpenAIManager.StressLevel.High:
                return "Let’s slow your breath with long exhales and gentle shoulders.";
            case OpenAIManager.StressLevel.Medium:
                return "Settle in, soften your jaw and breathe steadily.";
            case OpenAIManager.StressLevel.Low:
                return "Enjoy a light, easy breath and quiet focus.";
            default:
                return "Settle in, notice your breath, and relax your shoulders.";
        }
    }

    void LoadMeditationScene()
    {
        Debug.Log("Scene1 -> Scene2 loading...");
        SceneManager.LoadScene("meditationScene");
    }
}