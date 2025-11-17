using UnityEngine;
using System;

public class NetworkManager : MonoBehaviour
{
    [Header("Network Settings")]
    public float connectivityCheckInterval = 5f;
    
    private bool hasInternet = true;
    private bool isOfflineMode = false;
    public static Action OnInternetRestored;
    public static Action OnInternetLost;  // New event for connection loss
    
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetworkManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("NetworkManager");
                    instance = go.AddComponent<NetworkManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        hasInternet = Application.internetReachability != NetworkReachability.NotReachable;
        if (!hasInternet)
        {
            isOfflineMode = true;
            Debug.Log("Started in offline mode");
        }
        InvokeRepeating(nameof(CheckConnectivity), connectivityCheckInterval, connectivityCheckInterval);
    }
    
    private void CheckConnectivity()
    {
        bool previousState = hasInternet;
        hasInternet = Application.internetReachability != NetworkReachability.NotReachable;
        
        if (previousState != hasInternet)
        {
            OnConnectivityChanged(hasInternet);
        }
    }
    
    public bool HasInternetConnection()
    {
        return hasInternet;
    }
    
    public bool IsOfflineMode()
    {
        return isOfflineMode;
    }
    
    public void SetOfflineMode(bool offline)
    {
        isOfflineMode = offline;
    }
    
    private void OnConnectivityChanged(bool connected)
    {
        Debug.Log($"Internet connectivity changed: {connected}");
        
        if (!connected && !isOfflineMode)
        {
            Debug.Log("Lost internet connection - switching to offline mode");
            SetOfflineMode(true);
            OnInternetLost?.Invoke();  // Trigger cloud-to-local migration
        }
        else if (connected && isOfflineMode)
        {
            Debug.Log("Internet connection restored - exiting offline mode");
            SetOfflineMode(false);
            OnInternetRestored?.Invoke();
        }
    }
}
