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

    public EnemyData currentEnemy; //Es un ScriptableObject que contiene la información del enemigo actual, incluyendo su vida.

    // ==========================================
    // NUEVO: Variables para la Secuencia de Tesoros y Escena
    // ==========================================
    [Header("Secuencia de Victoria (Tesoros)")]
    [Tooltip("El CinematicPresentationController de tu PanelDeTesoros.")]
    public CinematicPresentationController treasurePanelCinematic;

    // NUEVO: Agregamos el campo para el sonido del tesoro
    [Tooltip("El clip de audio que se reproducirá al mostrar el tesoro.")]
    public AudioClip treasureAudioClip;

    [Tooltip("Segundos a esperar DESPUÉS de que el enemigo muere para mostrar el tesoro.")]
    public float delayBeforeTreasure = 2.0f;

    [Tooltip("Tiempo total que dura la cinemática del tesoro (Display Duration + Fade Out Duration).")]
    public float treasureCinematicDuration = 3.5f;

    [Header("Cambio de Escena")]
    [Tooltip("Tiempo a esperar DESPUÉS de que desaparece el panel de tesoros antes de cambiar de escena.")]
    public float delayBeforeSceneChange = 2.0f;

    [Tooltip("Referencia a tu script SceneManager (arrastra el objeto que lo contiene).")]
    public SceneManager customSceneManager;

    [Tooltip("El nombre exacto de la escena a la que quieres ir (Ej: GameScene2, Map, etc).")]
    public string nextSceneName = "MainMenu";
    // ==========================================

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

    void OnEnable()
    {
        if (currentEnemy != null)
        {
            currentEnemy.OnHealthChanged += OnEnemyHealthChanged;
        }
    }

    void OnDisable()
    {
        if (currentEnemy != null)
        {
            currentEnemy.OnHealthChanged -= OnEnemyHealthChanged;
        }
    }

    private void OnEnemyHealthChanged(int currentHealth, int maxHealth)
    {
        // Si la vida llega a cero, disparamos la transición
        if (currentHealth <= 0)
        {
            DeadEnemy();
        }
    }

    #region Manejo de Enemigos
    // Comprueba si el enemigo actual está muerto y, si es así, lanza la transición de escena.
    public void DeadEnemy()
    {
        if (currentEnemy == null) return;

        // Usamos la propiedad Health del EnemyData si está disponible
        int currentHealth = currentEnemy.Health;

        if (currentHealth <= 0 && !isMoving && !isFinished)
        {
            isFinished = true;
            StartCoroutine(MoveSceneCoutine()); // Sigue moviendo el fondo como lo tenías planeado

            // NUEVO: Iniciamos la secuencia del tesoro de forma paralela
            StartCoroutine(VictorySequenceRoutine());
        }
    }

    [ContextMenu("Ejecutar Transición de Prueba")]
    public void OnEnemyDefeated()
    {
        if (!isMoving || !isFinished)
        {
            StartCoroutine(MoveSceneCoutine());
        }
    }
    #endregion

    #region Corutinas
    // Corrutina que maneja los tiempos del Panel de Tesoros y el SceneManager
    private IEnumerator VictorySequenceRoutine()
    {
        // 1. Esperamos unos segundos inmediatamente después de que la vida llega a 0
        yield return new WaitForSeconds(delayBeforeTreasure);

        // 2. Activamos el panel de tesoros
        if (treasurePanelCinematic != null)
        {
            // NUEVO: Le pasamos el clip de audio a la función
            treasurePanelCinematic.StartPresentationSequence(treasureAudioClip);

            // 3. Esperamos a que la presentación del tesoro termine completamente
            yield return new WaitForSeconds(treasureCinematicDuration);
        }
        else
        {
            Debug.LogWarning("¡Falta asignar el Panel de Tesoros en el StageTransition!");
        }

        // 4. Esperamos los últimos segundos de gracia antes de cambiar la pantalla
        yield return new WaitForSeconds(delayBeforeSceneChange);

        // 5. ¡Cambiamos de escena usando tu script SceneManager!
        if (customSceneManager != null)
        {
            customSceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("¡Falta asignar el SceneManager en el StageTransition!");
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
    #endregion
}