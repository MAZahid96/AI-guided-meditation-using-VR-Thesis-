// using UnityEngine;
// using UnityEngine.SceneManagement;
// using System.Collections;

// public class Scene2Controller : MonoBehaviour
// {
//     private OpenAIManager gpt;

//     void Start()
//     {
//         gpt = FindObjectOfType<OpenAIManager>();
//         if (gpt == null)
//         {
//             Debug.LogError("OpenAIManager not found in scene!");
//             return;
//         }

//         StartCoroutine(MeditationRoutine());
//     }

//     IEnumerator MeditationRoutine()
//     {
//         // Initial instruction
//         yield return new WaitForSeconds(2);
//         gpt.SendCustomPrompt("Begin guided meditation. Focus on slow breathing.");
//         yield return new WaitForSeconds(15);

//         // Main 2-minute meditation loop
//         float meditationTime = 120f; // total meditation time
//         float elapsed = 0f;

//         while (elapsed < meditationTime)
//         {
//             gpt.SendCustomPrompt("Breathe in slowly.");
//             yield return new WaitForSeconds(8);

//             gpt.SendCustomPrompt("Breathe out gently.");
//             yield return new WaitForSeconds(8);

//             elapsed += 16f;
//         }

//         // End meditation message
//         bool messageFinished = false;
//         gpt.onResponseComplete = () => { messageFinished = true; };

//         gpt.SendCustomPrompt("Your meditation has ended. Open your eyes when ready.");

//         // Wait for TTS to finish
//         yield return new WaitUntil(() => messageFinished);

//         yield return new WaitForSeconds(2); // optional pause

//         // Change scene
//         SceneManager.LoadScene("Night Scene");
//     }
// }
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Scene2Controller : MonoBehaviour
{
    OpenAIManager gpt;

    void Start()
    {
        gpt = OpenAIManager.instance ?? FindObjectOfType<OpenAIManager>();
        if (!gpt)
        {
            Debug.LogError("OpenAIManager not found!");
            return;
        }
        StartCoroutine(MeditationRoutine());
    }

    IEnumerator MeditationRoutine()
    {
        // Decide duration based on stress
        var stress = gpt.CurrentStress;  // requires the getter we added earlier
        float minutes = stress switch
        {
            OpenAIManager.StressLevel.Low => 3f,
            OpenAIManager.StressLevel.Medium => 5f,
            OpenAIManager.StressLevel.High => 8f,
            _ => 4f, // fallback if Unknown
        };
        float meditationTime = minutes * 60f;

        // Tailor the intro line
        string intro = stress switch
        {
            OpenAIManager.StressLevel.High => "We will go slowly with longer exhales. Settle your shoulders.",
            OpenAIManager.StressLevel.Medium => "We will find a steady rhythm. Soften your jaw and breathe.",
            OpenAIManager.StressLevel.Low => "Keep an easy, light breath and relax.",
            _ => "Settle in and notice your breath.",
        };

        yield return new WaitForSeconds(2f);
        yield return Speak($"Begin guided meditation for {minutes:0} minutes. {intro}");

        // Main meditation loop
        float elapsed = 0f;
        const float inhaleSeconds = 4f;
        const float exhaleSeconds = 6f;

        while (elapsed < meditationTime)
        {
            yield return Speak("Breathe in slowly.");
            yield return new WaitForSeconds(inhaleSeconds);

            yield return Speak("Breathe out gently.");
            yield return new WaitForSeconds(exhaleSeconds);

            elapsed += inhaleSeconds + exhaleSeconds;
        }

        // Outro

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("Night Scene");
    }

    // Helper: send a prompt and wait until TTS finishes (uses gpt.onResponseComplete)
    IEnumerator Speak(string text)
    {
        bool done = false;
        gpt.onResponseComplete = () => done = true;
        gpt.SendCustomPrompt(text);
        yield return new WaitUntil(() => done);
    }
}
