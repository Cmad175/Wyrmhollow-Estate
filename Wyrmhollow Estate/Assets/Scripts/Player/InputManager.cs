using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    private InputSystem_Actions _inputSystemActions;

    private void Awake()
    {
        instance = this;
        _inputSystemActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputSystemActions.Enable();
    }

    private void OnDisable()
    {
        _inputSystemActions.Disable();
    }

    public InputSystem_Actions GetInputSystemActions() => _inputSystemActions;
}
