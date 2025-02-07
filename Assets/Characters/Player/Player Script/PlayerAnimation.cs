using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    public PlayerControl playerControl; // Reference to PlayerControl

    public void StopAnimationEvent()
    {
        if (playerControl != null)
        {
            playerControl.StopAnimation(); // Call StopAnimation() on PlayerControl
        }
    }
}