using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class GroupParallax
{
    [Tooltip("Añade aquí todos los objetos que compartirán la misma intensidad de movimiento")]
    public List<RectTransform> objetos = new List<RectTransform>();

    [Tooltip("Valores negativos hacen que se mueva en dirección opuesta al mouse")]
    public float intensidadX = -0.05f;
    public float intensidadY = -0.05f;

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
    [Tooltip("Se ejecutará cuando el escenario se mueva")]
    public UnityEvent alDesplazarEscenario;

    // Variable para controlar si el parallax está pausado por una transición
    private bool isPaused = false;

    #region Metodos Unity
    void Start()
    {
        // Al iniciar, recorremos cada grupo para guardar las posiciones iniciales
        foreach (var grupo in grupos)
        {
            grupo.posicionesBase = new Vector2[grupo.objetos.Count];

            for (int i = 0; i < grupo.objetos.Count; i++)
            {
                if (grupo.objetos[i] != null)
                {
                    grupo.posicionesBase[i] = grupo.objetos[i].anchoredPosition;
                }
            }
        }
    }

    void Update()
    {
        // Si estamos en medio de una transición, cancelamos el efecto Parallax para que no peleen los scripts
        if (isPaused) return;

        Vector3 mousePos = Input.mousePosition;
        float mouseX = (mousePos.x / Screen.width) - 0.5f;
        float mouseY = (mousePos.y / Screen.height) - 0.5f;

        foreach (var grupo in grupos)
        {
            float offsetX = mouseX * Screen.width * grupo.intensidadX;
            float offsetY = mouseY * Screen.height * grupo.intensidadY;

            for (int i = 0; i < grupo.objetos.Count; i++)
            {
                if (grupo.objetos[i] == null) continue;

                Vector2 posicionObjetivo = new Vector2(
                    grupo.posicionesBase[i].x + offsetX,
                    grupo.posicionesBase[i].y + offsetY
                );

                grupo.objetos[i].anchoredPosition = Vector2.Lerp(
                    grupo.objetos[i].anchoredPosition,
                    posicionObjetivo,
                    Time.deltaTime * suavizado
                );
            }
        }
    }
    #endregion

    #region Control de Transición
    public void PauseParallax()
    {
        isPaused = true;
    }

    // Este método actualiza las posiciones base al nuevo escenario y reactiva el efecto
    public void ResumeAndRecalibrate()
    {
        foreach (var grupo in grupos)
        {
            for (int i = 0; i < grupo.objetos.Count; i++)
            {
                if (grupo.objetos[i] != null)
                {
                    // Guardamos la nueva posición en la que terminaron después de la transición
                    grupo.posicionesBase[i] = grupo.objetos[i].anchoredPosition;
                }
            }
        }
        isPaused = false;
        alDesplazarEscenario?.Invoke();
    }
    #endregion
}