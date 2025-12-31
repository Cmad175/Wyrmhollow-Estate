using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [SerializeField] private List<EffectScriptableObject> effectList;
    [SerializeField] private WeaponStatsScriptableObject weaponStats;
    
    private Animator _animator;

    private int _currentDurability;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _currentDurability = weaponStats.durability;
    }

    public void AttackAnimationTrigger()
    {
        _animator.SetTrigger("Attack");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            _currentDurability--;
            Debug.Log(_currentDurability);
            var effectHandler = other.GetComponent<EffectHandler>();
            
            foreach (var effect in effectList)    
            {
                effectHandler.AddEffect(effect);
            }
        }
    }
}
