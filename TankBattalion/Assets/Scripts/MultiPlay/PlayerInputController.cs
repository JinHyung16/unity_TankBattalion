using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    private Animator anim;
    private AudioSource audio;

    private PlayerMovementController playerMovementController;
    private PlayerWeaponController playerWeaponController;

    private Vector2 direction = Vector2.zero;

    [HideInInspector] public float HorizontalInput;
    [HideInInspector] public float VerticalInput;
    [HideInInspector] public bool InputChange = false;
    [HideInInspector] public bool Fire = false;

    [SerializeField] private KeyCode fireKey = KeyCode.None;


    private void Start()
    {
        playerMovementController = GetComponentInChildren<PlayerMovementController>();
        playerWeaponController = GetComponentInChildren<PlayerWeaponController>();

        anim = GetComponentInChildren<Animator>();
        audio = GetComponentInChildren<AudioSource>();

        fireKey = KeyCode.Space;
    }
    private void Update()
    {
        InputMovement();
    }

    private void InputMovement()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        var fire = Input.GetKeyDown(fireKey);

        InputChange = (horizontalInput != HorizontalInput || verticalInput != VerticalInput || fire != Fire);

        HorizontalInput = horizontalInput;
        VerticalInput = verticalInput;
        Fire = fire;

        playerMovementController.SetDirectionMovement(HorizontalInput, VerticalInput);

        AnimationController();

        if (Fire)
        {
            playerWeaponController.AttackFire();
        }

    }

    private void AnimationController()
    {
        direction = new Vector2(HorizontalInput, VerticalInput);

        bool hUp = Input.GetButtonUp("Horizontal");
        bool vUp = Input.GetButtonUp("Vertical");
        bool hDown = Input.GetButtonDown("Horizontal");
        bool vDown = Input.GetButtonDown("Vertical");

        // check horizontal move
        if (vUp || hDown)
        {
            playerMovementController.SetIsHorizontalMove(true);
            //isHorizontal = true;
        }
        else if (hUp || vDown)
        {
            playerMovementController.SetIsHorizontalMove(false);
            //isHorizontal = false;
        }

        // animation
        anim.SetInteger("isHorizontal", (int)direction.x);
        anim.SetInteger("isVertical", (int)direction.y);

        if (anim.GetInteger("isVertical") == 1)
        {
            playerWeaponController.SetFirePos(Vector2.up);
        }
        if (anim.GetInteger("isVertical") == -1)
        {
            playerWeaponController.SetFirePos(Vector2.down);
        }
        if (anim.GetInteger("isHorizontal") == 1)
        {
            playerWeaponController.SetFirePos(Vector2.right);
        }
        if (anim.GetInteger("isHorizontal") == -1)
        {
            playerWeaponController.SetFirePos(Vector2.left);
        }
    }
}
