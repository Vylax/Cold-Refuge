using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Player GetPlayer() => player.GetComponent<Player>();
    private Seeker seeker;
    private AIPath aiPath;

    private bool isMoving => aiPath.desiredVelocity.magnitude > 0.01f;

    public float attackRange = 1f; // The range at which the enemy will attack the player
    public float attackCooldown = 1f; // The time between each attack

    private bool isAttacking = false; // Whether the enemy is currently attacking or not

    public int damage = 10; // The amount of damage the enemy deals to the player

    private Vector3 playerPos => player.GetComponent<Collider2D>().ClosestPoint(transform.position);

    void Start()
    {
        seeker = GetComponent<Seeker>();
        aiPath = GetComponent<AIPath>();

        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;

        // Set the target to the player's position
        if (player != null)
        {
            seeker.StartPath(transform.position, playerPos, OnPathComplete, GetGraphMask(playerPos));
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
            seeker.StartPath(transform.position, playerPos, OnPathComplete, GetGraphMask(playerPos));
        }
    }

    private void FixedUpdate()
    {
        if (aiPath.desiredVelocity.x >= 0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (aiPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        // Set the animation parameters depending on wether the enemy is moving or not

        GetComponent<Animator>().SetBool("isMoving", isMoving);

        if (Vector3.Distance(transform.position, playerPos) <= attackRange && !isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    private int GetGraphMask(Vector3 position)
    {
        return WorldGenerator.GetGraphMask(position);
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        GetComponent<Animator>().SetTrigger("Attack");

        GetPlayer().TakeDamage(damage);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        GetComponent<Animator>().SetTrigger("StopAttack");
    }

}
