using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Battle Arena"); // Ganti dengan nama scene kamu
    }

    public void ExitGame()
    {
        Debug.Log("Keluar dari game...");
        Application.Quit(); // Tidak berfungsi di editor
    }
}
