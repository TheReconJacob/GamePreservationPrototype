using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;

public class PlayFabSaveManager : MonoBehaviour
{
    [Header("Cloud Save Settings")]
    public bool requireCloudSaveSuccess = true;
    public float saveTimeout = 10f;
    
    private static PlayFabSaveManager instance;
    public static PlayFabSaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayFabSaveManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PlayFabSaveManager");
                    instance = go.AddComponent<PlayFabSaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveScoreToCloud(int score)
    {
        Debug.Log($"Attempting to save score to cloud: {score}");
        
        if (NetworkManager.Instance.IsOfflineMode())
        {
            Debug.Log("Offline mode - skipping cloud save");
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnCloudSaveComplete(true);
            }
            return;
        }
        
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogError("Cannot save to cloud - user not logged in!");
            if (requireCloudSaveSuccess)
            {
                OnCloudSaveFailed("User not authenticated");
            }
            return;
        }
        
        var dataToSave = new Dictionary<string, string>
        {
            { "PlayerScore", score.ToString() },
            { "LastSaveTime", System.DateTime.UtcNow.ToString() },
            { "GameSession", System.Guid.NewGuid().ToString() }
        };
        
        var request = new UpdateUserDataRequest
        {
            Data = dataToSave,
            Permission = UserDataPermission.Public
        };
        
        PlayFabClientAPI.UpdateUserData(request, OnCloudSaveSuccess, OnCloudSaveError);
    }
    
    private void OnCloudSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Score successfully saved to cloud!");
        Debug.Log($"Data version: {result.DataVersion}");
        
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnCloudSaveComplete(true);
        }
    }
    
    private void OnCloudSaveError(PlayFabError error)
    {
        Debug.LogError($"Cloud save failed: {error.GenerateErrorReport()}");
        
        if (requireCloudSaveSuccess)
        {
            OnCloudSaveFailed(error.ErrorMessage);
        }
        
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnCloudSaveComplete(false);
        }
    }
    
    private void OnCloudSaveFailed(string errorMessage)
    {
        Debug.LogError($"CRITICAL: Cloud save required but failed - {errorMessage}");

        ShowCloudSaveError(errorMessage);
        BlockGameplay();
    }
    
    private void BlockGameplay()
    {
        Debug.LogError("BLOCKING GAMEPLAY: Cloud save failed and is required to continue");
        
        var player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.enabled = false;
        }
        
        var shooting = FindObjectOfType<Shooting>();
        if (shooting != null)
        {
            shooting.enabled = false;
        }
        
        Time.timeScale = 0f;
        
        StartCoroutine(ShowCriticalError());
    }
    
    private System.Collections.IEnumerator ShowCriticalError()
    {
        yield return new WaitForSecondsRealtime(2f);
        
        Debug.LogError("GAME BLOCKED: Cannot continue without cloud save functionality");
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
    
    private void ShowCloudSaveError(string errorMessage)
    {
        var statusTexts = FindObjectsOfType<TMPro.TextMeshProUGUI>();
        foreach (var text in statusTexts)
        {
            if (text.name.Contains("Status") || text.name.Contains("Error"))
            {
                text.text = $"GAME ERROR: Cloud save failed - {errorMessage}\nGame cannot continue without cloud save.";
                text.color = Color.red;
                break;
            }
        }
    }
    
    public void LoadScoreFromCloud()
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogError("Cannot load from cloud - user not logged in!");
            return;
        }
        
        var request = new GetUserDataRequest
        {
            Keys = new List<string> { "PlayerScore", "LastSaveTime" }
        };
        
        PlayFabClientAPI.GetUserData(request, OnCloudLoadSuccess, OnCloudLoadError);
    }
    
    private void OnCloudLoadSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("PlayerScore"))
        {
            int cloudScore = int.Parse(result.Data["PlayerScore"].Value);
            Debug.Log($"Loaded score from cloud: {cloudScore}");
            
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.SetScoreFromCloud(cloudScore);
            }
        }
    }
    
    private void OnCloudLoadError(PlayFabError error)
    {
        Debug.LogError($"Failed to load data from cloud: {error.GenerateErrorReport()}");
    }
}
