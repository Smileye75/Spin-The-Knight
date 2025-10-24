using System.Collections;
using UnityEngine;

public class StompableShatter : MonoBehaviour
{
    [Tooltip("Root of the intact crate visuals (shown initially).")]
    public GameObject intactMeshRoot;
    [Tooltip("Root of the shattered crate visuals (disabled initially; has ShatterdCrate).")]
    public GameObject shatteredMeshRoot;
    [Tooltip("If true, use ShatterdCrate to burst pieces.")]
    public bool useScriptedShatter = true;
    [Tooltip("Delay before shatter (for stomp feedback).")]
    [Range(0f, 0.3f)] public float shatterDelay = 0.12f;

    public void Init()
    {
        if (shatteredMeshRoot) shatteredMeshRoot.SetActive(false);
        if (intactMeshRoot) intactMeshRoot.SetActive(true);
    }

    public void ShatterWithDelay()
    {
        StartCoroutine(ShatterAfterDelay());
    }

    IEnumerator ShatterAfterDelay()
    {
        yield return new WaitForSecondsRealtime(shatterDelay);
        ShatterInstant();
    }

    public void ShatterInstant()
    {
        if (!useScriptedShatter || shatteredMeshRoot == null)
        {
            Destroy(gameObject, 0.15f);
            return;
        }

        if (intactMeshRoot) intactMeshRoot.SetActive(false);
        shatteredMeshRoot.SetActive(true);

        var shatter = shatteredMeshRoot.GetComponent<ShatterdCrate>();
        if (!shatter)
        {
            Debug.LogWarning($"[StompableShatter] No ShatterdCrate on {shatteredMeshRoot.name}. Destroying normally.");
            Destroy(gameObject, 0.15f);
            return;
        }

        shatter.crateRoot = gameObject;
        shatter.Play();
    }
}
