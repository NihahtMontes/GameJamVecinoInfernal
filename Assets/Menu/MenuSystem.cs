using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    // Asigna este método al botón "Jugar" en el Inspector (OnClick)
    public void StartGame()
    {
        SceneManager.LoadScene("Main");
    }

    // Asigna este método al botón "Salir" en el Inspector (OnClick)
    public void ExitGame()
    {
        Debug.Log("[Menu] Saliendo del juego...");
        Application.Quit();
    }
}
