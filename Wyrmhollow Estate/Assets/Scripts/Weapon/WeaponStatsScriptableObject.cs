using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponStat", menuName = "Create New Weapon Stat")]
public class WeaponStatsScriptableObject : ScriptableObject
{
    public int durability;
    public int baseDamage;
}
