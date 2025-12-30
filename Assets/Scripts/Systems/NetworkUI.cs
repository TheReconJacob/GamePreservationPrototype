#if !DEDICATED_SERVER
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple UI for starting local network sessions.
/// Provides buttons for Host/Client/Server modes.
/// Displays connection status and player count.
/// CLIENT-ONLY: This entire script is excluded from dedicated server builds.
/// </summary>
public class NetworkUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Button to start as Host")]
    public Button hostButton;
    
    [Tooltip("Button to start as Client")]
    public Button clientButton;
    
    [Tooltip("Button to start as Server")]
    public Button serverButton;
    
    [Tooltip("Button to disconnect")]
    public Button disconnectButton;
    
    [Tooltip("Text field for IP address input")]
    public TMP_InputField ipInputField;
    
    [Tooltip("Status text display")]
    public TextMeshProUGUI statusText;
    
    [Header("Network Manager")]
    public LocalNetworkManager localNetworkManager;
    
    void Start()
    {
        // Setup button listeners
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostClicked);
        
        if (clientButton != null)
            clientButton.onClick.AddListener(OnClientClicked);
        
        if (serverButton != null)
            serverButton.onClick.AddListener(OnServerClicked);
        
        if (disconnectButton != null)
        {
            disconnectButton.onClick.AddListener(OnDisconnectClicked);
            disconnectButton.gameObject.SetActive(false); // Hidden until connected
        }
        
        // Set default IP in input field
        if (ipInputField != null && localNetworkManager != null)
        {
            ipInputField.text = localNetworkManager.ipAddress;
            ipInputField.onEndEdit.AddListener(OnIPAddressChanged);
        }
        
        UpdateStatusText("Ready to connect");
    }
    
    void OnHostClicked()
    {
        if (localNetworkManager == null)
        {
            Debug.LogError("[NetworkUI] LocalNetworkManager reference missing!");
            return;
        }
        
        localNetworkManager.StartHost();
        UpdateStatusText("Starting as Host...");
        ShowDisconnectButton();
        HideConnectionButtons();
    }
    
    void OnClientClicked()
    {
        if (localNetworkManager == null)
        {
            Debug.LogError("[NetworkUI] LocalNetworkManager reference missing!");
            return;
        }
        
        localNetworkManager.StartClient();
        UpdateStatusText("Connecting...");
        ShowDisconnectButton();
        HideConnectionButtons();
    }
    
    void OnServerClicked()
    {
        if (localNetworkManager == null)
        {
            Debug.LogError("[NetworkUI] LocalNetworkManager reference missing!");
            return;
        }
        
        localNetworkManager.StartServer();
        UpdateStatusText("Starting as Dedicated Server...");
        ShowDisconnectButton();
        HideConnectionButtons();
    }
    
    void OnDisconnectClicked()
    {
        if (localNetworkManager == null) return;
        
        localNetworkManager.Shutdown();
        UpdateStatusText("Disconnected");
        HideDisconnectButton();
        ShowConnectionButtons();
    }
    
    void OnIPAddressChanged(string newIP)
    {
        if (localNetworkManager != null)
        {
            localNetworkManager.SetIPAddress(newIP);
        }
    }
    
    public void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[NetworkUI] {message}");
    }
    
    void ShowDisconnectButton()
    {
        if (disconnectButton != null)
            disconnectButton.gameObject.SetActive(true);
    }
    
    void HideDisconnectButton()
    {
        if (disconnectButton != null)
            disconnectButton.gameObject.SetActive(false);
    }
    
    void HideConnectionButtons()
    {
        if (hostButton != null)
            hostButton.gameObject.SetActive(false);
        
        if (clientButton != null)
            clientButton.gameObject.SetActive(false);
        
        if (serverButton != null)
            serverButton.gameObject.SetActive(false);
        
        if (ipInputField != null)
            ipInputField.gameObject.SetActive(false);
    }
    
    void ShowConnectionButtons()
    {
        if (hostButton != null)
            hostButton.gameObject.SetActive(true);
        
        if (clientButton != null)
            clientButton.gameObject.SetActive(true);
        
        if (serverButton != null)
            serverButton.gameObject.SetActive(true);
        
        if (ipInputField != null)
            ipInputField.gameObject.SetActive(true);
    }
}
#endif // !DEDICATED_SERVER
