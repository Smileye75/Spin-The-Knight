using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State is an abstract base class for all states in a state machine.
/// It defines the required methods for entering, updating, and exiting a state.
/// All specific state classes (e.g., movement, attack, idle) should inherit from this class
/// and implement their own logic for Enter, Tick, and Exit.
/// </summary>
public abstract class State 
{
    /// <summary>
    /// Called when the state is entered. Use this to initialize state-specific logic.
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// Called every frame while this state is active. Use this for per-frame updates.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    public abstract void Tick(float deltaTime);

    /// <summary>
    /// Called when the state is exited. Use this to clean up or reset state-specific logic.
    /// </summary>
    public abstract void Exit();
}
