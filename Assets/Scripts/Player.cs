using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int _health = 100;
    public bool IsDead => _health <= 0;

    private bool isMoving => GetComponent<Rigidbody2D>().velocity.magnitude > 0.01f;

    public bool isAttacking = false;
    public float bladeRange = 2f;
    public float bladeAttackCooldown = 3f;
    public float bladeDamages = 34f;
    public float knockbackForce = 1f;

    public void TakeDamage(int damage)
    {
        _health -= damage;
        Debug.Log("Player took " + damage + " damage. Health is now " + _health);

        if (_health <= 0)
        {
            // set animator trigger "isDead"
            GetComponent<Animator>().SetTrigger("isDead");
            Die();
        }
        else
        {
            // set animator trigger "isHurt"
            GetComponent<Animator>().SetTrigger("isHurt");
        }
    }

    public void Heal(int amount)
    {
        _health += amount;
        Debug.Log("Player healed " + amount + " health. Health is now " + _health);

        if (_health > 100)
        {
            _health = 100;
        }
    }

    public void Die()
    {
        Debug.Log("Player died.");

        GameManager.Instance.GameOver();
    }

    private void FixedUpdate()
    {
        if (GetComponent<Rigidbody2D>().velocity.x >= 0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (GetComponent<Rigidbody2D>().velocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Set the animation parameters depending on wether the enemy is moving or not

        GetComponent<Animator>().SetBool("isMoving", isMoving);


        // if enemies are within bladerange and player is not attacking, attack automatically
        if (!isAttacking)
        {
            Collider2D[] hitEnemies = GetEnemiesWithinRange(bladeRange);
            if (hitEnemies.Length > 0)
            {
                StartCoroutine(Attack(hitEnemies));
            }
        }


    }

    private IEnumerator Attack(Collider2D[] hitEnemies)
    {
        isAttacking = true;
        GetComponent<Animator>().SetTrigger("Attack");

        foreach (Collider2D enemy in hitEnemies)
        {
            // skip enemy if it's dead
            if (enemy.GetComponent<Enemy>().IsDead)
            {
                continue;
            }

            Vector2 knockbackDirection = enemy.transform.position - transform.position;

            StartCoroutine(Knockback(enemy.GetComponent<Enemy>(), knockbackDirection));
        }

        yield return new WaitForSeconds(bladeAttackCooldown);
        isAttacking = false;
    }

    private Collider2D[] GetEnemiesWithinRange(float range)
    {
        return Physics2D.OverlapCircleAll(GetComponent<Collider2D>().bounds.center, range, LayerMask.GetMask("Enemy"));
    }

    // coroutine that knock the given enemy back along the given direction smoothly without using rigidbody2D
    private IEnumerator Knockback(Enemy enemy, Vector2 direction)
    {
        yield return new WaitForSeconds(0.3f);
        enemy.isKnockedBack = true;
        enemy.TakeDamage(bladeDamages);
        float knockbackDuration = 0.2f;
        float timer = 0;

        while (timer < knockbackDuration)
        {
            enemy.transform.position += (Vector3)direction.normalized * knockbackForce * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        enemy.isKnockedBack = false;
    }

}
