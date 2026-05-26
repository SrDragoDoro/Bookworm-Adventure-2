using UnityEngine;

public class TileEffect : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite; // Asigna tile_basic_0
    public Sprite hoverSprite;  // Asigna tile_basic_1

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public bool isSelected = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    // Efecto cuando el ratón pasa por encima
    void OnMouseEnter()
    {
        if (!isSelected) spriteRenderer.sprite = hoverSprite;
    }

    // Efecto cuando el ratón sale
    void OnMouseExit()
    {
        if (!isSelected) spriteRenderer.sprite = normalSprite;
    }

    // Seleccionar o deseleccionar al hacer clic
    void OnMouseDown()
    {
        if (!isSelected)
        {
            WordManager.Instance.SelectTile(this);
        }
        else
        {
            WordManager.Instance.DeselectTile(this);
        }
    }

    // Llamado por el Manager cuando esta ficha va al centro
    public void MoveToWordArea(Vector3 targetPosition)
    {
        isSelected = true;
        originalPosition = transform.position; // Guardamos su lugar en el tablero

        rb.bodyType = RigidbodyType2D.Kinematic; // Apagamos su gravedad
        rb.velocity = Vector2.zero;
        col.enabled = false; // Desactivamos colisiones para que no empuje a otras al subir

        transform.position = targetPosition; // Movemos la ficha al centro
    }

    // Llamado por el Manager si el jugador se arrepiente
    public void ReturnToBoard()
    {
        isSelected = false;
        transform.position = originalPosition; // Vuelve a su hueco exacto

        rb.bodyType = RigidbodyType2D.Dynamic; // Reactivamos su gravedad
        col.enabled = true; // Reactivamos colisiones
        spriteRenderer.sprite = normalSprite;
    }
}