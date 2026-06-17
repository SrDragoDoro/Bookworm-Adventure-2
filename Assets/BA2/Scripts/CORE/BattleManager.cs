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
            // Al volver al turno del jugador, evaluamos la palabra en el tablero para activar el botón
            WordBuilderManager.Instance.EvaluateCurrentWord();//Aplicar singleton
            GameManager.instance.wordBuilderManager.ScrambleAllTiles();
        }
        else
        {
            // Bloquea por completo el botón de ataque si es turno enemigo o si hay animaciones corriendo
            WordBuilderManager.Instance.attackButtonUI.interactable = false;
            // También bloquea el botón de revolver el tablero

        }

        if (currentState == BattleState.EnemyTurn)
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        // Pausa dramática antes de que Stuffy empiece a moverse/atacar
        yield return new WaitForSeconds(1.2f);

        // Ordena a Stuffy que ejecute su lógica de ataque
        if (enemyCombat != null)
        {
            enemyCombat.PerformAttack();
        }
    }
}