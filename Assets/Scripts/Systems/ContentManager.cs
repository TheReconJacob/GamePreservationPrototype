using UnityEngine;

/// <summary>
/// Manages offline-compatible game content loading.
/// Uses Resources folder to ensure content is available without network connectivity.
/// Implements Singleton pattern for easy access throughout the game.
/// </summary>
public class ContentManager : MonoBehaviour
{
    [Header("Content Configuration")]
    [Tooltip("Name of the GameContentSO file in Resources folder")]
    public string contentFileName = "DefaultGameContent";
    
    private static ContentManager instance;
    private GameContentSO currentContent;
    
    /// <summary>
    /// Singleton instance accessor.
    /// </summary>
    public static ContentManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ContentManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ContentManager");
                    instance = go.AddComponent<ContentManager>();
                    DontDestroyOnLoad(go);
                    instance.LoadContent();
                }
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Get the currently loaded game content.
    /// </summary>
    public GameContentSO CurrentContent => currentContent;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadContent();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load game content from Resources folder.
    /// This ensures offline availability as Resources are bundled with the build.
    /// </summary>
    public void LoadContent()
    {
        Debug.Log($"ContentManager: Loading game content from Resources/{contentFileName}");
        
        currentContent = Resources.Load<GameContentSO>(contentFileName);
        
        if (currentContent == null)
        {
            Debug.LogError($"ContentManager: Failed to load GameContentSO from Resources/{contentFileName}! " +
                          "Ensure a GameContentSO asset exists in Assets/Resources/ folder.");
            return;
        }
        
        if (!currentContent.ValidateConfiguration())
        {
            Debug.LogError("ContentManager: Loaded content has invalid configuration!");
            return;
        }
        
        Debug.Log($"ContentManager: Successfully loaded game content with {currentContent.targetPrefabs.Length} target types");
        LogContentDetails();
    }
    
    /// <summary>
    /// Swap to a different content configuration.
    /// Useful for content updates or different game modes.
    /// </summary>
    public bool LoadAlternativeContent(string fileName)
    {
        Debug.Log($"ContentManager: Attempting to load alternative content: {fileName}");
        
        GameContentSO newContent = Resources.Load<GameContentSO>(fileName);
        
        if (newContent == null)
        {
            Debug.LogError($"ContentManager: Failed to load alternative content from Resources/{fileName}");
            return false;
        }
        
        if (!newContent.ValidateConfiguration())
        {
            Debug.LogError($"ContentManager: Alternative content {fileName} has invalid configuration!");
            return false;
        }
        
        currentContent = newContent;
        contentFileName = fileName;
        Debug.Log($"ContentManager: Successfully switched to content: {fileName}");
        LogContentDetails();
        
        return true;
    }
    
    /// <summary>
    /// Get point value for a specific target type.
    /// </summary>
    public int GetPointValue(int targetTypeIndex)
    {
        if (currentContent == null)
        {
            Debug.LogWarning("ContentManager: No content loaded! Returning default value.");
            return 10;
        }
        
        return currentContent.GetPointValue(targetTypeIndex);
    }
    
    /// <summary>
    /// Get the current spawn rate based on player's score.
    /// </summary>
    public float GetSpawnRate(int currentScore)
    {
        if (currentContent == null)
        {
            Debug.LogWarning("ContentManager: No content loaded! Returning default spawn rate.");
            return 2.0f;
        }
        
        return currentContent.GetCurrentSpawnRate(currentScore);
    }
    
    /// <summary>
    /// Get a random target prefab from the loaded content.
    /// </summary>
    public GameObject GetRandomTargetPrefab(out int targetTypeIndex)
    {
        targetTypeIndex = 0;
        
        if (currentContent == null || currentContent.targetPrefabs == null || currentContent.targetPrefabs.Length == 0)
        {
            Debug.LogError("ContentManager: No target prefabs available!");
            return null;
        }
        
        targetTypeIndex = Random.Range(0, currentContent.targetPrefabs.Length);
        return currentContent.targetPrefabs[targetTypeIndex];
    }
    
    /// <summary>
    /// Get a specific target prefab by index.
    /// </summary>
    public GameObject GetTargetPrefab(int index)
    {
        if (currentContent == null || currentContent.targetPrefabs == null)
        {
            Debug.LogError("ContentManager: No target prefabs available!");
            return null;
        }
        
        if (index < 0 || index >= currentContent.targetPrefabs.Length)
        {
            Debug.LogWarning($"ContentManager: Invalid target index {index}");
            return null;
        }
        
        return currentContent.targetPrefabs[index];
    }
    
    /// <summary>
    /// Check if content is properly loaded and ready.
    /// </summary>
    public bool IsContentLoaded()
    {
        return currentContent != null && currentContent.ValidateConfiguration();
    }
    
    /// <summary>
    /// Log current content configuration details for debugging.
    /// </summary>
    private void LogContentDetails()
    {
        if (currentContent == null) return;
        
        Debug.Log("=== Content Configuration ===");
        Debug.Log($"Target Types: {currentContent.targetPrefabs.Length}");
        Debug.Log($"Base Spawn Rate: {currentContent.baseSpawnRate}s");
        Debug.Log($"Difficulty Milestone: Every {currentContent.difficultyMilestoneInterval} points");
        Debug.Log($"Starting Lives: {currentContent.startingLives}");
        Debug.Log($"Max Concurrent Targets: {currentContent.maxConcurrentTargets}");
        
        for (int i = 0; i < currentContent.pointValues.Length; i++)
        {
            string prefabName = i < currentContent.targetPrefabs.Length && currentContent.targetPrefabs[i] != null 
                ? currentContent.targetPrefabs[i].name 
                : "None";
            Debug.Log($"  Target Type {i}: {prefabName} = {currentContent.pointValues[i]} points");
        }
        Debug.Log("============================");
    }
}
