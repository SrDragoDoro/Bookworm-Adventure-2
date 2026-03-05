/*
Este te permite iniciar el juego, es decir, 
cargar la escena del juego. Para esto, debes 
asegurarte de que la escena del juego esté 
incluida en la configuración de escenas de tu 
proyecto. Luego, puedes usar el siguiente 
código para cargar la escena del juego cuando 
se haga clic en un botón :D 
*/

using UnityEngine;
using UnityEngine.SceneManagement; // Asegúrate de incluir esta línea para usar SceneManager

public class Jugar : MonoBehaviour
{ 
    public void IniciarJuego(string nombreEscena)
    {
        // Reemplaza "NombreDeTuEscena" con el nombre de la escena que deseas cargar
        SceneManager.LoadScene(nombreEscena);
    }
}
