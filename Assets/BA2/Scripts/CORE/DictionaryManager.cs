using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Diagnostics;

/*
Objetivo: Insertar una lista de millones de palabras.
El archivo de las palabras está en un txt. Debemos leer el txt y cargarlo en un HashSet para luego usarlo en el juego.
Debemos hacer que la palabra que formemos la verifique con la lista y mencione si es válida o no.
*/
public class DictionaryManager : MonoBehaviour
{
    [SerializeField] private string wordToCheck = "palabra";              // Ejemplo de palabra a verificar
    [SerializeField] private string wordToCheck2 = "palabrainventada123"; // Ejemplo de palabra inventada

    // Usamos HashSet en lugar de Dictionary. Es mucho más eficiente en memoria 
    // y velocidad cuando solo queremos comprobar si un elemento existe.
    private HashSet<string> dictionarySpanish = new HashSet<string>();

    // Nombre del archivo txt
    public string fileName = "0_palabras_todas.txt";

    void Start()
    {
        LoadDictionary();
    }
    
    void Update()
    {
        TestList();
    }
    #region Carga del diccionario
    void LoadDictionary()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            print("Cargando diccionario, esto puede tomar unos segundos...");

            // 1. Creamos e iniciamos el cronómetro
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string word = line.Trim().ToLower();

                    if (!string.IsNullOrEmpty(word))
                    {
                        dictionarySpanish.Add(word);
                    }
                }
            }

            // 2. Detenemos el cronómetro al terminar de leer todo
            stopwatch.Stop();

            // 3. Mostramos el resultado en la consola con el tiempo transcurrido
            print("¡Diccionario cargado con éxito! Total de palabras: " + dictionarySpanish.Count);
            print("Tiempo de carga: " + stopwatch.ElapsedMilliseconds + " ms (" + stopwatch.Elapsed.TotalSeconds.ToString("F2") + " segundos).");
        }
        else
        {
            print("No se encontró el archivo " + fileName + " en la ruta: " + filePath + ".");
        }
    }
    #endregion

    #region Chequeo de palabras
    // Método principal para verificar si la palabra formada por el jugador es válida
    public bool CheckWordIsValid(string wordToCheck)
    {
        if (string.IsNullOrEmpty(wordToCheck)) return false;

        // Formateamos la palabra de la misma forma que la guardamos (minúsculas y sin espacios)
        string formattedWord = wordToCheck.Trim().ToLower();

        // Verifica si la palabra está en nuestro HashSet
        bool isValid = dictionarySpanish.Contains(formattedWord);

        // Mencionamos si es válida o no en consola
        if (isValid)
        {
            print($"[Resultado] La palabra '{formattedWord}' ES válida.");
        }
        else
        {
            print($"[Resultado] La palabra '{formattedWord}' NO es válida.");
        }

        return isValid;
    }
    #endregion

    #region CÓDIGOS DE PRUEBA
    private void TestList()
    {
        // --- CÓDIGO DE PRUEBA ---
        // Presiona la tecla Espacio durante el juego para probar dos palabras: una válida y una inventada.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckWordIsValid(wordToCheck); // Palabra que sí está en tu archivo
            CheckWordIsValid(wordToCheck2); // Palabra que no está
        }
    }
    #endregion
}