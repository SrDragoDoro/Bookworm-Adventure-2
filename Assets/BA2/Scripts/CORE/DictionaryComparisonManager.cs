using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Diagnostics;

public enum ModoAlmacenamiento 
{ None, UsarHashSet, UsarDictionary }

public class DictionaryComparisonManager : MonoBehaviour
{

    [Header("Configuración del Experimento")]
    [SerializeField] private ModoAlmacenamiento modo = ModoAlmacenamiento.UsarDictionary;

    [Tooltip("Cantidad de palabras para la prueba segura.")]
    [SerializeField] private int maxPalabrasAClasificar = 500000;

    [Tooltip("¡Cuidado! Si activas esto, ignorará el límite e intentará leer las 7 millones de palabras.")]
    [SerializeField] private bool cargarLas7MillonesDePalabras = false;

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

        int limiteActual = cargarLas7MillonesDePalabras ? 7000000 : maxPalabrasAClasificar;
        print($"Iniciando prueba en modo [{modo}] cargando un máximo de {limiteActual} palabras...");

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        int contador = 0;

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Freno de mano de seguridad para no petar la PC
                if (contador >= limiteActual) break;

                string word = line.Trim().ToLower();

                if (!string.IsNullOrEmpty(word))
                {
                    if (modo == ModoAlmacenamiento.UsarHashSet)
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

        int totalCargado = (modo == ModoAlmacenamiento.UsarHashSet) ? hashSetDict.Count : dictionaryDict.Count;
        print($"[RESULTADO {modo}] Palabras cargadas con éxito: {totalCargado}");
        print($"Tiempo transcurrido: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F4} segundos).");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            VerificarPalabra(wordToCheck);
            VerificarPalabra(wordToCheck2);
        }
    }

    private void VerificarPalabra(string palabra)
    {
        string formateada = palabra.Trim().ToLower();
        bool existe = false;

        // Medimos la velocidad de respuesta en Ticks (1 tick = 100 nanosegundos)
        Stopwatch cronometroBusqueda = Stopwatch.StartNew();

        if (modo == ModoAlmacenamiento.UsarHashSet)
        {
            existe = hashSetDict.Contains(formateada);
        }
        else
        {
            existe = dictionaryDict.ContainsKey(formateada);
        }

        cronometroBusqueda.Stop();

        string resultado = existe ? "SÍ existe" : "NO existe";
        print($"[{modo}] La palabra '{formateada}' {resultado}. Tiempo de búsqueda: {cronometroBusqueda.ElapsedTicks} ticks.");
    }
}