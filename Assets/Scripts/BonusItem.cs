using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.bonusManager.PickUpBonus();

            // TODO: play sound
            // TODO: play animation

            Destroy(gameObject);
        }
    }
}
