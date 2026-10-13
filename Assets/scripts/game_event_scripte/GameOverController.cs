using System.Collections;
using TMPro;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    public BoatDamageController boatController;
    public GameObject gameOverPanel;
    public TMP_Text finalGoldText;

    [Header("Crew Members")]
    public CrewCombatant[] crewMembers = new CrewCombatant[3];

    [Header("Timing")]
    public float timeBeforeMainMenu = 3f;

    private bool gameOverStarted;

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (gameOverStarted)
        {
            return;
        }

        if (GameFlowManager.Instance == null)
        {
            Debug.LogError(
                "GameFlowManager was not loaded."
            );

            return;
        }

        bool boatDestroyed =
            boatController != null &&
            boatController.CurrentBoatHealth <= 0;

        bool allCrewDead =
            AreAllCrewMembersDead();

        if (boatDestroyed || allCrewDead)
        {
            StartGameOver();
        }
    }

    bool AreAllCrewMembersDead()
    {
        if (crewMembers == null ||
            crewMembers.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < crewMembers.Length; i++)
        {
            if (crewMembers[i] == null)
            {
                return false;
            }

            if (!crewMembers[i].IsDead)
            {
                return false;
            }
        }

        return true;
    }

    void StartGameOver()
    {
        gameOverStarted = true;

        GameFlowManager.Instance.TriggerGameOver();

        if (finalGoldText != null)
        {
            finalGoldText.text =
                "Final Gold: " +
                GameFlowManager.Instance.totalGold;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        StartCoroutine(
            ReturnToMainMenuAfterDelay()
        );
    }

    IEnumerator ReturnToMainMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(
            timeBeforeMainMenu
        );

        GameFlowManager.Instance.ReturnToMainMenu();
    }
}