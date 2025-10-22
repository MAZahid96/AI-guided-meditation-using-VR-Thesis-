
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
        tts.Speak("Before we start, how is your stress level: high or low?");
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
                return "Inhale when the light is bright and exhale when it fades away.";
            case OpenAIManager.StressLevel.Low:
                return "Inhale when the light is bright and exhale when it fades away.";
            default:
                return "Inhale when the light is bright and exhale when it fades away.";
        }
    }

    void LoadMeditationScene()
    {
        Debug.Log("Scene1 -> Scene2 loading...");
        SceneManager.LoadScene("meditationScene");
    }
}