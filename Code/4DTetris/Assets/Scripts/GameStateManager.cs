using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public TextMeshProUGUI gameOverIndicator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOverIndicator.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameOver()
    {
        //find the UI
        gameOverIndicator.gameObject.SetActive(true);
        //stop the game
        Time.timeScale = 0;


    }
}
