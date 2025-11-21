using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Target Settings")]
    public int pointValue = 10;
    
    [Header("Game Events")]
    [Tooltip("Reference to the GameEvents ScriptableObject")]
    public GameEventSO gameEvents;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Projectile"))
        {
            // Raise event instead of calling GameManager directly
            if (gameEvents != null)
            {
                gameEvents.RaiseTargetDestroyed(pointValue);
            }
            else
            {
                Debug.LogWarning("GameEvents ScriptableObject not assigned to Target!");
            }
            
            Debug.Log($"Target hit! Awarded {pointValue} points");
            
            Destroy(other.gameObject);
            
            Destroy(gameObject);
        }
    }
}
