using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Utils;

public class JoystickController : MonoBehaviour
{
    public VariableJoystick variableJoystick;

    public float speed;
    public float minSpeedThreshold;
    public Rigidbody2D rb;

    public bool canMove = true;

    private void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        InitJoystick();
    }

    public void FixedUpdate()
    {
        if (canMove) Move();
    }

    /// <summary>
    /// Reset the Joystick to its default settings
    /// </summary>
    private void InitJoystick()
    {
        variableJoystick.SetMode(JoystickType.Floating);
        variableJoystick.AxisOptions = AxisOptions.Both;
    }

    /// <summary>
    /// Update the rigidbody2D velocity according to the joystick direction
    /// </summary>
    private void Move()
    {
        Vector3 direction = CustomClamp(variableJoystick.Horizontal, variableJoystick.Vertical);
        rb.velocity = (direction.magnitude > minSpeedThreshold) ? direction * speed : Vector3.zero;
    }
}
