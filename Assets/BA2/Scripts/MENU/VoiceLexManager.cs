using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VoiceLexManager : MonoBehaviour
{
    public static VoiceLexManager Instancia { get; private set; }

    [Header("Referencias Centralizadas")]
    public Image personajeImagen;

    [Tooltip("0=Normal, 1=ah, 2=eh, 3=ee, 4=oo, 5=closed")]
    public Sprite[] bocas;

    public AudioSource audioSource;

    //SEPARAMOS LAS CORRUTINAS
    private Coroutine secuenciaAudioActual;
    private Coroutine animacionBocasActual;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EjecutarDiscurso(AudioClip[] clips, float solapamiento, int[] patron, float velocidad)
    {
        // 1. Detenemos la secuencia de audios anterior
        if (secuenciaAudioActual != null)
        {
            StopCoroutine(secuenciaAudioActual);
        }

        // 2. DETENEMOS LA ANIMACIÓN DE SPRITES ANTERIOR (Aquí estaba el bug)
        if (animacionBocasActual != null)
        {
            StopCoroutine(animacionBocasActual);
        }

        secuenciaAudioActual = StartCoroutine(SecuenciaHablarYAnimar(clips, solapamiento, patron, velocidad));
    }

    private IEnumerator SecuenciaHablarYAnimar(AudioClip[] clips, float solapamiento, int[] patron, float velocidad)
    {
        // Guardamos la referencia de la corrutina de las bocas para poder detenerla si pasas a otro botón
        animacionBocasActual = StartCoroutine(AnimarBocas(patron, velocidad));

        if (clips != null && clips.Length > 0)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    audioSource.clip = clips[i];
                    audioSource.Play();

                    if (i < clips.Length - 1)
                    {
                        yield return new WaitForSeconds(clips[i].length - solapamiento);
                    }
                }
            }
        }

        // Esperamos a que termine la animación visual antes de limpiar la variable principal
        yield return animacionBocasActual;
        secuenciaAudioActual = null;
    }

    private IEnumerator AnimarBocas(int[] patron, float velocidad)
    {
        foreach (int i in patron)
        {
            if (i >= 0 && i < bocas.Length && personajeImagen != null)
            {
                personajeImagen.sprite = bocas[i];
            }
            yield return new WaitForSeconds(velocidad);
        }

        // Al terminar de forma natural, vuelve a la boca neutral
        if (bocas.Length > 0 && personajeImagen != null)
        {
            personajeImagen.sprite = bocas[0];
        }
    }
}