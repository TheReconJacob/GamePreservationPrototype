using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Network-aware camera follow system.
/// Automatically attaches the main camera to the local player.
/// </summary>
public class NetworkCameraFollow : NetworkBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Offset from player position")]
    public Vector3 cameraOffset = new Vector3(0f, 5f, -10f);
    
    [Tooltip("Camera rotation (x, y, z)")]
    public Vector3 cameraRotation = new Vector3(30f, 0f, 0f);
    
    [Tooltip("Should camera be a child of player?")]
    public bool attachAsChild = true;
    
    private Transform cameraTransform;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Only set up camera for the local player
        if (IsOwner)
        {
            SetupCamera();
        }
    }
    
    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("[NetworkCameraFollow] No main camera found!");
            return;
        }
        
        cameraTransform = mainCamera.transform;
        
        if (attachAsChild)
        {
            // Attach camera as child of player
            cameraTransform.SetParent(transform);
            cameraTransform.localPosition = cameraOffset;
            cameraTransform.localRotation = Quaternion.Euler(cameraRotation);
        }
        else
        {
            // Manual follow in LateUpdate
            cameraTransform.position = transform.position + cameraOffset;
            cameraTransform.rotation = Quaternion.Euler(cameraRotation);
        }
        
        Debug.Log($"[NetworkCameraFollow] Camera attached to local player (Client {OwnerClientId})");
    }
    
    void LateUpdate()
    {
        // Only update for local player
        if (!IsOwner) return;
        
        // If not attached as child, manually follow
        if (!attachAsChild && cameraTransform != null)
        {
            cameraTransform.position = transform.position + cameraOffset;
        }
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        // Detach camera when player despawns
        if (IsOwner && cameraTransform != null && attachAsChild)
        {
            cameraTransform.SetParent(null);
        }
    }
}
