using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    private Rigidbody2D rigidbody2D;

    private Vector2 direction = Vector2.zero;

    //[SerializeField] private Transform movePoint;
    //[SerializeField] private LayerMask stopMovement;

    [SerializeField] private float moveSpeed = 0.0f;

    private bool IsHorizontalMove = false;

    private float horizontalMovement;
    private float verticalMovement;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();

        moveSpeed = 0.9f;
        //movePoint.parent = null;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        direction = new Vector2(horizontalMovement, verticalMovement);
        rigidbody2D.velocity = direction * moveSpeed;
    }
    
    public void SetDirectionMovement(float horizonValue, float verticalValue)
    {
        this.horizontalMovement = horizonValue;
        this.verticalMovement = verticalValue;
    }

    public void SetIsHorizontalMove(bool isHorizontal)
    {
        IsHorizontalMove = isHorizontal;
    }

}
