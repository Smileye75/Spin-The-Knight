using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PersistentUIRoot ensures that the UI root GameObject persists across scene loads,
/// so UI elements (such as menus or HUDs) are not destroyed when changing scenes.
/// Implements a simple singleton pattern to prevent duplicates.
/// </summary>
public class PersistentUIRoot : MonoBehaviour
{
    private static PersistentUIRoot _instance;

    /// <summary>
    /// On Awake, ensures only one instance exists and makes this GameObject persistent.
    /// Destroys any duplicate instances found on scene load.
    /// </summary>
    void Awake()
    {
        if (_instance)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
