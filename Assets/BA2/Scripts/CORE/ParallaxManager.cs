using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// GroupParallax para que contiene una lista de objetos que se moverán con la misma intensidad, y guarda su posición base individualmente
[System.Serializable]
public class GroupParallax
{
    [Tooltip("Añade aquí todos los objetos que compartirán la misma intensidad de movimiento")]
    public List<RectTransform> objetos = new List<RectTransform>();

    [Tooltip("Valores negativos hacen que se mueva en dirección opuesta al mouse")]
    public float intensidadX = -0.05f;
    public float intensidadY = -0.05f;

    // Arreglo oculto para guardar la posición original de CADA objeto de la lista
    [HideInInspector] public Vector2[] posicionesBase;
}
public class ParallaxManager : MonoBehaviour
{
    [Header("Grupos del Escenario")]
    [Tooltip("Agrupa los elementos por su nivel de profundidad")]
    public List<GroupParallax> grupos = new List<GroupParallax>();

    [Header("Configuración General")]
    public float suavizado = 5f;

    [Header("Eventos de Escenario")]
    [Tooltip("Se ejecutará cuando el escenario se mueva (ideal para sonidos o partículas)")]
    public UnityEvent alDesplazarEscenario;

    #region Metodos Unity
    void Start()
    {
        // Al iniciar, recorremos cada grupo para guardar las posiciones iniciales
        foreach (var grupo in grupos)
        {
            // Creamos un arreglo de posiciones del mismo tamaño que la lista de objetos
            grupo.posicionesBase = new Vector2[grupo.objetos.Count];

            for (int i = 0; i < grupo.objetos.Count; i++)
            {
                if (grupo.objetos[i] != null)
                {
                    // Guardamos la posición inicial de cada objeto individual
                    grupo.posicionesBase[i] = grupo.objetos[i].anchoredPosition;
                }
            }
        }
    }

    void Update()
    {
        // 1. Calculamos la posición del mouse normalizada (-0.5 a 0.5) una sola vez
        Vector3 mousePos = Input.mousePosition;
        float mouseX = (mousePos.x / Screen.width) - 0.5f;
        float mouseY = (mousePos.y / Screen.height) - 0.5f;

        // 2. Procesamos cada grupo
        foreach (var grupo in grupos)
        {
            // Calculamos el desfase una sola vez por grupo (¡Optimización!)
            float offsetX = mouseX * Screen.width * grupo.intensidadX;
            float offsetY = mouseY * Screen.height * grupo.intensidadY;

            // 3. Movemos todos los objetos dentro de este grupo
            for (int i = 0; i < grupo.objetos.Count; i++)
            {
                if (grupo.objetos[i] == null) continue;

                // Calculamos el objetivo sumando el desfase a la posición base específica del objeto
                Vector2 posicionObjetivo = new Vector2(
                    grupo.posicionesBase[i].x + offsetX,
                    grupo.posicionesBase[i].y + offsetY
                );

                // Aplicamos el movimiento suave
                grupo.objetos[i].anchoredPosition = Vector2.Lerp(
                    grupo.objetos[i].anchoredPosition,
                    posicionObjetivo,
                    Time.deltaTime * suavizado
                );
            }
        }
    }

    #endregion

    #region Public Methods
    /*
    Llama a esta función cuando Lex derrote a un enemigo para mover el escenario.
    Ejemplo de uso: MoverEscenario(-100f);
    */
    public void MoverEscenario(float cantidadX)
    {
        // Actualizamos todas las posiciones base para el nuevo centro del nivel
        foreach (var grupo in grupos)
        {
            for (int i = 0; i < grupo.posicionesBase.Length; i++)
            {
                grupo.posicionesBase[i].x += cantidadX;
            }
        }

        // Disparamos el evento
        alDesplazarEscenario.Invoke();
    }

    #endregion
}