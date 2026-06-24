using System;
using UnityEngine;
using UnityEngine.Events;

public class HealingPotion : MonoBehaviour, IHealer
{
    [SerializeField] private int healAmount = 10; // 10 ints = 2.5 corazones
    [SerializeField] private UnityEvent soundAction;

    public int GetHealAmount()
    {
        return healAmount;
    }

    // Este método es nativo de Unity y se activa al hacer clic en el Collider2D del objeto
    private void OnMouseDown()
    {
        // 1. Buscamos a Lex en la escena usando su Tag
        GameObject lex = GameObject.FindGameObjectWithTag("Player");

        if (lex != null)
        {
            // 2. Obtenemos su controlador de vida
            HealthController healthObj = lex.GetComponent<HealthController>();

            if (healthObj != null)
            {
                // 3. Lo curamos
                healthObj.Data.Heal(GetHealAmount());

                soundAction?.Invoke();
                // TODO: Puedes agregar un AudioSource.PlayClipAtPoint o instanciar partículas aquí antes de destruir

                // 4. Destruimos la poción
                Destroy(gameObject, 1);
            }
            else
            {
                Debug.LogWarning("Se encontró a Lex, pero no tiene el componente HealthController.");
            }
        }
        else
        {
            Debug.LogWarning("No se encontró ningún objeto con el tag 'Player' en la escena.");
        }
    }
}