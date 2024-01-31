using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int _health = 100;

    public void TakeDamage(int damage)
    {
        _health -= damage;
        Debug.Log("Player took " + damage + " damage. Health is now " + _health);

        if (_health <= 0)
        {
            Die();
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
}
