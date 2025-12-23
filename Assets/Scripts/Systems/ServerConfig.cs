using UnityEngine;

/// <summary>
/// Configuration for dedicated server mode.
/// Controls headless mode settings and server-specific behavior.
/// </summary>
[CreateAssetMenu(fileName = "ServerConfig", menuName = "Game/Server Configuration")]
public class ServerConfig : ScriptableObject
{
    [Header("Server Mode")]
    [Tooltip("Enable headless mode (no graphics rendering)")]
    public bool headlessMode = true;
    
    [Tooltip("Target frame rate for server (lower = less CPU usage)")]
    [Range(10, 60)]
    public int serverTickRate = 30;
    
    [Header("Network Settings")]
    [Tooltip("Port for dedicated server")]
    public ushort serverPort = 7777;
    
    [Tooltip("Maximum number of connected clients")]
    [Range(2, 16)]
    public int maxPlayers = 4;
    
    [Header("Logging")]
    [Tooltip("Enable verbose server logging")]
    public bool verboseLogging = true;
    
    [Tooltip("Log file path (relative to executable)")]
    public string logFilePath = "server_log.txt";
    
    /// <summary>
    /// Apply server configuration settings
    /// </summary>
    public void ApplySettings()
    {
        // Set target frame rate
        Application.targetFrameRate = serverTickRate;
        
        // Configure logging
        if (verboseLogging)
        {
            Debug.Log($"[ServerConfig] Headless Mode: {headlessMode}");
            Debug.Log($"[ServerConfig] Server Tick Rate: {serverTickRate}");
            Debug.Log($"[ServerConfig] Server Port: {serverPort}");
            Debug.Log($"[ServerConfig] Max Players: {maxPlayers}");
        }
        
        // Disable VSync in server mode
        QualitySettings.vSyncCount = 0;
        
        Debug.Log("[ServerConfig] Server configuration applied");
    }
}
