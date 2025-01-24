using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;
    public float jumpHeight = 2f; // Hauteur du saut

    [Header("References")]
    public Transform cameraTransform;

    private CharacterController characterController;
    private Vector3 velocity;

    [Header("Character Visual")]
    [SerializeField] private GameObject model;

    private void Start()
    {
        // Initialize CharacterController
        characterController = GetComponent<CharacterController>();

        // Disable control for non-local players
        if (!IsOwner)
        {
            cameraTransform.gameObject.SetActive(false); // Disable the camera for other players
            enabled = false;
        }
        else
        {
            model.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOwner) return; // Ensure only the local player executes movement logic

        HandleMovement();
        HandleMouseLook();
        HandleJump(); // Gérer le saut
    }

    private void HandleMovement()
    {
        // Get input for movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Apply movement to the CharacterController
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity
        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Reset Y velocity if grounded
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
    }

    private void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        // Rotate player body horizontally (left-right)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically (up-down)
        float currentRotationX = cameraTransform.localRotation.eulerAngles.x;

        // Handle rotation wraparound (Unity uses 0 to 360 degrees)
        if (currentRotationX > 180) currentRotationX -= 360;

        // Add the mouseY delta to the current vertical rotation and clamp it
        float newRotationX = currentRotationX - mouseY;
        newRotationX = Mathf.Clamp(newRotationX, -89f, 89f); // Limit vertical rotation

        // Apply the new vertical rotation
        cameraTransform.localRotation = Quaternion.Euler(newRotationX, 0f, 0f);
    }

    private void HandleJump()
    {
        // Check if the player is grounded and pressing the jump button (spacebar)
        if (characterController.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                // Apply jump force
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }
        }
    }
}
