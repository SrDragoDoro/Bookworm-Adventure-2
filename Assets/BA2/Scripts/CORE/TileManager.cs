using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Linq;

public class TileManager : MonoBehaviour
{
    public InputSystem_Actions inputs;

    [Header("Configuración Global")]
    [SerializeField] private Transform wordZoneCenter;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private float moveSpeed = 15f;

    [Header("Aspecto de las Fichas")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;

    [Header("Eventos")]
    public UnityEvent onTileInteracted;

    private class TileData
    {
        public Transform transform;
        public SpriteRenderer spriteRenderer;
        public Rigidbody2D rb;
        public BoxCollider2D col;

        public Vector3 originalPosition;
        public Vector3 targetPosition;
        public bool isSelected;
        public bool isMovingBack;
        public bool isMovingToTarget;
    }

    private List<TileData> allTiles = new List<TileData>();
    private List<TileData> selectedTiles = new List<TileData>();
    private TileData currentlyHoveredTile = null;

    private void Awake()
    {
        inputs = new InputSystem_Actions();
        InitializeTiles();
    }

    private void InitializeTiles()
    {
        allTiles.Clear();

        Rigidbody2D[] rigidbodies = FindObjectsOfType<Rigidbody2D>();

        foreach (Rigidbody2D rb2d in rigidbodies)
        {
            if (rb2d.gameObject == this.gameObject) continue;

            BoxCollider2D collider = rb2d.GetComponent<BoxCollider2D>();
            SpriteRenderer sr = rb2d.GetComponent<SpriteRenderer>();

            if (collider != null && sr != null)
            {
                TileData newTile = new TileData
                {
                    transform = rb2d.transform,
                    spriteRenderer = sr,
                    rb = rb2d,
                    col = collider,
                    originalPosition = rb2d.transform.position,
                    targetPosition = rb2d.transform.position,
                    isSelected = false,
                    isMovingBack = false,
                    isMovingToTarget = false
                };
                allTiles.Add(newTile);
            }
        }

        Debug.Log($"[BookwormManager] Se han detectado y registrado con éxito {allTiles.Count} fichas en el tablero.");
    }

    #region InputSystem
    private void OnEnable()
    {
        inputs.Enable();
        inputs.UI.Click.started += OnLeftClick;
        inputs.UI.RightClick.started += OnRightClick;
    }

    private void OnDisable()
    {
        inputs.Disable();
        inputs.UI.Click.started -= OnLeftClick;
        inputs.UI.RightClick.started -= OnRightClick;
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (Mouse.current == null) return;

        // 1. Obtenemos la posición exacta del cursor en el mundo
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        // 2. MÉTODO BLINDADO: Recorremos nuestras fichas para ver cuál coincide exactamente
        TileData clickedTile = null;
        foreach (TileData tile in allTiles)
        {
            if (tile.col != null && tile.col.OverlapPoint(worldPos))
            {
                clickedTile = tile;
                break;
            }
        }

        // 3. Si encontramos la ficha, ejecutamos la selección
        if (clickedTile != null)
        {
            onTileInteracted?.Invoke();

            if (!clickedTile.isSelected)
                SelectTile(clickedTile);
            else
                DeselectTile(clickedTile);
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (selectedTiles.Count > 0)
        {
            ClearAllTiles();
        }
    }
    #endregion

    private void Update()
    {
        HandleHover();
        HandleMovement();
    }

    private void HandleHover()
    {
        if (Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        // Verificamos de forma directa si el cursor colisiona con alguna de nuestras fichas
        TileData hitTile = null;
        foreach (TileData tile in allTiles)
        {
            if (tile.col != null && tile.col.OverlapPoint(worldPos))
            {
                hitTile = tile;
                break;
            }
        }

        // Gestión visual de sprites normales y hover
        if (currentlyHoveredTile != hitTile)
        {
            if (currentlyHoveredTile != null && !currentlyHoveredTile.isSelected)
                currentlyHoveredTile.spriteRenderer.sprite = normalSprite;

            currentlyHoveredTile = hitTile;

            if (currentlyHoveredTile != null && !currentlyHoveredTile.isSelected)
                currentlyHoveredTile.spriteRenderer.sprite = hoverSprite;
        }
    }

    private void HandleMovement()
    {
        foreach (TileData tile in allTiles)
        {
            if (tile.isMovingToTarget)
            {
                tile.transform.position = Vector3.MoveTowards(tile.transform.position, tile.targetPosition, moveSpeed * Time.deltaTime);

                if (tile.transform.position == tile.targetPosition)
                {
                    tile.isMovingToTarget = false;

                    if (tile.isMovingBack)
                    {
                        tile.isMovingBack = false;
                        tile.col.isTrigger = false;
                        CheckAndUnfreeze();
                    }
                }
            }
        }
    }

    private void SelectTile(TileData tile)
    {
        if (selectedTiles.Count == 0) FreezeBoard(true);

        tile.originalPosition = tile.transform.position; // Guardamos su posición de reposo actual en el tablero
        tile.isSelected = true;
        tile.isMovingBack = false;
        tile.rb.bodyType = RigidbodyType2D.Kinematic;
        tile.col.isTrigger = true;

        selectedTiles.Add(tile);
        UpdateWordPositions();
    }

    private void DeselectTile(TileData tile)
    {
        if (selectedTiles.Contains(tile))
        {
            selectedTiles.Remove(tile);

            tile.isSelected = false;
            tile.isMovingBack = true;
            tile.spriteRenderer.sprite = normalSprite;
            tile.rb.bodyType = RigidbodyType2D.Kinematic;

            SetTileTarget(tile, tile.originalPosition);
            UpdateWordPositions();
        }
    }

    private void ClearAllTiles()
    {
        for (int i = selectedTiles.Count - 1; i >= 0; i--)
        {
            TileData tile = selectedTiles[i];
            onTileInteracted?.Invoke();

            tile.isSelected = false;
            tile.isMovingBack = true;
            tile.spriteRenderer.sprite = normalSprite;
            tile.rb.bodyType = RigidbodyType2D.Kinematic;

            SetTileTarget(tile, tile.originalPosition);
        }
        selectedTiles.Clear();
    }

    private void UpdateWordPositions()
    {
        if (wordZoneCenter == null)
        {
            Debug.LogError("[BookwormManager] ¡ATENCIÓN! Olvidaste asignar el objeto 'Word Zone Center' en el Inspector.");
            return;
        }

        int count = selectedTiles.Count;
        if (count == 0) return;

        float startX = wordZoneCenter.position.x - (spacing * (count - 1) / 2f);

        for (int i = 0; i < count; i++)
        {
            Vector3 newPos = new Vector3(startX + (i * spacing), wordZoneCenter.position.y, wordZoneCenter.position.z);
            SetTileTarget(selectedTiles[i], newPos);
        }
    }

    private void SetTileTarget(TileData tile, Vector3 target)
    {
        tile.targetPosition = target;
        tile.isMovingToTarget = true;
    }

    private void FreezeBoard(bool freeze)
    {
        foreach (TileData t in allTiles)
        {
            if (!t.isSelected)
            {
                t.rb.bodyType = freeze ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
                if (!freeze) t.rb.WakeUp();
            }
        }
    }

    private void CheckAndUnfreeze()
    {
        if (selectedTiles.Count == 0)
        {
            bool anyMoving = false;
            foreach (TileData t in allTiles)
            {
                if (t.isMovingBack) anyMoving = true;
            }

            if (!anyMoving)
            {
                FreezeBoard(false);
            }
        }
    }
}