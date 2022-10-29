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

    private Rigidbody2D rigidbody2D;
    private Transform playerTransform;

    public float StateSyncTimer = 0.1f;
    private float stateSyncTimer;

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
            
            GameManager.Instance.SendMatchState(OpCodes.VelocityAndPosition, 
                MatchDataJson.VelocityAndPosition(rigidbody2D.velocity, playerTransform.position));

            stateSyncTimer = StateSyncTimer;
        }

        stateSyncTimer -= Time.deltaTime;

        if (!playerInputController.InputChange)
        {
            return;
        }

        GameManager.Instance.SendMatchState(OpCodes.Input,
            MatchDataJson.Input(playerInputController.HorizontalInput, playerInputController.VerticalInput, playerInputController.Fire));
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

    public void DieSound()
    {
        playerAudio.clip = exploSound;
        playerAudio.Play();
    }
}
