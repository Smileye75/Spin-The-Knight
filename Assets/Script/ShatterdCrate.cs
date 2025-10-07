using System.Collections;
using UnityEngine;

public class ShatterdCrate : MonoBehaviour
{
    private Vector2 speedRange = new Vector2(90f, 90f);
    public float duration = 0.05f;
    public GameObject crateRoot; // assign your parent (the one with StompableProps)

    Transform[] pieces;
    Vector3[] directions;
    float[] speeds;
    bool prepared;

    public void Play()
    {
        if (!prepared) PrepareTargets();
        StartCoroutine(BurstThenCleanup());
    }

    void PrepareTargets()
    {
        prepared = true;

        int count = transform.childCount;
        pieces     = new Transform[count];
        directions = new Vector3[count];
        speeds     = new float[count];

        Vector3 center = transform.position;

        for (int i = 0; i < count; i++)
        {
            var t = transform.GetChild(i);
            pieces[i] = t;

            // Always move outward on XZ plane
            Vector3 dir = t.position - center;
            dir = Vector3.ProjectOnPlane(dir, Vector3.up); // zeroes out Y

            // Fallback if at center
            if (dir.sqrMagnitude < 1e-6f)
                dir = new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f);

            dir = dir.normalized;
            directions[i] = dir;
            speeds[i]     = Random.Range(speedRange.x, speedRange.y);
        }
    }

    IEnumerator BurstThenCleanup()
    {
        float t = 0f;
        while (t < duration)
        {
            float dt = Time.deltaTime;
            t += dt;

            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i] == null) continue;
                pieces[i].position += directions[i] * speeds[i] * dt;
            }

            yield return null;
        }

        Destroy(crateRoot != null ? crateRoot : gameObject);
    }
    }
