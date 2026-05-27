using UnityEngine;

public class TileShot : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
  
    private Rigidbody2D rb;
    private bool hasCollided = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Le damos impulso inicial hacia la derecha (x = 1)
        rb.linearVelocity = new Vector2(speed, 0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Validamos si choca con el enemigo y que no haya chocado antes
        if (collision.gameObject.CompareTag("Enemy") && !hasCollided)
        {
            hasCollided = true;            

            // Rotación física al rebotar
            rb.angularVelocity = 180f;

            // Desaparecer/Destruir la loseta tras 1 segundo del rebote
            Destroy(gameObject, 1f);
        }
    }
}