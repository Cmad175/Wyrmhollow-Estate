using UnityEngine;

public class EffectInstance
{
    public EffectScriptableObject effect;
    public int currentTickAmount;
    public float currentTickRate;

    public EffectInstance(EffectScriptableObject effectSO)
    {
        effect = effectSO;
        currentTickAmount = 0;
        currentTickRate = effect.GetTickRate();
    }
}
