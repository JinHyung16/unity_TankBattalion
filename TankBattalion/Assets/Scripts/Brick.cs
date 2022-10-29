using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Brick : MonoBehaviour
{
    Tilemap tilemap;
    AudioSource audio;

    private Vector3Int breakPos;

    public AudioClip exploSound;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PBullet") || collision.CompareTag("EBullet")
            || collision.CompareTag("MultiBullet"))
        {
            PlaySound("Explo");
            breakPos = tilemap.WorldToCell(collision.transform.position);
            if (tilemap.GetTile(breakPos) == null)
            {
                if (tilemap.GetTile(new Vector3Int(breakPos.x + 1, breakPos.y, breakPos.z)) != null)
                {
                    breakPos.x += 1;
                }
                if (tilemap.GetTile(new Vector3Int(breakPos.x - 1, breakPos.y, breakPos.z)) != null)
                {
                    breakPos.x -= 1;
                }
                if (tilemap.GetTile(new Vector3Int(breakPos.x, breakPos.y + 1, breakPos.z)) != null)
                {
                    breakPos.y += 1;
                }
                if (tilemap.GetTile(new Vector3Int(breakPos.x, breakPos.y - 1, breakPos.z)) != null)
                {
                    breakPos.y -= 1;
                }
            }
            tilemap.SetTile(breakPos, null);
        }
    }

    private void PlaySound(string name)
    {
        switch(name)
        {
            case "Explo":
                audio.clip = exploSound;
                break;
        }

        audio.Play();
    }
}
