using UnityEngine;

public class LexAttackController : MonoBehaviour
{
    [SerializeField] private GameObject tileShotPrefab;
    [SerializeField] private Transform shootPoint; // Un objeto vacío en la boca de Lex

    // Este método lo llamará la animación de Unity
    public void ShotTile()
    {
        if (tileShotPrefab != null && shootPoint != null)
        {
            Instantiate(tileShotPrefab, shootPoint.position, Quaternion.identity);
        }
    }
}