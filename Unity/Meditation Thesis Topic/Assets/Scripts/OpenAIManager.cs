using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

[System.Serializable] public class Choice { public Message message; }
[System.Serializable] public class Message { public string role; public string content; }
[System.Serializable] public class ChatResponse { public Choice[] choices; }

public class OpenAIManager : MonoBehaviour
{
    private string apiKey;
    private const string url = "https://api.openai.com/v1/chat/completions";

    public static OpenAIManager instance;

    public Action onResponseComplete;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep alive between scenes
        }
        else
        {
            Destroy(gameObject); // avoid duplicates
            return;
        }

        // Load API key from Resources/openai_config.json
        TextAsset config = Resources.Load<TextAsset>("openai_config");
        if (config != null)
        {
            apiKey = JsonUtility.FromJson<OpenAIConfig>(config.text).api_key;
        }
        else
        {
            Debug.LogError("Missing openai_config in Resources folder!");
        }
    }

    public void SendCustomPrompt(string userPrompt)
    {
        StartCoroutine(SendPromptCoroutine(userPrompt));
    }

    private IEnumerator SendPromptCoroutine(string prompt)
    {
        Debug.Log("Sending GPT request…");

        string payload = "{\"model\":\"gpt-3.5-turbo\",\"messages\":[" +
                         "{\"role\":\"system\",\"content\":\"You are a meditation guide. Your replies must always be under 15 words and delivered in one short, calming sentence.\"}," +
                         "{\"role\":\"user\",\"content\":\"" + Escape(prompt) + "\"}]}";

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            ChatResponse resp = JsonUtility.FromJson<ChatResponse>(req.downloadHandler.text);
            string response = resp.choices[0].message.content.Trim();
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

    private static string Escape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
