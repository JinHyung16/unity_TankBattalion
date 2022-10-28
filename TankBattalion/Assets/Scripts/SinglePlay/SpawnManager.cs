using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // enemy respawn (»ó ÇÏ ÁÂ ¿ì 4°³ ±ÍÅüÀÌ TransPos ¹Þ¾Æ¼­ ³Ö¾î³õ±â)
    public GameObject enemy;

    [SerializeField] private Transform[] enemyRespawnPos;
    [SerializeField] private float enemyRespawnTime = 4.0f;
    private float curTime = 0.0f;

    private void Start()
    {
        if (enemy == null)
        {
            enemy = Resources.Load<GameObject>("Enemy");
            Instantiate(enemy);
        }

        if (enemyRespawnPos == null)
        {
            var enemySpawns = this.gameObject.GetComponentsInChildren<GameObject>();
            for (int i = 0; i < enemySpawns.Length; i++)
            {
                enemyRespawnPos[i] = enemySpawns[i].transform;
            }
        }
    }
    private void Update()
    {
        if (SinglePlayManager.Instance.isStart)
        {
            curTime += Time.deltaTime;
            if ((curTime > enemyRespawnTime) && !SinglePlayManager.Instance.isOver)
            {
                EnemyRespawn();
                curTime = 0.0f;
            }

            if (SinglePlayManager.Instance.isDefend)
            {
                enemyRespawnTime = 3.0f;
            }
            else
            {
                enemyRespawnTime = 5.0f;
            }
        }
    }

    private void EnemyRespawn()
    {
        int posIndex = Random.Range(0, 4);
        GameObject e = Instantiate(enemy, enemyRespawnPos[posIndex]);
        e.name = "Enemy";
    }
}
