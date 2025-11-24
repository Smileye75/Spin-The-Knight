using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossHeart : MonoBehaviour
{
    [Tooltip("Invoked when this heart is destroyed by the player. Call DestroyHeart() from heart logic.")]
    public UnityEvent onHeartDestroyed;

    // Call this from your heart-destruction logic (or when health reaches 0)
    public void DestroyHeart()
    {
        onHeartDestroyed?.Invoke();
        // optional: play VFX / SFX here
        Destroy(gameObject);
    }
}
