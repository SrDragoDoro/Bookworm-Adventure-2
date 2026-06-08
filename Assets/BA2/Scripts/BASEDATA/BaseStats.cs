using System;
using UnityEngine;

[Serializable]
public class BaseStats
{
    public string name;
    public string description;  

    [Header("Health Settings")]
    public int maxHealth = 12; // 12 int = 3 corazones
    private int currentHealth;

    // Evento que se dispara cada vez que la vida cambia.
    // Pasa dos parámetros: (Vida actual, Vida máxima)
    public event Action<int, int> OnHealthChanged;

    public int Health
    {
        get => currentHealth;
        private set
        {
            // Mantenemos la vida entre 0 y el máximo
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