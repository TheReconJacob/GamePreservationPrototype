using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using CustomNetworkManager = NetworkManager;

public class GameManager : NetworkBehaviour
{
    [Header("Game State")]
    private NetworkVariable<int> networkScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int currentScore = 0;
    
    [Header("Game Events")]
    [Tooltip("Reference to the GameEvents ScriptableObject")]
    public GameEventSO gameEvents;
    
    [Header("Online Dependency")]
    public bool requireOnlineAuth = true;
    
    void Start()
    {
        // Subscribe to network events
        CustomNetworkManager.OnInternetRestored += OnInternetRestored;
        CustomNetworkManager.OnInternetLost += OnInternetLost;
        
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
        
        if (CustomNetworkManager.Instance.IsOfflineMode() && !HasOfflineSession())
        {
            Debug.LogError("Game accessed in offline mode without an offline session! Redirecting to login...");
            SceneManager.LoadScene("LoginScene");
            return;
        }
        
        if (requireOnlineAuth && !IsPlayerAuthenticated() && !CustomNetworkManager.Instance.IsOfflineMode())
        {
            Debug.LogError("Game accessed without authentication! Redirecting to login...");
            SceneManager.LoadScene("LoginScene");
            return;
        }
        
        // Check if in network multiplayer mode
        bool isNetworkMode = Unity.Netcode.NetworkManager.Singleton != null && 
                            (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer);
        
        if (!isNetworkMode)
        {
            // Check if this is a fresh game launch (not a transition from online)
            // LastSessionScore should ONLY persist during online→offline transitions in same session
            bool wasOnlineThisSession = PlayerPrefs.GetInt("WasOnlineThisSession", 0) == 1;
            bool justLoggedInAfterOffline = PlayerPrefs.GetInt("JustLoggedInAfterOffline", 0) == 1;
            
            if (CustomNetworkManager.Instance.IsOfflineMode() && wasOnlineThisSession)
            {
                // Offline mode after being online: Load previous score
                currentScore = PlayerPrefs.GetInt("LastSessionScore", 0);
                Debug.Log($"[GameManager] Offline mode (transitioned from online) - loaded previous score: {currentScore}");
            }
            else if (justLoggedInAfterOffline)
            {
                // Just logged in after playing offline: Load offline progress
                currentScore = PlayerPrefs.GetInt("LastSessionScore", 0);
                PlayerPrefs.SetInt("JustLoggedInAfterOffline", 0); // Clear flag
                PlayerPrefs.SetInt("WasOnlineThisSession", 1); // Now we're online
                PlayerPrefs.Save();
                Debug.Log($"[GameManager] Logged in after offline play - loaded previous score: {currentScore}");
            }
            else
            {
                // Fresh start (offline or online): Start at 0
                currentScore = 0;
                Debug.Log("[GameManager] Fresh start - score set to 0");
                
                // Mark as online session if we have internet
                if (!CustomNetworkManager.Instance.IsOfflineMode())
                {
                    PlayerPrefs.SetInt("WasOnlineThisSession", 1);
                    PlayerPrefs.Save();
                    Debug.Log("[GameManager] Marked session as online");
                }
            }
            
            // Broadcast the initial score to UI
            if (gameEvents != null)
            {
                gameEvents.RaiseScoreChanged(currentScore);
            }
        }
        
        Debug.Log("Game started! Score: " + currentScore);
        Debug.Log("Player authenticated as: " + PlayerPrefs.GetString("PlayerUsername", "None"));
    }
    
    private void OnApplicationQuit()
    {
        // Clear session flag so next launch is treated as fresh start
        PlayerPrefs.SetInt("WasOnlineThisSession", 0);
        PlayerPrefs.SetInt("LastSessionScore", 0);
        PlayerPrefs.Save();
        Debug.Log("[GameManager] Application quit - session flags cleared");
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Network multiplayer mode: Reset score to 0 for both host and client
        // Clear LastSessionScore to avoid confusion (multiplayer scores don't persist)
        currentScore = 0;
        PlayerPrefs.SetInt("LastSessionScore", 0);
        PlayerPrefs.Save();
        
        // Subscribe to network score changes on all clients
        networkScore.OnValueChanged += OnNetworkScoreChanged;
        
        // If server, initialize the NetworkVariable
        if (IsServer)
        {
            networkScore.Value = 0;
            Debug.Log("[GameManager] Server initialized - score reset to 0");
        }
        else
        {
            // Client: sync with server's NetworkVariable value
            currentScore = networkScore.Value;
            Debug.Log($"[GameManager] Client connected - score synced to {currentScore}");
        }
        
        // Broadcast initial score to UI
        if (gameEvents != null)
        {
            gameEvents.RaiseScoreChanged(currentScore);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from network events
        CustomNetworkManager.OnInternetRestored -= OnInternetRestored;
        CustomNetworkManager.OnInternetLost -= OnInternetLost;
        
        // Unsubscribe from game events
        if (gameEvents != null)
        {
            gameEvents.OnTargetDestroyed.RemoveListener(OnTargetDestroyed);
        }
        
        // Unsubscribe from network score changes
        networkScore.OnValueChanged -= OnNetworkScoreChanged;
        
        // DO NOT clear session flags here - they need to persist across scene transitions
        // (e.g., when redirecting to LoginScene and back)
        // Flags are only cleared in OnApplicationQuit()
    }
    
    /// <summary>
    /// Network score synchronization callback - updates local score when network score changes
    /// </summary>
    private void OnNetworkScoreChanged(int previousValue, int newValue)
    {
        currentScore = newValue;
        Debug.Log($"[GameManager] Network score updated: {previousValue} -> {newValue}");
        
        // Update UI
        if (gameEvents != null)
        {
            gameEvents.RaiseScoreChanged(currentScore);
        }
    }
    
    private void OnInternetRestored()
    {
        bool wasBypassedLogin = PlayerPrefs.GetInt("IsBypassedLogin", 0) == 1;
        
        if (!CustomNetworkManager.Instance.IsOfflineMode() && wasBypassedLogin)
        {
            Debug.Log($"Internet restored - user bypassed login, redirecting to login screen. Current score: {currentScore}");
            
            // Save score to both JSON and PlayerPrefs for persistence
            LocalSaveManager.Instance.SaveToJSON(currentScore);
            PlayerPrefs.SetInt("LastSessionScore", currentScore);
            
            // Set flag so score persists after login
            PlayerPrefs.SetInt("JustLoggedInAfterOffline", 1);
            PlayerPrefs.Save();
            
            Debug.Log($"[GameManager] Saved score {currentScore} before login redirect");
            
            SceneManager.LoadScene("LoginScene");
        }
        else if (!CustomNetworkManager.Instance.IsOfflineMode())
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
        // Check if in network mode
        bool isNetworkMode = Unity.Netcode.NetworkManager.Singleton != null && 
                            (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer);
        
        if (isNetworkMode)
        {
            // Network mode: Only server can update score
            if (IsServer)
            {
                networkScore.Value += points;
                currentScore = networkScore.Value;
                Debug.Log($"[GameManager] Server score updated: {currentScore}");
                
                // Trigger score changed event (ScoreUI will automatically update)
                if (gameEvents != null)
                {
                    gameEvents.RaiseScoreChanged(currentScore);
                }
            }
            else
            {
                // Client: Request server to add score via RPC
                RequestAddScoreServerRpc(points);
            }
        }
        else
        {
            // Single-player mode: Direct score update
            currentScore += points;
            Debug.Log($"Score updated! Current score: {currentScore}");
            
            // Save to PlayerPrefs for session persistence (online->offline continuity)
            PlayerPrefs.SetInt("LastSessionScore", currentScore);
            PlayerPrefs.Save();
            
            // Save to local and cloud storage
            LocalSaveManager.Instance.SaveToJSON(currentScore);
            PlayFabSaveManager.Instance.SaveScoreToCloud(currentScore);
            
            // Trigger score changed event (ScoreUI will automatically update and flash)
            if (gameEvents != null)
            {
                gameEvents.RaiseScoreChanged(currentScore);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestAddScoreServerRpc(int points)
    {
        // Server adds score when requested by any client
        networkScore.Value += points;
        currentScore = networkScore.Value;
        Debug.Log($"[GameManager] Server score updated via RPC: {currentScore}");
        
        // Trigger score changed event
        if (gameEvents != null)
        {
            gameEvents.RaiseScoreChanged(currentScore);
        }
    }
    
    /// <summary>
    /// Request target destruction synchronized across network
    /// </summary>
    public void RequestTargetDestruction(string targetPath, int points)
    {
        if (IsServer)
        {
            // Server: Destroy immediately and notify clients
            DestroyTargetByPath(targetPath, points);
            DestroyTargetClientRpc(targetPath);
        }
        else
        {
            // Client: Request server to destroy
            RequestTargetDestructionServerRpc(targetPath, points);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestTargetDestructionServerRpc(string targetPath, int points)
    {
        Debug.Log($"[GameManager] Server received target destruction request: {targetPath}");
        DestroyTargetByPath(targetPath, points);
        DestroyTargetClientRpc(targetPath);
    }
    
    [ClientRpc]
    private void DestroyTargetClientRpc(string targetPath)
    {
        if (!IsServer)
        {
            DestroyTargetByPath(targetPath, 0); // Points already handled on server
        }
    }
    
    /// <summary>
    /// Find and destroy target by scene path
    /// </summary>
    private void DestroyTargetByPath(string targetPath, int points)
    {
        // Find target by reconstructing its path
        GameObject targetObj = GameObject.Find(targetPath);
        
        if (targetObj != null)
        {
            Target target = targetObj.GetComponent<Target>();
            if (target != null)
            {
                // Add score only on server
                if (IsServer && points > 0)
                {
                    AddScore(points);
                }
                
                target.DestroyTarget();
                Debug.Log($"[GameManager] Destroyed target: {targetPath}");
                return;
            }
        }
        
        Debug.LogWarning($"[GameManager] Could not find target: {targetPath}");
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
