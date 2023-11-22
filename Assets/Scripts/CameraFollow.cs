using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float offset = -10;

    public bool canFollow = true;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if(canFollow) FollowPlayer();
    }

    /// <summary>
    /// Place the camera above the player if it is declared
    /// </summary>
    private void FollowPlayer()
    {
        if (!player) return;
        transform.position = player.position + Vector3.forward * offset;
    }
}
