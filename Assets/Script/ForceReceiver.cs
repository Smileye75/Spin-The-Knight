using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceReceiver : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float fallMultiplier = 2.5f;


    private float verticalVelocity;
    private float gravity;

    public Vector3 movement => Vector3.up * verticalVelocity;

    void Update()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0f)
        {
            verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public void CancelJump()
    {
        if (verticalVelocity > 0f)
        {
            verticalVelocity *= 0.5f;
        }
    }

    public void Jump(float jumpForce)
    {
        verticalVelocity += jumpForce;
    }
    public void SetGravity(float gravityValue)
    {
        gravity = gravityValue;
    }

}
