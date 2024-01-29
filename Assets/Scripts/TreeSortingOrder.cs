using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSortingOrder : MonoBehaviour
{
    private GameObject[] trees;

    private void Start()
    {
        SortTrees();
    }

    private void SortTrees()
    {
        // Collect all trees GameObjects
        trees = GameObject.FindGameObjectsWithTag("Tree");

        // Asserts there is at least one tree in the scene, otherwise going on is pointless
        if (trees.Length == 0) return;

        // Get the base sorting order value from player if it exists, otherwise default to 100
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        int baseOrder = (player != null) ? player.GetComponent<SpriteRenderer>().sortingOrder : 100;

        // Get the highest y coordinates of tree bottom amongst all trees objects
        float maxY = -Mathf.Infinity;
        for(int i = 0; i < trees.Length; i++)
        {
            // Get the Collider2D component attached to the GameObject
            Collider2D collider = trees[i].GetComponent<Collider2D>();

            if (collider != null)
            {
                // Get the highest y coordinate of the current tree and update the global max y coordinate if needed
                float currY = collider.bounds.max.y;
                if (currY > maxY) maxY = currY;
            }
            else
            {
                Debug.LogError($"Collider2D component not found on the GameObject called {trees[i].name}.");
            }
        }

        foreach(GameObject tree in trees)
        {
            tree.GetComponent<SpriteRenderer>().sortingOrder = baseOrder + Mathf.CeilToInt(maxY - tree.transform.position.y);
        }

        Debug.Log("Trees where sorted successfully");
    }
}
