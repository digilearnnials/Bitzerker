using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputData : MonoBehaviour
{
    private Vector2 moveInput = default;
    private bool jumpInput = default;

    public Vector2 MoveInput => moveInput;

    public bool JumpInput
    {
        get => jumpInput;
        set => jumpInput = value;
    }
    
    private void Update()
    {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        moveInput = Vector2.ClampMagnitude(moveInput, 1);

        jumpInput |= Input.GetButtonDown("Jump");
    }
}
