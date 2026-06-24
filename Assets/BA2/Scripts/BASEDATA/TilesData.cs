using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#region DEFINICIONES DE DATOS GLOBALES
public enum TileType
{
    None,
    Basic,
    Amethyst,
    Diamond,
    Cursed,
    Plagued,
    Fire
}

[System.Serializable]
public struct TileVisualData
{
    public TileType type;
    public Sprite normalSprite;
    public Sprite hoverSprite;
    public UnityEvent AnimationTileGem;
    public int damageMultiplier; // Cálculo del daño de las gemas y tesoros
}
#endregion




public abstract class TilesData : MonoBehaviour
{
    // Para métodos comparidos
}