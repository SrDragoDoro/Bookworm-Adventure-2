using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Diagnostics;

public enum ModeToUse 
{ None, UseHashSet, UseDictionary }

public class DictionaryComparisonManager : MonoBehaviour
{
    [Header("Configuración del Experimento")]
    [SerializeField] private ModeToUse mode = ModeToUse.UseDictionary;

    [Tooltip("Cantidad de palabras para la prueba segura.")]
    [SerializeField] private int maxWords = 100000;

    [Tooltip("¡Cuidado! Si activas esto, ignorará el límite e intentará leer las 600 mil palabras.")]
    [SerializeField] private bool loadAll600ThousandWords = false;

    [Header("Palabras a Verificar (Espacio para probar)")]
    [SerializeField] private string wordToCheck = "palabra";
    [SerializeField] private string wordToCheck2 = "palabrainventada123";

    public string fileName = "0_palabras_todas.txt";

    // Estructuras de datos para comparar
    private HashSet<string> hashSetDict = new HashSet<string>();
    private Dictionary<string, object> dictionaryDict = new Dictionary<string, object>();

    // Objeto comodín para no saturar la memoria RAM con el Dictionary
    private static readonly object dummyValue = new object();

    void Start()
    {
        ProbarCarga();
    }

    void ProbarCarga()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(filePath))
        {
            print("No se encontró el archivo " + fileName + " en " + filePath);
            return;
        }

        // Limpiamos ambos contenedores antes de la prueba
        hashSetDict.Clear();
        dictionaryDict.Clear();

        int currentLimit = loadAll600ThousandWords ? 600000 : maxWords;
        print($"Iniciando prueba en modo [{mode}] cargando un máximo de {currentLimit} palabras...");

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        int contador = 0;

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Freno de mano de seguridad para no petar la PC
                if (contador >= currentLimit) break;

                string word = line.Trim().ToLower();

                if (!string.IsNullOrEmpty(word))
                {
                    if (mode == ModeToUse.UseHashSet)
                    {
                        hashSetDict.Add(word);
                    }
                    else
                    {
                        // Estructura Dictionary<string, object>
                        if (!dictionaryDict.ContainsKey(word))
                        {
                            dictionaryDict.Add(word, dummyValue);
                        }
                    }
                    contador++;
                }
            }
        }

        stopwatch.Stop();

        int totalCargado = (mode == ModeToUse.UseHashSet) ? hashSetDict.Count : dictionaryDict.Count;
        print($"[RESULTADO {mode}] Palabras cargadas con éxito: {totalCargado}");
        print($"Tiempo transcurrido: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F4} segundos).");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            VerifyWord(wordToCheck);
            VerifyWord(wordToCheck2);
        }
    }

    private void VerifyWord(string palabra)
    {
        string formatted = palabra.Trim().ToLower();
        bool exist = false;

        // Medimos la velocidad de respuesta en Ticks (1 tick = 100 nanosegundos)
        Stopwatch searchStopwatch = Stopwatch.StartNew();

        if (mode == ModeToUse.UseHashSet)
        {
            exist = hashSetDict.Contains(formatted);
        }
        else
        {
            exist = dictionaryDict.ContainsKey(formatted);
        }

        searchStopwatch.Stop();

        string result = exist ? "SÍ existe" : "NO existe";
        print($"[{mode}] La palabra '{formatted}' {result}. Tiempo de búsqueda: {searchStopwatch.ElapsedTicks} ticks.");
    }
}