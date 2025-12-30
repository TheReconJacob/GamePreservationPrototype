using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Simple NetworkBehaviour to sync lobby visibility across all clients.
/// Handles the ClientRpc to hide lobby when host starts the game.
/// </summary>
public class LobbySync : NetworkBehaviour
{
    /// <summary>
    /// ClientRpc to hide lobby on all clients
    /// </summary>
    [ClientRpc]
    public void HideLobbyClientRpc()
    {
        // Find the lobby UI and hide it
        LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
        if (lobbyUI != null)
        {
            lobbyUI.SetVisible(false);
        }
    }
    
    /// <summary>
    /// ClientRpc to hide lobby for specific client (used for dedicated server)
    /// </summary>
    [ClientRpc]
    public void HideLobbyForClientClientRpc(ClientRpcParams clientRpcParams = default)
    {
        LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
        if (lobbyUI != null)
        {
            lobbyUI.SetVisible(false);
            Debug.Log("[LobbySync] Client connected to dedicated server - lobby hidden");
        }
        
        // Update status text to show we're connected to a dedicated server
        #if !DEDICATED_SERVER
        NetworkUI networkUI = FindObjectOfType<NetworkUI>();
        if (networkUI != null)
        {
            LocalNetworkManager localNetworkManager = FindObjectOfType<LocalNetworkManager>();
            if (localNetworkManager != null)
            {
                networkUI.UpdateStatusText($"Connected to dedicated server at {localNetworkManager.ipAddress}");
            }
        }
        #endif
    }
    
    /// <summary>
    /// ClientRpc to show lobby for specific client (used for host with lobby)
    /// </summary>
    [ClientRpc]
    public void ShowLobbyForClientClientRpc(ClientRpcParams clientRpcParams = default)
    {
        LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
        if (lobbyUI != null)
        {
            lobbyUI.SetVisible(true);
            Debug.Log("[LobbySync] Client connected to host - lobby shown");
        }
        
        // Update status text to show we're connected to a hosted lobby
        #if !DEDICATED_SERVER
        NetworkUI networkUI = FindObjectOfType<NetworkUI>();
        if (networkUI != null)
        {
            networkUI.UpdateStatusText("Connected to hosted lobby");
        }
        #endif
    }
}
