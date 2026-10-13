using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string gameplaySceneName = "Game";

    [Header("Menu Panels")]
    public GameObject highScorePanel;

    [Header("Game Progress")]
    public int currentRound = 1;
    public int totalGold = 0;

    [Header("Enemy Difficulty")]
    public int startingEnemiesPerLevel = 5;
    public int enemiesThisLevel = 5;
    public int additionalEnemiesPerLevel = 3;

    public bool gameOver { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;

        currentRound = 1;
        totalGold = 0;
        enemiesThisLevel = startingEnemiesPerLevel;
        gameOver = false;

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void CompleteLevel()
    {
        int completedRound = currentRound;
    
        AddGold(1000);
    
        currentRound++;
        enemiesThisLevel += additionalEnemiesPerLevel;
    
        Debug.Log(
            "Level " + completedRound +
            " complete. Total gold: " +
            totalGold
        );
    }

    public void ContinueToNextLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenHighScorePanel()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(true);
        }
    }

    public void CloseHighScorePanel()
    {
        if (highScorePanel != null)
        {
            highScorePanel.SetActive(false);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void AddGold(int amount)
    {
        totalGold += amount;
    }

    public void TriggerGameOver()
    {
        if (gameOver)
        {
            return;
        }
    
        gameOver = true;
    }
}