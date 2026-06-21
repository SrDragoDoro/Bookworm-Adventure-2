using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StageTransition : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el ParallaxManager para pausarlo durante la transición y evitar teletransportes.")]
    public ParallaxManager parallaxManager;

    [Header("Configuración de Objetos del Canvas")]
    [Tooltip("Arrastra aquí los elementos específicos del Canvas que quieres mover.")]
    public List<RectTransform> SpecificObjects = new List<RectTransform>();

    [Header("Parámetros de Movimiento")]
    [Tooltip("Distancia en el eje X. Valores negativos mueven a la izquierda.")]
    public float distanceDisplacementX = -1800f;

    [Tooltip("Tiempo en segundos que durará la animación de movimiento.")]
    public float transitionDuration = 5f;

    [Header("Eventos")]
    [Tooltip("Añade aquí funciones que deben ocurrir cuando termine de moverse.")]
    public UnityEvent onTransitionComplete;

    public EnemyData currentEnemy;

    private bool isMoving = false;
    private bool isFinished = false;


    #region CÓDIGOS DE PRUEBA
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[Test] Forzando movimiento del escenario...");
            OnEnemyDefeated();
        }
    }
    #endregion

    public void DeadEnemy()
    {
        if (!isFinished)
        {
            isFinished = true;
        }
    }

    [ContextMenu("Ejecutar Transición de Prueba")]
    public void OnEnemyDefeated()
    {
        if (!isMoving ||isFinished)
        {
            StartCoroutine(MoveSceneCoutine());
        }
    }

    private IEnumerator MoveSceneCoutine()
    {
        isMoving = true;

        // 1. Pausamos el Parallax antes de empezar a mover
        if (parallaxManager != null)
        {
            parallaxManager.PauseParallax();
        }

        List<Vector2> initialPositions = new List<Vector2>();
        List<Vector2> finalPositions = new List<Vector2>();

        foreach (RectTransform obj in SpecificObjects)
        {
            if (obj != null)
            {
                initialPositions.Add(obj.anchoredPosition);
                Vector2 destino = new Vector2(obj.anchoredPosition.x + distanceDisplacementX, obj.anchoredPosition.y);
                finalPositions.Add(destino);
            }
            else
            {
                initialPositions.Add(Vector2.zero);
                finalPositions.Add(Vector2.zero);
            }
        }

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentage = elapsedTime / transitionDuration;
            float smoothCurve = Mathf.SmoothStep(0f, 1f, percentage);

            for (int i = 0; i < SpecificObjects.Count; i++)
            {
                if (SpecificObjects[i] != null)
                {
                    SpecificObjects[i].anchoredPosition = Vector2.Lerp(initialPositions[i], finalPositions[i], smoothCurve);
                }
            }

            yield return null;
        }

        for (int i = 0; i < SpecificObjects.Count; i++)
        {
            if (SpecificObjects[i] != null)
            {
                SpecificObjects[i].anchoredPosition = finalPositions[i];
            }
        }

        // 2. Reactivamos y recalibramos el Parallax en su nueva ubicación
        if (parallaxManager != null)
        {
            parallaxManager.ResumeAndRecalibrate();
        }

        isMoving = false;
        onTransitionComplete?.Invoke();
    }
}