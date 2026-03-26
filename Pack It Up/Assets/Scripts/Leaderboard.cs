using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    // Assign these in the Unity Inspector
    public TMP_Text[] scoreTexts;
    public TMP_Text[] nameTexts;
    public TMP_InputField nameInputField;
    public Button submitButton;
    public GameObject inputFieldPanel;

    private int currentScore = 0;
    private const string SaveFileName = "highscores.json";
    private string saveFilePath;

    [System.Serializable]
    public class HighScoreEntry
    {
        public string name;
        public int score;
    }

    [System.Serializable]
    public class HighScoreList
    {
        public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    }

    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        inputFieldPanel.SetActive(false);
        LoadHighScores();
        DisplayHighScores();
    }

    public void SetCurrentScore(int score)
    {
        currentScore = score;
        if (IsHighScore(currentScore))
        {
            inputFieldPanel.SetActive(true);
            submitButton.onClick.AddListener(SubmitScore);
        }
    }

    private bool IsHighScore(int score)
    {
        // Check if the score is higher than the 10th (lowest) score on the board
        if (highScoresData.highScores.Count < 10)
        {
            return true;
        }
        return score > highScoresData.highScores.Min(x => x.score);
    }

    public void SubmitScore()
    {
        string playerName = nameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            AddNewHighScore(playerName, currentScore);
            inputFieldPanel.SetActive(false);
        }
    }

    private HighScoreList highScoresData = new HighScoreList();

    public void AddNewHighScore(string name, int score)
    {
        highScoresData.highScores.Add(new HighScoreEntry { name = name, score = score });
        highScoresData.highScores = highScoresData.highScores.OrderByDescending(x => x.score).Take(10).ToList();
        SaveHighScores();
        DisplayHighScores();
    }

    private void DisplayHighScores()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i < highScoresData.highScores.Count)
            {
                nameTexts[i].text = highScoresData.highScores[i].name;
                scoreTexts[i].text = highScoresData.highScores[i].score.ToString();
            }
            else
            {
                nameTexts[i].text = "---";
                scoreTexts[i].text = "0";
            }
        }
    }

    private void SaveHighScores()
    {
        string json = JsonUtility.ToJson(highScoresData, true);
        File.WriteAllText(saveFilePath, json);
    }

    private void LoadHighScores()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            highScoresData = JsonUtility.FromJson<HighScoreList>(json);
        }
        else
        {
            highScoresData = new HighScoreList();
        }
    }
}
