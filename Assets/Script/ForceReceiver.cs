using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceReceiver : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float knockbackSmoothTime = 0.3f;
    [SerializeField] private float knockbackStrength = 5f;
    [SerializeField] private float upwardForce = 6f;


    private float verticalVelocity;
    private float gravity;
    private Vector3 impact;
    private Vector3 dampingVelocity;

    public Vector3 movement => impact + Vector3.up * verticalVelocity;

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

        impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dampingVelocity, knockbackSmoothTime);
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
    public void ApplyKnockback(Vector3 sourcePosition)
    {
        Vector3 direction = (transform.position - sourcePosition).normalized;
        direction.y = 0f;

        impact += direction * knockbackStrength;
        verticalVelocity = upwardForce;
    }

}
