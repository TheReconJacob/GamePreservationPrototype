using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Networked player movement system using NetworkBehaviour.
/// Supports both local single-player and networked multiplayer.
/// Only the owner of this NetworkObject can control movement.
/// </summary>
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    [Header("Mod Integration")]
    [Tooltip("Allow ModManager to override movement speed")]
    public bool useModdedSpeed = true;
    
    private Vector3 moveDirection;
    private float actualMoveSpeed;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Load movement speed from ModManager if available
        if (useModdedSpeed && ModManager.Instance != null)
        {
            actualMoveSpeed = ModManager.Instance.GetPlayerSpeedModifier();
            Debug.Log($"[PlayerMovement] Loaded modded speed: {actualMoveSpeed}");
        }
        else
        {
            actualMoveSpeed = moveSpeed;
        }
        
        // Log network ownership info
        if (IsOwner)
        {
            Debug.Log($"[PlayerMovement] You own this player (Client ID: {OwnerClientId})");
        }
        else
        {
            Debug.Log($"[PlayerMovement] This is another player's character (Owner: {OwnerClientId})");
        }
    }
    
    void Update()
    {
        // Check if in network mode - if so, only allow input for owner
        if (Unity.Netcode.NetworkManager.Singleton != null && 
            (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer))
        {
            if (!IsOwner) return;
        }
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        // Debug: Log when input is detected
        if (moveDirection != Vector3.zero)
        {
            Debug.Log($"[PlayerMovement] Client {OwnerClientId} input: {moveDirection}");
        }
    }
    
    void FixedUpdate()
    {
        // Check if in network mode - if so, only allow movement for owner
        if (Unity.Netcode.NetworkManager.Singleton != null && 
            (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer))
        {
            if (!IsOwner) return;
        }
        
        // Use modded speed if available, otherwise use default
        float speedToUse = useModdedSpeed && ModManager.Instance != null 
            ? ModManager.Instance.GetPlayerSpeedModifier() 
            : actualMoveSpeed;
        
        // Use transform.position instead of Translate for NetworkTransform compatibility
        transform.position += moveDirection * speedToUse * Time.fixedDeltaTime;
    }
}
