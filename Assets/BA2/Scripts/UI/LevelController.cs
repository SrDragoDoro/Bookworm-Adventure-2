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

    // 2. Creas una variable para saber en qué estado estás ahora
    public GameState currentState;

    private bool introPlayed = false;

    void Start()
    {
        // Inicias el nivel en la fase Intro
        currentState = GameState.Intro;
    }

    void Update()
    {
        // 3. Revisas si estás en la intro y aún no se ha jugado
        if (!introPlayed && currentState == GameState.Intro)
        {
            introPlayed = true;
            dialogueManager.StartDialogue(prologueSequence);
        }

        // 4. Cambiar de fase cuando el diálogo termine
        if (introPlayed && !dialogueManager.IsDialogueActive && currentState == GameState.Intro)
        {
            // El diálogo terminó, pasamos al turno del jugador
            currentState = GameState.PlayerTurn;
            Debug.Log("¡A jugar! Turno de Lex.");
        }

        if (dialogueManager.IsDialogueActive)
        {
            return; // Bloquea inputs mientras hablan
        }

        // Lógica de turnos...
    }
}