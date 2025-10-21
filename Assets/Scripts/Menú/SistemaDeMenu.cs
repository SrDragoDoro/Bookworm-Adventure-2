using UnityEngine;
using UnityEngine.SceneManagement;
public class SistemanDeMenú : MonoBehaviour
{
    public void Opciones()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }


    public void Salir()
    {
        print("Salir del juego");
        Application.Quit();

    }

}
