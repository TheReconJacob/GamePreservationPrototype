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
}
