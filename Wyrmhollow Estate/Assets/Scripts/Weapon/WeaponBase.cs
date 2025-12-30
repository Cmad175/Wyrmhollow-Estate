using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [SerializeField] private List<EffectScriptableObject> effectList;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void AttackAnimationTrigger()
    {
        _animator.SetTrigger("Attack");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Found Enemy");
            var effectHandler = other.GetComponent<EffectHandler>();
            
            foreach (var effect in effectList)    
            {
                effectHandler.AddEffect(effect);
            }
        }
    }
}
