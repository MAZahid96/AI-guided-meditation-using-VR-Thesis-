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
//         yield return new WaitForSeconds(2);
//         gpt.SendCustomPrompt("Begin guided meditation. Focus on slow breathing.");
//         yield return new WaitForSeconds(15);

//         float meditationTime = 120f; // 2 minutes
//         float elapsed = 0f;

//         while (elapsed < meditationTime)
//         {
//             gpt.SendCustomPrompt("Breathe in slowly.");
//             yield return new WaitForSeconds(8);

//             gpt.SendCustomPrompt("Breathe out gently.");
//             yield return new WaitForSeconds(8);

//             elapsed += 16f;
//         }

//         bool messageFinished = false;
//         gpt.onResponseComplete = () => { messageFinished = true; };

//         gpt.SendCustomPrompt("Your meditation has ended. Open your eyes when ready.");

//         // Wait until TTS finishes speaking
//         yield return new WaitUntil(() => messageFinished);

//         // Delay for 2 seconds after TTS ends (optional)
//         yield return new WaitForSeconds(2);

//         SceneManager.LoadScene("Night Scene");
//     }
// }
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Scene2Controller : MonoBehaviour
{
    private OpenAIManager gpt;

    void Start()
    {
        gpt = FindObjectOfType<OpenAIManager>();
        if (gpt == null)
        {
            Debug.LogError("OpenAIManager not found in scene!");
            return;
        }

        StartCoroutine(MeditationRoutine());
    }

    IEnumerator MeditationRoutine()
    {
        // Initial instruction
        yield return new WaitForSeconds(2);
        gpt.SendCustomPrompt("Begin guided meditation. Focus on slow breathing.");
        yield return new WaitForSeconds(15);

        // Main 2-minute meditation loop
        float meditationTime = 120f; // total meditation time
        float elapsed = 0f;

        while (elapsed < meditationTime)
        {
            gpt.SendCustomPrompt("Breathe in slowly.");
            yield return new WaitForSeconds(8);

            gpt.SendCustomPrompt("Breathe out gently.");
            yield return new WaitForSeconds(8);

            elapsed += 16f;
        }

        // End meditation message
        bool messageFinished = false;
        gpt.onResponseComplete = () => { messageFinished = true; };

        gpt.SendCustomPrompt("Your meditation has ended. Open your eyes when ready.");

        // Wait for TTS to finish
        yield return new WaitUntil(() => messageFinished);

        yield return new WaitForSeconds(2); // optional pause

        // Change scene
        SceneManager.LoadScene("Night Scene");
    }
}
