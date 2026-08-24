using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerMovement : NetworkBehaviour
{
    Rigidbody2D rb;
    float moveHorizontal, moveVertical;
    float moveSpeed = 10;
    Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    void FixedUpdate()
    {
        if(!IsOwner) return;
        Move();
    }

    void Move()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");

        movement = new Vector2(moveHorizontal, moveVertical).normalized;
        rb.linearVelocity = movement * moveSpeed;
    }
}
