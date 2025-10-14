using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;

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
    
    private void Start()
    {
        statusText.text = "Please login to continue";
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        
        PlayFabSettings.staticSettings.TitleId = playfabTitleId;
        
        Debug.Log("Login screen loaded - game access blocked until authentication");
    }
    
    public void OnLoginButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter both username and password";
            return;
        }
        
        statusText.text = "Connecting to online services...";
        loginButton.interactable = false;
        
        StartCoroutine(SimulateLoginRequest(username, password));
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
