/*
Definimos un script llamado "LexVocal" 
que nos ayudara a definir patrones de 
animación para la boca del personaje 
para usarlos en cada boton del menu.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LexVocal : MonoBehaviour
{
    public Sprite[] bocas;             // 0=Normal,1=ah,2=eh,3=ee,4=oo,5=closed
    public Image personajeImagen;

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
            personajeImagen.sprite = bocas[i];
            yield return new WaitForSeconds(velocidad);
        }

        // Al terminar vuelve a boca neutral (ej: índice 0)
        personajeImagen.sprite = bocas[0];
        animacionActual = null;
    }
}
