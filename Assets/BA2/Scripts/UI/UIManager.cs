using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI EnemyName;

    public static Action<string> OnEnemyNameChange;

    private void OnEnable()
    {
        OnEnemyNameChange += UpdateEnemyName;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateEnemyName(string value)
    {
        EnemyName.text = value; 
    }
}
