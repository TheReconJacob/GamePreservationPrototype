using UnityEngine;
using TMPro;

/// <summary>
/// Represents a single player entry in the lobby list.
/// Shows client ID only (minimal version).
/// </summary>
public class LobbyPlayerEntry : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text to display client ID")]
    public TextMeshProUGUI clientIdText;
    
    private ulong clientId;
    
    /// <summary>
    /// Set the client ID for this entry
    /// </summary>
    public void SetClientId(ulong id)
    {
        clientId = id;
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update the text display
    /// </summary>
    void UpdateDisplay()
    {
        if (clientIdText != null)
        {
            // Show "Host" for client 0 (server), "Client X" for others
            if (clientId == 0)
            {
                clientIdText.text = "Host (Client 0)";
            }
            else
            {
                clientIdText.text = $"Client {clientId}";
            }
        }
    }
}
