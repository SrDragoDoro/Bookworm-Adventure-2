using UnityEngine;
public enum Gems
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
    Diamond

}

[CreateAssetMenu(fileName = "CharacterData", menuName = "BookWorm/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string LetterID = "";
    [Range(0.0f,100f)]public float Frequency = 0;
    public float BaseDamage; //antes era int
    public Gems gem = Gems.Normal;
}
