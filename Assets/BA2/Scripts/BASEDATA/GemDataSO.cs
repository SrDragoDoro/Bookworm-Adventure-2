using UnityEngine;

public enum GemType
{
    Normal, Amethyst, Ruby, Sapphire, Emerald, Diamond, Topaz, Garnet, Crystal, Rainbow
}

[CreateAssetMenu(fileName = "NewGemData", menuName = "BookWorm/GemData")]
public class GemDataSO : ScriptableObject
{
    [Header("Configuración Base")]
    public string gemName;
    public GemType gemType = GemType.Normal;
    public bool isSpecialGem;

    [Header("Visuales")]
    public Sprite baseSprite;       // Sprite A (Estado normal)
    public Sprite overlaySprite;    // Sprite B (Hover para Normal, Glow superpuesto para Gemas)

    [Header("Atributos de Combate")]
    public float damageMultiplier = 1.0f;
    public int healAmount = 0; // Cantidad de curación que proporciona la gema (si aplica)
    public SkillType skillType = SkillType.None;
}