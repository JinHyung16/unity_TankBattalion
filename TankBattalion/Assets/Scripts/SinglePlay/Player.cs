using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Animator anim;
    AudioSource audio;

    // movement
    private Vector2 direction = Vector2.zero;

    [SerializeField] private Transform movePoint;
    [SerializeField] private LayerMask stopMovement;

    [SerializeField]
    private float moveSpeed = 0.0f;

    private bool isHorizontal = false;
    private bool defending = false;

    // start Pos
    [SerializeField] private Vector3 startPos;

    // bullet prefab
    [SerializeField] private GameObject pBullet;
    private Vector2 firePos;

    // bullet fire
    [SerializeField] private KeyCode fireKey = KeyCode.None;
    [SerializeField] private float maxDelay = 1.5f;
    [SerializeField] private float curDelay = 0.0f;

    // check effect
    [SerializeField] private float defendTime = 0.0f;
    [SerializeField] private float maxDefendTime = 5.0f;

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

        moveSpeed = 0.9f;
        startPos = new Vector3(-1.74f, -2.75f, 0);
        this.gameObject.transform.position = startPos;
        movePoint.parent = null;
    }
    private void Update()
    {
        InputMovement();

        curDelay += Time.deltaTime;
        if (curDelay > maxDelay)
        {
            Fire();
        }

        if(defending)
        {
            defendTime += Time.deltaTime;
            if (defendTime >= maxDefendTime)
            {
                Debug.Log("Player: 방어만 하는 중");
                SinglePlayManager.GetInstance.isDefend = true;
            }
            defendTime = 0.0f;
        }
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("EBullet"))
        {
            // effect
            GameObject effect = Instantiate(boomEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);

            SinglePlayManager.GetInstance.isClear = false;
            SinglePlayManager.GetInstance.HealthDown();

            Destroy(collision.gameObject);

            // sound
            PlaySound("Die");
        }

        if(collision.CompareTag("Defend"))
        {
            defending = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Defend"))
        {
            defendTime = 0.0f;
            defending = false;
            SinglePlayManager.GetInstance.isDefend = false;
        }
    }

    #endregion

    #region Private Method
    private void InputMovement()
    {
        // move direction
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // check button down && up
        bool hUp = Input.GetButtonUp("Horizontal");
        bool vUp = Input.GetButtonUp("Vertical");
        bool hDown = Input.GetButtonDown("Horizontal");
        bool vDown = Input.GetButtonDown("Vertical");

        // check horizontal move
        if(vUp || hDown)
        {
            isHorizontal = true;
        }
        else if(hUp || vDown)
        {
            isHorizontal = false;
        }

        // animation
        anim.SetInteger("isHorizontal", (int)direction.x);
        anim.SetInteger("isVertical", (int)direction.y);

        if(anim.GetInteger("isVertical") == 1)
        {
            firePos = Vector2.up;
        }
        if (anim.GetInteger("isVertical") == -1)
        {
            firePos = Vector2.down;
        }
        if (anim.GetInteger("isHorizontal") == 1)
        {
            firePos = Vector2.right;
        }
        if (anim.GetInteger("isHorizontal") == -1)
        {
            firePos = Vector2.left;
        }
    }
    private void Movement()
    {
        // movement direction
        Vector3 moveVec = isHorizontal ? new Vector2(direction.x, 0) : new Vector2(0, direction.y);

        // gird movement 0.5 pixel
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, movePoint.position) <= 0.0001f)
        {
            if(Mathf.Abs(moveVec.x) == 1.0f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(moveVec.x/2, 0, 0), 0.2f, stopMovement))
                {
                    movePoint.position += new Vector3(moveVec.x/2, 0, 0);
                }
            }

            if(Mathf.Abs(moveVec.y) == 1.0f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0, moveVec.y/2, 0), 0.2f, stopMovement))
                {
                    movePoint.position += new Vector3(0, moveVec.y/2, 0);
                }
            }
        }
    }

    private void Fire()
    {
        if (Input.GetKeyDown(fireKey))
        {
            // fire the bullet
            GameObject bullet = Instantiate(pBullet, transform.position, transform.rotation);
            Rigidbody2D bulletRigid = bullet.GetComponent<Rigidbody2D>();
            bulletRigid.AddForce(firePos * 3.0f, ForceMode2D.Impulse);

            // sound
            PlaySound("Fire");

            curDelay = 0.0f;
        }
    }

    private void PlaySound(string name)
    {
        switch(name)
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

    #endregion
}
