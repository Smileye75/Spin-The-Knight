using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform respawnLocation;
    [SerializeField] private GameObject skull;
    [SerializeField] private BoxCollider checkpointTrigger;

    [Header("FX")]
    [SerializeField] private ParticleSystem idleFire;
    [SerializeField] private ParticleSystem activeFire;

    private static Checkpoint s_active;
    private static Transform s_respawn;

    public static bool HasActive => s_active != null && s_respawn != null;
    public static Vector3 ActiveRespawnPosition => HasActive ? s_respawn.position : Vector3.zero;

    private bool _activated = false;
    private CharacterController _player;

    private void Awake()
    {
        if (checkpointTrigger) checkpointTrigger.isTrigger = true;
        if (idleFire) idleFire.Play();
        if (activeFire) activeFire.Stop();
    }

    private void OnEnable()  { PlayerStats.OnPlayerLostLife += OnPlayerLostLife; }
    private void OnDisable() { PlayerStats.OnPlayerLostLife -= OnPlayerLostLife; }

    private void OnPlayerLostLife(GameObject player)
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var cc = other.GetComponentInParent<CharacterController>();
            if (cc != null) _player = cc;
        }

        if (!_activated && other.CompareTag("Weapon"))
        {
            Activate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_player == null) return;

        var cc = other.GetComponentInParent<CharacterController>();
        if (cc == _player) _player = null;
    }

    private void Activate()
    {
        _activated = true;

        if (s_active != null && s_active != this)
            s_active.Deactivate();

        s_active = this;
        s_respawn = respawnLocation;

        if (idleFire) idleFire.Stop();
        if (activeFire) activeFire.Play();

        if (skull) skull.SetActive(false);

        if (checkpointTrigger) checkpointTrigger.enabled = false;

        // Restore player health when checkpoint is triggered
        if (_player != null && _player.TryGetComponent<PlayerStats>(out var stats))
        {
            stats.Heal(stats.maxHealth);
        }
    }

    private void Deactivate()
    {
        if (idleFire) idleFire.Stop();
        if (activeFire) activeFire.Stop();
        _activated = false;
    }

    private void Update()
    {
        FacePlayer();
    }

    private void FacePlayer()
    {
        if (_activated) return;
        if (!skull || !_player) return;

        Vector3 toPlayer = _player.transform.position - skull.transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(toPlayer);
        skull.transform.rotation = Quaternion.Slerp(
            skull.transform.rotation,
            target,
            10f * Time.deltaTime
        );
    }
}

