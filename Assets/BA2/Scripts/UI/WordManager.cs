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
        Instance = this;
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
        // Solo permitimos deseleccionar la última ficha (como en el juego original)
        // o puedes modificarlo para deseleccionar cualquiera y reordenar
        if (selectedTiles[selectedTiles.Count - 1] == tile)
        {
            selectedTiles.Remove(tile);
            tile.ReturnToBoard();
            UpdateWordPositions();

            // Si ya no hay fichas seleccionadas, descongelamos el tablero
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