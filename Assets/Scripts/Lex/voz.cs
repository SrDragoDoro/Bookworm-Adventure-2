using UnityEngine;
using UnityEngine.EventSystems;

public class Voz : MonoBehaviour, IPointerEnterHandler
{    
    public LexVocal personaje;

    [Tooltip("Orden de bocas que se usarán (0=a,1=i,2=u,3=e,4=o)")]
    public int[] patronBocas;

    [Tooltip("Velocidad entre cambios de bocas")]
    public float velocidad = 0.1f;

    public void OnPointerEnter(PointerEventData eventData)
    {        
        personaje.ReproducirPatron(patronBocas, velocidad);
    }
}
