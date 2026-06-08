using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [Header("Referencias Locales")]
    public HealthController healthController;
    public Animator unitAnimator;

    [Header("Objetivo de Ataque")]
    public Transform attackTarget; // Para Stuffy, arrastra aquí a Lex. Para Lex, arrastra a Stuffy.

    [HideInInspector] public int pendingDamage; // Almacena temporalmente el daño del turno activo

    /// <summary>
    /// Inicia la secuencia de ataque activando el Trigger en el Animator.
    /// </summary>
    public void PerformAttack()
    {
        // Seguridad: Si la unidad está muerta, no ataca
        if (healthController != null && healthController.stats.Health <= 0) return;

        if (gameObject.CompareTag("Player"))
        {
            // El daño de Lex ya fue calculado e inyectado desde WordBuilderManager antes de llamar este método
            if (unitAnimator != null)
            {
                unitAnimator.SetTrigger("OnAttack");
            }
        }
        else
        {
            // TURNO DEL ENEMIGO (Stuffy):
            // Leemos directamente el daño base de sus BaseStats asignadas en su HealthController
            if (healthController != null && healthController.stats != null)
            {
                pendingDamage = healthController.stats.baseDamage;
            }
            else
            {
                pendingDamage = 4; // Fallback de seguridad (1 corazón completo)
            }

            if (unitAnimator != null)
            {
                unitAnimator.SetTrigger("OnAttack"); // Dispara la animación de ataque de Stuffy
            }
            else
            {
                // Fallback por si el enemigo aún no tiene animaciones: aplica el daño de inmediato
                ApplyDamageEvent();
                FinishTurnEvent();
            }
        }
    }

    // ==========================================
    // MÉTODOS PARA EVENTOS DE ANIMACIÓN (ANIMATION EVENTS)
    // ==========================================

    /// <summary>
    /// Coloca este evento en el frame exacto donde Stuffy asesta el golpe o muerde.
    /// </summary>
    public void ApplyDamageEvent()
    {
        if (attackTarget == null) return;

        HealthController targetHealth = attackTarget.GetComponent<HealthController>();
        if (targetHealth != null)
        {
            // Aplica el daño guardado en las estadísticas del objetivo
            targetHealth.stats.TakeDamage(pendingDamage);
            Debug.Log($"[COMBATE] {gameObject.name} aplicó {pendingDamage} de daño a {attackTarget.name}");
        }
    }

    /// <summary>
    /// Coloca este evento en el ÚLTIMO frame de la animación de ataque de Stuffy.
    /// </summary>
    public void FinishTurnEvent()
    {
        if (gameObject.CompareTag("Player"))
        {
            // Si Lex terminó de atacar, pasa al turno del enemigo
            BattleManager.Instance.ChangeState(BattleState.EnemyTurn);
        }
        else
        {
            // Si Stuffy terminó de atacar, le devuelve el turno al jugador
            BattleManager.Instance.ChangeState(BattleState.PlayerTurn);
        }
    }
}