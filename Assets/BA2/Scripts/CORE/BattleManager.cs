using System.Collections;
using UnityEngine;

public enum BattleState { PlayerTurn, EnemyTurn, Busy }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Estado del Combate")]
    public BattleState currentState;

    [Header("Componentes de Combate")]
    public CharacterCombat playerCombat;
    public CharacterCombat enemyCombat;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ChangeState(BattleState.PlayerTurn);
    }

    public void ChangeState(BattleState newState)
    {
        currentState = newState;

        if (currentState == BattleState.PlayerTurn)
        {
            // Al volver al turno del jugador, evaluamos la palabra en tablero para activar/desactivar el botón
            WordBuilderManager.Instance.EvaluateCurrentWord();
        }
        else
        {
            // Bloquea por completo el botón de ataque si es turno enemigo o si hay animaciones corriendo
            WordBuilderManager.Instance.attackButtonUI.interactable = false;
        }

        if (currentState == BattleState.EnemyTurn)
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1.2f); // Pausa dramática antes de que Sfuffy ataque
        enemyCombat.PerformAttack();
    }
}