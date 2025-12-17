using UnityEngine;
using System.Collections;

/// <summary>
/// Example target spawner that uses ContentManager for offline-compatible content.
/// Demonstrates how to use dynamic spawn rates and prefab loading.
/// </summary>
public class TargetSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [Tooltip("Reference to GameManager to get current score")]
    public GameManager gameManager;
    
    [Tooltip("Spawn area bounds (targets spawn within this area)")]
    public Vector3 spawnAreaSize = new Vector3(10f, 1f, 10f);
    
    [Tooltip("Enable to visualize spawn area in Scene view")]
    public bool showSpawnGizmo = true;
    
    [Header("Game Events")]
    [Tooltip("Reference to GameEvents for target assignment")]
    public GameEventSO gameEvents;
    
    private bool isSpawning = false;
    private int currentTargetCount = 0;
    
    void Start()
    {
        // Verify ContentManager is ready
        if (!ContentManager.Instance.IsContentLoaded())
        {
            Debug.LogError("TargetSpawner: ContentManager not ready! Cannot spawn targets.");
            return;
        }
        
        Debug.Log("TargetSpawner: Ready to spawn content-driven targets");
        StartSpawning();
    }
    
    /// <summary>
    /// Start the spawning coroutine.
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
            Debug.Log("TargetSpawner: Started spawning");
        }
    }
    
    /// <summary>
    /// Stop spawning targets.
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("TargetSpawner: Stopped spawning");
    }
    
    /// <summary>
    /// Main spawn routine - uses ContentManager for spawn rates.
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            // Get current spawn rate from ContentManager based on score
            int currentScore = gameManager != null ? gameManager.currentScore : 0;
            float spawnRate = ContentManager.Instance.GetSpawnRate(currentScore);
            
            // Check if we can spawn more targets
            int maxTargets = ContentManager.Instance.CurrentContent.maxConcurrentTargets;
            
            if (currentTargetCount < maxTargets)
            {
                SpawnTarget();
            }
            else
            {
                Debug.Log($"TargetSpawner: Max targets ({maxTargets}) reached, waiting...");
            }
            
            // Wait based on content-driven spawn rate
            yield return new WaitForSeconds(spawnRate);
        }
    }
    
    /// <summary>
    /// Spawn a single target using ContentManager prefabs.
    /// </summary>
    private void SpawnTarget()
    {
        // Get random target prefab from ContentManager
        GameObject targetPrefab = ContentManager.Instance.GetRandomTargetPrefab(out int targetTypeIndex);
        
        if (targetPrefab == null)
        {
            Debug.LogError("TargetSpawner: Failed to get target prefab from ContentManager!");
            return;
        }
        
        // Calculate random spawn position within bounds
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        
        Vector3 spawnPosition = transform.position + randomOffset;
        
        // Instantiate target
        GameObject spawnedTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
        
        // Configure the target component
        Target targetComponent = spawnedTarget.GetComponent<Target>();
        if (targetComponent != null)
        {
            targetComponent.targetTypeIndex = targetTypeIndex;
            targetComponent.gameEvents = gameEvents;
            
            int pointValue = ContentManager.Instance.GetPointValue(targetTypeIndex);
            Debug.Log($"TargetSpawner: Spawned {targetPrefab.name} (Type: {targetTypeIndex}, Points: {pointValue})");
        }
        else
        {
            Debug.LogWarning($"TargetSpawner: Spawned prefab missing Target component!");
        }
        
        currentTargetCount++;
        
        // Register for destruction notification to update count
        TargetTracker tracker = spawnedTarget.AddComponent<TargetTracker>();
        tracker.Initialize(this);
    }
    
    /// <summary>
    /// Called when a spawned target is destroyed.
    /// </summary>
    public void OnTargetDestroyed()
    {
        currentTargetCount--;
        Debug.Log($"TargetSpawner: Target destroyed, current count: {currentTargetCount}");
    }
    
    /// <summary>
    /// Visualize spawn area in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showSpawnGizmo)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, spawnAreaSize);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, spawnAreaSize);
        }
    }
}

/// <summary>
/// Helper component to track when targets are destroyed.
/// </summary>
public class TargetTracker : MonoBehaviour
{
    private TargetSpawner spawner;
    
    public void Initialize(TargetSpawner targetSpawner)
    {
        spawner = targetSpawner;
    }
    
    private void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnTargetDestroyed();
        }
    }
}
