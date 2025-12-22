using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Networked shooting system.
/// Only the owner can shoot, projectiles are spawned on server and replicated to all clients.
/// </summary>
public class Shooting : NetworkBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;
    
    void Start()
    {
        if (shootPoint == null)
        {
            GameObject shootPointObj = new GameObject("ShootPoint");
            shootPointObj.transform.SetParent(transform);
            shootPointObj.transform.localPosition = new Vector3(0, 0, 0.6f);
            shootPoint = shootPointObj.transform;
        }
    }
    
    void Update()
    {
        // Check if in network mode - if so, only allow shooting for owner
        if (Unity.Netcode.NetworkManager.Singleton != null && 
            (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer))
        {
            if (!IsSpawned || !IsOwner) return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }
    
    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned to Shooting script!");
            return;
        }
        
        // Check if in network mode
        if (Unity.Netcode.NetworkManager.Singleton != null && 
            (Unity.Netcode.NetworkManager.Singleton.IsClient || Unity.Netcode.NetworkManager.Singleton.IsServer))
        {
            // Request server to spawn projectile in network mode
            ShootServerRpc(shootPoint.position, transform.forward);
        }
        else
        {
            // Local mode - spawn projectile directly
            SpawnProjectileLocal(shootPoint.position, transform.forward);
        }
    }
    
    private void SpawnProjectileLocal(Vector3 position, Vector3 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, position, Quaternion.LookRotation(direction));
        
        ProjectileMovement projMovement = projectile.GetComponent<ProjectileMovement>();
        if (projMovement == null)
        {
            projMovement = projectile.AddComponent<ProjectileMovement>();
        }
        projMovement.Initialize(direction, projectileSpeed);
        
        Destroy(projectile, projectileLifetime);
        
        Debug.Log("Projectile fired locally!");
    }
    
    [ServerRpc]
    private void ShootServerRpc(Vector3 position, Vector3 direction)
    {
        // Server spawns the projectile
        GameObject projectile = Instantiate(projectilePrefab, position, Quaternion.LookRotation(direction));
        
        // If projectile has NetworkObject, spawn it on network first
        Unity.Netcode.NetworkObject networkObject = projectile.GetComponent<Unity.Netcode.NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true);
        }
        
        // Initialize movement after network spawn
        ProjectileMovement projMovement = projectile.GetComponent<ProjectileMovement>();
        if (projMovement == null)
        {
            projMovement = projectile.AddComponent<ProjectileMovement>();
        }
        projMovement.Initialize(direction, projectileSpeed);
        
        // Destroy after lifetime (use NetworkObject.Despawn if networked)
        if (networkObject != null)
        {
            StartCoroutine(DespawnAfterDelay(networkObject, projectileLifetime));
        }
        else
        {
            Destroy(projectile, projectileLifetime);
        }
        
        Debug.Log($"Projectile spawned for Client {OwnerClientId}");
    }
    
    private System.Collections.IEnumerator DespawnAfterDelay(Unity.Netcode.NetworkObject networkObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn();
            Destroy(networkObject.gameObject);
        }
    }
}
