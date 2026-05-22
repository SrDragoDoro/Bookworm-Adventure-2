using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControladorVozLex : MonoBehaviour, IPointerEnterHandler
{
    public enum ModoActivacion { AlInicio, AlPasarCursor }

    [Header("Configuración Principal")]
    [Tooltip("Elige cuándo se activará este script.")]
    public ModoActivacion modoActivacion = ModoActivacion.AlPasarCursor;

    [Tooltip("La imagen de la cabeza de Lex en el Canvas.")]
    public Image personajeImagen;

    [Tooltip("0=Normal, 1=ah, 2=eh, 3=ee, 4=oo, 5=closed")]
    public Sprite[] bocas;

    public AudioSource audioSource;

    [Header("Configuración de Audio")]
    [Tooltip("Agrega 1 audio para botones, o 2 para la intro secuencial.")]
    public AudioClip[] clipsDeAudio;

    [Tooltip("Tiempo a restar del primer audio antes de reproducir el segundo (Ej: 1.28 para la intro).")]
    public float solapamientoIntro = 0f;

    [Header("Configuración de Animación")]
    public int[] patronBocas;
    public float velocidadCambioBoca = 0.1f;

    // Usamos variables estáticas para que todos los botones compartan la misma información.
    // Esto evita que dos botones intenten animar a Lex al mismo tiempo.
    private static Coroutine animacionActiva;
    private static ControladorVozLex controladorActual;

    void Start()
    {
        if (modoActivacion == ModoActivacion.AlInicio)
        {
            ActivarSecuencia();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (modoActivacion == ModoActivacion.AlPasarCursor)
        {
            ActivarSecuencia();
        }
    }

    public void ActivarSecuencia()
    {
        // Detiene la animación anterior si mueves el cursor rápido entre opciones
        if (animacionActiva != null && controladorActual != null)
        {
            controladorActual.StopCoroutine(animacionActiva);
        }

        controladorActual = this;
        animacionActiva = StartCoroutine(SecuenciaHablarYAnimar());
    }

    private IEnumerator SecuenciaHablarYAnimar()
    {
        // 1. Iniciar la animación de los sprites (bocas) en paralelo
        Coroutine rutinaBocas = StartCoroutine(AnimarBocas());

        // 2. Manejar la reproducción de audios secuenciales
        if (clipsDeAudio != null && clipsDeAudio.Length > 0)
        {
            for (int i = 0; i < clipsDeAudio.Length; i++)
            {
                if (clipsDeAudio[i] != null)
                {
                    audioSource.clip = clipsDeAudio[i];
                    audioSource.Play();

                    // Si hay un siguiente clip, esperamos la duración del actual menos el solapamiento
                    if (i < clipsDeAudio.Length - 1)
                    {
                        yield return new WaitForSeconds(clipsDeAudio[i].length - solapamientoIntro);
                    }
                }
            }
        }

        // Esperar a que la animación visual termine
        yield return rutinaBocas;
        animacionActiva = null;
    }

    private IEnumerator AnimarBocas()
    {
        foreach (int i in patronBocas)
        {
            if (i >= 0 && i < bocas.Length)
            {
                personajeImagen.sprite = bocas[i];
            }
            yield return new WaitForSeconds(velocidadCambioBoca);
        }

        // Al terminar, siempre vuelve a la boca neutral (índice 0)
        if (bocas.Length > 0 && personajeImagen != null)
        {
            personajeImagen.sprite = bocas[0];
        }
    }
}