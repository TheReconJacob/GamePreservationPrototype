using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameEvents", menuName = "Game/Game Events")]
public class GameEventSO : ScriptableObject
{
    [Header("Score Events")]
    [Tooltip("Triggered when score changes. Parameter: new score value")]
    public UnityEvent<int> OnScoreChanged;
    
    [Header("Target Events")]
    [Tooltip("Triggered when a target is destroyed. Parameter: point value")]
    public UnityEvent<int> OnTargetDestroyed;
    
    [Header("Game State Events")]
    [Tooltip("Triggered when game state changes (pause, game over, etc)")]
    public UnityEvent<string> OnGameStateChanged;
    
    // Methods to invoke events (optional convenience methods)
    public void RaiseTargetDestroyed(int pointValue)
    {
        Debug.Log($"[GameEvent] Target destroyed with {pointValue} points");
        OnTargetDestroyed?.Invoke(pointValue);
    }
    
    public void RaiseScoreChanged(int newScore)
    {
        Debug.Log($"[GameEvent] Score changed to {newScore}");
        OnScoreChanged?.Invoke(newScore);
    }
    
    public void RaiseGameStateChanged(string newState)
    {
        Debug.Log($"[GameEvent] Game state changed to: {newState}");
        OnGameStateChanged?.Invoke(newState);
    }
}
