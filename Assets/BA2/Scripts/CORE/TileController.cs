using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
/*
TileController se encarga de manejar el
comportamiento individual de cada baldosa,
incluyendo su interacción con el mouse, su
movimiento hacia el tablero o hacia su posición
original, y su estado (seleccionada o no). 
*/
public class TileController : MonoBehaviour
{
    private WordBuilderManager wordBuilderManager;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite hoverSprite;

    [Header("Eventos")]
    public UnityEvent onTileInteracted;

    [Header("Movimiento")]
    public float moveSpeed = 15f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool isMovingBack = false;

    private Vector3 targetPosition;
    private bool isHovered = false;

    private bool isMovingToTarget = false; //Controla si la baldosa debe moverse por código o si dejamos actuar a la gravedad

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        HandleMouseInput();
        HandleMovement();
    }

    private void HandleMouseInput()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        bool wasHovered = isHovered;
        isHovered = col.OverlapPoint(mousePos);

        // Lógica de Hover
        if (isHovered && !wasHovered && !isSelected)
            spriteRenderer.sprite = hoverSprite;
        else if (!isHovered && wasHovered && !isSelected)
            spriteRenderer.sprite = normalSprite;

        // Clic Izquierdo para Seleccionar/Deseleccionar
        if (isHovered && Mouse.current.leftButton.wasPressedThisFrame)
        {
            onTileInteracted?.Invoke();

            if (!isSelected)
                WordBuilderManager.Instance.SelectTile(this);
            else
                WordBuilderManager.Instance.DeselectTile(this);
        }

        // Clic Derecho: Regresar TODAS las baldosas al tablero a la vez
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (WordBuilderManager.Instance.GetSelectedTiles().Count > 0)
            {
                WordBuilderManager.Instance.ClearAllTiles();
            }
        }
    }

    private void HandleMovement()
    {
        // SOLO nos movemos por código si el Manager nos dio una orden (isMovingToTarget = true)
        if (isMovingToTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Si acaba de llegar a su destino
            if (transform.position == targetPosition)
            {
                isMovingToTarget = false; // Ya llegó, apagamos el movimiento por código

                if (isMovingBack)
                {
                    isMovingBack = false;
                    col.isTrigger = false;
                    WordBuilderManager.Instance.CheckAndUnfreeze(); // Avisa al manager que llegó al tablero
                }
            }
        }
    }



    public void MoveTo(Vector3 newPos, bool selecting)
    {
        isMovingToTarget = true; // Activamos el movimiento por código para que vaya al destino

        if (selecting && !isSelected)
        {
            originalPosition = transform.position; // Guarda su posición real después de caer
            isSelected = true;
            isMovingBack = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
            col.isTrigger = true;
        }
        else if (!selecting)
        {
            isSelected = false;
            isMovingBack = true;
            spriteRenderer.sprite = normalSprite;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        targetPosition = newPos;
    }

    public void SetDynamic()
    {
        if (!isSelected)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.WakeUp();
        }
    }

    public void SetStatic()
    {
        if (!isSelected)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
}