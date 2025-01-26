using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FpsCamera : MonoBehaviour
{
    [Header("Player Settings")]
    public float lookSensitivity = 2f;
    [SerializeField]
    private Transform cameraTransform;

    // Update is called once per frame
    void Update()
    {
        HandleMouseLook();
    }
    
    private void HandleMouseLook()
    {
        Vector2 lookInput = Mouse.current.delta.ReadValue() * Time.smoothDeltaTime;
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        // Rotate the player horizontally (left-right)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically (up-down)
        float currentRotationX = cameraTransform.localRotation.eulerAngles.x;
        if (currentRotationX > 180) currentRotationX -= 360;
        float newRotationX = Mathf.Clamp(currentRotationX - mouseY, -89f, 89f);

        cameraTransform.localRotation = Quaternion.Euler(newRotationX, 0f, 0f);
    }
}
