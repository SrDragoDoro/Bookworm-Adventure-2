using UnityEngine;
using UnityEngine.EventSystems;
public enum ModeActivation { None, AtStart, OnPointerEnter }

public class VoiceTriggerLex : MonoBehaviour, IPointerEnterHandler
{
    

    [Header("Tipo de Activación")]
    public ModeActivation modeActivation = ModeActivation.OnPointerEnter;

    [Header("Configuración de Audio")]
    [Tooltip("Pon 1 audio para botones normales, o 2 audios si es la intro de bienvenida.")]
    public AudioClip[] audioClips;

    [Tooltip("Tiempo a restar del primer audio antes de reproducir el segundo (Solo si hay más de 1 audio).")]
    public float introOverlap = 0f;

    [Header("Gesto / Frase Específica")]
    [Tooltip("El orden de bocas que hará este botón en específico.")]
    public int[] mouthPattern;
    public float mouthChangeSpeed = 0.1f;

    void Start()
    {
        if (modeActivation == ModeActivation.AtStart)
        {
            Disparar();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (modeActivation == ModeActivation.OnPointerEnter)
        {
            Disparar();
        }
    }

    private void Disparar()
    {
        // Validamos que el administrador central exista en la escena antes de llamarlo
        if (VoiceLexManager.Instance != null)
        {
            VoiceLexManager.Instance.EjecutarDiscurso(audioClips, introOverlap, mouthPattern, mouthChangeSpeed);
        }
    }
}