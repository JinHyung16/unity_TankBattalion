using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    private SpriteRenderer renderer;

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
            SinglePlayManager.GetInstance.isClear = false;
            SinglePlayManager.GetInstance.GameOver();
            this.gameObject.SetActive(false);
        }
    }
}
