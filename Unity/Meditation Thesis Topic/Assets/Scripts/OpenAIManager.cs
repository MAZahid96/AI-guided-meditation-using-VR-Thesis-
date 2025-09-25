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
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using App.Config;

[System.Serializable] public class Choice { public Message message; }
[System.Serializable] public class Message { public string role; public string content; }
[System.Serializable] public class ChatResponse { public Choice[] choices; }

public class OpenAIManager : MonoBehaviour
{
    // NEW: stress level
    public enum StressLevel { Unknown, Low, Medium, High }
    [SerializeField] private StressLevel currentStress = StressLevel.Unknown;

    private string apiKey;
    private const string url = "https://api.openai.com/v1/chat/completions";

    public static OpenAIManager instance;
    public Action onResponseComplete;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        TextAsset config = Resources.Load<TextAsset>("openai_config");
        if (config != null)
        {
            apiKey = JsonUtility.FromJson<OpenAIConfig>(config.text).api_key;
        }
    }

    // === NEW: public API to set stress level ===
    public void SetStressLevel(StressLevel level)
    {
        currentStress = level;
        Debug.Log($"Stress set to: {currentStress}");
    }
    public void SetStressLevelFromString(string userText)
    {
        if (string.IsNullOrEmpty(userText)) { currentStress = StressLevel.Unknown; return; }
        string s = userText.Trim().ToLowerInvariant();
        if (s.Contains("high")) currentStress = StressLevel.High;
        else if (s.Contains("medium") || s.Contains("mid")) currentStress = StressLevel.Medium;
        else if (s.Contains("low")) currentStress = StressLevel.Low;
        else currentStress = StressLevel.Unknown;
        Debug.Log($"Stress parsed from '{userText}' -> {currentStress}");
    }

    // === Public entry points ===
    public void SendCustomPrompt(string userPrompt)
    {
        StartCoroutine(SendPromptCoroutine(userPrompt));
    }

    // OPTIONAL: ask stress first, then send actual prompt when your app supplies the answer
    public IEnumerator AskStressLevelThenPrompt(string finalUserPrompt, Func<string> pollForUserAnswer, float pollIntervalSeconds = 0.3f)
    {
        // Ask
        if (TTSManager.instance != null)
            TTSManager.instance.Speak("Before we start, is your stress level high, medium, or low?");
        else
            Debug.Log("ASK: Before we start, is your stress level high, medium, or low?");

        // Wait until your UI/ASR sets an answer we can parse (via poll function you pass in)
        string answer = null;
        while (string.IsNullOrEmpty(answer))
        {
            answer = pollForUserAnswer?.Invoke();
            yield return new WaitForSeconds(pollIntervalSeconds);
        }

        SetStressLevelFromString(answer);
        yield return SendPromptCoroutine(finalUserPrompt);
    }

    // === Internal request ===
    private IEnumerator SendPromptCoroutine(string prompt)
    {
        Debug.Log("Sending GPT request…");

        string stressGuidance = StressGuidance(currentStress);

        // Build messages with stress-aware system content
        string payload = "{" +
            "\"model\":\"gpt-3.5-turbo\"," +
            "\"messages\":[" +
                "{\"role\":\"system\",\"content\":" + Quote(
                    "You are a meditation guide. Replies must always be under 15 words," +
                    " one short calming sentence, varied each time." +
                    " The user's current stress is: " + currentStress + "." +
                    " " + stressGuidance
                ) + "}," +
                "{\"role\":\"user\",\"content\":" + Quote(prompt) + "}" +
            "]" +
        "}";

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            ChatResponse resp = JsonUtility.FromJson<ChatResponse>(req.downloadHandler.text);
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

    // === Helpers ===
    private static string Quote(string s) => "\"" + Escape(s) + "\"";
    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string StressGuidance(StressLevel level)
    {
        switch (level)
        {
            case StressLevel.High:
                return "Use slower pace, reassurance, safety cues, deeper breathing cues.";
            case StressLevel.Medium:
                return "Use steady, balancing tone, mention releasing shoulder tension and calm focus.";
            case StressLevel.Low:
                return "Use light, uplifting tone, emphasize gentle presence and gratitude.";
            default:
                return "If stress is unknown, ask gently about current state before guiding.";
        }
    }
}


