using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    public string loginSceneName = "LoginScene";
    public string gameSceneName = "GameScene";
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    public static void LoadLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }
    
    public static void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
    
    public static void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
