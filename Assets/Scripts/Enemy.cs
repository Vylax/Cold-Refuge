using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    private Seeker seeker;
    private AIPath aiPath;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        aiPath = GetComponent<AIPath>();

        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;

        // Set the target to the player's position
        if (player != null)
        {
            seeker.StartPath(transform.position, player.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        // Handle the path completion if needed
    }

    void Update()
    {
        // Update the target position (in case the player moves)
        if (player != null)
        {
            seeker.StartPath(transform.position, player.position, OnPathComplete);
        }
    }
}
