using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MovingPlatform moves between a series of points in the scene, pausing at each point for a set duration.
/// It calculates and exposes its current velocity for use by other scripts (such as player movement on platforms).
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("Path Settings")]
    [Tooltip("Points the platform will move between.")]
    [SerializeField] private Transform[] points;

    [Header("Movement Settings")]
    [Tooltip("Speed at which the platform moves.")]
    [SerializeField] private float speed = 2f;

    [Tooltip("Time to wait at each point before moving to the next.")]
    [SerializeField] private float waitTime = 1f;

    private int currentIndex = 0;         // Current target point index
    private float waitCounter = 0f;       // Countdown timer for waiting
    private bool waiting = false;         // Is the platform currently waiting?

    private Vector3 lastPosition;         // Last frame's position for velocity calculation

    /// <summary>
    /// The current velocity of the platform (used by player movement scripts).
    /// </summary>
    public Vector3 Velocity { get; private set; }

    /// <summary>
    /// Initializes the platform's last position.
    /// </summary>
    private void Start()
    {
        lastPosition = transform.position;
    }

    /// <summary>
    /// Handles platform movement, waiting at points, and velocity calculation each frame.
    /// </summary>
    private void Update()
    {
        if (waiting)
        {
            // Countdown the wait timer while paused at a point
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0f)
                waiting = false;

            Velocity = Vector3.zero; // Ensure velocity is zero while waiting
            return;
        }

        // Move towards the current target point
        Transform target = points[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // If reached the target point, start waiting
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentIndex = (currentIndex + 1) % points.Length;
            waitCounter = waitTime;
            waiting = true;
        }

        // Calculate platform velocity based on position change
        Velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }
}
