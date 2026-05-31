using UnityEngine;

public class SceneManager : MonoBehaviour
{

    void Start()
    {
        
    }

   
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }

}
