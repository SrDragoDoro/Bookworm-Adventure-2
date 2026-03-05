/*
Aca se puede configurar el personaje y 
el patron de bocas que se usará al pasar 
el mouse por encima del objeto. El 
patron de bocas se configura con un 
array de enteros, donde cada entero 
representa una boca (1=ah,2=eh,3=ee,4=oo,5=closed)). 
La velocidad es el tiempo entre cada cambio 
de boca. Por ejemplo, al pasar el mouse 
por encima del objeto, se reproducirá el 
patrón de bocas configurado en el 
personaje seleccionado. 
*/

using UnityEngine;
using UnityEngine.EventSystems;

public class Voz : MonoBehaviour, IPointerEnterHandler
{    
    public LexVocal personaje;

    [Tooltip("Orden de bocas que se usarán (0=Normal,1=ah,2=eh,3=ee,4=oo,5=closed)")]
    public int[] patronBocas;

    [Tooltip("Velocidad entre cambios de bocas")]
    public float velocidad = 0.1f;

    public void OnPointerEnter(PointerEventData eventData) //Cuando el cursor entra en el área del objeto
    {        
        personaje.ReproducirPatron(patronBocas, velocidad);
    }
}
