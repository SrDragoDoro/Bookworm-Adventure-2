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
    [Range(0.0f,100f)]public float Frequency = 0;
    public float BaseDamage; //antes era int
    public Gems gem = Gems.Normal;
}
