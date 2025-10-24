using System.Collections.Generic;
using UnityEngine;

public class StackableCrates : MonoBehaviour
{
    [Header("Crate/Stack Settings")]
    public List<StompableProps> stack = new List<StompableProps>(); // bottom -> top
    public float crateHeight = 1f;            // world-space height per crate
    public Transform bottomPosition;          // anchor at the base of the stack
    public Vector3 upAxis = Vector3.up;       // in case your stack is tilted
    public float settleTime = 0.08f;          // if your MoveTo... uses time

    // internal debounce so multiple destroys in one frame don't cause drift
    bool _needsCompact;

    void LateUpdate()
    {
        if (_needsCompact)
        {
            _needsCompact = false;
            RebuildAndCompact();
        }
    }

    public void RegisterCrate(StompableProps crate)
    {
        if (crate == null) return;
        if (!stack.Contains(crate))
        {
            stack.Add(crate);
            _needsCompact = true;
        }
    }

    // Call this from the crate when it actually dies (once).
    public void OnCrateDestroyed(StompableProps destroyedCrate)
    {
        // Remove any nulls or dupes first (defensive)
        for (int i = stack.Count - 1; i >= 0; i--)
        {
            if (stack[i] == null) stack.RemoveAt(i);
        }

        int idx = stack.IndexOf(destroyedCrate);
        if (idx >= 0) stack.RemoveAt(idx);

        // Batch compaction to next LateUpdate to avoid multiple moves this frame
        _needsCompact = true;
    }

    void RebuildAndCompact()
    {
        // Clean up nulls again just in case
        for (int i = stack.Count - 1; i >= 0; i--)
            if (stack[i] == null) stack.RemoveAt(i);

        // Sort by current Y so we maintain bottom->top order even if things wobbled
        stack.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            return a.transform.position.y.CompareTo(b.transform.position.y);
        });

        // Recompute deterministic target slots from the bottom anchor
        Vector3 basePos = bottomPosition != null ? bottomPosition.position : transform.position;
        for (int i = 0; i < stack.Count; i++)
        {
            var crate = stack[i];
            if (crate == null) continue;

            Vector3 target = basePos + upAxis.normalized * (crateHeight * i);

            if (HasMoveMethod(crate))
            {
                // Your crate script should snap or tween to target
                crate.MoveToBelowCrate(target); // rename if you like: MoveToSlot(target, settleTime)
            }
            else
            {
                // Hard snap if you don't want tweening
                crate.transform.position = target;
                var rb = crate.GetComponent<Rigidbody>();
                if (rb && !rb.isKinematic) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            }
        }
    }

    bool HasMoveMethod(StompableProps c)
    {
        // If you sometimes use plain objects without the mover, keep this guard.
        // You can remove this if every crate has the method.
        return c != null;
    }
}
