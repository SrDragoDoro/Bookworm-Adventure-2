using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StageTransition : MonoBehaviour
{
    [Header("Configuración de Objetos del Canvas")]
    [Tooltip("Arrastra aquí los elementos específicos del Canvas que quieres mover.")]
    public List<RectTransform> SpecificObjects = new List<RectTransform>();

    [Header("Parámetros de Movimiento")]
    [Tooltip("Distancia en el eje X. Valores negativos mueven a la izquierda (simula que Lex avanza hacia la derecha).")]
    public float distanceDisplacementX = -1800f;

    [Tooltip("Tiempo en segundos que durará la animación de movimiento.")]
    public float transitionDuration = 5f;

    [Header("Eventos")]
    [Tooltip("Añade aquí funciones que deben ocurrir cuando termine de moverse (Ej: Aparecer el nuevo enemigo).")]
    public UnityEvent onTransitionComplete;

    private bool isMoving = false;

    #region CÓDIGOS DE PRUEBA
    void Update()
    {
        // Presionar la tecla 'T' durante el juego para forzar el movimiento
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[Test] Forzando movimiento del escenario...");
            OnEnemyDefeated();
        }
    }
    #endregion

    // Llama a esta función exactamente cuando el HP del enemigo llegue a 0 y sea derrotado.    
    [ContextMenu("Ejecutar Transición de Prueba")]
    public void OnEnemyDefeated()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveSceneCoutine());
        }
    }

    private IEnumerator MoveSceneCoutine()
    {
        isMoving = true;

        // 1. Guardamos la posición de inicio y calculamos la posición de destino para cada objeto
        List<Vector2> initialPositions = new List<Vector2>();
        List<Vector2> finalPositions = new List<Vector2>();

        foreach (RectTransform obj in SpecificObjects)
        {
            if (obj != null)
            {
                initialPositions.Add(obj.anchoredPosition);

                // Sumamos la distancia configurada a la posición actual en X
                Vector2 destino = new Vector2(obj.anchoredPosition.x + distanceDisplacementX, obj.anchoredPosition.y);
                finalPositions.Add(destino);
            }
            else
            {
                // Relleno de seguridad por si hay un hueco vacío en el Inspector
                initialPositions.Add(Vector2.zero);
                finalPositions.Add(Vector2.zero);
            }
        }

        float elapsedTime = 0f;

        // 2. Bucle de interpolación: Mueve los objetos suavemente frame por frame
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculamos el porcentaje del tiempo (de 0 a 1)
            float percentage = elapsedTime / transitionDuration;

            // Aplicamos un suavizado (Ease In-Out) para que no empiece ni frene de golpe
            float smoothCurve = Mathf.SmoothStep(0f, 1f, percentage);

            for (int i = 0; i < SpecificObjects.Count; i++)
            {
                if (SpecificObjects[i] != null)
                {
                    // Lerp calcula la posición intermedia basándose en la curva
                    SpecificObjects[i].anchoredPosition = Vector2.Lerp(initialPositions[i], finalPositions[i], smoothCurve);
                }
            }

            yield return null; // Espera al siguiente frame
        }

        // 3. Limpieza final: Nos aseguramos de que terminen exactamente en la coordenada matemática exacta
        for (int i = 0; i < SpecificObjects.Count; i++)
        {
            if (SpecificObjects[i] != null)
            {
                SpecificObjects[i].anchoredPosition = finalPositions[i];
            }
        }

        isMoving = false;

        // 4. Disparamos el evento final (por ejemplo, para que aparezca el enemigo o se vuelva a jugar)
        onTransitionComplete?.Invoke();
    }
}