using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = Vector3.one;

    void Start()
    {

    }

    void Update()
    {
        transform.position = player.position + offset;
    }
}
