using UnityEngine;

public class Projectile : MonoBehaviour
{
    void Start()
    {
        if (!gameObject.CompareTag("Projectile"))
        {
            gameObject.tag = "Projectile";
        }
    }
}

public class ProjectileMovement : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    
    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir;
        speed = spd;
    }
    
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
