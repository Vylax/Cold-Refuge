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

    private int _health = 100;
    public bool IsDead => _health <= 0;

    public bool isKnockedBack = false;

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
        if (IsDead || isKnockedBack)
        {
            aiPath.destination = transform.position;
            return;
        }

        // Update the target position (in case the player moves)
        if (player != null)
        {
            seeker.StartPath(transform.position, playerPos, OnPathComplete, GetGraphMask(playerPos));
        }
    }

    private void FixedUpdate()
    {
        if (IsDead || isKnockedBack)
        {
            aiPath.destination = transform.position;
            return;
        }

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

        if (Vector3.Distance(transform.position, playerPos) <= attackRange && !isAttacking && !GetPlayer().IsDead)
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

    public void TakeDamage(float damage)
    {
        _health -= Mathf.RoundToInt(damage);
        Debug.Log("Enemy took " + damage + " damage. Health is now " + _health);

        if (_health <= 0)
        {
            Die();
        }
        else
        {
            // set animator trigger "isHurt"
            GetComponent<Animator>().SetTrigger("isHurt");
        }
    }

    public void Die()
    {
        Debug.Log("Enemy died.");

        // play death animation
        GetComponent<Animator>().SetTrigger("isDead");
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2f);

        // TODO: decrease WaveSystem enemy count by 1 here
    }
}
