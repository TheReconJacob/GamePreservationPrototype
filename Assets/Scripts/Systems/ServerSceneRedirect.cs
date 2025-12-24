using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Redirects to GameScene if launched with -server argument.
/// Runs after first scene loads and immediately switches if in server mode.
/// </summary>
public static class ServerSceneRedirect
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RedirectToGameSceneIfServer()
    {
        // Check for server mode command-line arguments
        string[] args = System.Environment.GetCommandLineArgs();
        bool isServerMode = false;
        
        foreach (string arg in args)
        {
            if (arg.ToLower() == "-server" || arg.ToLower() == "--server")
            {
                isServerMode = true;
                break;
            }
        }
        
        // Also check for batch mode (Unity headless)
        if (Application.isBatchMode)
        {
            isServerMode = true;
        }
        
        // If in server mode and not already in GameScene, switch to it
        if (isServerMode && SceneManager.GetActiveScene().name != "GameScene")
        {
            Debug.Log("[ServerSceneRedirect] Server mode detected in LoginScene - switching to GameScene");
            SceneManager.LoadScene("GameScene");
        }
    }
}
