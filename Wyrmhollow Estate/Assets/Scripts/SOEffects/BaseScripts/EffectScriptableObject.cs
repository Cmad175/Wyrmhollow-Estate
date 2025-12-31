using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Create New Effect")]
public class EffectScriptableObject : ScriptableObject
{
    [SerializeField] private EffectTargetArea effectTargetArea;
    [SerializeField] private EffectRateType effectRateType;
    
    [SerializeField] private float amount;
    
    [ShowIf("effectRateType", EffectRateType.OverTime)]
    [SerializeField] private float tickRate;
    [ShowIf("effectRateType", EffectRateType.OverTime)]
    [SerializeField] private int tickAmount;
    
    public EffectTargetArea GetTargetArea() => effectTargetArea;
    public EffectRateType GetRateType() => effectRateType;
    public float GetTickRate() => tickRate;
    public int GetTickAmount() => tickAmount;
    public float GetAmount() => amount; 
}


