using UnityEngine;

public class Shooting : MonoBehaviour
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
        
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        
        ProjectileMovement projMovement = projectile.GetComponent<ProjectileMovement>();
        if (projMovement == null)
        {
            projMovement = projectile.AddComponent<ProjectileMovement>();
        }
        projMovement.Initialize(transform.forward, projectileSpeed);
        
        Destroy(projectile, projectileLifetime);
        
        Debug.Log("Projectile fired!");
    }
}
