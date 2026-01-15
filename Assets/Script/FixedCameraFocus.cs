using UnityEngine;

public class FixedCameraFocus : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStateMachine playerSM; // drag your player root
    [SerializeField] private Transform player;            // optional; auto from playerSM if empty

    [Header("Axis Switch")]
    public bool followXAxis = false;

    [Header("Locking")]
    public float fixedX = 0f;
    public float fixedZ = 0f;

    [Header("Y Follow (Grounded Only)")]
    [SerializeField] private float ySmooth = 10f;         // higher = snappier
    [SerializeField] private float ySnapThreshold = 0.75f; // snap if landing on a much higher/lower platform
    [SerializeField] private float yExtraOffset = 0f;      // if you want to bias the anchor up/down

    private float currentY;
    private bool wasGrounded;

    void Awake()
    {
        if (!playerSM && player) playerSM = player.GetComponent<PlayerStateMachine>();
        if (!player && playerSM) player = playerSM.transform;
    }

    void Start()
    {
        if (player) currentY = player.position.y + yExtraOffset;
        wasGrounded = IsGrounded();
    }

    void LateUpdate()
    {
        if (!player || !playerSM || playerSM.characterController == null) return;

        bool grounded = IsGrounded();
        float targetY = player.position.y + yExtraOffset;

        // Update Y only while grounded
        if (grounded)
        {
            // If we JUST landed and the height changed a lot, snap to avoid weird delay
            bool justLanded = !wasGrounded && grounded;
            if (justLanded && Mathf.Abs(targetY - currentY) > ySnapThreshold)
            {
                currentY = targetY;
            }
            else
            {
                currentY = Mathf.Lerp(currentY, targetY, ySmooth * Time.deltaTime);
            }
        }

        if(followXAxis)
        {
            // Z locked, X follows
            transform.position = new Vector3(
                player.position.x + fixedX,
                currentY,
                fixedZ
            );
            wasGrounded = grounded;
            return;
        }
        else
        {
            // X locked, Z follows
            transform.position = new Vector3(
                fixedX,
                currentY,
                player.position.z + fixedZ
            );

            wasGrounded = grounded;
            return;
        
        }
    }
    public void ChangeFixedAxis(bool followX)
    {
        followXAxis = followX;
    }

    private bool IsGrounded()
    {
        return playerSM.characterController.isGrounded;
    }
}
