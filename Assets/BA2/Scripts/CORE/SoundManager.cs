using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Instancia estática para el patrón Singleton
    public static SoundManager Instance { get; private set; }

    [Header("Fuentes de Audio")]
    [Tooltip("AudioSource dedicado a la música o sonidos en bucle.")]
    public AudioSource [] loopSource;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
            // Mantiene el SoundManager vivo entre escenas
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destruye duplicados si vuelves a cargar la escena inicial
            Destroy(gameObject);
        }
    }


    // Reproduce un clip de audio de forma continua (en bucle). 
    public void PlayLoop(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("El AudioClip está vacío.");
            return;
        }

        // Asigna el clip, activa el bucle y reproduce
        foreach (AudioSource source in loopSource)
        {
            source.clip = audioClip;
            source.loop = true;
            source.Play();
        }
    }

    // Detiene el audio en bucle actual.
    public void StopLoop()
    {
        foreach (AudioSource source in loopSource)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }
}