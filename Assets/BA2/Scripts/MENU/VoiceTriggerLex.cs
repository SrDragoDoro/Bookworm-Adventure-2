using UnityEngine;
using UnityEngine.EventSystems;

public class VoiceTriggerLex : MonoBehaviour, IPointerEnterHandler
{
    public enum ModoActivacion { AlInicio, AlPasarCursor }

    [Header("Tipo de Activación")]
    public ModoActivacion modoActivacion = ModoActivacion.AlPasarCursor;

    [Header("Configuración de Audio")]
    [Tooltip("Pon 1 audio para botones normales, o 2 audios si es la intro de bienvenida.")]
    public AudioClip[] clipsDeAudio;

    [Tooltip("Tiempo a restar del primer audio antes de reproducir el segundo (Solo si hay más de 1 audio).")]
    public float solapamientoIntro = 0f;

    [Header("Gesto / Frase Específica")]
    [Tooltip("El orden de bocas que hará este botón en específico.")]
    public int[] patronBocas;
    public float velocidadCambioBoca = 0.1f;

    void Start()
    {
        if (modoActivacion == ModoActivacion.AlInicio)
        {
            Disparar();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (modoActivacion == ModoActivacion.AlPasarCursor)
        {
            Disparar();
        }
    }

    private void Disparar()
    {
        // Validamos que el administrador central exista en la escena antes de llamarlo
        if (VoiceLexManager.Instancia != null)
        {
            VoiceLexManager.Instancia.EjecutarDiscurso(clipsDeAudio, solapamientoIntro, patronBocas, velocidadCambioBoca);
        }
    }
}