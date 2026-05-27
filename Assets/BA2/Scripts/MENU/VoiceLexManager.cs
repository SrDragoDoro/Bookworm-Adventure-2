using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VoiceLexManager : MonoBehaviour
{
    public static VoiceLexManager Instance { get; private set; }

    [Header("Referencias Centralizadas")]
    public Image characterImage;

    [Tooltip("0=Normal, 1=ah, 2=eh, 3=ee, 4=oo, 5=closed")]
    public Sprite[] mouthSprites;

    public AudioSource audioSource;

    //SEPARAMOS LAS CORRUTINAS
    private Coroutine sequenceCurrentAudio;
    private Coroutine animationMouthCurrent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EjecutarDiscurso(AudioClip[] clips, float overlap, int[] pattern, float speed)
    {
        // 1. Detenemos la secuencia de audios anterior
        if (sequenceCurrentAudio != null)
        {
            StopCoroutine(sequenceCurrentAudio);
        }

        // 2. DETENEMOS LA ANIMACIÓN DE SPRITES ANTERIOR (Aquí estaba el bug)
        if (animationMouthCurrent != null)
        {
            StopCoroutine(animationMouthCurrent);
        }

        sequenceCurrentAudio = StartCoroutine(SecuenciaHablarYAnimar(clips, overlap, pattern, speed));
    }

    private IEnumerator SecuenciaHablarYAnimar(AudioClip[] clips, float overlap, int[] pattern, float speed)
    {
        // Guardamos la referencia de la corrutina de las bocas para poder detenerla si pasas a otro botón
        animationMouthCurrent = StartCoroutine(AnimateMouth(pattern, speed));

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
                        yield return new WaitForSeconds(clips[i].length - overlap);
                    }
                }
            }
        }

        // Esperamos a que termine la animación visual antes de limpiar la variable principal
        yield return animationMouthCurrent;
        sequenceCurrentAudio = null;
    }

    private IEnumerator AnimateMouth(int[] patron, float velocidad)
    {
        foreach (int i in patron)
        {
            if (i >= 0 && i < mouthSprites.Length && characterImage != null)
            {
                characterImage.sprite = mouthSprites[i];
            }
            yield return new WaitForSeconds(velocidad);
        }

        // Al terminar de forma natural, vuelve a la boca neutral
        if (mouthSprites.Length > 0 && characterImage != null)
        {
            characterImage.sprite = mouthSprites[0];
        }
    }
}