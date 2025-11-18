using UnityEngine;

public interface ISpawnerListener
{
    // Called when the spawner has spawned all enemies and none remain alive.
    void OnSpawnerCleared(PlayerStateMachine playerStateMachine);
}