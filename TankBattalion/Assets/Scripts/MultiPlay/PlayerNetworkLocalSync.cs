using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkLocalSync : MonoBehaviour
{
    private PlayerInputController playerInputController;
    private AudioSource playerAudio;

    // effect prefab
    [SerializeField] private GameObject boomEffect;

    [SerializeField] private AudioClip exploSound;

    private Rigidbody2D rigidbody2D;
    private Transform playerTransform;

    public float StateSyncTimer = 0.1f;
    private float stateSyncTimer = 0.0f;

    private void Start()
    {
        playerAudio = GetComponentInChildren<AudioSource>();
        rigidbody2D = GetComponentInChildren<Rigidbody2D>();
        playerInputController = GetComponent<PlayerInputController>();
        playerTransform = rigidbody2D.GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        if (stateSyncTimer <= 0)
        {
            
            GameManager.GetInstance.SendMatchState(OpCodes.VelocityAndPosition, 
                MatchDataJson.VelocityAndPosition(rigidbody2D.velocity, playerTransform.position));

            stateSyncTimer = StateSyncTimer;
        }

        stateSyncTimer -= Time.deltaTime;

        if (!playerInputController.InputChange)
        {
            return;
        }

        GameManager.GetInstance.SendMatchState(OpCodes.Input,
            MatchDataJson.Input(playerInputController.HorizontalInput, playerInputController.VerticalInput, playerInputController.Fire));
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

            GameManager.GetInstance.LocalPlayerDied(this.gameObject);
        }
    }
    public void DieSound()
    {
        playerAudio.clip = exploSound;
        playerAudio.Play();
    }
}
