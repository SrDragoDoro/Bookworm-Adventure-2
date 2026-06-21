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


[CreateAssetMenu(fileName = "EnemyData", menuName = "BookWorm/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string EnemyName;
    public int maxHealth = 10; //Vida actual
    public int BaseAttack;
    public MeleeAttackType MeleeAttackType;
    public TileAttackType TileAttackType;
}
