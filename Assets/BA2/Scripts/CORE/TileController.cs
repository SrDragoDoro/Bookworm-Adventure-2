using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
/*
TileController se encarga de manejar el
comportamiento individual de cada baldosa,
incluyendo su interacción con el mouse, su
movimiento hacia el tablero o hacia su posición
original, y su estado (seleccionada o no). 
*/

public class TileController : TilesData, IDamageDealer
{
    private WordBuilderManager wordBuilderManager;

    [Header("Sprites")]
    [HideInInspector] public Sprite normalSprite;
    [HideInInspector] public Sprite hoverSprite;
    public int currentDamageMultiplier = 1;

    [Header("Configuración del Glow Individual")]
    [SerializeField] private SpriteRenderer glowSpriteRenderer;// El SpriteRenderer hijo para el resplandor
    [SerializeField] private Sprite glowLeft;                  // Sprite del extremo izquierdo
    [SerializeField] private Sprite glowCenter;                // Sprite del cuerpo central
    [SerializeField] private Sprite glowRight;                 // Sprite del extremo derecho

    [Header("Eventos")]
    public UnityEvent onTileInteracted;

    [Header("Movimiento")]
    public float moveSpeed = 15f;

    [Header("Configuración Visual y Tipos")]
    public TileType currentTileType = TileType.Basic;
    public List<TileVisualData> visualDatabase;             // Aquí van los sprites en el Inspector

    [Header("Referencias Hijas")]
    public TextMeshPro textMesh;                            // Arrastrar el TextMeshPro hijo aquí desde el Inspector
    [HideInInspector] public string currentLetter = "";   

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool isMovingBack = false;

    private Vector3 targetPosition;
    private bool isHovered = false;

    private bool isMovingToTarget = false; //Controlar si la baldosa debe moverse por código o si dejamos actuar a la gravedad

    private SortingGroup sortingGroup;

    #region Prueba de Palabras especificas desde el Inspector
    //void Start()
    //{
    //    // Si la loseta ya tiene un TextMeshPro asignado y tú escribiste algo en él desde el inspector...
    //    if (textMesh != null && string.IsNullOrEmpty(currentLetter) && !string.IsNullOrEmpty(textMesh.text))
    //    {
    //        // Sincronizamos la variable interna con el texto visual del componente
    //        currentLetter = textMesh.text;

    //        // Corremos el SetupTile para asegurarnos de que cargue sus sprites e información base
    //        SetupTile(currentTileType, currentLetter);
    //    }
    //}
    #endregion

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        // 3. Obtenemos el componente. Si se te olvida ponerlo en el editor, el código lo añade por seguridad.
        sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup == null) sortingGroup = gameObject.AddComponent<SortingGroup>();
    }

    void Update()
    {
        HandleMouseInput();
        HandleMovement();
    }

    #region IMPLEMENTACIÓN DE INTERFAZ
    public int GetFinalDamage(int baseDamage)
    {
        // La loseta calcula su daño multiplicando el valor base de la letra por su multiplicador de gema
        return baseDamage * currentDamageMultiplier;
    }
    #endregion


    #region Glow
    /// <summary>
    /// Activa el resplandor de esta loseta adaptando el sprite según su posición en la palabra.
    /// </summary>
    /// <param name="positionIndex">0 = Izquierda, 1 = Centro, 2 = Derecha</param>

    public void EnableTileGlow(int positionIndex)
    {
        if (glowSpriteRenderer == null) return;

        glowSpriteRenderer.gameObject.SetActive(true);

        switch (positionIndex)
        {
            case 0:
                glowSpriteRenderer.sprite = glowLeft;
                break;
            case 1:
                glowSpriteRenderer.sprite = glowCenter;
                break;
            case 2:
                glowSpriteRenderer.sprite = glowRight;
                break;
        }
    }
        
    // Apaga por completo el resplandor de esta loseta.   
    public void DisableTileGlow()
    {
        if (glowSpriteRenderer != null)
        {
            glowSpriteRenderer.gameObject.SetActive(false);
        }
    }
    #endregion

    
    // Llama a esta función justo después de instanciar la loseta
    // para definir qué es (Gema, Básica, etc.) y qué letra tiene.
    public void SetupTile(TileType newType, string letter)
    {
        currentTileType = newType;
        currentLetter = letter;

        if (textMesh != null)
        {
            textMesh.text = currentLetter;
        }
        // Buscar el diseño correspondiente en nuestra base de datos
        foreach (TileVisualData visual in visualDatabase)
        {
            if (visual.type == newType)
            {
                // Asignamos los sprites correctos para este tipo
                normalSprite = visual.normalSprite;
                hoverSprite = visual.hoverSprite;
                currentDamageMultiplier = visual.damageMultiplier;

                // Actualizamos el renderizador inmediatamente
                spriteRenderer.sprite = isHovered ? hoverSprite : normalSprite;
                break;
            }
        }
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
            // 4. TRAER AL FRENTE: Subimos el orden general de toda la loseta
            if (sortingGroup != null) sortingGroup.sortingOrder = 10; // Un número alto para que tape el escenario
        }
        else if (!selecting)
        {
            isSelected = false;
            isMovingBack = true;
            spriteRenderer.sprite = normalSprite;
            rb.bodyType = RigidbodyType2D.Kinematic;

            // 5. ENVIAR AL FONDO: Regresamos la loseta a su estado oculto tras el tablero
            // Ajusta este número al valor negativo necesario para que se esconda detrás de tu marco
            if (sortingGroup != null) sortingGroup.sortingOrder = -5;
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