using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EyeGazeTracker : MonoBehaviour
{
    public Camera vrCamera;
    private InputDevice eyeDevice;

    private float eyesClosedTimer = 0f;
    private const float eyesClosedThreshold = 3f; // 3 seconds to trigger scene

    void Start()
    {
        var eyeDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.CenterEye, eyeDevices);

        if (eyeDevices.Count > 0)
        {
            eyeDevice = eyeDevices[0];
            Debug.Log("Eye tracking device found: " + eyeDevice.name);
        }
        else
        {
            Debug.LogWarning("No eye tracking device found!");
        }
    }

    void Update()
    {
        if (eyeDevice.isValid && eyeDevice.TryGetFeatureValue(CommonUsages.eyesData, out Eyes eyesData))
        {
            // ✅ Eye openness (0.0 closed, 1.0 open)
            float leftOpen = 1f, rightOpen = 1f;

            if (eyesData.TryGetLeftEyeOpenAmount(out float left)) leftOpen = left;
            if (eyesData.TryGetRightEyeOpenAmount(out float right)) rightOpen = right;

            // Debug log openness
            Debug.Log($"Left Eye: {leftOpen:F2}, Right Eye: {rightOpen:F2}");

            // Check if both eyes are closed
            if (leftOpen < 0.2f && rightOpen < 0.2f)
            {
                eyesClosedTimer += Time.deltaTime;
                Debug.Log("Eyes closed for " + eyesClosedTimer.ToString("F1") + " seconds");

                // Trigger scene switch if closed long enough
                if (eyesClosedTimer >= eyesClosedThreshold)
                {
                    Debug.Log("✅ Eyes closed for 3 seconds, switching scene...");
                    SceneManager.LoadScene("Scene2"); // <-- Change to your meditation scene name
                }
            }
            else
            {
                eyesClosedTimer = 0f; // Reset if eyes open again
            }
        }

        // Gaze tracking (debug ray)
        if (TryGetEyeGaze(out Vector3 origin, out Vector3 direction))
        {
            Debug.DrawRay(origin, direction * 10f, Color.green);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 20f))
            {
                Debug.Log("Looking at: " + hit.collider.name);
            }
        }
    }

    bool TryGetEyeGaze(out Vector3 origin, out Vector3 direction)
    {
        origin = vrCamera.transform.position;
        direction = vrCamera.transform.forward;

        if (eyeDevice.isValid && eyeDevice.TryGetFeatureValue(CommonUsages.eyesData, out Eyes eyesData))
        {
            if (eyesData.TryGetFixationPoint(out Vector3 fixationPoint))
            {
                direction = (fixationPoint - origin).normalized;
                return true;
            }
        }

        return false;
    }
}
