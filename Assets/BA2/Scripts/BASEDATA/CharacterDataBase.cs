using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Sirenix.OdinInspector;
// Este scriptable object es el encargado de almacenar los datos de cada letra, como su daño base, su tipo de gema, etc.
[CreateAssetMenu(fileName = "CharacterDataBase", menuName = "BookWorm/CharacterDataBase")]
public class CharacterDataBase : SerializedScriptableObject
{
    [SerializeField]public Dictionary<string, CharacterData> DataBase;
    private float _totalWeight = 0f;

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

    // 2. Quitamos el 'static' para poder leer el DataBase de esta instancia
    public void CalculateTotalWeight()
    {
        _totalWeight = 0f;
        // Iteramos sobre el diccionario directamente
        foreach (var kvp in DataBase) //kvp es un KeyValuePair<string, CharacterData>
        {
            // Sumamos la variable Frequency de cada CharacterData
            _totalWeight += kvp.Value.Frequency;
        }
    }

    // 3. Quitamos el 'static' y usamos floats para la probabilidad
    public string GetRandomLetter()
    {
        // Medida de seguridad: Si el peso es 0 (ej. primera vez que se llama), lo calculamos
        if (_totalWeight <= 0f)
        {
            CalculateTotalWeight();
        }

        // Generamos un float aleatorio entre 0 y el peso total
        float randomValue = UnityEngine.Random.Range(0f, _totalWeight);

        foreach (var kvp in DataBase)
        {
            randomValue -= kvp.Value.Frequency;

            // Si el valor llega a 0 o menos, esta es la letra elegida
            if (randomValue <= 0f)
            {
                return kvp.Key; // Retornamos la clave del diccionario (que asumo es la letra)
            }
        }

        // Fallback de seguridad en caso de errores de redondeo de punto flotante
        return "A";
    }

    public int GetTotalDamage(string tileLetter)
    {
        // Como ahora mandas letra por letra desde la loseta (ej. "A" o "Qu"),
        // solo necesitamos buscar esa clave exacta directamente.
        if (DataBase.ContainsKey(tileLetter))
        {
            return (int)DataBase[tileLetter].BaseDamage;
        }

        return 0; // Si por algún motivo no existe, devuelve 0 daño
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
