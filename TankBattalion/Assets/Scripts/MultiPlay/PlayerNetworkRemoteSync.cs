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
    public RemotePlayerNetworkData NetWorkData;

    private MatchManager matchManager;
    private PlayerMovementController playerMovementController;
    private PlayerWeaponController playerWeaponController;
    // effect prefab
    [SerializeField] private GameObject boomEffect;

    // audio
    [SerializeField] private AudioClip exploSound;

    // interpolation to the player move speed
    public float LerpTime = 0.05f;

    private Rigidbody2D rigid2D;
    private Transform playerTransform;

    private float lerpTimer;
    private Vector3 lerpFromPosition;
    private Vector3 lerpToPosition;
    private bool lerpPosition;

    private void Start()
    {
        matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();

        //playerAnim = this.gameObject.GetComponent<Animator>();
        playerAudio = GetComponentInChildren<AudioSource>();

        playerMovementController = GetComponentInChildren<PlayerMovementController>();
        playerWeaponController = GetComponentInChildren<PlayerWeaponController>();
        rigid2D = GetComponentInChildren<Rigidbody2D>();
        playerTransform = rigid2D.GetComponent<Transform>();

        matchManager.hughServer.Socket.ReceivedMatchState += EnqueueOnReceivedMatchState;
    }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MultiBullet"))
        {
            // 총알 지우기
            Destroy(collision.gameObject);

            // sound
            DieSound();

            // effect
            GameObject effect = Instantiate(boomEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);

            matchManager.LocalPlayerDied(this.gameObject);
        }
    }

    public void DieSound()
    {
        playerAudio.clip = exploSound;
        playerAudio.Play();
    }
    private void EnqueueOnReceivedMatchState(IMatchState matchState)
    {
        var mainThread = UnityMainThreadDispatcher.Instance();
        mainThread.Enqueue(() => OnReceivedMatchState(matchState));
    }

    private void OnReceivedMatchState(IMatchState matchState)
    {
        Debug.Log("OnReceivedMatchState 연결");
        // If the incoming data is not related to this remote player, ignore it and return early.
        if (matchState.UserPresence.SessionId != NetWorkData.User.SessionId)
        {
            return;
        }

        // Decide what to do based on the Operation Code of the incoming state data as defined in OpCodes.
        switch (matchState.OpCode)
        {
            case OpCodes.VelocityAndPosition:
                UpdateVelocityAndPositionFromState(matchState.State);
                break;
            case OpCodes.Input:
                SetInputFromState(matchState.State);
                break;
            case OpCodes.Died:
                DieSound();
                break;
            default:
                break;
        }
    }

    private IDictionary<string, string> GetStateAsDictionary(byte[] state)
    {
        return Encoding.UTF8.GetString(state).FromJson<Dictionary<string, string>>();
    }
    public void SetInputFromState(byte[] state)
    {
        var myState = GetStateAsDictionary(state);

        playerMovementController.SetDirectionMovement(float.Parse(myState["hor_input"]), float.Parse(myState["ver_input"]));

        if (bool.Parse(myState["fire"]))
        {
            playerWeaponController.AttackFire();
        }
    }

    public void UpdateVelocityAndPositionFromState(byte[] state)
    {
        var myState = GetStateAsDictionary(state);

        rigid2D.velocity = new Vector2(float.Parse(myState["velocity_x"]), float.Parse(myState["velocity_y"]));

        var pos = new Vector3(
            float.Parse(myState["position_x"]),
            float.Parse(myState["position_y"]),
            0);

        lerpFromPosition = playerTransform.position;
        lerpToPosition = pos;
        lerpTimer = 0;
        lerpPosition = true;
    }
}
