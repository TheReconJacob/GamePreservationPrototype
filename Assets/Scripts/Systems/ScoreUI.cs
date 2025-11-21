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
    
    [Header("Game Events")]
    [Tooltip("Reference to the GameEvents ScriptableObject")]
    public GameEventSO gameEvents;
    
    void Start()
    {
        // Initialize display first (before subscribing to events)
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            UpdateScoreDisplay(gameManager.GetScore());
        }
        else
        {
            UpdateScoreDisplay(0);
        }
        
        // Subscribe to score changed event
        if (gameEvents != null)
        {
            gameEvents.OnScoreChanged.AddListener(OnScoreChanged);
            Debug.Log("ScoreUI subscribed to score changed events");
        }
        else
        {
            Debug.LogWarning("GameEvents ScriptableObject not assigned to ScoreUI!");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (gameEvents != null)
        {
            gameEvents.OnScoreChanged.RemoveListener(OnScoreChanged);
        }
    }
    
    // Event handler for score changes
    private void OnScoreChanged(int newScore)
    {
        Debug.Log($"[ScoreUI] Score changed event received: {newScore}");
        UpdateScoreDisplay(newScore);
        FlashScore();
    }
    
    private void UpdateScoreDisplay(int currentScore)
    {
        if (scoreText == null) return;
        
        scoreText.text = scorePrefix + currentScore.ToString();
        
        // Update high score
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
