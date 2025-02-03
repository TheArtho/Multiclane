using System;
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

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        inputActions.Enable();

        // Pause Menu input
        inputActions.Menu.Pause.performed += (ctx) =>
        {
            GameManager.Main.pauseMenu.gameObject.SetActive(!GameManager.Main.pauseMenu.gameObject.activeSelf);

            if (GameManager.Main.pauseMenu.gameObject.activeSelf)
            {
                GameManager.Main.pauseMenu.Activate();
            }
            else
            {
                GameManager.Main.pauseMenu.Deactivate();
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseLook();
    }

    private void OnDestroy()
    {
        inputActions.Disable();
    }
    
    private void HandleMouseLook()
    {
        // Can't move if pause menu is active
        if (GameManager.Main.pauseMenu.gameObject.activeSelf) return;
        
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
