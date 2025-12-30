using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectHandler : MonoBehaviour
{
    private List<EffectInstance> _currentEffects;
    private Health _health;

    private void Awake()
    {
        _currentEffects = new List<EffectInstance>();
        _health = GetComponent<Health>();
    }

    public void AddEffect(EffectScriptableObject effect)
    {
        _currentEffects.Add(new EffectInstance(effect));
    }

    private void Update()
    {
        if (_currentEffects.Count <= 0)
        {
            return;
        }

        TriggerEffects();
    }

    private void TriggerEffects()
    {
        foreach (var instance in _currentEffects)
        {
            if (instance.currentTickRate > 0)
            {
                instance.currentTickRate -= Time.deltaTime;
            }
            else
            {
                switch (instance.effect.GetTargetArea())
                {
                    case EffectTargetArea.Health:
                        _health.ChangeHealth(instance.effect.GetAmount());
                        break;
                }

                instance.currentTickAmount++;
                instance.currentTickRate = instance.effect.GetTickRate();
            }
            
        }

        _currentEffects.RemoveAll(x => x.effect.GetRateType() == EffectRateType.OneShot);
        _currentEffects.RemoveAll(x => x.currentTickAmount >= x.effect.GetTickAmount());
    }
}
