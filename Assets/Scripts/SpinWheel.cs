using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpinWheel : MonoBehaviour
{
    public float StopPower;
    private Rigidbody2D rbody;
    private bool isRotating = false;
    int rewardsCount = 12;
    float rewardsAngle;

    public GameObject wheelParent;

    private List<int> itemsList;

    public void Init(List<int> itemsList)
    {
        rbody = GetComponent<Rigidbody2D>();
        rewardsAngle = 360f / rewardsCount;
        this.itemsList = itemsList;
        Rotate(/*Random.RandomRange(1000,2000)*/);
    }

    private void FixedUpdate()
    {
        if (rbody.angularVelocity > 0)
        {
            rbody.angularVelocity -= StopPower * Time.fixedDeltaTime;
            rbody.angularVelocity = Mathf.Clamp(rbody.angularVelocity, 0, 2000);
        }

        if (rbody.angularVelocity == 0 && isRotating)
        {
            GetReward();
            isRotating = false;
        }
    }

    public void Rotate(float RotatePower = 150000)
    {
        if (!isRotating)
        {
            rbody.angularVelocity = RotatePower * Mathf.Deg2Rad;
            isRotating = true;
        }
    }

    public void GetReward()
    {
        // centers the wheel on the reward it landed on (there are 12 rewards)
        float angle = Mathf.Round(transform.eulerAngles.z / rewardsAngle) * rewardsAngle;
        transform.eulerAngles = new Vector3(0, 0, angle);

        // Get all gameobjects with the tag ItemSlot and find the one with the highest y coordinate
        GameObject reward = GameObject.FindGameObjectsWithTag("ItemSlot").OrderByDescending(go => go.transform.position.y).First();
        int rewardId = int.Parse(reward.name);

        //int rewardSlot = (int)(angle / rewardsAngle) + 1 + 2;
        Win(rewardId);
    }

    public void Win(int rewardId)
    {
        /*Debug.Log($"ItemList.count={itemsList.Count}");
        Debug.Log(string.Join(", ", itemsList.ConvertAll(i => i.ToString()).ToArray()));
        Debug.Log($"slot={rewardSlot}");*/
        Debug.Log($"itemId={rewardId}");
        // Uncomment the line below if you want to enable the SpinWin coroutine
        StartCoroutine(GameManager.Instance.SpinWin(rewardId, wheelParent));
    }
}
