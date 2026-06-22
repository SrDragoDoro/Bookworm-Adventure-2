using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public enum BattleState { PlayerTurn, EnemyTurn, Busy }

public class BattleManager : MonoBehaviour, IDamageDealer
{
    //public static BattleManager Instance;

    [Header("Estado del Combate")]
    public BattleState currentState;

    [Header("Componentes de Combate")]
    public CharacterCombat playerCombat;
    public CharacterCombat enemyCombat;

    [Header("Cinemática del Jefe")]
    public CinematicPresentationController bossIntroController;
    public AudioClip bossIntroMusic; // Tu archivo de audio .mp3 o .wav

    [Header("Cinemática del Tesoro")]
    public CinematicPresentationController treasureController;
    public AudioClip treasureSFX;

    

    void Awake()
    {
        //Instance = this;
    }

    void Start()
    {
        ChangeState(BattleState.PlayerTurn);      
    }

    #region Suscripción a eventos de salud
    void OnEnable()
    {
        // Subscribe to health change events if possible
        TrySubscribeToHealthEvents();
    }

    void OnDisable()
    {
        TryUnsubscribeFromHealthEvents();
    }

    private void TrySubscribeToHealthEvents()
    {
        if (playerCombat != null && playerCombat.healthController != null && playerCombat.healthController.Data != null)
        {
            playerCombat.healthController.Data.OnHealthChanged += OnAnyEntityHealthChanged;
        }

        if (enemyCombat != null && enemyCombat.healthController != null && enemyCombat.healthController.Data != null)
        {
            enemyCombat.healthController.Data.OnHealthChanged += OnAnyEntityHealthChanged;
        }
    }

    private void TryUnsubscribeFromHealthEvents()
    {
        if (playerCombat != null && playerCombat.healthController != null && playerCombat.healthController.Data != null)
        {
            playerCombat.healthController.Data.OnHealthChanged -= OnAnyEntityHealthChanged;
        }

        if (enemyCombat != null && enemyCombat.healthController != null && enemyCombat.healthController.Data != null)
        {
            enemyCombat.healthController.Data.OnHealthChanged -= OnAnyEntityHealthChanged;
        }
    }

    #endregion


    // Handler que se dispara cuando cualquiera de las entidades cambia de vida
    private void OnAnyEntityHealthChanged(int currentHealth, int maxHealth)
    {
        // Siempre alertamos al enemigo cuando cualquiera recibe daño
        if (enemyCombat == null || enemyCombat.healthController == null || enemyCombat.healthController.Data == null) return;

        var enemyData = enemyCombat.healthController.Data;

        // Si la vida del enemigo llegó a 0 -> Dead
        if (enemyData.Health <= 0)
        {
            // Para detener todas las losetas:
            TileController.OnGlobalTableStateChanged?.Invoke(TableState.Stopped);
            enemyData.EnemyState = EnemyState.Dead;
            enemyData.SetEnemyState(EnemyState.Dead); // <- Así de limpio
            Debug.Log($"[BattleManager] Enemy state -> Dead");
            return;
        }

        // Si cualquiera recibe daño pero el enemigo sigue vivo, lo ponemos en alerta
        enemyData.EnemyState = EnemyState.Alerted;
        enemyData.SetEnemyState(EnemyState.Alerted); // <- Así de limpio
        Debug.Log($"[BattleManager] Enemy state -> Alerted");
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


    public int GetFinalDamage(int baseDamage)
    {        

        return baseDamage; // Por ahora, no hay multiplicadores ni efectos adicionales, pero aquí es donde se aplicarían.
    }

}