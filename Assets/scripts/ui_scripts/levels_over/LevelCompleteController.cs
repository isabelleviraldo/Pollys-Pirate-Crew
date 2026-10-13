using System.Collections;
using TMPro;
using UnityEngine;

public class LevelCompleteController : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner;
    public GameObject levelCompletePanel;

    [Header("Optional Text")]
    public TMP_Text levelCompleteText;
    public TMP_Text rewardText;
    public TMP_Text nextLevelText;

    [Header("Automatic Continue")]
    public float timeBeforeNextLevel = 3f;

    private bool levelHasEnded;

    void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }
    
    void Update()
    {
        if (levelHasEnded)
        {
            return;
        }
    
        if (enemySpawner == null)
        {
            Debug.LogError(
                "LevelCompleteController is missing EnemySpawner."
            );
    
            return;
        }
    
        if (!enemySpawner.SpawningFinished)
        {
            return;
        }
    
        EnemyController[] remainingEnemies =
            FindObjectsOfType<EnemyController>();
    
        Debug.Log(
            "Spawner finished. Remaining enemies: " +
            remainingEnemies.Length
        );
    
        if (remainingEnemies.Length == 0)
        {
            FinishLevel();
        }
    }

    void FinishLevel()
    {
        levelHasEnded = true;

        if (GameFlowManager.Instance == null)
        {
            Debug.LogError(
                "GameFlowManager is missing."
            );

            return;
        }

        int completedRound =
            GameFlowManager.Instance.currentRound;

        GameFlowManager.Instance.CompleteLevel();

        if (levelCompleteText != null)
        {
            levelCompleteText.text =
                "Level " +
                completedRound +
                " Complete!";
        }

        if (rewardText != null)
        {
            rewardText.text =
                "Gold Earned: 1000";
        }

        if (nextLevelText != null)
        {
            nextLevelText.text =
                "Next Level Enemies: " +
                GameFlowManager.Instance.enemiesThisLevel;
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        StartCoroutine(
            AutomaticallyStartNextLevel()
        );
    }

    IEnumerator AutomaticallyStartNextLevel()
    {
        // Realtime continues even while Time.timeScale is zero.
        yield return new WaitForSecondsRealtime(
            timeBeforeNextLevel
        );

        GameFlowManager.Instance.ContinueToNextLevel();
    }
}