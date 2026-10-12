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
        currentRound = 1;
        totalGold = 0;

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
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void AddGold(int amount)
    {
        totalGold += amount;
    }
}