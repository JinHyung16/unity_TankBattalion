using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkLocalSync : MonoBehaviour
{
    private MatchManager matchManager;
    private PlayerInputController playerInputController;
    private AudioSource playerAudio;

    // effect prefab
    [SerializeField] private GameObject boomEffect;

    [SerializeField] private AudioClip exploSound;
    private Rigidbody2D rigid2D;

    private Transform playerTransform;

    public float StateFrequency = 0.1f;
    private float stateSyncTimer = 0.0f;

    private void Start()
    {
        matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();

        playerInputController = GetComponent<PlayerInputController>();

        rigid2D = GetComponentInChildren<Rigidbody2D>();
        playerTransform = rigid2D.GetComponent<Transform>();

        playerAudio = GetComponentInChildren<AudioSource>();
    }

    private void LateUpdate()
    {
        if (stateSyncTimer <= 0)
        {
            matchManager.SendMatchState(
                OpCodes.VelocityAndPosition,
                MatchDataJson.VelocityAndPosition(rigid2D.velocity, playerTransform.position)
                );

            stateSyncTimer = StateFrequency;
        }

        stateSyncTimer -= Time.deltaTime;

        if (!playerInputController.InputChange)
        {
            return;
        }

        matchManager.SendMatchState(
            OpCodes.Input,
            MatchDataJson.Input(playerInputController.HorizontalInput, playerInputController.VerticalInput, playerInputController.Fire)
            );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MultiBullet"))
        {
            // ÃÑ¾Ë Áö¿ì±â
            Destroy(collision.gameObject);

            // effect
            GameObject effect = Instantiate(boomEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);

            // sound
            DieSound();

            matchManager.LocalPlayerDied(this.gameObject);
        }
    }
    public void DieSound()
    {
        playerAudio.clip = exploSound;
        playerAudio.Play();
    }
}
