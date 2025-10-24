using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BossPlatformsSimple : MonoBehaviour
{
    public List<Transform> row1 = new List<Transform>(4);
    public List<Transform> row2 = new List<Transform>(4);
    public List<Transform> row3 = new List<Transform>(4);
    public List<Transform> row4 = new List<Transform>(4);
    public List<Transform> row5 = new List<Transform>(4);
    public List<Transform> row6 = new List<Transform>(4);


    [Header("Motion")]
    public float riseSpeed = 5f;

    // Internal state
    private List<Transform> allPlatforms = new List<Transform>(12);
    private Dictionary<Transform, float> originalY = new Dictionary<Transform, float>();
    private List<Transform> raisedPlatforms = new List<Transform>();

    void Awake()
    {
        allPlatforms.Clear();
        allPlatforms.AddRange(row1);
        allPlatforms.AddRange(row2);
        allPlatforms.AddRange(row3);
        allPlatforms.AddRange(row4);
        allPlatforms.AddRange(row5);
        allPlatforms.AddRange(row6);

        originalY.Clear();
        foreach (var t in allPlatforms)
        {
            if (t) originalY[t] = t.localPosition.y;
        }
    }

    // Call this when boss gets dizzy
    public void RaiseRandomPlatforms()
    {
        ResetAll(); // Always reset first

        raisedPlatforms.Clear();
        var rows = new List<List<Transform>> { row1, row2, row3, row4, row5, row6 };

        int prevIdx = -1;
        HashSet<int> usedIndices = new HashSet<int>();

        for (int rowNum = 0; rowNum < rows.Count; rowNum++)
        {
            var row = rows[rowNum];
            if (row == null || row.Count == 0) continue;

            List<int> allowed = new List<int>();

            if (rowNum == 0)
            {
                // First row: pick any
                for (int i = 0; i < row.Count; i++)
                    allowed.Add(i);
            }
            else
            {
                // Prefer indices within 2 of prevIdx and not already used
                for (int i = 0; i < row.Count; i++)
                {
                    if (Mathf.Abs(i - prevIdx) <= 1 && !usedIndices.Contains(i))
                        allowed.Add(i);
                }
                // If none, allow adjacent even if already used
                if (allowed.Count == 0)
                {
                    for (int i = 0; i < row.Count; i++)
                    {
                        if (Mathf.Abs(i - prevIdx) <= 2)
                            allowed.Add(i);
                    }
                }
                // If still none, allow any unused
                if (allowed.Count == 0)
                {
                    for (int i = 0; i < row.Count; i++)
                    {
                        if (!usedIndices.Contains(i))
                            allowed.Add(i);
                    }
                }
                // If still none, allow any
                if (allowed.Count == 0)
                {
                    for (int i = 0; i < row.Count; i++)
                        allowed.Add(i);
                }

                // Prefer not to pick the opposite edge if previous was an edge, unless it's the only option
                if (prevIdx == row.Count - 1 && allowed.Contains(0) && allowed.Count > 1)
                    allowed.Remove(0);
                if (prevIdx == 0 && allowed.Contains(row.Count - 1) && allowed.Count > 1)
                    allowed.Remove(row.Count - 1);
            }

            int idx = allowed[Random.Range(0, allowed.Count)];
            usedIndices.Add(idx);
            prevIdx = idx;

            var t = row[idx];
            if (t)
            {
                raisedPlatforms.Add(t);
                StartCoroutine(RaisePlatform(t));
            }
        }
    }  

    // Call this when boss is hit (dizzy ends)
    public void ResetAll()
    {
        StopAllCoroutines();
        foreach (var t in allPlatforms)
        {
            if (t && originalY.ContainsKey(t))
            {
                var pos = t.localPosition;
                pos.y = originalY[t];
                t.localPosition = pos;
            }
        }
        raisedPlatforms.Clear();
    }

    private IEnumerator RaisePlatform(Transform t)
    {
        if (!t || !originalY.ContainsKey(t)) yield break;
        float targetY = 0f;
        while (Mathf.Abs(t.localPosition.y - targetY) > 0.01f)
        {
            var pos = t.localPosition;
            pos.y = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
            t.localPosition = pos;
            yield return null;
        }
        var finalPos = t.localPosition;
        finalPos.y = targetY;
        t.localPosition = finalPos;
    }
}