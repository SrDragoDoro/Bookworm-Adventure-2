using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct CommentaryTier
{
    [Tooltip("Longitud mínima de la palabra")]
    public int minLength;
    [Tooltip("Longitud máxima de la palabra")]
    public int maxLength;
    [Tooltip("Texto que dirá Lex (ej: ¡BUENA!)")]
    public string phrase;
    [Tooltip("Audio correspondiente a la frase")]
    public AudioClip audioClip;
}

public class LexCommentaryManager : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("El GameObject padre de la nube para activarlo/desactivarlo.")]
    public GameObject speechBubbleObject;
    [Tooltip("El SpriteRenderer de la nube de diálogo.")]
    public SpriteRenderer speechBubbleRenderer;
    [Tooltip("El componente de texto TMP.")]
    public TMP_Text commentaryText;

    [Header("Configuración de Audio")]
    public AudioSource lexAudioSource;

    [Header("Tiempos de Animación")]
    [Tooltip("Tiempo en segundos que el texto se queda en pantalla antes de desvanecerse.")]
    public float displayDuration = 1.5f;
    [Tooltip("Tiempo en segundos que tarda en desaparecer (Fade Out).")]
    public float fadeDuration = 0.5f;

    [Header("Niveles de Comentarios")]
    public List<CommentaryTier> commentaryTiers = new List<CommentaryTier>();

    private Coroutine currentFadeCoroutine;

    void Start()
    {
        // Nos aseguramos de que la nube esté invisible y desactivada al iniciar
        if (speechBubbleObject != null)
        {
            SetAlpha(0f);
            speechBubbleObject.SetActive(false);
        }
    }

    public void TriggerCommentary(int wordLength)
    {
        CommentaryTier? selectedTier = null;

        // Buscamos si la longitud de la palabra encaja en algún rango
        foreach (var tier in commentaryTiers)
        {
            if (wordLength >= tier.minLength && wordLength <= tier.maxLength)
            {
                selectedTier = tier;
                break;
            }
        }

        if (selectedTier.HasValue)
        {
            commentaryText.text = selectedTier.Value.phrase;

            if (selectedTier.Value.audioClip != null && lexAudioSource != null)
            {
                lexAudioSource.clip = selectedTier.Value.audioClip;
                lexAudioSource.Play();
            }

            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }

            // Verificamos que el objeto que tiene este script esté activo para lanzar la corrutina
            if (gameObject.activeInHierarchy)
            {
                currentFadeCoroutine = StartCoroutine(ShowAndFadeRoutine());
            }
        }
    }

    private IEnumerator ShowAndFadeRoutine()
    {
        // 1. Aparición instantánea
        speechBubbleObject.SetActive(true);
        SetAlpha(1f);

        // 2. Espera el tiempo de visualización
        yield return new WaitForSeconds(displayDuration);

        // 3. Fade Out progresivo
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // 4. Asegurar que quede completamente invisible y desactivar
        SetAlpha(0f);
        speechBubbleObject.SetActive(false);
    }

    /// <summary>
    /// Cambia la opacidad (alpha) del Sprite y del Texto simultáneamente.
    /// </summary>
    private void SetAlpha(float alpha)
    {
        if (speechBubbleRenderer != null)
        {
            Color spriteColor = speechBubbleRenderer.color;
            spriteColor.a = alpha;
            speechBubbleRenderer.color = spriteColor;
        }

        if (commentaryText != null)
        {
            Color textColor = commentaryText.color;
            textColor.a = alpha;
            commentaryText.color = textColor;
        }
    }
}