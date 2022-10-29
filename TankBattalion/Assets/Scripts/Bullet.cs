using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject brickEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Border":
                Destroy(this.gameObject);
                break;
            case "PBullet":
                Destroy(this.gameObject);
                break;
            case "EBullet":
                Destroy(this.gameObject);
                break;
            case "Flag":
                Destroy(this.gameObject);
                break;
            case "Brick":
                GameObject effect = Instantiate(brickEffect, transform.position, transform.rotation);
                Destroy(effect, 0.5f);
                Destroy(this.gameObject);
                break;
            case "MultiBullet":
                Destroy(this.gameObject);
                break;

        }
    }
}
