using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AttackAnimationTier
{
    [Tooltip("Longitud mínima de la palabra")]
    public int minLength;
    [Tooltip("Longitud máxima de la palabra")]
    public int maxLength;
    [Tooltip("Nombre del Trigger en el Animator (ej: ShotTile, SuperPunch)")]
    public string animationTriggerName;
}

public class LexAttackAnimationManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El Animator del personaje Lex")]
    public Animator lexAnimator;

    [Header("Niveles de Animación de Ataque")]
    public List<AttackAnimationTier> attackTiers = new List<AttackAnimationTier>();

    /// <summary>
    /// Selecciona y dispara la animación de ataque correspondiente a la longitud de la palabra.
    /// </summary>
    /// <param name="wordLength">La longitud de la palabra armada por el jugador.</param>
    public void TriggerAttackAnimation(int wordLength)
    {
        if (lexAnimator == null)
        {
            Debug.LogWarning("Falta asignar el Animator en LexAttackAnimationManager.");
            return;
        }

        AttackAnimationTier? selectedTier = null;

        // Buscamos si la longitud de la palabra encaja en algún rango
        foreach (var tier in attackTiers)
        {
            if (wordLength >= tier.minLength && wordLength <= tier.maxLength)
            {
                selectedTier = tier;
                break;
            }
        }

        // Si encontramos un rango válido, disparamos el trigger
        if (selectedTier.HasValue && !string.IsNullOrEmpty(selectedTier.Value.animationTriggerName))
        {
            lexAnimator.SetTrigger(selectedTier.Value.animationTriggerName);
            Debug.Log($"[LexAttack] Animación disparada: {selectedTier.Value.animationTriggerName} para palabra de {wordLength} letras.");
        }
        else
        {
            // Opcional: Puedes poner un ataque por defecto aquí si la palabra no entra en los rangos
            Debug.Log($"[LexAttack] No se encontró animación para una palabra de {wordLength} letras.");
        }
    }
}