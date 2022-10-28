using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Enemy : MonoBehaviour
{
    Animator anim;
    AudioSource audio;

    // movement
    [SerializeField] private Transform movePoint;

    [Tooltip("닿으면 지나갈 수 없는 장애물")]
    [SerializeField] private LayerMask stopMovement;

    [Tooltip("무조건 피해야 하는 장애물")]
    [SerializeField] private LayerMask avoidMovement;

    Vector2[] directions = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
    private Vector2 direction = Vector2.zero;
    private int nextMove = 0;

    // check around
    private bool isUp = false;
    private bool isDown = false;
    private bool isLeft = false;
    private bool isRight = false;

    [SerializeField]
    private float moveSpeed = 0.0f;

    // check movement flag
    private bool isHorizontal = false;

    // time
    [SerializeField] private float reloadTime = 0.0f;
    private float fireTime = 0.0f;

    // Bullet Prefab
    [SerializeField] private GameObject eBullet;
    private Vector2 firePos;

    // effect prefab
    [SerializeField] private GameObject boomEffect;

    // audio
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip exploSound;

    #region Unity Method
    private void Start()
    {
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();

        movePoint.parent = null;

        nextMove = Random.Range(0, 4);
        direction = directions[nextMove];

        DirectionChange();
    }
    private void Update()
    {
        InputCheck();

        fireTime += Time.deltaTime;
        if (fireTime > reloadTime)
        {
            Fire();
            fireTime = 0.0f;
        }

        // destory enemy when game over
        if (SinglePlayManager.Instance.isOver)
        {
            movePoint.parent = this.gameObject.transform;
            Destroy(this.gameObject);
        }

        if (SinglePlayManager.Instance.isDefend)
        {
            Debug.Log("Enemy: 방어만 하여 더 빠르게 공격!!");
            reloadTime = 1.5f;
        }
        else
        {
            Debug.Log("Enemy: 다시 원래대로 공격!!");
            reloadTime = 3.0f;
        }
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // expresion animation
        if (collision.CompareTag("PBullet"))
        {
            GameObject effect = Instantiate(boomEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);

            SinglePlayManager.Instance.EnemyDown();

            // audio
            PlaySound("Die");

            // destroy setting
            Destroy(collision.gameObject);
            movePoint.parent = this.gameObject.transform;
            Destroy(this.gameObject);
        }
    }
    #endregion

    #region Private Method
    private void InputCheck()
    {
        if (direction == Vector2.up || direction == Vector2.down)
        {
            isHorizontal = false;
        }
        else if (direction == Vector2.right || direction == Vector2.left)
        {
            isHorizontal = true;
        }

        // 상하좌우 체크해서 bool이 ture인 곳을 찾는다.
        isUp = Physics2D.OverlapCircle(this.transform.position + new Vector3(0, 0.5f, 0), 0.2f, stopMovement);
        isDown = Physics2D.OverlapCircle(this.transform.position + new Vector3(0, -0.5f, 0), 0.2f, stopMovement);
        isLeft = Physics2D.OverlapCircle(this.transform.position + new Vector3(-0.5f, 0, 0), 0.2f, stopMovement);
        isRight = Physics2D.OverlapCircle(this.transform.position + new Vector3(0.5f, 0, 0), 0.2f, stopMovement);

        // animation
        anim.SetInteger("isHorizontal", (int)direction.x);
        anim.SetInteger("isVertical", (int)direction.y);

        if (anim.GetInteger("isVertical") == 1)
        {
            firePos = Vector2.up;
            direction = Vector2.up;
        }
        if (anim.GetInteger("isVertical") == -1)
        {
            firePos = Vector2.down;
            direction = Vector2.down;
        }
        if (anim.GetInteger("isHorizontal") == 1)
        {
            firePos = Vector2.right;
            direction = Vector2.right;
        }
        if (anim.GetInteger("isHorizontal") == -1)
        {
            firePos = Vector2.left;
            direction = Vector2.left;
        }
    }

    private void Movement()
    {
        // movement direction
        Vector3 wayPoint = isHorizontal ? new Vector2(direction.x, 0) : new Vector2(0, direction.y);

        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, movePoint.position) <= 0.0001f)
        {
            if (Mathf.Abs(wayPoint.x) == 1.0f)
            {
                if (!Physics2D.OverlapCircle(transform.position + new Vector3(wayPoint.x / 2, 0.0f, 0.0f), 0.2f, stopMovement))
                {
                    if (Physics2D.OverlapCircle(movePoint.position + new Vector3(wayPoint.x / 2, 0.0f, 0.0f), 0.2f, avoidMovement))
                    {
                        DirectionChange();
                    }
                    else
                    {
                        movePoint.position += new Vector3(wayPoint.x / 2, 0, 0);
                    }
                }
                else
                {
                    DirectionChange();
                }
            }
            if (Mathf.Abs(wayPoint.y) == 1.0f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0.0f, wayPoint.y / 2, 0.0f), 0.2f, stopMovement))
                {
                    if (Physics2D.OverlapCircle(movePoint.position + new Vector3(0.0f, wayPoint.y / 2, 0.0f), 0.2f, avoidMovement))
                    {
                        DirectionChange();
                    }
                    else
                    {
                        movePoint.position += new Vector3(0, wayPoint.y / 2, 0);
                    }
                }
                else
                {
                    DirectionChange();
                }
            }
        }
    }

    private void DirectionChange()
    {
        StopAllCoroutines();
        StartCoroutine(AutoChangeDirecion());
    }

    private void Fire()
    {
        // fire the bullet
        GameObject bullet = Instantiate(eBullet, transform.position, transform.rotation);
        Rigidbody2D bulletRigid2D = bullet.GetComponent<Rigidbody2D>();
        bulletRigid2D.AddForce(firePos * 1.5f, ForceMode2D.Impulse);

        // audio
        PlaySound("Fire");
    }

    private void PlaySound(string name)
    {
        switch (name)
        {
            case "Fire":
                audio.clip = fireSound;
                break;
            case "Die":
                audio.clip = exploSound;
                break;
        }

        audio.Play();
    }

    private IEnumerator AutoChangeDirecion()
    {
        while (true)
        {
            List<Vector2> randomDirection = new List<Vector2>();

            if (!isUp)
            {
                randomDirection.Add(Vector2.up);
            }
            if (!isDown)
            {
                randomDirection.Add(Vector2.down);
            }
            if (!isLeft)
            {
                randomDirection.Add(Vector2.left);
            }
            if (!isRight)
            {
                randomDirection.Add(Vector2.right);
            }

            if (randomDirection != null)
            {
                int rand = Random.Range(0, randomDirection.Count);
                direction = randomDirection[rand];
            }

            randomDirection.Clear();

            yield return Cashing.YieldInstruction.WaitForSeconds(0.5f);
        }
    }

    #endregion

}
