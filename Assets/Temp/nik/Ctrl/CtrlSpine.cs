using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CtrlSpine : Ctrl
{
    public Transform ctrl0;
    public Transform ctrl1;
    public Transform boneMid;
    [Range(0f, 1f)] public float weight;
    public bool isStretchable;
    public float dist0 = 10;

    private Vector3 p0, p1, p2, p3;
    private Vector3[] points = new Vector3[10];

    private Vector3 mid0, mid1;

    void Start()
    {
        
    }

    private void Update()
    {
        if (!isActive) return;

        //calculate bezier
        p0 = ctrl0.position;
        p3 = ctrl1.position;

        float dist = Vector3.Distance(p0, p3) / 3f;
        p1 = p0 + ctrl0.up * dist;
        p2 = p3 - ctrl1.up * dist;

        //calculate mid point by weight
        mid0 = BezierPoint(p0, p1, p2, p3, weight);
        mid1 = BezierPoint(p0, p1, p2, p3, weight + .1f);
        boneMid.position = mid0;
        boneMid.LookAt(mid1, ctrl0.forward  + ctrl1.forward);
        boneMid.rotation = SetRot(boneMid.rotation, Axis.py, Axis.pz);

        if (isStretchable)
        {
            var scaleZ = 3f * dist / dist0;
            var scaleX = Mathf.Sqrt(1f / scaleZ);
            boneMid.localScale = new Vector3(scaleX, scaleX, scaleZ);
        }

        if (showGizmos)
        {
            for (int i = 0; i < 10; i++)
            {
                points[i] = BezierPoint(p0, p1, p2, p3, i * .1f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        for (int i = 0; i < 9; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
        Gizmos.DrawLine(points[9], p3);
    }

    private Vector3 BezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float weight)
    {
        var t = weight;
        var t1 = 1f - t;
        return t1 * t1 * t1 * p0 + 3 * t * t1 * t1 * p1 + 3 * t * t * t1 * p2 + t * t * t * p3;
    }
}
