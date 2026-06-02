using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StageTransition : MonoBehaviour
{
    [Header("Configuración de Objetos del Canvas")]
    [Tooltip("Arrastra aquí los elementos específicos del Canvas que quieres mover.")]
    public List<RectTransform> objetosEpecificos = new List<RectTransform>();

    [Header("Parámetros de Movimiento")]
    [Tooltip("Distancia en el eje X. Valores negativos mueven a la izquierda (simula que Lex avanza hacia la derecha).")]
    public float distanciaDesplazamientoX = -1000f;

    [Tooltip("Tiempo en segundos que durará la animación de movimiento.")]
    public float duracionTransicion = 2f;

    [Header("Eventos")]
    [Tooltip("Añade aquí funciones que deben ocurrir cuando termine de moverse (Ej: Aparecer el nuevo enemigo).")]
    public UnityEvent alTerminarTransicion;

    private bool estaMoviendose = false;

    #region CÓDIGOS DE PRUEBA
    void Update()
    {
        // Presiona la tecla 'T' (de Test) durante el juego para forzar el movimiento
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[Test] Forzando movimiento del escenario...");
            OnEnemyDefeated();
        }
    }
    #endregion

    /// <summary>
    /// Llama a esta función exactamente cuando el HP del enemigo llegue a 0 y sea derrotado.
    /// </summary>
    [ContextMenu("Ejecutar Transición de Prueba")] // <--- AÑADE ESTA LÍNEA AQUÍ
    public void OnEnemyDefeated()
    {
        if (!estaMoviendose)
        {
            StartCoroutine(MoverEscenarioRoutine());
        }
    }

    private IEnumerator MoverEscenarioRoutine()
    {
        estaMoviendose = true;

        // 1. Guardamos la posición de inicio y calculamos la posición de destino para cada objeto
        List<Vector2> posicionesIniciales = new List<Vector2>();
        List<Vector2> posicionesFinales = new List<Vector2>();

        foreach (RectTransform obj in objetosEpecificos)
        {
            if (obj != null)
            {
                posicionesIniciales.Add(obj.anchoredPosition);

                // Sumamos la distancia configurada a la posición actual en X
                Vector2 destino = new Vector2(obj.anchoredPosition.x + distanciaDesplazamientoX, obj.anchoredPosition.y);
                posicionesFinales.Add(destino);
            }
            else
            {
                // Relleno de seguridad por si dejas un hueco vacío en el Inspector
                posicionesIniciales.Add(Vector2.zero);
                posicionesFinales.Add(Vector2.zero);
            }
        }

        float tiempoTranscurrido = 0f;

        // 2. Bucle de interpolación: Mueve los objetos suavemente frame por frame
        while (tiempoTranscurrido < duracionTransicion)
        {
            tiempoTranscurrido += Time.deltaTime;

            // Calculamos el porcentaje del tiempo (de 0 a 1)
            float porcentaje = tiempoTranscurrido / duracionTransicion;

            // Aplicamos un suavizado (Ease In-Out) para que no empiece ni frene de golpe
            float curvaSuave = Mathf.SmoothStep(0f, 1f, porcentaje);

            for (int i = 0; i < objetosEpecificos.Count; i++)
            {
                if (objetosEpecificos[i] != null)
                {
                    // Lerp calcula la posición intermedia basándose en la curva
                    objetosEpecificos[i].anchoredPosition = Vector2.Lerp(posicionesIniciales[i], posicionesFinales[i], curvaSuave);
                }
            }

            yield return null; // Espera al siguiente frame
        }

        // 3. Limpieza final: Nos aseguramos de que terminen exactamente en la coordenada matemática exacta
        for (int i = 0; i < objetosEpecificos.Count; i++)
        {
            if (objetosEpecificos[i] != null)
            {
                objetosEpecificos[i].anchoredPosition = posicionesFinales[i];
            }
        }

        estaMoviendose = false;

        // 4. Disparamos el evento final (por ejemplo, para que aparezca el enemigo o se vuelva a jugar)
        alTerminarTransicion?.Invoke();
    }
}