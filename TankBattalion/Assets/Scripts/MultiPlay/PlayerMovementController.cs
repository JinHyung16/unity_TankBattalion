using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    private Rigidbody2D rigid2D;

    //[SerializeField] private Transform movePoint;
    //[SerializeField] private LayerMask stopMovement;

    [SerializeField] private float moveSpeed = 0.0f;
    private float currentSpeed = 0.0f;

    private float horizontalMovement;
    private float verticalMovement;

    private void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        moveSpeed = 3.0f;
        //movePoint.parent = null;
    }


    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        //ChangeSpeed(horizontalMovement, verticalMovement);
        rigid2D.velocity = new Vector2(horizontalMovement * moveSpeed, verticalMovement * moveSpeed);
    }

    private void ChangeSpeed(float inputHorDirection, float inputVerDirection)
    {
        //if the movemont started
        if (inputHorDirection != 0 || inputVerDirection != 0)
        {
            //Start Ramping Speed until reach the limit
            currentSpeed += 0.75f;
            if (currentSpeed >= moveSpeed)
                currentSpeed = moveSpeed;
        }
        else
        {
            currentSpeed -= 0.5f;

            //Stop moving when the speed go to 0
            if (currentSpeed <= 0)
            {
                currentSpeed = 0;
            }
        }
    }

    public void FixedMove(float horValue, float verValue)
    {
        rigid2D.velocity = new Vector2(horValue * moveSpeed, verValue * moveSpeed);
    }

    public void SetDirectionMovement(float horizonValue, float verticalValue)
    {
        this.horizontalMovement = horizonValue;
        this.verticalMovement = verticalValue;
    }

}
