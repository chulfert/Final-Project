
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;


public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance { get; private set; }
    private List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    private string saveFilePath;

    // Data structure to store score entries
    [System.Serializable]
    public class HighScoreEntry
    {
        public string playerName;
        public int score;
        public System.DateTime date;

        public HighScoreEntry(string name, int score)
        {
            this.playerName = name;
            this.score = score;
            this.date = System.DateTime.Now;
        }
    }

    [System.Serializable]
    private class HighScoreData
    {
        public List<HighScoreEntry> scores;

        public HighScoreData(List<HighScoreEntry> scores)
        {
            this.scores = scores;
        }
    }


    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Set up file path
            saveFilePath = Path.Combine(Application.persistentDataPath, "highscores.json");

            // Load scores when game starts
            LoadHighScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsHighScore(int score)
    {
        // If we have fewer than max scores, any score qualifies
        if (highScores.Count < 5)
            return true;

        // Otherwise, check if score is higher than the lowest current high score
        return score > highScores.Min(entry => entry.score);
    } 

    public bool AddHighScore(string playerName, int score)
    {
        if (!IsHighScore(score))
            return false;

        // Create new entry
        HighScoreEntry newEntry = new HighScoreEntry(playerName, score);
        highScores.Add(newEntry);

        // Sort scores (highest first)
        highScores = highScores.OrderByDescending(entry => entry.score).ToList();

        // Keep only the top scores
        if (highScores.Count > 5)
            highScores = highScores.Take(5).ToList();

        // Save to file
        SaveHighScores();

        return true;
    }

    public List<HighScoreEntry> GetHighScores()
    {
        return new List<HighScoreEntry>(highScores); // Return a copy
    }

    public void LoadHighScores()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                HighScoreData data = JsonUtility.FromJson<HighScoreData>(json);
                highScores = data.scores;

                // Sort scores in case the file was tampered with
                highScores = highScores.OrderByDescending(entry => entry.score).ToList();

                Debug.Log("High scores loaded successfully");
            }
            else
            {
                Debug.Log("No high score file found, starting with empty list");
                highScores = new List<HighScoreEntry>();
                foreach (HighScoreEntry entry in highScores)
                {
                    Debug.Log(entry.playerName + " " + entry.score);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading high scores: " + e.Message);
            highScores = new List<HighScoreEntry>();
        }
    }
    public void SaveHighScores()
    {
        try
        {
            HighScoreData data = new HighScoreData(highScores);
            string json = JsonUtility.ToJson(data, true); // Pretty print
            File.WriteAllText(saveFilePath, json);
            Debug.Log("High scores saved to: " + saveFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving high scores: " + e.Message);
        }
    }

    public void SubmitScore(int score)
    {
        if (IsHighScore(score))
        {
            string playerName = "Player"; 
            AddHighScore(playerName, score);
        }
    }

    // Clear all high scores (for testing)
    public void ClearHighScores()
    {
        highScores.Clear();
        SaveHighScores();
        Debug.Log("High scores cleared");
    }
}
