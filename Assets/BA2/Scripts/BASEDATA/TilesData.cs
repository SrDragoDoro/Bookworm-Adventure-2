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
    public GameObject hoverAnimationObject; 
    public int damageMultiplier; // Cálculo del daño de las gemas y tesoros
}
#endregion




public abstract class TilesData : MonoBehaviour
{
    public virtual void Initialize(TileVisualData data)
    {
        // Implementación en clases derivadas para aplicar la inicialización visual/datos del tile.
    }

    public virtual void OnHover(bool isEntering, int hoverIndex, GameObject hoverObject) 
    {
        // Sobrecarga del método para manejar el comportamiento de entrada/salida del hover.
        // Implementación en clases derivadas para manejar el comportamiento de entrada/salida del hover.
    }

    public virtual int CalculateDamage(TileVisualData data)
    {
        // Implementación en clases derivadas para calcular el daño del tile.
        return Mathf.Max(1, data.damageMultiplier);
    }
}