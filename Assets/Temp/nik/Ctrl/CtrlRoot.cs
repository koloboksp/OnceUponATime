using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CtrlRoot : MonoBehaviour
{
    public List<Matrix4x4> bones;
    public SkinnedMeshRenderer skin;

    public List<Transform> controllers;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveBones()
    {
        if (skin == null) return;

        var skinBones = skin.bones;
        var local = Matrix4x4.identity;
        var root = transform;

        for (int i = 0; i < skinBones.Length; i++)
        {
            local = root.worldToLocalMatrix * skinBones[i].localToWorldMatrix;
            bones.Add(local);
        }

    }

    public void ResetBones()
    {
        if (skin == null) return;

        var skinBones = skin.bones;
        Matrix4x4 local;
        var root = transform;

        for (int i = 0; i < skinBones.Length; i++)
        {
            local = root.localToWorldMatrix * bones[i];
            skinBones[i].SetPositionAndRotation(local.GetColumn(3), local.rotation);
            skinBones[i].localScale = local.lossyScale;
        }
    }

    public void ResetControllers()
    {
        foreach (var c in controllers)
        {
            c.localPosition = Vector3.zero;
            c.localRotation = Quaternion.identity;
            c.localScale = Vector3.one;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(CtrlRoot))]
public class CtrlRootEditor : Editor
{
    private CtrlRoot t;

    private void OnEnable()
    {
        t = (CtrlRoot)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Save Bones")) t.SaveBones();
        if (GUILayout.Button("Reset Bones")) t.ResetBones();
        if (GUILayout.Button("Reset Controllers")) t.ResetControllers();
    }
}

#endif
