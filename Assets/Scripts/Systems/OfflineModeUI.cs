using UnityEngine;
using TMPro;

public class OfflineModeUI : MonoBehaviour
{
    [Header("Offline Mode Display")]
    public TextMeshProUGUI offlineModeText;
    public bool showOfflineIndicator = true;
    
    private void Start()
    {
        UpdateOfflineModeDisplay();
    }
    
    private void Update()
    {
        if (offlineModeText != null)
        {
            bool shouldShow = NetworkManager.Instance.IsOfflineMode() && showOfflineIndicator;
            if (offlineModeText.gameObject.activeSelf != shouldShow)
            {
                offlineModeText.gameObject.SetActive(shouldShow);
                Debug.Log($"Offline mode UI updated: {shouldShow}");
            }
        }
    }
    
    private void UpdateOfflineModeDisplay()
    {
        if (offlineModeText != null && showOfflineIndicator)
        {
            offlineModeText.gameObject.SetActive(NetworkManager.Instance.IsOfflineMode());
        }
    }
}
