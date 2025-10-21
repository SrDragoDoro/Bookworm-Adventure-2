using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LexVocal : MonoBehaviour
{
    public Sprite[] bocas;             // 0=a, 1=i, 2=u, 3=e, 4=o (por ejemplo)
    public Image personajeImage;

    private Coroutine animacionActual;

    // Llamado desde un botón para reproducir un patrón específico
    public void ReproducirPatron(int[] patron, float velocidad)
    {
        if (animacionActual != null)
            StopCoroutine(animacionActual);

        animacionActual = StartCoroutine(AnimarBocas(patron, velocidad));
    }

    private IEnumerator AnimarBocas(int[] patron, float velocidad)
    {
        foreach (int i in patron)
        {
            personajeImage.sprite = bocas[i];
            yield return new WaitForSeconds(velocidad);
        }

        // Al terminar vuelve a boca neutral (ej: índice 0)
        personajeImage.sprite = bocas[0];
        animacionActual = null;
    }
}
