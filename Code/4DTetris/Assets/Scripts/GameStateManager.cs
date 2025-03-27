using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static HighScoreManager;

public class GameStateManager : MonoBehaviour
{

    public TextMeshProUGUI gameOverIndicator;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI rules;
    public TextMeshProUGUI highScores;
    public GameObject rulesPanel;
    private int score = 0;

    public bool gameOver = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1;
        gameOver = false;
        gameOverIndicator.gameObject.SetActive(false);

        rulesPanel = GameObject.Find("RulesPanel");

        List<HighScoreEntry> scores = HighScoreManager.Instance.GetHighScores();
        highScores.text = "High Scores:\n";
        if (scores.Count > 0)
        {
            int num = 1;
            foreach (HighScoreEntry entry in scores)
            {
                highScores.text += num.ToString() + ". " + entry.playerName + ": " + entry.score + "\n";
                num++;
            }
        }
        highScores.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();

        // rules are only there for 30s
        if (Time.timeSinceLevelLoad > 30)
        {
            rules.gameObject.SetActive(false);
            rulesPanel.SetActive(false);
            highScores.gameObject.SetActive(true);
        }
    }

    public void GameOver()
    {
        if (FindAnyObjectByType<AudioManager>() != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }
        gameOverIndicator.text = "Game Over! Your score was: " + score + "\n Press a to continue.";
        gameOverIndicator.gameObject.SetActive(true);
        if (FindAnyObjectByType<HighScoreManager>() != null)
            HighScoreManager.Instance.SubmitScore(score);
        Time.timeScale = 0;
        gameOver = true;
    }

    public void RestartGame()
    {
        // Reset time scale first
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Reset the board
        GameObject board = GameObject.Find("Board");
        if (board != null)
        {
            BoardState boardState = board.GetComponent<BoardState>();
            if (boardState != null)
            {
                boardState.ResetBoard();
            }
        }

        // Reset game state
        gameOver = false;
        score = 0;
        gameOverIndicator.gameObject.SetActive(false);

        // Spawn a new polynomino to start the game
        GameObject.Find("GameManager").GetComponent<PolyManager>().Init();
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public int GetScore()
    {
        return score;
    }


}
