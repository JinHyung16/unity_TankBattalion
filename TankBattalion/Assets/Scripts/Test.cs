using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private Transform astar;

    private void Start()
    {
        astar.parent = null;
        astar.transform.position = new Vector3(-0.27f, 0, 0);
    }
}
