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

    [Header("Configuración de la zona de palabra")]
    [SerializeField] private Transform wordZoneCenter;
    [SerializeField] private float spacing = 1.2f;

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