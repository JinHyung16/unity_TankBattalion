using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    private Vector2 direction = Vector2.zero;

    [SerializeField] private Transform movePoint;
    [SerializeField] private LayerMask stopMovement;

    [SerializeField]
    private float moveSpeed = 0.0f;

    private float horizontalMovement;
    private float verticalMovement;

    private void Start()
    {
        movePoint.parent = null;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        direction = new Vector2(horizontalMovement, verticalMovement);

        Vector3 moveVec = horizontalMovement != 0 ? new Vector2(direction.x, 0) : new Vector2(0, direction.y);

        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) <= 0.0001f)
        {
            if (Mathf.Abs(moveVec.x) == 1.0f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(moveVec.x / 2, 0, 0), 0.2f, stopMovement))
                {
                    movePoint.position += new Vector3(moveVec.x / 2, 0, 0);
                }
            }

            if (Mathf.Abs(moveVec.y) == 1.0f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0, moveVec.y / 2, 0), 0.2f, stopMovement))
                {
                    movePoint.position += new Vector3(0, moveVec.y / 2, 0);
                }
            }
        }
    }
    
    public void SetHorizontalMovement(float horizonValue, float verticalValue)
    {
        this.horizontalMovement = horizonValue;
        this.verticalMovement = verticalValue;
    }

}
