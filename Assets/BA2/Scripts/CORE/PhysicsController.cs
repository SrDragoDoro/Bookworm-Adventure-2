using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsController : MonoBehaviour
{
    private Rigidbody2D rb;
    public LayerMask layer;

    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        // Esto hace que la capa "Losetas" ignore a la capa "Suelo" por código
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Losetas"), LayerMask.NameToLayer("Suelo"), true);

    }

    // Update is called once per frame
    private void FixedUpdate() // Usamos FixedUpdate para la física
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 20, layer);

        if (hit.collider != null)
        {
            Debug.Log("Hit: " + hit.collider.name);
        }
    }
    private void Update()
    {       
    }
  
}
