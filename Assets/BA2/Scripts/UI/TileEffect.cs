using UnityEngine;
using UnityEngine.InputSystem;

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
    // Variable para controlar si el ratón está encima en este frame
    private bool isHovered = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // 1. Verificamos que el ratón exista en el Input System
        if (Mouse.current == null) return;

        // 2. Obtenemos la posición del ratón en la pantalla y la convertimos al mundo 2D
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // 3. Comprobamos si la posición del ratón está dentro de este Collider
        bool wasHovered = isHovered;
        isHovered = col.OverlapPoint(mouseWorldPosition);

        // --- LÓGICA DE HOVER (Equivalente a OnMouseEnter / OnMouseExit) ---
        if (isHovered && !wasHovered)
        {
            if (!isSelected) spriteRenderer.sprite = hoverSprite;
        }
        else if (!isHovered && wasHovered)
        {
            if (!isSelected) spriteRenderer.sprite = normalSprite;
        }

        // --- LÓGICA DE CLIC (Equivalente a OnMouseDown) ---
        // wasPressedThisFrame detecta el momento exacto en que se hace clic izquierdo
        if (isHovered && Mouse.current.leftButton.wasPressedThisFrame)
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
    }

    public void MoveToWordArea(Vector3 targetPosition)
    {
        // Solo guardamos la posición original si es la PRIMERA vez que se selecciona.
        // Esto evita que se sobrescriba cuando el Manager reordena las letras arriba.
        if (!isSelected)
        {
            originalPosition = transform.position;
        }

        isSelected = true;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        col.isTrigger = true;

        transform.position = targetPosition;
    }


    public void ReturnToBoard() //Al regresar aun está aplicando la gravedad por lo que se sobre escribe uno ensima de otro.
    {
        isSelected = false;
        transform.position = originalPosition;

        rb.bodyType = RigidbodyType2D.Dynamic;

        // Revertimos el Trigger a la normalidad
        col.isTrigger = false;

        spriteRenderer.sprite = normalSprite;
    }
}