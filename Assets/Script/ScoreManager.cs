using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private Text scoreText;
    private int player1Score = 0;
    private int player2Score = 0;
    private const int winningScore = 4;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the score text in the new scene
        var scoreTextObj = GameObject.FindGameObjectWithTag("ScoreText");
        if (scoreTextObj != null)
        {
            scoreText = scoreTextObj.GetComponent<Text>();
            UpdateScoreUI();
        }

        // Unpause game when new scene loads
        Time.timeScale = 1f;
    }

    public void AddScore(bool isPlayer1)
    {
        if (isPlayer1)
            player1Score++;
        else
            player2Score++;

        UpdateScoreUI();

        if (CheckGameOver())
        {
            FindObjectOfType<GameOverController>().ShowGameOver(GetWinner());
        }
        else
        {
            // Reload scene after delay if not game over
            Invoke("ReloadScene", 2f);
        }
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool CheckGameOver()
    {
        return player1Score >= winningScore || player2Score >= winningScore;
    }

    public string GetWinner()
    {
        if (player1Score >= winningScore) return "Player 1";
        if (player2Score >= winningScore) return "Player 2";
        return "";
    }

    public void ResetScores()
    {
        player1Score = 0;
        player2Score = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"P1: {player1Score} - P2: {player2Score}";
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}