using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;
    private PlayerMovement movementScript;

    [Header("Animation Thresholds")]
    public float fallThreshold = -0.1f;
    public float jumpThreshold = 0.1f;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>(); // Use InChildren if Animator is on a model
        controller = GetComponent<CharacterController>();
        movementScript = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        UpdateAnimationStates();
    }

    private void UpdateAnimationStates()
    {
        // Safety check
        if (animator == null || controller == null || movementScript == null)
            return;

        // Movement input check
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        animator.SetBool("IsMoving", isMoving);

        // Vertical velocity
        float verticalVelocity = controller.velocity.y;

        // Ground check
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, movementScript.playerHeight * 0.5f + 0.3f, movementScript.whatIsGround);

        // Set airborne states
        animator.SetBool("IsJumping", !isGrounded && verticalVelocity > jumpThreshold);
        animator.SetBool("IsFalling", !isGrounded && verticalVelocity < fallThreshold);
    }

    public void TriggerAttack()
    {
        if (animator == null) return;

        
        if (!animator.GetBool("IsAttacking"))
        {
            animator.SetBool("IsAttacking", true);
        }
    }
    public bool IsAttacking()
    {
        return animator != null && animator.GetBool("IsAttacking");
    }
    public void EndAttack()
    {
        if (animator == null) return;

        animator.SetBool("IsAttacking", false);
    }
}
