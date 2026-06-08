using UnityEngine;

public class LexAttackController : MonoBehaviour
{
    [SerializeField] private GameObject tileShotPrefab; // Tu prefab del sprite volador
    [SerializeField] private Transform shootPoint;      // Punto de salida de la boca

    private CharacterCombat combatUnit;

    void Awake()
    {
        combatUnit = GetComponent<CharacterCombat>();
    }

    // Este método lo llamará el Animation Event de tu animación de ataque
    public void ShotTile()
    {
        if (tileShotPrefab != null && shootPoint != null && combatUnit != null)
        {
            GameObject projObj = Instantiate(tileShotPrefab, shootPoint.position, Quaternion.identity);
            TileProjectile projectile = projObj.GetComponent<TileProjectile>();

            if (projectile != null)
            {
                // Inyectamos los datos del combate al proyectil instanciado
                bool isPlayer = gameObject.CompareTag("Player");
                projectile.Setup(combatUnit.attackTarget, combatUnit.pendingDamage, isPlayer);
            }
        }
    }
}