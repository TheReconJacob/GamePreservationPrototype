using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int currentScore = 0;
    
    void Start()
    {
        Debug.Log("Game started! Score: " + currentScore);
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"Score updated! Current score: {currentScore}");
    }
    
    public int GetScore()
    {
        return currentScore;
    }
}
