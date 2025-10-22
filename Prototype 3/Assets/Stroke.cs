using System.Collections.Generic;
using UnityEngine;

public class Stroke : MonoBehaviour
{
    [SerializeField] float minPointDistance = 0.005f; // meters
    LineRenderer lr;
    readonly List<Vector3> points = new();

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.useWorldSpace = true;      
    }

    public void AddPoint(Vector3 p)
    {
        if (points.Count == 0 || Vector3.Distance(points[^1], p) >= minPointDistance)
        {
            points.Add(p);
            lr.positionCount = points.Count;
            lr.SetPosition(points.Count - 1, p);
        }
    }

    public void TryEraseAt(Vector3 worldPos, float radius)
    {
        if (points.Count < 2) return;

        bool removed = false;
        for (int i = points.Count - 1; i >= 0; --i)
        {
            if (Vector3.Distance(points[i], worldPos) <= radius)
            {
                points.RemoveAt(i);
                removed = true;
            }
        }

        if (!removed) return;

        if (points.Count < 2) { Destroy(gameObject); return; }

        lr.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) lr.SetPosition(i, points[i]);
    }
}
