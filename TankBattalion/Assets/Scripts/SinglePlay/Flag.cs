using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    SpriteRenderer renderer;

    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.color = new Color(1, 1, 0, 1);

        StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        while(true)
        {
            renderer.color = new Color(1, 1, 0, 0);
            yield return Cashing.YieldInstruction.WaitForSeconds(0.5f);
            renderer.color = new Color(1, 1, 0, 1);
            yield return Cashing.YieldInstruction.WaitForSeconds(0.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("EBullet"))
        {
            SinglePlayManager.Instance.isClear = false;
            SinglePlayManager.Instance.GameOver();
            this.gameObject.SetActive(false);
        }
    }
}
