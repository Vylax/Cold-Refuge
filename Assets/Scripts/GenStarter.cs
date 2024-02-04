using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenStarter : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.GetComponent<WorldGenerator>().StartGen();
    }
}
