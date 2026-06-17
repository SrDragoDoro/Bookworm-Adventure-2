using UnityEngine;

public class GameManager : MonoBehaviour
{      
    public static GameManager instance;
    public DictionaryManager manager;

    //DictionaryManager
    //SoundManager
    //BattleManager
    //

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    void Start()
    {
       
    }

    void Update()
    {
        
    }
}
