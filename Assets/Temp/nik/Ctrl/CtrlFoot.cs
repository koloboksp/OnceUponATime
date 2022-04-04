using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[ExecuteInEditMode]
public class CtrlFoot : Ctrl
{
    public Transform ikRoot;

    public Transform[] bones; //4 bones: hip, leg, foot0, foot1

    [Range(0f, 1f)] public float heelLift; //to animate
    private float ang;

    public bool isStretchable;
    private float stretch;

    //saved at setup
    [SerializeField] private Vector3 pntFootMid0; //pos in ctrlFoot space
    [SerializeField] private Vector3 pntFootEnd0;
    [SerializeField] private float r0, r1, r2; //length of bones

    //runtame calculated
    private Vector3 pntFootRoot;
    private Vector3 pntFootMid; //pos in worldSpace
    private Vector3 pntFootEnd;
    private Vector3 pntIKMid;
 
    private bool isInit = false;

    void Start()
    {
        
    }

    void Update()
    {
        if (!isActive) return;
        if (!isInit) return;

        //define pntFootMid, convert to world space
        pntFootMid = transform.localToWorldMatrix.MultiplyPoint3x4(pntFootMid0);
        pntFootEnd = transform.localToWorldMatrix.MultiplyPoint3x4(pntFootEnd0); 

        //define pntFootRoot using heel up
        ang = heelLift;
        Vector3 axisX = (transform.position - pntFootMid).normalized;
        Vector3 axisY = Vector3.Cross(transform.right, axisX);
        pntFootRoot = (axisX * Mathf.Cos(ang) + axisY.normalized * Mathf.Sin(ang)) * r2 + pntFootMid;

        //define ik mid using new pntFootRoot as IKEndPoint
        pntIKMid = SolveIK2(ikRoot.position, pntFootRoot, transform.forward, r0, r1, isStretchable, out stretch);

        //apply to bones
        ReLookAt(bones[0], pntIKMid, ikRoot.position - transform.position, Axis.py, Axis.nx);
        ReLookAt(bones[1], pntFootRoot, transform.forward, Axis.py, Axis.nx);

        /*
        //apply stretch
        if (stretch > 1)
        {
            bones[0].localScale = Vector3.one + Vector3.right * (stretch - 1f);
            var scaleX = bones[1].worldToLocalMatrix.MultiplyVector(transform.right);
            var scaleY = bones[1].worldToLocalMatrix.MultiplyVector(transform.up);
            var scaleZ = bones[1].worldToLocalMatrix.MultiplyVector(transform.forward);

            bones[2].localScale = new Vector3(Vector3.Dot(transform.right, scaleX),
                                              Vector3.Dot(transform.up, scaleY),
                                              Vector3.Dot(transform.forward, scaleZ));
        }
        */

        ReLookAt(bones[2], pntFootMid, transform.up, Axis.py, Axis.nx);
        ReLookAt(bones[3], pntFootEnd, transform.up, Axis.py, Axis.nx);
    }

    public void Init()
    {
        if (bones.Length != 4) return;

        r0 = Vector3.Distance(bones[1].position, bones[0].position);
        r1 = Vector3.Distance(bones[2].position, bones[1].position);
        r2 = Vector3.Distance(bones[3].position, bones[2].position);

        pntFootMid0 = transform.worldToLocalMatrix.MultiplyPoint3x4(bones[3].position); //save position in ctrlFoot space
        pntFootEnd0 = transform.worldToLocalMatrix.MultiplyPoint3x4(bones[3].position - bones[3].right * r2);
        
        isInit = true;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.DrawWireCube(pntFootMid, Vector3.one * .05f);
        Gizmos.DrawWireCube(pntFootRoot, Vector3.one * .05f);

        Gizmos.DrawWireCube(pntIKMid, Vector3.one * .05f);

        Gizmos.DrawWireCube(pntFootEnd, Vector3.one * .03f);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(CtrlFoot))]
public class CtrlFootEditor : Editor
{
    private CtrlFoot t;

    private void OnEnable()
    {
        t = (CtrlFoot)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Init")) t.Init();
    }
}

#endif
