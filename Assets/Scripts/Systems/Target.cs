using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Target Settings")]
    public int pointValue = 10;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Projectile"))
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.AddScore(pointValue);
            }
            
            Debug.Log($"Target hit! Awarded {pointValue} points");
            
            Destroy(other.gameObject);
            
            Destroy(gameObject);
        }
    }
}
