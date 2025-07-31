using UnityEngine;

public class TTSInitializer : MonoBehaviour
{
    void Awake()
    {
        if (TTSManager.instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("TTSManager");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("TTSManager instantiated from prefab.");
            }
            else
            {
                Debug.LogError("TTSManager prefab not found in Resources folder!");
            }
        }
    }
}
