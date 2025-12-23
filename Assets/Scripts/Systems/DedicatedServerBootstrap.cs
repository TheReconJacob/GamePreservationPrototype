using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Bootstrap script for dedicated server mode.
/// Detects command-line arguments and automatically starts server.
/// </summary>
public class DedicatedServerBootstrap : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ServerConfig serverConfig;
    
    [Header("References")]
    [SerializeField] private LocalNetworkManager networkManager;
    
    private bool isServerMode = false;
    
    void Awake()
    {
        // Check for server mode command-line arguments
        string[] args = System.Environment.GetCommandLineArgs();
        
        foreach (string arg in args)
        {
            if (arg.ToLower() == "-server" || arg.ToLower() == "--server")
            {
                isServerMode = true;
                Debug.Log("[DedicatedServer] Server mode detected via command-line argument");
                break;
            }
        }
        
        // Also check for headless mode (Unity's built-in argument)
        if (Application.isBatchMode)
        {
            isServerMode = true;
            Debug.Log("[DedicatedServer] Headless mode detected (batchmode)");
        }
        
        if (isServerMode)
        {
            InitializeServer();
        }
    }
    
    void Start()
    {
        if (isServerMode)
        {
            StartServer();
        }
    }
    
    /// <summary>
    /// Initialize server configuration
    /// </summary>
    private void InitializeServer()
    {
        if (serverConfig != null)
        {
            serverConfig.ApplySettings();
        }
        else
        {
            Debug.LogWarning("[DedicatedServer] No ServerConfig assigned, using defaults");
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
        }
        
        // Disable audio in server mode
        AudioListener.volume = 0f;
        
        // Find all cameras and disable them in headless mode
        if (serverConfig != null && serverConfig.headlessMode)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                cam.enabled = false;
                Debug.Log($"[DedicatedServer] Disabled camera: {cam.name}");
            }
        }
        
        Debug.Log("[DedicatedServer] Server initialization complete");
    }
    
    /// <summary>
    /// Start the dedicated server
    /// </summary>
    private void StartServer()
    {
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<LocalNetworkManager>();
        }
        
        if (networkManager == null)
        {
            Debug.LogError("[DedicatedServer] LocalNetworkManager not found!");
            return;
        }
        
        // Apply port from config
        if (serverConfig != null)
        {
            networkManager.SetPort(serverConfig.serverPort);
        }
        
        // Start server
        networkManager.StartServer();
        
        Debug.Log("[DedicatedServer] Dedicated server started successfully");
        Debug.Log($"[DedicatedServer] Listening on port: {(serverConfig != null ? serverConfig.serverPort : 7777)}");
    }
    
    /// <summary>
    /// Check if running in server mode
    /// </summary>
    public bool IsServerMode()
    {
        return isServerMode;
    }
}
