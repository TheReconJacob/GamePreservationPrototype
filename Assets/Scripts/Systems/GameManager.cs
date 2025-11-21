using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int currentScore = 0;
    
    [Header("Game Events")]
    [Tooltip("Reference to the GameEvents ScriptableObject")]
    public GameEventSO gameEvents;
    
    [Header("Online Dependency")]
    public bool requireOnlineAuth = true;
    
    void Start()
    {
        // Subscribe to network events
        NetworkManager.OnInternetRestored += OnInternetRestored;
        NetworkManager.OnInternetLost += OnInternetLost;
        
        // Subscribe to game events
        if (gameEvents != null)
        {
            gameEvents.OnTargetDestroyed.AddListener(OnTargetDestroyed);
            Debug.Log("GameManager subscribed to game events");
        }
        else
        {
            Debug.LogWarning("GameEvents ScriptableObject not assigned to GameManager!");
        }
        
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
        
        SaveGameData saveData = LocalSaveManager.Instance.LoadFromJSON();
        if (saveData != null)
        {
            currentScore = saveData.score;
            Debug.Log($"Restored score from local JSON save: {currentScore}");
        }
        else
        {
            int savedOfflineScore = PlayerPrefs.GetInt("OfflineScore", 0);
            if (savedOfflineScore > 0)
            {
                currentScore = savedOfflineScore;
                PlayerPrefs.DeleteKey("OfflineScore");
                Debug.Log($"Restored score from legacy offline session: {currentScore}");
                LocalSaveManager.Instance.SaveToJSON(currentScore);
            }
        }
        
        // Broadcast the loaded score to UI immediately
        if (gameEvents != null && currentScore > 0)
        {
            gameEvents.RaiseScoreChanged(currentScore);
        }
        
        Debug.Log("Game started! Score: " + currentScore);
        Debug.Log("Player authenticated as: " + PlayerPrefs.GetString("PlayerUsername", "None"));
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from network events
        NetworkManager.OnInternetRestored -= OnInternetRestored;
        NetworkManager.OnInternetLost -= OnInternetLost;
        
        // Unsubscribe from game events
        if (gameEvents != null)
        {
            gameEvents.OnTargetDestroyed.RemoveListener(OnTargetDestroyed);
        }
    }
    
    private void OnInternetRestored()
    {
        bool wasBypassedLogin = PlayerPrefs.GetInt("IsBypassedLogin", 0) == 1;
        
        if (!NetworkManager.Instance.IsOfflineMode() && wasBypassedLogin)
        {
            Debug.Log("Internet restored - user bypassed login, redirecting to login screen");
            LocalSaveManager.Instance.SaveToJSON(currentScore);
            SceneManager.LoadScene("LoginScene");
        }
        else if (!NetworkManager.Instance.IsOfflineMode())
        {
            Debug.Log("Internet restored - resuming online gameplay with existing authentication");
        }
    }
    
    private void OnInternetLost()
    {
        Debug.Log("Connection lost - attempting to migrate cloud saves to local storage");
        
        // Only migrate from cloud if we're not in a bypassed login session
        bool wasBypassedLogin = PlayerPrefs.GetInt("IsBypassedLogin", 0) == 1;
        if (!wasBypassedLogin)
        {
            // Load latest score from cloud and save to local JSON
            PlayFabSaveManager.Instance.LoadScoreFromCloud();
        }
        else
        {
            // Already playing offline with local save
            Debug.Log("Already in offline bypass mode - local saves will continue");
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
    
    // Event handler for target destruction
    private void OnTargetDestroyed(int pointValue)
    {
        Debug.Log($"[GameManager] Target destroyed event received with {pointValue} points");
        AddScore(pointValue);
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"Score updated! Current score: {currentScore}");
        
        // Save to local and cloud storage
        LocalSaveManager.Instance.SaveToJSON(currentScore);
        PlayFabSaveManager.Instance.SaveScoreToCloud(currentScore);
        
        // Trigger score changed event (ScoreUI will automatically update and flash)
        if (gameEvents != null)
        {
            gameEvents.RaiseScoreChanged(currentScore);
        }
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
