using UnityEngine;

/// <summary>
/// Controlador de ataques para enemigos. Permite reproducir una animación de ataque
/// elegida aleatoriamente. La cantidad de variantes se configura manualmente desde el Inspector.
/// Ejemplo: si baseStateName = "Attack" y attackVariants = 3, intentará reproducir "Attack1","Attack2" o "Attack3".
/// Si no encuentra el Animator o las variantes, se usa como fallback el Trigger "Attack".
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    [Header("Configuración de animación")]
    [Tooltip("Animator que controla las animaciones del enemigo. Si no se asigna, se buscará en el GameObject/children/parent.")]
    [SerializeField] private Animator animator;

    [Tooltip("Base del nombre del estado en el Animator. Ej: 'Attack' -> Attack1, Attack2, ...")]
    [SerializeField] private string baseStateName = "Attack";

    [Tooltip("Número de variantes de ataque disponibles. Introduce manualmente cuántas animaciones existen (1,2,3,...)")]
    [SerializeField] private int attackVariants = 1;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null) animator = GetComponentInParent<Animator>();
        }
    }

    /// <summary>
    /// Reproduce una animación de ataque aleatoria según la configuración del inspector.
    /// </summary>
    public void PlayRandomAttack()
    {
        if (animator == null)
        {
            Debug.LogWarning($"[EnemyAttack] Animator no asignado para {gameObject.name}. Intentando fallback con Trigger 'Attack'.");
            // Intentamos hacer fallback con un trigger en caso de no tener Animator
            var cc = GetComponent<CharacterCombat>();
            if (cc != null && cc.unitAnimator != null)
            {
                cc.unitAnimator.SetTrigger("Attack");
            }
            return;
        }

        if (attackVariants <= 0)
        {
            // Si no hay variantes definidas, usamos Trigger por compatibilidad
            Debug.Log($"[EnemyAttack] attackVariants <= 0 en {gameObject.name}. Usando Trigger 'Attack' en Animator.");
            animator.SetTrigger("Attack");
            return;
        }

        int index = Random.Range(1, attackVariants + 1); // 1..attackVariants
        string stateName = baseStateName + index;

        Debug.Log($"[EnemyAttack] Reproduciendo animación aleatoria '{stateName}' en {gameObject.name} (variantes={attackVariants}).");

        // Intentamos reproducir por nombre de estado. Si no existe en el controller, el desarrollador debe ajustar nombres.
        animator.Play(stateName);
    }
}
