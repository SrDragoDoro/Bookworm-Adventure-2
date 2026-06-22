using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(AudioSource))]
public class CinematicPresentationController : MonoBehaviour
{
    [Header("Grupo 1: Efecto Destello (Flash)")]
    [Tooltip("El SpriteRenderer del rectángulo blanco para el destello inicial.")]
    [SerializeField] private SpriteRenderer flashSprite;
    [Tooltip("Tiempo en segundos que tarda en desaparecer el destello blanco.")]
    [SerializeField] private float flashFadeDuration = 0.5f;

    [Header("Grupo 2: Elementos de Presentación")]
    [Tooltip("El CanvasGroup del panel que contiene las imágenes y textos del evento.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("Arrastra aquí los SpriteRenderers del mundo (letras, círculo, etc).")]
    [SerializeField] private SpriteRenderer[] worldSprites;

    [Header("Tiempos de Presentación (Grupo 2)")]
    [Tooltip("Tiempo que la presentación se quedará en pantalla al máximo visible.")]
    [SerializeField] private float displayDuration = 2.5f;
    [Tooltip("Tiempo que tarda en desvanecerse (Fade Out) la presentación al final.")]
    [SerializeField] private float fadeOutDuration = 1.0f;

    private AudioSource audioSource;
    private Coroutine activeSequenceRoutine;

    // Array para guardar la opacidad máxima de cada sprite según el Inspector
    private float[] originalSpriteAlphas;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // 1. Guardamos la opacidad que le pusiste a cada sprite en el Inspector (ej. 175)
        originalSpriteAlphas = new float[worldSprites.Length];
        for (int i = 0; i < worldSprites.Length; i++)
        {
            if (worldSprites[i] != null)
            {
                originalSpriteAlphas[i] = worldSprites[i].color.a;
            }
        }

        // 2. Empezamos con todo oculto por defecto al iniciar la escena
        SetGroup2State(0f);
        SetFlashState(0f);
    }

    /// <summary>
    /// Inicia toda la secuencia cinemática: Aparece de golpe + Destello -> Espera -> Fade Out.
    /// </summary>
    [Button("Probar Cinemática")]
    public void StartPresentationSequence(AudioClip clipToPlay = null)
    {
        if (activeSequenceRoutine != null) StopCoroutine(activeSequenceRoutine);

        // 1. Iniciamos la rutina visual
        activeSequenceRoutine = StartCoroutine(PresentationRoutine());

        // 2. Iniciamos la rutina del audio con 0.5 segundos de retraso
        if (clipToPlay != null && audioSource != null)
        {
            StartCoroutine(PlayAudioWithDelay(clipToPlay, 0.5f));
        }
    }

    // Nueva corrutina exclusiva para manejar el retraso del sonido
    private IEnumerator PlayAudioWithDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay); // Esperamos el tiempo indicado
        audioSource.PlayOneShot(clip);          // ¡Reproducimos!
    }

    private IEnumerator PresentationRoutine()
    {
        // ==========================================
        // 1. APARICIÓN INSTANTÁNEA DE TODO
        // ==========================================
        SetGroup2State(1f); // Se multiplica internamente por el alpha original
        SetFlashState(1f);  // El destello blanco tapa todo al instante

        // Bloqueamos interacciones del Canvas
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // ==========================================
        // 2. FADE OUT DEL DESTELLO (GRUPO 1) Y ESPERA
        // ==========================================
        float timer = 0f;

        while (timer < displayDuration)
        {
            timer += Time.deltaTime;

            if (flashSprite != null && timer <= flashFadeDuration)
            {
                float flashAlpha = Mathf.Lerp(1f, 0f, timer / flashFadeDuration);
                SetFlashAlpha(flashAlpha);
            }
            else if (flashSprite != null && timer > flashFadeDuration && flashSprite.color.a > 0)
            {
                SetFlashAlpha(0f);
            }

            yield return null;
        }

        // ==========================================
        // 3. FADE OUT DE LA PRESENTACIÓN (GRUPO 2)
        // ==========================================
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        float fadeTimer = 0f;
        while (fadeTimer < fadeOutDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutDuration);
            SetGroup2State(alpha);
            yield return null;
        }

        SetGroup2State(0f);
        activeSequenceRoutine = null;
    }

    #region Funciones de Ayuda Visual
    private void SetGroup2State(float alphaPercent)
    {
        // Aplica alpha a la UI
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alphaPercent;
        }

        // Aplica alpha a los Sprites del mundo RESPETANDO su opacidad original
        for (int i = 0; i < worldSprites.Length; i++)
        {
            if (worldSprites[i] != null)
            {
                Color c = worldSprites[i].color;
                // Multiplicamos el porcentaje actual (0 a 1) por el alpha que pusiste en el Inspector
                c.a = alphaPercent * originalSpriteAlphas[i];
                worldSprites[i].color = c;
            }
        }
    }

    private void SetFlashState(float alpha)
    {
        if (flashSprite != null)
        {
            SetFlashAlpha(alpha);
            flashSprite.gameObject.SetActive(alpha > 0f);
        }
    }

    private void SetFlashAlpha(float alpha)
    {
        Color c = flashSprite.color;
        c.a = alpha;
        flashSprite.color = c;
    }
    #endregion
}