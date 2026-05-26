using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    public static WordManager Instance;

    [Header("Configuración de la zona de palabra")]
    public Transform wordZoneCenter; // Crea un objeto vacío en el centro/arriba de tu pantalla y asígnalo aquí
    public float spacing = 1.2f; // Espacio entre letras seleccionadas

    private List<TileEffect> selectedTiles = new List<TileEffect>();

    void Awake()
    {
        Instance = this; // Singleton para acceso fácil desde las fichas
    }

    public void SelectTile(TileEffect tile)
    {
        // Si es la primera ficha que seleccionamos, congelamos el tablero entero
        if (selectedTiles.Count == 0)
        {
            FreezeBoard(true);
        }

        selectedTiles.Add(tile);
        UpdateWordPositions();
    }

    public void DeselectTile(TileEffect tile)
    {
        // 1. Verificamos si la ficha realmente está en la lista de seleccionadas
        if (selectedTiles.Contains(tile))
        {
            // 2. La removemos de la lista de la palabra en construcción
            selectedTiles.Remove(tile);

            // 3. Devolvemos la ficha a su posición original en el tablero (vuelve a tener gravedad)
            tile.ReturnToBoard();

            // 4. REORDENAR: Volvemos a calcular las posiciones de las fichas que se quedaron en el centro
            UpdateWordPositions();

            // 5. Si ya no queda ninguna ficha seleccionada, descongelamos las físicas del tablero
            if (selectedTiles.Count == 0)
            {
                FreezeBoard(false);
            }
        }
    }

    // Calcula la posición en el centro para formar la palabra en orden
    private void UpdateWordPositions()
    {
        int count = selectedTiles.Count;
        if (count == 0) return;

        // Calcula el desplazamiento inicial para que la palabra quede centrada
        float startX = wordZoneCenter.position.x - (spacing * (count - 1) / 2f);

        for (int i = 0; i < count; i++)
        {
            Vector3 newPos = new Vector3(startX + (i * spacing), wordZoneCenter.position.y, wordZoneCenter.position.z);
            selectedTiles[i].MoveToWordArea(newPos);
        }
    }

    // El "truco" para evitar que las fichas caigan por los huecos vacíos
    private void FreezeBoard(bool freeze)
    {
        // Encuentra todas las fichas en la escena
        TileEffect[] allTiles = FindObjectsOfType<TileEffect>();

        foreach (TileEffect t in allTiles)
        {
            // Solo afectamos a las que NO están seleccionadas
            if (!t.isSelected)
            {
                Rigidbody2D rb = t.GetComponent<Rigidbody2D>();
                if (freeze)
                {
                    rb.bodyType = RigidbodyType2D.Static; // Congela todo movimiento
                }
                else
                {
                    rb.bodyType = RigidbodyType2D.Dynamic; // Vuelve a reaccionar a la gravedad
                    rb.WakeUp();
                }
            }
        }
    }
}