using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float airSpeed;
    [SerializeField] private float groundDrag;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchedYScale;
    
    [Header("Ground Check Settings")]
    [SerializeField] private float raycastLength;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Slope Handling Settings")]
    [SerializeField] private float maxSlopeAngle;

    [SerializeField] private Transform orientation;
    
    private Vector2 _moveInput;

    private float _moveSpeed;
    private Vector3 _moveDirection;

    private float _normalYScale;
    
    private bool _isReadyToJump;

    private RaycastHit _slopeHit;
    
    private Rigidbody _rigidbody;
    private MovementState _movementState;

    private InputSystem_Actions _inputSystemActions;
    
    private enum MovementState
    {
        Crouching,
        Walking,
        Sprinting,
        Jumping
    }
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        _isReadyToJump = true;

        _normalYScale = transform.localScale.y;
    }

    private void Start()
    {
        _movementState = MovementState.Walking;
        _inputSystemActions = InputManager.instance.GetInputSystemActions();
        
        _inputSystemActions.Player.Jump.performed += OnJump;
        _inputSystemActions.Player.Sprint.performed += OnSprintStart;
        _inputSystemActions.Player.Sprint.canceled += OnSprintEnd;
        _inputSystemActions.Player.Crouch.performed += OnCrouchStart;
        _inputSystemActions.Player.Crouch.canceled += OnCrouchEnd;
    }

    private void OnDisable()
    {
        _inputSystemActions.Player.Jump.performed -= OnJump;
        _inputSystemActions.Player.Sprint.performed -= OnSprintStart;
        _inputSystemActions.Player.Sprint.canceled -= OnSprintEnd;
        _inputSystemActions.Player.Crouch.performed -= OnCrouchStart;
        _inputSystemActions.Player.Crouch.canceled -= OnCrouchEnd;
    }
    private void Update()
    {
        SetMovementState();
        GetInput();
        LimitSpeed();
        
        Debug.Log(_movementState);
        if (IsGrounded())
        {
            _rigidbody.linearDamping = groundDrag;
        }
        else
        {
            _rigidbody.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }


    private void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (!_isReadyToJump || !IsGrounded())
            return;
        _movementState = MovementState.Jumping;
        _isReadyToJump = false;
        
        _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
        Invoke(nameof(ResetJump), jumpCooldown);
    }
    private void ResetJump() => _isReadyToJump = true;

    private void OnSprintStart(InputAction.CallbackContext callbackContext) => _movementState = MovementState.Sprinting;
    private void OnSprintEnd(InputAction.CallbackContext callbackContext) => _movementState = MovementState.Walking;
    private void OnCrouchStart(InputAction.CallbackContext callbackContext) => _movementState = MovementState.Crouching;
    private void OnCrouchEnd(InputAction.CallbackContext callbackContext) => _movementState = MovementState.Walking;


    private void GetInput() => _moveInput = _inputSystemActions.Player.Move.ReadValue<Vector2>();

    private void SetMovementState()
    {
        transform.localScale = new Vector3(transform.localScale.x, _normalYScale, transform.localScale.z);
        
        switch (_movementState)
        {
            case MovementState.Crouching:
                transform.localScale = new Vector3(transform.localScale.x, crouchedYScale, transform.localScale.z);
                _moveSpeed = crouchSpeed;
                break;
            case MovementState.Walking:
                _moveSpeed = walkSpeed;
                break;
            case MovementState.Sprinting:
                _moveSpeed = sprintSpeed;
                break;
            case MovementState.Jumping:
                _moveSpeed = airSpeed;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MovePlayer()
    {
        _moveDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;
        
        if (OnSlope())
        {
            _rigidbody.AddForce(GetSlopeMoveDirection() * _moveSpeed, ForceMode.Force);

            if (_rigidbody.linearVelocity.y > 0)
            {
                _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        
        _rigidbody.AddForce(_moveDirection.normalized * _moveSpeed, ForceMode.Force);

        _rigidbody.useGravity = !OnSlope();
    }

    private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, raycastLength, groundLayerMask);

    private void LimitSpeed()
    {
        if (OnSlope())
        {
            if (_rigidbody.linearVelocity.magnitude > _moveSpeed)
            {
                _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _moveSpeed;
            }
        }
        else
        {
            Vector3 linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.y);
            if (linearVelocity.magnitude > _moveSpeed)
            {
                Vector3 limitedVelocity = linearVelocity.normalized * _moveSpeed;
                _rigidbody.linearVelocity = new Vector3(limitedVelocity.x, _rigidbody.linearVelocity.y, limitedVelocity.z);
            }
        }
    }


    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, raycastLength))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection() => Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;

}
