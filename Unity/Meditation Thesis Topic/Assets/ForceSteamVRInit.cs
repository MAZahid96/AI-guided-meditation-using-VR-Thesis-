using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class ForceSteamVRInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (!OpenVR.IsHmdPresent())
        {
            Debug.LogError("[VR]NO HMD resent"); return;

        }
        if(!SteamVR.active)
        {
            SteamVR.Initialize();
            Debug.Log("[VR]SteamVR initialiyed");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
