using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float healthMax;

    private float _health;

    private void Awake()
    {
        _health = healthMax;
    }

    public void ChangeHealth(float amount)
    {
        _health += amount;
        
        Debug.Log(_health);
    }
}
