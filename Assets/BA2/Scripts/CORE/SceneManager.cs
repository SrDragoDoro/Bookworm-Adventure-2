using System;
using System.Collections;
using UnityEngine; // Quité Unity.VectorGraphics si no lo usas para limpiar un poco

public enum typeScene
{
    MainMenu,
    GameScene,
    Settings,
    Credits
}

public class SceneManager : MonoBehaviour
{
    [Header("Ajustes de cambio de escenas por tiempo")]
    [SerializeField] typeScene scene = typeScene.MainMenu;
    [SerializeField] private float timeToChangeScene;
    [SerializeField] private string nextSceneName;


    // NUEVO: Parámetro para definir si espera a que otro script lo inicie
    [SerializeField] private bool esperarLlamadaExterna = false;

    void Start()
    {
        // Si NO está esperando una llamada externa, inicia normalmente
        if (!esperarLlamadaExterna)
        {
            ActivateScene();
        }
    }

    #region Control de Escenas por botones
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }
    #endregion

    #region Control de Escenas por tiempo
    private void ActivateScene()
    {
        if (scene == typeScene.GameScene)
        {
            StartCoroutine(ChangeSceneToTutorial());
        }
    }

    // NUEVO: Método público para que LevelController pueda iniciar el contador
    public void ActivarContador()
    {
        if (scene == typeScene.GameScene)
        {
            StartCoroutine(ChangeSceneToTutorial());
        }
    }

    private IEnumerator ChangeSceneToTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(timeToChangeScene);
        yield return waitForSeconds;
        LoadScene(nextSceneName);
    }
    #endregion
}