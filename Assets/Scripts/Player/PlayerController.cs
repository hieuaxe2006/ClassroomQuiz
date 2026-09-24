using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
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
        if (!IsOwner) return;
        Move();
    }

    void Move()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");

        movement = new Vector2(moveHorizontal, moveVertical).normalized;
        rb.linearVelocity = movement * moveSpeed;
    }
    public override void OnNetworkSpawn()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            cam.enabled = IsOwner;
        }
    }
}