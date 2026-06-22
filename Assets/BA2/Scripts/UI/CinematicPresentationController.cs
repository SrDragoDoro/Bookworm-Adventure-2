using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(AudioSource))]
public class CinematicPresentationController : MonoBehaviour
{
    [Header("Referencias de UI (Canvas)")]
    [Tooltip("El CanvasGroup del panel que contiene las imágenes y textos del evento.")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Referencias de Sprites Extras (Fuera del Canvas)")]
    [Tooltip("Arrastra aquí los SpriteRenderers del mundo que deban desvanecerse junto a la UI (opcional).")]
    [SerializeField] private SpriteRenderer[] worldSprites;

    [Header("Configuración del Fade")]
    [SerializeField] private float fadeDuration = 1.0f; // Tiempo en segundos que tarda en aparecer/desaparecer

    private AudioSource audioSource;
    private Coroutine activeFadeRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Si no asignaste el CanvasGroup, intentamos buscarlo en este mismo GameObject
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Empezamos con todo oculto por defecto al iniciar el juego
        SetInitialState(0f);
    }

    private void Start()
    {
        
    }

    /// <summary>
    /// Muestra la pantalla cinemática con un efecto Fade In y reproduce un audio.
    /// </summary>

    [Button]
    public void ShowPresentation(AudioClip clipToPlay = null)
    {
        if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);

        // Bloqueamos interacciones del Canvas de inmediato para que el jugador no de clicks por detrás
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // Reproducir sonido si se asignó uno
        if (clipToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }

        activeFadeRoutine = StartCoroutine(FadeRoutine(1f));
    }

    /// <summary>
    /// Oculta la pantalla cinemática con un efecto Fade Out.
    /// </summary>
    public void HidePresentation()
    {
        if (activeFadeRoutine != null) StopCoroutine(activeFadeRoutine);

        // Desactivamos la detección de clicks antes de desvanecer para liberar el juego
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        activeFadeRoutine = StartCoroutine(FadeRoutine(0f));
    }

    // Rutina matemática que cambia el Alpha frame a frame suavemente
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlphaUI = canvasGroup != null ? canvasGroup.alpha : 0f;

        // Guardamos las opacidades iniciales de los sprites del mundo
        float[] startAlphasSprites = new float[worldSprites.Length];
        for (int i = 0; i < worldSprites.Length; i++)
        {
            if (worldSprites[i] != null) startAlphasSprites[i] = worldSprites[i].color.a;
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;

            // 1. Lerp para el Canvas de la UI
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlphaUI, targetAlpha, normalizedTime);
            }

            // 2. Lerp para los Sprites del escenario
            for (int i = 0; i < worldSprites.Length; i++)
            {
                if (worldSprites[i] != null)
                {
                    Color c = worldSprites[i].color;
                    c.a = Mathf.Lerp(startAlphasSprites[i], targetAlpha, normalizedTime);
                    worldSprites[i].color = c;
                }
            }

            yield return null; // Espera al siguiente frame
        }

        // Aseguramos los valores exactos al terminar
        SetInitialState(targetAlpha);
        activeFadeRoutine = null;
    }

    private void SetInitialState(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = alpha > 0.5f;
            canvasGroup.interactable = alpha > 0.5f;
        }

        foreach (var sprite in worldSprites)
        {
            if (sprite != null)
            {
                Color c = sprite.color;
                c.a = alpha;
                sprite.color = c;
            }
        }
    }
}