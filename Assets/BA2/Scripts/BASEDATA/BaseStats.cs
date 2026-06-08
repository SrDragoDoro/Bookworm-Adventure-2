using System;
using UnityEngine;

[Serializable]
public class BaseStats
{
    [Header("Datos Generales")]
    public string name;
    public string description;
    public float xpValue;

    [Header("Combate")]
    public int baseDamage = 4; // Daño base (útil para el enemigo)

    [Header("Vida (1 Corazón = 4 Puntos)")]
    public int maxHealth = 16; // Ej: 4 corazones = 16 int
    private int currentHealth;

    // Evento vital para que HealthController dibuje los corazones
    public event Action<int, int> OnHealthChanged;

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
}