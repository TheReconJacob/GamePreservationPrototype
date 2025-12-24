using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Simple lobby UI showing connected players.
/// Host can start the game when ready.
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Container for player list entries")]
    public Transform playerListContainer;
    
    [Tooltip("Prefab for individual player entries")]
    public GameObject playerEntryPrefab;
    
    [Tooltip("Button to start the game (host only)")]
    public Button startGameButton;
    
    [Tooltip("Status text")]
    public TextMeshProUGUI statusText;
    
    [Header("Network References")]
    [Tooltip("Reference to LocalNetworkManager")]
    public LocalNetworkManager networkManager;
    
    [Header("Settings")]
    [Tooltip("Scene to load when game starts (not used in minimal lobby - kept for future)")]
    public string gameSceneName = "GameScene";
    
    private Dictionary<ulong, GameObject> playerEntries = new Dictionary<ulong, GameObject>();
    
    void Start()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
        
        UpdateUI();
    }
    
    void OnEnable()
    {
        // Subscribe to network events
        if (Unity.Netcode.NetworkManager.Singleton != null)
        {
            Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    void OnDisable()
    {
        // Unsubscribe from network events
        if (Unity.Netcode.NetworkManager.Singleton != null)
        {
            Unity.Netcode.NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    
    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[LobbyUI] Client {clientId} connected to lobby");
        UpdatePlayerList();
    }
    
    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[LobbyUI] Client {clientId} disconnected from lobby");
        UpdatePlayerList();
    }
    
    /// <summary>
    /// Update the player list UI
    /// </summary>
    void UpdatePlayerList()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null || !Unity.Netcode.NetworkManager.Singleton.IsClient)
            return;
        
        // Clear existing entries
        foreach (var entry in playerEntries.Values)
        {
            if (entry != null)
                Destroy(entry);
        }
        playerEntries.Clear();
        
        // Create entry for each connected client
        if (Unity.Netcode.NetworkManager.Singleton.ConnectedClients != null)
        {
            foreach (var kvp in Unity.Netcode.NetworkManager.Singleton.ConnectedClients)
            {
                CreatePlayerEntry(kvp.Key);
            }
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// Create a player entry in the list
    /// </summary>
    void CreatePlayerEntry(ulong clientId)
    {
        if (playerEntryPrefab == null || playerListContainer == null)
        {
            Debug.LogError("[LobbyUI] PlayerEntryPrefab or PlayerListContainer not assigned!");
            return;
        }
        
        GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
        LobbyPlayerEntry entryScript = entry.GetComponent<LobbyPlayerEntry>();
        
        if (entryScript != null)
        {
            entryScript.SetClientId(clientId);
        }
        
        playerEntries[clientId] = entry;
    }
    
    /// <summary>
    /// Update UI state (button visibility, status text)
    /// </summary>
    void UpdateUI()
    {
        bool isHost = Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsHost;
        
        // Start button only visible for host
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(isHost);
        }
        
        // Update status text
        if (statusText != null)
        {
            if (Unity.Netcode.NetworkManager.Singleton == null || !Unity.Netcode.NetworkManager.Singleton.IsClient)
            {
                statusText.text = "Not connected";
            }
            else
            {
                int playerCount = Unity.Netcode.NetworkManager.Singleton.ConnectedClients.Count;
                string role = isHost ? "Host" : "Client";
                statusText.text = $"{role} - {playerCount} player(s) connected";
            }
        }
    }
    
    /// <summary>
    /// Start the game (host only)
    /// </summary>
    void OnStartGameClicked()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null || !Unity.Netcode.NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("[LobbyUI] Only the host can start the game!");
            return;
        }
        
        if (networkManager == null)
        {
            Debug.LogError("[LobbyUI] NetworkManager reference is missing! Cannot start game.");
            return;
        }
        
        Debug.Log("[LobbyUI] Starting game - hiding lobby");
        
        // Hide lobby and start the game (players are already spawned)
        networkManager.HideLobbyAndStartGame();
    }
    
    /// <summary>
    /// Public method to show/hide lobby
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        
        if (visible)
        {
            UpdatePlayerList();
        }
    }
}
