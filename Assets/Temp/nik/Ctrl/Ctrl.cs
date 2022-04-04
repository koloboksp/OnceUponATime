using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ctrl : MonoBehaviour
{
    public enum Axis { px, py, pz, nx, ny, nz }

    public bool isActive;
    public bool showGizmos;

    private Vector4 outx; //column 0
    private Vector4 outy; //column 1
    private Vector4 outz; //column 2

    protected Quaternion SetRot(Quaternion rot, Axis axisFwd, Axis axisUp)
    {
        if (axisFwd == axisUp) return rot; //incorrect input

        var mxIn = Matrix4x4.Rotate(rot);
        var mxOut = Matrix4x4.identity;
        var ifwd = (int)axisFwd;
        var iup = (int)axisUp;
        outz = mxIn.GetColumn(ifwd % 3) * ((ifwd > 2) ? -1 : 1);
        outy = mxIn.GetColumn(iup % 3) * ((iup > 2) ? -1 : 1);
        outx = Vector3.Cross(outy, outz);

        mxOut.SetColumn(0, outx);
        mxOut.SetColumn(1, outy);
        mxOut.SetColumn(2, outz);

        return mxOut.rotation;
    }

    protected void ReLookAt(Transform obj, Vector3 inFwd, Vector3 inUp, Axis outFwd, Axis outUp)
    {
        obj.LookAt(inFwd, inUp);
        obj.rotation = SetRot(obj.rotation, outFwd, outUp);
    }

    protected Vector3 SolveIK2(Vector3 p0, Vector3 p1, Vector3 vLook, float r0, float r1, bool isStretchable, out float stretch)
    {
        var ikMid = p0;

        //find look point
        var l = Vector3.Distance(p0, p1);

        //find ik plane
        var dirX = (p1 - p0).normalized; // from start to end
        var dirY = vLook.normalized; //look on ctrlLook
        var dirZ = Vector3.Cross(dirX, dirY).normalized;
        dirY = Vector3.Cross(dirZ, dirX);

        stretch = 1f;

        if (l > r0 + r1)
        {
            if (isStretchable) stretch = l / (r0 + r1);
            ikMid = dirX * r0 * stretch;
        }
        else
        {
            var x = (r0 * r0 - r1 * r1 + l * l) / (2f * l);
            var y = Mathf.Sqrt(r0 * r0 - x * x);
            ikMid = dirX * x + dirY * y;
        }

        return p0 + ikMid;
    }
}
