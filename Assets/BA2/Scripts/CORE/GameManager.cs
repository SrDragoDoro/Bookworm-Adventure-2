using UnityEngine;

public class GameManager : MonoBehaviour
{      
    public static GameManager instance;
    //managers
    public DictionaryManager manager;
    //public SoundManager soundManager;
    public BattleManager battleManager;
    public UIManager uiManager;
    //
    public TileController tileController;
    public WordBuilderManager wordBuilderManager;

    //Data
    public CharacterData characterData;
    public EnemyData enemyData;
    public CharacterDataBase characterDataBase;
    public ModifiersDataBase modifiersData;



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
