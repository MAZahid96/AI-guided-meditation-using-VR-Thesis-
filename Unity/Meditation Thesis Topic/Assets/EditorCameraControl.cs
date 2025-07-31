using UnityEngine;

public class EditorCameraControl : MonoBehaviour
{
    public float sensitivity = 2f; // Define sensitivity

    void Update()
    {
        // Only allow mouse look when right mouse button is held (optional)
        if (Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse X") * sensitivity;
            float rotY = Input.GetAxis("Mouse Y") * sensitivity;

            Vector3 currentRotation = transform.localEulerAngles;
            currentRotation.y += rotX;
            currentRotation.x -= rotY;

            transform.localEulerAngles = currentRotation;
        }
    }
}
