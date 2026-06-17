using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "CharacterDataBase", menuName = "BookWorm/CharacterDataBase")]
public class CharacterDataBase : SerializedScriptableObject
{
    [SerializeField]public Dictionary<string, CharacterData> DataBase;

    public CharacterData GetChar(string value ,out Gems gem)
    {
        if(DataBase.TryGetValue(value, out CharacterData data))
        {
            //->Roll
            gem = Gems.Normal;

            //->random range para modificar Gem




            return data; 
        }


        gem = Gems.Normal;

        return null;

    }

    public int GetTotalDamage(string word)
    {
        int total = 0;  
        for (int i = 0; i < word.Length; i++)
        {
            total += DataBase[word[i].ToString()].BaseDamage;
        }
        return total;
    }
    /*public CharacterData GetRandomChar()
    {
        // 1. Copy dictionary keys to an indexable List
        List<string> keyList = new List<string>(DataBase.Keys);

        Random rand = new Random();

        // 2. Select a random key using the list's indexer
        string randomKey = keyList[rand.Next(keyList.Count)];

        // 3. Retrieve the value from the dictionary
        string randomValue = dict[randomKey];

        int random = Random.Range(0, DataBase.Count);
        DataBase[random]
    }*/
}
