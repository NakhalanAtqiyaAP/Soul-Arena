using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance { get; private set; } // ?? Tambahkan ini

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Hapus duplikat jika ada
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    public void ShowGameOver(string winner)
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        gameOverText.text = $"{winner} Wins!";
        Cursor.visible = true;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        ScoreManager.Instance.ResetScores();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ScoreManager.Instance.ResetScores();
        SceneManager.LoadScene("MainMenu");
    }
}
