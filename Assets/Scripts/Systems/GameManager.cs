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
    }
    
    public int GetScore()
    {
        return currentScore;
    }
}
