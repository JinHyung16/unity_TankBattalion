using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkLocalSync : MonoBehaviour
{
    private PlayerInputController playerInputController;
    private AudioSource playerAudio;

    // effect prefab
    [SerializeField] private GameObject boomEffect;

    // audio
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip exploSound;

    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private Transform playerTransform;
    public float StateSyncTimer = 0.0f;

    private float stateSyncTimer;

    private void Start()
    {
        playerAudio = GetComponent<AudioSource>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerInputController = GetComponent<PlayerInputController>();
        playerTransform = rigidbody2D.GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        if (stateSyncTimer <= 0)
        {
            GameManager.Instance.SendMatchState(
                OpCodes.VelocityAndPosition, 
                MatchDataJson.VelocityAndPosition(rigidbody2D.velocity, playerTransform.position));

            stateSyncTimer = StateSyncTimer;
        }

        stateSyncTimer -= Time.deltaTime;

        if (!playerInputController.InputChange)
        {
            return;
        }

        GameManager.Instance.SendMatchState(
            OpCodes.Input,
            MatchDataJson.Input(playerInputController.HorizontalInput, playerInputController.VerticalInput, playerInputController.Fire));
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

    public void DieSound()
    {
        playerAudio.clip = exploSound;
        playerAudio.Play();
    }
}
