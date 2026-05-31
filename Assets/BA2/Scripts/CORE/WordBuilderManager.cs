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




public class WordBuilderManager : MonoBehaviour
{
    public static WordBuilderManager Instance;
    public InputSystem_Actions inputs;

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

    void Awake()
    {
        inputs = new();
        Instance = this;
    }

    void Start()
    {
        // Busca todas las baldosas al inicio para no gastar recursos haciéndolo cada vez
        allTiles = FindObjectsOfType<TileController>();
    }

    void Update()
    {
        
    }
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


    public void SelectTile(TileController tile)
    {
        if (selectedTiles.Count == 0)
        {
            FreezeBoard(true); // Congela todo cuando se levanta la primera letra
        }

        selectedTiles.Add(tile);
        UpdateWordPositions();
    }

    public void DeselectTile(TileController tile)
    {
        if (selectedTiles.Contains(tile))
        {
            selectedTiles.Remove(tile);
            tile.MoveTo(tile.originalPosition, false);
            UpdateWordPositions(); // Reorganiza las letras restantes en el escenario
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
}