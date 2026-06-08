using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    [Header("Referencias Locales")]
    public HealthController healthController;
    public Animator unitAnimator;

    [Header("Objetivo de Ataque")]
    public Transform attackTarget;

    [HideInInspector] public int pendingDamage;

    // Llama a esto el WordBuilderManager (o el BattleManager para el enemigo)
    public void PerformAttack()
    {
        if (healthController.stats.Health <= 0) return;

        if (gameObject.CompareTag("Player"))
        {
            if (unitAnimator != null)
            {
                // Configura el daño para que el Animator sepa qué animación elegir (si tienes varias)
                //unitAnimator.SetInteger("AttackPower", pendingDamage);
                unitAnimator.SetTrigger("OnAttack");
            }
        }
        else
        {
            pendingDamage = healthController.stats.baseDamage;
            if (unitAnimator != null)
            {
                unitAnimator.SetTrigger("OnAttack");
            }
        }
    }

    // ==========================================
    // METODOS PARA ANIMATION EVENTS
    // ==========================================

    // 1. Pon este evento en el frame donde visualmente el golpe conecta (o la loseta choca)
    public void ApplyDamageEvent()
    {
        HealthController targetHealth = attackTarget.GetComponent<HealthController>();
        if (targetHealth != null)
        {
            targetHealth.stats.TakeDamage(pendingDamage);
            Debug.Log($"{gameObject.name} aplicó {pendingDamage} de daño a {attackTarget.name}");
        }
    }

    // 2. Pon este evento en el ÚLTIMO frame de la animación de ataque
    public void FinishTurnEvent()
    {
        if (gameObject.CompareTag("Player"))
        {
            BattleManager.Instance.ChangeState(BattleState.EnemyTurn);
        }
        else
        {
            BattleManager.Instance.ChangeState(BattleState.PlayerTurn);
        }
    }
}