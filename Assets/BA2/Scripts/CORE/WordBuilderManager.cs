using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Configuración del Botón de Ataque")]
    public UnityEngine.UI.Button attackButtonUI;  // El botón de la UI de Unity
    public ParticleSystem validWordParticles;     // Tus partículas de éxito
    public AudioSource validWordSound;            // <-- CAMBIO: Ahora es un AudioSource

    [Header("Referencias de Animación")]
    public Animator lexAnimator;                  // El Animator asignado al GameObject de Lex

    [Header("Generación de Losetas")]
    public GameObject tilePrefab;        // El prefab base de tu loseta
    public LetterManager letterManager;  // Referencia al script de probabilidades
    public float spawnHeight = 8f;       // Altura en Y (fuera de cámara) desde donde caerán
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
                string randomLetter = AlphabetData.GetRandomLetter();
                tile.SetupTile(TileType.Basic, randomLetter);
            }
        }
    }

    void Update()
    {

    }
    #endregion

    #region Tile Management  
    public void ConsumeTilesAndRefill()
    {
        if (selectedTiles.Count == 0) return;

        Dictionary<float, int> missingTilesPerColumn = new Dictionary<float, int>();

        foreach (TileController tile in selectedTiles)
        {
            float colX = Mathf.Round(tile.originalPosition.x * 10f) / 10f;

            if (missingTilesPerColumn.ContainsKey(colX))
            {
                missingTilesPerColumn[colX]++;
            }
            else
            {
                missingTilesPerColumn[colX] = 1;
            }

            Destroy(tile.gameObject);
        }

        selectedTiles.Clear();
        RefillColumns(missingTilesPerColumn);
    }

    private void RefillColumns(Dictionary<float, int> missingData)
    {
        foreach (KeyValuePair<float, int> column in missingData)
        {
            float targetX = column.Key;
            int tilesToSpawn = column.Value;

            for (int i = 0; i < tilesToSpawn; i++)
            {
                Vector3 spawnPos = new Vector3(targetX, spawnHeight + (i * verticalSpacing), 0);

                GameObject newTileObj = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
                TileController newTile = newTileObj.GetComponent<TileController>();

                string randomLetter = AlphabetData.GetRandomLetter();
                newTile.SetupTile(TileType.Basic, randomLetter);
            }
        }

        allTiles = FindObjectsOfType<TileController>();
        FreezeBoard(false);
    }
    #endregion

    #region Word Validation and Visual Feedback  
    public void EvaluateCurrentWord()
    {
        if (selectedTiles.Count == 0)
        {
            isCurrentWordValid = false;
            attackButtonUI.interactable = false;
            if (validWordParticles.isPlaying) validWordParticles.Stop();

            // Opcional: Detener sonido si el tablero queda vacío
            if (validWordSound != null && validWordSound.isPlaying) validWordSound.Stop();

            return;
        }

        foreach (TileController tile in selectedTiles)
        {
            tile.DisableTileGlow();
        }

        string currentWord = "";
        foreach (TileController tile in selectedTiles)
        {
            currentWord += tile.currentLetter;
        }

        isCurrentWordValid = dictionaryManager.CheckWordIsValid(currentWord);

        if (isCurrentWordValid)
        {
            attackButtonUI.interactable = true;
            if (!validWordParticles.isPlaying) validWordParticles.Play();

            // <-- CAMBIO: Lógica segura para reproducir el AudioSource
            if (validWordSound != null && !validWordSound.isPlaying)
            {
                validWordSound.Play();
            }

            int totalTiles = selectedTiles.Count;

            for (int i = 0; i < totalTiles; i++)
            {
                if (totalTiles == 1)
                {
                    selectedTiles[i].EnableTileGlow(1);
                }
                else if (i == 0)
                {
                    selectedTiles[i].EnableTileGlow(0);
                }
                else if (i == totalTiles - 1)
                {
                    selectedTiles[i].EnableTileGlow(2);
                }
                else
                {
                    selectedTiles[i].EnableTileGlow(1);
                }
            }
        }
        else
        {
            attackButtonUI.interactable = false;
            if (validWordParticles.isPlaying) validWordParticles.Stop();

            // <-- CAMBIO: Detiene el sonido si el jugador añade una letra y la palabra deja de ser válida
            if (validWordSound != null && validWordSound.isPlaying)
            {
                validWordSound.Stop();
            }
        }
    }
    #endregion

    #region Word Building Logic
    public void SelectTile(TileController tile)
    {
        if (selectedTiles.Count == 0)
        {
            FreezeBoard(true);
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
            UpdateWordPositions();
            EvaluateCurrentWord();
        }
    }

    public void ClearAllTiles()
    {
        for (int i = selectedTiles.Count - 1; i >= 0; i--)
        {
            TileController tile = selectedTiles[i];
            tile.onTileInteracted?.Invoke();
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
            if (t != null)
            {
                if (freeze)
                    t.SetStatic();
                else
                    t.SetDynamic();
            }
        }
    }
    #endregion

    #region Tile Interaction
    public void CheckAndUnfreeze()
    {
        if (selectedTiles.Count == 0)
        {
            bool anyMoving = false;

            foreach (TileController t in allTiles)
            {
                if (t.isMovingBack) anyMoving = true;
            }

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

    public void ScrambleAllTiles()
    {
        foreach (TileController tile in allTiles)
        {
            if (tile != null)
            {
                tile.StartScrambleEffect();
            }
        }
    }
    #endregion

    #region Attack Execution   
    public void ExecuteAttack()
    {
        int totalDamage = 0;

        foreach (TileController tile in selectedTiles)
        {

            int baseLetterDamage = AlphabetData.GetBaseDamage(tile.currentLetter);

            if (tile is IDamageDealer damageDealer)
            {
                totalDamage += damageDealer.GetFinalDamage(baseLetterDamage);
            }
            else
            {
                totalDamage += baseLetterDamage * tile.currentDamageMultiplier;
            }
        }

        Debug.Log($"¡Ataque ejecutado! Palabra válida. Daño total calculado: {totalDamage}");

        // ================= CAMBIOS PARA EL TURNO RPG =================

        // 1. Cambiamos el estado a ocupado para bloquear interacciones de botones
        BattleManager.Instance.ChangeState(BattleState.Busy);

        // 2. Pasamos el daño calculado a Lex e iniciamos su secuencia de ataque
        CharacterCombat playerCombat = BattleManager.Instance.playerCombat;
        playerCombat.pendingDamage = totalDamage;
        playerCombat.PerformAttack();

        // =============================================================

        if (validWordParticles.isPlaying) validWordParticles.Stop();
        if (glowRenderer != null) glowRenderer.gameObject.SetActive(false);

        ConsumeTilesAndRefill();
    }
    #endregion
}