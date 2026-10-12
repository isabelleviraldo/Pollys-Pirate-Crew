using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;

    public HighScoreEntry(
        string name,
        int value
    )
    {
        playerName = name;
        score = value;
    }
}

public class HighScoreManager : MonoBehaviour
{
    public const int MaximumScores = 10;

    public List<HighScoreEntry> scores =
        new List<HighScoreEntry>();

    void Awake()
    {
        LoadScores();
    }

    public bool IsTopTen(int score)
    {
        if (scores.Count < MaximumScores)
        {
            return true;
        }

        return score > scores[scores.Count - 1].score;
    }

    public void AddScore(
        string playerName,
        int score
    )
    {
        scores.Add(
            new HighScoreEntry(
                playerName,
                score
            )
        );

        scores.Sort(
            (a, b) =>
                b.score.CompareTo(a.score)
        );

        if (scores.Count > MaximumScores)
        {
            scores.RemoveAt(
                scores.Count - 1
            );
        }

        SaveScores();
    }

    void SaveScores()
    {
        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetString(
                "HighScoreName_" + i,
                scores[i].playerName
            );

            PlayerPrefs.SetInt(
                "HighScoreValue_" + i,
                scores[i].score
            );
        }

        PlayerPrefs.SetInt(
            "HighScoreCount",
            scores.Count
        );

        PlayerPrefs.Save();
    }

    void LoadScores()
    {
        scores.Clear();

        int count =
            PlayerPrefs.GetInt(
                "HighScoreCount",
                0
            );

        for (int i = 0; i < count; i++)
        {
            string playerName =
                PlayerPrefs.GetString(
                    "HighScoreName_" + i,
                    "---"
                );

            int score =
                PlayerPrefs.GetInt(
                    "HighScoreValue_" + i,
                    0
                );

            scores.Add(
                new HighScoreEntry(
                    playerName,
                    score
                )
            );
        }
    }
}