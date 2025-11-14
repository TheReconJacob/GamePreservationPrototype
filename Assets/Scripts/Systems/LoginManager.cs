using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI statusText;
    
    [Header("Scene Management")]
    public string gameSceneName = "GameScene";
    
    [Header("Simulated Online Dependency")]
    public bool simulateOnlineOnly = true;
    public float loginDelay = 2f;
    
    [Header("PlayFab Configuration")]
    public string playfabTitleId = "1042B5";
    
    private bool isOfflineModeActive = false;
    private Canvas loginCanvas;
    
    private void Start()
    {
        loginCanvas = GetComponentInParent<Canvas>();
        if (loginCanvas == null)
        {
            loginCanvas = GetComponent<Canvas>();
        }
        
        NetworkManager.OnInternetRestored += OnInternetRestored;
        
        if (!NetworkManager.Instance.HasInternetConnection())
        {
            Debug.Log("Started without internet connection - bypassing login");
            BypassAuthentication();
            return;
        }
        
        InitializeLoginUI();
        Debug.Log("Login screen loaded - game access blocked until authentication");
    }
    
    private void OnDestroy()
    {
        NetworkManager.OnInternetRestored -= OnInternetRestored;
    }
    
    private void InitializeLoginUI()
    {
        statusText.text = "Please login to continue";
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        PlayFabSettings.staticSettings.TitleId = playfabTitleId;
        isOfflineModeActive = false;
    }
    
    private void OnInternetRestored()
    {
        if (isOfflineModeActive)
        {
            Debug.Log("Internet restored - returning to login screen");
            isOfflineModeActive = false;
            SceneManager.LoadScene("LoginScene");
        }
    }

    private void BypassAuthentication()
    {
        isOfflineModeActive = true;
        NetworkManager.Instance.SetOfflineMode(true);
        if (statusText != null)
        {
            statusText.text = "No internet connection detected.\nLoading offline mode...";
        }
        if (loginCanvas != null)
        {
            loginCanvas.gameObject.SetActive(false);
        }
        PlayerPrefs.SetString("PlayerUsername", "OfflinePlayer");
        PlayerPrefs.SetString("AuthToken", "offline_token");
        PlayerPrefs.SetInt("IsAuthenticated", 1);
        PlayerPrefs.SetInt("IsBypassedLogin", 1);
        Invoke("LoadGameScene", 1.5f);
    }
    
    public void OnLoginButtonClicked()
    {
        if (!NetworkManager.Instance.HasInternetConnection())
        {
            statusText.text = "No internet connection detected.";
            loginButton.interactable = true;
            return;
        }
        string username = usernameInput.text;
        string password = passwordInput.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter both username and password";
            return;
        }
        statusText.text = "Connecting to PlayFab...";
        loginButton.interactable = false;
        AttemptPlayFabLogin(username, password);
    }
    
    private System.Collections.IEnumerator SimulateLoginRequest(string username, string password)
    {
        yield return new UnityEngine.WaitForSeconds(loginDelay);
        
        bool hasInternet = CheckInternetConnection();
        
        if (!hasInternet && simulateOnlineOnly)
        {
            OnLoginFailure("No internet connection");
            yield break;
        }
        
        if (username.Length >= 3 && password.Length >= 3)
        {
            OnLoginSuccess(username);
        }
        else
        {
            OnLoginFailure("Invalid credentials");
        }
    }
    
    private bool CheckInternetConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
    
    private void AttemptPlayFabLogin(string username, string password)
    {
        Debug.Log($"PlayFab Title ID: {PlayFabSettings.staticSettings.TitleId}");
        Debug.Log($"Attempting login with username: {username}");
        Debug.Log($"PlayFab API URL: {PlayFabSettings.staticSettings.ProductionEnvironmentUrl}");
        
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            statusText.text = "No internet connection";
            loginButton.interactable = true;
            return;
        }
        
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };
        
        Debug.Log("Sending PlayFab login request with username...");
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnPlayFabLoginSuccess, 
            (error) => TryEmailLogin(username, password, error));
    }
    
    private void OnPlayFabLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFab login successful!");
        statusText.text = "PlayFab login successful! Loading game...";
        
        PlayerPrefs.SetString("PlayerUsername", result.PlayFabId);
        PlayerPrefs.SetString("AuthToken", result.SessionTicket);
        PlayerPrefs.SetString("PlayFabId", result.PlayFabId);
        PlayerPrefs.SetInt("IsAuthenticated", 1);
        PlayerPrefs.SetInt("IsBypassedLogin", 0);
        
        Invoke("LoadGameScene", 1f);
    }
    
    private void TryEmailLogin(string emailOrUsername, string password, PlayFabError usernameError)
    {
        Debug.LogWarning($"Username login failed, trying email: {usernameError.ErrorMessage}");
        
        var emailLoginRequest = new LoginWithEmailAddressRequest
        {
            Email = emailOrUsername,
            Password = password
        };
        
        Debug.Log("Trying PlayFab login with email...");
        PlayFabClientAPI.LoginWithEmailAddress(emailLoginRequest, OnPlayFabLoginSuccess, OnPlayFabLoginFailure);
    }
    
    private void OnPlayFabLoginFailure(PlayFabError error)
    {
        Debug.LogError($"PlayFab login failed: {error.GenerateErrorReport()}");
        Debug.LogError($"Error Code: {error.Error}");
        Debug.LogError($"Error Message: {error.ErrorMessage}");
        Debug.LogError($"Error Details: {error.ErrorDetails}");
        
        statusText.text = $"PlayFab Login Failed: {error.ErrorMessage}";
        
        loginButton.interactable = true;
    }
    
    private void OnLoginSuccess(string username)
    {
        Debug.Log("Login successful!");
        statusText.text = "Login successful! Loading game...";
        
        PlayerPrefs.SetString("PlayerUsername", username);
        PlayerPrefs.SetString("AuthToken", "simulated_token_" + System.DateTime.Now.Ticks);
        PlayerPrefs.SetInt("IsAuthenticated", 1);
        
        Invoke("LoadGameScene", 1f);
    }
    
    private void OnLoginFailure(string errorMessage)
    {
        Debug.LogError($"Login failed: {errorMessage}");
        
        statusText.text = $"Login failed: {errorMessage}";
        
        loginButton.interactable = true;
        
        if (simulateOnlineOnly)
        {
            statusText.text += "\n\nGame cannot be accessed without online authentication.";
        }
    }
    
    private void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void TestLogin()
    {
        Debug.Log("Test login - bypassing authentication for development");
        OnLoginSuccess("TestUser");
    }
    
    public void SimulateOffline()
    {
        simulateOnlineOnly = true;
        statusText.text = "Connection lost. Please check your internet connection.";
        loginButton.interactable = true;
    }
}
