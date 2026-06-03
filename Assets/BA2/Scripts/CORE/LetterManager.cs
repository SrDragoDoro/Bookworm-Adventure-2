using System.Collections.Generic;
using UnityEngine;

// Esta estructura define cada letra y su "peso" (frecuencia de aparición)
//[System.Serializable]
//public struct LetterFrequency
//{
//    public string letter; // Ej: "A", "E", "X", "Qu"
//    public int weight;    // Ej: 30, 5, 1
//}

public class LetterManager : MonoBehaviour
{
    [Header("Configuración de Frecuencias")]
    [Tooltip("Lista de letras y su peso de probabilidad.")]
    public List<LetterFrequency> alphabetFrequencies = new List<LetterFrequency>();

    private int totalWeight = 0;

    void Start()
    {
        CalculateTotalWeight();
    }

   
    // Suma todos los pesos configurados en el Inspector. 
    // Se hace una sola vez al inicio para optimizar el rendimiento.  
    private void CalculateTotalWeight()
    {
        totalWeight = 0;
        foreach (LetterFrequency item in alphabetFrequencies)
        {
            totalWeight += item.weight;
        }

        // Validación en consola para ver si la lista está vacía
        if (totalWeight == 0)
        {
            Debug.LogWarning("El peso total de las letras es 0. Revisa la lista en el Inspector.");
        }
    }

   
    // Devuelve una letra aleatoria basada en su peso estadístico.   
    public string GetRandomLetter()
    {
        if (totalWeight == 0) CalculateTotalWeight(); // Seguro por si se llama antes de Start()

        // 1. Generamos un número aleatorio entre 0 y el peso total exclusivo.
        int randomValue = Random.Range(0, totalWeight);

        // 2. Iteramos sobre nuestras letras restando su peso al número aleatorio.
        foreach (LetterFrequency item in alphabetFrequencies)
        {
            randomValue -= item.weight;

            // 3. Cuando el número cae por debajo de cero, encontramos nuestra letra ganadora.
            if (randomValue < 0)
            {
                return item.letter;
            }
        }

        // Fallback de seguridad (teóricamente no debería ejecutarse nunca si los pesos están bien)
        return alphabetFrequencies.Count > 0 ? alphabetFrequencies[0].letter : "A";
    }
}