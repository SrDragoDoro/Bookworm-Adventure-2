using System;
using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;

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


    void Start()
    {
        ActivateScene();
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


    private IEnumerator ChangeSceneToTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(timeToChangeScene);
        yield return waitForSeconds;
        LoadScene("Tutorial");
    }
    #endregion

}
