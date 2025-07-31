// using UnityEngine;
// using UnityEngine.Networking;
// using System;
// using System.Collections;

// public class TTSManager : MonoBehaviour
// {
//     public AudioSource audioSource;

//     public void Speak(string text, Action onComplete = null)
//     {
//         StartCoroutine(DownloadAndPlay(text, onComplete));
//     }

//     private IEnumerator DownloadAndPlay(string text, Action onComplete = null)
//     {
//         string url = $"https://translate.google.com/translate_tts?ie=UTF-8&q={UnityWebRequest.EscapeURL(text)}&tl=en&client=tw-ob";

//         using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
//         {
//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("TTS Error: " + www.error);
//             }
//             else
//             {
//                 AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
//                 audioSource.clip = clip;
//                 audioSource.Play();

//                 yield return new WaitForSeconds(clip.length + 3f);
//                 onComplete?.Invoke();  // Callback when done
//             }
//         }
//     }

//     void Awake()
//     {
//         DontDestroyOnLoad(this.gameObject);
//     }
// }
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TTSManager : MonoBehaviour
{
    public static TTSManager instance;
    public AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-add AudioSource if missing
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        else
        {
            Destroy(gameObject); // avoid duplicates
        }
    }

    public void Speak(string text, System.Action onComplete = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning("TTSManager received empty text.");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DownloadAndPlay(text, onComplete));
    }

    private IEnumerator DownloadAndPlay(string text, System.Action onComplete = null)
    {
        string url = $"https://translate.google.com/translate_tts?ie=UTF-8&q={UnityWebRequest.EscapeURL(text)}&tl=en&client=tw-ob";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("TTS Error: " + www.error);
                onComplete?.Invoke();
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

            if (audioSource == null)
            {
                Debug.LogWarning("TTSManager has no AudioSource.");
                onComplete?.Invoke();
                yield break;
            }

            audioSource.clip = clip;
            audioSource.Play();
            Debug.Log("TTS playing: " + text);

            yield return new WaitForSeconds(clip.length + 0.2f);
            onComplete?.Invoke(); // Callback when done
        }
    }
}
