using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#region DEFINICIONES DE DATOS GLOBALES
public enum TileType
{
    None,
    Normal,
    Amethyst,
    Ruby,
    Saphire,
    Topaz,
    Garnet,
    Crystal,
    Emerald,
    Diamond,
    Rainbow
}

[System.Serializable]
public struct TileVisualData
{
    public TileType type;
    public Sprite normalSprite;
    public GameObject hoverSprite;
    //public UnityEvent AnimationTileGem;
    public GameObject hoverAnimationObject; // <--- Reemplaza al UnityEvent
    public int damageMultiplier; // Cálculo del daño de las gemas y tesoros
}
#endregion




public abstract class TilesData : MonoBehaviour
{
    // Para métodos comparidos
}