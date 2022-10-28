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

    private PlayerMovementController playerMovementController;
    private PlayerWeaponController playerWeaponController;
    // effect prefab
    [SerializeField] private GameObject boomEffect;

    // audio
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip exploSound;

    public RemotePlayerNetworkData NetworkData;

    // interpolation to the player move speed
    public float LerpTime = 0.05f;

    public float playerMoveSpeed;
    public Transform playerTransform;

    private float lerpTimer;
    private Vector3 lerpFromPosition;
    private Vector3 lerpToPosition;
    private bool lerpPosition;

    private void Start()
    {
        //playerAnim = this.gameObject.GetComponent<Animator>();
        playerAudio = this.gameObject.GetComponent<AudioSource>();

        playerMovementController = GetComponentInChildren<PlayerMovementController>();
        playerWeaponController = GetComponentInChildren<PlayerWeaponController>();

        GameManager.Instance.SetDisplayName(this.gameObject.name);

        HughServer.GetInstace.Socket.ReceivedMatchState += EnqueueOnReceivedMatchState;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EBullet"))
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
        if (matchState.UserPresence.SessionId != NetworkData.User.SessionId)
        {
            return;
        }

        // Decide what to do based on the Operation Code of the incoming state data as defined in OpCodes.
        switch (matchState.OpCode)
        {
            case OpCodes.VelocityAndPosition:
                UpdateSpeedAndPositionFromState(matchState.State);
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


    private void UpdateSpeedAndPositionFromState(byte[] state)
    {
        var stateDictionary = GetStateAsDictionary(state);

        playerMoveSpeed = float.Parse(stateDictionary["moveSpeed"]);

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

        playerMovementController.SetHorizontalMovement(float.Parse(stateDictionary["horizontal"]), float.Parse(stateDictionary["vertical"]));
        
        if (bool.Parse(stateDictionary["fire"]))
        {
            playerWeaponController.AttackFire();
        }
    }
    #endregion
}
