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
    public Sprite baseSprite;       // Sprite A: Estado normal (Para normales y gemas)
    public Sprite overlaySprite;    // Sprite B: Hover (Reemplaza a Normal) o Glow (Superpuesto en Gemas)

    // Controlador de animación de esta gema (Animación C)
    public RuntimeAnimatorController gemAnimatorController;

    [Header("Atributos de Combate")]
    public float damageMultiplier = 1.0f;
    public int healAmount = 0;
    public SkillType skillType = SkillType.None;
}