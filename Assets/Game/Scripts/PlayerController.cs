using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
    [FormerlySerializedAs("_readyToJump")] [SerializeField]
    private bool readyToJump;

    [Space]
    
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    [SerializeField] 
    private bool grounded;
    
    [FormerlySerializedAs("_walkSpeed")]
    [Space]
    
    [Header("Movement Multipliers")]
    
    [SerializeField]
    private float walkSpeed = 1;
    [FormerlySerializedAs("_sprintMultiplier")] [SerializeField]
    private float sprintMultiplier = 2f;

    private Vector3 _moveDirection;

    private Rigidbody _rb;
    private RaycastHit _hit;
    
    [Space]

    [Header("References")]
    public Transform cameraTransform;
    
    [Space]
    private Vector3 _velocity;

    [Header("Character Visual")]
    [SerializeField] private GameObject model;

    [Space]
    
    [Header("Collision")]
    public float sphereCastThickness = 0.5f;

    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;
    private bool _jumpInput;
    private bool _sprintInput;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();

        //InitializeInputs();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Start()
    {
        transform.position = PlayerSpawn.Main!.transform.position;
        
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        readyToJump = true;

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
            InitializeInputs();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        grounded = RotaryHeart.Lib.PhysicsExtension.Physics.SphereCast(transform.position, sphereCastThickness, Vector3.down, out _hit, playerHeight * 0.3f, ~(1 << LayerMask.NameToLayer("OnSecondScreen") | 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Ignore Raycast")), RotaryHeart.Lib.PhysicsExtension.PreviewCondition.Editor);

        HandleInputs();
        HandleMouseLook();
        SpeedControl();

        // handle drag
        if (grounded)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void InitializeInputs()
    {
        // Bind inputs
        _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        _inputActions.Player.Jump.performed += ctx =>
        {
            _jumpInput = true;
        };
        _inputActions.Player.Jump.canceled += ctx =>
        {
            _jumpInput = false;
        };

        _inputActions.Player.Sprint.performed += ctx =>
        {
            // ToggleRagdoll();
            _sprintInput = true;
        };
        _inputActions.Player.Sprint.canceled += ctx => _sprintInput = false;
    }
    
    private void MovePlayer()
    {
        // calculate movement direction
        _moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        
        if (grounded)   // On the ground
        {
            _rb.AddForce(_moveDirection.normalized * (moveSpeed * 10f * (sprinting ? walkSpeed*sprintMultiplier : walkSpeed)), ForceMode.Acceleration);
        }
        else if (!grounded) // In the air
        {
            _rb.AddForce(
                _moveDirection.normalized * (moveSpeed * 10f * airMultiplier * (sprinting ? walkSpeed * sprintMultiplier : walkSpeed)), ForceMode.Acceleration);
        }
    }
    
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }
    }

    private void HandleInputs()
    {
        // Convert move input to 3D movement
        Vector3 movement = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        // Handle sprinting
        sprinting = _sprintInput && grounded && movement.magnitude > 0;

        // Looks for jumping
        if (_jumpInput && readyToJump && grounded)
        {
            readyToJump = false;
            
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void Jump()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(transform.up * jumpForce * _rb.mass, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
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
    
    public void ToggleRagdoll()
    {
        _rb.freezeRotation = !_rb.freezeRotation;
    }
}
