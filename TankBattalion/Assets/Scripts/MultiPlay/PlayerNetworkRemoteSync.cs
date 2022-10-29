using System.Collections.Generic;
using UnityEngine;
using Nakama;
using Nakama.TinyJson;
using System.Text;
using UnityEngine.Audio;

public class PlayerNetworkRemoteSync : MonoBehaviour
{
    //Animator playerAnim;
    AudioSource playerAudio;
    public RemotePlayerNetworkData netWorkData;

    private PlayerMovementController playerMovementController;
    private PlayerWeaponController playerWeaponController;
    // effect prefab
    [SerializeField] private GameObject boomEffect;

    // audio
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip exploSound;

    // interpolation to the player move speed
    public float LerpTime = 0.05f;

    public Rigidbody2D rigidBody2D;
    public Transform playerTransform;

    private float lerpTimer;
    private Vector3 lerpFromPosition;
    private Vector3 lerpToPosition;
    private bool lerpPosition;

    private void Start()
    {
        HughServer.GetInstace.Socket.ReceivedMatchState += EnqueueOnReceivedMatchState;

        //playerAnim = this.gameObject.GetComponent<Animator>();
        playerAudio = GetComponentInChildren<AudioSource>();

        playerMovementController = GetComponentInChildren<PlayerMovementController>();
        playerWeaponController = GetComponentInChildren<PlayerWeaponController>();

        rigidBody2D = GetComponentInChildren<Rigidbody2D>();
        playerTransform = rigidBody2D.GetComponent<Transform>();

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MultiBullet"))
        {
            // effect
            GameObject effect = Instantiate(boomEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);

            MultiPlayManager.Instance.HealthDown();

            // ÃÑ¾Ë Áö¿ì±â
            Destroy(collision.gameObject);

            // sound
            DieSound();
        }
    }

    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MultiBullet"))
        {
            // effect
            GameObject effect = Instantiate(boomEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);

            MultiPlayManager.Instance.HealthDown();

            // ÃÑ¾Ë Áö¿ì±â
            Destroy(collision.gameObject);

            // sound
            DieSound();
        }
    }
    */

    private void OnDestroy()
    {
        HughServer.GetInstace.Socket.ReceivedMatchState -= EnqueueOnReceivedMatchState;
    }

    public void DieSound()
    {
        playerAudio.clip = exploSound;
        playerAudio.Play();
    }

    #region Nakama Match Function
    private void LateUpdate()
    {
        if (!lerpPosition)
        {
            return;
        }

        playerTransform.position = Vector3.Lerp(lerpFromPosition, lerpToPosition, lerpTimer / LerpTime);
        lerpTimer += Time.deltaTime;

        if (lerpTimer >= LerpTime)
        {
            playerTransform.position = lerpToPosition;
            lerpPosition = false;
        }
    }

    private void EnqueueOnReceivedMatchState(IMatchState matchState)
    {
        var mainThread = UnityMainThreadDispatcher.Instance();
        mainThread.Enqueue(() => OnReceivedMatchState(matchState));
    }

    private void OnReceivedMatchState(IMatchState matchState)
    {
        // If the incoming data is not related to this remote player, ignore it and return early.
        if (matchState.UserPresence.SessionId != netWorkData.User.SessionId)
        {
            return;
        }

        // Decide what to do based on the Operation Code of the incoming state data as defined in OpCodes.
        switch (matchState.OpCode)
        {
            case OpCodes.VelocityAndPosition:
                UpdateVelocityAndPosition(matchState.State);
                break;
            case OpCodes.Input:
                SetInputFromState(matchState.State);
                break;
            case OpCodes.Died:
                 //playerMovementController.PlayDeathAnimation();
                 break;
            default:
                break;
        }
    }

    private IDictionary<string, string> GetStateAsDictionary(byte[] state)
    {
        return Encoding.UTF8.GetString(state).FromJson<Dictionary<string, string>>();
    }


    private void UpdateVelocityAndPosition(byte[] state)
    {
        var stateDictionary = GetStateAsDictionary(state);

        rigidBody2D.velocity = new Vector2(float.Parse(stateDictionary["velocity.x"]), 
            float.Parse(stateDictionary["velocity.y"])); ;

        var position = new Vector2(
            float.Parse(stateDictionary["position.x"]),
            float.Parse(stateDictionary["position.y"]));

        lerpFromPosition = playerTransform.position;
        lerpToPosition = position;
        lerpTimer = 0;
        lerpPosition = true;
    }

    private void SetInputFromState(byte[] state)
    {
        var stateDictionary = GetStateAsDictionary(state);

        playerMovementController.SetDirectionMovement(float.Parse(stateDictionary["horizontalInput"]), 
            float.Parse(stateDictionary["verticalInput"]));
        
        if (bool.Parse(stateDictionary["fire"]))
        {
            playerWeaponController.AttackFire();
        }
    }
    #endregion
}
