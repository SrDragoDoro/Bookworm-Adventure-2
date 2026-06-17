using UnityEngine;
public enum Gems
{
    None,
    Normal,
    Green,
    Blue,
    Red,
    Yellow,
    Purple,
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "BookWorms/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string LetterID = "";
    [Range(0,100)]public int Frequency = 0;
    public int BaseDamage;
    public Gems gem = Gems.Normal;
}
