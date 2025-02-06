using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    public Rigidbody playerRB;
    public float playerMovementSpeed;
    public InputActionReference playerAction;
    public Camera mainCamera;
    public float rotationSpeed = 360f; // Degrees per second
    public float cooldownTime = 2f; // Cooldown in seconds

    private Vector3 playerMovement;
    private bool isRotating = false;
    private float cooldownTimer = 0f;

    void Update()
    {
        playerMovement = playerAction.action.ReadValue<Vector3>();

        // Rotate toward the mouse cursor if not in cooldown
        if (!isRotating)
        {
            RotateTowardsMouse();
        }

        // Check for 360-degree rotation if not on cooldown
        if (Mouse.current.leftButton.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            StartCoroutine(Rotate360());
        }

        // Cooldown timer countdown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime; // Decrease the cooldown over time
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

    private IEnumerator Rotate360()
    {
        isRotating = true;
        float rotatedAngle = 0f;

        while (rotatedAngle < 360f)
        {
            float rotationStep = rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotationStep);
            rotatedAngle += rotationStep;
            yield return null; // Wait for the next frame
        }

        // Start cooldown after rotation
        cooldownTimer = cooldownTime;

        isRotating = false;
    }
}
