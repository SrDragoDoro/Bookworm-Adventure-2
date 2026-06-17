using UnityEngine;


public enum MeleeAttackType
{
    None,
    Normal,
    Poison,
    Fire,
}
public enum TileAttackType
{
    None,
    Debuff,
    Confunsion
}


[CreateAssetMenu(fileName = "EnemyData", menuName = "BookWorms/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string EnemyName;
    public int maxHeartsPerRow = 10;
    public int BaseAttack;
    public MeleeAttackType MeleeAttackType;
    public TileAttackType TileAttackType;
}
