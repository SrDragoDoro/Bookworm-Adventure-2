using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    public EnemyData Data;
    public enum SpawnDirection { None, LeftToRight, RightToLeft }

    [Header("Datos y Referencias")]
    //public BaseStats stats;
    public GameObject heartPrefab;
    public Transform heartsContainer; // El objeto padre donde se crearán los corazones
    [SerializeField] private UnityEvent soundAction;


    [Header("Configuración de Cuadrícula")]
    public SpawnDirection spawnDirection = SpawnDirection.LeftToRight;
    public int maxHeartsPerRow = 10; // Número máximo de corazones por fila antes de saltar a la siguiente
    public float horizontalSpacing = 0.4f;
    public float verticalSpacing = -0.4f; // Negativo para que baje de fila

    private List<HeartUI> heartsList = new List<HeartUI>();

    void Start()
    {
        Data.Initialize(); 
        Data.OnHealthChanged += UpdateHeartsDisplay;
        InitializeHearts();
    }

    private void InitializeHearts()
    {
        UIManager.OnEnemyNameChange?.Invoke(Data.nameEntity);
        //maxHeartsPerRow = Data.maxHeartsPerRow;// Actualizamos el valor de maxHeartsPerRow según los datos del enemigo
        //stats.maxHealth = Data.maxHealth; // Actualizamos la salud máxima según los datos del enemigo

        // Limpiamos corazones previos si los hay
        foreach (Transform child in heartsContainer) Destroy(child.gameObject);
        heartsList.Clear();

        // Calculamos cuántos corazones enteros necesitamos como máximo
        int totalHearts = Mathf.CeilToInt((float)Data.maxHealth / 4f);

        for (int i = 0; i < totalHearts; i++)
        {
            // Calculamos fila y columna
            int row = i / maxHeartsPerRow;
            int column = i % maxHeartsPerRow;

            // Invertimos la columna si es de derecha a izquierda
            float xDir = (spawnDirection == SpawnDirection.LeftToRight) ? 1f : -1f;

            Vector3 spawnPos = heartsContainer.position + new Vector3(
                column * horizontalSpacing * xDir,
                row * verticalSpacing,
                0
            );

            GameObject newHeart = Instantiate(heartPrefab, spawnPos, Quaternion.identity, heartsContainer);
            HeartUI heartUI = newHeart.GetComponent<HeartUI>();

            // Forzamos a que inicie lleno
            heartUI.UpdatePieces(4);
            heartsList.Add(heartUI);
        }

        // Forzamos la primera actualización real para reflejar la vida exacta actual
        UpdateHeartsDisplay(Data.GetHealth(), Data.maxHealth);
    }

    private void UpdateHeartsDisplay(int currentHealth, int maxHealth)
    {
        int remainingHealth = currentHealth;

        for (int i = 0; i < heartsList.Count; i++)
        {
            int piecesForThisHeart = 0;

            if (remainingHealth >= 4)
            {
                piecesForThisHeart = 4;
                remainingHealth -= 4;
            }
            else if (remainingHealth > 0)
            {
                piecesForThisHeart = remainingHealth;
                remainingHealth = 0;
            }

            heartsList[i].UpdatePieces(piecesForThisHeart);
        }
    }


    // Método de prueba rápida desde un botón o evento
    [Button]
    public void TestDamage(int dmg) => Data.TakeDamage(dmg);
}