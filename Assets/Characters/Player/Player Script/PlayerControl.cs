using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    public Rigidbody playerRB;
    public float playerMovementSpeed;
    public InputActionReference playerAction;
    public Camera mainCamera;
    public float cooldownTime = 2f; // Cooldown in seconds

    private Vector3 playerMovement;
    private bool isRotating = false;
    private float cooldownTimer = 0f;
    public Animator animator; // Reference to Animator on a separate GameObject

    void Start()
    {
        if (animator != null)
        {
            animator.enabled = false; // Disable Animator initially
        }
    }

    void Update()
    {
        playerMovement = playerAction.action.ReadValue<Vector3>();

        // Rotate toward the mouse cursor ONLY if not rotating (animation playing)
        if (!isRotating)
        {
            RotateTowardsMouse();
        }

        // If left mouse button is clicked & cooldown is over, trigger the animation
        if (Mouse.current.leftButton.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            TriggerRotationAnimation();
        }

        // Handle cooldown timer
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        playerRB.velocity = new Vector3(playerMovement.x * playerMovementSpeed, 0, playerMovement.z * playerMovementSpeed);
    }

    private void RotateTowardsMouse()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float hitDistance))
        {
            Vector3 targetPoint = ray.GetPoint(hitDistance);
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    public void TriggerRotationAnimation()
    {
        if (animator != null)
        {
            animator.enabled = true; // Enable Animator before playing animation
            animator.SetTrigger("Spin"); // Replace with your actual animation trigger
        }

        isRotating = true; // Stop the player from following the mouse during animation
        cooldownTimer = cooldownTime; // Start cooldown
    }

    // This function should be called at the end of the animation
    public void StopAnimation()
    {
        isRotating = false; // Allow mouse rotation again

        if (animator != null)
        {
            animator.enabled = false; // Disable Animator after animation finishes
        }
    }
}
