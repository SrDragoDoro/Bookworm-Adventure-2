using System.Collections; // ¡Importante para usar Corrutinas!
using UnityEngine;

// 1. Definis los estados posibles de tu batalla
public enum GameState
{
    Intro,
    PlayerTurn,
    EnemyTurn,
    Victory
}

public class LevelController : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSequence prologueSequence;

    // NUEVO: Referencias para la cinemática del Boss
    [Header("Configuración de Cinemática")]
    [SerializeField] private CinematicPresentationController bossCinematic;
    [SerializeField] private AudioClip bossCinematicClip; // Opcional, por si quieres asignarle un sonido
    [Tooltip("Tiempo total que dura la cinemática antes de pasar al delay normal.")]
    [SerializeField] private float cinematicWaitTime = 3.5f;

    [Header("Configuración de Tiempos")]
    [SerializeField] private float delayBeforePlaying = 2.0f; // Segundos de pausa tras el diálogo (y la cinemática)

    // 2. Variables de estado
    public GameState currentState;
    public TableState TableState; // Referencia al TableState

    private bool introPlayed = false;
    private bool isWaitingToPlay = false; // Control para evitar iniciar la corrutina más de una vez

    void Start()
    {
        // Inicias el nivel en la fase Intro
        currentState = GameState.Intro;

        // Detenemos el tablero y avisamos a las losetas
        SetTableState(TableState.Stopped);
    }

    void Update()
    {
        // 3. Revisas si estás en la intro y aún no se ha jugado
        if (!introPlayed && currentState == GameState.Intro)
        {
            introPlayed = true;
            dialogueManager.StartDialogue(prologueSequence);
        }

        // 4. Cambiar de fase cuando el diálogo termine (y comprobamos que no estemos ya esperando)
        if (introPlayed && !dialogueManager.IsDialogueActive && currentState == GameState.Intro && !isWaitingToPlay)
        {
            // Iniciamos la cuenta regresiva antes de jugar
            StartCoroutine(TransitionToPlayerTurn());
        }

        // 5. Bloqueo de Gameplay (durante el diálogo O durante los segundos de espera)
        if (dialogueManager.IsDialogueActive || isWaitingToPlay)
        {
            return; // Bloquea inputs y mecánicas
        }

        // --- A partir de aquí va la lógica normal de turnos (cuando ya se está jugando) ---
    }

    // Corrutina para manejar la pausa de tiempo y la cinemática
    private IEnumerator TransitionToPlayerTurn()
    {
        isWaitingToPlay = true;

        // Mantenemos el estado detenido solo por seguridad
        SetTableState(TableState.Stopped);

        Debug.Log("El diálogo terminó. Iniciando cinemática del Boss...");

        // NUEVO: Disparamos la cinemática si está asignada
        if (bossCinematic != null)
        {
            bossCinematic.StartPresentationSequence(bossCinematicClip);

            // Esperamos a que la cinemática termine (Display Duration + Fade Out Duration en tu otro script)
            yield return new WaitForSeconds(cinematicWaitTime);
        }

        Debug.Log($"Cinemática terminada. Esperando {delayBeforePlaying} segundos de gracia...");

        // Esperamos los segundos extra especificados (opcional, puedes ponerlo en 0 si la cinemática ya es suficiente pausa)
        yield return new WaitForSeconds(delayBeforePlaying);

        // Ahora sí, pasamos al turno del jugador y activamos el tablero
        currentState = GameState.PlayerTurn;
        SetTableState(TableState.Playing);

        isWaitingToPlay = false; // Terminamos de esperar
        Debug.Log("¡A jugar! Turno de Lex.");
    }

    // Método centralizado para cambiar el estado del tablero
    private void SetTableState(TableState newState)
    {
        TableState = newState;
        // Llamamos al evento de tu TileController.cs para que todas las letras obedezcan
        TileController.OnGlobalTableStateChanged?.Invoke(newState);
    }
}