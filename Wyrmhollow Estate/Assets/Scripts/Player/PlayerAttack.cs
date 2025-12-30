using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private InputManager _inputManager;
    private WeaponBase _weapon;

    private void Awake()
    {
        _weapon = GetComponentInChildren<WeaponBase>();
    }

    private void Start()
    {
        _inputManager = InputManager.instance;

        _inputManager.GetInputSystemActions().Player.Attack.performed += Attack;
    }


    private void Attack(InputAction.CallbackContext callbackContext)
    {
        _weapon.AttackAnimationTrigger();
    }
}
