using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Manages network connections for local multiplayer gameplay.
/// Uses Unity Transport (UTP) for LAN-based connections.
/// Supports Host/Client architecture for game preservation.
/// </summary>
public class LocalNetworkManager : MonoBehaviour
{
    [Header("Network Configuration")]
    [Tooltip("Default IP address for client connections (localhost for same machine)")]
    public string ipAddress = "127.0.0.1";
    
    [Tooltip("Port for network communication")]
    public ushort port = 7777;
    
    [Header("Lobby Configuration")]
    [Tooltip("Reference to the Lobby UI (optional - will be hidden when game starts)")]
    public LobbyUI lobbyUI;
    
    [Header("Player Spawning")]
    [Tooltip("Player prefab to spawn for each connected client")]
    public GameObject playerPrefab;
    
    [Tooltip("Spawn positions for players")]
    public Transform[] spawnPoints;
    
    private UnityTransport transport;
    private bool gameStarted = false;
    private bool isDedicatedServer = false;
    private System.Collections.Generic.List<ulong> pendingClients = new System.Collections.Generic.List<ulong>();
    
    void Start()
    {
        // Get the Unity Transport component from NetworkManager
        if (Unity.Netcode.NetworkManager.Singleton != null)
        {
            transport = Unity.Netcode.NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                // Configure transport for local network
                transport.ConnectionData.Address = ipAddress;
                transport.ConnectionData.Port = port;
                Debug.Log($"[LocalNetworkManager] Configured transport - IP: {ipAddress}, Port: {port}");
            }
            else
            {
                Debug.LogError("[LocalNetworkManager] UnityTransport component not found on NetworkManager!");
            }
            
            // Enable connection approval and set up callback BEFORE starting network
            Unity.Netcode.NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            Unity.Netcode.NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            
            // Disable automatic player spawning - we'll handle it manually
            Unity.Netcode.NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
        }
        else
        {
            Debug.LogError("[LocalNetworkManager] Unity.Netcode.NetworkManager.Singleton is null! Ensure NetworkManager exists in scene.");
        }
    }
    
    /// <summary>
    /// Connection approval callback - approves all connections for LAN play
    /// </summary>
    private void ApprovalCheck(Unity.Netcode.NetworkManager.ConnectionApprovalRequest request, Unity.Netcode.NetworkManager.ConnectionApprovalResponse response)
    {
        // Approve all connections for local LAN play
        response.Approved = true;
        response.CreatePlayerObject = false; // We handle player spawning manually
        response.Pending = false; // Don't delay the response
        response.Position = Vector3.zero; // Not used since CreatePlayerObject = false
        response.Rotation = Quaternion.identity; // Not used since CreatePlayerObject = false
        
        Debug.Log($"[LocalNetworkManager] Connection approved for client ID: {request.ClientNetworkId}");
    }
    
    /// <summary>
    /// Start as Host (Server + Client).
    /// Host runs the server and plays simultaneously.
    /// </summary>
    public void StartHost()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null)
        {
            Debug.LogError("[LocalNetworkManager] Cannot start host - NetworkManager missing!");
            return;
        }
        
        // Destroy any existing player GameObjects in the scene
        DestroyScenePlayers();
        
        bool success = Unity.Netcode.NetworkManager.Singleton.StartHost();
        
        if (success)
        {
            Debug.Log("[LocalNetworkManager] Started as Host (Server + Local Client)");
            Debug.Log($"[LocalNetworkManager] Other players can connect to: {ipAddress}:{port}");
            
            // Show lobby UI after successful host start
            if (lobbyUI != null)
            {
                lobbyUI.SetVisible(true);
            }
        }
        else
        {
            Debug.LogError("[LocalNetworkManager] Failed to start Host!");
        }
    }
    
    /// <summary>
    /// Start as Client and connect to host.
    /// Client connects to the IP address configured in transport.
    /// </summary>
    public void StartClient()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null)
        {
            Debug.LogError("[LocalNetworkManager] Cannot start client - NetworkManager missing!");
            return;
        }
        
        // Destroy any existing player GameObjects in the scene
        DestroyScenePlayers();
        
        // Update transport address before connecting
        if (transport != null)
        {
            transport.ConnectionData.Address = ipAddress;
            transport.ConnectionData.Port = port;
        }
        
        bool success = Unity.Netcode.NetworkManager.Singleton.StartClient();
        
        if (success)
        {
            Debug.Log($"[LocalNetworkManager] Started as Client, attempting connection to {ipAddress}:{port}");
            
            // Show lobby by default - will be hidden if connecting to dedicated server
            if (lobbyUI != null)
            {
                lobbyUI.SetVisible(true);
            }
        }
        else
        {
            Debug.LogError("[LocalNetworkManager] Failed to start Client!");
        }
    }
    
    /// <summary>
    /// Start as dedicated Server (no local player).
    /// Server only manages game state, doesn't play.
    /// </summary>
    public void StartServer()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null)
        {
            Debug.LogError("[LocalNetworkManager] Cannot start server - NetworkManager missing!");
            return;
        }
        
        // Destroy any existing player GameObjects in the scene
        DestroyScenePlayers();
        
        bool success = Unity.Netcode.NetworkManager.Singleton.StartServer();
        
        if (success)
        {
            Debug.Log("[LocalNetworkManager] Started as Dedicated Server");
            Debug.Log($"[LocalNetworkManager] Listening on port: {port}");
            
            // Mark as dedicated server
            isDedicatedServer = true;
            
            // Hide lobby UI for dedicated server (server doesn't need UI)
            if (lobbyUI != null)
            {
                lobbyUI.SetVisible(false);
            }
            Debug.Log("[LocalNetworkManager] Dedicated server mode - lobby UI hidden");
            
            // Mark game as started immediately (no lobby wait for server)
            gameStarted = true;
            
            // Spawn a player for the server (clientId 0 = server)
            StartCoroutine(SpawnPlayerDelayed(Unity.Netcode.NetworkManager.ServerClientId));
        }
        else
        {
            Debug.LogError("[LocalNetworkManager] Failed to start Server!");
        }
    }
    
    /// <summary>
    /// Disconnect and shutdown network.
    /// </summary>
    public void Shutdown()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null) return;
        
        if (Unity.Netcode.NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[LocalNetworkManager] Shutting down Host...");
        }
        else if (Unity.Netcode.NetworkManager.Singleton.IsClient)
        {
            Debug.Log("[LocalNetworkManager] Disconnecting Client...");
        }
        else if (Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[LocalNetworkManager] Shutting down Server...");
        }
        
        Unity.Netcode.NetworkManager.Singleton.Shutdown();
        Debug.Log("[LocalNetworkManager] Network shutdown complete");
    }
    
    /// <summary>
    /// Set IP address for client connections.
    /// </summary>
    public void SetIPAddress(string newIP)
    {
        ipAddress = newIP;
        if (transport != null)
        {
            transport.ConnectionData.Address = ipAddress;
            Debug.Log($"[LocalNetworkManager] IP Address updated to: {ipAddress}");
        }
    }
    
    /// <summary>
    /// Set port for network communication.
    /// </summary>
    public void SetPort(ushort newPort)
    {
        port = newPort;
        if (transport != null)
        {
            transport.ConnectionData.Port = port;
            Debug.Log($"[LocalNetworkManager] Port updated to: {port}");
        }
    }
    
    /// <summary>
    /// Get spawn position for a new player.
    /// </summary>
    public Vector3 GetSpawnPosition(int playerIndex)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[LocalNetworkManager] No spawn points defined, using default (0,1,0)");
            return new Vector3(0, 1, 0);
        }
        
        int index = playerIndex % spawnPoints.Length;
        return spawnPoints[index].position;
    }
    
    /// <summary>
    /// Hide lobby and start the game.
    /// Called by LobbyUI when host clicks "Start Game".
    /// Spawns all pending players and hides the lobby.
    /// </summary>
    public void HideLobbyAndStartGame()
    {
        if (!Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("[LocalNetworkManager] Only the server/host can start the game!");
            return;
        }
        
        // Mark game as started
        gameStarted = true;
        
        // Spawn all pending clients
        Debug.Log($"[LocalNetworkManager] Starting game - spawning {pendingClients.Count} pending players");
        foreach (ulong clientId in pendingClients)
        {
            SpawnPlayerForClient(clientId);
        }
        pendingClients.Clear();
        
        // Hide lobby on all clients
        if (lobbyUI != null)
        {
            LobbySync lobbySync = FindObjectOfType<LobbySync>();
            if (lobbySync != null)
            {
                lobbySync.HideLobbyClientRpc();
            }
            Debug.Log("[LocalNetworkManager] Game started - lobby hidden!");
        }
    }
    
    // Network event callbacks
    void OnEnable()
    {
        // Subscribe to events after NetworkManager is initialized
        StartCoroutine(SubscribeToNetworkEvents());
    }
    
    void OnDisable()
    {
        if (Unity.Netcode.NetworkManager.Singleton != null)
        {
            Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    
    private System.Collections.IEnumerator SubscribeToNetworkEvents()
    {
        // Wait until NetworkManager exists
        while (Unity.Netcode.NetworkManager.Singleton == null)
        {
            yield return null;
        }
        
        Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[LocalNetworkManager] Client {clientId} connected!");
        
        if (Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"[LocalNetworkManager] Total connected clients: {Unity.Netcode.NetworkManager.Singleton.ConnectedClients.Count}");
            
            // Tell client whether to show lobby or not based on server type
            LobbySync lobbySync = FindObjectOfType<LobbySync>();
            if (lobbySync != null)
            {
                if (isDedicatedServer)
                {
                    // Dedicated server - hide lobby on client
                    lobbySync.HideLobbyForClientClientRpc(new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { clientId }
                        }
                    });
                }
                else
                {
                    // Host with lobby - show lobby on client
                    lobbySync.ShowLobbyForClientClientRpc(new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { clientId }
                        }
                    });
                }
            }
            
            // If game has started, spawn immediately. Otherwise, add to pending list.
            if (gameStarted)
            {
                StartCoroutine(SpawnPlayerDelayed(clientId));
            }
            else
            {
                // Add to pending list - will spawn when host starts game
                if (!pendingClients.Contains(clientId))
                {
                    pendingClients.Add(clientId);
                    Debug.Log($"[LocalNetworkManager] Client {clientId} added to lobby - waiting for game start");
                }
            }
        }
    }
    
    /// <summary>
    /// Spawn player with a small delay to ensure network is fully initialized
    /// </summary>
    private System.Collections.IEnumerator SpawnPlayerDelayed(ulong clientId)
    {
        yield return new WaitForSeconds(0.1f);
        SpawnPlayerForClient(clientId);
    }
    
    /// <summary>
    /// Spawn a player instance for the connected client.
    /// </summary>
    private void SpawnPlayerForClient(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[LocalNetworkManager] Cannot spawn player - playerPrefab is not assigned!");
            return;
        }
        
        // Get spawn position based on client count
        int playerIndex = Unity.Netcode.NetworkManager.Singleton.ConnectedClients.Count - 1;
        Vector3 spawnPosition = GetSpawnPosition(playerIndex);
        
        // Instantiate player GameObject
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        // Get NetworkObject component
        Unity.Netcode.NetworkObject networkObject = playerInstance.GetComponent<Unity.Netcode.NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("[LocalNetworkManager] Player prefab doesn't have NetworkObject component!");
            Destroy(playerInstance);
            return;
        }
        
        // Spawn with ownership assigned to this client
        networkObject.SpawnAsPlayerObject(clientId, true);
        
        Debug.Log($"[LocalNetworkManager] Spawned player for Client {clientId} at {spawnPosition}");
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[LocalNetworkManager] Client {clientId} disconnected!");
        
        if (Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"[LocalNetworkManager] Remaining clients: {Unity.Netcode.NetworkManager.Singleton.ConnectedClients.Count}");
        }
    }
    
    /// <summary>
    /// Destroy all existing player GameObjects in the scene before starting network mode.
    /// </summary>
    private void DestroyScenePlayers()
    {
        PlayerMovement[] existingPlayers = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in existingPlayers)
        {
            Debug.Log($"[LocalNetworkManager] Destroying scene player: {player.gameObject.name}");
            Destroy(player.gameObject);
        }
    }
}
