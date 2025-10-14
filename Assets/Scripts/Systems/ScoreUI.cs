using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    
    [Header("Display Settings")]
    public string scorePrefix = "Score: ";
    public string highScorePrefix = "Best: ";
    
    private GameManager gameManager;
    private int lastDisplayedScore = -1;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        UpdateScoreDisplay();
    }
    
    void Update()
    {
        if (gameManager != null && scoreText != null)
        {
            int currentScore = gameManager.GetScore();
            if (currentScore != lastDisplayedScore)
            {
                UpdateScoreDisplay();
                lastDisplayedScore = currentScore;
            }
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (gameManager == null || scoreText == null) return;
        
        int currentScore = gameManager.GetScore();
        scoreText.text = scorePrefix + currentScore.ToString();
        
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            highScore = currentScore;
        }
        
        if (highScoreText != null)
        {
            highScoreText.text = highScorePrefix + highScore.ToString();
        }
    }
    
    public void FlashScore()
    {
        if (scoreText != null)
        {
            StartCoroutine(FlashEffect());
        }
    }
    
    private System.Collections.IEnumerator FlashEffect()
    {
        Color originalColor = scoreText.color;
        scoreText.color = Color.green;
        yield return new WaitForSeconds(0.1f);
        scoreText.color = originalColor;
    }
}
