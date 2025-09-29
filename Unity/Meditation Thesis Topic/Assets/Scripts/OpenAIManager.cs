// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections;
// using System.Text;
// using System;

// [System.Serializable] public class Choice { public Message message; }
// [System.Serializable] public class Message { public string role; public string content; }
// [System.Serializable] public class ChatResponse { public Choice[] choices; }

// public class OpenAIManager : MonoBehaviour
// {
//     private string apiKey;
//     private const string url = "https://api.openai.com/v1/chat/completions";

//     public static OpenAIManager instance;

//     public Action onResponseComplete;

//     void Awake()
//     {
//         if (instance == null)
//         {
//             instance = this;
//             DontDestroyOnLoad(gameObject); // Keep alive between scenes
//         }
//         else
//         {
//             Destroy(gameObject); // avoid duplicates
//             return;
//         }

//         // Load API key from Resources/openai_config.json
//         TextAsset config = Resources.Load<TextAsset>("openai_config");
//         if (config != null)
//         {
//             apiKey = JsonUtility.FromJson<OpenAIConfig>(config.text).api_key;
//         }
//         else
//         {
//             Debug.LogError("Missing openai_config in Resources folder!");
//         }
//     }

//     public void SendCustomPrompt(string userPrompt)
//     {
//         StartCoroutine(SendPromptCoroutine(userPrompt));
//     }

//     private IEnumerator SendPromptCoroutine(string prompt)
//     {
//         Debug.Log("Sending GPT request…");

//         string payload = "{\"model\":\"gpt-3.5-turbo\",\"messages\":[" +
//                          "{\"role\":\"system\",\"content\":\"You are a meditation guide. Your replies must always be under 15 words and delivered in one short, calming sentence. And each sentence should be different from the rest of the statements.\"}," +
//                          "{\"role\":\"user\",\"content\":\"" + Escape(prompt) + "\"}]}";

//         UnityWebRequest req = new UnityWebRequest(url, "POST");
//         req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
//         req.downloadHandler = new DownloadHandlerBuffer();
//         req.SetRequestHeader("Content-Type", "application/json");
//         req.SetRequestHeader("Authorization", "Bearer " + apiKey);

//         yield return req.SendWebRequest();

//         if (req.result == UnityWebRequest.Result.Success)
//         {
//             ChatResponse resp = JsonUtility.FromJson<ChatResponse>(req.downloadHandler.text);
//             string response = resp.choices[0].message.content.Trim();
//             Debug.Log("GPT: " + response);

//             if (TTSManager.instance != null)
//             {
//                 TTSManager.instance.Speak(response, () =>
//                 {
//                     Debug.Log("TTS finished speaking: " + response);
//                     onResponseComplete?.Invoke();
//                 });
//             }
//             else
//             {
//                 Debug.LogWarning("TTSManager instance is missing!");
//                 onResponseComplete?.Invoke();
//             }
//         }
//         else
//         {
//             Debug.LogError("GPT ERROR " + req.responseCode + ": " + req.downloadHandler.text);
//             onResponseComplete?.Invoke();
//         }
//     }

//     private static string Escape(string s)
//     {
//         return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
//     }
// }
// Assets/Scripts/OpenAIManager.cs
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using App.Config; // <-- needed to see OpenAIConfig

[Serializable] public class Choice { public Message message; }
[Serializable] public class Message { public string role; public string content; }
[Serializable] public class ChatResponse { public Choice[] choices; }

public class OpenAIManager : MonoBehaviour
{
    // ---- Stress level support ----
    public enum StressLevel { Unknown, Low, Medium, High }
    [SerializeField] private StressLevel currentStress = StressLevel.Unknown;

    private string apiKey;
    private const string url = "https://api.openai.com/v1/chat/completions";

    public static OpenAIManager instance;

    // Raised after TTS finishes speaking the response (or immediately on error)
    public Action onResponseComplete;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load API key from Resources/openai_config.json
        TextAsset config = Resources.Load<TextAsset>("openai_config");
        if (config != null)
        {
            try
            {
                apiKey = JsonUtility.FromJson<OpenAIConfig>(config.text).api_key;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed parsing openai_config.json: " + e);
            }
        }
        else
        {
            Debug.LogError("Missing openai_config.json in a Resources/ folder!");
        }
    }

    // -------- Public API --------
    public void SendCustomPrompt(string userPrompt)
    {
        StartCoroutine(SendPromptCoroutine(userPrompt));
    }

    public void SetStressLevel(StressLevel level)
    {
        currentStress = level;
        Debug.Log($"Stress set to: {currentStress}");
    }

    public void SetStressLevelFromString(string userText)
    {
        if (string.IsNullOrWhiteSpace(userText)) { currentStress = StressLevel.Unknown; return; }
        string s = userText.Trim().ToLowerInvariant();
        if (s.Contains("high")) currentStress = StressLevel.High;
        else if (s.Contains("medium") || s.Contains("mid")) currentStress = StressLevel.Medium;
        else if (s.Contains("low")) currentStress = StressLevel.Low;
        else currentStress = StressLevel.Unknown;

        Debug.Log($"Stress parsed from '{userText}' -> {currentStress}");
    }

    /// <summary>
    /// Optional helper: speaks the stress question, waits for your app to supply the answer,
    /// then sends the real user prompt.
    /// Provide a poll function that returns the latest recognized text (or null/empty if not yet).
    /// </summary>
    public IEnumerator AskStressLevelThenPrompt(string finalUserPrompt, Func<string> pollForUserAnswer, float pollIntervalSeconds = 0.3f)
    {
        // Ask user
        if (TTSManager.instance != null)
            TTSManager.instance.Speak("Before we start, is your stress level high, medium, or low?");
        else
            Debug.Log("ASK: Before we start, is your stress level high, medium, or low?");

        // Wait for external UI/ASR to provide text
        string answer = null;
        while (string.IsNullOrEmpty(answer))
        {
            answer = pollForUserAnswer?.Invoke();
            yield return new WaitForSeconds(pollIntervalSeconds);
        }

        SetStressLevelFromString(answer);
        yield return SendPromptCoroutine(finalUserPrompt);
    }

    // -------- Internal request flow --------
    private IEnumerator SendPromptCoroutine(string prompt)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("OpenAI API key is empty. Check Resources/openai_config.json.");
            onResponseComplete?.Invoke();
            yield break;
        }

        Debug.Log("Sending GPT request…");

        string stressGuidance = StressGuidance(currentStress);

        // Build messages with stress-aware system prompt
        var payload = "{" +
            "\"model\":\"gpt-3.5-turbo\"," +
            "\"messages\":[" +
                "{\"role\":\"system\",\"content\":" + Quote(
                    "You are a meditation guide. Replies must always be under 15 words, one short calming sentence, varied each time. " +
                    "The user's current stress is: " + currentStress + ". " +
                    stressGuidance
                ) + "}," +
                "{\"role\":\"user\",\"content\":" + Quote(prompt) + "}" +
            "]" +
        "}";

        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            ChatResponse resp = null;
            try { resp = JsonUtility.FromJson<ChatResponse>(req.downloadHandler.text); }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse GPT response JSON: " + e + "\n" + req.downloadHandler.text);
            }

            string response = resp?.choices != null && resp.choices.Length > 0
                ? resp.choices[0].message.content.Trim()
                : "(no response)";

            Debug.Log("GPT: " + response);

            if (TTSManager.instance != null)
            {
                TTSManager.instance.Speak(response, () =>
                {
                    Debug.Log("TTS finished speaking: " + response);
                    onResponseComplete?.Invoke();
                });
            }
            else
            {
                Debug.LogWarning("TTSManager instance is missing!");
                onResponseComplete?.Invoke();
            }
        }
        else
        {
            Debug.LogError("GPT ERROR " + req.responseCode + ": " + req.downloadHandler.text);
            onResponseComplete?.Invoke();
        }
    }

    // -------- Helpers --------
    private static string Quote(string s) => "\"" + Escape(s) + "\"";
    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string StressGuidance(StressLevel level)
    {
        switch (level)
        {
            case StressLevel.High:
                return "Use slower pace, reassurance, safety cues, and deeper breathing guidance.";
            case StressLevel.Medium:
                return "Use steady, balancing tone; mention releasing shoulder tension and calm focus.";
            case StressLevel.Low:
                return "Use light, uplifting tone; emphasize gentle presence and gratitude.";
            default:
                return "If stress is unknown, ask gently about current state before guiding.";
        }
    }
}
