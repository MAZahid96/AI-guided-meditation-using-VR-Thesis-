/*
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
        var stress = gpt.CurrentStress;  // requires the getter in OpenAIManager
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

        // Brief pause then intro (does not count towards the timed session)
        yield return new WaitForSeconds(2f);
        yield return Speak($"Begin guided meditation for {minutes:0} minutes. {intro}");

        // --- Wall-clock timing starts AFTER the intro finishes ---
        float endTime = Time.time + meditationTime;

        const float inhaleSeconds = 4f;
        const float exhaleSeconds = 6f;

        while (Time.time < endTime)
        {
            // Inhale cue
            yield return Speak("Breathe in slowly.");
            float remaining = endTime - Time.time;
            if (remaining <= 0f) break;
            yield return new WaitForSeconds(Mathf.Min(inhaleSeconds, remaining));

            // Exhale cue
            remaining = endTime - Time.time;
            if (remaining <= 0f) break;
            yield return Speak("Breathe out gently.");
            remaining = endTime - Time.time;
            if (remaining <= 0f) break;
            yield return new WaitForSeconds(Mathf.Min(exhaleSeconds, remaining));
        }

        // Outro
        yield return Speak("Your meditation has ended. Open your eyes when ready.");
        yield return new WaitForSeconds(2f);

        // Move on
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
}*/
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
        var stress = gpt.CurrentStress;  // requires the getter in OpenAIManager
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

        // Brief pause then intro (does not count towards the timed session)
        yield return new WaitForSeconds(2f);
        yield return Speak($"Begin guided meditation for {minutes:0} minutes. {intro}");

        // --- Timing starts AFTER the intro finishes ---
        float endTime = Time.time + meditationTime;

        const float gapSeconds = 12f; // ✅ fixed 12s gap after each statement

        while (Time.time < endTime)
        {
            // Inhale cue + gap
            yield return Speak("Breathe in slowly.");
            float remaining = endTime - Time.time;
            if (remaining <= 0f) break;
            yield return new WaitForSeconds(Mathf.Min(gapSeconds, remaining));

            // Exhale cue + gap
            remaining = endTime - Time.time;
            if (remaining <= 0f) break;
            yield return Speak("Breathe out gently.");
            remaining = endTime - Time.time;
            if (remaining <= 0f) break;
            yield return new WaitForSeconds(Mathf.Min(gapSeconds, remaining));
        }

        // Outro
        yield return Speak("Your meditation has ended. Open your eyes when ready.");
        yield return new WaitForSeconds(2f);

        // Move on
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