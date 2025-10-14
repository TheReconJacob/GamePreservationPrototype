using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int currentScore = 0;
    
    [Header("Online Dependency")]
    public bool requireOnlineAuth = true;
    
    void Start()
    {
        if (requireOnlineAuth && !IsPlayerAuthenticated())
        {
            Debug.LogError("Game accessed without authentication! Redirecting to login...");
            SceneManager.LoadScene("LoginScene");
            return;
        }
        
        Debug.Log("Game started! Score: " + currentScore);
        Debug.Log("Player authenticated as: " + PlayerPrefs.GetString("PlayerUsername", "None"));
    }
    
    private bool IsPlayerAuthenticated()
    {
        int isAuth = PlayerPrefs.GetInt("IsAuthenticated", 0);
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        
        return isAuth == 1 && !string.IsNullOrEmpty(authToken);
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"Score updated! Current score: {currentScore}");
        
        var scoreUI = FindObjectOfType<ScoreUI>();
        if (scoreUI != null)
        {
            scoreUI.FlashScore();
        }
        
        PlayFabSaveManager.Instance.SaveScoreToCloud(currentScore);
    }
    
    public int GetScore()
    {
        return currentScore;
    }
    
    public void OnCloudSaveComplete(bool success)
    {
        if (success)
        {
            Debug.Log("Cloud save successful - game can continue");
        }
        else
        {
            Debug.LogError("CRITICAL: Cloud save failed - game will be blocked");
        }
    }
    
    public void SetScoreFromCloud(int cloudScore)
    {
        currentScore = cloudScore;
        Debug.Log($"Score loaded from cloud: {currentScore}");
    }
}
