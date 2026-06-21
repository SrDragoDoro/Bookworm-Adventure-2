using UnityEngine;

public class TileProjectile : MonoBehaviour
{
    private Transform target;
    private int damageToApply;
    private bool isPlayerProjectile;
    private float flySpeed = 12f;

    public void Setup(Transform targetTransform, int damage, bool fromPlayer)
    {
        target = targetTransform;
        damageToApply = damage;
        isPlayerProjectile = fromPlayer;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Mover el proyectil hacia el objetivo de forma fluida
        transform.position = Vector3.MoveTowards(transform.position, target.position, flySpeed * Time.deltaTime);

        // Control de llegada / Impacto
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        HealthController targetHealth = target.GetComponent<HealthController>();

        if (targetHealth != null)
        {
            // Aquí se aplica el daño real a las estadísticas y se caen los corazones
            targetHealth.stats.TakeDamage(damageToApply);
        }

        // Cambio de turnos coordinado post-impacto
        if (isPlayerProjectile)
        {
            GameManager.instance.battleManager.ChangeState(BattleState.EnemyTurn);
        }
        else
        {
            GameManager.instance.battleManager.ChangeState(BattleState.PlayerTurn);
        }

        Destroy(gameObject);
    }
}