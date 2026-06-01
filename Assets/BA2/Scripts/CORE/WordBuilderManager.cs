using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/*
El WordBuilderManager se encarga de coordinar las
acciones entre las baldosas y el tablero con relación
al escenario, asegurándose de que las baldosas se muevan 
correctamente y que el juego funcione sin problemas. 
*/

[System.Serializable]
public struct LetterDamageData
{
    public string letter;
    public int baseDamage;
}


public class WordBuilderManager : MonoBehaviour
{
    public static WordBuilderManager Instance;
    public InputSystem_Actions inputs;

    [Header("Configuración del Botón de Ataque")]
    public UnityEngine.UI.Button attackButtonUI;  // El botón de la UI de Unity
    public ParticleSystem validWordParticles;     // Tus partículas de éxito

    [Header("Referencias de Animación")]
    public Animator lexAnimator;                  // El Animator asignado al GameObject de Lex

    [Header("Balance de Daño por Letra")]
    public List<LetterDamageData> letterDamageDatabase;
    public int defaultLetterDamage = 1;           // Daño por defecto si una letra no está en la lista

    [Header("Generación de Losetas")]
    public GameObject tilePrefab; // El prefab base de tu loseta
    public LetterManager letterManager; // Referencia al script de probabilidades
    public float spawnHeight = 8f; // Altura en Y (fuera de cámara) desde donde caerán
    public float verticalSpacing = 1.2f; // Espacio vertical al instanciar varias en la misma columna

    [Header("Configuración de la zona de palabra")]
    [SerializeField] private Transform wordZoneCenter;
    [SerializeField] private float spacing = 1.2f;

    [Header("Validación y Resplandor")]
    public DictionaryManager dictionaryManager; // Arrastra tu objeto con el DictionaryManager aquí
    public SpriteRenderer glowRenderer;         // Arrastra el objeto del resplandor aquí
    public GameObject attackButton;             // Tu botón de ataque (UI o Sprite con Collider)

    // Ajusta este valor en el inspector para que el resplandor cubra bien las letras
    public float glowWidthPerTile = 1.2f;

    [HideInInspector] public bool isCurrentWordValid = false;

    private List<TileController> selectedTiles = new List<TileController>();
    private TileController[] allTiles;

    #region Unity Methods
    void Awake()
    {
        inputs = new();
        Instance = this;
    }

    void Start()
    {
        // 1. Buscamos todas las baldosas pre-colocadas en la escena
        allTiles = FindObjectsOfType<TileController>();

        // 2. ¡Las inicializamos con letras aleatorias!
        foreach (TileController tile in allTiles)
        {
            if (tile != null)
            {
                // Obtenemos una letra de tu sistema de frecuencias weighted random
                string randomLetter = letterManager.GetRandomLetter();

                // SetupTile se encarga de guardarla en 'currentLetter' y pintarla en el TextMeshPro
                tile.SetupTile(TileType.Basic, randomLetter);
            }
        }
    }

    void Update()
    {
        
    }
#endregion

    #region InputSystem
    private void OnEnable()
    {
        inputs.UI.Click.started += OnLeftClick;
        inputs.UI.RightClick.started += OnRightClick;

    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Tile Management
    /// <summary>
    /// Se llama cuando se ejecuta un ataque exitoso.
    /// Consume las letras usadas y repuebla las columnas afectadas.
    /// </summary>
    public void ConsumeTilesAndRefill()
    {
        if (selectedTiles.Count == 0) return;

        // Diccionario para contar cuántas losetas faltan en cada coordenada X (columna)
        Dictionary<float, int> missingTilesPerColumn = new Dictionary<float, int>();

        foreach (TileController tile in selectedTiles)
        {
            // Redondeamos la coordenada X a 1 decimal para evitar errores de punto flotante en Unity
            float colX = Mathf.Round(tile.originalPosition.x * 10f) / 10f;

            if (missingTilesPerColumn.ContainsKey(colX))
            {
                missingTilesPerColumn[colX]++;
            }
            else
            {
                missingTilesPerColumn[colX] = 1;
            }

            // Destruimos el GameObject de la loseta consumida
            Destroy(tile.gameObject);
        }

        // Vaciamos la lista de seleccionadas porque ya se destruyeron
        selectedTiles.Clear();

        // Procedemos a generar las nuevas losetas
        RefillColumns(missingTilesPerColumn);
    }

    private void RefillColumns(Dictionary<float, int> missingData)
    {
        // Iteramos sobre cada columna que necesita losetas
        foreach (KeyValuePair<float, int> column in missingData)
        {
            float targetX = column.Key;       // La posición X de la columna
            int tilesToSpawn = column.Value;  // Cantidad de losetas a reponer aquí

            for (int i = 0; i < tilesToSpawn; i++)
            {
                // Calculamos la posición de spawn. 
                // Sumamos 'i * verticalSpacing' para que, si caen 3 en la misma columna, 
                // aparezcan una encima de la otra y no colisionen dentro del mismo espacio.
                Vector3 spawnPos = new Vector3(targetX, spawnHeight + (i * verticalSpacing), 0);

                // Instanciamos la nueva loseta
                GameObject newTileObj = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
                TileController newTile = newTileObj.GetComponent<TileController>();

                // Obtenemos una letra aleatoria basada en probabilidades
                string randomLetter = letterManager.GetRandomLetter();

                // Configuramos su aspecto y letra. Por ahora es Basic, luego puedes 
                // agregar lógica aleatoria para que a veces nazca como Gema.
                newTile.SetupTile(TileType.Basic, randomLetter);
            }
        }

        // Es importante volver a buscar todas las losetas en escena para actualizar el arreglo 'allTiles'
        // ya que destruimos algunas y creamos otras nuevas.
        allTiles = FindObjectsOfType<TileController>();
    }

    #endregion

    #region Word Validation and Visual Feedback
    /// <summary>
    /// Lee las losetas actuales, valida contra el diccionario y actualiza el estado visual.
    /// </summary>
    public void EvaluateCurrentWord()
    {
        // 1. Si no hay letras seleccionadas, apagamos todo
        if (selectedTiles.Count == 0)
        {
            isCurrentWordValid = false;
            glowRenderer.gameObject.SetActive(false);

            // Desactivar botón y apagar partículas
            attackButtonUI.interactable = false;
            if (validWordParticles.isPlaying) validWordParticles.Stop();
            return;
        }

        // 2. Concatenar la palabra leyendo de izquierda a derecha
        string currentWord = "";
        foreach (TileController tile in selectedTiles)
        {
            currentWord += tile.currentLetter;
        }

        // 3. Validar con el DictionaryManager
        isCurrentWordValid = dictionaryManager.CheckWordIsValid(currentWord);

        // 4. Aplicar Feedback Visual
        if (isCurrentWordValid)
        {
            // Encender el resplandor y el botón de ataque
            glowRenderer.gameObject.SetActive(true);
            attackButton.SetActive(true);

            // Control de partículas: Solo play si estaban apagadas para evitar reinicios bruscos
            if (!validWordParticles.isPlaying) validWordParticles.Play();

            // Calcular el nuevo tamaño del resplandor
            // Solo modificamos el ancho (X), manteniendo su altura original (Y)
            float newWidth = selectedTiles.Count * glowWidthPerTile;
            glowRenderer.size = new Vector2(newWidth, glowRenderer.size.y);

            // Ubicar el resplandor exactamente en el centro geométrico de la palabra formada
            Vector3 firstTilePos = selectedTiles[0].transform.position;
            Vector3 lastTilePos = selectedTiles[selectedTiles.Count - 1].transform.position;
            Vector3 centerPos = (firstTilePos + lastTilePos) / 2f;

            glowRenderer.transform.position = new Vector3(centerPos.x, centerPos.y, glowRenderer.transform.position.z);

            // Aquí también podrías instanciar un sistema de partículas de chispas
        }
        else
        {
            // La palabra no es válida o está incompleta
            glowRenderer.gameObject.SetActive(false);
            attackButton.SetActive(false);
            if (validWordParticles.isPlaying) validWordParticles.Stop();
        }
    }
    #endregion

    #region Word Building Logic
    public void SelectTile(TileController tile)
    {
        if (selectedTiles.Count == 0)
        {
            FreezeBoard(true); // Congela todo cuando se levanta la primera letra
        }

        selectedTiles.Add(tile);
        UpdateWordPositions();
        EvaluateCurrentWord();
    }

    public void DeselectTile(TileController tile)
    {
        if (selectedTiles.Contains(tile))
        {
            selectedTiles.Remove(tile);
            tile.MoveTo(tile.originalPosition, false);
            UpdateWordPositions(); // Reorganiza las letras restantes en el escenario
            EvaluateCurrentWord();
        }
    }

    public void ClearAllTiles()
    {
        // Iteramos hacia atrás al remover elementos de una lista
        for (int i = selectedTiles.Count - 1; i >= 0; i--)
        {
            TileController tile = selectedTiles[i];
            tile.onTileInteracted?.Invoke(); // Reproducir sonido también al limpiar todo
            tile.MoveTo(tile.originalPosition, false);
        }
        selectedTiles.Clear();
        EvaluateCurrentWord();
    }

    private void UpdateWordPositions()
    {
        int count = selectedTiles.Count;
        if (count == 0) return;

        float startX = wordZoneCenter.position.x - (spacing * (count - 1) / 2f);

        for (int i = 0; i < count; i++)
        {
            Vector3 newPos = new Vector3(startX + (i * spacing), wordZoneCenter.position.y, wordZoneCenter.position.z);
            selectedTiles[i].MoveTo(newPos, true);
        }
    }

    private void FreezeBoard(bool freeze)
    {
        foreach (TileController t in allTiles)
        {
            if (freeze) t.SetStatic();
        }
    }

    #endregion

    #region Tile Interaction
    // Este método es llamado por las fichas individuales cuando llegan a su destino
    public void CheckAndUnfreeze()
    {
        // Si ya no hay letras seleccionadas en el escenario...
        if (selectedTiles.Count == 0)
        {
            bool anyMoving = false;

            // Verificamos si todavía hay alguna letra en pleno viaje de regreso
            foreach (TileController t in allTiles)
            {
                if (t.isMovingBack) anyMoving = true;
            }

            // Si todas llegaron, reactivamos la gravedad
            if (!anyMoving)
            {
                foreach (TileController t in allTiles)
                {
                    t.SetDynamic();
                }
            }
        }
    }

    public List<TileController> GetSelectedTiles()
    {
        return selectedTiles;
    }

    #endregion

    #region Attack Execution
    /// <summary>
    /// Se ejecuta al hacer clic en el botón de ataque. 
    /// Calcula el daño, dispara la animación de Lex y limpia el tablero.
    /// </summary>
    public void ExecuteAttack()
    {
        // Validación de seguridad por si acaso
        if (!isCurrentWordValid || selectedTiles.Count == 0) return;

        int totalDamage = 0;

        // 1. CALCULAR DAÑO TOTAL EVALUANDO LETRAS Y GEMAS
        foreach (TileController tile in selectedTiles)
        {
            int baseLetterDamage = defaultLetterDamage;

            // Buscamos si la letra tiene un valor asignado en nuestra base de datos de daño
            foreach (LetterDamageData damageData in letterDamageDatabase)
            {
                if (damageData.letter.Equals(tile.currentLetter, StringComparison.OrdinalIgnoreCase))
                {
                    baseLetterDamage = damageData.baseDamage;
                    break;
                }
            }

            // Daño de la loseta = (Daño base de la letra) * (Multiplicador de gema/estado de la loseta)
            totalDamage += baseLetterDamage * tile.currentDamageMultiplier;
        }

        Debug.Log($"¡Ataque ejecutado! Palabra válida. Daño total calculado: {totalDamage}");

        // 2. ANIMACIÓN DINÁMICA
        // Pasamos el poder total al Animator. En tu Animator de Unity, puedes crear un parámetro 
        // de tipo Int llamado "AttackPower" y usar transiciones basadas en condiciones.
        if (lexAnimator != null)
        {
            lexAnimator.SetInteger("AttackPower", totalDamage);
            lexAnimator.SetTrigger("Attack");
            // Nota: En tu máquina de estados del Animator, si Attack Trigger se activa y 
            // AttackPower es < 10 va a una animación simple ("perro"), si es >= 10 va al ataque especial ("intracelular").
        }

        // 3. LIMPIEZA Y GENERACIÓN SIMULTÁNEA
        // Apagamos las partículas del modo preparación inmediatamente
        if (validWordParticles.isPlaying) validWordParticles.Stop();
        glowRenderer.gameObject.SetActive(false);

        // Llamamos al método de consumo y relleno que programamos en el paso anterior.
        // Al ejecutarse en este mismo frame, las losetas usadas desaparecen y las nuevas
        // empiezan a caer desde el cielo al mismo tiempo que Lex inicia su animación de ataque.
        ConsumeTilesAndRefill();
    }
    #endregion


}