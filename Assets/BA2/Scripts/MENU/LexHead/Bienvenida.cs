/*
A diferencia del script "Voz", este script se ejecuta 
automáticamente al iniciar la escena, sin necesidad de 
interacción del usuario. Al iniciar, el personaje 
seleccionado reproducirá el patrón de bocas configurado, 
lo que puede ser útil para dar la bienvenida al jugador o 
para mostrar una animación inicial del personaje.
*/
using UnityEngine;

public class Bienvenida : MonoBehaviour
{
    public LexVocal personaje;

    [Tooltip("Orden de bocas que se usarán (0=Normal,1=ah,2=eh,3=ee,4=oo,5=closed)")]
    public int[] patronBocas;

    [Tooltip("Velocidad entre cambios de bocas")]
    public float velocidad = 0.1f;

    void Start()
    {
        personaje.ReproducirPatron(patronBocas, velocidad);
    }
}