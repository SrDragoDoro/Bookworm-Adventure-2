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

public enum TableState
{
    None,
    Playing,
    Stopped    
}


public class TileController : TilesData
{
    private WordBuilderManager wordBuilderManager;
    public TableState currentTableState = TableState.Playing; // Estado actual del tablero (jugando, detenido, etc.)

    [Header("Sprites de la Loseta")]
    [HideInInspector] public Sprite normalSprite;
    [HideInInspector] public Sprite hoverSprite;
    [HideInInspector] public int currentDamageMultiplier = 1;

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
    // NOTA: TileType y TileVisualData se leen automáticamente del padre TilesData.cs
    public TileType currentTileType = TileType.Basic;
    public List<TileVisualData> visualDatabase;             // Aquí van los sprites en el Inspector

    [Header("Referencias Hijas")]
    public TextMeshPro textMesh;                            // Arrastrar el TextMeshPro hijo aquí desde el Inspector
    [HideInInspector] public string currentLetter = "";     // Guarda la letra / sílaba actual(Ej: "A", "Ñ", "Qu")

    [Header("Efecto Revolver")]
    [SerializeField] private int scrambleCycles = 6;           // Cuántas letras falsas mostrará antes de la real
    [SerializeField] private float scrambleDelay = 0.08f;      // Tiempo entre cada cambio de letra (en segundos)

    [Header("Configuración de Tamaño de Fuente")]
    [SerializeField] private float normalFontSize = 1f; // Pon aquí el tamaño por defecto de tus letras en el Inspector
    [SerializeField] private float doubleLetterFontSize = 0.5f; // El tamaño más pequeño para "QU" o similares

    private Coroutine scrambleCoroutine;     // Para detenerla si el jugador interactúa rápido (llamados simultaneos)

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private SpriteRenderer spriteRenderer;
    private SortingGroup sortingGroup;

    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool isMovingBack = false;
    private Vector3 targetPosition;
    private bool isHovered = false;
    private bool isMovingToTarget = false; //Controlar si la baldosa debe moverse por código o si dejamos actuar a la gravedad

    public static System.Action<TableState> OnGlobalTableStateChanged;


    #region Prueba de Palabras especificas desde el Inspector
    void Start()
    {
        // Si la loseta ya tiene un TextMeshPro asignado y tú escribiste algo en él desde el inspector...
        if (textMesh != null && string.IsNullOrEmpty(currentLetter) && !string.IsNullOrEmpty(textMesh.text))
        {
            // Sincronizamos la variable interna con el texto visual del componente
            currentLetter = textMesh.text;

            // Corremos el SetupTile para asegurarnos de que cargue sus sprites e información base
            SetupTile(currentTileType, currentLetter);
        }
    }
    #endregion

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        // 3. Obtenemos el componente. Por si no está en el editor, el código lo añade por seguridad.
        sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup == null) sortingGroup = gameObject.AddComponent<SortingGroup>();
    }

    void Update()
    {
        LayerOnTable();
        if (currentTableState == TableState.Stopped) return;  HandleMouseInput();
        HandleMovement();        
    }

    //Para controlor del tablero durante cinemáticas o cambio de enemigo, etc. Evita que el jugador pueda interactuar o mover las losetas.
    #region Eventos Globales de Estado del Tablero
    private void OnEnable()
    {
        // Se suscriben al evento al aparecer en el juego
        OnGlobalTableStateChanged += ChangeTableState;
    }

    private void OnDisable()
    {
        // Se desuscriben al ser destruidas o consumidas para evitar errores de memoria
        OnGlobalTableStateChanged -= ChangeTableState;
    }

    private void ChangeTableState(TableState newState)
    {
        currentTableState = newState;
    }
    #endregion

    #region Glow

    // Activa el resplandor de esta loseta adaptando el sprite según su posición en la palabra.
    // name="positionIndex">0 = Izquierda, 1 = Centro, 2 = Derecha

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

    #region Revolver Letras

    // Inicia el efecto visual de revolver letras.
    // Solo funciona si la loseta está en el tablero (no seleccionada).

    public void StartScrambleEffect()
    {
        //No revolvemos losetas que el jugador ya tiene en la zona de palabra
        //if (isSelected || isMovingToTarget) return;

        // Si ya estaba girando, la detenemos para no sobreescribir procesos
        if (scrambleCoroutine != null)
        {
            StopCoroutine(scrambleCoroutine);
        }

        // Iniciamos la animación
        scrambleCoroutine = StartCoroutine(ScrambleRoutine());
    }

    private System.Collections.IEnumerator ScrambleRoutine()
    {
        // 1. Ciclo de letras falsas
        for (int i = 0; i < scrambleCycles; i++)
        {
            // Obtenemos una letra aleatoria y la mostramos SIN alterar el tipo visual
            string fakeLetter = GameManager.instance.characterDataBase.GetRandomLetter();
            //string fakeLetter = AlphabetData.GetRandomLetter();

            // Actualizamos solo el texto y la variable interna
            currentLetter = fakeLetter;
            if (textMesh != null) UpdateTextFontSize(currentLetter);//textMesh.text = currentLetter;

            // Esperamos unos milisegundos antes del siguiente cambio
            yield return new WaitForSeconds(scrambleDelay);
        }

        // 2. Resultado Final Real
        string finalLetter = GameManager.instance.characterDataBase.GetRandomLetter();
        //string finalLetter = AlphabetData.GetRandomLetter();

        // Aquí SÍ llamamos a tu método completo por si quieres que el "revolver" 
        // también tenga una pequeña probabilidad de convertir la loseta en una Gema
        SetupTile(currentTileType, finalLetter);

        scrambleCoroutine = null;
    }
    #endregion

    #region Control de Capas al Llegar al Tablero
    public void LayerOnTable()
    {
        // Modifica la condición de llegada en el Update de TileController.cs
        if (transform.position == targetPosition)
        {
            isMovingToTarget = false; // Ya llegó, apagamos el movimiento por código

            if (isMovingBack)
            {
                isMovingBack = false;
                col.isTrigger = false;
                WordBuilderManager.Instance.CheckAndUnfreeze(); // Avisa al manager que llegó al tablero

                // NUEVO: Cambiamos el orden al fondo ÚNICAMENTE cuando ya se encuentra en su posición original
                if (sortingGroup != null)
                {
                    sortingGroup.sortingOrder = -5;
                }
            }
        }
    }
    #endregion


    #region Control Visual de Acentos
    //Recibe un 'string' en lugar de un 'char'
    public void SetVisualLetter(string correctText)
    {
        // Usamos la función que formatea todo correctamente (Qu en vez de QU)
        UpdateTextFontSize(correctText);
    }

    // Devuelve la loseta a su letra original sin tilde si la palabra se desarma
    public void ResetVisualLetter()
    {
        // Usamos la función que formatea todo correctamente
        UpdateTextFontSize(currentLetter);
    }
    #endregion

    // Reemplaza o añade este método para actualizar el texto de forma segura controlando el tamaño
    public void UpdateTextFontSize(string textValue)
    {
        if (textMesh == null || string.IsNullOrEmpty(textValue)) return;

        // Formateamos el texto: Primera letra mayúscula, el resto minúsculas.
        // Ej: "qU" o "qu" se convierten en "Qu". "a" se convierte en "A".
        string formattedText = char.ToUpper(textValue[0]) + textValue.Substring(1).ToLower();

        textMesh.text = formattedText;

        // Si el texto tiene 2 o más letras (como "Qu"), reducimos el tamaño de la fuente
        if (textValue.Length > 1)
        {
            textMesh.fontSize = doubleLetterFontSize;
        }
        else
        {
            textMesh.fontSize = normalFontSize;
        }
    }

    // Llama a esta función justo después de instanciar la loseta
    // para definir qué es (Gema, Básica, etc.) y qué letra tiene.
    public void SetupTile(TileType newType, string letter)
    {
        currentTileType = newType;
        currentLetter = letter;

        if (textMesh != null)
        {
            UpdateTextFontSize(currentLetter);
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

    private void HandleMouseInput() // Maneja la interacción del mouse con la loseta
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

    private void HandleMovement() // Maneja el movimiento de la loseta hacia su destino o de regreso a su posición original
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