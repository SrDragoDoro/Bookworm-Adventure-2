using System;
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

public enum EnemyType
{
    None,
    Normal,
    Boss
}

public enum EnemyState
{
    None,
    Idle,
    Alerted,
    Attacking,
    Dead
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "BookWorm/EnemyData")]
public class EnemyData : ScriptableObject
{

    public MeleeAttackType MeleeAttackType;
    public TileAttackType TileAttackType;
    public EnemyType EnemyType;
    public EnemyState EnemyState;

    [Header("Datos Generales")]
    public string nameEntity;
    public string description;
    public float xpValue;

    [Header("Combate")]
    public int baseDamage = 4; // Daño base (útil para el enemigo)

    [Header("Vida (1 Corazón = 4 Puntos)")]
    public int maxHealth = 16; // Ej: 4 corazones = 16 int
    private int currentHealth;

    // Evento vital para que HealthController dibuje los corazones
    public event Action<int, int> OnHealthChanged;

    public event Action<EnemyState> OnStateChanged; // Evento para notificar cambios de estado del enemigo

    public void SetEnemyState(EnemyState newState)
    {
        EnemyState = newState;
        OnStateChanged?.Invoke(newState); // Aquí sí es legal llamarlo porque estamos dentro de su propia clase
    }

    public int Health
    {
        get => currentHealth;
        private set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    public void Initialize()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        Health -= amount;
    }

    public void Heal(int amount)
    {
        Health += amount;
    }


    public int GetHealth()
    {
        return maxHealth;
    }

}
