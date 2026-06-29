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

    [Header("Referencias Visuales de Gemas")]
    [SerializeField] private SpriteRenderer gemGlowRenderer; // SpriteRenderer hijo para el Sprite B (Glow superpuesto)
    [SerializeField] private Animator gemAnimator;           // Animator hijo para la Animación C (Brillo)

    // Referencia a la data actual de esta loseta
    private GemDataSO currentGemData;
    // --- NUEVO: Propiedad pública para que el WordBuilderManager pueda leer el ScriptableObject ---
    public GemDataSO CurrentGemData => currentGemData;

    [Header("Sprites de la Loseta")]
    [HideInInspector] public Sprite normalSprite;
    [HideInInspector] public Sprite hoverSprite;
    [HideInInspector] public UnityEvent AnimationTileGem;
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

            // NUEVO: Solo corremos el SetupTile si la data no es nula
            if (currentGemData != null)
            {
                SetupTile(currentGemData, currentLetter);
            }
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
        SetupTile(currentGemData, finalLetter);

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
    // Modificamos SetupTile para recibir el nuevo ScriptableObject optimizado
    // Llama a esta función justo después de instanciar la loseta
    // para definir qué es (Gema, Básica, etc.) y qué letra tiene.
    public void SetupTile(GemDataSO gemData, string letter)
    {
        if (gemData == null)
        {
            Debug.LogWarning($"[TileController] Cuidado: Intentaste configurar la loseta '{gameObject.name}' con un GemData nulo.");
            return;
        }

        currentGemData = gemData;
        currentLetter = letter;

        if (textMesh != null) UpdateTextFontSize(currentLetter);

        // 1. Configuración base compartida
        normalSprite = currentGemData.baseSprite;
        hoverSprite = currentGemData.overlaySprite; // Asignamos el sprite desde el SO
        currentDamageMultiplier = (int)currentGemData.damageMultiplier;

        if (spriteRenderer != null) spriteRenderer.sprite = normalSprite;

        // 2. Comportamiento especial si es gema
        if (currentGemData.isSpecialGem)
        {
            if (gemGlowRenderer != null)
            {
                gemGlowRenderer.sprite = currentGemData.overlaySprite;
                gemGlowRenderer.gameObject.SetActive(false); // Apagado hasta hacer hover
            }

            if (gemAnimator != null)
            {
                gemAnimator.enabled = true; // <-- CORRECCIÓN: Encendemos solo el componente
                gemAnimator.runtimeAnimatorController = currentGemData.gemAnimatorController;
            }
        }
        else
        {
            // Lógica para losetas normales
            if (gemGlowRenderer != null) gemGlowRenderer.gameObject.SetActive(false);

            if (gemAnimator != null)
            {
                gemAnimator.runtimeAnimatorController = null; // Limpiamos por si acaso
                gemAnimator.enabled = false; // <-- CORRECCIÓN: Apagamos solo el componente, NO el GameObject entero
            }
        }
    }

    private void HandleMouseInput()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        bool wasHovered = isHovered;
        isHovered = col.OverlapPoint(mousePos);

        // 1. LÓGICA DE HOVER Y ESTADOS VISUALES
        if (isHovered && !wasHovered && !isSelected)
        {
            AnimationTileGem?.Invoke();

            if (currentGemData != null && currentGemData.isSpecialGem)
            {
                // Efecto Gema (A + B + C): Enciende el overlay superpuesto y activa animación
                if (gemGlowRenderer != null) gemGlowRenderer.gameObject.SetActive(true);
                if (gemAnimator != null) gemAnimator.SetTrigger("PlaySparkle");
            }
            else
            {
                // Efecto Normal: Cambia Sprite A por B directamente
                if (hoverSprite != null) spriteRenderer.sprite = hoverSprite;
            }
        }
        else if (!isHovered && wasHovered && !isSelected)
        {
            if (currentGemData != null && currentGemData.isSpecialGem)
            {
                // Apaga el overlay de la gema, el Sprite A base sigue visible detrás
                if (gemGlowRenderer != null) gemGlowRenderer.gameObject.SetActive(false);
            }
            else
            {
                // Regresa la loseta normal a su Sprite A
                spriteRenderer.sprite = normalSprite;
            }
        }

        // 2. LÓGICA DE SELECCIÓN (CLIC) ---> ¡Esto era lo que se había borrado!
        if (isHovered && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Dispara el evento que le avisa al WordBuilderManager que esta loseta fue tocada
            onTileInteracted?.Invoke();
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
        isMovingToTarget = true;

        if (selecting && !isSelected)
        {
            originalPosition = transform.position;
            isSelected = true;
            isMovingBack = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
            col.isTrigger = true;
            if (sortingGroup != null) sortingGroup.sortingOrder = 10;

            // --- CORRECCIÓN: Apagar el hover/glow al enviarla a la palabra ---
            if (currentGemData != null)
            {
                if (currentGemData.isSpecialGem && gemGlowRenderer != null)
                    gemGlowRenderer.gameObject.SetActive(false); // Apaga glow de gema superpuesto

                // Siempre nos aseguramos de que el sprite base regrese a la normalidad
                spriteRenderer.sprite = normalSprite;
            }
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