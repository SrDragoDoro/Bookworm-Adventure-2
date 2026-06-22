using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Diagnostics;

public class DictionaryManager : MonoBehaviour
{
    [SerializeField] private string wordToCheck = "mama";
    [SerializeField] private string wordToCheck2 = "palabrainventada123";

    // CLAVE: Palabra sin tildes ni diéresis (ej. "nino")
    // VALOR: Palabra original correcta (ej. "niño")
    private Dictionary<string, string> dictionarySpanish = new Dictionary<string, string>();

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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string originalWord = line.Trim().ToLower();

                    if (!string.IsNullOrEmpty(originalWord))
                    {
                        string normalizedWord = RemoveAccents(originalWord);

                        // Si la palabra no existe, la añadimos.
                        // Si ya existe (ej. "bebe"), y la nueva tiene tilde ("bebé"), la sobreescribimos 
                        // para darle prioridad a la visualización de la tilde/eñe en el juego.
                        if (!dictionarySpanish.ContainsKey(normalizedWord))
                        {
                            dictionarySpanish.Add(normalizedWord, originalWord);
                        }
                        else if (originalWord != normalizedWord)
                        {
                            dictionarySpanish[normalizedWord] = originalWord;
                        }
                    }
                }
            }

            stopwatch.Stop();
            print("¡Diccionario cargado con éxito! Total de palabras únicas base: " + dictionarySpanish.Count);
            print("Tiempo de carga: " + stopwatch.ElapsedMilliseconds + " ms.");
        }
        else
        {
            print("No se encontró el archivo " + fileName + " en la ruta: " + filePath + ".");
        }
    }

    // Método mágico que le quita las tildes, diéresis y transforma la ñ en n
    private string RemoveAccents(string input)
    {
        return input.Replace('á', 'a').Replace('é', 'e').Replace('í', 'i')
                    .Replace('ó', 'o').Replace('ú', 'u').Replace('ü', 'u')
                    .Replace('ñ', 'n');
    }
    #endregion

    #region Chequeo de palabras
    // AHORA DEVUELVE TAMBIÉN LA PALABRA CORREGIDA A TRAVÉS DE 'out string correctedWord'
    public bool CheckWordIsValid(string wordToCheck, out string correctedWord)
    {
        correctedWord = "";
        if (string.IsNullOrEmpty(wordToCheck)) return false;

        // Limpiamos la palabra que armó el jugador (por si acaso)
        string normalizedInput = RemoveAccents(wordToCheck.Trim().ToLower());

        // Buscamos si existe la versión sin tildes en el diccionario
        if (dictionarySpanish.TryGetValue(normalizedInput, out correctedWord))
        {
            //print($"[Resultado] Valida. Entrada: '{normalizedInput}', Corregida a: '{correctedWord}'");
            return true;
        }

        return false;
    }
    #endregion

    #region CÓDIGOS DE PRUEBA
    private void TestList()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckWordIsValid(wordToCheck, out string res1);
            CheckWordIsValid(wordToCheck2, out string res2);
        }
    }
    #endregion
}