// using UnityEngine;
// using UnityEngine.SceneManagement;
// using System.Collections;

// public class Scene2Controller : MonoBehaviour
// {
//     OpenAIManager gpt;

//     void Start()
//     {
//         gpt = FindObjectOfType<OpenAIManager>();
//         StartCoroutine(MeditationRoutine());
//     }

//     IEnumerator MeditationRoutine()
//     {
//         yield return new WaitForSeconds(5);

//         gpt.SendCustomPrompt("Begin guided meditation. Start with deep breathing.");
//         yield return Wait(10f);

//         gpt.SendCustomPrompt("Breathe in.");
//         yield return Wait(10f);

//         gpt.SendCustomPrompt("Breathe out.");
//         yield return Wait(10f);

//         gpt.SendCustomPrompt("Focus on the sound of your breath.");
//         yield return Wait(10f);

//         gpt.SendCustomPrompt("When you feel ready, gently open your eyes.");
//         yield return Wait(10f);

//         SceneManager.LoadScene("Night Scene");  // 
//     }

//     IEnumerator Wait(float t)
//     {
//         yield return new WaitForSeconds(t);
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
        gpt = FindObjectOfType<OpenAIManager>();
        StartCoroutine(MeditationRoutine());
    }

    IEnumerator MeditationRoutine()
    {
        yield return new WaitForSeconds(2);
        gpt.SendCustomPrompt("Begin guided meditation. Focus on slow breathing.");
        yield return new WaitForSeconds(15);

        float meditationTime = 120f; // 2 minutes
        float elapsed = 0f;

        while (elapsed < meditationTime)
        {
            gpt.SendCustomPrompt("Breathe in slowly.");
            yield return new WaitForSeconds(8);

            gpt.SendCustomPrompt("Breathe out gently.");
            yield return new WaitForSeconds(8);

            elapsed += 16f;
        }

        bool messageFinished = false;
        gpt.onResponseComplete = () => { messageFinished = true; };

        gpt.SendCustomPrompt("Your meditation has ended. Open your eyes when ready.");

        // Wait until TTS finishes speaking
        yield return new WaitUntil(() => messageFinished);

        // Delay for 2 seconds after TTS ends (optional)
        yield return new WaitForSeconds(2);

        SceneManager.LoadScene("Night Scene");
    }
}
