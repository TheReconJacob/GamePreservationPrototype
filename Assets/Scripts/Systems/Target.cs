using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Index of this target type in the GameContentSO configuration")]
    public int targetTypeIndex = 0;
    
    [Tooltip("Override point value (0 = use ContentManager value)")]
    public int pointValueOverride = 0;
    
    [Header("Game Events")]
    [Tooltip("Reference to the GameEvents ScriptableObject")]
    public GameEventSO gameEvents;
    
    private int actualPointValue;
    
    void Start()
    {
        // Get point value from ModManager (which can fallback to ContentManager)
        if (pointValueOverride > 0)
        {
            actualPointValue = pointValueOverride;
            Debug.Log($"Target using override point value: {actualPointValue}");
        }
        else
        {
            // Try ModManager first (for JSON mod customization)
            if (ModManager.Instance != null)
            {
                actualPointValue = ModManager.Instance.GetTargetPointValue(targetTypeIndex);
                Debug.Log($"Target loaded point value from ModManager: {actualPointValue} (Type: {targetTypeIndex})");
            }
            // Fallback to ContentManager if ModManager unavailable
            else if (ContentManager.Instance != null && ContentManager.Instance.IsContentLoaded())
            {
                actualPointValue = ContentManager.Instance.GetPointValue(targetTypeIndex);
                Debug.Log($"Target loaded point value from ContentManager: {actualPointValue} (Type: {targetTypeIndex})");
            }
            // Final fallback to default if both unavailable
            else
            {
                actualPointValue = 10;
                Debug.LogWarning("ModManager and ContentManager not available, using default point value: 10");
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Projectile"))
        {
            // Raise event instead of calling GameManager directly
            if (gameEvents != null)
            {
                gameEvents.RaiseTargetDestroyed(actualPointValue);
            }
            else
            {
                Debug.LogWarning("GameEvents ScriptableObject not assigned to Target!");
            }
            
            Debug.Log($"Target hit! Awarded {actualPointValue} points");
            
            Destroy(other.gameObject);
            
            Destroy(gameObject);
        }
    }
}
