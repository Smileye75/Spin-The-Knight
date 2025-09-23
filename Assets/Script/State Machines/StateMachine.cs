using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StateMachine is an abstract MonoBehaviour that manages state transitions for any state-driven object.
/// It holds a reference to the current state, calls its Tick() method every frame,
/// and handles switching between states by calling Exit() on the old state and Enter() on the new one.
/// All specific state machines (e.g., player, enemy) should inherit from this class.
/// </summary>
public abstract class StateMachine : MonoBehaviour
{
    private State currentState; // The currently active state

    /// <summary>
    /// Unity's Update loop. Calls Tick() on the current state every frame.
    /// </summary>
    private void Update()
    {
        if (currentState != null)
        {
            currentState.Tick(Time.deltaTime);
        }
    }

    /// <summary>
    /// Switches from the current state to a new state.
    /// Calls Exit() on the old state and Enter() on the new state.
    /// </summary>
    /// <param name="newState">The new state to switch to.</param>
    public void SwitchState(State newState)
    {
        if(currentState != null) 
        {
            currentState.Exit();
        }
        
        currentState = newState;

        if (currentState != null)
        {
            currentState.Enter();
        }
    }
}
