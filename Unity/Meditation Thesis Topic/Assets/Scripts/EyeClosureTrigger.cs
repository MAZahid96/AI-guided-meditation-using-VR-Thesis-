using UnityEngine;
using UnityEngine.SceneManagement;

public class EyeClosureTrigger : MonoBehaviour
{
    public string nextSceneName = "meditationScene";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            SceneManager.LoadScene(nextSceneName);
    }
}
