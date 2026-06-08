using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [Header("Referencias Locales")]
    public HealthController healthController;
    public Animator unitAnimator;

    [Header("Objetivo de Ataque")]
    public Transform attackTarget; // Lex apuntará al transform de Sfuffy y viceversa

    [HideInInspector] public int pendingDamage; // Almacena el daño antes de aplicar el golpe final

    public void PerformAttack()
    {
        // Si la unidad ya fue derrotada, no hace nada
        if (healthController.stats.Health <= 0) return;

        if (gameObject.CompareTag("Player"))
        {
            // Turno de Lex: El daño de su palabra ya fue calculado e inyectado por el WordBuilderManager
            if (unitAnimator != null)
            {
                unitAnimator.SetInteger("AttackPower", pendingDamage);
                unitAnimator.SetTrigger("Attack");
            }
        }
        else
        {
            // Turno del Enemigo (Sfuffy): Usa su daño por defecto definido en sus BaseStats
            pendingDamage = healthController.stats.baseDamage;

            if (unitAnimator != null)
            {
                unitAnimator.SetTrigger("Attack");
            }
            else
            {
                // Si Sfuffy aún no tiene animaciones listas, aplica daño directo para pruebas
                ApplyDirectDamageFallback();
            }
        }
    }

    // Método seguro por si el enemigo realiza un ataque instantáneo sin proyectiles físicos
    private void ApplyDirectDamageFallback()
    {
        HealthController targetHealth = attackTarget.GetComponent<HealthController>();
        if (targetHealth != null) targetHealth.stats.TakeDamage(pendingDamage);

        // Regresa el turno al jugador
        BattleManager.Instance.ChangeState(BattleState.PlayerTurn);
    }
}