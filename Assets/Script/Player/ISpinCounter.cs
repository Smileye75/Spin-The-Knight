public interface ISpinCounter
{
    void OnSpinCycle(); // called by the Spinning clip's single animation event each loop
    void EndAttack();   // called by the End Attack animation event
}