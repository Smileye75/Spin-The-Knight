/// <summary>
/// ISpinCounter defines a contract for any class that wants to respond to spin attack animation events.
/// Implement this interface if you want your object to react to each spin cycle and to the end of a spin attack,
/// such as for combo counting, hit detection, or resetting state.
/// </summary>
public interface ISpinCounter
{
    /// <summary>
    /// Called once per loop of the spinning attack animation.
    /// Use this to trigger logic for each spin cycle (e.g., apply damage, increment combo, play effects).
    /// </summary>
    void OnSpinCycle();

    /// <summary>
    /// Called by the End Attack animation event.
    /// Use this to clean up or reset state when the spin attack finishes (e.g., stop effects, reset counters).
    /// </summary>
    void EndAttack();
}