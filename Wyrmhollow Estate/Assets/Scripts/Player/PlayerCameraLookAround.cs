using System;
using UnityEngine;

public class PlayerCameraLookAround : MonoBehaviour
{
    //This script goes on the camera
    //The camera needs to be parented to another object that follows where you actually want the camera
    [SerializeField] private float xSensitivity;
    [SerializeField] private float ySensitivity;

    [SerializeField] private Transform orientation;

    private float _xRotation;
    private float _yRotation;
    private Vector2 _lookRotation;
    private InputSystem_Actions _inputSystemActions;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _inputSystemActions = InputManager.instance.GetInputSystemActions();
    }

    private void Update()
    {
        _lookRotation = _inputSystemActions.Player.Look.ReadValue<Vector2>();
        _lookRotation = new Vector2(_lookRotation.x * xSensitivity, _lookRotation.y * ySensitivity) * Time.deltaTime;
        
        _yRotation += _lookRotation.x;
        _xRotation -= _lookRotation.y;

        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
    }
}
