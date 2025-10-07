
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using System.Collections;

// public class Scene2Controller : MonoBehaviour
// {
//     OpenAIManager gpt;

//     void Start()
//     {
//         gpt = OpenAIManager.instance ?? FindObjectOfType<OpenAIManager>();
//         if (!gpt)
//         {
//             Debug.LogError("OpenAIManager not found!");
//             return;
//         }
//         StartCoroutine(MeditationRoutine());
//     }

//     IEnumerator MeditationRoutine()
//     {
//         // Decide duration based on stress
//         var stress = gpt.CurrentStress;  // requires the getter we added earlier
//         float minutes = stress switch
//         {
//             OpenAIManager.StressLevel.Low => 3f,
//             OpenAIManager.StressLevel.Medium => 5f,
//             OpenAIManager.StressLevel.High => 8f,
//             _ => 4f, // fallback if Unknown
//         };
//         float meditationTime = minutes * 60f;

//         // Tailor the intro line
//         string intro = stress switch
//         {
//             OpenAIManager.StressLevel.High => "We will go slowly with longer exhales. Settle your shoulders.",
//             OpenAIManager.StressLevel.Medium => "We will find a steady rhythm. Soften your jaw and breathe.",
//             OpenAIManager.StressLevel.Low => "Keep an easy, light breath and relax.",
//             _ => "Settle in and notice your breath.",
//         };

//         yield return new WaitForSeconds(2f);
//         yield return Speak($"Begin guided meditation for {minutes:0} minutes. {intro}");

//         // Main meditation loop
//         float elapsed = 0f;
//         const float inhaleSeconds = 4f;
//         const float exhaleSeconds = 6f;

//         while (elapsed < meditationTime)
//         {
//             yield return Speak("Breathe in slowly.");
//             yield return new WaitForSeconds(inhaleSeconds);

//             yield return Speak("Breathe out gently.");
//             yield return new WaitForSeconds(exhaleSeconds);

//             elapsed += inhaleSeconds + exhaleSeconds;
//         }

//         // Outro

//         yield return new WaitForSeconds(2f);

//         SceneManager.LoadScene("Night Scene");
//     }

//     // Helper: send a prompt and wait until TTS finishes (uses gpt.onResponseComplete)
//     IEnumerator Speak(string text)
//     {
//         bool done = false;
//         gpt.onResponseComplete = () => done = true;
//         gpt.SendCustomPrompt(text);
//         yield return new WaitUntil(() => done);
//     }
// }
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Scene2Controller : MonoBehaviour
{
    OpenAIManager gpt;
    TTSManager tts;

    [Header("Gaps between lines (seconds)")]
    public Vector2 lowStressGap = new Vector2(8f, 12f);
    public Vector2 mediumStressGap = new Vector2(10f, 15f);
    public Vector2 highStressGap = new Vector2(12f, 18f);

    [Header("Safety margins")]
    public int extraLines = 6; // request a few more lines than needed

    void Start()
    {
        gpt = OpenAIManager.instance ?? FindObjectOfType<OpenAIManager>();
        tts = TTSManager.instance ?? FindObjectOfType<TTSManager>();
        if (!gpt || !tts)
        {
            Debug.LogError("Missing OpenAIManager or TTSManager in scene!");
            return;
        }
        StartCoroutine(MeditationRoutine());
    }

    IEnumerator MeditationRoutine()
    {
        // 1) Duration by stress
        var stress = gpt.CurrentStress;
        float minutes = stress switch
        {
            OpenAIManager.StressLevel.Low => 3f,
            OpenAIManager.StressLevel.Medium => 5f,
            OpenAIManager.StressLevel.High => 8f,
            _ => 4f
        };
        float totalSeconds = minutes * 60f;

        // 2) Gap range by stress
        Vector2 gapRange = stress switch
        {
            OpenAIManager.StressLevel.High => highStressGap,
            OpenAIManager.StressLevel.Medium => mediumStressGap,
            OpenAIManager.StressLevel.Low => lowStressGap,
            _ => new Vector2(10f, 15f)
        };
        float avgGap = (gapRange.x + gapRange.y) * 0.5f;

        // 3) Intro (not counted in the timer)
        yield return SpeakLocal($"We will meditate for {minutes:0} minutes. Close your eyes and follow my guidance.");

        // 4) Ask GPT ONCE for a list of unique cues
        int linesNeeded = Mathf.CeilToInt(totalSeconds / avgGap) + extraLines;

        bool received = false;
        string script = null;
        string userPrompt =
            $"Create {linesNeeded} unique calm meditation cues. " +
            $"One short line per cue (15 words max). " +
            $"Vary language. Avoid repeated phrases. " +
            $"Focus on inhale, exhale, relaxing shoulders, softening jaw, present attention.";

        gpt.RequestText(userPrompt, s => { script = s; received = true; });
        yield return new WaitUntil(() => received);

        // 5) Parse script into clean lines
        List<string> lines = ParseCueLines(script);
        if (lines.Count == 0)
        {
            // Fallback set
            lines = Enumerable.Repeat("Inhale slowly. Exhale gently. Soften your shoulders. Stay present.", linesNeeded).ToList();
        }

        // 6) Start wall-clock timer AFTER we have the script
        float endTime = Time.time + totalSeconds;

        int i = 0;
        while (Time.time < endTime && i < lines.Count)
        {
            // Say next unique line
            yield return SpeakLocal(lines[i++]);

            // Wait a random silence between min/max, but don't exceed end time
            float remaining = endTime - Time.time;
            if (remaining <= 0f) break;

            float gap = Random.Range(gapRange.x, gapRange.y);
            yield return new WaitForSeconds(Mathf.Min(gap, remaining));
        }

        // 7) Outro
        yield return SpeakLocal("Your meditation has ended. Open your eyes when you are ready.");
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("Night Scene");
    }

    List<string> ParseCueLines(string script)
    {
        if (string.IsNullOrEmpty(script)) return new List<string>();

        // Split and clean
        var raw = script.Split(new[] { '\n', '\r', '|' }, System.StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => s.Length > 0)
                        .Select(RemoveLeadingNumbering)
                        .Select(s => s.Trim('\"'))
                        .ToList();

        // De-duplicate while preserving order
        var set = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        var unique = new List<string>();
        foreach (var line in raw)
        {
            if (set.Add(line)) unique.Add(line);
        }
        return unique;
    }

    string RemoveLeadingNumbering(string s)
    {
        // Remove "1. ", "2) ", "- ", "* ", etc.
        int idx = 0;
        while (idx < s.Length && (char.IsDigit(s[idx]) || s[idx] == '.' || s[idx] == ')' || s[idx] == '-' || s[idx] == '*'))
            idx++;
        // Trim one space after if present
        if (idx < s.Length && s[idx] == ' ') idx++;
        return s.Substring(idx);
    }

    // Speak via local TTS and wait until finished (no extra GPT latency)
    IEnumerator SpeakLocal(string text)
    {
        bool done = false;
        tts.Speak(text, () => done = true);
        yield return new WaitUntil(() => done);
    }
}
