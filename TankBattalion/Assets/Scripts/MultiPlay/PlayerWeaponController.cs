using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    // bullet prefab
    [SerializeField] private GameObject pBullet;
    private Vector2 firePos;

    [SerializeField] private float attackTime = 1.5f;
    [SerializeField] private float gunDelay = 1.5f;
   
    private void Update()
    {
        gunDelay -= Time.deltaTime;
    }

    public void AttackFire()
    {
        if (gunDelay <= 0)
        {
            FireBullet();
            gunDelay = attackTime;
        }
    }

    private void FireBullet()
    {
        GameObject bullet = Instantiate(pBullet, transform.position, transform.rotation);
        Rigidbody2D bulletRigid = bullet.GetComponent<Rigidbody2D>();
        bulletRigid.AddForce(firePos * 3.0f, ForceMode2D.Impulse);

        // sound
        //PlaySound("Fire");
    }

    public void SetFirePos(Vector2 pos)
    {
        firePos = pos;
    }
}
