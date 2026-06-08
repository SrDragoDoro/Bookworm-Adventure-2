using UnityEngine;

public class TileShot : MonoBehaviour
{
    [SerializeField] private float speed = 12f;

    private Rigidbody2D rb;
    private bool hasCollided = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Vuela hacia la derecha
        rb.linearVelocity = new Vector2(speed, 0f);

        // Mecanismo de seguridad: se destruye sí o sí después de 3 segundos por si sale del mapa
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo un efecto visual de rebote al chocar con el enemigo
        if (collision.gameObject.CompareTag("Enemy") && !hasCollided)
        {
            hasCollided = true;

            // Frenamos y hacemos que rebote hacia arriba/atrás
            rb.linearVelocity = new Vector2(-2f, 4f);
            rb.angularVelocity = 360f;

            // Desaparecer tras 0.5 segundos del rebote
            Destroy(gameObject, 0.5f);
        }
    }
}