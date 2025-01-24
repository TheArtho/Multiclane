using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Player Settings")]
    public float lookSensitivity = 2f;
    [Header("Movement Values")]
    public float moveSpeed;
    public float groundDrag;
    public bool sprinting = false;
    public float slopeLimit;

    [Space]
    
    [Header("Jump Values")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    [SerializeField]
    private bool _readyToJump;

    [Space]
    
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    [SerializeField]
    bool grounded;
    
    [Space]
    
    [Header("Movement Multipliers")]
    
    [SerializeField]
    private float _walkSpeed = 1;
    [SerializeField]
    private float _sprintMultiplier = 2f;

    private Vector3 moveDirection;

    private Rigidbody rb;
    private RaycastHit hit;
    
    [Space]

    [Header("References")]
    public Transform cameraTransform;
    
    [Space]
    private Vector3 velocity;

    [Header("Character Visual")]
    [SerializeField] private GameObject model;

    [Space]
    
    [Header("Collision")]
    public float sphereCastThickness = 0.5f;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool jumpInput;
    private bool sprintInput;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        // Bind inputs
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => jumpInput = true;
        inputActions.Player.Jump.canceled += ctx => jumpInput = false;

        inputActions.Player.Sprint.performed += ctx => sprintInput = true;
        inputActions.Player.Sprint.canceled += ctx => sprintInput = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        _readyToJump = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!IsOwner)
        {
            cameraTransform.gameObject.SetActive(false);
            enabled = false;
        }
        else
        {
            model.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        grounded = RotaryHeart.Lib.PhysicsExtension.Physics.SphereCast(transform.position, sphereCastThickness, Vector3.down, out hit, playerHeight * 0.3f, ~(1 << LayerMask.NameToLayer("OnSecondScreen") | 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Ignore Raycast")), RotaryHeart.Lib.PhysicsExtension.PreviewCondition.Editor);

        HandleInputs();
        HandleMouseLook();
        SpeedControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        if (grounded)   // On the ground
        {
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * (sprinting ? _walkSpeed*_sprintMultiplier : _walkSpeed)), ForceMode.Acceleration);
        }
        else if (!grounded) // In the air
        {
            rb.AddForce(
                moveDirection.normalized * (moveSpeed * 10f * airMultiplier * (sprinting ? _walkSpeed * _sprintMultiplier : _walkSpeed)), ForceMode.Acceleration);
        }
    }
    
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void HandleInputs()
    {
        // Convert move input to 3D movement
        Vector3 movement = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Handle sprinting
        sprinting = sprintInput && grounded && movement.magnitude > 0;

        // Looks for jumping
        if (jumpInput && _readyToJump && grounded)
        {
            _readyToJump = false;
            
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce * rb.mass, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        _readyToJump = true;
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
