using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float crouchMultiplier = 0.5f;
    [SerializeField] private float airControl = 0.3f;
    [SerializeField] private float acceleration = 10f;
    
    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private int maxAirJumps = 0;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    
    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float slopeSlideSpeed = 8f;
    
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraPosition;
    
    private CapsuleCollider _capsuleCollider;

    private Vector3 _orientationStartPosition;
    private Rigidbody _rigidbody;
    private InputSystem_Actions _input;
    
    private Vector2 _moveInput;
    private bool _jumpPressed;
    private bool _sprintHeld;
    private bool _crouchHeld;
    
    private bool _isGrounded;
    private bool _onSlope;
    private Vector3 _slopeNormal;
    private float _slopeAngle;
    
    private int _airJumpsRemaining;
    private float _targetHeight;
    private float _currentHeight;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        _capsuleCollider = GetComponent<CapsuleCollider>();
        
        _currentHeight = standHeight;
        _targetHeight = standHeight;
        
    }
    
    private void Start()
    {
        _input = InputManager.instance.GetInputSystemActions();
        
        _input.Player.Jump.performed += ctx => _jumpPressed = true;
        _input.Player.Sprint.performed += ctx => _sprintHeld = true;
        _input.Player.Sprint.canceled += ctx => _sprintHeld = false;
        _input.Player.Crouch.performed += ctx => _crouchHeld = true;
        _input.Player.Crouch.canceled += ctx => _crouchHeld = false;
    }
    
    private void Update()
    {
        _moveInput = _input.Player.Move.ReadValue<Vector2>();
        
        HandleCrouch();
        
        CheckGround();
        
        if (_isGrounded)
            _airJumpsRemaining = maxAirJumps;
    }
    
    private void FixedUpdate()
    {
        ApplyMovement();
        
        if (_jumpPressed)
        {
            Jump();
            _jumpPressed = false;
        }
        
        
        if (!_isGrounded)
        {
            _rigidbody.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }
        
        
        if (_onSlope && _slopeAngle > maxSlopeAngle)
        {
            _rigidbody.AddForce(Vector3.ProjectOnPlane(Vector3.down, _slopeNormal) * slopeSlideSpeed, ForceMode.Force);
        }
    }
    
    private void ApplyMovement()
    {
        // Get move direction relative to camera
        Vector3 forward = orientation.forward;
        Vector3 right = orientation.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        Vector3 moveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;
        
        // Calculate target speed
        float targetSpeed = moveSpeed;
        
        if (_sprintHeld && !_crouchHeld && _isGrounded)
            targetSpeed *= sprintMultiplier;
        else if (_crouchHeld)
            targetSpeed *= crouchMultiplier;
        
        // Adjust for air control
        if (!_isGrounded)
            targetSpeed *= airControl;
        
        // Project movement onto slope if applicable
        if (_onSlope && _slopeAngle <= maxSlopeAngle)
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, _slopeNormal).normalized;
        }
        
        Vector3 targetVelocity = moveDirection * targetSpeed;
        
        // Get current velocity (use full velocity on slopes to prevent bouncing)
        Vector3 currentVelocity;
        if (_onSlope && _slopeAngle <= maxSlopeAngle)
        {
            currentVelocity = _rigidbody.linearVelocity;
        }
        else
        {
            currentVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        }
        
        // Calculate force needed
        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = Vector3.ClampMagnitude(velocityChange, acceleration * Time.fixedDeltaTime * targetSpeed);
        
        // Apply force
        _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        
        // Limit max speed
        if (_onSlope && _slopeAngle <= maxSlopeAngle)
        {
            // On slopes, limit the total velocity magnitude
            if (_rigidbody.linearVelocity.magnitude > targetSpeed)
            {
                _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * targetSpeed;
            }
        }
        else
        {
            // On flat ground/air, only limit horizontal velocity
            Vector3 flatVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            if (flatVelocity.magnitude > targetSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * targetSpeed;
                _rigidbody.linearVelocity = new Vector3(limitedVelocity.x, _rigidbody.linearVelocity.y, limitedVelocity.z);
            }
        }
    }
    
    private void Jump()
    {
        if (_isGrounded || _airJumpsRemaining > 0)
        {
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            
            float jumpForce = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * gravityMultiplier * jumpHeight);
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            
            if (!_isGrounded)
                _airJumpsRemaining--;
        }
    }
    
    private void CheckGround()
    {
        Vector3 checkPosition = groundCheck != null ? groundCheck.position : transform.position;
        _isGrounded = Physics.CheckSphere(checkPosition, groundDistance, groundMask);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, _capsuleCollider.height * 0.5f + 0.5f, groundMask))
        {
            _slopeNormal = hit.normal;
            _slopeAngle = Vector3.Angle(Vector3.up, _slopeNormal);
            _onSlope = _slopeAngle > 0.1f && _slopeAngle <= maxSlopeAngle + 10f; 
        }
        else
        {
            _onSlope = false;
            _slopeAngle = 0f;
        }
    }
    
    private void HandleCrouch()
    {
        if (_crouchHeld)
            _targetHeight = crouchHeight;
        else if (!CheckCeiling())
            _targetHeight = standHeight;
        
        if (Mathf.Abs(_currentHeight - _targetHeight) > 0.01f)
        {
            _currentHeight = Mathf.Lerp(_currentHeight, _targetHeight, Time.deltaTime * crouchTransitionSpeed);
            
            _capsuleCollider.height = _currentHeight;
            _capsuleCollider.center = new Vector3(0f, _currentHeight * 0.5f, 0f);
            
            Vector3 camPos = cameraPosition.localPosition;
            camPos.y = _currentHeight - 0.2f; 
            cameraPosition.localPosition = camPos;
        }
    }
    
    private bool CheckCeiling()
    {
        float checkHeight = standHeight - _currentHeight + 0.1f;
        return Physics.Raycast(transform.position, Vector3.up, checkHeight, groundMask);
    }
}
