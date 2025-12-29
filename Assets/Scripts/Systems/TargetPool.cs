using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Object pool for targets with network synchronization support.
/// Spawns targets within a BoxCollider zone with Y always at 1.
/// Works in offline, lobby, and dedicated server modes.
/// </summary>
public class TargetPool : NetworkBehaviour
{
    [Header("Pool Configuration")]
    [Tooltip("Target prefab to pool")]
    public GameObject targetPrefab;
    
    [Tooltip("Initial pool size")]
    public int poolSize = 20;
    
    [Tooltip("Maximum concurrent active targets")]
    public int maxActiveTargets = 5;
    
    [Header("Spawn Zone")]
    [Tooltip("BoxCollider defining spawn area (must be on this GameObject)")]
    public BoxCollider spawnZone;
    
    [Tooltip("Y position for all spawned targets")]
    public float spawnHeight = 1f;
    
    [Header("Spawn Settings")]
    [Tooltip("Base spawn rate in seconds")]
    public float baseSpawnRate = 2.0f;
    
    [Tooltip("Minimum spawn rate (fastest)")]
    public float minSpawnRate = 0.5f;
    
    [Tooltip("Score interval for difficulty increase")]
    public int difficultyInterval = 50;
    
    [Tooltip("Spawn rate decrease per difficulty milestone")]
    public float spawnRateDecrease = 0.1f;
    
    [Header("References")]
    [Tooltip("GameEvents for target configuration")]
    public GameEventSO gameEvents;
    
    [Tooltip("GameManager reference for score tracking")]
    public GameManager gameManager;
    
    // Pool management
    private List<GameObject> pooledTargets = new List<GameObject>();
    private List<GameObject> activeTargets = new List<GameObject>();
    private bool isSpawning = false;
    
    void Start()
    {
        // Validate spawn zone
        if (spawnZone == null)
        {
            spawnZone = GetComponent<BoxCollider>();
            if (spawnZone == null)
            {
                Debug.LogError("[TargetPool] No BoxCollider found! Please assign a spawn zone.");
                return;
            }
        }
        
        // Validate target prefab
        if (targetPrefab == null)
        {
            Debug.LogError("[TargetPool] No target prefab assigned!");
            return;
        }
        
        // Initialize pool on all clients (needed for synced indices)
        InitializePool();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            // Server starts spawning
            StartSpawning();
            Debug.Log("[TargetPool] Server started spawning");
        }
        else
        {
            // Client: Deactivate all local targets and wait for server sync
            DeactivateAllTargets();
            Debug.Log("[TargetPool] Client deactivated all local targets, waiting for server sync");
            
            // Request current target state from server
            RequestTargetStateSyncServerRpc();
        }
    }
    
    /// <summary>
    /// Deactivate all targets in the pool
    /// </summary>
    void DeactivateAllTargets()
    {
        foreach (GameObject target in pooledTargets)
        {
            if (target.activeInHierarchy)
            {
                target.SetActive(false);
            }
        }
        activeTargets.Clear();
        Debug.Log("[TargetPool] All targets deactivated");
    }
    
    /// <summary>
    /// Client requests current target state from server
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    void RequestTargetStateSyncServerRpc(ServerRpcParams rpcParams = default)
    {
        // Server sends current state to the requesting client
        ulong clientId = rpcParams.Receive.SenderClientId;
        
        Debug.Log($"[TargetPool] Syncing {activeTargets.Count} active targets to client {clientId}");
        
        // Send each active target's state
        foreach (GameObject target in activeTargets)
        {
            int targetIndex = pooledTargets.IndexOf(target);
            if (targetIndex >= 0)
            {
                // Send to specific client
                ClientRpcParams clientParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                
                SyncTargetSpawnClientRpc(targetIndex, target.transform.position, clientParams);
            }
        }
    }
    
    void OnNetworkDespawn()
    {
        // Stop spawning when network disconnects
        StopSpawning();
    }
    
    /// <summary>
    /// For offline mode, start spawning after a short delay
    /// </summary>
    void Update()
    {
        // Check if offline mode and not yet spawning
        bool isNetworkMode = Unity.Netcode.NetworkManager.Singleton != null && 
                            (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer);
        
        if (!isNetworkMode && !isSpawning && pooledTargets.Count > 0)
        {
            StartSpawning();
            Debug.Log("[TargetPool] Offline mode - started spawning");
        }
    }
    
    /// <summary>
    /// Initialize the object pool with inactive targets
    /// </summary>
    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject target = Instantiate(targetPrefab, transform);
            target.name = $"PooledTarget_{i}";
            target.SetActive(false);
            pooledTargets.Add(target);
            
            // Configure target component
            Target targetComponent = target.GetComponent<Target>();
            if (targetComponent != null)
            {
                targetComponent.gameEvents = gameEvents;
                
                // Register callback for when target is destroyed
                TargetPoolHelper helper = target.AddComponent<TargetPoolHelper>();
                helper.Initialize(this);
            }
        }
        
        Debug.Log($"[TargetPool] Initialized pool with {poolSize} targets");
    }
    
    /// <summary>
    /// Start spawning targets
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
            Debug.Log("[TargetPool] Started spawning");
        }
    }
    
    /// <summary>
    /// Stop spawning targets
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("[TargetPool] Stopped spawning");
    }
    
    /// <summary>
    /// Main spawn routine
    /// </summary>
    private System.Collections.IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            // Calculate spawn rate based on score
            int currentScore = gameManager != null ? gameManager.GetScore() : 0;
            float spawnRate = CalculateSpawnRate(currentScore);
            
            // Check if we can spawn more targets
            if (activeTargets.Count < maxActiveTargets)
            {
                SpawnTarget();
            }
            
            yield return new WaitForSeconds(spawnRate);
        }
    }
    
    /// <summary>
    /// Calculate spawn rate based on difficulty
    /// </summary>
    float CalculateSpawnRate(int score)
    {
        int milestones = score / difficultyInterval;
        float rate = baseSpawnRate - (milestones * spawnRateDecrease);
        return Mathf.Max(rate, minSpawnRate);
    }
    
    /// <summary>
    /// Spawn a target from the pool
    /// </summary>
    void SpawnTarget()
    {
        // Get inactive target from pool
        GameObject target = GetPooledTarget();
        if (target == null)
        {
            Debug.LogWarning("[TargetPool] No available targets in pool!");
            return;
        }
        
        // Calculate random spawn position within zone
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Position target and activate
        target.transform.position = spawnPosition;
        target.transform.rotation = Quaternion.identity;
        target.SetActive(true);
        
        // Track as active
        activeTargets.Add(target);
        
        Debug.Log($"[TargetPool] Spawned target at {spawnPosition}, active count: {activeTargets.Count}");
        
        // If in network mode, sync to clients
        bool isNetworkMode = Unity.Netcode.NetworkManager.Singleton != null && 
                            (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer);
        
        if (isNetworkMode && IsServer)
        {
            // Get target index in pool
            int targetIndex = pooledTargets.IndexOf(target);
            if (targetIndex >= 0)
            {
                SyncTargetSpawnClientRpc(targetIndex, spawnPosition);
            }
        }
    }
    
    /// <summary>
    /// Get an inactive target from the pool
    /// </summary>
    GameObject GetPooledTarget()
    {
        foreach (GameObject target in pooledTargets)
        {
            if (!target.activeInHierarchy)
            {
                return target;
            }
        }
        
        // Pool exhausted - expand if needed
        Debug.LogWarning("[TargetPool] Pool exhausted, consider increasing pool size");
        return null;
    }
    
    /// <summary>
    /// Get random spawn position within BoxCollider bounds
    /// </summary>
    Vector3 GetRandomSpawnPosition()
    {
        // Get BoxCollider bounds
        Bounds bounds = spawnZone.bounds;
        
        // Random X and Z within bounds
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        
        // Fixed Y at spawn height
        return new Vector3(randomX, spawnHeight, randomZ);
    }
    
    /// <summary>
    /// Return target to pool (called by TargetPoolHelper when target is destroyed)
    /// </summary>
    public void ReturnToPool(GameObject target)
    {
        if (activeTargets.Contains(target))
        {
            activeTargets.Remove(target);
            target.SetActive(false);
            Debug.Log($"[TargetPool] Target returned to pool, active count: {activeTargets.Count}");
            
            // If in network mode and server, sync to clients
            bool isNetworkMode = Unity.Netcode.NetworkManager.Singleton != null && 
                                (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer);
            
            if (isNetworkMode && IsServer)
            {
                int targetIndex = pooledTargets.IndexOf(target);
                if (targetIndex >= 0)
                {
                    SyncTargetDespawnClientRpc(targetIndex);
                }
            }
        }
    }
    
    /// <summary>
    /// ClientRpc to sync target spawn on all clients
    /// </summary>
    [ClientRpc]
    void SyncTargetSpawnClientRpc(int targetIndex, Vector3 spawnPosition, ClientRpcParams clientRpcParams = default)
    {
        // Skip on server (already spawned locally)
        if (IsServer)
            return;
        
        // Wait for pool to be initialized
        if (pooledTargets.Count == 0)
        {
            Debug.LogWarning("[TargetPool] Client received spawn RPC before pool initialized!");
            return;
        }
        
        if (targetIndex >= 0 && targetIndex < pooledTargets.Count)
        {
            GameObject target = pooledTargets[targetIndex];
            target.transform.position = spawnPosition;
            target.transform.rotation = Quaternion.identity;
            target.SetActive(true);
            
            if (!activeTargets.Contains(target))
            {
                activeTargets.Add(target);
            }
            
            Debug.Log($"[TargetPool] Client synced target spawn at {spawnPosition}, active: {activeTargets.Count}");
        }
        else
        {
            Debug.LogError($"[TargetPool] Client received invalid target index: {targetIndex}");
        }
    }
    
    /// <summary>
    /// ClientRpc to sync target despawn on all clients
    /// </summary>
    [ClientRpc]
    void SyncTargetDespawnClientRpc(int targetIndex)
    {
        // Skip on server (already despawned locally)
        if (IsServer)
            return;
        
        if (targetIndex >= 0 && targetIndex < pooledTargets.Count)
        {
            GameObject target = pooledTargets[targetIndex];
            target.SetActive(false);
            
            if (activeTargets.Contains(target))
            {
                activeTargets.Remove(target);
            }
            
            Debug.Log($"[TargetPool] Client synced target despawn, active: {activeTargets.Count}");
        }
    }
    
    /// <summary>
    /// Visualize spawn zone in Scene view
    /// </summary>
    void OnDrawGizmos()
    {
        if (spawnZone != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(spawnZone.bounds.center, spawnZone.bounds.size);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnZone.bounds.center, spawnZone.bounds.size);
            
            // Draw spawn height plane
            Gizmos.color = Color.yellow;
            Vector3 planeCenter = new Vector3(spawnZone.bounds.center.x, spawnHeight, spawnZone.bounds.center.z);
            Vector3 planeSize = new Vector3(spawnZone.bounds.size.x, 0.1f, spawnZone.bounds.size.z);
            Gizmos.DrawCube(planeCenter, planeSize);
        }
    }
    
    /// <summary>
    /// Clean up pool on destroy
    /// </summary>
    void OnDestroy()
    {
        StopSpawning();
    }
}

/// <summary>
/// Helper component attached to pooled targets to handle return to pool
/// </summary>
public class TargetPoolHelper : MonoBehaviour
{
    private TargetPool pool;
    private bool isBeingDestroyed = false;
    
    public void Initialize(TargetPool targetPool)
    {
        pool = targetPool;
    }
    
    void OnDisable()
    {
        // Prevent multiple calls during destruction
        if (isBeingDestroyed || pool == null)
            return;
        
        isBeingDestroyed = true;
        
        // Return to pool when disabled
        pool.ReturnToPool(gameObject);
        
        isBeingDestroyed = false;
    }
}
