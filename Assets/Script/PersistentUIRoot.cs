using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentUIRoot : MonoBehaviour
{
    private static PersistentUIRoot _instance;
    void Awake()
    {
        if (_instance) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
