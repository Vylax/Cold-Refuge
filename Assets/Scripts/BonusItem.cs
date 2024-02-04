using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusItem : MonoBehaviour
{
    public int scoreValue = 50;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BonusManager.Instance.PickUpBonus();

            // TODO: play sound
            // TODO: play animation

            GameManager.Instance.scoreSystem.AddScore(scoreValue);

            Destroy(gameObject);
        }
    }
}
