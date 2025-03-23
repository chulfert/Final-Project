using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{

    public TextMeshProUGUI gameOverIndicator;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI rules;
    public GameObject rulesPanel;
    private int score = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOverIndicator.gameObject.SetActive(false);
        rulesPanel = GameObject.Find("RulesPanel");
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
        }
    }

    public void GameOver()
    {
        gameOverIndicator.text = "Game Over! Your score was: " + score;
        gameOverIndicator.gameObject.SetActive(true);
        Time.timeScale = 0;
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
