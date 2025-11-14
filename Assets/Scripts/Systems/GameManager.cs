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
        NetworkManager.OnInternetRestored += OnInternetRestored;
        
        if (NetworkManager.Instance.IsOfflineMode() && !HasOfflineSession())
        {
            Debug.LogError("Game accessed in offline mode without an offline session! Redirecting to login...");
            SceneManager.LoadScene("LoginScene");
            return;
        }
        
        if (requireOnlineAuth && !IsPlayerAuthenticated() && !NetworkManager.Instance.IsOfflineMode())
        {
            Debug.LogError("Game accessed without authentication! Redirecting to login...");
            SceneManager.LoadScene("LoginScene");
            return;
        }
        
        int savedOfflineScore = PlayerPrefs.GetInt("OfflineScore", 0);
        if (savedOfflineScore > 0)
        {
            currentScore = savedOfflineScore;
            PlayerPrefs.DeleteKey("OfflineScore");
            Debug.Log($"Restored score from offline session: {currentScore}");
        }
        
        Debug.Log("Game started! Score: " + currentScore);
        Debug.Log("Player authenticated as: " + PlayerPrefs.GetString("PlayerUsername", "None"));
    }
    
    private void OnDestroy()
    {
        NetworkManager.OnInternetRestored -= OnInternetRestored;
    }
    
    private void OnInternetRestored()
    {
        bool wasBypassedLogin = PlayerPrefs.GetInt("IsBypassedLogin", 0) == 1;
        
        if (!NetworkManager.Instance.IsOfflineMode() && wasBypassedLogin)
        {
            Debug.Log("Internet restored - user bypassed login, redirecting to login screen");
            PlayerPrefs.SetInt("OfflineScore", currentScore);
            SceneManager.LoadScene("LoginScene");
        }
        else if (!NetworkManager.Instance.IsOfflineMode())
        {
            Debug.Log("Internet restored - resuming online gameplay with existing authentication");
        }
    }
    
    private bool IsPlayerAuthenticated()
    {
        int isAuth = PlayerPrefs.GetInt("IsAuthenticated", 0);
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        
        return isAuth == 1 && !string.IsNullOrEmpty(authToken);
    }
    
    private bool HasOfflineSession()
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
