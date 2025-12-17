using UnityEngine;

/// <summary>
/// ScriptableObject for offline-compatible game content updates.
/// Stores all configurable game values that can be updated without server dependencies.
/// Load from Resources folder to ensure offline availability.
/// </summary>
[CreateAssetMenu(fileName = "GameContent", menuName = "Game/Content Configuration", order = 1)]
public class GameContentSO : ScriptableObject
{
    [Header("Target Configuration")]
    [Tooltip("Array of target prefabs that can spawn in the game")]
    public GameObject[] targetPrefabs;
    
    [Tooltip("Point values corresponding to each target type (must match targetPrefabs length)")]
    public int[] pointValues = new int[] { 10, 20, 30 };
    
    [Header("Spawn Settings")]
    [Tooltip("Base spawn rate in seconds between target spawns")]
    public float baseSpawnRate = 2.0f;
    
    [Tooltip("Minimum spawn rate (fastest spawning)")]
    public float minSpawnRate = 0.5f;
    
    [Tooltip("Maximum number of targets that can exist simultaneously")]
    public int maxConcurrentTargets = 5;
    
    [Header("Difficulty Settings")]
    [Tooltip("How much the spawn rate decreases per score milestone")]
    public float spawnRateDecreasePerMilestone = 0.1f;
    
    [Tooltip("Score interval for increasing difficulty")]
    public int difficultyMilestoneInterval = 50;
    
    [Tooltip("Target movement speed multiplier")]
    public float targetSpeedMultiplier = 1.0f;
    
    [Header("Game Rules")]
    [Tooltip("Starting lives for the player")]
    public int startingLives = 3;
    
    [Tooltip("Bonus points awarded for combo hits")]
    public int comboBonus = 5;
    
    [Tooltip("Time window for combo hits (seconds)")]
    public float comboTimeWindow = 2.0f;
    
    /// <summary>
    /// Get point value for a specific target type index.
    /// </summary>
    public int GetPointValue(int targetTypeIndex)
    {
        if (targetTypeIndex < 0 || targetTypeIndex >= pointValues.Length)
        {
            Debug.LogWarning($"Invalid target type index: {targetTypeIndex}. Returning default value.");
            return pointValues.Length > 0 ? pointValues[0] : 10;
        }
        return pointValues[targetTypeIndex];
    }
    
    /// <summary>
    /// Calculate current spawn rate based on player score.
    /// </summary>
    public float GetCurrentSpawnRate(int currentScore)
    {
        int milestonesPassed = currentScore / difficultyMilestoneInterval;
        float calculatedRate = baseSpawnRate - (milestonesPassed * spawnRateDecreasePerMilestone);
        return Mathf.Max(calculatedRate, minSpawnRate);
    }
    
    /// <summary>
    /// Validate that the content configuration is properly set up.
    /// </summary>
    public bool ValidateConfiguration()
    {
        if (targetPrefabs == null || targetPrefabs.Length == 0)
        {
            Debug.LogError("GameContentSO: No target prefabs assigned!");
            return false;
        }
        
        if (pointValues == null || pointValues.Length == 0)
        {
            Debug.LogError("GameContentSO: No point values assigned!");
            return false;
        }
        
        if (targetPrefabs.Length != pointValues.Length)
        {
            Debug.LogWarning($"GameContentSO: Mismatch between target prefabs ({targetPrefabs.Length}) and point values ({pointValues.Length})");
        }
        
        return true;
    }
}
