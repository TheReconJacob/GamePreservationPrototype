using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    private Vector3 moveDirection;
    
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }
    
    void FixedUpdate()
    {
        transform.Translate(moveDirection * moveSpeed * Time.fixedDeltaTime);
    }
}
