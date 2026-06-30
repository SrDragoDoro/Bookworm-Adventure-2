using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Events;


/* quietar metodos de ataque y daño de las baldodosas, este script solo debe encargarse de la 
 * logica de formacion de palabras, validacion y comunicacion con el tablero, el daño y ataque 
 * lo maneja cada loseta individualmente, asi el WordBuilderManager no se sobrecarga con 
 * responsabilidades que no le corresponden.
El WordBuilderManager se encarga de coordinar las
acciones entre las baldosas y el tablero con relación
al escenario, asegurándose de que las baldosas se muevan 
correctamente y que la formacion de palabras funcione. 
*/

public class WordBuilderManager : MonoBehaviour
{
    public static WordBuilderManager Instance;
    public InputSystem_Actions inputs;

    [Header("Comentarios de Lex")]
    public LexCommentaryManager lexCommentaryManager; // <-- AÑADIR AQUÍ

    [Header("Configuración del Botón de Ataque")]
    public UnityEngine.UI.Button attackButtonUI;  // El botón de la UI de Unity
    public ParticleSystem validWordParticles;     // Tus partículas de éxito
    public AudioSource validWordSound;            // <-- CAMBIO: Ahora es un AudioSource

    [Header("Referencias de Animación")]
    public Animator lexAnimator;                  // El Animator asignado al GameObject de Lex

    [Header("Generación de Losetas")]
    public GameObject tilePrefab;        // El prefab base de tu loseta
    //public LetterManager letterManager;  // Referencia al script de probabilidades    
    public float spawnHeight = 8f;       // Altura en Y (fuera de cámara) desde donde caerán
    public float verticalSpacing = 1.2f; // Espacio vertical al instanciar varias en la misma columna
    // Añade esta variable al inicio de tu script WordBuilderManager
    private string lastSpokenWord = "";

    // ================= NUEVO: SISTEMA DE PROBABILIDADES =================
    [System.Serializable]
    public struct TileSpawnWeight
    {
        public TileType tileType;
        [Tooltip("Cuanto mayor sea el número, más probable es que aparezca.")]
        public float weight;
    }

    [Header("Probabilidades de Tipos de Loseta")]
    [Tooltip("Añade aquí los tipos de losetas y su peso para aparecer.")]
    public List<TileSpawnWeight> spawnWeights = new List<TileSpawnWeight>();
    // =====================================================================

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

        // 2. ¡Las inicializamos respetando su tipo actual!
        foreach (TileController tile in allTiles)
        {
            if (tile != null)
            {
                // Obtenemos la letra aleatoria normalmente
                string randomLetter = GameManager.instance.characterDataBase.GetRandomLetter();

                // CLAVE: En lugar de usar GetRandomTileType(), le pasamos su propio "currentTileType".
                // De esta forma, si en el inspector pusiste que era Ruby o Amethyst, mantendrá ese tipo.
                tile.SetupTile(tile.currentTileType, randomLetter);
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

                string randomLetter = GameManager.instance.characterDataBase.GetRandomLetter();

                // === AQUÍ ESTÁ EL CAMBIO IMPORTANTE ===
                // Calculamos el tipo aleatorio basado en los pesos
                TileType randomType = GetRandomTileType();

                // Le asignamos ese tipo aleatorio a la nueva loseta
                newTile.SetupTile(randomType, randomLetter);
                // ======================================
            }
        }

        allTiles = FindObjectsOfType<TileController>();
        FreezeBoard(false);
    }
    #endregion

    // === AQUÍ PONES EL NUEVO MÉTODO, FUERA DE REFILL COLUMNS ===
    private TileType GetRandomTileType()
    {
        // Si no hay pesos configurados, devolvemos Normal por defecto para evitar errores
        if (spawnWeights == null || spawnWeights.Count == 0)
            return TileType.Normal;

        float totalWeight = 0;

        // Sumamos todos los pesos
        foreach (var spawn in spawnWeights)
        {
            totalWeight += spawn.weight;
        }

        // Elegimos un número al azar entre 0 y el peso total
        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float cumulativeWeight = 0;

        // Recorremos la lista para ver en qué rango cayó el número
        foreach (var spawn in spawnWeights)
        {
            cumulativeWeight += spawn.weight;
            if (randomValue <= cumulativeWeight)
            {
                return spawn.tileType; // Encontramos la loseta ganadora
            }
        }

        return TileType.Normal; // Respaldo por si algo falla
    }


    #region Word Validation and Visual Feedback  
    public void EvaluateCurrentWord()
    {
        if (selectedTiles.Count == 0 || selectedTiles.Count <= 2)
        {
            isCurrentWordValid = false;
            attackButtonUI.interactable = false;
            if (validWordParticles.isPlaying) validWordParticles.Stop();
            if (validWordSound != null && validWordSound.isPlaying) validWordSound.Stop();

            // Reseteamos visualmente por si deshicieron una palabra válida
            foreach (TileController tile in selectedTiles)
            {
                tile.DisableTileGlow();
                tile.ResetVisualLetter(); // <-- Vuelve la Á a ser A
            }
            return;
        }

        string currentWord = "";
        foreach (TileController tile in selectedTiles)
        {
            tile.DisableTileGlow();
            currentWord += tile.currentLetter;
        }

        // CAMBIO: Ahora pedimos la palabra corregida (con tildes)
        isCurrentWordValid = dictionaryManager.CheckWordIsValid(currentWord, out string correctedWord);

        if (isCurrentWordValid)
        {
            attackButtonUI.interactable = true;
            if (!validWordParticles.isPlaying) validWordParticles.Play();
            if (validWordSound != null && !validWordSound.isPlaying) validWordSound.Play();

            // ================= NUEVO: COMENTARIO DE LEX =================
            // Verificamos que Lex no haya reaccionado ya a esta misma palabra exacta
            if (lexCommentaryManager != null && currentWord != lastSpokenWord)
            {
                lexCommentaryManager.TriggerCommentary(currentWord.Length);
                lastSpokenWord = currentWord; // Guardamos la palabra para que no repita
            }
            // ============================================================

            int totalTiles = selectedTiles.Count;
            int charIndex = 0; // NUEVO: Llevamos el control de qué parte de la palabra estamos leyendo

            for (int i = 0; i < totalTiles; i++)
            {
                // Vemos cuántos caracteres tiene la loseta original (Ej: "A" = 1, "Qu" = 2)
                int letterLength = selectedTiles[i].currentLetter.Length;

                // Extraemos ese mismo número de caracteres de la palabra corregida
                string tileCorrectText = correctedWord.Substring(charIndex, letterLength);

                // CAMBIO: Le pasamos el texto exacto a la loseta
                selectedTiles[i].SetVisualLetter(tileCorrectText);

                // Avanzamos el índice para la siguiente loseta
                charIndex += letterLength;

                if (totalTiles == 1) selectedTiles[i].EnableTileGlow(1);
                else if (i == 0) selectedTiles[i].EnableTileGlow(0);
                else if (i == totalTiles - 1) selectedTiles[i].EnableTileGlow(2);
                else selectedTiles[i].EnableTileGlow(1);
            }
        }
        else
        {
            attackButtonUI.interactable = false;
            if (validWordParticles.isPlaying) validWordParticles.Stop();
            if (validWordSound != null && validWordSound.isPlaying) validWordSound.Stop();

            // ================= NUEVO: RESETEO DE COMENTARIO =================
            // Si la palabra ya no es válida (ej. borraron una letra), olvidamos la última palabra
            lastSpokenWord = "";
            // ================================================================

            // Si se volvió inválida, regresamos las letras a su estado base Y apagamos el glow
            foreach (TileController tile in selectedTiles)
            {
                tile.ResetVisualLetter();
                tile.DisableTileGlow(); // <-- Añade esta línea
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
        if (GameManager.instance.battleManager.currentState != BattleState.PlayerTurn)
            return;

        if (selectedTiles.Contains(tile))
        {
            selectedTiles.Remove(tile);

            // Apagamos el glow y reseteamos la letra al soltar la loseta
            tile.DisableTileGlow();
            tile.ResetVisualLetter();

            tile.MoveTo(tile.originalPosition, false);
            tile.SetDynamic();

            // ==========================================
            // NUEVO: ¡Llamamos a esta función para que las 
            // letras restantes cierren el hueco y se centren!
            UpdateWordPositions();
            // ==========================================

            EvaluateCurrentWord();
        }
    }

    public void ClearAllTiles()
    {
        for (int i = selectedTiles.Count - 1; i >= 0; i--)
        {
            TileController tile = selectedTiles[i];
            tile.onTileInteracted?.Invoke();

            // NUEVO: Apagamos el glow y reseteamos la letra individualmente
            // antes de que la loseta sea eliminada de la lista
            tile.DisableTileGlow();
            tile.ResetVisualLetter();

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
        string finalWord = ""; // <-- AÑADIR AQUÍ para recolectar la palabra completa

        foreach (TileController tile in selectedTiles)
        {
            finalWord += tile.currentLetter; // <-- AÑADIR AQUÍ para saber la longitud real

            int baseLetterDamage = GameManager.instance.characterDataBase.GetTotalDamage(tile.currentLetter);

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
        
        // ===============================================================

        // ================= CAMBIOS PARA EL TURNO RPG =================

        // 1. Cambiamos el estado a ocupado para bloquear interacciones de botones
        GameManager.instance.battleManager.ChangeState(BattleState.Busy);

        // 2. Pasamos el daño calculado a Lex e iniciamos su secuencia de ataque
        CharacterCombat playerCombat = GameManager.instance.battleManager.playerCombat;
        playerCombat.pendingDamage = totalDamage;
        playerCombat.PerformAttack();

        // =============================================================

        if (validWordParticles.isPlaying) validWordParticles.Stop();
        if (glowRenderer != null) glowRenderer.gameObject.SetActive(false);

        ConsumeTilesAndRefill();
    }
    #endregion  
}