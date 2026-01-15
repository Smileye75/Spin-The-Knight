using Cinemachine;
using UnityEngine;

/// <summary>
/// CameraTriggerZone handles switching between virtual cameras when the player enters or exits a trigger zone.
/// It raises the priority of the assigned CinemachineVirtualCamera when the player enters,
/// and (optionally) lowers it when the player exits, reverting to the main camera.
/// </summary>
public class CameraTriggerZone : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Camera to activate when player enters.")]
    [SerializeField] private CinemachineVirtualCamera targetCamera;

    [Header("Axis Focus")]
    [SerializeField] private FixedCameraFocus fixedCameraFocus;
    [SerializeField] private bool changeToXAxis;
    [SerializeField] private float newFixedAxisValue;
    private static CameraTriggerZone activeZone = null;

    private int activePriority = 30;    // Priority when this camera should be active
    private int inactivePriority = 5;   // Priority when this camera should be inactive

    /// <summary>
    /// Raises the camera priority when the player enters the trigger zone,
    /// making this virtual camera the active one.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Boss"))
        {
            if (activeZone != null && activeZone != this)
                activeZone.SetInactive();

            SetActive();
            activeZone = this;

            // Set axis mode based on inspector value
            if (fixedCameraFocus != null)
            {
                if(changeToXAxis)
                {
                    fixedCameraFocus.fixedZ = newFixedAxisValue;
                    fixedCameraFocus.fixedX = 0f;
                } 
                else
                {
                    fixedCameraFocus.fixedX = newFixedAxisValue;
                    fixedCameraFocus.fixedZ = 0f;
                }   

                fixedCameraFocus.ChangeFixedAxis(changeToXAxis);
            }
        }
    }

    /// <summary>
    /// Lowers the camera priority (reverting to main camera) when the player exits the trigger zone,
    /// if revertToMainCamera is enabled.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Boss")) && activeZone == this)
        {
            SetInactive();
            activeZone = null;
        }
    }

    private void SetActive()
    {
        if (targetCamera != null)
            targetCamera.Priority = activePriority;
    }

    private void SetInactive()
    {
        if (targetCamera != null)
            targetCamera.Priority = inactivePriority;
    }
}
