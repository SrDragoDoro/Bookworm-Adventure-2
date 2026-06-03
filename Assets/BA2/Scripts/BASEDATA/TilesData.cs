using System.Collections.Generic;
using UnityEngine;

#region DEFINICIONES DE DATOS GLOBALES
public enum TileType
{
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
    public int damageMultiplier; //  Calculo del daño de las gemas y tesoros
}

[System.Serializable]
public struct LetterDamageData
{
    public string letter;
    public int baseDamage;
}
#endregion

public abstract class TilesData : MonoBehaviour
{
    // Esta clase queda como base abstracta. 
    // Si en el futuro necesitas que todas las losetas compartan variables o métodos cores,
    // los programaremos aquí dentro.
}